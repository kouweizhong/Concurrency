using MartinSu.Concurrency.Core;
using MartinSu.Concurrency.Fibers;
using System;

namespace MartinSu.Concurrency.Channels
{
    /// <summary>
    /// Subscription for actions on a channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChannelSubscription<T> : BaseSubscription<T>
    {
        private readonly Action<T> _receiver;

        private readonly IFiber _fiber;

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
        /// Construct the subscription
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receiver"></param>
        public ChannelSubscription(IFiber fiber, Action<T> receiver)
        {
            this._fiber = fiber;
            this._receiver = receiver;
        }

        /// <summary>
        /// Receives the action and queues the execution on the target fiber.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            this._fiber.Enqueue(delegate
            {
                this._receiver(msg);
            });
        }
    }
}
