namespace Framework.Messaging.EventBus.Configurations
{
    public interface IEventBusConfiguration
    {
        ILogger Logger { get; }
        IMessageTypeResolver MessageTypeResolver { get; }
    }
}
