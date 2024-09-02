using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class TelemetryPipeline<TRecord> : IDisposable
    {
        private readonly AddToLocalDBTelemetryStep<TRecord> _addToLocalDBTelemetryStep;
        private readonly SendToCollectorTelemetryStep<TRecord> _sendToCollectorTelemetryStep;
        private readonly MarkUnsuccessfulInLocalDBTelemetryStep<TRecord> _markUnsuccessfulInLocalDBTelemetryStep;
        private readonly DeleteFromLocalDBTelemetryStep<TRecord> _deleteFromLocalDBTelemetryStep;
        private readonly ITelemetryRetryService<TRecord> _retryService;

        internal sealed class Config<TTRecord>
        {
            internal required ITelemetryDB<TTRecord> TelemetryDB { get; init; }
            internal required ITelemetryCollector<TTRecord> Collector { get; init; }
            internal ILogger? Logger { get; init; }
            internal int AddToLocalDBTelemetryStepBufferSize { get; init; } = 4;
            internal int SendToCollectorTelemetryStepBufferSize { get; init; } = 4;
            internal int MarkUnsuccessfulInLocalDBTelemetryStepBufferSize { get; init; } = 4;
            internal int DeleteFromLocalDBTelemetryStepBufferSize { get; init; } = 4;
            internal TimeSpan? RetryServicePollingInterval { get; init; }
            internal int RetryServicePollingSize { get; init; } = 10;
        }

        internal TelemetryPipeline(Config<TRecord> config)
        {
            _deleteFromLocalDBTelemetryStep = new(config.DeleteFromLocalDBTelemetryStepBufferSize, config.TelemetryDB, config.Logger);
            _markUnsuccessfulInLocalDBTelemetryStep = new(config.MarkUnsuccessfulInLocalDBTelemetryStepBufferSize, config.TelemetryDB, config.Logger);
            _sendToCollectorTelemetryStep = new(config.SendToCollectorTelemetryStepBufferSize, config.Collector, _deleteFromLocalDBTelemetryStep, _markUnsuccessfulInLocalDBTelemetryStep, config.Logger);
            _addToLocalDBTelemetryStep = new(config.AddToLocalDBTelemetryStepBufferSize, config.TelemetryDB, _sendToCollectorTelemetryStep, config.Logger);
            _retryService = new TelemetryRetryService<TRecord>(new TelemetryRetryService<TRecord>.Config<TRecord>()
            {
                PollingInterval = TimeSpan.FromSeconds(30),
                PollingSize = 10,
                SendToCollectorStep = _sendToCollectorTelemetryStep,
                TelemetryDB = config.TelemetryDB,
                Logger = config.Logger,
            });

            _deleteFromLocalDBTelemetryStep.StartConsumer();
            _markUnsuccessfulInLocalDBTelemetryStep.StartConsumer();
            _sendToCollectorTelemetryStep.StartConsumer();
            _addToLocalDBTelemetryStep.StartConsumer();
            _retryService.StartMonitoring();
        }

        public void Dispose()
        {
            _deleteFromLocalDBTelemetryStep.Dispose();
            _markUnsuccessfulInLocalDBTelemetryStep.Dispose();
            _sendToCollectorTelemetryStep.Dispose();
            _addToLocalDBTelemetryStep.Dispose();
            _retryService.Dispose();
        }

        internal void Process(TRecord record)
        {
            _addToLocalDBTelemetryStep.Process(record);
        }
    }
}
