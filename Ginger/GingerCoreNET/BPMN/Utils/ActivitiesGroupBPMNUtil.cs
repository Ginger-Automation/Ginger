using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            AttachIdentifiersToActivities(activityGroup, solutionFacade);
            IEnumerable<Activity> activities = activityGroup
                    .ActivitiesIdentifiers
                    .Select(identifier => identifier.IdentifiedActivity)
                    .Where(activity => activity != null && activity.Active);

            if (!activities.Any())
            {
                throw new BPMNConversionException($"No eligible {GingerDicser.GetTermResValue(eTermResKey.Activity)} found for creating BPMN.");
            }

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
    }
}
