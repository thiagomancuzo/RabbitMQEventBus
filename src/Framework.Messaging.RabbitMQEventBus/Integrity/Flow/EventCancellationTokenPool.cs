using Framework.Messaging.EventBus.RabbitMQ.Events;
using Framework.Messaging.EventBus.RabbitMQ.Subscriptions;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Framework.Messaging.EventBus.RabbitMQ.Integrity.Flow
{
    internal class EventCancellationTokenPool : IDisposable
    {
        private static readonly object locker = new object();

        private readonly SubscriptionsManager subscriptionsManager;
        private readonly IDictionary<string, CancellationTokenSource> cancellationTokenPerSubscriptionDictionary = new Dictionary<string, CancellationTokenSource>();

        public EventCancellationTokenPool(SubscriptionsManager subscriptionsManager)
        {
            this.subscriptionsManager = subscriptionsManager;

            subscriptionsManager.OnEventAdded += OnEventAdded;
            subscriptionsManager.OnEventRemoved += OnEventRemoved;
        }

        private void OnEventAdded(object sender, EventEventArgs e)
        {
            lock(locker)
            {
                if(!cancellationTokenPerSubscriptionDictionary.ContainsKey(e.EventName))
                {
                    cancellationTokenPerSubscriptionDictionary.Add(e.EventName, new CancellationTokenSource());
                }
                else
                {
                    CancellationTokenSource cancellationTokenSource;
                    if (cancellationTokenPerSubscriptionDictionary.TryGetValue(e.EventName, out cancellationTokenSource))
                    {
                        cancellationTokenSource.Dispose();
                        cancellationTokenPerSubscriptionDictionary[e.EventName] = new CancellationTokenSource();
                    }
                }
            }
        }

        private void OnEventRemoved(object _, EventEventArgs e)
        {
            lock (locker)
            {
                CancellationTokenSource tokenSource;
                if (cancellationTokenPerSubscriptionDictionary.TryGetValue(e.EventName, out tokenSource))
                {
                    tokenSource.Cancel();
                    cancellationTokenPerSubscriptionDictionary.Remove(e.EventName);
                }
            }
        }

        public CancellationToken GetCancellationToken(string eventName)
        {
            CancellationTokenSource tokenSource;
            cancellationTokenPerSubscriptionDictionary.TryGetValue(eventName, out tokenSource);

            return tokenSource != null ? tokenSource.Token : CancellationToken.None;
        }

        public void Dispose()
        {
            try
            {
                lock(locker)
                {
                    if(cancellationTokenPerSubscriptionDictionary.Values.Count > 0)
                    {
                        ((List<CancellationTokenSource>)cancellationTokenPerSubscriptionDictionary.Values).ForEach(x => { x.Dispose(); });
                        cancellationTokenPerSubscriptionDictionary.Clear();
                    }
                }
            }
            catch(Exception ex) { }
            
        }
    }
}
