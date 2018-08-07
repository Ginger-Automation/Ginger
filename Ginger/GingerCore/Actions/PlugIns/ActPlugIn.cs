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
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerPlugIns.ActionsLib;
using System.Collections.Generic;

namespace GingerCore.Actions.PlugIns
{
    public class ActPlugIn : ActWithoutDriver
    {
        public new static class Fields
        {
            public static string PluginID = "PluginID";
            public static string PlugInName = "PlugInName";
            public static string PlugInActionID = "PlugInActionID";
            public static string PluginDescription = "PluginDescription";
            public static string PluginUserDescription = "PluginUserDescription";
            public static string PluginUserRecommendedUseCase = "PluginUserRecommendedUseCase";
        }

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
                // return this.GetInputParamValue(Fields.PluginDescription); 
                return "aaaaaa";  //TODO: Fix me
            }
        }

        public override string ActionEditPage { get { return "PlugIns.ActPlugInEditPage"; } }  

        public override string ActionUserDescription
        {
            get
            {
                // return this.GetInputParamValue(Fields.PluginUserDescription);
                return "zzzzz";   //TODO: Fix me
            }
        }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            // TBH.AddText(this.GetInputParamValue(Fields.PluginUserRecommendedUseCase)); 
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

        public string PlugInName { get; set; }

        public string PlugInID { get { return this.GetInputParamValue(Fields.PluginID); } }

        public string PlugInActionID { get { return this.GetInputParamValue(Fields.PlugInActionID); } }

        private GingerAction mGingerAction = null;
        public GingerAction GingerAction
        {
            get
            {
                if (mGingerAction == null)
                {
                    mGingerAction = new GingerAction();
                    mGingerAction.ID = PlugInActionID;
                    mGingerAction.SolutionFolder = SolutionFolder;
                    foreach (ActInputValue AIV in this.InputValues)
                    {
                        ActionParam AP = mGingerAction.GetOrCreateParam(AIV.Param);
                        AP.Value = AIV.Value;
                    }
                }
                return mGingerAction;
            }
        }
        
        public override void Execute() { }// Execute is being performed inside Ginger Runner
    }
}
