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
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile
{
    public enum eDevicePlatformType
    {
        Android = 0,
        iOS = 1
    }

    public enum eDeviceSource
    {
        LocalAppium = 0,
        MicroFoucsUFTMLab = 1
    }

    public enum eAppType
    {
        NativeHybride = 0,
        Web = 1,
    }

    public enum eSwipeSide
    {
        Up, Down, Left, Right
    }

    public enum eDeviceOrientation
    {
        Portrait, Landscape
    }

    public enum eAutoScreenshotRefreshMode
    {
        Live, PostOperation, Disabled
    }

    public enum eVolumeOperation
    {
        Up, Down
    }

    public enum eLockOperation
    {
        Lock, UnLock
    }

    public class MobileDriverCommon
    {

    }
}
