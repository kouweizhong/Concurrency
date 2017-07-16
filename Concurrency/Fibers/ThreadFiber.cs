using MartinSu.Concurrency.Core;
using System;
using System.Threading;

namespace MartinSu.Concurrency.Fibers
{
    public class ThreadFiber : IFiber, ISubscriptionRegistry, IExecutionContext, IScheduler, IDisposable
    {
        private static int THREAD_COUNT;

        private readonly Subscriptions _subscriptions = new Subscriptions();

        private readonly Thread _thread;

        private readonly IQueue _queue;

        private readonly Scheduler _scheduler;

        public Thread Thread
        {
            get
            {
                return this._thread;
            }
        }

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
        /// Create a thread fiber with the default queue.
        /// </summary>
        public ThreadFiber() : this(new DefaultQueue())
        {
        }

        /// <summary>
        /// Creates a thread fiber with a specified queue.
        /// </summary>
        /// <param name="queue"></param>
        public ThreadFiber(IQueue queue) : this(queue, "ThreadFiber-" + ThreadFiber.GetNextThreadId())
        {
        }

        /// <summary>
        /// Creates a thread fiber with a specified name.
        /// </summary>
        /// /// <param name="threadName"></param>
        public ThreadFiber(string threadName) : this(new DefaultQueue(), threadName)
        {
        }

        public ThreadFiber(IQueue queue, string threadName) : this(queue, threadName, true, ThreadPriority.Normal)
        {
        }

        /// <summary>
        /// Creates a thread fiber.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="threadName"></param>
        /// <param name="isBackground"></param>
        /// <param name="priority"></param>
        public ThreadFiber(IQueue queue, string threadName, bool isBackground, ThreadPriority priority)
        {
            this._queue = queue;
            this._thread = new Thread(new ThreadStart(this.RunThread))
            {
                Name = threadName,
                IsBackground = isBackground,
                Priority = priority
            };
            this._scheduler = new Scheduler(this);
        }

        private static int GetNextThreadId()
        {
            return Interlocked.Increment(ref ThreadFiber.THREAD_COUNT);
        }

        private void RunThread()
        {
            this._queue.Run();
        }

        /// <summary>
        /// Enqueue a single action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            this._queue.Enqueue(action);
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

        public IDisposable Schedule(Action action, long firstInMs)
        {
            return this._scheduler.Schedule(action, firstInMs);
        }

        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return this._scheduler.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        public void Start()
        {
            this._thread.Start();
        }

        /// <summary>
        ///  Calls join on the thread.
        /// </summary>
        public void Join()
        {
            this._thread.Join();
        }

        /// <summary>
        /// Stops the thread.
        /// </summary>
        public void Dispose()
        {
            this._scheduler.Dispose();
            this._subscriptions.Dispose();
            this._queue.Stop();
        }
    }
}
