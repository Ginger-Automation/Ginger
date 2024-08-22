using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal interface ITelemetryDB<TRecord>
    {
        public Task AddAsync(TRecord record);
        public Task DeleteAsync(TRecord record);
        public Task MarkFailedToUpload(TRecord record);
    }
}
