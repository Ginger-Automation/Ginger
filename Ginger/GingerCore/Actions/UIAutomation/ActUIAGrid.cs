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
using Amdocs.Ginger.Common.InterfacesLib;
// This class is for Grid actions
namespace GingerCore.Actions
{    
    public class ActUIAGrid : Act
    {
        public override string ActionDescription { get { return "UI Grid Action"; } }
        public override string ActionUserDescription { get { return string.Empty; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }        

        public override string ActionEditPage { get { return "ActUIAGridEditPage"; } }
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

        public enum eGridAction
        {
            ClickCell = 1,
            GetRowCnt = 2,
            GetColumnCnt = 3,
            GetCellFont = 4,
            SelectRow = 5,
            GetCellControlType = 6,
            GetCellValue = 7,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
            GetFullGridData =25
        }

        [IsSerializedForLocalRepository]
        public eGridAction GridAction{get;set;}

        public override String ActionType { get
            {
                return "Grid." + GridAction.ToString();
            }
             }

        public override System.Drawing.Image Image { get { return Resources.List; } }
    }
}