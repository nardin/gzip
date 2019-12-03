using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Gzip.Lib.Common;

namespace Gzip.Lib.Collection
{
    /// <summary>
    /// Ordered pipe. 
    /// </summary>
    public class OrderingPipe : IPipe<FileChunk>
    {
        private readonly int _boundedCapacity;
        private readonly object _lockRead = new object();
        private volatile int _countLink = 0;
        private volatile int _current = 0;
        private volatile int _watch;
        private volatile int _readwatch;

        private readonly Queue<FileChunk> _collection;

        public OrderingPipe(int boundedCapacity)
        {
            this._boundedCapacity = boundedCapacity;
            this._collection = new Queue<FileChunk>();
        }

        public void Attach()
        {
            Interlocked.Increment(ref this._countLink);
        }

        public void Deattach()
        {
            Interlocked.Decrement(ref this._countLink);
        }

        public bool IsCompleted => _countLink == 0;

        public void Add(FileChunk t)
        {
            lock (_lockRead)
            {
                var watch = new Stopwatch();
                while ((this._collection.Count >= this._boundedCapacity && this._boundedCapacity != 0 && !this.IsCompleted) || this._current != t.Number)
                {
                    watch.Start();
                    Monitor.Wait(_lockRead);
                    watch.Stop();
                }
                Interlocked.Add(ref _watch, (int)watch.ElapsedMilliseconds);
                Console.WriteLine("OWatch: " + _watch);

                Interlocked.Increment(ref this._current); 

                this._collection.Enqueue(t);
                Monitor.PulseAll(_lockRead);
            }
        }

        public FileChunk Take()
        {
            lock (_lockRead)
            {
                var watch = new Stopwatch();
                while (this._collection.Count == 0 && !this.IsCompleted)
                {
                    watch.Start();
                    Monitor.Wait(_lockRead);
                    watch.Stop();
                }

                Interlocked.Add(ref _readwatch, (int)watch.ElapsedMilliseconds);
                Console.WriteLine("OReadwatch: " + _readwatch);

                if (this._collection.Count == 0)
                {
                    return default(FileChunk);
                }

                var outValue = this._collection.Dequeue();
                Monitor.Pulse(_lockRead);
                return outValue;
            }
        }
    }
}
