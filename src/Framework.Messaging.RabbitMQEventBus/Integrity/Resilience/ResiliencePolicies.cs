using Framework.Messaging.EventBus.Configurations;
using Polly;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;

namespace Framework.Messaging.EventBus.RabbitMQ.Integrity.Resilience
{
    public delegate Policy PolicyFactory(ILogger logger);

    internal static class  ResiliencePolicies
    {
        public static readonly PolicyFactory ConnectionPolicyFactory = (logger) =>
        {
            return Policy.Handle<SocketException>()
                .Or<BrokerUnreachableException>()
                .WaitAndRetry(8, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) => { logger.LogCritical(ex.ToString()); }
                );
        };

        public static readonly PolicyFactory GeneralRetryPolicyFactory = (logger) =>
        {
            return Policy.Handle<Exception>()
                    .WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(2),
                    (ex, time) => { logger.LogCritical(ex.ToString()); }
                );
        };
    }
}
