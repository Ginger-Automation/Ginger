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
namespace GingerCore.Actions.Common
{
    // this action is for device Media actions like: Record Audio, Video Play and more
    public class ActMedia : Act
    {
        public override string ActionDescription { get { return "Media Action"; } }
        public override string ActionUserDescription { get { return "Media Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need Media actions like: Record Audio, Video Play...");
        }

        public override string ActionEditPage { get { return "_Common.ActMediaEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static String MediaAction = "MediaAction";            
        }

        public enum eMediaAction
        {
            RecordAudio,      
            RecordVideo,
            
        }

        [IsSerializedForLocalRepository]
        public eMediaAction MediaAction { get; set; }
        
        public override String ActionType
        {
            get
            {
                return "MediaAction. " + MediaAction;
            }
        }

        //TODO: put icon of Media
        public override System.Drawing.Image Image { get { return Resources.MobileDevice_16x16; } }
    }
}