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
        private readonly TelemetryRetryService<TRecord> _retryService;

        internal sealed class Options<TTRecord>
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

        internal TelemetryPipeline(Options<TRecord> options)
        {
            _deleteFromLocalDBTelemetryStep = new(options.DeleteFromLocalDBTelemetryStepBufferSize, options.TelemetryDB, options.Logger);
            _markUnsuccessfulInLocalDBTelemetryStep = new(options.MarkUnsuccessfulInLocalDBTelemetryStepBufferSize, options.TelemetryDB, options.Logger);
            _sendToCollectorTelemetryStep = new(options.SendToCollectorTelemetryStepBufferSize, options.Collector, _deleteFromLocalDBTelemetryStep, _markUnsuccessfulInLocalDBTelemetryStep, options.Logger);
            _addToLocalDBTelemetryStep = new(options.AddToLocalDBTelemetryStepBufferSize, options.TelemetryDB, _sendToCollectorTelemetryStep, options.Logger);
            _retryService = new(options.TelemetryDB, _sendToCollectorTelemetryStep, options.Logger);

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
