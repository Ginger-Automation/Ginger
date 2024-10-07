#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using GingerCore;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Utils
{
    internal static class ActivityBPMNUtil
    {
        internal static IEnumerable<FlowControl> GetFlowControls(Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException(paramName: nameof(activity));
            }

            List<FlowControl> flowControls = [];

            if (activity.Acts == null)
            {
                return flowControls;
            }

            foreach (IAct action in activity.Acts)
            {
                if (action == null || action.FlowControls == null)
                {
                    continue;
                }

                flowControls.AddRange(action.FlowControls);
            }

            return flowControls;
        }

        /// <summary>
        /// Checks whether the given <paramref name="activity"/> is of WebServices platform or not.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to check the platform of.</param>
        /// <returns><see langword="true"/> if the given <paramref name="activity"/> is of WebServices platform, <see langword="false"/> otherwise.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="ApplicationPlatform"/> is found for the given <paramref name="activity"/> target application name.</exception>
        internal static bool IsWebServicesActivity(Activity activity, ISolutionFacadeForBPMN solutionFacade)
        {
            IEnumerable<ApplicationPlatform> applicationPlatforms = solutionFacade.GetApplicationPlatforms();
            ApplicationPlatform? activityAppPlatform = applicationPlatforms.FirstOrDefault(platform => string.Equals(platform.AppName, activity.TargetApplication));
            if (activityAppPlatform == null)
            {
                throw new BPMNConversionException($"No Application Platform found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} with {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} '{activity.TargetApplication}'.");
            }

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }

        /// <summary>
        /// Get the first <see cref="Consumer"/> for the given <paramref name="activity"/>.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to get the Consumer of.</param>
        /// <returns>First <see cref="Consumer"/> for the given <paramref name="activity"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Consumer"/> is found for the given <paramref name="activity"/>.</exception>
        internal static Consumer GetActivityFirstConsumer(Activity activity)
        {
            Consumer? firstConsumer = activity.ConsumerApplications.FirstOrDefault();

            if (firstConsumer == null)
            {
                throw new BPMNConversionException($"No Consumer found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{activity.ActivityName}'");
            }

            return firstConsumer;
        }
    }
}
