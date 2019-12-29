using Framework.Messaging.EventBus;
using Framework.Messaging.EventBus.Configurations;
using Framework.Messaging.EventBus.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.RabbitMQ.Subscriptions
{
    public class Subscription
    {
        public Type HandlerType { get; private set; }

        public Type EventType { get; private set; }

        private Subscription(Type handlerType, Type eventType)
        {
            HandlerType = handlerType;
            EventType = eventType;
        }

        public async Task Handle(string message, IMessageTypeResolver messageTypeResolver)
        {
            object eventData;
            if (EventType == typeof(string))
            {
                eventData = message;
            }
            else
            {
                eventData = JsonConvert.DeserializeObject(message, EventType, GlobalSettings.SerializerSettings);
            }
            var handler = messageTypeResolver.Resolve(HandlerType);
            var concreteType = typeof(IEventHandler<>).MakeGenericType(EventType);
            await (Task)concreteType.GetMethod("Handle")
                .Invoke(handler, new[] { eventData });
        }

        public static Subscription New(Type handlerType, Type eventType)
        {
            return new Subscription(handlerType, eventType);
        }
    }
}
