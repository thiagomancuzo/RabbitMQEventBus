namespace Framework.Messaging.EventBus.RabbitMQ.Configuration
{
    public interface IAckConfiguration
    {
        AckDecisionProvider AckDecisionProvider { get; }
    }
}
