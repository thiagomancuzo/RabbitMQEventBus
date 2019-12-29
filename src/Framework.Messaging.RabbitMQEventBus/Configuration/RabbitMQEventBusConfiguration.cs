using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.RabbitMQ.Configuration.Security;
using Framework.Messaging.EventBus.RabbitMQ.Internal;
using Framework.Messaging.EventBus.Resilience;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public class RabbitMQEventBusConfiguration : IConfiguration
    {
        private readonly IConnectionConfiguration connectionConfiguration;
        private readonly string exchangeName;
        private readonly string queueName;
        private readonly ushort prefetchCount;

        private AckDecisionProvider ackDecisionProvider;
        private DeadLetterExchangeConfiguration deadLetterExchangeConfiguration;

        private TimeSpan? queueMessageTTL = null;
        private ISecurityCofiguration securityConfiguration;
        private ILogger logger;
        private IMessageTypeResolver messageTypeResolver;

        public RabbitMQEventBusConfiguration(string userName, string password, string hostName, int port, string exchangeName, string queueName, ushort prefetchCount)
            : this(new CredentialsConnectionConfiguration(userName, password, hostName, port), exchangeName, queueName, prefetchCount)
        { }

        public RabbitMQEventBusConfiguration(string userName, string password, string hostName, int port, string virtualHost, string exchangeName, string queueName, ushort prefetchCount)
            : this(new CredentialsConnectionConfiguration(userName, password, hostName, port, virtualHost), exchangeName, queueName, prefetchCount)
        { }

        public RabbitMQEventBusConfiguration(Uri connectionUri, string exchangeName, string queueName, ushort prefetchCount)
            : this(new UriConnectionConfiguration(connectionUri), exchangeName, queueName, prefetchCount)
        { }

        private RabbitMQEventBusConfiguration(IConnectionConfiguration connectionConfiguration, string exchangeName, string queueName, ushort prefetchCount)
        {
            this.connectionConfiguration = connectionConfiguration;
            this.exchangeName = exchangeName;
            this.queueName = queueName;
            this.prefetchCount = prefetchCount;

            this.DurableExchange = false;
            this.DurableQueue = false;
            this.ExchangeType = ExchangeTypeEnum.Direct;
        }

        public string ExchangeName
        {
            get
            {
                return this.exchangeName;
            }
        }

        public string QueueName
        {
            get
            {
                return this.queueName;
            }
        }

        public ushort PrefetchCount
        {
            get
            {
                return this.prefetchCount;
            }
        }

        public ILogger Logger
        {
            get
            {
                return this.logger ?? new DefaultLogger((log) => Console.WriteLine(log));
            }
        }

        public IMessageTypeResolver MessageTypeResolver
        {
            get
            {
                return this.messageTypeResolver ?? new DefaultMessageTypeResolver((t) => Activator.CreateInstance(t));
            }
        }

        public DeadLetterExchangeConfiguration DeadLetterExchangeConfiguration
        {
            get
            {
                return this.deadLetterExchangeConfiguration;
            }
        }

        public AckDecisionProvider AckDecisionProvider
        {
            get
            {
                return this.ackDecisionProvider ?? new AckDecisionProvider((_, __) => AckDecisionEnum.OnlyNack);
            }
        }

        public IConnectionConfiguration ConnectionConfiguration
        {
            get
            {
                return this.connectionConfiguration;
            }
        }

        public TimeSpan? QueueMessageTTL
        {
            get
            {
                return this.queueMessageTTL;
            }
        }

        public ISecurityCofiguration SecurityConfiguration
        {
            get
            {
                return this.securityConfiguration;
            }
        }

        public bool DurableExchange { get; private set; }

        public bool DurableQueue { get; private set; }

        public ExchangeTypeEnum ExchangeType { get; private set; }

        public void CreateDeadLetterExchangeConfiguration(TimeSpan queueMessagesTTL, string suffix, bool cyclcDLQ)
        {
            this.CreateDeadLetterExchangeConfiguration(queueMessagesTTL, null, suffix, cyclcDLQ);
        }

        public void CreateDeadLetterExchangeConfiguration(TimeSpan queueMessagesTTL, DeadLetterRouteKeyProvider deadLetterRouteKeyProvider, string suffix, bool cyclcDLQ)
        {
            this.deadLetterExchangeConfiguration = new DeadLetterExchangeConfiguration(this, queueMessagesTTL, deadLetterRouteKeyProvider, suffix, cyclcDLQ);
        }

        public void CreateAckDecision(AckDecisionProvider ackDecisionProvider)
        {
            this.ackDecisionProvider = ackDecisionProvider;
        }

        public void CreateAckDecisionByMessageRetryManager(IMessageRetryManager messageRetryManager)
        {
            this.ackDecisionProvider =
                new AckDecisionProvider((exception, retryCount) =>
                {
                    return
                        messageRetryManager.MustRetry(exception, retryCount) ?
                        AckDecisionEnum.OnlyNack :
                        AckDecisionEnum.Ack;
                });
        }

        public void CreateDefaultTls12SecurityConfiguration(string certificateCommonName)
        {
            if (string.IsNullOrEmpty(certificateCommonName)) throw new ArgumentNullException("certificateCommonName");

            this.securityConfiguration = new Tls12WithoutCertificateSecurityConfiguration(certificateCommonName);
        }

        public void SetQueueMessageTTL(TimeSpan queueMessageTTL)
        {
            this.queueMessageTTL = queueMessageTTL;
        }

        public void SetExchangeType(ExchangeTypeEnum exchangeType)
        {
            this.ExchangeType = exchangeType;
        }

        public void UseDurableExchange()
        {
            this.DurableExchange = true;
        }

        public void UseDurableQueue()
        {
            this.DurableQueue = true;
        }

        public IEventBusConfiguration GetEventBusConfiguration()
        {
            return this as IEventBusConfiguration;
        }

        public IAckConfiguration GetAckConfiguration()
        {
            return this as IAckConfiguration;
        }

        public IQueueConfiguration GetQueueConfiguration()
        {
            return this as IQueueConfiguration;
        }

        public void UseToLog(Action<string> logAction)
        {
            this.UseToLog(new DefaultLogger(logAction));
        }

        public void UseToResolveTypes(Func<Type, object> typeResolverFunc)
        {
            this.UseToResolveTypes(new DefaultMessageTypeResolver(typeResolverFunc));
        }

        public void UseToLog(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            this.logger = logger;
        }

        public void UseToResolveTypes(IMessageTypeResolver typeResolver)
        {
            if (typeResolver == null) throw new ArgumentNullException("typeResolver");
            this.messageTypeResolver = typeResolver;
        }
    }
}
