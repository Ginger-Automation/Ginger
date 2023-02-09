#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
        public static void OnDriverWindowEvent(DriverWindowEventArgs.eEventType eventType, DriverBase driver, object dataObject)
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
            ShowDriverWindow, 
            CloseDriverWindow
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
