using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface ITelemetry
    {
        ITelemetry GetTelemetry { get; }

        ITelemetry SetTelemetry { set; }
    }
}
