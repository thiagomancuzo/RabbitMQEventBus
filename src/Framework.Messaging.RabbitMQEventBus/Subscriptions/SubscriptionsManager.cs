using Framework.Messaging.EventBus.Events;
using Framework.Messaging.EventBus.Handlers;
using Framework.Messaging.EventBus.RabbitMQ.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Messaging.EventBus.RabbitMQ.Subscriptions
{
    public class SubscriptionsManager
    {
        private readonly IDictionary<string, IList<Subscription>> _handlers = new Dictionary<string, IList<Subscription>>();

        public bool IsEmpty
        {
            get
            {
                return !_handlers.Keys.Any();
            }
        }

        public event EventHandler<EventEventArgs> OnEventRemoved;
        public event EventHandler<EventEventArgs> OnEventAdded;

        public void AddSubscription<TEvent, TEventHandler>()
            where TEventHandler : IEventHandler<TEvent>
        {
            AddSubscription(
                typeof(TEventHandler),
                typeof(TEvent).Name,
                typeof(TEvent)
                );
        }

        public void AddSubscription<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>
        {
            AddSubscription(
                typeof(TEventHandler),
                routingKey,
                typeof(TEvent)
                );
        }

        private void AddSubscription(Type handlerType, string eventName, Type eventType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<Subscription>());
                if (OnEventAdded != null)
                {
                    OnEventAdded.Invoke(this, new EventEventArgs(eventName));
                }
            }

            if (((List<Subscription>)_handlers[eventName]).Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    string.Format("Handler Type {0} already registered for '{1}'", handlerType.Name, eventName), "handlerType");
            }

            _handlers[eventName].Add(Subscription.New(
                handlerType, eventType)
                );
        }

        public void RemoveSubscription<TEvent, TEventHandler>()
            where TEventHandler : IEventHandler<TEvent>
        {

            var eventName = typeof(TEvent).Name;
            var handlerToRemove = FindSubscriptionToRemove<TEventHandler>(eventName);
            RemoveSubscription(eventName, handlerToRemove);
        }

        public void RemoveSubscription<TEvent, TEventHandler>(string routingKey)
            where TEventHandler : IEventHandler<TEvent>
        {

            var handlerToRemove = FindSubscriptionToRemove<TEventHandler>(routingKey);
            RemoveSubscription(routingKey, handlerToRemove);
        }

        Subscription FindSubscriptionToRemove<TEventHandler>(string eventName)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == typeof(TEventHandler));
        }

        private void RemoveSubscription(
            string eventName,
            Subscription subsToRemove
            )
        {
            if (subsToRemove == null) return;
            
            // a necessidade da reatribuição abaixo é justificada pelo uso desta lista durante o consumo. 
            // se a mesma for modificada, será gerada uma exceção de coleção modificada no consumo de mensagens.
            _handlers[eventName] = new List<Subscription>(_handlers[eventName]);
            _handlers[eventName].Remove(subsToRemove);

            if (_handlers[eventName].Any()) return;

            _handlers.Remove(eventName);
            if (OnEventRemoved != null)
            {
                OnEventRemoved.Invoke(this, new EventEventArgs(eventName));
            }
        }

        public bool HasSubscriptionsForEvent(string eventName)
        {
            return _handlers.ContainsKey(eventName);
        }

        public IEnumerable<Subscription> GetHandlersForEvent(string eventName)
        {
            return _handlers[eventName];
        }

        public Type GetEventTypeByName(string eventName)
        {
            if (_handlers[eventName] != null)
            {
                var first = _handlers[eventName].FirstOrDefault();
                if(first != null)
                {
                    return first.EventType;
                }
            }

            return null;
        }
    }
}
