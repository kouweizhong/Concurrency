using MartinSu.Concurrency.Core;
using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Default QueueChannel implementation. Once and only once delivery to first available consumer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueueChannel<T> : IQueueChannel<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();

        internal event Action SignalEvent;

        internal int Count
        {
            get
            {
                int count;
                lock (this._queue)
                {
                    count = this._queue.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Subscribe to executor messages. 
        /// </summary>
        /// <param name="executionContext"></param>
        /// <param name="onMessage"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IExecutionContext executionContext, Action<T> onMessage)
        {
            QueueConsumer<T> consumer = new QueueConsumer<T>(executionContext, onMessage, this);
            consumer.Subscribe();
            return consumer;
        }

        internal bool Pop(out T msg)
        {
            lock (this._queue)
            {
                if (this._queue.Count > 0)
                {
                    msg = this._queue.Dequeue();
                    return true;
                }
            }
            msg = default(T);
            return false;
        }

        /// <summary>
        /// Publish message onto queue. Notify consumers of message.
        /// </summary>
        /// <param name="message"></param>
        public void Publish(T message)
        {
            lock (this._queue)
            {
                this._queue.Enqueue(message);
            }
            Action onSignal = this.SignalEvent;
            if (onSignal != null)
            {
                onSignal();
            }
        }
    }
}
