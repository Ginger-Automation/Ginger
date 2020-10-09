#region License
/*
Copyright © 2014-2020 European Support Limited

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

using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace GingerCore.Actions
{
    // This class is for UI link element
    public class ActLogAction : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Log Action"; } }
        public override string ActionUserDescription { get { return "Log Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("This is Log action which you can use to log any value as per configuration.");
        }

        public static partial class Fields
        {
            public static string SelectedLogType = "SelectedLogType";
            public static string LogText = "LogText";
        }

        public override string ActionEditPage { get { return "ActLogActionPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }
        
        public string LogText
        {
            get
            {
                return GetInputParamValue("LogText");
            }
            set
            {
                AddOrUpdateInputParamValue("LogText", value);
                OnPropertyChanged(nameof(LogText));
            }
        }

        [IsSerializedForLocalRepository]
        public eLogLevel SelectedLogLevel { get; set; }

        public eLogLevel LogTypes { get; set; }

        // return the list of platforms this action is supported on
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

        public override String ActionType
        {
            get
            {
                return "ActLogAction";
            }
        }

        public override eImageType Image { get { return eImageType.Empty; } }

        public override void Execute()
        {            
            Reporter.ToLog(SelectedLogLevel, GetInputParamCalculatedValue("LogText"));            
        }
    }
}
