using System;

namespace MartinSu.Concurrency.Core
{
    internal class RecurringEvent : IPendingEvent, IDisposable
    {
        private readonly IExecutionContext _executionContext;

        private readonly Action _toExecute;

        private readonly long _regularInterval;

        private long _expiration;

        private bool _canceled;

        public long Expiration
        {
            get
            {
                return this._expiration;
            }
        }

        public RecurringEvent(IExecutionContext executionContext, Action toExecute, long scheduledTimeInMs, long regularInterval, long currentTime)
        {
            this._expiration = currentTime + scheduledTimeInMs;
            this._executionContext = executionContext;
            this._toExecute = toExecute;
            this._regularInterval = regularInterval;
        }

        public IPendingEvent Execute(long currentTime)
        {
            if (!this._canceled)
            {
                this._executionContext.Enqueue(this._toExecute);
                this._expiration = currentTime + this._regularInterval;
                return this;
            }
            return null;
        }

        public void Dispose()
        {
            this._canceled = true;
        }
    }
}
