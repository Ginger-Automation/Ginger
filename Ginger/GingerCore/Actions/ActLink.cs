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
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    //This class is for UI link elemnet
    public class ActLink : Act
    {
        public override string ActionDescription { get { return "Link Action"; } }
        public override string ActionUserDescription { get { return "Click on a link object"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to automate a click on an object from type Link."
                                        + Environment.NewLine + Environment.NewLine +
                                           "For Mobile use this action only in case running the flow on the native browser.");
        }        

        public override string ActionEditPage { get { return "ActLinkEditPage"; } }
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

        public enum eLinkAction
        {
            Click = 1,
            Hover= 2, //This is needed for hovering to expand menus.
            GetValue=3, //for validation
            Visible=4, //for validation
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        [IsSerializedForLocalRepository]        
        public eLinkAction LinkAction { get; set; }

        public override String ToString()
        {            
                return "Link: " + GetInputParamValue("Value");            
        }

        public override String ActionType
        {
            get
            {
                return "Link: " + LinkAction.ToString();
            }
        }

        public override System.Drawing.Image Image { get { return Resources.ActLink; } } 
    }
}
