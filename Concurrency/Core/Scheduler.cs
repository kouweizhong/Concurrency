using System;
using System.Collections.Generic;
using System.Threading;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    ///  Enqueues actions on to context after schedule elapses.  
    /// </summary>
    public class Scheduler : ISchedulerRegistry, IScheduler, IDisposable
    {
        private volatile bool _running = true;

        private readonly IExecutionContext _executionContext;

        private List<IDisposable> _pending = new List<IDisposable>();

        internal int PendingCount
        {
            get
            {
                if (this._pending != null)
                {
                    return this._pending.Count;
                }
                return 0;
            }
        }

        /// <summary>
        ///  Constructs new instance.
        /// </summary>
        public Scheduler(IExecutionContext executionContext)
        {
            this._executionContext = executionContext;
        }

        /// <summary>
        ///  Enqueues action on to context after timer elapses.  
        /// </summary>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (firstInMs <= 0L)
            {
                PendingAction pending = new PendingAction(action);
                this._executionContext.Enqueue(new Action(pending.Execute));
                return pending;
            }
            TimerAction pending2 = new TimerAction(this, action, firstInMs, -1L);
            this.AddPending(pending2);
            return pending2;
        }

        /// <summary>
        ///  Enqueues actions on to context after schedule elapses.  
        /// </summary>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            TimerAction pending = new TimerAction(this, action, firstInMs, regularInMs);
            this.AddPending(pending);
            return pending;
        }

        /// <summary>
        ///  Removes a pending scheduled action.
        /// </summary>
        /// <param name="toRemove"></param>
        public void Remove(IDisposable toRemove)
        {
            this._executionContext.Enqueue(delegate
            {
                this._pending.Remove(toRemove);
            });
        }

        /// <summary>
        ///  Enqueues actions on to context immediately.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            this._executionContext.Enqueue(action);
        }

        private void AddPending(TimerAction pending)
        {
            Action addAction = delegate
            {
                if (this._running)
                {
                    this._pending.Add(pending);
                    pending.Schedule();
                }
            };
            this._executionContext.Enqueue(addAction);
        }

        /// <summary>
        ///  Cancels all pending actions
        /// </summary>
        public void Dispose()
        {
            this._running = false;
            List<IDisposable> old = Interlocked.Exchange<List<IDisposable>>(ref this._pending, new List<IDisposable>());
            foreach (IDisposable timer in old)
            {
                timer.Dispose();
            }
        }
    }
}
