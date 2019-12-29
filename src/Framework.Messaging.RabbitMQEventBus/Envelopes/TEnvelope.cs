using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Envelopes
{
    public class Envelope<T> : BaseEnvelope
    {
        public Envelope(T message, string routingKey) : base(routingKey)
        {
            if (message == null) throw new ArgumentNullException("message");

            this.Message = message;
        }

        public T Message { get; private set; }
    }
}
