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
using System.ComponentModel;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    public class ActSetConfig : Act,  INotifyPropertyChanged
    {
        public override string ActionDescription { get { return "Set Config Action"; } }
        public override string ActionUserDescription { get { return "Set config"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to automate a config on an object.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action, select property type from Locate By drop down and then enter property value in Locate Value textbox and then the url and run the action.");
        }        

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }
        public override bool IsSelectableAction { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    // Since, the action isn't supported by Windows && Web Platform hence, it's commented
                    mPlatforms.Add(ePlatformType.Mobile);
                }
                return mPlatforms;
            }
        }

        public override String ActionType
        {
            get
            {
                return "Set Config";
            }
        }

        public string sValue
        {
            get
            {
                return GetOrCreateInputParam(nameof(sValue)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(sValue), value);
            }
        }

        public string sParam
        {
            get
            {
                return GetOrCreateInputParam(nameof(sParam)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(sParam), value);
            }
        }
    }
}