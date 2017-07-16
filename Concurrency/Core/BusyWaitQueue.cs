using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    /// Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    public class BusyWaitQueue : IQueue
    {
        private readonly object _lock = new object();

        private readonly IExecutor _executor;

        private readonly int _spinsBeforeTimeCheck;

        private readonly int _msBeforeBlockingWait;

        private bool _running = true;

        private List<Action> _actions = new List<Action>();

        private List<Action> _toPass = new List<Action>();

        /// <summary>
        ///  BusyWaitQueue with custom executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="spinsBeforeTimeCheck"></param>
        /// <param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(IExecutor executor, int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            this._executor = executor;
            this._spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            this._msBeforeBlockingWait = msBeforeBlockingWait;
        }

        /// <summary>
        ///  BusyWaitQueue with default executor.
        /// </summary>
        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait) : this(new DefaultExecutor(), spinsBeforeTimeCheck, msBeforeBlockingWait)
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
            int spins = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(this._lock))
                    {
                    }
                    if (this._running)
                    {
                        List<Action> toReturn = this.TryDequeue();
                        if (toReturn != null)
                        {
                            List<Action> result = toReturn;
                            return result;
                        }
                        if (this.TryBlockingWait(stopwatch, ref spins))
                        {
                            if (!this._running)
                            {
                                break;
                            }
                            toReturn = this.TryDequeue();
                            if (toReturn != null)
                            {
                                List<Action> result = toReturn;
                                return result;
                            }
                        }
                        continue;
                    }
                }
                finally
                {
                    Monitor.Exit(this._lock);
                }
                break;
            }
            return null;
        }

        private bool TryBlockingWait(Stopwatch stopwatch, ref int spins)
        {
            if (spins++ < this._spinsBeforeTimeCheck)
            {
                return false;
            }
            spins = 0;
            if (stopwatch.ElapsedMilliseconds > (long)this._msBeforeBlockingWait)
            {
                Monitor.Wait(this._lock);
                stopwatch.Reset();
                stopwatch.Start();
                return true;
            }
            return false;
        }

        private List<Action> TryDequeue()
        {
            if (this._actions.Count > 0)
            {
                Lists.Swap(ref this._actions, ref this._toPass);
                this._actions.Clear();
                return this._toPass;
            }
            return null;
        }

        private bool ExecuteNextBatch()
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
