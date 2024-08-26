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
        public Task<bool> DeleteAsync(TRecord record);
        public Task<bool> MarkFailedToUpload(TRecord record);
    }
}
