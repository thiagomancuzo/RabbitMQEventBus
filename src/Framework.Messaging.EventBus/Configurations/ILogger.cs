using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Messaging.EventBus.Configurations
{
    /// <summary>
    /// Interface criada para padronizar os logs. TODO : Necessário desacoplar este contrato em outro projeto futuramente.
    /// </summary>
    public interface ILogger
    {
        void LogWarning(string log);
        void LogInformation(string log);
        void LogCritical(string log);
    }
}
