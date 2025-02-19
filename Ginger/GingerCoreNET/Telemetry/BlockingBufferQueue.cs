#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    public sealed class BlockingBufferQueue<T> : IDisposable
    {
        private readonly LinkedList<T> _collection;
        private readonly int _bufferSize;
        private readonly SemaphoreSlim _syncBufferSizeSemaphore;
        private readonly AutoResetEvent _syncEnqueueEvent;
        private readonly AutoResetEvent _syncDequeueEvent;

        private CancellationTokenSource? _syncBufferSizeSemaphoreCTS;
        private bool _isDisposed;

        public int Count => _collection.Count;

        public BlockingBufferQueue(int bufferSize)
        {
            _collection = [];
            _bufferSize = bufferSize;
            _syncBufferSizeSemaphore = new(initialCount: 0);
            _syncEnqueueEvent = new(initialState: true);
            _syncDequeueEvent = new(initialState: true);

            _isDisposed = false;
        }

        public void Enqueue(T item)
        {
            try
            {
                ThrowIfDisposed();

                _syncEnqueueEvent.WaitOne();

                ThrowIfDisposed();

                _collection.AddLast(item);
                _syncBufferSizeSemaphore.Release();
            }
            finally
            {
                if (!_isDisposed)
                {
                    _syncEnqueueEvent.Set();
                }
            }
        }

        public IEnumerable<T> Dequeue()
        {
            try
            {
                ThrowIfDisposed();

                _syncDequeueEvent.WaitOne();

                ThrowIfDisposed();

                List<T> items = new(_bufferSize);
                for (var availableItemCount = 0; availableItemCount < _bufferSize; availableItemCount++)
                {
                    _syncBufferSizeSemaphoreCTS = new();

                    try
                    {
                        _syncBufferSizeSemaphore.Wait(_syncBufferSizeSemaphoreCTS.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

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
                if (!_isDisposed)
                {
                    _syncDequeueEvent.Set();
                }
            }
        }

        public void Flush()
        {
            _syncBufferSizeSemaphoreCTS?.Cancel();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            Flush();

            _syncBufferSizeSemaphore.Dispose();
            _syncEnqueueEvent.Dispose();
            _syncDequeueEvent.Dispose();
            _syncBufferSizeSemaphoreCTS?.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(message: "Cannot perform operation on disposedException", innerException: null);
            }
        }
    }
}
