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
    public class ActHandleBrowserAlert : Act
    {
        public override string ActionDescription { get { return "Handle Browser Alerts"; } }
        public override string ActionUserDescription { get { return "Handle Browser Alerts"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to test/handle browser alert on any web pages");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select type of action from Action Type drop down and then enter the page url in value textbox and run the action.");
        }

        public override string ActionEditPage { get { return "ActHandleBrowserAlert"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public enum eHandleBrowseAlert
        {
            AcceptAlertBox = 1,
            DismissAlertBox = 2,
            GetAlertBoxText = 3,
            SendKeysAlertBox = 4,
        }

        [IsSerializedForLocalRepository]
        public eHandleBrowseAlert GenElementAction { get; set; }

        public override String ToString()
        {
            return "Generic Web Element: " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Generic Web Element: " + GenElementAction.ToString();
            }
        }
        public override System.Drawing.Image Image { get { return Resources.ActLink; } }
    }
}