using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.DriversWindow
{
    public interface IMobileWindowDriver
    {
        eDevicePlatformType GetDevicePlatformType();

        eAppType GetAppType();

        void PerformBackButtonPress();

        void PerformHomeButtonPress();

        void PerformMenuButtonPress();
    }
}
