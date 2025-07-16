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

using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    public class ActBrowserElement : Act, IActPluginExecution
    {
        public new static partial class Fields
        {
            public static string ControlAction = "ControlAction";
            public static string ValueUC = "ValueUC";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string GotoURLType = "GotoURLType";
            public static string ImplicitWait = "ImplicitWait";
            public static string URLSrc = "URLSrc";
            public static string PomGUID = "PomGUID";
            public static string BlockedUrls = "sBlockedUrls";
            public readonly static string RequestFileName = "RequestFileName";
            public readonly static string ResponseFileName = "ResponseFileName";
            public readonly static string SaveLogToFile = "SaveLogToFile";
            public readonly static string ClearExistingLog = "ClearExistingLog";
        }

        public override string ActionDescription { get { return "Browser Action"; } }
        public override string ActionUserDescription { get { return "Browser Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
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

        public enum eURLSrc
        {
            [EnumValueDescription("Static")]
            Static,
            [EnumValueDescription("Page Objects Model URL")]
            UrlPOM,
        }


        public int ImplicitWait
        {
            get
            {
                int.TryParse(GetOrCreateInputParam(nameof(ImplicitWait)).Value, out var value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ImplicitWait), value.ToString());
                OnPropertyChanged(nameof(ImplicitWait));
            }
        }

       public bool ClearExistingLog
        {
            get
            {
                bool.TryParse(GetOrCreateInputParam(nameof(ClearExistingLog)).Value, out var value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ClearExistingLog), value.ToString());
                OnPropertyChanged(nameof(ClearExistingLog));
            }
        }
        public bool SaveLogToFile
        {
            get
            {
                bool.TryParse(GetOrCreateInputParam(nameof(SaveLogToFile)).Value, out var value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SaveLogToFile), value.ToString());
                OnPropertyChanged(nameof(SaveLogToFile));
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
            [EnumValueDescription("Get Browser Logs")]
            GetBrowserLog,
            [EnumValueDescription("Get Console Logs")]
            GetConsoleLog,
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
            RunJavaScript,
            [EnumValueDescription("Start Monitoring Network Logs")]
            StartMonitoringNetworkLog,
            [EnumValueDescription("Get Network Logs")]
            GetNetworkLog,
            [EnumValueDescription("Stop Monitoring Network Logs")]
            StopMonitoringNetworkLog,
            [EnumValueDescription("Set Blocked Urls")]
            SetBlockedUrls,
            [EnumValueDescription("Unblock Urls")]
            UnblockeUrls,
            [EnumValueDescription("Switch To Shadow DOM")]
            SwitchToShadowDOM,
            [EnumValueDescription("Switch To Default DOM")]
            SwitchToDefaultDOM

        }

        //TODO: For ActBroswer ObjectLocatrosConfigNeeded is false 
        //But still for Switch frame , intialize browser etc the locate by and locate value is binded with Act.cs LocateBy and LocateValue fields
        //We override this field to ignore ObjectConfigNeeded check only for this action
        //Need to remove all of this once restructring Act.cs
        public override eLocateBy LocateBy
        {
            get
            {
                return GetOrCreateInputParam<eLocateBy>(Act.Fields.LocateBy, eLocateBy.NA);
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateBy, value.ToString());
                OnPropertyChanged(Act.Fields.LocateBy);
                OnPropertyChanged(Act.Fields.Details);
            }
        }

        public override string LocateValue
        {
            get
            {
                return GetOrCreateInputParam(Act.Fields.LocateValue).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(Act.Fields.LocateValue, value);
                OnPropertyChanged(Act.Fields.LocateValue);
                OnPropertyChanged(Act.Fields.Details);
            }
        }



        public eControlAction ControlAction
        {
            get
            {
                return GetOrCreateInputParam<eControlAction>(Fields.ControlAction, eControlAction.GotoURL);
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.ControlAction, value.ToString());

                OnPropertyChanged(nameof(ControlAction));
            }
        }

        public override String ToString()
        {
            return "BrowserControl - " + ControlAction;
        }

        public bool NetworkLog
        {
            get
            {
                _ = bool.TryParse(GetOrCreateInputParam(nameof(NetworkLog)).Value, out var value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(NetworkLog), value.ToString());
                OnPropertyChanged(nameof(NetworkLog));
            }
        }

        public string NetworkUrl
        {
            get
            {
                return GetOrCreateInputParam(nameof(NetworkUrl)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(NetworkUrl), value);
                OnPropertyChanged(nameof(NetworkUrl));
            }
        }

        public string GetName()
        {
            return "BrowserAction";
        }

        public override String ActionType
        {
            get
            {
                return "BrowserControl: " + ControlAction.ToString();
            }
        }

        public override eImageType Image { get { return eImageType.Globe; } }
        public string PomGUID
        {
            get
            {
                return GetOrCreateInputParam(Fields.PomGUID).Value;
            }
            set
            {
                GetOrCreateInputParam(Fields.PomGUID).Value = value;
                OnPropertyChanged(nameof(PomGUID));
            }
        }



        public PlatformAction GetAsPlatformAction()
        {
            PlatformAction platformAction = new PlatformAction(this);




            foreach (ActInputValue aiv in this.InputValues)
            {
                if (!platformAction.InputParams.ContainsKey(aiv.Param))
                {
                    platformAction.InputParams.Add(aiv.Param, aiv.ValueForDriver);
                }
            }


            return platformAction;
        }

        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound && name == "GotoURLRadioButton")
            {
                return true;
            }
            return false;
        }

        public enum eMonitorUrl
        {
            [EnumValueDescription("All")]
            AllUrl,
            [EnumValueDescription("Specific URLs")]
            SelectedUrl,
        }

        public enum eRequestTypes
        {
            [EnumValueDescription("All")]
            All,
            [EnumValueDescription("Fetch/XHR")]
            FetchOrXHR,

        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> UpdateOperationInputValues = [];
        public string sBlockedUrls
        {
            get
            {
                string value = GetOrCreateInputParam(nameof(sBlockedUrls)).Value;
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(sBlockedUrls), value);
                OnPropertyChanged(nameof(sBlockedUrls));
            }
        }
        public string ResponseFileName
        {
            get
            {
                string value = GetOrCreateInputParam(nameof(ResponseFileName)).Value;
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ResponseFileName), value);
                OnPropertyChanged(nameof(ResponseFileName));
            }
        }
        public string RequestFileName
        {
            get
            {
                string value = GetOrCreateInputParam(nameof(RequestFileName)).Value;
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(RequestFileName), value);
                OnPropertyChanged(nameof(RequestFileName));
            }
        }

    }
}
