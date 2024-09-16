using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class MockTelemetryCollector<TRecord> : ITelemetryCollector<TRecord>
    {
        private readonly Func<IEnumerable<TRecord>, Task<ITelemetryCollector<TRecord>.AddResult>> _recordsHandler;

        internal MockTelemetryCollector(Func<IEnumerable<TRecord>, Task<ITelemetryCollector<TRecord>.AddResult>> recordsHandler)
        {
            _recordsHandler = recordsHandler;
        }

        public Task<ITelemetryCollector<TRecord>.AddResult> AddAsync(IEnumerable<TRecord> records)
        {
            return _recordsHandler(records);
        }
    }
}
