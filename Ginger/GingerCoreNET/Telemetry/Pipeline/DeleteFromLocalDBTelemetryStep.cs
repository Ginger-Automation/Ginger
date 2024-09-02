using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal sealed class DeleteFromLocalDBTelemetryStep<TRecord> : BufferedTelemetryStep<TRecord>
    {
        private readonly ITelemetryDB<TRecord> _localDB;

        internal DeleteFromLocalDBTelemetryStep(int bufferSize, 
            ITelemetryDB<TRecord> localDB, 
            ILogger? logger = null) :
            base(nameof(DeleteFromLocalDBTelemetryStep<TRecord>), bufferSize, logger)
        {
            _localDB = localDB;
        }

        protected internal override async Task Process(IEnumerable<TRecord> records)
        {
            foreach (TRecord record in records)
            {
                try
                {
                    bool wasFound = await _localDB.DeleteAsync(record);
                    if (!wasFound)
                    {
                        _logger?.LogError("no record found in local DB");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError("failed to delete record from local DB\n{ex}", ex);
                }
            }
        }
    }
}
