using System;

namespace Framework.Messaging.EventBus.Configurations
{
    public interface IMessageTypeResolver
    {
        object Resolve(Type type);
    }
}
