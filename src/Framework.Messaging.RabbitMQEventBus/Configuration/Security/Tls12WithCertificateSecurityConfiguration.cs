﻿using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration.Security
{
    internal class Tls12WithCertificateSecurityConfiguration : ISecurityCofiguration
    {
        private readonly string certificateCommonName;
        private readonly X509CertificateCollection certificates;

        internal Tls12WithCertificateSecurityConfiguration(string certificateCommonName, X509Certificate2 certificate)
        {
            this.certificateCommonName = certificateCommonName;
            this.certificates = new X509CertificateCollection() { certificate };
        }

        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public string CertificateCommonName
        {
            get
            {
                return this.certificateCommonName;
            }
        }

        public X509CertificateCollection Certificates
        {
            get
            {
                return certificates;
            }
        }

        public SslProtocols Version
        {
            get
            {
                return SslProtocols.Tls12;
            }
        }
    }
}
