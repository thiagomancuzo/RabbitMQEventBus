using Newtonsoft.Json;

namespace Framework.Messaging.EventBus.RabbitMQ
{
    public static class GlobalSettings
    {
        static GlobalSettings()
        {
            SerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
        }

        public static JsonSerializerSettings SerializerSettings { get; set; }
    }
}
