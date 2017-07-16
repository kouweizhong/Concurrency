using System;
using System.Threading;

namespace MartinSu.Concurrency.Core
{
    internal class TimerAction : IDisposable
    {
        private readonly long _firstIntervalInMs;

        private readonly long _intervalInMs;

        private readonly ISchedulerRegistry _registry;

        private Action _action;

        private Timer _timer;

        private bool _cancelled;

        public TimerAction(ISchedulerRegistry registry, Action action, long firstIntervalInMs, long intervalInMs)
        {
            this._registry = registry;
            this._action = action;
            this._firstIntervalInMs = firstIntervalInMs;
            this._intervalInMs = intervalInMs;
        }

        public void Schedule()
        {
            this._timer = new Timer(delegate (object x)
            {
                this.ExecuteOnTimerThread();
            }, null, this._firstIntervalInMs, this._intervalInMs);
        }

        public void ExecuteOnTimerThread()
        {
            if (this._intervalInMs == -1L || this._cancelled)
            {
                this._registry.Remove(this);
                Timer timer = Interlocked.Exchange<Timer>(ref this._timer, null);
                if (timer != null)
                {
                    timer.Dispose();
                }
            }
            if (!this._cancelled)
            {
                this._registry.Enqueue(new Action(this.ExecuteOnFiberThread));
            }
        }

        public void ExecuteOnFiberThread()
        {
            if (!this._cancelled)
            {
                Action action = this._action;
                if (this._action != null)
                {
                    action();
                }
            }
        }

        public virtual void Dispose()
        {
            this._cancelled = true;
            this._action = null;
            this._registry.Remove(this);
            Timer timer = Interlocked.Exchange<Timer>(ref this._timer, null);
            if (timer != null)
            {
                timer.Dispose();
            }
        }
    }
}
