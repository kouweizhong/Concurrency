using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    ///  For use only in testing.  Allows for controlled execution of scheduled actions on the StubFiber.
    /// </summary>
    public class StubScheduledAction : IDisposable
    {
        private readonly Action _action;

        private readonly long _firstIntervalInMs;

        private readonly long _intervalInMs;

        private readonly List<StubScheduledAction> _registry;

        /// <summary>
        ///  First interval in milliseconds.
        /// </summary>
        public long FirstIntervalInMs
        {
            get
            {
                return this._firstIntervalInMs;
            }
        }

        /// <summary>
        ///  Recurring interval in milliseconds.
        /// </summary>
        public long IntervalInMs
        {
            get
            {
                return this._intervalInMs;
            }
        }

        /// <summary>
        ///  Use for recurring scheduled actions.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstIntervalInMs"></param>
        /// <param name="intervalInMs"></param>
        /// <param name="registry"></param>
        public StubScheduledAction(Action action, long firstIntervalInMs, long intervalInMs, List<StubScheduledAction> registry)
        {
            this._action = action;
            this._firstIntervalInMs = firstIntervalInMs;
            this._intervalInMs = intervalInMs;
            this._registry = registry;
        }

        /// <summary>
        ///  Use for scheduled actions that only occur once.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="timeTilEnqueueInMs"></param>
        /// <param name="registry"></param>
        public StubScheduledAction(Action action, long timeTilEnqueueInMs, List<StubScheduledAction> registry) : this(action, timeTilEnqueueInMs, -1L, registry)
        {
        }

        /// <summary>
        ///  Executes the scheduled action.  If the action is not recurring it will be removed from the registry.
        /// </summary>
        public void Execute()
        {
            this._action();
            if (this._intervalInMs == -1L)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Cancels scheduled action.  Removes scheduled action from registry.
        /// </summary>
        public void Dispose()
        {
            this._registry.Remove(this);
        }
    }
}
