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
    internal sealed class TelemetryRetryService<TRecord> : IDisposable
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
            _monitoringTask = new(async () => await MonitoringTaskAction(), _monitoringCancellationToken.Token);
        }

        public void Dispose()
        {
            _logger?.LogDebug("cancelling telemetry monitoring task");

            _monitoringCancellationToken.Cancel();
        }

        internal void StartMonitoring()
        {
            _logger?.LogDebug("starting telemetry monitoring task");

            _monitoringTask.Start();
        }

        private async Task MonitoringTaskAction()
        {
            _logger?.LogDebug("telemetry monitoring task started");

            while (!_monitoringCancellationToken.IsCancellationRequested)
            {
                try
                {
                    var records = await _telemetryDB.GetFailedToUploadRecords(_pollingSize);
                    foreach (var record in records)
                    {
                        _sendToCollectorStep.Process(record);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError("error while retrying to send records \n{ex}", ex);
                }

                await Task.Delay(_pollingInterval);
            }

            _logger?.LogDebug("telemetry monitoring task stopped");
        }
    }
}
