using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryLogRecord : TelemetryBaseRecord
    {
        public required string Level { get; init; }

        public required string Message { get; init; }

        public string Metadata { get; init; }
    }
}
