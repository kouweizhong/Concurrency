using System;
using System.Collections.Generic;
using System.Threading;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    /// Queue with bounded capacity.  Will throw exception if capacity does not recede prior to wait time.
    /// </summary>
    public class BoundedQueue : IQueue
    {
        private readonly object _lock = new object();

        private readonly IExecutor _executor;

        private bool _running = true;

        private int _maxQueueDepth = -1;

        private int _maxEnqueueWaitTime;

        private List<Action> _actions = new List<Action>();

        private List<Action> _toPass = new List<Action>();

        /// <summary>
        /// Max number of actions to be queued.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return this._maxQueueDepth;
            }
            set
            {
                this._maxQueueDepth = value;
            }
        }

        /// <summary>
        /// Max time to wait for space in the queue.
        /// </summary>
        public int MaxEnqueueWaitTime
        {
            get
            {
                return this._maxEnqueueWaitTime;
            }
            set
            {
                this._maxEnqueueWaitTime = value;
            }
        }

        /// <summary>
        ///  Creates a bounded queue with a custom executor.
        /// </summary>
        /// <param name="executor"></param>
        public BoundedQueue(IExecutor executor)
        {
            this._executor = executor;
        }

        /// <summary>
        ///  Creates a bounded queue with the default executor.
        /// </summary>
        public BoundedQueue() : this(new DefaultExecutor())
        {
        }

        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (this._lock)
            {
                if (this.SpaceAvailable(1))
                {
                    this._actions.Add(action);
                    Monitor.PulseAll(this._lock);
                }
            }
        }

        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            while (this.ExecuteNextBatch())
            {
            }
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            lock (this._lock)
            {
                this._running = false;
                Monitor.PulseAll(this._lock);
            }
        }

        private bool SpaceAvailable(int toAdd)
        {
            if (!this._running)
            {
                return false;
            }
            while (this._maxQueueDepth > 0 && this._actions.Count + toAdd > this._maxQueueDepth)
            {
                if (this._maxEnqueueWaitTime <= 0)
                {
                    throw new QueueFullException(this._actions.Count);
                }
                Monitor.Wait(this._lock, this._maxEnqueueWaitTime);
                if (!this._running)
                {
                    return false;
                }
                if (this._maxQueueDepth > 0 && this._actions.Count + toAdd > this._maxQueueDepth)
                {
                    throw new QueueFullException(this._actions.Count);
                }
            }
            return true;
        }

        private List<Action> DequeueAll()
        {
            List<Action> result;
            lock (this._lock)
            {
                if (this.ReadyToDequeue())
                {
                    Lists.Swap(ref this._actions, ref this._toPass);
                    this._actions.Clear();
                    Monitor.PulseAll(this._lock);
                    result = this._toPass;
                }
                else
                {
                    result = null;
                }
            }
            return result;
        }

        private bool ReadyToDequeue()
        {
            while (this._actions.Count == 0 && this._running)
            {
                Monitor.Wait(this._lock);
            }
            return this._running;
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteNextBatch()
        {
            List<Action> toExecute = this.DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            this._executor.Execute(toExecute);
            return true;
        }
    }
}
