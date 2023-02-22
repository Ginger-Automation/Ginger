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
    //This class is for multiselect list element
    public class ActMultiselectList : Act
    {
        public override string ActionDescription { get { return "Multi Select Action"; } }
        public override string ActionUserDescription { get { return "Click on a multi select object"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any multi select actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a multi select action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property" +
            " that you set in Locate By type and then enter the page url in value textbox and run the action.");
        }        

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        public override bool IsSelectableAction { get { return false; } }
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

        public enum eActMultiselectListAction
        {
            SetSelectedValueByIndex = 1,
            SetSelectedValueByValue = 2,
            SetSelectedValueByText = 3,
            ClearAllSelectedValues = 4,
            SetFocus = 5,
        }

        public eActMultiselectListAction ActMultiselectListAction
        {
            get
            {
                return (eActMultiselectListAction)GetOrCreateInputParam<eActMultiselectListAction>(nameof(ActMultiselectListAction), eActMultiselectListAction.SetSelectedValueByIndex);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ActMultiselectListAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "MultiselectList:" + ActMultiselectListAction.ToString();
            }
        }
    }
}