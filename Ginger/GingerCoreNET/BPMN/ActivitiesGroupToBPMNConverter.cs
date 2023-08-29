using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Repository;
using Applitools;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    //TODO: BPMN - How to handle WithoutPlatformApps
    public sealed class ActivitiesGroupToBPMNConverter
    {
        public Collaboration Convert(ActivitiesGroup activityGroup)
        {
            Collaboration collaboration = new(
                guid: activityGroup.Guid,
                CollaborationType.SubProcess,
                name: activityGroup.Name,
                description: activityGroup.Description);

            IEnumerable<TargetBase> targetApps = GetTargetAppsInActivityGroup(activityGroup);
            foreach (TargetBase targetApp in targetApps)
            {
                Participant participant = new(targetApp.Guid, targetApp.Name);
                collaboration.AddParticipant(participant);
            }

            IEnumerable<Activity> activitiesInActivityGroup = GetActivitiesFromActivityGroup(activityGroup);

            IFlowSource? previousFlowSource = null;

            Activity? firstActivity = activitiesInActivityGroup.FirstOrDefault();
            if(firstActivity != null)
            {
                Participant participant = GetParticipantForTargetAppName(collaboration, firstActivity.TargetApplication);
                previousFlowSource = participant.Process.AddStartEvent(name: string.Empty);
            }

            foreach (Activity activity in activitiesInActivityGroup)
            {
                Participant participant = GetParticipantForTargetAppName(collaboration, activity.TargetApplication);

                Task task;
                if (IsWebActivity(activity))
                {
                    task = participant.Process.AddUserTask(new Process.AddTaskArguments(activity.Guid, activity.ActivityName));
                }
                else
                {
                    task = participant.Process.AddTask(new Process.AddTaskArguments(activity.Guid, activity.ActivityName));
                }

                if (previousFlowSource != null)
                {
                    Flow.Create(name: string.Empty, previousFlowSource, task);
                }

                previousFlowSource = task;
            }

            Activity? lastActivity = activitiesInActivityGroup.LastOrDefault();
            if(lastActivity != null)
            {
                Participant participant = GetParticipantForTargetAppName(collaboration, lastActivity.TargetApplication);
                EndEvent endEvent = participant.Process.AddEndEvent(name: string.Empty);

                if(previousFlowSource != null)
                {
                    Flow.Create(name: string.Empty, previousFlowSource, endEvent);
                }
            }

            return collaboration;
        }

        private IEnumerable<TargetBase> GetTargetAppsInActivityGroup(ActivitiesGroup activityGroup)
        {
            IEnumerable<string> targetAppNames = activityGroup
                .ActivitiesIdentifiers
                .Select(identifier => identifier.IdentifiedActivity.TargetApplication)
                .Distinct();

            IEnumerable<TargetBase> targetApps = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .Where(targetApp => targetAppNames.Contains(targetApp.Name.ToString()));

            return targetApps;
        }

        private IEnumerable<Activity> GetActivitiesFromActivityGroup(ActivitiesGroup activityGroup)
        {
            return 
                activityGroup
                    .ActivitiesIdentifiers
                    .Select(identifier => identifier.IdentifiedActivity);
        }

        private Participant GetParticipantForTargetAppName(Collaboration collaboration, string targetAppName)
        {
            return collaboration.Participants.First(participant => string.Equals(participant.Name, targetAppName));
        }

        private bool IsWebActivity(Activity activity)
        {
            ApplicationPlatform activityAppPlatform = WorkSpace.Instance.Solution
                .ApplicationPlatforms
                .First(platform => string.Equals(platform.AppName, activity.TargetApplication));

            return activityAppPlatform.Platform == ePlatformType.Web;
        }
    }
}

/*
        public Collaboration Convert(ActivitiesGroup activityGroup)
        {
            Collaboration collaboration = new(
                guid: activityGroup.Guid, 
                CollaborationType.SubProcess, 
                name: activityGroup.Name, 
                description: activityGroup.Description);

            IEnumerable<TargetBase> targetApps = GetTargetApps(activityGroup);

            foreach(TargetBase targetApp in targetApps)
            {
                IEnumerable<Activity> targetAppActivities = GetActivitiesForTargetApp(activityGroup, targetApp);
                Participant participant = CreateParticipant(targetApp, targetAppActivities);
                collaboration.AddParticipant(participant);
            }

            Activity? firstActivity = GetFirstActivity(activityGroup);
            if (firstActivity != null)
            {
                Participant? participant = GetParticipantForActivity(collaboration.Participants, firstActivity);
                participant?.Process.AddStartEvent(name: "");
            }

            Activity? lastActivity = GetLastActivity(activityGroup);
            if(lastActivity != null)
            {
                Participant? participant = GetParticipantForActivity(collaboration.Participants, lastActivity);
                participant?.Process.AddEndEvent(name: "");
            }

            return collaboration;
        }

        private IEnumerable<TargetBase> GetTargetApps(ActivitiesGroup activityGroup)
        {
            IEnumerable<string> targetAppNames = activityGroup
                .ActivitiesIdentifiers
                .Select(identifier => identifier.IdentifiedActivity.TargetApplication);

            IEnumerable<TargetBase> targetApps = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .Where(targetApp => targetAppNames.Contains(targetApp.Name.ToString()));

            return targetApps;
        }

        private IEnumerable<Activity> GetActivitiesForTargetApp(ActivitiesGroup activityGroup, TargetBase targetApp)
        {
            string targetAppName = targetApp.Name;
            return activityGroup
                .ActivitiesIdentifiers
                .Select(identifier => identifier.IdentifiedActivity)
                .Where(activity => string.Equals(targetAppName, activity.TargetApplication));
        }

        private Participant CreateParticipant(TargetBase targetApp, IEnumerable<Activity> activities)
        {
            Participant participant = new(targetApp.Guid, targetApp.Name);
            foreach(Activity activity in activities)
            {
                participant.Process.AddTask(new Process.AddTaskArguments(activity.Guid, activity.ActivityName));
            }
            return participant;
        }

        private Activity? GetFirstActivity(ActivitiesGroup activityGroup)
        {
            ActivityIdentifiers? firstIdentifier = activityGroup.ActivitiesIdentifiers.FirstOrDefault();
            return firstIdentifier?.IdentifiedActivity;
        }

        private Activity? GetLastActivity(ActivitiesGroup activityGroup)
        {
            ActivityIdentifiers? lastIdentifier = activityGroup.ActivitiesIdentifiers.LastOrDefault();
            return lastIdentifier?.IdentifiedActivity;
        }

        private Participant? GetParticipantForActivity(IEnumerable<Participant> participants, Activity activity)
        {
            return participants.FirstOrDefault(participant => string.Equals(participant.Name, activity.TargetApplication));
        }
 */