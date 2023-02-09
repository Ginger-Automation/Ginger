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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions.Android
{
    public class ActShell : Act
    {
        public override string ActionDescription { get { return "Shell Action"; } }
        public override string ActionUserDescription { get { return "Run Shell Command"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to run Shell action like: ps, ls etc.");                                        
        }       

        public override string ActionEditPage { get { return null; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    //mPlatforms.Add(ePlatformType.AndroidDevice);                    
                }
                return mPlatforms;
            }
        }
      
        public override String ActionType 
        { 
            get
            {
                return "Shell";
            }
        }

        public override eImageType Image { get { return eImageType.Linux; } }
    }
}
