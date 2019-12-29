using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Resilience;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public interface ICredentialsConnectionConfiguration : IConnectionConfiguration
    {
        string UserName { get; }

        string Password { get; }

        string HostName { get; }

        int Port { get; }

        string VirtualHost { get; }
    }
}
