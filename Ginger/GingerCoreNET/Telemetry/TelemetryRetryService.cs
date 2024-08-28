using Amdocs.Ginger.CoreNET.Telemetry.Pipeline;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryRetryService<TRecord> : IDisposable
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(30);
        private static readonly int PollingSize = 4;

        private readonly ITelemetryDB<TRecord> _telemetryDB;
        private readonly SendToCollectorTelemetryStep<TRecord> _sendToCollectorTelemetryStep;
        private readonly ILogger? _logger;
        private readonly CancellationTokenSource _monitoringCancellationToken;
        private readonly Task _monitoringTask;

        internal TelemetryRetryService(ITelemetryDB<TRecord> telemetryDB, SendToCollectorTelemetryStep<TRecord> sendToCollectorTelemetryStep, ILogger? logger = null)
        {
            _telemetryDB = telemetryDB;
            _sendToCollectorTelemetryStep = sendToCollectorTelemetryStep;
            _logger = logger;
            _monitoringCancellationToken = new();
            _monitoringTask = new(async () => await MonitorActionAsync(), _monitoringCancellationToken.Token);
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

        private async Task MonitorActionAsync()
        {
            _logger?.LogDebug("telemetry monitoring task started");

            while (!_monitoringCancellationToken.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<TRecord> records = await _telemetryDB.GetFailedToUploadRecords(PollingSize);
                    foreach(TRecord record in records)
                    {
                        _sendToCollectorTelemetryStep.Process(record);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError("error while retrying to send records \n{ex}", ex);
                }

                await Task.Delay(PollingInterval);
            }

            _logger?.LogDebug("telemetry monitoring task stopped");
        }
    }
}
