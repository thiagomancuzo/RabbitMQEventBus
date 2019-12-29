using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.Resilience
{
    public interface IMessageRetryManager
    {
        bool MustRetry(Exception exception, int retryCount);
    }
}
