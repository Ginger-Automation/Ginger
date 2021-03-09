using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.DriversWindow
{
    public class DriversWindowUtils
    {
        public static event DriverWindowEventHandler DriverWindowEvent;
        public delegate void DriverWindowEventHandler(DriverWindowEventArgs args);
        public static void OnAutomateBusinessFlowEvent(DriverWindowEventArgs.eEventType eventType, DriverBase driver, object dataObject)
        {
            DriverWindowEventHandler handler = DriverWindowEvent;
            if (handler != null)
            {
                handler(new DriverWindowEventArgs(eventType, driver, dataObject));
            }
        }
    }

    public class DriverWindowEventArgs
    {
        public enum eEventType
        {
            DriverStart
        }

        public eEventType EventType;
        public DriverBase Driver;
        public Object DataObject;

        public DriverWindowEventArgs(eEventType eventType, DriverBase driver, object dataObject)
        {
            this.EventType = eventType;
            this.Driver = driver;
            this.DataObject = dataObject;
        }
    }
}
