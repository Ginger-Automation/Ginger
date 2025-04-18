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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Amdocs.Ginger.CoreNET.BPMN.Conversion
{
    /// <summary>
    /// A facade to expose solution data for BPMN related operations.
    /// </summary>
    public interface ISolutionFacadeForBPMN
    {
        /// <summary>
        /// Get all <see cref="Activity"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<Activity> GetActivitiesFromSharedRepository();

        /// <summary>
        /// Get all <see cref="ApplicationPlatform"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<ApplicationPlatform> GetApplicationPlatforms();

        /// <summary>
        /// Get all <see cref="TargetBase"/> from SharedRepository.
        /// </summary>
        /// <returns></returns>
        public ObservableList<TargetBase> GetTargetApplications();
    }
}
