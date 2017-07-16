using System;
using System.Collections.Generic;
using System.Threading;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    /// Default implementation.
    /// </summary>
    public class DefaultQueue : IQueue
    {
        private readonly object _lock = new object();

        private readonly IExecutor _executor;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();

        private List<Action> _toPass = new List<Action>();

        /// <summary>
        ///  Default queue with custom executor
        /// </summary>
        /// <param name="executor"></param>
        public DefaultQueue(IExecutor executor)
        {
            this._executor = executor;
        }

        /// <summary>
        ///  Default queue with default executor
        /// </summary>
        public DefaultQueue() : this(new DefaultExecutor())
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
                this._actions.Add(action);
                Monitor.PulseAll(this._lock);
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

        private List<Action> DequeueAll()
        {
            List<Action> result;
            lock (this._lock)
            {
                if (this.ReadyToDequeue())
                {
                    Lists.Swap(ref this._actions, ref this._toPass);
                    this._actions.Clear();
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
