using Framework.Messaging.EventBus.Events;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.Publishers
{
    public interface IProducer<T>
        where T : BasicEvent
    {
        Task Publish(T message);
    }
}
