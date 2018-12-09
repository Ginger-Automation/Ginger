#region License
/*
Copyright © 2014-2018 European Support Limited

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

namespace GingerCore.Actions
{
    public class ActMobileDevice : Act
    {
        public override string ActionDescription { get { return "Mobile Device Action"; } }
        public override string ActionUserDescription { get { return "Run events which can be performed on mobile device"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
        }        

        public override string ActionEditPage { get { return "ActMobileDeviceEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public enum eMobileDeviceAction
        {
            PressXY,
            LongPressXY,
            TapXY,
            DragXYXY,
            PressBackButton ,
            PressHomeButton,
            PressMenuButton,
            PressCamera,
            PressVolumeUp,
            PressVolumeDown,
            PressSwitchApp,
            PressLongHome,
            SwipeUp,
            SwipeDown,
            SwipeRight,
            SwipeLeft,
            Wait,
            TakeScreenShot,
            RefreshDeviceScreenImage,
            PressKey,
            OpenAppByName
        }

        public enum ePressKey
        {
            OK,
            MENU,
            HOME,
            BACK,
            LEFT,
            RIGHT,
            UP,
            DOWN,
            CLEAR,
            KEYBOARD_GO,
            KEYBOARD_SEARCH,
            KEYBOARD_SEND,
            KEYBOARD_NEXT,
            KEYBOARD_DONE,
            KEYBOARD_PREVIOUS,
            KEYBOARD_ENTER,
            KEYBOARD_SUBMIT
        }

        [IsSerializedForLocalRepository]
        public eMobileDeviceAction MobileDeviceAction { get; set; }

        [IsSerializedForLocalRepository]
        public ePressKey MobilePressKey { get; set; }

        public override String ToString()
        {
            return "Mobile Device Action: " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Mobile Device Action: " + MobileDeviceAction.ToString();
            }
        }

        public override System.Drawing.Image Image { get { return Resources.MobileDevice_32x32; } }
    }
}
