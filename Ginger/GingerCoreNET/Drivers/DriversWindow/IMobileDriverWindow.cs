#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.DriversWindow
{
    public interface IMobileDriverWindow
    {
        bool IsDeviceConnected { get; set; }

        bool IsUftLabDevice { get; }

        eAutoScreenshotRefreshMode DeviceAutoScreenshotRefreshMode { get; set; }

        eDevicePlatformType GetDevicePlatformType();

        eAppType GetAppType();

        void PerformBackButtonPress();

        void PerformHomeButtonPress();

        void PerformMenuButtonPress();

        void PerformVolumeButtonPress(eVolumeOperation volumeOperation);

        void PerformLockButtonPress(eLockOperation lockOperation, ActMobileDevice act=null);

        Byte[] GetScreenshotImage();

        void PerformTap(long x, long y);

        void PerformLongPress(long x, long y, TimeSpan? pressDuration = null);

        void PerformDrag(System.Drawing.Point start, System.Drawing.Point end, TimeSpan pressDuration, TimeSpan dragDuration);

        void PerformScreenSwipe(eSwipeSide swipeSide, double swipeScale, TimeSpan swipeDuration);

        void SwitchToLandscape();

        void SwitchToPortrait();

        eDeviceOrientation GetOrientation();

        void PerformSendKey(string key);

        void OpenDeviceSettings();

        void OpenDeviceExternalView();

        Dictionary<string, string> GetDeviceCPUInfo();

        Dictionary<string, string> GetDeviceMemoryInfo();

        Task<string> GetDeviceNetworkInfo();

        Dictionary<string, string> GetDeviceBatteryInfo();

        Dictionary<string, object> GetDeviceGeneralInfo();
        Dictionary<string, string> GetDeviceActivityAndPackage();

        Task<bool> IsRealDeviceAsync();
        string GetDeviceUDID();

    }
}
