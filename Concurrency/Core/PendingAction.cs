using System;

namespace MartinSu.Concurrency.Core
{
    internal class PendingAction : IDisposable
    {
        private readonly Action _action;

        private bool _cancelled;

        public PendingAction(Action action)
        {
            this._action = action;
        }

        public void Dispose()
        {
            this._cancelled = true;
        }

        public void Execute()
        {
            if (!this._cancelled)
            {
                this._action();
            }
        }
    }
}
