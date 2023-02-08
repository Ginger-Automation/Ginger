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

using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
namespace GingerCore.Actions
{
    //This class is for UI Radio Button element
    public class ActRadioButton : Act
    {
        public override string ActionDescription { get { return "Radio Button Action"; } }
        public override string ActionUserDescription { get { return "Performs Radio button Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any Radio button actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a Radio button action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property" +
            " that you set in Locate By type.Then select Action Type and then enter the page url in value textbox and run the action.");
        }        

        public override string ActionEditPage { get { return null; } }
        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
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

        public enum eActRadioButtonAction
        {
            SelectByIndex = 1,
            // SelectByText = 2, //TODO: Re-enable SelectByText action for Radio Button once logic for it is in SeleniumDriver.cs, assuming it's worth effort.
            SelectByValue=3,
            Clear = 4,
            SetFocus = 5,
            GetValue=6,
            IsDisabled = 7,
            GetAvailableValues=8,
            IsDisplayed = 9,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        public eActRadioButtonAction RadioButtonAction
        {
            get
            {
                return (eActRadioButtonAction)GetOrCreateInputParam<eActRadioButtonAction>(nameof(RadioButtonAction), eActRadioButtonAction.SelectByIndex);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(RadioButtonAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "Radio Button:" + RadioButtonAction.ToString();
            }
        }
    }
}