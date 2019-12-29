using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.RabbitMQ.Configuration.Security;
using Framework.Messaging.EventBus.Resilience;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    // TODO: Garça - futuramente este contrato deverá ser refatorado para especificações mais configuráveis de forma mais elegante utilizando:
    // builder, imutabilidade, segregando as responsabilidades

    public interface IConfiguration : 
        IEventBusConfiguration, 
        IAckConfiguration, 
        IQueueConfiguration
    {
        IConnectionConfiguration ConnectionConfiguration { get; }

        ISecurityCofiguration SecurityConfiguration { get; }

        void SetQueueMessageTTL(TimeSpan queueMessageTTL);

        void CreateAckDecisionByMessageRetryManager(IMessageRetryManager messageRetryManager);

        void CreateDefaultTls12SecurityConfiguration(string certificateCommonName);

        void CreateDeadLetterExchangeConfiguration(TimeSpan queueMessagesTTL, DeadLetterRouteKeyProvider deadLetterRouteKeyProvider, string suffix, bool cyclcDLQ);

        void UseDurableExchange();

        void UseDurableQueue();

        void SetExchangeType(ExchangeTypeEnum exchangeType);

        void UseToLog(Action<string> logAction);

        void UseToResolveTypes(Func<Type, object> typeResolverFunc);

        void UseToLog(ILogger logger);

        void UseToResolveTypes(IMessageTypeResolver typeResolver);

        IEventBusConfiguration GetEventBusConfiguration();

        IAckConfiguration GetAckConfiguration();

        IQueueConfiguration GetQueueConfiguration();
    }
}
