using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.Drivers.DriversWindows
{
    public class DriverWindowHandler
    {

        public static void Init()
        {
            DriversWindowUtils.DriverWindowEvent += DriverWindowUtils_DriverWindowEvent;
        }

        private static void DriverWindowUtils_DriverWindowEvent(DriverWindowEventArgs args)
        {
            switch(args.EventType)
            {
                case DriverWindowEventArgs.eEventType.DriverStart:
                    break;
            }
        }

                
    }
}
