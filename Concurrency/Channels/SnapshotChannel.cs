using MartinSu.Concurrency.Fibers;
using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    ///  A SnapshotChannel is a channel that allows for the transmission of an initial snapshot followed by incremental updates.
    ///  The class is thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SnapshotChannel<T> : ISnapshotChannel<T>, IPublisher<T>
    {
        private readonly int _timeoutInMs;

        private readonly IChannel<T> _updatesChannel = new Channel<T>();

        private readonly RequestReplyChannel<object, T> _requestChannel = new RequestReplyChannel<object, T>();

        /// <summary>
        /// </summary>
        /// <param name="timeoutInMs">For initial snapshot</param>
        public SnapshotChannel(int timeoutInMs)
        {
            this._timeoutInMs = timeoutInMs;
        }

        /// <summary>
        ///  Subscribes for an initial snapshot and then incremental update.
        /// </summary>
        /// <param name="fiber">the target executor to receive the message</param>
        /// <param name="receive"></param>
        public void PrimedSubscribe(IFiber fiber, Action<T> receive)
        {
            using (IReply<T> reply = this._requestChannel.SendRequest(new object()))
            {
                if (reply == null)
                {
                    throw new ArgumentException(typeof(T).Name + " synchronous request has no reply subscriber.");
                }
                T result;
                if (!reply.Receive(this._timeoutInMs, out result))
                {
                    throw new ArgumentException(typeof(T).Name + " synchronous request timed out in " + this._timeoutInMs);
                }
                receive(result);
                this._updatesChannel.Subscribe(fiber, receive);
            }
        }

        /// <summary>
        ///  Publishes the incremental update.
        /// </summary>
        /// <param name="update"></param>
        public bool Publish(T update)
        {
            return this._updatesChannel.Publish(update);
        }

        /// <summary>
        ///  Ressponds to the request for an initial snapshot.
        /// </summary>
        /// <param name="fiber">the target executor to receive the message</param>
        /// <param name="reply">returns the snapshot update</param>
        public void ReplyToPrimingRequest(IFiber fiber, Func<T> reply)
        {
            this._requestChannel.Subscribe(fiber, delegate (IRequest<object, T> request)
            {
                request.SendReply(reply());
            });
        }
    }
}
