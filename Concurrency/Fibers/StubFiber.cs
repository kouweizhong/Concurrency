using MartinSu.Concurrency.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    /// StubFiber does not use a backing thread or a thread pool for execution. Actions are added to pending
    /// lists for execution. These actions can be executed synchronously by the calling thread. This class
    /// is not thread safe and should not be used in production code. 
    ///
    /// The class is typically used for testing asynchronous code to make it completely synchronous and
    /// deterministic.
    /// </summary>
    public class StubFiber : IFiber, ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        private readonly List<Action> _pending = new List<Action>();

        private readonly List<StubScheduledAction> _scheduled = new List<StubScheduledAction>();

        private bool _root = true;

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
        /// All subscriptions.
        /// </summary>
        public List<IDisposable> Subscriptions
        {
            get
            {
                return this._subscriptions;
            }
        }

        /// <summary>
        /// All pending actions.
        /// </summary>
        public List<Action> Pending
        {
            get
            {
                return this._pending;
            }
        }

        /// <summary>
        /// All scheduled actions.
        /// </summary>
        public List<StubScheduledAction> Scheduled
        {
            get
            {
                return this._scheduled;
            }
        }

        /// <summary>
        /// If true events will be executed immediately rather than added to the pending list.
        /// </summary>
        public bool ExecutePendingImmediately
        {
            get;
            set;
        }

        /// <summary>
        /// No Op
        /// </summary>
        public void Start()
        {
        }

        /// <summary>
        /// Clears all subscriptions, scheduled, and pending actions.
        /// </summary>
        public void Dispose()
        {
            this._scheduled.ToList<StubScheduledAction>().ForEach(delegate (StubScheduledAction x)
            {
                x.Dispose();
            });
            this._subscriptions.ToList<IDisposable>().ForEach(delegate (IDisposable x)
            {
                x.Dispose();
            });
            this._pending.Clear();
        }

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            if (this._root && this.ExecutePendingImmediately)
            {
                try
                {
                    this._root = false;
                    action();
                    this.ExecuteAllPendingUntilEmpty();
                    return;
                }
                finally
                {
                    this._root = true;
                }
            }
            this._pending.Add(action);
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

        /// <summary>
        /// Adds a scheduled action to the list. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            StubScheduledAction toAdd = new StubScheduledAction(action, firstInMs, this._scheduled);
            this._scheduled.Add(toAdd);
            return toAdd;
        }

        /// <summary>
        /// Adds scheduled action to list.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            StubScheduledAction toAdd = new StubScheduledAction(action, firstInMs, regularInMs, this._scheduled);
            this._scheduled.Add(toAdd);
            return toAdd;
        }

        /// <summary>
        /// Execute all actions in the pending list.  If any of the executed actions enqueue more actions, execute those as well.
        /// </summary>
        public void ExecuteAllPendingUntilEmpty()
        {
            while (this._pending.Count > 0)
            {
                Action toExecute = this._pending[0];
                this._pending.RemoveAt(0);
                toExecute();
            }
        }

        /// <summary>
        /// Execute all actions in the pending list.
        /// </summary>
        public void ExecuteAllPending()
        {
            Action[] copy = this._pending.ToArray();
            this._pending.Clear();
            Action[] array = copy;
            for (int i = 0; i < array.Length; i++)
            {
                Action pending = array[i];
                pending();
            }
        }

        /// <summary>
        /// Execute all actions in the scheduled list.
        /// </summary>
        public void ExecuteAllScheduled()
        {
            StubScheduledAction[] array = this._scheduled.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                StubScheduledAction scheduled = array[i];
                scheduled.Execute();
            }
        }
    }
}
