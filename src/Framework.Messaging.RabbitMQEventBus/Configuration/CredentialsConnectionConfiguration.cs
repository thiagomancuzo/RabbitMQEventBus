namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public class CredentialsConnectionConfiguration : ICredentialsConnectionConfiguration
    {

        public CredentialsConnectionConfiguration(string userName, string password, string hostName, int port)
            : this(userName, password, hostName, port, "/")
        { }

        public CredentialsConnectionConfiguration(string userName, string password, string hostName, int port, string virtualHost)
        {
            this.UserName = userName;
            this.Password = password;
            this.HostName = hostName;
            this.Port = port;
            this.VirtualHost = virtualHost;
        }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public string HostName { get; private set; }

        public int Port { get; private set; }

        public string VirtualHost { get; private set; }

        public ConnectionTypeEnum ConnectionType
        {
            get
            {
                return ConnectionTypeEnum.Credentials;
            }
        }
    }
}
