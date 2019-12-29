using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Envelopes
{
    public class Envelope : BaseEnvelope
    {
        public Envelope(object message, string routingKey) : base(routingKey)
        {
            if (message == null) throw new ArgumentNullException("message");

            this.Message = message;
        }

        public object Message { get; private set; }
    }
}
