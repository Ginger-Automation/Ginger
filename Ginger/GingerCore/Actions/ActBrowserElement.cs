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
using Amdocs.Ginger.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    public class ActBrowserElement : Act
    {
        public new static partial class Fields
        {
            public static string ControlAction = "ControlAction";
            public static string ValueUC = "ValueUC";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string GotoURLType = "GotoURLType";
            public static string ImplicitWait = "ImplicitWait";
        }

        public override string ActionDescription { get { return "Browser Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }
            
        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
            {
                TBH.AddText("Action to handle Browser and Widgets");
                TBH.AddLineBreak();
                TBH.AddText("Widget are Internet Explorer browser inside Java form, they contain their own HTML and can have many HTML controls like regular Web Page");
                TBH.AddLineBreak();
                TBH.AddText("For Java- This action enable you to set the current active browser control and then locate a specific HTML element and run action on this element");
            }
        
            public override string ActionEditPage { get { return "ActBrowserElementEditPage"; } }
            public override bool ObjectLocatorConfigsNeeded { get { return false; } }
            public override bool ValueConfigsNeeded { get { return false; } }

            // return the list of platforms this action is supported on
            public override List<ePlatformType> Platforms
            {
                get
                {
                    if (mPlatforms.Count == 0)
                    {
                        mPlatforms.Add(ePlatformType.Java);
                        mPlatforms.Add(ePlatformType.Web);
                        mPlatforms.Add(ePlatformType.Mobile);
                        mPlatforms.Add(ePlatformType.PowerBuilder);
                        mPlatforms.Add(ePlatformType.Windows);
                    }
                    return mPlatforms;
                }
            }

        public enum eGotoURLType
        {
            [EnumValueDescription("Current")]
            Current,
            [EnumValueDescription("New Tab")]
            NewTab,
            [EnumValueDescription("New Window")]
            NewWindow,
        }

        [IsSerializedForLocalRepository]
        public eGotoURLType GotoURLRadioButton
        {
            get { return gotoURLRadioButton; }
            set
            {
                gotoURLRadioButton = value;
            }
        }
        private eGotoURLType gotoURLRadioButton = eGotoURLType.Current;

        private int mImplicitWait = 60;
        [IsSerializedForLocalRepository]
        public int ImplicitWait
        {
            get { return mImplicitWait; }
            set
            {
                mImplicitWait = value;
                OnPropertyChanged(Fields.ImplicitWait);
            }
        }

        public enum eControlAction
        {
            [EnumValueDescription("Initialize Browser")]
            InitializeBrowser,
            [EnumValueDescription("Get Page Source")]
            GetPageSource,
            [EnumValueDescription("Get Page URL")]
            GetPageURL,
            [EnumValueDescription("Switch Frame")]
            SwitchFrame,
            [EnumValueDescription("Switch to Default Frame")]
            SwitchToDefaultFrame,
            [EnumValueDescription("Switch to Parent Frame")]
            SwitchToParentFrame,
            [EnumValueDescription("Maximize Window")]
            Maximize,
            [EnumValueDescription("Close Tab")]
            Close,
            [EnumValueDescription("Switch Window")]
            SwitchWindow,
            [EnumValueDescription("Switch to Default Window")]
            SwitchToDefaultWindow,
            [EnumValueDescription("Inject Java Script")]
            InjectJS,
            [EnumValueDescription("Wait till page loaded")]
            CheckPageLoaded,
            [EnumValueDescription("Open URL in New Tab")]
            OpenURLNewTab,
            [EnumValueDescription("Goto URL")]
            GotoURL,
            [EnumValueDescription("Close Tab Except")]
            CloseTabExcept,
            [EnumValueDescription("Close All")]
            CloseAll,
            [EnumValueDescription("Refresh")]
            Refresh,
            [EnumValueDescription("Navigate Back")]
            NavigateBack,
            [EnumValueDescription("Dismiss Message Box")]
            DismissMessageBox,
            [EnumValueDescription("Delete All Cookies")]
            DeleteAllCookies,
            [EnumValueDescription("Accept Message Box")]
            AcceptMessageBox,
            [EnumValueDescription("Get Window Title")]
            GetWindowTitle,
            [EnumValueDescription("Get MessageBox Text")]
            GetMessageBoxText,
            [EnumValueDescription("Set AlertBox Text")]
            SetAlertBoxText,
            [EnumValueDescription("Run Java Script")]
            RunJavaScript
        }


        [IsSerializedForLocalRepository]
            public eControlAction ControlAction { get; set; }

            public override String ToString()
            {
                return "BrowserControl - " + ControlAction;
            }

            public override String ActionType
            {
                get
                {
                    return "BrowserControl: " + ControlAction.ToString();
                }
            }

            public override System.Drawing.Image Image { get { return Resources.ASCF16x16; } }
    }
}