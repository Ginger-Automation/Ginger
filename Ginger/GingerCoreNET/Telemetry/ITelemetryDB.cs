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
        public Task AddAsync(IEnumerable<TRecord> records);
        public Task DeleteAsync(IEnumerable<TRecord> records);
        public Task IncrementUploadAttemptCountAsync(IEnumerable<TRecord> records);
        public Task<IEnumerable<TRecord>> GetRecordsForRetryAsync(IEnumerable<TRecord> exclude, int limit);
    }
}
