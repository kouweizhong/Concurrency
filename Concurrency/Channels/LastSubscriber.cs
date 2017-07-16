using MartinSu.Concurrency.Core;
using MartinSu.Concurrency.Fibers;
using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Subscribes to last action received on the channel. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly IFiber _fiber;

        private readonly Action<T> _target;

        private readonly int _flushIntervalInMs;

        private bool _flushPending;

        private T _pending;

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
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="flushIntervalInMs"></param>
        public LastSubscriber(Action<T> target, IFiber fiber, int flushIntervalInMs)
        {
            this._fiber = fiber;
            this._target = target;
            this._flushIntervalInMs = flushIntervalInMs;
        }

        /// <summary>
        /// Receives message from producer thread.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (this._batchLock)
            {
                if (!this._flushPending)
                {
                    this._fiber.Schedule(new Action(this.Flush), (long)this._flushIntervalInMs);
                    this._flushPending = true;
                }
                this._pending = msg;
            }
        }

        /// <summary>
        /// Flushes on IFiber thread.
        /// </summary>
        private void Flush()
        {
            T toReturn = this.ClearPending();
            this._target(toReturn);
        }

        private T ClearPending()
        {
            T pending;
            lock (this._batchLock)
            {
                this._flushPending = false;
                pending = this._pending;
            }
            return pending;
        }
    }
}
