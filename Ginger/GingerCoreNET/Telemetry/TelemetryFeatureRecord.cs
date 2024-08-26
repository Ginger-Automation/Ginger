using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryFeatureRecord : TelemetryBaseRecord
    {
        public required string FeatureId { get; init; }

        public TimeSpan? Duration { get; init; }
    }
}
