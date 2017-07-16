using MartinSu.Concurrency.Core;
using System;
using System.ComponentModel;

namespace MartinSu.Concurrency.Fibers
{
    internal class FormAdapter : IExecutionContext
    {
        private readonly ISynchronizeInvoke _invoker;

        public FormAdapter(ISynchronizeInvoke invoker)
        {
            this._invoker = invoker;
        }

        public void Enqueue(Action action)
        {
            this._invoker.BeginInvoke(action, null);
        }
    }
}
