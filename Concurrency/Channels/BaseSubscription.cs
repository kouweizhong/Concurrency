using MartinSu.Concurrency.Core;
using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Base implementation for subscription
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSubscription<T> : ISubscribable<T>, IProducerThreadSubscriber<T>
    {
        private Filter<T> _filterOnProducerThread;
        public Filter<T> FilterOnProducerThread
        {
            get
            {
                return this._filterOnProducerThread;
            }
            set
            {
                this._filterOnProducerThread = value;
            }
        }

        /// <summary>
        ///  Allows for the registration and deregistration of subscriptions
        /// </summary>
        public abstract ISubscriptionRegistry Subscriptions
        {
            get;
        }

        private bool PassesProducerThreadFilter(T msg)
        {
            return this._filterOnProducerThread == null || this._filterOnProducerThread(msg);
        }

        public void ReceiveOnProducerThread(T msg)
        {
            if (this.PassesProducerThreadFilter(msg))
            {
                this.OnMessageOnProducerThread(msg);
            }
        }

        /// <summary>
        /// Called after message has been filtered.
        /// </summary>
        /// <param name="msg"></param>
        protected abstract void OnMessageOnProducerThread(T msg);
    }
}
