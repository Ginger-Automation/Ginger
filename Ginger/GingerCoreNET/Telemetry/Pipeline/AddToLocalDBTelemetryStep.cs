using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class AddToLocalDBTelemetryStep<TRecord> : BufferedTelemetryStep<TRecord>
    {
        private readonly ITelemetryDB<TRecord> _localDB;
        private readonly ITelemetryStep<TRecord> _sendToCollectorStep;

        internal AddToLocalDBTelemetryStep(int bufferSize, ITelemetryDB<TRecord> localDB, 
            ITelemetryStep<TRecord> sendToCollectorStep, ILogger? logger = null) : 
            base(nameof(AddToLocalDBTelemetryStep<TRecord>), bufferSize, logger)
        {
            _localDB = localDB;
            _sendToCollectorStep = sendToCollectorStep;
        }

        protected override async Task ConsumerRecordsAsync(IEnumerable<TRecord> records)
        {
            foreach (TRecord record in records)
            {
                try
                {
                    await _localDB.AddAsync(record);
                }
                catch (Exception ex)
                {
                    _logger?.LogError("failed to add record to local DB, skipping record forwarding for next step\n{ex}", ex);
                    continue;
                }
                _sendToCollectorStep.Process(record);
            }
        }
    }
}
