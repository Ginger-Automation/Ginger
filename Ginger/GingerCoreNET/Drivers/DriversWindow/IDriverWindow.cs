using GingerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.DriversWindow
{
    public interface IDriverWindow
    {
        bool ShowWindow { get; }

        string GetDriverWindowName(Agent.eDriverType driverSubType = Agent.eDriverType.NA);
    }
}
