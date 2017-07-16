using MartinSu.Concurrency.Core;
using System;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    /// Extended executor should support pausing
    /// </summary>
    public interface IExtendedExecutor : IExecutor
    {
        bool IsPaused
        {
            get;
        }

        void Pause();

        void Resume(Action executeFirstAction = null);
    }
}
