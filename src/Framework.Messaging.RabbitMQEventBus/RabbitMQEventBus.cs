using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Events;
using Framework.Messaging.EventBus.Handlers;
using Framework.Messaging.EventBus.RabbitMQ.Configuration;
using Framework.Messaging.EventBus.RabbitMQ.Connections;
using Framework.Messaging.EventBus.RabbitMQ.Envelopes;
using Framework.Messaging.EventBus.RabbitMQ.Events;
using Framework.Messaging.EventBus.RabbitMQ.Extensions;
using Framework.Messaging.EventBus.RabbitMQ.Integrity.Flow;
using Framework.Messaging.EventBus.RabbitMQ.Integrity.Resilience;
using Framework.Messaging.EventBus.RabbitMQ.Internal;
using Framework.Messaging.EventBus.RabbitMQ.Subscriptions;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.RabbitMQ
{
    public class RabbitMQEventBus : IDisposable, IRabbitMQEventBus
    {
        private readonly OnSetRunner _onSetRunner;
        private readonly SubscriptionsManager _subscriptionManager;
        private readonly EventCancellationTokenPool _eventCancellationTokenPool;
        private readonly PersisterConnection _connection;

        private readonly IEventBusConfiguration _eventBusConfiguration;
        private readonly IQueueConfiguration _queueConfiguration;
        private readonly IAckConfiguration _ackConfiguration;

        private IModel _consumerChannel = null;

        public string ExchangeName { get { return _queueConfiguration.ExchangeName; } }

        public string QueueName { get { return _queueConfiguration.QueueName; } }

        public bool IsConnected { get { return _connection.IsConnected; } }

        public RabbitMQEventBus(
            PersisterConnection connection,
            IEventBusConfiguration eventBusConfiguration,
            IQueueConfiguration queueConfiguration,
            IAckConfiguration ackConfiguration
            )
        {
            _connection = connection;

            _eventBusConfiguration = eventBusConfiguration;

            _queueConfiguration = queueConfiguration;

            _ackConfiguration = ackConfiguration;

            _subscriptionManager = new SubscriptionsManager();
            _subscriptionManager.OnEventRemoved += OnSubscriptionManagerEventRemoved;
            _subscriptionManager.OnEventAdded += OnSubscriptionManagerEventAdded;

            _eventCancellationTokenPool = new EventCancellationTokenPool(_subscriptionManager);

            _onSetRunner = new OnSetRunner(true);
        }

        void OnSubscriptionManagerEventAdded(object _, EventEventArgs @event)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect(ResiliencePolicies.ConnectionPolicyFactory);
            }

            if (_consumerChannel == null)
            {
                _consumerChannel = CreateConsumerChannel(@event.EventName);
            }
        }

        void OnSubscriptionManagerEventRemoved(object _, EventEventArgs @event)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect(ResiliencePolicies.ConnectionPolicyFactory);
            }

            using (var channel = _connection.CreateModel())
            {
                if (!_subscriptionManager.IsEmpty) return;

                _consumerChannel.Close();
            }
        }


        public void Publish(BasicEvent @event)
        {
            InternalPublish(@event, ExchangeName, @event.GetType().Name);
        }

        public void Publish<T>(Envelope<T> envelope)
        {
            InternalPublish(envelope.Message, ExchangeName, envelope.RoutingKey);
        }

        public void Publish(Envelope envelope)
        {
            InternalPublish(envelope.Message, ExchangeName, envelope.RoutingKey);
        }

        public Task PublishAsync(BasicEvent @event)
        {
            return Task.Run(() => this.Publish(@event));
        }

        public Task PublishAsync<T>(Envelope<T> envelope)
        {
            return Task.Run(() => this.Publish(envelope));
        }

        public Task PublishAsync(Envelope envelope)
        {
            return Task.Run(() => this.Publish(envelope));
        }

        private void InternalPublish(object @event, string exchangeName, string routingKey)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect(ResiliencePolicies.ConnectionPolicyFactory);
            }

            using (var channel = _connection.CreateModel())
            {
                _onSetRunner.RunIfSet(() => channel.ExchangeDeclare(exchange: exchangeName, type: _queueConfiguration.ExchangeType.ToString().ToLower(), durable: _queueConfiguration.DurableExchange));

                string message;
                if (@event is string)
                {
                    message = (string)@event;
                }
                else
                {
                    message = JsonConvert.SerializeObject(@event, GlobalSettings.SerializerSettings);
                }

                var body = Encoding.UTF8.GetBytes(message);

                ResiliencePolicies.
                    ConnectionPolicyFactory(_eventBusConfiguration.Logger)
                    .Execute(() =>
                    {
                        channel.BasicPublish(exchange: exchangeName,
                        routingKey: routingKey,
                        basicProperties: null,
                        body: body);
                    });
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : BasicEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            _subscriptionManager.AddSubscription<TEvent, TEventHandler>();
        }

        public void Subscribe<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>
        {
            _subscriptionManager.AddSubscription<TEvent, TEventHandler>(routingKey);
        }

        public uint MessageCount()
        {
            if (_connection.IsConnected)
            {
                using (var channel = _connection.CreateModel())
                {
                    return channel.MessageCount(QueueName);
                }
            }
            else
                return 0;

        }

        public void Unsubscribe<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>
        {
            _subscriptionManager.RemoveSubscription<TEvent, TEventHandler>(routingKey);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : BasicEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            _subscriptionManager.RemoveSubscription<TEvent, TEventHandler>();
        }

        private IModel CreateConsumerChannel(string eventName)
        {
            if (!_connection.IsConnected)
            {
                _connection.TryConnect(ResiliencePolicies.ConnectionPolicyFactory);
            }

            var channel = _connection.CreateModel();

            var args = ResolveQueueArgs(channel, eventName);

            channel.ExchangeDeclare(ExchangeName, _queueConfiguration.ExchangeType.ToString().ToLower(), _queueConfiguration.DurableExchange);
            channel.QueueDeclare(_queueConfiguration.QueueName, _queueConfiguration.DurableQueue, false, false, args);
            channel.BasicQos(0, _queueConfiguration.PrefetchCount, false);

            var consumer = new EventingBasicConsumer(channel);

            var token = _eventCancellationTokenPool.GetCancellationToken(eventName);
            consumer.Received += (model, ea) =>
            {
                Task.Run(async () =>
                {
                    string message = string.Empty;
                    try
                    {
                        var localEventName = ea.RoutingKey;

                        using (var stream = new MemoryStream(ea.Body))
                        using (var reader = new StreamReader(stream))
                        {
                            message = reader.ReadToEnd();
                        }

                        if (token.IsCancellationRequested)
                        {
                            ResiliencePolicies.GeneralRetryPolicyFactory(_eventBusConfiguration.Logger)
                            .Execute(() => channel.BasicNack(ea.DeliveryTag, false, true));
                            return;
                        }

                        await HandleEvent(localEventName, message);
                        ResiliencePolicies.GeneralRetryPolicyFactory(_eventBusConfiguration.Logger)
                            .Execute(() => channel.BasicAck(ea.DeliveryTag, false));
                    }
                    catch (Exception ex)
                    {
                        HandleAckException(ex, message, ea.DeliveryTag, channel, new MessageHeaders(ea.BasicProperties.Headers));
                    }
                }, token);
            };

            channel.QueueBind(
                queue: _queueConfiguration.QueueName,
                exchange: ExchangeName,
                routingKey: eventName
                );

            channel.BasicConsume(queue: _queueConfiguration.QueueName,
                autoAck: false,
                consumer: consumer);

            channel.CallbackException += (sender, ea) =>
            {
                if (ea.Exception != null)
                {
                    _eventBusConfiguration.Logger.LogWarning(ea.Exception.ToString());
                }

                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel(eventName);
            };

            return channel;
        }

        private async Task HandleEvent(string eventName, string message)
        {
            if (!_subscriptionManager.HasSubscriptionsForEvent(eventName))
            {
                return;
            }

            var subscriptions = _subscriptionManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                await subscription.Handle(message, _eventBusConfiguration.MessageTypeResolver);
            }
        }

        private void HandleAckException(Exception exception, string message, ulong deliveryTag, IModel channel, MessageHeaders messageHeaders)
        {
            try
            {
                _eventBusConfiguration.Logger.LogCritical(exception.ToString());
                ResiliencePolicies.GeneralRetryPolicyFactory(_eventBusConfiguration.Logger)
                    .Execute(GetAckOnExceptionAction(message, deliveryTag, exception, channel, messageHeaders));
            }
            catch (Exception ex)
            {
                _eventBusConfiguration.Logger.LogCritical(ex.ToString());
            }
        }

        private Action GetAckOnExceptionAction(string message, ulong deliveryTag, Exception exception, IModel channel, MessageHeaders messageHeaders)
        {
            var decision = _ackConfiguration.AckDecisionProvider(exception, messageHeaders.GetXDeathCount());
            if (decision == AckDecisionEnum.OnlyNack)
            {
                return () => channel.BasicNack(deliveryTag, false, false);
            }
            else if (decision == AckDecisionEnum.NackAndRequeue)
            {
                return () => channel.BasicNack(deliveryTag, false, true);
            }
            else
            {
                return () => channel.BasicAck(deliveryTag, false);
            }
        }

        private IDictionary<string, object> ResolveQueueArgs(IModel channel, string routeKey)
        {
            IDictionary<string, object> args = ResolveDlxOptional(channel, routeKey);

            if (_queueConfiguration.QueueMessageTTL.HasValue)
            {
                if (args == null) args = new Dictionary<string, object>();
                args.Add("x-message-ttl", (int)_queueConfiguration.QueueMessageTTL.Value.TotalMilliseconds);
            }

            return args;
        }

        private IDictionary<string, object> ResolveDlxOptional(IModel channel, string routeKey)
        {
            var deadLetterExchangeConfiguration = _queueConfiguration.DeadLetterExchangeConfiguration;

            if (deadLetterExchangeConfiguration != null)
            {
                var dlxArgs = new Dictionary<string, object>
                {
                    { "x-message-ttl", (int)deadLetterExchangeConfiguration.QueueMessagesTTL.TotalMilliseconds },
                };

                if (deadLetterExchangeConfiguration.CyclicDLQ)
                {
                    dlxArgs.Add("x-dead-letter-exchange", ExchangeName);
                    dlxArgs.Add("x-dead-letter-routing-key", routeKey);
                }

                channel.ExchangeDeclare(deadLetterExchangeConfiguration.DeadLetterExchangeName, _queueConfiguration.ExchangeType.ToString().ToLower(), _queueConfiguration.DurableExchange);
                channel.QueueDeclare(deadLetterExchangeConfiguration.DeadLetterQueueName, _queueConfiguration.DurableQueue, false, false, dlxArgs);
                channel.QueueBind(deadLetterExchangeConfiguration.DeadLetterQueueName, deadLetterExchangeConfiguration.DeadLetterExchangeName, deadLetterExchangeConfiguration.DeadLetterRouteKeyProvider(routeKey), null);

                return new Dictionary<string, object>()
                                {
                                    {"x-dead-letter-exchange", deadLetterExchangeConfiguration.DeadLetterExchangeName },
                                    {"x-dead-letter-routing-key", deadLetterExchangeConfiguration.DeadLetterRouteKeyProvider(routeKey)},
                                };
            }

            return null;
        }

        public void Dispose()
        {
            try
            {
                _connection.Dispose();
                if (_consumerChannel != null)
                {
                    _consumerChannel.Dispose();
                    _consumerChannel = null;
                }

                _eventCancellationTokenPool.Dispose();
            }
            catch
            { } // Necessário suprimir exceções no dispose para evitar quebras desnecessárias
        }
    }
}