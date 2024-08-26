using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class MarkUnsuccessfulInLocalDBTelemetryStep<TRecord> : BufferedTelemetryStep<TRecord>
    {
        private readonly ITelemetryDB<TRecord> _localDB;

        internal MarkUnsuccessfulInLocalDBTelemetryStep(int bufferSize, 
            ITelemetryDB<TRecord> localDB, 
            ILogger? logger = null) :
            base(nameof(MarkUnsuccessfulInLocalDBTelemetryStep<TRecord>), bufferSize, logger)
        {
            _localDB = localDB;
        }

        internal override async Task ProcessRecordsAsync(IEnumerable<TRecord> records)
        {
            foreach (TRecord record in records)
            {
                try
                {
                    bool wasFound = await _localDB.MarkFailedToUpload(record);
                    if (!wasFound)
                    {
                        _logger?.LogError("no record found in local DB");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError("failed to mark record as 'FailedToUpload' in local DB\n{ex}", ex);
                }
            }
        }
    }
}
