using Framework.Messaging.EventBus.Configurations;
using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Internal
{
    internal class DefaultLogger : ILogger
    {
        private readonly Action<string> logAction;

        public DefaultLogger(Action<string> logAction)
        {
            if (logAction == null) throw new ArgumentNullException("logAction");

            this.logAction = logAction;
        }

        public void LogCritical(string log)
        {
            try
            {
                this.logAction(log);
            }
            catch(Exception ex)
            {
                Console.WriteLine("An error was occurred during a log operation. \r\n Error: {0} \r\n Log: {1}", ex.ToString(), log);
            }
        }

        public void LogInformation(string log)
        {
            this.LogCritical(log);
        }

        public void LogWarning(string log)
        {
            this.LogCritical(log);
        }
    }
}
