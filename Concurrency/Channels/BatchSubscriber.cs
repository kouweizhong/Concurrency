using MartinSu.Concurrency.Core;
using MartinSu.Concurrency.Fibers;
using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Batches actions for the consuming thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BatchSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();

        private readonly IFiber _fiber;

        private readonly Action<IList<T>> _receive;

        private readonly int _interval;

        private List<T> _pending;

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
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        public BatchSubscriber(IFiber fiber, Action<IList<T>> receive, int interval)
        {
            this._fiber = fiber;
            this._receive = receive;
            this._interval = interval;
        }

        /// <summary>
        /// Receives message and batches as needed.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (this._batchLock)
            {
                if (this._pending == null)
                {
                    this._pending = new List<T>();
                    this._fiber.Schedule(new Action(this.Flush), (long)this._interval);
                }
                this._pending.Add(msg);
            }
        }

        private void Flush()
        {
            IList<T> toFlush = null;
            lock (this._batchLock)
            {
                if (this._pending != null)
                {
                    toFlush = this._pending;
                    this._pending = null;
                }
            }
            if (toFlush != null)
            {
                this._receive(toFlush);
            }
        }
    }
}
