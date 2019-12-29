using Framework.Messaging.EventBus;
using Framework.Messaging.EventBus.Bus;
using Framework.Messaging.EventBus.Configurations;
using System;

namespace Framework.Messaging.EventBus.Providers
{
    public abstract class EventBusProvider
    {
        protected readonly IEventBusConfiguration Configuration;
        public EventBusProvider(IEventBusConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public abstract IEventBus Create();
    }
}
