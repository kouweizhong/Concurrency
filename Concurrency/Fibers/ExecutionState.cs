using System;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    ///  Fiber execution state management
    /// </summary>
    public enum ExecutionState
    {
        /// <summary>
        ///  Created but not running
        /// </summary>
        Created,
        /// <summary>
        ///  After start
        /// </summary>
        Running,
        /// <summary>
        ///  After stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// State after Pause
        /// </summary>
        Paused
    }
}
