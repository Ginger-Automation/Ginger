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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    public class ActUIASwitchWindow : Act
    {
        public override string ActionEditPage { get { return "ActUIASwitchWindowEditPage"; } }
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
                    // Since, the action isn't supported by Windows Platform hence, it's commented
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                }
                return mPlatforms;
            }
        }

        public enum eWindow
        {
            Application = 1,
            Parent = 2,
            Dialog = 3,
        }

        public override string ActionDescription { get { return "UI Switch Windows Action"; } }
        public override string ActionUserDescription { get { return "Allow to switch window by using title of window. "; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("1. Please select Locate ByTitle, please provide Window Title in Locate value.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("2. For No Title window please provide string NoTitleWindow in locate value.");
        }        

        public eWindow UIASwitchWindowAction
        {
            get
            {
                return (eWindow)GetOrCreateInputParam<eWindow>(nameof(UIASwitchWindowAction), eWindow.Application);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(UIASwitchWindowAction), value.ToString());
            }
        }

        public override String ActionType
        {
            get
            {
                return "UIASwitchWindow:" + UIASwitchWindowAction.ToString();
            }
        }
    }
}