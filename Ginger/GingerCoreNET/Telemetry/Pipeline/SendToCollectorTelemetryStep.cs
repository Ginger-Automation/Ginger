using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class SendToCollectorTelemetryStep<TRecord> : BufferedTelemetryStep<TRecord>
    {
        private readonly ITelemetryCollector<TRecord> _telemetryCollector;
        private readonly ITelemetryStep<TRecord> _deleteFromLocalDBStep;
        private readonly ITelemetryStep<TRecord> _markUnsuccessfulInLocalDBStep;

        internal SendToCollectorTelemetryStep(int bufferSize, 
            ITelemetryCollector<TRecord> telemetryCollector,
            ITelemetryStep<TRecord> deleteFromLocalDBStep, 
            ITelemetryStep<TRecord> markUnsuccessfulInLocalDBStep, 
            ILogger? logger = null) :
            base(nameof(SendToCollectorTelemetryStep<TRecord>), bufferSize, logger)
        {
            _telemetryCollector = telemetryCollector;
            _deleteFromLocalDBStep = deleteFromLocalDBStep;
            _markUnsuccessfulInLocalDBStep = markUnsuccessfulInLocalDBStep;
        }

        protected override async Task ConsumeRecordsAsync(IEnumerable<TRecord> records)
        {
            ITelemetryCollector<TRecord>.AddResult result;
            try
            {
                result = await _telemetryCollector.AddAsync(records);
            }
            catch (Exception ex)
            {
                _logger?.LogError("failed to add record to collector\n{ex}", ex);
                result = new ITelemetryCollector<TRecord>.AddResult()
                {
                    Successful = false,
                };
            }

            foreach (TRecord record in records)
            {
                if (result.Successful)
                {
                    _deleteFromLocalDBStep.Process(record);
                }
                else
                {
                    _markUnsuccessfulInLocalDBStep.Process(record);
                }
            }
        }
    }
}
