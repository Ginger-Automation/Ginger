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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using static Ginger.Reports.ExecutionLoggerConfiguration;

namespace GingerCore.Actions
{
    public class ActPublishArtifacts : ActWithoutDriver
    {
        private const long MaxUploadSizeBytes = 5242879;  // 5MB = 5242880 bytes, but somewhere the calculation is referring it as 5242879 byte-s

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

        public override string ActionUserDescription { get { return "This action uploads artifacts at the end of execution, along with other artifacts to be published."; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to upload artifacts for centralized HTML report usage, artifacts e.g. Reports generated outside ginger.");
        }

        public override void Execute()
        {
            try
            {
                if (ActInputValues == null || ActInputValues.Count == 0)
                {
                    Error = "No artifact files provided.";
                    return;
                }

                if (!WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationIsEnabled &&
                    WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ePublishToCentralDB.No)
                {
                    Error = "Please Enable Local/Centralized Execution Logger Settings from \"Configurations => Reports => Execution Logger Configurations\" to execute Publish Artifacts actions.";
                    return;
                }

                if (WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ePublishToCentralDB.Yes && GingerPlayUtils.IsGingerPlayConfigured() &&
                    WorkSpace.Instance.Solution.LoggerConfigurations.UploadArtifactsToCentralizedReport == eUploadExecutionArtifactsToCentralizedReport.No)
                {
                    Error = "Please Enable Upload execution artifacts in \"CONFIGURATIONS => Reports => Execution Logger Configurations => Centralized Execution Logger Settings\" to execute Publish Artifacts actions.";
                    return;
                }

                foreach (ActInputValue item in ActInputValues)
                {
                    if (!System.IO.File.Exists(item.ValueForDriver))
                    {
                        Error += $"Artifact File Path is invalid/doesn't exist/not enough permissions to access file: {item.ValueForDriver}" + Environment.NewLine;
                        continue;
                    }

                    Act.AddArtifactToAction(Path.GetFileName(item.ValueForDriver), this, item.ValueForDriver);
                }
            }
            catch (Exception ex)
            {
                Error += $"Failed to upload artifacts: {ex.Message}" + Environment.NewLine;
                return;
            }
        }
    }
}