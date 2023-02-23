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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions
{
    public class ActMobileDevice : Act
    {
        public override string ActionDescription { get { return "Mobile Device Action"; } }
        public override string ActionUserDescription { get { return "USed to performe operations on the connected mobile device"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override String ToString()
        {
            return "Mobile Device Action: " + MobileDeviceAction.ToString();
        }

        public override String ActionType
        {
            get
            {
                return "Mobile Device Action: " + MobileDeviceAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.Mobile; } }

        public eMobileDeviceAction MobileDeviceAction
        {
            get
            {
                return (eMobileDeviceAction)GetOrCreateInputParam<eMobileDeviceAction>(nameof(MobileDeviceAction), eMobileDeviceAction.PressBackButton);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MobileDeviceAction), value.ToString());
                OnPropertyChanged(nameof(MobileDeviceAction));
            }
        }

        public ePressKey MobilePressKey
        {
            get
            {
                return (ePressKey)GetOrCreateInputParam<ePressKey>(nameof(MobilePressKey), ePressKey.Keycode_HOME);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MobilePressKey), value.ToString());
                OnPropertyChanged(nameof(MobilePressKey));
            }
        }

        public ActInputValue X1
        {
            get
            {
                return GetOrCreateInputParam(nameof(X1), "0");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(X1), value.ToString());
                OnPropertyChanged(nameof(X1));
            }
        }

        public ActInputValue Y1
        {
            get
            {
                return GetOrCreateInputParam(nameof(Y1), "0");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Y1), value.ToString());
                OnPropertyChanged(nameof(Y1));
            }
        }

        public ActInputValue X2
        {
            get
            {
                return GetOrCreateInputParam(nameof(X2), "0");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(X2), value.ToString());
                OnPropertyChanged(nameof(X2));
            }
        }

        public ActInputValue Y2
        {
            get
            {
                return GetOrCreateInputParam(nameof(Y2), "0");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Y2), value.ToString());
                OnPropertyChanged(nameof(Y2));
            }
        }

        public string mSimulatedPhotoPath;
        public ActInputValue SimulatedPhotoPath
        {
            get
            {
                return this.GetOrCreateInputParam(nameof(SimulatedPhotoPath), "");
            }
            set
            {
                this.GetOrCreateInputParam(nameof(SimulatedPhotoPath), value.ToString());
                OnPropertyChanged(nameof(SimulatedPhotoPath));
            }
        }

        public override string ActionEditPage { get { return "ActMobileDeviceEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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
            [EnumValueDescription("Press XY")]
            PressXY,
            [EnumValueDescription("Long Press XY")]
            LongPressXY,
            [EnumValueDescription("Tap XY")]
            TapXY,
            [EnumValueDescription("Drag XY to XY")]
            DragXYXY,
            [EnumValueDescription("Get Current Application Identifiers")]
            GetCurrentApplicationInfo,

            [EnumValueDescription("Press Back Button")]
            PressBackButton,
            [EnumValueDescription("Press Home Button")]
            PressHomeButton,
            [EnumValueDescription("Press Menu Button")]
            PressMenuButton,
            [EnumValueDescription("Open Device Camera")]
            OpenCamera,
            [EnumValueDescription("Press Volume Up Button")]
            PressVolumeUp,
            [EnumValueDescription("Press Volume Down Button")]
            PressVolumeDown,
            [EnumValueDescription("Press Key")]
            PressKey,
            [EnumValueDescription("Long Press Key")]
            LongPressKey,
            [EnumValueDescription("Swipe Up")]
            SwipeUp,
            [EnumValueDescription("Swipe Down")]
            SwipeDown,
            [EnumValueDescription("Swipe Right")]
            SwipeRight,
            [EnumValueDescription("Swipe Left")]
            SwipeLeft,
            [EnumValueDescription("Swipe by XY")]
            SwipeByCoordinates,
            [EnumValueDescription("Take Screenshot")]
            TakeScreenShot,
            [EnumValueDescription("Open App")]
            OpenApp,
            [EnumValueDescription("Close App")]
            CloseApp,
            [EnumValueDescription("Unlock Device")]
            UnlockDevice,
            [EnumValueDescription("Lock Device")]
            LockDevice,
            [EnumValueDescription("Get Page Source")]
            GetPageSource,
            [EnumValueDescription("Get Device Battery %")]
            GetDeviceBattery,
            [EnumValueDescription("Get Device Network Info")]
            GetDeviceNetwork,
            [EnumValueDescription("Get App CPU Usage")]
            GetDeviceCPUUsage,
            [EnumValueDescription("Get App RAM Usage")]
            GetDeviceRAMUsage,
            [EnumValueDescription("Get Device General Info")]
            GetDeviceGeneralInfo,
            [EnumValueDescription("Simulate Photo")]
            SimulatePhoto,
            [EnumValueDescription("Stop Simulate Photo\\Video")]
            StopSimulatePhotoOrVideo,
        }

        public enum ePressKey
        {
            Keycode_PAGE_UP = 92,
            Keycode_PAGE_DOWN = 93,
            Keycode_P = 44,
            Keycode_O = 43,
            Keycode_Q = 45,
            Keycode_SPACE = 62,
            Keycode_SLASH = 76,
            Keycode_SHIFT_RIGHT = 60,
            Keycode_SHIFT_LEFT = 59,
            Keycode_SETTINGS = 176,
            Keycode_SEMICOLON = 74,
            Keycode_SEARCH = 84,
            Keycode_S = 47,
            Keycode_R = 46,
            Keycode_MOVE_HOME = 122,
            Keycode_MOVE_END = 123,
            Keycode_MINUS = 69,
            Keycode_MENU = 82,
            Keycode_MEDIA_STOP = 86,
            Keycode_MEDIA_REWIND = 89,
            Keycode_MEDIA_RECORD = 130,
            Keycode_MEDIA_PREVIOUS = 88,
            Keycode_MEDIA_PLAY = 126,
            Keycode_MEDIA_PAUSE = 127,
            Keycode_MEDIA_NEXT = 87,
            Keycode_MUSIC = 209,
            Keycode_N = 42,
            Keycode_NOTIFICATION = 83,
            Keycode_MUTE = 91,
            Keycode_ZOOM_OUT = 169,
            Keycode_ZOOM_IN = 168,
            Keycode_Z = 54,
            Keycode_Y = 53,
            Keycode_X = 52,
            Keycode_W = 51,
            Keycode_VOLUME_UP = 24,
            Keycode_VOLUME_MUTE = 164,
            Keycode_VOLUME_DOWN = 25,
            Keycode_VOICE_ASSIST = 231,
            Keycode_V = 50,
            Keycode_U = 49,
            Keycode_T = 48,
            Keycode_BRIGHTNESS_UP = 221,
            Keycode_BRIGHTNESS_DOWN = 220,
            Keycode_BACKSLASH = 73,
            Keycode_BACK = 4,
            Keycode_B = 30,
            Keycode_ASSIST = 219,
            Keycode_APP_SWITCH = 187,
            Keycode_A = 29,
            Keycode_9 = 16,
            Keycode_8 = 15,
            Keycode_7 = 14,
            Keycode_6 = 13,
            Keycode_5 = 12,
            Keycode_4 = 11,
            Keycode_3 = 10,
            Keycode_2 = 9,
            Keycode_1 = 8,
            Keycode_0 = 7,
            Keycode_G = 35,
            Keycode_H = 36,
            Keycode_HELP = 259,
            Keycode_M = 41,
            Keycode_L = 40,
            Keycode_K = 39,
            Keycode_J = 38,
            Keycode_I = 37,
            Keycode_HOME = 3,
            Keycode_F = 34,
            Keycode_CLEAR = 28,
            Keycode_CAMERA = 27,
            Keycode_CALL = 5,
            Keycode_CALENDAR = 208,
            Keycode_CALCULATOR = 210,
            Keycode_C = 31,
            Keycode_CONTACTS = 207,
            Keycode_ENTER = 66,
            Keycode_ENDCALL = 6,
            Keycode_E = 33,
            Keycode_DEL = 67,
            Keycode_D = 32,
        }

        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.SetValueException)
            {
                if (name == "MobilePressKey")
                {
                    return true;
                }
                else if (name == nameof(MobileDeviceAction) && value == "Wait")
                {
                    return true;
                }
                else if (name == "MobilePressXY")
                {
                    this.MobileDeviceAction = eMobileDeviceAction.PressXY;
                    return true;
                }
            }
            return false;
        }
    }
}
