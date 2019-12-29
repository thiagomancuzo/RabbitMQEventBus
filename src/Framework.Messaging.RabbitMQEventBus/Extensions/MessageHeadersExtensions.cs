using Framework.Messaging.EventBus.RabbitMQ.Internal;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Extensions
{
    internal static class MessageHeadersExtensions
    {
        internal static int GetXDeathCount(this MessageHeaders messageHeaders) => Convert.ToInt32(messageHeaders.GetValue("x-death", "count"));
    }
}
