using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.RabbitMQ.Integrity.Resilience;
using RabbitMQ.Client;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Connections
{

    public class PersisterConnection :
        IDisposable
    {

        private readonly IConnectionFactory _factory;
        private readonly ILogger _logger;

        public PersisterConnection(IConnectionFactory factory, ILogger logger)
        {
            _factory = factory;
            _logger = logger;
        }

        private IConnection _connection;
        public bool IsConnected
        {
            get
            {
                return _connection != null &&
                        _connection.IsOpen &&
                        !Disposed;
            }
        }

        private readonly object _lockObject = new object();


        public bool TryConnect(PolicyFactory persisterConnectionPolicyFactory)
        {
            lock (_lockObject)
            {
                if (IsConnected) return true;

                persisterConnectionPolicyFactory(_logger)
                    .Execute(() =>
                    {
                        _connection = _factory.CreateConnection();
                    });

                if (!IsConnected)
                {
                    return false;
                }

                return true;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException(
                    "There are RabbitMQ connections available to perform this action"
                    );
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (Disposed) return;
            Disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (Exception ex)
            {
            }
        }
        public bool Disposed { get; private set; }
    }
}
