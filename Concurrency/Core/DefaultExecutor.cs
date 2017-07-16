using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    /// Default executor.
    /// </summary>
    public class DefaultExecutor : IExecutor
    {
        private bool _running = true;

        /// <summary>
        /// When disabled, actions will be ignored by executor. The executor is typically disabled at shutdown
        /// to prevent any pending actions from being executed. 
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return this._running;
            }
            set
            {
                this._running = value;
            }
        }

        /// <summary>
        /// Executes all actions.
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(List<Action> toExecute)
        {
            foreach (Action action in toExecute)
            {
                this.Execute(action);
            }
        }

        /// <summary>
        ///  Executes a single action. 
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(Action toExecute)
        {
            if (this._running)
            {
                toExecute();
            }
        }
    }
}
