using System;
using System.Collections.Generic;

namespace MartinSu.Concurrency.Core
{
    /// <summary>
    /// Registry for subscriptions. Provides thread safe methods for list of subscriptions.
    /// </summary>
    internal class Subscriptions : IDisposable
    {
        private readonly object _lock = new object();

        private readonly List<IDisposable> _items = new List<IDisposable>();

        /// <summary>
        /// Number of registered disposables.
        /// </summary>
        public int Count
        {
            get
            {
                int count;
                lock (this._lock)
                {
                    count = this._items.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// Add Disposable
        /// </summary>
        /// <param name="toAdd"></param>
        public void Add(IDisposable toAdd)
        {
            lock (this._lock)
            {
                this._items.Add(toAdd);
            }
        }

        /// <summary>
        /// Remove Disposable.
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        public bool Remove(IDisposable toRemove)
        {
            bool result;
            lock (this._lock)
            {
                result = this._items.Remove(toRemove);
            }
            return result;
        }

        /// <summary>
        /// Disposes all disposables registered in list.
        /// </summary>
        public void Dispose()
        {
            lock (this._lock)
            {
                IDisposable[] array = this._items.ToArray();
                for (int i = 0; i < array.Length; i++)
                {
                    IDisposable victim = array[i];
                    victim.Dispose();
                }
                this._items.Clear();
            }
        }
    }
}
