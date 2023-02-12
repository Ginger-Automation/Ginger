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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
//This class is for Button actions
namespace GingerCore.Actions
{
    public class ActUIAButton : Act
    {
        public override string ActionDescription { get { return "UI Button Action"; } }
        public override string ActionUserDescription { get { return "UI Button Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public override string ActionEditPage { get { return "ActUIAButtonEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }
        public override bool IsSelectableAction { get => false; }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    // Since, the action isn't supported by Windows Platform hence, it's commented
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                }
                return mPlatforms;
            }
        }

        public enum eButtonAction
        {
            Click = 1,
            GetValue = 2,
            IsDisabled = 3,
            GetFont = 4,
            IsDisplayed = 5,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        public eButtonAction ButtonAction
        {
            get
            {
                return (eButtonAction)GetOrCreateInputParam<eButtonAction>(nameof(ButtonAction), eButtonAction.Click);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ButtonAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "Button." + ButtonAction;
            }
        }

        public override eImageType Image { get { return eImageType.Button; } }
    }
}
