using System;
using System.Threading;

namespace MartinSu.Concurrency.Fibers
{
    /// <summary>
    /// PoolFiber which supports pausing
    /// </summary>
    public class ExtendedPoolFiber : PoolFiber, IExtendedFiber
    {
        private readonly IExtendedExecutor executor;

        public IExtendedExecutor Executor
        {
            get
            {
                return this.executor;
            }
        }

        public bool IsPaused
        {
            get
            {
                return this.started == 3;
            }
        }

        public ExtendedPoolFiber(IExtendedExecutor executor) : base(executor)
        {
            if (executor == null)
            {
                throw new ArgumentNullException("executor");
            }
            this.executor = executor;
        }

        public void Pause()
        {
            if (this.started == 2)
            {
                throw new ThreadStateException("Already stopped");
            }
            if (this.started != 1)
            {
                return;
            }
            this.executor.Pause();
            Interlocked.Exchange(ref this.started, 3);
        }

        public void Resume(Action executeFirstAction = null)
        {
            if (this.started != 3)
            {
                throw new ThreadStateException("Fiber is not paused");
            }
            int result = Interlocked.CompareExchange(ref this.started, 1, 3);
            if (result != 3)
            {
                return;
            }
            this.executor.Resume(executeFirstAction);
            base.Enqueue(delegate
            {
            });
        }
    }
}
