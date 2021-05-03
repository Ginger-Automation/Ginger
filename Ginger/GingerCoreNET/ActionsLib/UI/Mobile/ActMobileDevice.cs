#region License
/*
Copyright © 2014-2021 European Support Limited

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
                return (ePressKey)GetOrCreateInputParam<ePressKey>(nameof(MobilePressKey), ePressKey.Home);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MobilePressKey), value.ToString());
                OnPropertyChanged(nameof(MobilePressKey));
            }
        }

        public string X1
        {
            get
            {
                return GetOrCreateInputParam(nameof(X1), "0").ToString();
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(X1), value.ToString());
                OnPropertyChanged(nameof(X1));
            }
        }

        public string Y1
        {
            get
            {
                return GetOrCreateInputParam(nameof(Y1), "0").ToString();
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Y1), value.ToString());
                OnPropertyChanged(nameof(Y1));
            }
        }

        public string X2
        {
            get
            {
                return GetOrCreateInputParam(nameof(X2), "0").ToString();
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(X2), value.ToString());
                OnPropertyChanged(nameof(X2));
            }
        }

        public string Y2
        {
            get
            {
                return GetOrCreateInputParam(nameof(Y2), "0").ToString();
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Y2), value.ToString());
                OnPropertyChanged(nameof(Y2));
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
        }

        public enum ePressKey
        {
            Keycode_PROG_BLUE,
            Keycode_POWER,
            Keycode_POUND,
            Keycode_PLUS,
            Keycode_PICTSYMBOLS,
            Keycode_PERIOD,
            Keycode_PAIRING,
            Keycode_PAGE_UP,
            Keycode_PAGE_DOWN,
            Keycode_P,
            Keycode_O,
            Keycode_NUM_LOCK,
            KeycodeNumpad_SUBTRACT,
            KeycodeNumpad_RIGHT_PAREN,
            KeycodeNumpad_MULTIPLY,
            Keycode_PROG_GREEN,
            KeycodeNumpad_LEFT_PAREN,
            Keycode_PROG_RED,
            Keycode_Q,
            Keycode_SPACE,
            KeycodeSoft_RIGHT,
            KeycodeSoft_LEFT,
            Keycode_SLEEP,
            Keycode_SLASH,
            Keycode_SHIFT_RIGHT,
            Keycode_SHIFT_LEFT,
            Keycode_SETTINGS,
            Keycode_SEMICOLON,
            Keycode_SEARCH,
            Keycode_SCROLL_LOCK,
            Keycode_S,
            Keycode_RO,
            Keycode_RIGHT_BRACKET,
            Keycode_R,
            Keycode_PROG_YELLOW,
            Keycode_STAR,
            KeycodeNumpad_EQUALS,
            KeycodeNumpad_DOT,
            Keycode_MOVE_HOME,
            Keycode_MOVE_END,
            Keycode_MINUS,
            Keycode_META_RIGHT,
            Keycode_META_LEFT,
            Keycode_MENU,
            Keycode_MEDIA_TOP_MENU,
            Keycode_MEDIA_STOP,
            Keycode_MEDIA_REWIND,
            Keycode_MEDIA_RECORD,
            Keycode_MEDIA_PREVIOUS,
            Keycode_MEDIA_PLAY_PAUSE,
            Keycode_MEDIA_PLAY,
            Keycode_MEDIA_PAUSE,
            Keycode_MEDIA_NEXT,
            Keycode_MUHENKAN,
            KeycodeNumpad_ENTER,
            Keycode_MUSIC,
            Keycode_N,
            KeycodeNumpad_DIVIDE,
            KeycodeNumpad_COMMA,
            KeycodeNumpad_ADD,
            KeycodeNumpad_9,
            KeycodeNumpad_8,
            KeycodeNumpad_7,
            KeycodeNumpad_6,
            KeycodeNumpad_5,
            KeycodeNumpad_4,
            KeycodeNumpad_3,
            KeycodeNumpad_2,
            KeycodeNumpad_1,
            KeycodeNumpad_0,
            Keycode_NUM,
            Keycode_NOTIFICATION,
            Keycode_MUTE,
            KeycodeSTB_INPUT,
            KeycodeSTB_POWER,
            Keycode_SWITCH_CHARSET,
            MetaAlt_LEFT_ON,
            Keycode_MAX,
            Keycode_ZOOM_OUT,
            Keycode_ZOOM_IN,
            Keycode_ZENKAKU_HANKAKU,
            Keycode_Z,
            Keycode_YEN,
            Keycode_Y,
            Keycode_X,
            Keycode_WINDOW,
            Keycode_WAKEUP,
            Keycode_W,
            Keycode_VOLUME_UP,
            Keycode_VOLUME_MUTE,
            Keycode_VOLUME_DOWN,
            MetaAlt_MASK,
            Keycode_VOICE_ASSIST,
            MetaAlt_ON,
            MetaCaps_LOCK_ON,
            MetaShift_RIGHT_ON,
            MetaShift_ON,
            MetaShift_MASK,
            MetaShift_LEFT_ON,
            MetaScroll_LOCK_ON,
            MetaNum_LOCK_ON,
            MetaMeta_RIGHT_ON,
            MetaMeta_ON,
            MetaMeta_MASK,
            MetaMeta_LEFT_ON,
            MetaFunction_ON,
            MetaCtrl_RIGHT_ON,
            MetaCtrl_ON,
            MetaCtrl_MASK,
            MetaCtrl_LEFT_ON,
            MetaAlt_RIGHT_ON,
            Keycode_V,
            Keycode_UNKNOWN,
            Keycode_U,
            KeycodeTV_INPUT_COMPOSITE_1,
            KeycodeTV_INPUT_COMPONENT_2,
            KeycodeTV_INPUT_COMPONENT_1,
            KeycodeTV_INPUT,
            KeycodeTV_DATA_SERVICE,
            KeycodeTV_CONTENTS_MENU,
            KeycodeTV_AUDIO_DESCRIPTION_MIX_UP,
            KeycodeTV_AUDIO_DESCRIPTION_MIX_DOWN,
            KeycodeTV_AUDIO_DESCRIPTION,
            KeycodeTV_ANTENNA_CABLE,
            Keycode_TV,
            Keycode_TAB,
            Keycode_T,
            Keycode_SYSRQ,
            Keycode_SYM,
            KeycodeTV_INPUT_COMPOSITE_2,
            KeycodeTV_INPUT_HDMI_1,
            KeycodeTV_INPUT_HDMI_2,
            KeycodeTV_INPUT_HDMI_3,
            KeycodeTV_ZOOM_MODE,
            KeycodeTV_TIMER_PROGRAMMING,
            KeycodeTV_TERRESTRIAL_DIGITAL,
            KeycodeTV_TERRESTRIAL_ANALOG,
            KeycodeTV_TELETEXT,
            KeycodeTV_SATELLITE_SERVICE,
            KeycodeTV_SATELLITE_CS,
            Keycode_MEDIA_FAST_FORWARD,
            KeycodeTV_SATELLITE_BS,
            KeycodeTV_RADIO_SERVICE,
            KeycodeTV_POWER,
            KeycodeTV_NUMBER_ENTRY,
            KeycodeTV_NETWORK,
            KeycodeTV_MEDIA_CONTEXT_MENU,
            KeycodeTV_INPUT_VGA_1,
            KeycodeTV_INPUT_HDMI_4,
            KeycodeTV_SATELLITE,
            MetaSym_ON,
            Keycode_MEDIA_EJECT,
            Keycode_MEDIA_AUDIO_TRACK,
            KeycodeButton_11,
            KeycodeButton_10,
            KeycodeButton_1,
            Keycode_BRIGHTNESS_UP,
            Keycode_BRIGHTNESS_DOWN,
            Keycode_BREAK,
            Keycode_BOOKMARK,
            Keycode_BACKSLASH,
            Keycode_BACK,
            Keycode_B,
            Keycode_AVR_POWER,
            Keycode_AVR_INPUT,
            Keycode_AT,
            Keycode_ASSIST,
            Keycode_APP_SWITCH,
            KeycodeButton_12,
            Keycode_APOSTROPHE,
            KeycodeButton_13,
            KeycodeButton_15,
            KeycodeButton_MODE,
            KeycodeButton_L2,
            KeycodeButton_L1,
            KeycodeButton_C,
            KeycodeButton_B,
            KeycodeButton_A,
            KeycodeButton_9,
            KeycodeButton_8,
            KeycodeButton_7,
            KeycodeButton_6,
            KeycodeButton_5,
            KeycodeButton_4,
            KeycodeButton_3,
            KeycodeButton_2,
            KeycodeButton_16,
            KeycodeButton_14,
            KeycodeButton_R1,
            Keycode_ALT_RIGHT,
            Keycode_A,
            FlagFromSystem,
            FlagFallback,
            FlagEditorAction,
            FlagCanceledLongPress,
            FlagCanceled,
            ActionUp,
            ActionMultiple,
            ActionDown,
            Space,
            Settings,
            Menu,
            Home,
            Enter,
            Del,
            BackSpace,
            FlagKeepTouchMode,
            Keycode_ALT_LEFT,
            FlagLongPress,
            FlagTracking,
            Keycode_9,
            Keycode_8,
            Keycode_7,
            Keycode_6,
            Keycode_5,
            Keycode_4,
            Keycode_3D_MODE,
            Keycode_3,
            Keycode_2,
            Keycode_12,
            Keycode_11,
            Keycode_1,
            Keycode_0,
            FlagWokeHere,
            FlagVirtualHardKey,
            FlagSoftKeyboard,
            KeycodeButton_R2,
            KeycodeButton_SELECT,
            KeycodeButton_START,
            Keycode_GRAVE,
            Keycode_G,
            Keycode_FUNCTION,
            Keycode_FORWARD_DEL,
            Keycode_FORWARD,
            Keycode_FOCUS,
            Keycode_F9,
            Keycode_F8,
            Keycode_F7,
            Keycode_F6,
            Keycode_F5,
            Keycode_F4,
            Keycode_F3,
            Keycode_F2,
            Keycode_F12,
            Keycode_GUIDE,
            Keycode_F11,
            Keycode_H,
            Keycode_HELP,
            Keycode_MANNER_MODE,
            Keycode_M,
            Keycode_LEFT_BRACKET,
            Keycode_LAST_CHANNEL,
            Keycode_LANGUAGE_SWITCH,
            Keycode_L,
            Keycode_KATAKANA_HIRAGANA,
            Keycode_KANA,
            Keycode_K,
            Keycode_J,
            Keycode_INSERT,
            Keycode_INFO,
            Keycode_I,
            Keycode_HOME,
            Keycode_HENKAN,
            Keycode_HEADSETHOOK,
            Keycode_F10,
            Keycode_F1,
            Keycode_F,
            Keycode_CLEAR,
            Keycode_CHANNEL_UP,
            Keycode_CHANNEL_DOWN,
            Keycode_CAPTIONS,
            Keycode_CAPS_LOCK,
            Keycode_CAMERA,
            Keycode_CALL,
            Keycode_CALENDAR,
            Keycode_CALCULATOR,
            Keycode_C,
            KeycodeButton_Z,
            KeycodeButton_Y,
            KeycodeButton_X,
            KeycodeButton_THUMBR,
            KeycodeButton_THUMBL,
            Keycode_COMMA,
            Keycode_CONTACTS,
            Keycode_CTRL_LEFT,
            Keycode_CTRL_RIGHT,
            Keycode_EXPLORER,
            Keycode_ESCAPE,
            Keycode_EQUALS,
            Keycode_ENVELOPE,
            Keycode_ENTER,
            Keycode_ENDCALL,
            Keycode_EISU,
            Keycode_MEDIA_CLOSE,
            Keycode_E,
            Keycode_DPAD_UP,
            Keycode_DPAD_RIGHT,
            Keycode_DPAD_LEFT,
            Keycode_DPAD_DOWN,
            Keycode_DPAD_CENTER,
            Keycode_DEL,
            Keycode_D,
            Keycode_DVR
        }


    }
}
