using MartinSu.Concurrency.Core;
using System;

namespace MartinSu.Concurrency.Channels
{
    internal class QueueConsumer<T> : IDisposable
    {
        private bool _flushPending;

        private readonly IExecutionContext _target;

        private readonly Action<T> _callback;

        private readonly QueueChannel<T> _channel;

        public QueueConsumer(IExecutionContext target, Action<T> callback, QueueChannel<T> channel)
        {
            this._target = target;
            this._callback = callback;
            this._channel = channel;
        }

        public void Signal()
        {
            lock (this)
            {
                if (!this._flushPending)
                {
                    this._target.Enqueue(new Action(this.ConsumeNext));
                    this._flushPending = true;
                }
            }
        }

        private void ConsumeNext()
        {
            try
            {
                T msg;
                if (this._channel.Pop(out msg))
                {
                    this._callback(msg);
                }
            }
            finally
            {
                lock (this)
                {
                    if (this._channel.Count == 0)
                    {
                        this._flushPending = false;
                    }
                    else
                    {
                        this._target.Enqueue(new Action(this.ConsumeNext));
                    }
                }
            }
        }

        public void Dispose()
        {
            this._channel.SignalEvent -= new Action(this.Signal);
        }

        internal void Subscribe()
        {
            this._channel.SignalEvent += new Action(this.Signal);
        }
    }
}
