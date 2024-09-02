using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class TelemetryRetryService<TRecord> : ITelemetryRetryService<TRecord>
    {
        internal sealed class Config<TTRecord>
        {
            internal required ITelemetryDB<TTRecord> TelemetryDB { get; init; }
            internal required ITelemetryStep<TTRecord> SendToCollectorStep { get; init; }
            internal required int PollingSize { get; init; }
            internal required TimeSpan PollingInterval { get; init; }
            internal ILogger? Logger { get; init; }
        }

        private readonly ITelemetryDB<TRecord> _telemetryDB;
        private readonly ITelemetryStep<TRecord> _sendToCollectorStep;
        private readonly TimeSpan _pollingInterval;
        private readonly int _pollingSize;
        private readonly ILogger? _logger;
        private readonly CancellationTokenSource _monitoringCancellationToken;
        private readonly Task _monitoringTask;

        internal TelemetryRetryService(Config<TRecord> config)
        {
            if (config.TelemetryDB == null)
            {
                throw new ArgumentNullException(paramName: nameof(config.TelemetryDB));
            }
            _telemetryDB = config.TelemetryDB;

            if (config.SendToCollectorStep == null)
            {
                throw new ArgumentNullException(paramName: nameof(config.SendToCollectorStep));
            }
            _sendToCollectorStep = config.SendToCollectorStep;

            _pollingInterval = config.PollingInterval;
            _pollingSize = config.PollingSize;
            _logger = config.Logger;

            _monitoringCancellationToken = new();
            _monitoringTask = CreateMonitoringTask();
        }

        private Task CreateMonitoringTask()
        {
            return new Task(async () =>
            {
                _logger?.LogDebug("telemetry monitoring task started");

                while (!_monitoringCancellationToken.IsCancellationRequested)
                {
                    await ProcessRecordsAsync(_pollingSize);
                    await Task.Delay(_pollingInterval);
                }

                _logger?.LogDebug("telemetry monitoring task stopped");

            });
        }

        public void Dispose()
        {
            StopMonitoring();
        }

        public void StopMonitoring()
        {
            _logger?.LogDebug("cancelling telemetry monitoring task");

            if (_monitoringCancellationToken.IsCancellationRequested)
            {
                _logger?.LogDebug("telemetry monitoring task cancellation already requested");
                return;
            }

            _monitoringCancellationToken.Cancel();
        }

        public void StartMonitoring()
        {
            _logger?.LogDebug("starting telemetry monitoring task");

            _monitoringTask.Start();
        }

        internal async Task ProcessRecordsAsync(int pollingSize)
        {
            IEnumerable<TRecord> records;
            try
            {
                records = await _telemetryDB.GetFailedToUploadRecords(pollingSize);
            }
            catch (Exception ex)
            {
                records = Array.Empty<TRecord>();
                _logger?.LogError("unable to get FailedToUpload records\n{ex}", ex);
            }

            foreach (TRecord record in records)
            {
                try
                {
                    _sendToCollectorStep.Process(record);
                }
                catch (Exception ex)
                {
                    _logger?.LogError("unable to send records to SendToCollector step\n{ex}", ex);
                }
            }
        }
    }
}
