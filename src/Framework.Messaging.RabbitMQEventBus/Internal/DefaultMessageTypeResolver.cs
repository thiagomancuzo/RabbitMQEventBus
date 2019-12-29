using System;
using Framework.Messaging.EventBus.Configurations;

namespace Framework.Messaging.EventBus.RabbitMQ.Internal
{
    internal class DefaultMessageTypeResolver : IMessageTypeResolver
    {
        public readonly Func<Type, object> typeResolverFunc;

        public DefaultMessageTypeResolver(Func<Type, object> typeResolverFunc)
        {
            if (typeResolverFunc == null) throw new ArgumentNullException("typeResolverFunc");

            this.typeResolverFunc = typeResolverFunc;
        }

        public object Resolve(Type type)
        {
            return this.typeResolverFunc(type);
        }
    }
}
