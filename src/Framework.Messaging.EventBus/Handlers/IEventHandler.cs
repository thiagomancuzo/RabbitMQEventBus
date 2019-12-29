using Framework.Messaging.EventBus.Events;
using System;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.Handlers
{
    public interface IEventHandler<TEvent>
    {
        Task Handle(TEvent @event);
    }
}
