using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Events
{
    public class EventEventArgs : EventArgs
    {
        public EventEventArgs(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; private set; }
    }
}
