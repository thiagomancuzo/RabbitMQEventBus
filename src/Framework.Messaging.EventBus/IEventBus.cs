using Framework.Messaging.EventBus.Events;
using Framework.Messaging.EventBus.Handlers;
using System;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.Bus
{
    public interface IEventBus : IDisposable
    {
        void Publish(BasicEvent @event);

        Task PublishAsync(BasicEvent @event);

        void Subscribe<TEvent, TEventHandler>()
            where TEvent : BasicEvent
            where TEventHandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : BasicEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}
