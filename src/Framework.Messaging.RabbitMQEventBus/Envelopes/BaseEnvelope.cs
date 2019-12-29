using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Envelopes
{
    public abstract class BaseEnvelope
    {
        public BaseEnvelope(string routingKey)
        {
            if (string.IsNullOrEmpty(routingKey)) throw new ArgumentNullException("routingKey");

            this.RoutingKey = routingKey;
        }

        public string RoutingKey { get; private set; }
    }
}
