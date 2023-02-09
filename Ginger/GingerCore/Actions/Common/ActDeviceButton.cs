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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.Actions.Common
{
    // common action to mimic real device button operation like the home button
    public class ActDeviceButton : Act
    {
        public override string ActionDescription { get { return "Device Button"; } }
        public override string ActionUserDescription { get { return "Device Button"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to run click on device button like: Power/Volume/Home");
        }

        public override string ActionEditPage { get { return "_Common.ActDeviceButtonEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    //mPlatforms.Add(ePlatformType.AndroidDevice);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static String DeviceButtonAction = "DeviceButtonAction";
            public static String PressKey = "PressKey";            
        }

        public enum eDeviceButtonAction
        {
            Press,  // see ePressKey enum for options  - Home,Back etc...
            PressKeyCode,  // any int code - will be in Value  - need to lookup device keymap - a number
            Input  // Will send several chars/keys/string:  ABcd123!#
            // DO we need Long press? like for Power
        }

        // Press key is available for Press
        public enum ePressKey
        {
            Home,
            Menu,            
            Search,
            Back,
            Delete,
            DPadCenter,
            DPadDown,
            DPadLeft,
            DPadRight,
            DPadUp,
            Enter,
            RecentApps
        }

        public eDeviceButtonAction DeviceButtonAction
        {
            get
            {
                return (eDeviceButtonAction)GetOrCreateInputParam<eDeviceButtonAction>(nameof(DeviceButtonAction), eDeviceButtonAction.Press);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DeviceButtonAction), value.ToString());
            }
        }

        [IsSerializedForLocalRepository]
        public ePressKey PressKey { get; set; }

        public override String ActionType
        {
            get
            {
                return "DeviceButton. " + DeviceButtonAction;
            }
        }

        public override eImageType Image { get { return eImageType.BullsEye; } }
    }
}
