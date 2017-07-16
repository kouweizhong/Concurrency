using System;

namespace MartinSu.Concurrency.Core
{
    internal class SingleEvent : IPendingEvent, IDisposable
    {
        private readonly IExecutionContext _executionContext;

        private readonly Action _toExecute;

        private readonly long _expiration;

        private bool _canceled;

        public long Expiration
        {
            get
            {
                return this._expiration;
            }
        }

        public SingleEvent(IExecutionContext executionContext, Action toExecute, long scheduledTimeInMs, long now)
        {
            this._expiration = now + scheduledTimeInMs;
            this._executionContext = executionContext;
            this._toExecute = toExecute;
        }

        public IPendingEvent Execute(long currentTime)
        {
            if (!this._canceled)
            {
                this._executionContext.Enqueue(this._toExecute);
            }
            return null;
        }

        public void Dispose()
        {
            this._canceled = true;
        }
    }
}
