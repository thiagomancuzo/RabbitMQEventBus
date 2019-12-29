using Framework.Messaging.EventBus.Bus;
using Framework.Messaging.EventBus.Events;
using Framework.Messaging.EventBus.Handlers;
using Framework.Messaging.EventBus.RabbitMQ.Envelopes;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.RabbitMQ
{
    public interface IRabbitMQEventBus : IEventBus
    {
        void Subscribe<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>;

        void Publish<T>(Envelope<T> envelope);

        Task PublishAsync<T>(Envelope<T> envelope);

        void Publish(Envelope envelope);

        Task PublishAsync(Envelope envelope);

        uint MessageCount();

        bool IsConnected { get; }
    }
}
