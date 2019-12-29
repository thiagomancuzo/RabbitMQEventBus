using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public delegate string DeadLetterRouteKeyProvider(string routeKey);
    public class DeadLetterExchangeConfiguration
    {
        private readonly RabbitMQEventBusConfiguration rabbitMQEventBusConfiguration;

        private readonly string deadLetterQueueName;
        private readonly string deadLetterExchangeName;
        private readonly DeadLetterRouteKeyProvider deadLetterRouteKeyProvider;
        private readonly TimeSpan queueMessagesTTL;
        private readonly string suffix;
        private readonly bool cyclicDLQ;

        public DeadLetterExchangeConfiguration(RabbitMQEventBusConfiguration rabbitMQEventBusConfiguration, TimeSpan queueMessagesTTL, DeadLetterRouteKeyProvider deadLetterRouteKeyProvider, string suffix, bool cyclicDLQ)
        {
            this.rabbitMQEventBusConfiguration = rabbitMQEventBusConfiguration;

            this.suffix = suffix;
            this.cyclicDLQ = cyclicDLQ;

            this.deadLetterExchangeName = string.Format("{0}_{1}", rabbitMQEventBusConfiguration.ExchangeName, suffix);
            this.deadLetterQueueName = string.Format("{0}_{1}", rabbitMQEventBusConfiguration.QueueName, suffix);
            this.deadLetterRouteKeyProvider = deadLetterRouteKeyProvider ?? new DeadLetterRouteKeyProvider((routeKey) => string.Format("{0}.{1}", routeKey, suffix));
            this.queueMessagesTTL = queueMessagesTTL;
        }

        public string DeadLetterQueueName { get { return this.deadLetterQueueName; } }
        public string DeadLetterExchangeName { get { return this.deadLetterExchangeName; } }
        public DeadLetterRouteKeyProvider DeadLetterRouteKeyProvider { get { return this.deadLetterRouteKeyProvider; } }
        public TimeSpan QueueMessagesTTL { get { return this.queueMessagesTTL; } }        
        public bool CyclicDLQ { get { return this.cyclicDLQ; } }
    }
}
