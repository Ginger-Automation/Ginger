using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Repository;
using Applitools;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.Graph;
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
            IEnumerable<TargetBase> targetApps = GetTargetAppsInActivityGroup(activityGroup);

            Collaboration collaboration = new(activityGroup.Guid, CollaborationType.SubProcess)
            {
                Name = activityGroup.Name,
                SystemRef = targetApps.First().Guid.ToString(),
                Description = activityGroup.Description
            };

            foreach (TargetBase targetApp in targetApps)
            {
                Participant participant = new(targetApp.Guid)
                {
                    Name = targetApp.Name,
                    SystemRef = targetApp.Guid.ToString()
                };
                collaboration.AddParticipant(participant);
            }

            IEnumerable<Activity> activitiesInActivityGroup = GetActivitiesFromActivityGroup(activityGroup);

            IFlowSource previousFlowSource;

            Activity firstActivity = activitiesInActivityGroup.First();
            Participant firstActivityParticipant = GetParticipantForTargetAppName(collaboration, firstActivity.TargetApplication);
            StartEvent startEvent = firstActivityParticipant.Process.AddStartEvent(name: string.Empty);
            previousFlowSource = startEvent;

            foreach (Activity activity in activitiesInActivityGroup)
            {
                Participant activityParticipant = GetParticipantForTargetAppName(collaboration, activity.TargetApplication);
                if(IsWebServicesActivity(activity))
                {
                    string consumerAppName = GetTargetAppNameFromConsumerId(activity.ConsumerApplications.First());
                    Participant consumerParticipant = GetParticipantForTargetAppName(collaboration, consumerAppName);
                    Task requestSourceTask = consumerParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_RequestSource");
                    Task requestTargetTask = activityParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_RequestTarget");
                    Task responseSourceTask = activityParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_ResponseSource");
                    Task responseTargetTask = consumerParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_ResponseTarget");
                    Flow.Create(name: string.Empty, previousFlowSource, requestSourceTask);
                    Flow requestFlow = Flow.Create(name: $"{activity.ActivityName}_IN", requestSourceTask, requestTargetTask);
                    if(requestFlow is MessageFlow requestMessageFlow)
                    {
                        requestMessageFlow.MessageRef = activity.Guid.ToString().Remove(activity.Guid.ToString().Length - 2) + "aa";
                    }
                    Flow responseFlow = Flow.Create(name: $"{activity.ActivityName}_OUT", responseSourceTask, responseTargetTask);
                    if(responseFlow is MessageFlow responseMessageFlow)
                    {
                        responseMessageFlow.MessageRef = activity.Guid.ToString().Remove(activity.Guid.ToString().Length - 2) + "bb";
                    }
                    previousFlowSource = responseTargetTask;
                }
                else
                {
                    UserTask userTask = activityParticipant.Process.AddTask<UserTask>(guid: activity.Guid, name: activity.ActivityName);
                    userTask.MessageRef = activity.Guid.ToString().Remove(activity.Guid.ToString().Length - 2) + "aa";
                    Flow.Create(name: string.Empty, previousFlowSource, userTask);
                    previousFlowSource = userTask;
                }
            }

            Participant lastTaskParticipant = GetParticipantForProcessId(collaboration, previousFlowSource.ProcessId);
            EndEvent endEvent = lastTaskParticipant.Process.AddEndEvent(name: string.Empty, EndEventType.Termination);
            Flow.Create(name: string.Empty, previousFlowSource, endEvent);

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

        private string GetTargetAppNameFromConsumerId(Consumer consumer)
        {
            IEnumerable<TargetBase> targetApps = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            TargetBase consumerTargetApp = targetApps.First(targetApp => string.Equals(targetApp.Guid, consumer.ConsumerGuid));
            return consumerTargetApp.Name;
        }

        private Participant GetParticipantForTargetAppName(Collaboration collaboration, string targetAppName)
        {
            return collaboration.Participants.First(participant => string.Equals(participant.Name, targetAppName));
        }

        private Participant GetParticipantForProcessId(Collaboration collaboration, string processId)
        {
            return collaboration.Participants.First(participant => string.Equals(participant.Process.Id, processId));
        }

        private bool IsWebServicesActivity(Activity activity)
        {
            ApplicationPlatform activityAppPlatform = WorkSpace.Instance.Solution
                .ApplicationPlatforms
                .First(platform => string.Equals(platform.AppName, activity.TargetApplication));

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }
    }
}

/*
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
 
 */