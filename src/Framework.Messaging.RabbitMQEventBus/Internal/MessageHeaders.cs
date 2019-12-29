using System;
using System.Collections.Generic;

namespace Framework.Messaging.EventBus.RabbitMQ.Internal
{
    internal class MessageHeaders
    {
        private readonly IDictionary<string, object> headers;

        public MessageHeaders(IDictionary<string, object> headers)
        {
            this.headers = headers;
        }

        internal object GetValue(string headerName, string headerPropertyName)
        {
            if (headers == null) return null;
            if (!headers.ContainsKey(headerName)) return null;

            var xDeath = (List<Object>)headers[headerName];
            var mainQueue = ((Dictionary<string, Object>)xDeath[0]);
            if (mainQueue.ContainsKey(headerPropertyName))
            {
                return Convert.ToInt32(mainQueue[headerPropertyName]);
            }

            return null;
        }

    }
}
