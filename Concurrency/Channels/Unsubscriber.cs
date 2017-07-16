using MartinSu.Concurrency.Core;
using System;

namespace MartinSu.Concurrency.Channels
{
    internal class Unsubscriber<T> : IDisposable
    {
        private readonly Action<T> _receiver;

        private readonly Channel<T> _channel;

        private readonly ISubscriptionRegistry _subscriptions;

        public Unsubscriber(Action<T> receiver, Channel<T> channel, ISubscriptionRegistry subscriptions)
        {
            this._receiver = receiver;
            this._channel = channel;
            this._subscriptions = subscriptions;
        }

        public void Dispose()
        {
            this._channel.Unsubscribe(this._receiver);
            this._subscriptions.DeregisterSubscription(this);
        }
    }
}
