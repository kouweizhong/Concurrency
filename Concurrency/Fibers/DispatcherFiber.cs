using MartinSu.Concurrency.Core;
using System;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    /// Adapts Dispatcher to a Fiber. Transparently moves actions onto the Dispatcher thread.
    /// </summary>
    public class DispatcherFiber : GuiFiber
    {
        /// <summary>
        /// Constructs a Fiber that executes on dispatcher thread.
        /// </summary>
        public DispatcherFiber(IExecutionContext context, IExecutor executor) : base(context, executor)
        {
        }
    }
}
