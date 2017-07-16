using MartinSu.Concurrency.Core;
using MartinSu.Concurrency.Fibers;
using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    ///  Default Channel Implementation. Methods are thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Channel<T> : IChannel<T>, ISubscriber<T>, IPublisher<T>
    {
        private readonly object subscriberLock = new object();

        private event Action<T> _subscribers;

        public bool HasSubscriptions
        {
            get
            {
                return this._subscribers != null;
            }
        }

        /// <summary>
        ///  Number of subscribers
        /// </summary>
        public int NumSubscribers
        {
            get
            {
                if (this._subscribers != null)
                {
                    return this._subscribers.GetInvocationList().Length;
                }
                return 0;
            }
        }

        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return this.SubscribeOnProducerThreads(new ChannelSubscription<T>(fiber, receive));
        }

        public IDisposable SubscribeToBatch(IFiber fiber, Action<IList<T>> receive, int intervalInMs)
        {
            return this.SubscribeOnProducerThreads(new BatchSubscriber<T>(fiber, receive, intervalInMs));
        }

        public IDisposable SubscribeToKeyedBatch<K>(IFiber fiber, Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs)
        {
            return this.SubscribeOnProducerThreads(new KeyedBatchSubscriber<K, T>(keyResolver, receive, fiber, intervalInMs));
        }

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread.  If a newer message arrives before the consuming thread
        /// has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToLast(IFiber fiber, Action<T> receive, int intervalInMs)
        {
            return this.SubscribeOnProducerThreads(new LastSubscriber<T>(receive, fiber, intervalInMs));
        }

        /// <summary>
        /// Subscribes to actions on producer threads. Subscriber could be called from multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public IDisposable SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber)
        {
            return this.SubscribeOnProducerThreads(new Action<T>(subscriber.ReceiveOnProducerThread), subscriber.Subscriptions);
        }

        /// <summary>
        /// Subscribes an action to be executed for every action posted to the channel. Action should be thread safe. 
        /// Action may be invoked on multiple threads.
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        private IDisposable SubscribeOnProducerThreads(Action<T> subscriber, ISubscriptionRegistry subscriptions)
        {
            lock (this.subscriberLock)
            {
                this._subscribers += subscriber;
            }
            Unsubscriber<T> unsubscriber = new Unsubscriber<T>(subscriber, this, subscriptions);
            subscriptions.RegisterSubscription(unsubscriber);
            return unsubscriber;
        }

        internal void Unsubscribe(Action<T> toUnsubscribe)
        {
            lock (this.subscriberLock)
            {
                this._subscribers -= toUnsubscribe;
            }
        }

        public bool Publish(T msg)
        {
            Action<T> evnt = this._subscribers;
            if (evnt != null)
            {
                evnt(msg);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove all subscribers.
        /// </summary>
        public void ClearSubscribers()
        {
            this._subscribers = null;
        }
    }
}
