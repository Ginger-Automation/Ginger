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

namespace GingerCore.Drivers.Common.Devices
{
    //TODO: move to common area for devices so can be used also for IOS devices or other
    public class DeviceControllerConfig
    {
        // This class is serialized to JSON, it holds the device controller config, image buttons etc
        [DataContract]
        public class AndroidDeviceConfig
        {
            // Any name the user like: Amazon Fire new controller
            [DataMember(Order = 1, IsRequired = true)]
            public string Name { get; set; }

            // Nexus 5, Galaxy Note 4...
            [DataMember(Order = 2, IsRequired = true)]
            public string ControllerName { get; set; }

            // LG, Samsusng...
            [DataMember]
            public string ControllerVendor { get; set; }


            // Image file of the controller - picture to show the user which look like the real controller
            [DataMember]
            public string ControllerImage { get; set; }


            // Define all the controller actions/buttons
            [DataMember]
            public List<DeviceButton> ControllerButtons { get; set; }
        }
    }
}
