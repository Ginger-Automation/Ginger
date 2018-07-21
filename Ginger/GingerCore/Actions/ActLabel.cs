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

namespace GingerCore.Actions
{
    public class ActLabel : Act
    {
        public override string ActionDescription { get { return "Label Action"; } }
        public override string ActionUserDescription { get { return "Set a label object"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to automate a set property on an object from type label.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select property type of label from Locate By drop down and then enter label property value and then the url and run the action.");
        }        

        public override string ActionEditPage { get { return null; } }
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

        public enum eLabelAction
        {
            IsVisible = 1,
            GetInnerText=2,
            GetWidth = 22,
            GetHeight = 23,
            GetStyle = 24,
        }

        [IsSerializedForLocalRepository]
        public eLabelAction LabelAction{get;set;}

        public override String ActionType
        {
            get
            {
                return "LabelName";
            }
        }

        public override System.Drawing.Image Image { get { return Resources.Act; } }
    }
}
