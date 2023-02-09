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

using Amdocs.Ginger.Common;
using System;
using System.Xml;

namespace GingerCore.Drivers.InternalBrowserLib
{
    public class DeviceEmulation 
    {
        public string Devicename {get; set;}
        public int Height { get; set; }
        public int Width { get; set; }
        public string UserAgent { get; set; }

        public static ObservableList<DeviceEmulation> DevicelistCombo()
        {
            ObservableList<DeviceEmulation> Devices = new ObservableList<DeviceEmulation>();
            
            string devicelist = Properties.Resources.DevicesList;
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(devicelist);
            XmlNodeList xnList = xml.SelectNodes("//Device");

            foreach (XmlNode xn in xnList)
            {
                DeviceEmulation Dev = new DeviceEmulation();

                Dev.Devicename = xn.Attributes["name"].Value;
                Dev.Height = Int32.Parse(xn.Attributes["height"].Value);
                Dev.Width = Int32.Parse(xn.Attributes["width"].Value);
                Dev.UserAgent = xn.Attributes["useragent"].Value;

                Devices.Add(Dev);
            }

            DeviceEmulation Desktop = new DeviceEmulation();
            Desktop.Devicename = "Desktop";
            Desktop.UserAgent = null;
            Devices.Add(Desktop);

            return Devices;
        }
    }
}
