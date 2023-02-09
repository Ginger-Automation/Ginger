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

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;
using System.ComponentModel;

namespace GingerCore.Drivers.Common.Devices
{
    //TODO: move to common area for devices so can be used also for IOS devices or other
    // This class is serialized to JSON, it holds the device config
    [DataContract]
    public class DeviceConfig : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static class Fields
        {
            public static string Name = "Name";
            public static string DeviceName = "DeviceName";
            public static string DeviceImage = "DeviceImage";
            public static string DeviceImageScreenLeft = "DeviceImageScreenLeft";
            public static string DeviceImageScreenTop = "DeviceImageScreenTop";
            public static string DeviceImageScreenRight = "DeviceImageScreenRight";
            public static string DeviceImageScreenBottom = "DeviceImageScreenBottom";            
        }

        // Any name the user like: Nexus 5 with T-Mobile Logo, or Amazon Fire with new controller

        private string mName;
        [DataMember(Order = 1, IsRequired = true)]
        public string Name { get { return mName; } set { mName = value; OnPropertyChanged(Fields.Name); } }

        // Nexus 5, Galaxy Note 4...
        [DataMember(Order = 2, IsRequired = true)]
        public string DeviceName { get; set; }

        // LG, Samsusng...
        [DataMember]
        public string DeviceVendor { get; set; }

        
        // Image file of the device - picture to show the user which look like the real device or picture of the rela device
        [DataMember]        
        public string DeviceImage { get; set; }

        // On the device image where is the actual screen located

        private double mDeviceImageScreenLeft { get; set; }
        [DataMember]
        public double DeviceImageScreenLeft { get { return mDeviceImageScreenLeft; } set { mDeviceImageScreenLeft = value; OnPropertyChanged(Fields.DeviceImageScreenLeft); } }
        [DataMember]
        public double DeviceImageScreenTop { get; set; }        
        [DataMember]
        public double DeviceImageScreenRight { get; set; }
        [DataMember]
        public double DeviceImageScreenBottom { get; set; }

        //Buttons on the device itself
        [DataMember]
        public List<DeviceButton> DeviceButtons = new List<DeviceButton>();


        // Define all the controllers available for this device, usually remote controller
        [DataMember]
        public List<DeviceControllerConfig> DeviceControllers { get; set; }


        public void Save(string DeviceFolder)
        {
            FileStream FS = new FileStream(Path.Combine(DeviceFolder, "DeviceConfig.json.dat"), System.IO.FileMode.Create);
            DataContractJsonSerializerSettings formatting = new DataContractJsonSerializerSettings() { RootName = "root" };            
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DeviceConfig), formatting);            
            ser.WriteObject(FS, this);
            FS.Flush();
            FS.Close();
        }

        public static DeviceConfig LoadFromDeviceFolder(string DeviceFolder)
        {                        
            FileStream FS = new FileStream(Path.Combine(DeviceFolder,"DeviceConfig.json.dat"), System.IO.FileMode.Open, FileAccess.Read);            
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(DeviceConfig));
            DeviceConfig ADC = (DeviceConfig)ser.ReadObject(FS);            
            FS.Close();
            return ADC;
        }


        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
