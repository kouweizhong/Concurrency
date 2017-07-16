using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Callback method and parameters for a channel subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISubscribable<T> : IProducerThreadSubscriber<T>
    {
        /// <summary>
        /// Filter called from producer threads. Should be thread safe as it may be called from
        /// multiple threads.
        /// </summary>
        Filter<T> FilterOnProducerThread
        {
            get;
            set;
        }
    }
}
