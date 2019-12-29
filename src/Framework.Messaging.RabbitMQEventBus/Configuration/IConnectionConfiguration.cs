using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Resilience;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public interface IConnectionConfiguration
    {
        ConnectionTypeEnum ConnectionType { get; }
    }
}
