using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration.Security
{
    public interface ISecurityCofiguration
    {
        bool Enabled { get; }

        string CertificateCommonName { get; }

        X509CertificateCollection Certificates { get; }

        SslProtocols Version { get; }
    }
}
