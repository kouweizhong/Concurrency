using MartinSu.Concurrency.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    ///  Allows interaction with Windows Forms.  Transparently moves actions onto the Form's thread.
    /// </summary>
    public class GuiFiber : IFiber, ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        private readonly Subscriptions _subscriptions = new Subscriptions();

        private readonly object _lock = new object();

        private readonly IExecutionContext _executionContext;

        private readonly Scheduler _timer;

        private readonly IExecutor _executor;

        private readonly List<Action> _queue = new List<Action>();

        private volatile ExecutionState _started;

        /// <summary>
        ///  Number of subscriptions.
        /// </summary>
        public int NumSubscriptions
        {
            get
            {
                return this._subscriptions.Count;
            }
        }

        /// <summary>
        /// Creates an instance.
        /// </summary>
        public GuiFiber(IExecutionContext executionContext, IExecutor executor)
        {
            this._timer = new Scheduler(this);
            this._executionContext = executionContext;
            this._executor = executor;
        }

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (this._started == ExecutionState.Stopped)
            {
                return;
            }
            if (this._started == ExecutionState.Created)
            {
                lock (this._lock)
                {
                    if (this._started == ExecutionState.Created)
                    {
                        this._queue.Add(action);
                        return;
                    }
                }
            }
            this._executionContext.Enqueue(delegate
            {
                this._executor.Execute(action);
            });
        }

        /// <summary>
        ///  Register subscription to be unsubcribed from when the fiber is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            this._subscriptions.Add(toAdd);
        }

        /// <summary>
        ///  Deregister a subscription.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return this._subscriptions.Remove(toRemove);
        }

        public IDisposable Schedule(Action action, long firstInMs)
        {
            return this._timer.Schedule(action, firstInMs);
        }
        
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return this._timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        public void Start()
        {
            if (this._started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            lock (this._lock)
            {
                List<Action> actions = this._queue.ToList<Action>();
                this._queue.Clear();
                if (actions.Count > 0)
                {
                    this._executionContext.Enqueue(delegate
                    {
                        this._executor.Execute(actions);
                    });
                }
                this._started = ExecutionState.Running;
            }
        }

        /// <summary>
        /// <see cref="M:System.IDisposable.Dispose" />
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Stop()
        {
            this._timer.Dispose();
            this._started = ExecutionState.Stopped;
            this._subscriptions.Dispose();
        }
    }
}
