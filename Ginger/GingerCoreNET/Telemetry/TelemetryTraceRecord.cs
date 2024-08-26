using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry
{
    internal sealed class TelemetryTraceRecord : TelemetryBaseRecord
    {
        internal Dictionary<string, string> Attributes { get; } = [];
    }
}
