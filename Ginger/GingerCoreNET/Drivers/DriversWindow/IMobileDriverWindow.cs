using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.DriversWindow
{
    public interface IMobileDriverWindow
    {
        bool IsDeviceConnected { get; set; }

        eAutoScreenshotRefreshMode DeviceAutoScreenshotRefreshMode { get; set; }

        eDevicePlatformType GetDevicePlatformType();

        eAppType GetAppType();        

        void PerformBackButtonPress();

        void PerformHomeButtonPress();

        void PerformMenuButtonPress();

        void PerformVolumeButtonPress(eVolumeOperation volumeOperation);

        void PerformLockButtonPress(eLockOperation lockOperation);

        Byte[] GetScreenshotImage();

        void PerformTap(long x, long y);

        void PerformDrag(System.Drawing.Point start, System.Drawing.Point end);

        void SwitchToLandscape();

        void SwitchToPortrait();

        eDeviceOrientation GetOrientation();
    }
}
