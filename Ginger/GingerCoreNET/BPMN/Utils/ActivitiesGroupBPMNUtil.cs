#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using GingerCore;
using GingerCore.Activities;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Utils
{
    internal static class ActivitiesGroupBPMNUtil
    {
        /// <summary>
        /// Get list of <see cref="Activity"/> from <see cref="ActivitiesGroup"/> which are eligible for conversion.
        /// </summary>
        /// <returns>List of <see cref="Activity"/>.</returns>
        /// <exception cref="BPMNConversionException">If <see cref="ActivitiesGroup"/> is empty or no <see cref="Activity"/> is eligible for conversion.</exception>
        internal static IEnumerable<Activity> GetActivities(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            IEnumerable<Activity> activities = GetActivitiesOrEmpty(activityGroup, solutionFacade);

            if (!activities.Any())
            {
                throw NoValidActivityFoundInGroupException.WithDefaultMessage(activityGroup.Name);
            }

            return activities;
        }

        private static IEnumerable<Activity> GetActivitiesOrEmpty(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            AttachIdentifiersToActivities(activityGroup, solutionFacade);
            IEnumerable<Activity> activities = activityGroup
                    .ActivitiesIdentifiers
                    .Select(identifier => identifier.IdentifiedActivity)
                    .Where(activity => activity != null && activity.Active);
            return activities;
        }

        /// <summary>
        /// Attach ActivityGroup's ActivityIdentifiers to their relevant Activities from SharedRepository
        /// </summary>
        private static void AttachIdentifiersToActivities(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            foreach (ActivityIdentifiers identifier in activityGroup.ActivitiesIdentifiers)
            {
                identifier.IdentifiedActivity = GetActivityFromSharedRepositoryByIdentifier(identifier, solutionFacade);

                if (identifier.IdentifiedActivity == null)
                {
                    identifier.ExistInRepository = false;
                }
            }
        }

        /// <summary>
        /// Get <see cref="Activity"/> from SharedRepository based on <see cref="ActivityIdentifiers"/>.
        /// </summary>
        /// <param name="activityIdentifier"></param>
        /// <returns></returns>
        private static Activity? GetActivityFromSharedRepositoryByIdentifier(ActivityIdentifiers activityIdentifier, ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<Activity> activitiesInRepository = solutionFacade.GetActivitiesFromSharedRepository();

            Activity? activityInRepository = activitiesInRepository
                .FirstOrDefault(activity =>
                    activity.Guid == activityIdentifier.ActivityGuid &&
                    string.Equals(activity.ActivityName, activityIdentifier.ActivityName));

            if (activityInRepository == null)
            {
                activityInRepository = activitiesInRepository
                    .FirstOrDefault(x =>
                        x.Guid == activityIdentifier.ActivityGuid);
            }

            if (activityInRepository == null)
            {
                activityInRepository = activitiesInRepository
                    .FirstOrDefault(x =>
                        string.Equals(x.ActivityName, activityIdentifier.ActivityName));
            }

            return activityInRepository;
        }

        internal static bool IsActive(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            return GetActivitiesOrEmpty(activityGroup, solutionFacade).All(activity => activity.Active);
        }
    }
}
