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

namespace GingerCore.Drivers.Common.Devices
{
    // class for Device Button, can be on the device itself - the phone/tablet etc, or on the remote control, like Amazon Fire TV, each button on the real controller is one action
    public class DeviceButton
    {
        public enum eButtonShape
        {
            Rectangle,
            Ellipse
        }

        public string Name {get; set;}
        public string ToolTip { get; set; }

        //info about the action to send, for Andorid can be 'input keyevent 3'
        public string SendCommand { get; set; }
        public eButtonShape ButtonShape { get; set; }

        //Location where this control is located 
        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
