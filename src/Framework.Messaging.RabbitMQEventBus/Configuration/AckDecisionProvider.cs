using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public delegate AckDecisionEnum AckDecisionProvider(Exception exception, int retryCount);
}
