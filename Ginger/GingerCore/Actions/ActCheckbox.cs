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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    //This class is for UI checkbox elemnet
    public class ActCheckbox : Act
    {
        public override string ActionDescription { get { return "Check Box Action"; } }
        public override string ActionUserDescription { get { return "Check/Un-Check a checkbox object"; } }
        
        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to automate a check/Un-check an object from type Checkbox." + Environment.NewLine + Environment.NewLine + "For Mobile use this action only in case running the flow on the native browser.");
        }        

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.ASCF);
                    mPlatforms.Add(ePlatformType.Web);
                    // Since, the action isn't supported by Windows Platform hence, it's commented
                    
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string CheckboxAction = "CheckboxAction";         
        }
        
        public enum eCheckboxAction
        {
            Check = 1,
            Uncheck = 2,
            SetFocus = 3,
            GetValue=4,
            IsDisplayed = 5,
            Click=6,
            IsDisabled=7,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        [IsSerializedForLocalRepository]
        public eCheckboxAction CheckboxAction { get; set; }

        public override String ActionType
        {
            get
            {
                return "Checkbox:" + CheckboxAction.ToString();
            }
        }
    }
}