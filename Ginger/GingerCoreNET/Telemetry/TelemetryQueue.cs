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

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryQueue<TRecord> : IDisposable
    {
        private static readonly TimeSpan QueueFlushWaitTime = TimeSpan.FromSeconds(1);

        internal sealed class Config
        {
            internal required int BufferSize { get; init; }
            internal required ITelemetryDB<TRecord> DB { get; init; }
            internal required ITelemetryCollector<TRecord> Collector { get; init; }
            internal required int RetryPollingSize { get; init; }
            internal required TimeSpan RetryPollingInterval { get; init; }
            internal ILogger? Logger { get; set; }
        }

        private readonly BlockingBufferQueue<TRecord> _queue;
        private readonly ITelemetryDB<TRecord> _db;
        private readonly ITelemetryCollector<TRecord> _collector;
        private readonly int _retryPollingSize;
        private readonly TimeSpan _retryPollingInterval;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _consumerTask;
        private readonly Task _retryTask;
        private readonly HashSet<TRecord> _recordsBeingProcessed;
        private readonly ILogger? _logger;

        private bool _isDisposed;

        private string QueueName { get; } = $"{typeof(TRecord).Name}-Telemetry-Queue";

        internal TelemetryQueue(Config config)
        {
            _queue = new(config.BufferSize);
            _db = config.DB;
            _collector = config.Collector;
            _retryPollingSize = config.RetryPollingSize;
            _retryPollingInterval = config.RetryPollingInterval;
            _cancellationTokenSource = new();
            _consumerTask = StartConsumerTask();
            _retryTask = StartRetryService();
            _recordsBeingProcessed = [];
            _logger = config.Logger;

            _isDisposed = false;
        }

        private Task StartConsumerTask()
        {
            return Task.Run(async () =>
            {
                _logger?.LogDebug("started consumer task in {queueName}", QueueName);

                while (!_isDisposed)
                {
                    string corrId = NewCorrelationId();

                    IEnumerable<TRecord> records = Dequeue(corrId);

                    try
                    {
                        await ProcessAsync(records, corrId);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError("error while processing records in {queueName}\n{ex}", QueueName, ex);
                    }
                }

                _logger?.LogDebug("stopped consumer task in {queueName}", QueueName);
            });
        }

        private Task StartRetryService()
        {
            return Task.Run(async () =>
            {
                _logger?.LogDebug("started retry task in in {queueName}", QueueName);

                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    string corrId = NewCorrelationId();

                    await Task.Delay(_retryPollingInterval, _cancellationTokenSource.Token);

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    IEnumerable<TRecord> records = await TryGetRecordsForRetryFromDB(corrId);

                    try
                    {
                        await ReprocessAsync(records, corrId);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError("{corrId} error while reprocessing records in {queueName}\n{ex}", corrId, QueueName, ex);
                    }
                }

                _logger?.LogDebug("stopped retry task in {queueName}", QueueName);
            });
        }

        private string NewCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }

        public void Enqueue(TRecord record)
        {
            _logger?.LogTrace("enqueing record in {queueName}", QueueName);
            try
            {
                _queue.Enqueue(record);
            }
            catch (Exception ex)
            {
                _logger?.LogError("error while enqueing records in {queueName}\n{ex}", QueueName, ex);
            }
        }

        private IEnumerable<TRecord> Dequeue(string corrId = "")
        {
            _logger?.LogTrace("{corrId} dequeing records from {queueName}", corrId, QueueName);
            try
            {
                return _queue.Dequeue();
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while dequeing records in {queueName}\n{ex}", corrId, QueueName, ex);
                return [];
            }
        }

        internal async Task ProcessAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            if (!records.Any())
            {
                _logger?.LogTrace("{corrId} no records to process in {queueName}", corrId, QueueName);
                return;
            }

            _logger?.LogTrace("{corrId} processing records in {queueName}", corrId, QueueName);

            try
            {
                _recordsBeingProcessed.AddRange(records);

                await TryAddToDBAsync(records, corrId);

                ITelemetryCollector<TRecord>.AddResult? result = await TrySendToCollectorAsync(records, corrId);

                if (result != null && result.Successful)
                {
                    await TryDeleteRecordsFromDBAsync(records, corrId);
                }
                else
                {
                    await TryIncrementUploadAttemptCountAsync(records, corrId);
                }
            }
            finally
            {
                _recordsBeingProcessed.Clear();
            }
        }

        internal async Task ReprocessAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            if (!records.Any())
            {
                _logger?.LogTrace("{corrId} no records to reprocess in {queueName}", corrId, QueueName);
                return;
            }

            _logger?.LogTrace("{corrId} reprocessing records in {queueName}", corrId, QueueName);

            ITelemetryCollector<TRecord>.AddResult? result = await TrySendToCollectorAsync(records, corrId);
            if (result != null && result.Successful)
            {
                await TryDeleteRecordsFromDBAsync(records, corrId);
            }
            else
            {
                await TryIncrementUploadAttemptCountAsync(records, corrId);
            }
        }

        private async Task<bool> TryAddToDBAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            _logger?.LogTrace("{corrId} adding records to db in {queueName}", corrId, QueueName);
            try
            {
                await _db.AddAsync(records);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while adding records to db in {queueName}\n{ex}", corrId, QueueName, ex);
                return false;
            }
        }

        private async Task<ITelemetryCollector<TRecord>.AddResult?> TrySendToCollectorAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            _logger?.LogTrace("{corrId} sending records to collector in {queueName}", corrId, QueueName);
            try
            {
                return await _collector.AddAsync(records);
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while sending records to collector in {queueName}\n{ex}", corrId, QueueName, ex);
                return null;
            }
        }

        private async Task<bool> TryDeleteRecordsFromDBAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            _logger?.LogTrace("{corrId} deleting records from db in {queueName}", corrId, QueueName);
            try
            {
                await _db.DeleteAsync(records);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while deleting records from db in {queueName}\n{ex}", corrId, QueueName, ex);
                return false;
            }
        }

        private async Task<bool> TryIncrementUploadAttemptCountAsync(IEnumerable<TRecord> records, string corrId = "")
        {
            _logger?.LogTrace("{corrId} incrementing 'UploadAttemptCount' for records in db in {queueName}", corrId, QueueName);
            try
            {
                await _db.IncrementUploadAttemptCountAsync(records);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while incrementing 'UploadAttemptCount' for records in db in {queueName}\n{ex}", corrId, QueueName, ex);
                return false;
            }
        }

        private async Task<IEnumerable<TRecord>> TryGetRecordsForRetryFromDB(string corrId = "")
        {
            _logger?.LogTrace("{corrId} getting records for retry from db in {queueName}", corrId, QueueName);
            try
            {
                return await _db.GetRecordsForRetryAsync(exclude: _recordsBeingProcessed, limit: _retryPollingSize);
            }
            catch (Exception ex)
            {
                _logger?.LogError("{corrId} error while getting records for retry from db in {queueName}\n{ex}", corrId, QueueName, ex);
                return [];
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _logger?.LogDebug("disposing {queueName}", QueueName);

            _isDisposed = true;
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _queue.Dispose();

            _consumerTask.Wait(QueueFlushWaitTime);
        }
    }
}
