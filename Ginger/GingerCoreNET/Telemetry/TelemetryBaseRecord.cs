using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal class TelemetryBaseRecord
    {
        public string Id { get; init; } = Guid.NewGuid().ToString();

        public required DateTime CreationTimestamp { get; init; }
        
        public required DateTime LastUpdateTimestamp { get; set; }

        public required string AppVersion { get; init; }

        public required string UserId { get; init; }

        public bool FailedToUpload { get; set; }

        public int RetryAttempt { get; set; }
    }
}
