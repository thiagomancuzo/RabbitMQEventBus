using RabbitMQ.Client;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration.Security
{
    internal class SecurityConfigurationParser : ISecurityConfigurationParser
    {
        public SslOption Parse(ISecurityCofiguration securityCofiguration)
        {
            return new SslOption(securityCofiguration.CertificateCommonName)
            {
                Enabled = securityCofiguration.Enabled,
                Certs = securityCofiguration.Certificates,
                Version = securityCofiguration.Version
            };
        }
    }

    internal static class SecurityConfigurationParserExtension
    {
        private static Lazy<ISecurityConfigurationParser> securityConfigurationParser = new Lazy<ISecurityConfigurationParser>(() => new SecurityConfigurationParser());
        internal static SslOption ToSslOption(this ISecurityCofiguration securityCofiguration)
        {
            return securityConfigurationParser.Value.Parse(securityCofiguration);
        }
    }
}
