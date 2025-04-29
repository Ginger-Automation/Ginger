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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

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
                return GetOrCreateInputParam<eMobileDeviceAction>(nameof(MobileDeviceAction), eMobileDeviceAction.PressBackButton);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MobileDeviceAction), value.ToString());
                OnPropertyChanged(nameof(MobileDeviceAction));
            }
        }

        public eAuthResultSimulation AuthResultSimulation
        {
            get
            {
                return GetOrCreateInputParam<eAuthResultSimulation>(nameof(AuthResultSimulation), eAuthResultSimulation.Success);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(AuthResultSimulation), value.ToString());
                OnPropertyChanged(nameof(AuthResultSimulation));
            }
        }
        public eRotateDeviceState RotateDeviceState
        {
            get
            {
                return GetOrCreateInputParam<eRotateDeviceState>(nameof(RotateDeviceState), eRotateDeviceState.Portrait);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(RotateDeviceState), value.ToString());
                OnPropertyChanged(nameof(RotateDeviceState));
            }
        }

        public ePerformanceTypes PerformanceTypes
        {
            get
            {
                return GetOrCreateInputParam<ePerformanceTypes>(nameof(PerformanceTypes), ePerformanceTypes.Batteryinfo);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PerformanceTypes), value.ToString());
                OnPropertyChanged(nameof(PerformanceTypes));
            }
        }


        public eAuthResultDetailsFailureSimulation AuthResultDetailsFailureSimulation
        {
            get
            {
                return GetOrCreateInputParam<eAuthResultDetailsFailureSimulation>(nameof(AuthResultDetailsFailureSimulation), eAuthResultDetailsFailureSimulation.NotRecognized);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(AuthResultDetailsFailureSimulation), value.ToString());
                OnPropertyChanged(nameof(AuthResultDetailsFailureSimulation));
            }
        }

        public eAuthResultDetailsCancelSimulation AuthResultDetailsCancelSimulation
        {
            get
            {
                return GetOrCreateInputParam<eAuthResultDetailsCancelSimulation>(nameof(AuthResultDetailsCancelSimulation), eAuthResultDetailsCancelSimulation.User);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(AuthResultDetailsCancelSimulation), value.ToString());
                OnPropertyChanged(nameof(AuthResultDetailsCancelSimulation));
            }
        }

        public ePressKey MobilePressKey
        {
            get
            {
                return GetOrCreateInputParam<ePressKey>(nameof(MobilePressKey), ePressKey.Keycode_HOME);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MobilePressKey), value.ToString());
                OnPropertyChanged(nameof(MobilePressKey));
            }
        }


        public ActInputValue FilePathInput
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilePathInput));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilePathInput), value.ToString());
                OnPropertyChanged(nameof(FilePathInput));
            }
        }
        public ActInputValue LocalFolderPathInput // need to check
        {
            get
            {
                return GetOrCreateInputParam(nameof(LocalFolderPathInput));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(LocalFolderPathInput), value.ToString());
                OnPropertyChanged(nameof(LocalFolderPathInput));
            }
        }

        public ActInputValue FolderPathInput
        {
            get
            {
                return GetOrCreateInputParam(nameof(FolderPathInput));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FolderPathInput), value.ToString());
                OnPropertyChanged(nameof(FolderPathInput));
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

        public ActInputValue ActionAppPackage
        {
            get
            {
                return GetOrCreateInputParam(nameof(ActionAppPackage), "default");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActionAppPackage), value.ToString());
                OnPropertyChanged(nameof(ActionAppPackage));
            }
        }

        public ActInputValue ActionInput
        {
            get
            {
                return GetOrCreateInputParam(nameof(ActionInput));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActionInput), value.ToString());
                OnPropertyChanged(nameof(ActionInput));
            }
        }
      
        public ActInputValue PressDuration
        {
            get
            {
                return GetOrCreateInputParam(nameof(PressDuration), "200");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PressDuration), value.ToString());
                OnPropertyChanged(nameof(PressDuration));
            }
        }

        public ActInputValue DragDuration
        {
            get
            {
                return GetOrCreateInputParam(nameof(DragDuration), "200");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DragDuration), value.ToString());
                OnPropertyChanged(nameof(DragDuration));
            }
        }

        public ActInputValue SwipeScale
        {
            get
            {
                return GetOrCreateInputParam(nameof(SwipeScale), "1");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SwipeScale), value.ToString());
                OnPropertyChanged(nameof(SwipeScale));
            }
        }

        public ActInputValue SwipeDuration
        {
            get
            {
                return GetOrCreateInputParam(nameof(SwipeDuration), "200");
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SwipeDuration), value.ToString());
                OnPropertyChanged(nameof(SwipeDuration));
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

        ObservableList<MobileTouchOperation> mMobileTouchOperations = [];
        [IsSerializedForLocalRepository]
        public ObservableList<MobileTouchOperation> MobileTouchOperations
        {
            get
            {
                return mMobileTouchOperations;
            }
            set
            {
                mMobileTouchOperations = value;
                OnPropertyChanged(nameof(MobileTouchOperations));
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

        public enum eAuthResultSimulation
        {
            [EnumValueDescription("Success")]
            Success,
            [EnumValueDescription("Failure")]
            Failure,
            [EnumValueDescription("Cancel")]
            Cancel
        }
        public enum ePerformanceTypes
        {
            [EnumValueDescription("cpuinfo")]
            Cpuinfo,
            [EnumValueDescription("memoryinfo")]
            Memoryinfo, 
            [EnumValueDescription("batteryinfo")]
            Batteryinfo,
            [EnumValueDescription("networkinfo")]
            Networkinfo,
            [EnumValueDescription("diskinfo")]
            Diskinfo,
        }



        public enum eRotateDeviceState
        {
            [EnumValueDescription("Landscape")]
            Landscape,
            [EnumValueDescription("Portrait")]
            Portrait
        }
        public enum eAuthResultDetailsFailureSimulation
        {
            [EnumValueDescription("Not Recognized")]
            NotRecognized,
            [EnumValueDescription("Lockout")]
            Lockout,
            [EnumValueDescription("Finger Incomplete (Android Only)")]
            FingerIncomplete,
            [EnumValueDescription("Sensor Dirty (Android Only)")]
            SensorDirty,
            [EnumValueDescription("No Fingerprint Registered (iOS Only)")]
            NoFingerprintRegistered
        }

        public enum eAuthResultDetailsCancelSimulation
        {
            [EnumValueDescription("User Cancel")]
            User,
            [EnumValueDescription("System Cancel")]
            System
        }

        public enum eMobileDeviceAction
        {
            [EnumValueDescription("Press XY")]
            PressXY,
            [EnumValueDescription("Long Press XY")]
            LongPressXY,
            [EnumValueDescription("Tap XY")]
            TapXY,
            [EnumValueDescription("Double Tap XY")]
            DoubleTapXY,
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
            [EnumValueDescription("Simulate Barcode")]
            SimulateBarcode,
            [EnumValueDescription("Simulate Biometrics")]
            SimulateBiometrics,
            [EnumValueDescription("Stop Simulate Photo\\Video")]
            StopSimulatePhotoOrVideo,
            [EnumValueDescription("Get Available Contexts")]
            GetAvailableContexts,
            [EnumValueDescription("Set Context")]
            SetContext,
            [EnumValueDescription("Open Deep Link")]
            OpenDeeplink,
            [EnumValueDescription("Is Keyboard Visible")]
            IsKeyboardVisible,
            [EnumValueDescription("Is Device Locked")] 
            IsLocked,
            [EnumValueDescription("Is App Installed")]
            IsAppInstalled,
            [EnumValueDescription("Remove App")]
            RemoveApp,
            [EnumValueDescription("Get App State")]
            QueryAppState,
            [EnumValueDescription("Simulate Device Rotation")]
            RotateSimulation,
            [EnumValueDescription("Run Script")]
            RunScript,
            [EnumValueDescription("Start Recording Screen")]
            StartRecordingScreen,
            [EnumValueDescription("Stop Recording Screen")]
            StopRecordingScreen,
            [EnumValueDescription("Hide Keyboard")]
            HideKeyboard,
            [EnumValueDescription("Push File to Device")]
            PushFileToDevice,
            [EnumValueDescription("Pull File From Device")]
            PullFileFromDevice,
            [EnumValueDescription("Set Clipboard Text")]
            SetClipboardText,
            [EnumValueDescription("Get Specific Performance Data")]
            GetSpecificPerformanceData,
            [EnumValueDescription("Get Device Logs")]
            GetDeviceLogs,
            [EnumValueDescription("Get Clipboard Text")]
            GetClipboardText,
            [EnumValueDescription("Perform Multi Touch")]
            PerformMultiTouch,
            [EnumValueDescription("Grant App Permission")]
            GrantAppPermission,
            [EnumValueDescription("Type Using keyboard")]
            TypeUsingIOSkeyboard,
            [EnumValueDescription("Clear App Data")]
            ClearAppData,
            [EnumValueDescription("Get Screen Size")]
            ScreenSize,
            [EnumValueDescription("Open Notifications Panel")]
            OpenNotificationsPanel,
            [EnumValueDescription("Get Device Time")]
            GetDeviceTime,
            [EnumValueDescription("Get Orientation")]
            GetOrientation,
            [EnumValueDescription("Get App Package")]
            GetAppPackage,
            [EnumValueDescription("Get Current Activity Details")]
            GetCurrentActivityDetails,
            [EnumValueDescription("Lock for Duration")]
            LockForDuration,
            [EnumValueDescription("Get Settings")]
            GetSettings,
            [EnumValueDescription("Toggle Location Services")]
            ToggleLocationServices,
            [EnumValueDescription("Toggle Data")]
            ToggleData,
            [EnumValueDescription("Toggle Airplane Mode")]
            ToggleAirplaneMode,
            [EnumValueDescription("Toggle WIFI")] 
            ToggleWifi,
            [EnumValueDescription("Is IME Active")] 
            IsIMEActive,
            [EnumValueDescription("Get IME Active Engine")]
            GetIMEActiveEngine,
            [EnumValueDescription("Start Activity")] 
            StartActivity,
            [EnumValueDescription("Get GeoLocation")] 
            GetGeoLocation,
            [EnumValueDescription("Send App to Background")] 
            SendAppToBackground,
            [EnumValueDescription("Set Network Connection")]
            SetNetworkConnection,
            [EnumValueDescription("Get Device OS Type")]
            GetDeviceOSType,



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
