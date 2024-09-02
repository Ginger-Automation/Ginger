using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal interface ITelemetryRetryService<TRecord> : IDisposable
    {
        public void StartMonitoring();

        public void StopMonitoring();
    }
}
