using System;

namespace Framework.Messaging.EventBus.RabbitMQ.Integrity.Flow
{
    internal class OnSetRunner
    {
        private readonly object locker = new object();
        private bool mustRun;

        public OnSetRunner()
        {
            mustRun = false;
        }

        public OnSetRunner(bool mustRun)
        {
            this.mustRun = mustRun;
        }

        public void SetToRun()
        {
            lock (this.locker)
            {
                this.mustRun = true;
            }
        }

        public bool RunIfSet(Action action)
        {
            lock (this.locker)
            {
                if (!this.mustRun) return false;
                this.mustRun = false;
            }

            action();

            return true;
        }
    }
}
