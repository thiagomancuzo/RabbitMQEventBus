using RabbitMQ.Client;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration.Security
{
    internal interface ISecurityConfigurationParser
    {
        SslOption Parse(ISecurityCofiguration securityCofiguration);
    }
}
