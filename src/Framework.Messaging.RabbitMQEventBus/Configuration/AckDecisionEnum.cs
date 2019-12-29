namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public enum AckDecisionEnum
    {
        Ack,
        OnlyNack,
        NackAndRequeue
    }
}
