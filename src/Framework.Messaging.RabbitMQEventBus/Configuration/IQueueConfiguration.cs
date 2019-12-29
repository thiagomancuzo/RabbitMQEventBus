using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public interface IQueueConfiguration
    {
        string ExchangeName { get; }

        string QueueName { get; }

        ushort PrefetchCount { get; }

        TimeSpan? QueueMessageTTL { get; }

        bool DurableExchange { get; }

        bool DurableQueue { get; }

        ExchangeTypeEnum ExchangeType { get; }

        DeadLetterExchangeConfiguration DeadLetterExchangeConfiguration { get; }

    }
}
