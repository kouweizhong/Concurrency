using MartinSu.Concurrency.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    /// Fiber that uses a thread pool for execution.
    /// </summary>
    public class PoolFiber : IFiber, ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        private readonly Subscriptions subscriptions = new Subscriptions();

        private readonly object @lock = new object();

        private readonly IThreadPool pool;

        private readonly Scheduler timer;

        private readonly IExecutor executor;

        private List<Action> queue = new List<Action>();

        private List<Action> toPass = new List<Action>();

        protected int started;

        private bool flushPending;

        internal Scheduler Scheduler
        {
            get
            {
                return this.timer;
            }
        }

        /// <summary>
        /// Number of subscriptions.
        /// </summary>
        public int NumSubscriptions
        {
            get
            {
                return this.subscriptions.Count;
            }
        }

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="executor"></param>
        public PoolFiber(IThreadPool pool, IExecutor executor)
        {
            this.timer = new Scheduler(this);
            this.pool = pool;
            this.executor = executor;
        }

        /// <summary>
        /// Create a pool fiber with the default thread pool.
        /// </summary>
        public PoolFiber(IExecutor executor) : this(new DefaultThreadPool(), executor)
        {
        }

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (this.@lock)
            {
                this.queue.Add(action);
                if (this.started == 0 || this.started == 3)
                {
                    return;
                }
                if (!this.flushPending)
                {
                    this.pool.Queue(new WaitCallback(this.Flush));
                    this.flushPending = true;
                }
            }
        }

        /// <summary>
        /// Register subscription to be unsubcribed from when the fiber is disposed.
        /// </summary>
        /// <param name="toAdd"></param>
        public void RegisterSubscription(IDisposable toAdd)
        {
            this.subscriptions.Add(toAdd);
        }

        /// <summary>
        /// Deregister a subscription.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return this.subscriptions.Remove(toRemove);
        }

        public IDisposable Schedule(Action action, long firstInMs)
        {
            return this.timer.Schedule(action, firstInMs);
        }

        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return this.timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        /// <summary>
        /// Start consuming actions.
        /// </summary>
        public void Start()
        {
            if (this.started == 1 || this.started == 3)
            {
                throw new ThreadStateException("Already Started");
            }
            this.started = 1;
            this.Enqueue(delegate
            {
            });
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            this.timer.Dispose();
            this.started = 2;
            this.subscriptions.Dispose();
        }

        /// <summary>
        /// Stops the fiber.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }

        private void Flush(object state)
        {
            List<Action> toExecute = this.ClearActions();
            if (toExecute == null)
            {
                return;
            }
            try
            {
                this.executor.Execute(toExecute);
            }
            finally
            {
                lock (this.@lock)
                {
                    if (this.queue.Count > 0)
                    {
                        this.pool.Queue(new WaitCallback(this.Flush));
                    }
                    else
                    {
                        this.flushPending = false;
                    }
                }
            }
        }

        private List<Action> ClearActions()
        {
            int numberOfItemsToPass;
            lock (this.@lock)
            {
                if (this.queue.Count == 0)
                {
                    this.flushPending = false;
                    return null;
                }
                Lists.Swap(ref this.queue, ref this.toPass);
                this.queue.Clear();
                numberOfItemsToPass = this.toPass.Count;
            }
            return this.toPass;
        }
    }
}
