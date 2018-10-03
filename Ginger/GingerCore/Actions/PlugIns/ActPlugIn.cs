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

using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;

namespace GingerCore.Actions.PlugIns
{
    public class ActPlugIn : ActWithoutDriver
    {
      
        public override string ActionType
        {
            get
            {
                return "PlugIn";  //TODO: return the plug in ActionType Name
            }
        }

        public override string ActionDescription
        {
            get
            {                
                return "Plugin Description";
            }
        }

        public override string ActionEditPage { get { return "PlugIns.ActPlugInEditPage"; } }  

        public override string ActionUserDescription
        {
            get
            {                
                return "Plugin Action";   
            }
        }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("See Plugin Documentation");             
        }

        public override bool ObjectLocatorConfigsNeeded
        {
            get
            {
                return false;
            }
        }

        public override bool ValueConfigsNeeded
        {
            get
            {
                return false;
            }
        }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        
        public string PluginId { get { return this.GetInputParamValue(nameof(PluginId)); } }

        public string ServiceId { get { return this.GetInputParamValue(nameof(ServiceId)); } }

        public string GingerActionId { get { return this.GetInputParamValue(nameof(GingerActionId)); } }

        
        
        public override void Execute() { }// Execute is being performed inside Ginger Runner
    }
}
