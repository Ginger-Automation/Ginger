#region License
/*
Copyright © 2014-2019 European Support Limited

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
namespace GingerCore.Actions
{
    //This class is for UI link element
    public class ActSmartSync : Act
    {
        public override string ActionDescription { get { return "Smart Sync Action"; } }
        public override string ActionUserDescription { get { return "Smart Sync"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate smart sync.");
        }        

        public override string ActionEditPage { get { return "ActSmartSyncEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.Windows);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                }
                return mPlatforms;
            }
        }

        public enum eSmartSyncAction
        {
            WaitUntilDisplay = 1,
            WaitUntilDisapear = 2, 
        }
        public new static partial class Fields
        {
            public static string WaitTime = "WaitTime";
        }
        [IsSerializedForLocalRepository]
        public int? WaitTime { set; get; }
        [IsSerializedForLocalRepository]
        public eSmartSyncAction SmartSyncAction { get; set; }

        public override String ToString()
        {
            return "SmartSync: " + GetInputParamValue("Value");            
        }

        public override String ActionType
        {
            get
            {
                return "SmartSync: " + SmartSyncAction.ToString();
            }
        }
        public override System.Drawing.Image Image { get { return Resources.ActLink; } } 
    }
}
