using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Gzip.Lib.Collection
{
    /// <summary>
    /// Blocking pipe.
    /// Thread blocking when try read from empty pipe or write to full pipe.
    /// </summary>
    /// <typeparam name="T">Message's type</typeparam>
    internal class BlockingPipe<T> : IPipe<T>
    {
        /// <summary>
        /// Bounded capacity.
        /// </summary>
        private readonly int _boundedCapacity;
        /// <summary>
        /// Object for lock
        /// </summary>
        private readonly object _lockRead = new object();
        /// <summary>
        /// Count of producer.
        /// </summary>
        private volatile int _countLink = 0;

        /// <summary>
        /// Internal collection.
        /// </summary>
        private readonly Queue<T> _collection;

        public BlockingPipe(int boundedCapacity)
        {
            this._boundedCapacity = boundedCapacity;
            this._collection = new Queue<T>();
        }

        public void Attach()
        {
            lock (_lockRead)
            {
                Interlocked.Increment(ref this._countLink);
                Monitor.PulseAll(_lockRead);
            }
        }

        public void Deattach()
        {
            lock (_lockRead)
            {
                Interlocked.Decrement(ref this._countLink);
                Monitor.PulseAll(_lockRead);
            }
        }

        public bool IsCompleted => _countLink == 0;

        public void Add(T t)
        {
            lock (_lockRead)
            {
                while (this._collection.Count >= this._boundedCapacity && this._boundedCapacity != 0 && !this.IsCompleted)
                {
                    Monitor.Wait(_lockRead);
                }

                this._collection.Enqueue(t);
                Monitor.PulseAll(_lockRead);
            }
        }

        public T Take()
        {
            lock (_lockRead)
            {
                while (this._collection.Count == 0 && !this.IsCompleted)
                {
                    Monitor.Wait(_lockRead);
                }

                if (this._collection.Count == 0)
                {
                    return default(T);
                }

                var outValue = this._collection.Dequeue();
                Monitor.PulseAll(_lockRead);
                return outValue;
            }
        }
    }
}
