using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal abstract class BufferedTelemetryStep<TRecord> : ITelemetryStep<TRecord>, IDisposable
    {
        private readonly string _name;
        private readonly BlockingBufferQueue<TRecord> _queue;
        private readonly Task _consumerTask;
        private readonly CancellationTokenSource _consumerCancellationTokenSource;
        protected readonly ILogger? _logger;

        internal string Name => _name;
        internal int RecordsInBuffer => _queue.Count;

        internal BufferedTelemetryStep(string name, int bufferSize, ILogger? logger = null)
        {
            _name = name;
            _queue = new(bufferSize);
            _consumerCancellationTokenSource = new();
            _consumerTask = CreateConsumerTask();
            _logger = logger;
        }

        private Task CreateConsumerTask()
        {
            return new Task(async () =>
            {
                _logger?.LogDebug("consumer task started for {name}", _name);

                while (!_consumerCancellationTokenSource.IsCancellationRequested)
                {
                    var records = _queue.Dequeue();
                    try
                    {
                        await Process(records);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError("error while processing records for {name}\n{ex}", _name, ex);
                    }
                }

                _logger?.LogDebug("consumer task stopped for {name}", _name);
            });
        }

        protected internal abstract Task Process(IEnumerable<TRecord> records);

        internal void StartConsumer()
        {
            _logger?.LogDebug("starting consumer task for {name}", _name);

            _consumerTask.Start();
        }

        public void Dispose()
        {
            _logger?.LogDebug("cancelling consumer task for {name}", _name);

            _consumerCancellationTokenSource.Cancel();
        }

        public void Process(TRecord record)
        {
            _queue.Enqueue(record);
        }
    }
}
