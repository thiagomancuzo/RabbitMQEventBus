using Framework.Messaging.EventBus.Bus;
using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Providers;
using Framework.Messaging.EventBus.RabbitMQ.Configuration;
using Framework.Messaging.EventBus.RabbitMQ.Configuration.Security;
using Framework.Messaging.EventBus.RabbitMQ.Connections;
using RabbitMQ.Client;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Factory
{
    public class RabbitMQEventBusProvider : EventBusProvider
    {
        public RabbitMQEventBusProvider(IEventBusConfiguration configuration) : base(configuration)
        {
        }

        public override IEventBus Create()
        {
            var configuration = (IConfiguration)base.Configuration;
            var queueConfiguration = configuration.GetQueueConfiguration();
            var ackConfiguration = configuration.GetAckConfiguration();
            var eventBusConfiguration = configuration.GetEventBusConfiguration();
            var securityConfiguration = configuration.SecurityConfiguration;

            ConnectionFactory factory;

            if (configuration.ConnectionConfiguration.ConnectionType == ConnectionTypeEnum.ConnectionUri)
            {
                var connectionConfiguration = configuration.ConnectionConfiguration as IUriConnectionConfiguration;
                factory = new ConnectionFactory
                {
                    Uri = connectionConfiguration.ConnectionUri,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                    RequestedHeartbeat = 10
                };
            }
            else
            {
                var connectionConfiguration = configuration.ConnectionConfiguration as ICredentialsConnectionConfiguration;
                factory = new ConnectionFactory
                {
                    UserName = connectionConfiguration.UserName,
                    Password = connectionConfiguration.Password,
                    HostName = connectionConfiguration.HostName,
                    Port = connectionConfiguration.Port,
                    VirtualHost = connectionConfiguration.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                    RequestedHeartbeat = 10
                };
            }

            if (securityConfiguration != null)
            {
                factory.Ssl = securityConfiguration.ToSslOption();
            }

            return new RabbitMQEventBus(
                new PersisterConnection(factory, configuration.Logger),
                eventBusConfiguration,
                queueConfiguration,
                ackConfiguration);
        }
    }
}
