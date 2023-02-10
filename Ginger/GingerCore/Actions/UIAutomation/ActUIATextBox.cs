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

namespace GingerCore.Actions
{
    //This class is for Text Box actions
    public class ActUIATextBox : Act
    {
        public override string ActionEditPage { get { return "ActUIATextBoxEditPage"; } }
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

        public enum eUIATextBoxAction
        {
            SetValue = 1,
            SetFocus = 2,
            Clear = 3,
            GetValue = 4,
            IsDisabled = 5,
            IsRequired = 6,
            GetFont = 7,
            IsPrepopulated = 8,
            IsDisplayed = 9,
            GetInputLength = 10,
            GetWidth = 22,
            GetHeight = 23,
            //GetStyle = 24, not sure how to do this in UIA
            IsKeyboardFocusable = 25,
        }

        public override string ActionDescription { get { return "UI TextBox Action"; } }
        public override string ActionUserDescription { get { return "UI TextBox Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

        public eUIATextBoxAction UIATextBoxAction
        {
            get
            {
                return (eUIATextBoxAction)GetOrCreateInputParam<eUIATextBoxAction>(nameof(UIATextBoxAction), eUIATextBoxAction.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(UIATextBoxAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "UIATextBox:" + UIATextBoxAction.ToString();
            }
        }
        public override eImageType Image { get { return eImageType.TextBox; } }
    }
}
