using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    public sealed class BlockingBufferQueue<T>
    {
        private readonly LinkedList<T> _collection;
        private readonly int _bufferSize;
        private readonly SemaphoreSlim _syncBufferSizeSemaphore;
        private readonly AutoResetEvent _syncEnqueueEvent;
        private readonly AutoResetEvent _syncDequeueEvent;

        public int Count => _collection.Count;

        public BlockingBufferQueue(int bufferSize)
        {
            _collection = [];
            _bufferSize = bufferSize;
            _syncBufferSizeSemaphore = new(initialCount: 0);
            _syncEnqueueEvent = new(initialState: true);
            _syncDequeueEvent = new(initialState: true);
        }

        public void Enqueue(T item)
        {
            try
            {
                _syncEnqueueEvent.WaitOne();

                _collection.AddLast(item);
                _syncBufferSizeSemaphore.Release();
            }
            finally
            {
                _syncEnqueueEvent.Set();
            }
        }

        public IEnumerable<T> Dequeue()
        {
            try
            {
                _syncDequeueEvent.WaitOne();

                List<T> items = new(_bufferSize);
                for (var availableItemCount = 0; availableItemCount < _bufferSize; availableItemCount++)
                {
                    _syncBufferSizeSemaphore.Wait();

                    if (_collection.Count > 0)
                    {
                        var item = _collection.First();
                        _collection.RemoveFirst();
                        items.Add(item);
                    }
                }

                return items;
            }
            finally
            {
                _syncDequeueEvent.Set();
            }
        }
    }
}
