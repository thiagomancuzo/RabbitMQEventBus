using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public class UriConnectionConfiguration : IUriConnectionConfiguration
    {
        public UriConnectionConfiguration(Uri connectionUri)
        {
            this.ConnectionUri = connectionUri;
        }

        public Uri ConnectionUri { get; private set; }

        public ConnectionTypeEnum ConnectionType
        {
            get { return ConnectionTypeEnum.ConnectionUri; }
        }
    }
}
