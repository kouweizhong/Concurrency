using System;

namespace MartinSu.Concurrency.Fibers
{
    public interface IExtendedFiber
    {
        IExtendedExecutor Executor
        {
            get;
        }

        bool IsPaused
        {
            get;
        }

        void Pause();

        void Resume(Action executeFirstAction = null);
    }
}
