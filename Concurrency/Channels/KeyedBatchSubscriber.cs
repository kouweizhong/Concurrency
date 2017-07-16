using MartinSu.Concurrency.Core;
using MartinSu.Concurrency.Fibers;
using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Channel subscription that drops duplicates based upon a key.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class KeyedBatchSubscriber<K, T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly IFiber _fiber;

        private readonly Action<IDictionary<K, T>> _target;

        private readonly int _flushIntervalInMs;

        private readonly Converter<T, K> _keyResolver;

        private Dictionary<K, T> _pending;

        /// <summary>
        ///  Allows for the registration and deregistration of subscriptions
        /// </summary>
        public override ISubscriptionRegistry Subscriptions
        {
            get
            {
                return this._fiber;
            }
        }

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="flushIntervalInMs"></param>
        public KeyedBatchSubscriber(Converter<T, K> keyResolver, Action<IDictionary<K, T>> target, IFiber fiber, int flushIntervalInMs)
        {
            this._keyResolver = keyResolver;
            this._fiber = fiber;
            this._target = target;
            this._flushIntervalInMs = flushIntervalInMs;
        }

        /// <summary>
        /// received on delivery thread
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (this._batchLock)
            {
                K key = this._keyResolver(msg);
                if (this._pending == null)
                {
                    this._pending = new Dictionary<K, T>();
                    this._fiber.Schedule(new Action(this.Flush), (long)this._flushIntervalInMs);
                }
                this._pending[key] = msg;
            }
        }

        /// <summary>
        /// Flushed from fiber
        /// </summary>
        public void Flush()
        {
            IDictionary<K, T> toReturn = this.ClearPending();
            if (toReturn != null)
            {
                this._target(toReturn);
            }
        }

        private IDictionary<K, T> ClearPending()
        {
            IDictionary<K, T> result;
            lock (this._batchLock)
            {
                if (this._pending == null || this._pending.Count == 0)
                {
                    this._pending = null;
                    result = null;
                }
                else
                {
                    IDictionary<K, T> toReturn = this._pending;
                    this._pending = null;
                    result = toReturn;
                }
            }
            return result;
        }
    }
}
