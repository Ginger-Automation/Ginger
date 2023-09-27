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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    //TODO: BPMN - How to handle WithoutPlatformApps
    /// <summary>
    /// Converts <see cref="ActivitiesGroup"/> to a BPMN <see cref="Collaboration"/>.
    /// </summary>
    public sealed class ActivitiesGroupToBPMNConverter
    {
        /// <summary>
        /// Convert <see cref="ActivitiesGroup"/> to a BPMN <see cref="Collaboration"/>.
        /// </summary>
        /// <param name="activityGroup"><see cref="ActivitiesGroup"/> to convert.</param>
        /// <returns>BPMN <see cref="Collaboration"/>.</returns>
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
            TargetBase? consumerTargetApp = targetApps.FirstOrDefault(targetApp => string.Equals(targetApp.Guid, consumer.ConsumerGuid));
            if (consumerTargetApp == null)
            {
                throw new InvalidOperationException($"No Target Application found for Consumer with Guid '{consumer.ConsumerGuid}'.");
            }
            return consumerTargetApp.Name;
        }

        private Participant GetParticipantForTargetAppName(Collaboration collaboration, string targetAppName)
        {
            Participant? participant = collaboration.Participants.FirstOrDefault(participant => string.Equals(participant.Name, targetAppName));
            if(participant == null)
            {
                throw new InvalidOperationException($"No Participant found for Target Application name '{targetAppName}'.");
            }
            return participant;
        }

        private Participant GetParticipantForProcessId(Collaboration collaboration, string processId)
        {
            Participant? participant = collaboration.Participants.FirstOrDefault(participant => string.Equals(participant.Process.Id, processId));
            if(participant == null)
            {
                throw new InvalidOperationException($"No Participant found for process id '{processId}'.");
            }
            return participant;
        }

        private bool IsWebServicesActivity(Activity activity)
        {
            ApplicationPlatform? activityAppPlatform = WorkSpace.Instance.Solution
                .ApplicationPlatforms
                .FirstOrDefault(platform => string.Equals(platform.AppName, activity.TargetApplication));
            if(activityAppPlatform == null)
            {
                throw new InvalidOperationException($"No Application Platform found for Activity with Target Application '{activity.TargetApplication}'.");
            }

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }
    }
}