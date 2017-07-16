using System;
using System.Collections.Generic;
using System.Threading;

namespace MartinSu.Concurrency.Channels
{
    internal class ChannelRequest<R, M> : IRequest<R, M>, IReply<M>, IDisposable
    {
        private readonly object _lock = new object();

        private readonly R _req;

        private readonly Queue<M> _resp = new Queue<M>();

        private bool _disposed;

        public R Request
        {
            get
            {
                return this._req;
            }
        }

        public ChannelRequest(R req)
        {
            this._req = req;
        }

        public bool SendReply(M response)
        {
            bool result;
            lock (this._lock)
            {
                if (this._disposed)
                {
                    result = false;
                }
                else
                {
                    this._resp.Enqueue(response);
                    Monitor.PulseAll(this._lock);
                    result = true;
                }
            }
            return result;
        }

        public bool Receive(int timeout, out M result)
        {
            lock (this._lock)
            {
                if (this._resp.Count > 0)
                {
                    result = this._resp.Dequeue();
                    bool result2 = true;
                    return result2;
                }
                if (this._disposed)
                {
                    result = default(M);
                    bool result2 = false;
                    return result2;
                }
                Monitor.Wait(this._lock, timeout);
                if (this._resp.Count > 0)
                {
                    result = this._resp.Dequeue();
                    bool result2 = true;
                    return result2;
                }
            }
            result = default(M);
            return false;
        }

        /// <summary>
        /// Stop receiving replies.
        /// </summary>
        public void Dispose()
        {
            lock (this._lock)
            {
                this._disposed = true;
                Monitor.PulseAll(this._lock);
            }
        }
    }
}
