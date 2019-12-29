using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Resilience;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public interface IUriConnectionConfiguration : IConnectionConfiguration
    {
        Uri ConnectionUri { get; }
    }
}
