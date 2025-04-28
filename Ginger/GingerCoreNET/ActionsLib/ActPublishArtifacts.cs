#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.Actions
{
    public class ActPublishArtifacts : ActWithoutDriver
    {
        public override string ActionType
        {
            get
            {
                return "Publish Artifacts";
            }
        }

        public override string ActionDescription
        {
            get
            {
                return "Publish Artifacts";
            }
        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return true; } }

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
        
        public override string ActionEditPage { get { return "ActPublishArtifactsEditPage"; } }

        public override string ActionUserDescription { get { return "Upload of Artifacts for HTML Report usage."; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action for uploading of Artifacts for HTML Report usage.");
        }

        // Keep in CoreNET

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
