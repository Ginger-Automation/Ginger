using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal interface ITelemetryCollector<TRecord>
    {
        internal class AddResult
        {
            public required bool Successful { get; init; }
        }

        public Task<AddResult> AddAsync(IEnumerable<TRecord> records);
    }
}
