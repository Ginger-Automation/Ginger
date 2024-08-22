using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Telemetry.Pipeline
{
    internal interface ITelemetryStep<TRecord>
    {
        public void Process(TRecord record);
    }
}
