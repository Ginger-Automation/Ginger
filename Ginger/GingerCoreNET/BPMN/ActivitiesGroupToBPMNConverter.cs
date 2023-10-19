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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            AttachIdentifiersToActivities(activityGroup.ActivitiesIdentifiers);
            IEnumerable<Activity> activitiesInActivityGroup = GetActivitiesFromActivityGroup(activityGroup);
            Activity? firstActivity = activitiesInActivityGroup.FirstOrDefault(activity => activity.Active);
            if (firstActivity == null)
            {
                throw new BPMNExportException($"No {GingerDicser.GetTermResValue(eTermResKey.Activity)} found, make sure all the {GingerDicser.GetTermResValue(eTermResKey.Activity)} are not in-active.");
            }

            TargetBase? targetAppForSystemRef;
            if (IsWebServicesActivity(firstActivity))
            {
                targetAppForSystemRef = GetTargetAppFromConsumer(firstActivity.ConsumerApplications.First());
            }
            else
            {
                targetAppForSystemRef = GetTargetAppFromTargetAppName(firstActivity.TargetApplication);
            }

            if(targetAppForSystemRef == null)
            {
                throw new BPMNExportException($"No suitable {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found for Collaboration System Ref.");
            }

            Collaboration collaboration = new(activityGroup.Guid, CollaborationType.SubProcess)
            {
                Name = activityGroup.Name,
                SystemRef = targetAppForSystemRef.Guid.ToString(),
                Description = activityGroup.Description
            };


            IEnumerable<TargetBase> targetApps = GetTargetAppsInActivityGroup(activityGroup);

            foreach (TargetBase targetApp in targetApps)
            {
                Participant participant = new(targetApp.Guid)
                {
                    Name = targetApp.Name,
                    SystemRef = targetApp.Guid.ToString()
                };
                collaboration.AddParticipant(participant);
            }

            IFlowSource previousFlowSource;
            
            Participant participantForStartEvent;
            if(!IsWebServicesActivity(firstActivity))
            {
                participantForStartEvent = GetParticipantForTargetAppName(collaboration, firstActivity.TargetApplication);
            }
            else
            {
                Consumer firstActivityConsumer = firstActivity.ConsumerApplications[0];
                string targetAppName = GetTargetAppFromConsumer(firstActivityConsumer).Name;
                participantForStartEvent = GetParticipantForTargetAppName(collaboration, targetAppName);
            }
            StartEvent startEvent = participantForStartEvent.Process.AddStartEvent(name: string.Empty);
            previousFlowSource = startEvent;

            foreach (Activity activity in activitiesInActivityGroup)
            {
                if(!activity.Active)
                {
                    continue;
                }

                Participant activityParticipant = GetParticipantForTargetAppName(collaboration, activity.TargetApplication);
                if(IsWebServicesActivity(activity))
                {
                    string consumerAppName = GetTargetAppFromConsumer(activity.ConsumerApplications.First()).Name;
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
                    Flow.Create(name: string.Empty, requestTargetTask, responseSourceTask);
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

        private void AttachIdentifiersToActivities(IEnumerable<ActivityIdentifiers> activityIdentifiers)
        {
            foreach (ActivityIdentifiers identifier in activityIdentifiers)
            {
                if (identifier.IdentifiedActivity == null)
                {
                    identifier.IdentifiedActivity = GetActivityFromSharedRepositoryByIdentifier(identifier);
                    if (identifier.IdentifiedActivity == null)
                    {
                        identifier.ExistInRepository = false;
                    }
                }
            }
        }

        private IEnumerable<TargetBase> GetTargetAppsInActivityGroup(ActivitiesGroup activityGroup)
        {
            IEnumerable<string> targetAppNames = activityGroup
                .ActivitiesIdentifiers
                .Where(identifier => identifier.IdentifiedActivity != null)
                .Select(identifier => identifier.IdentifiedActivity.TargetApplication)
                .Distinct();

            IEnumerable<Guid> consumerGuids = activityGroup
                .ActivitiesIdentifiers
                .Where(identifier => identifier.IdentifiedActivity != null)
                .SelectMany(identifier => identifier.IdentifiedActivity.ConsumerApplications)
                .Select(consumer => consumer.ConsumerGuid);

            IEnumerable<TargetBase> targetApps = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .Where(targetApp => targetAppNames.Contains(targetApp.Name.ToString()));

            IEnumerable<TargetBase> consumerTargetApps = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .Where(targetApp => consumerGuids.Contains(targetApp.Guid));

            return consumerTargetApps.Concat(targetApps).Distinct(new TargetBaseEqualityComparer());
        }

        private IEnumerable<Activity> GetActivitiesFromActivityGroup(ActivitiesGroup activityGroup)
        {
            return
                activityGroup
                    .ActivitiesIdentifiers
                    .Where(identifier => identifier.IdentifiedActivity != null)
                    .Select(identifier => identifier.IdentifiedActivity);
        }

        private Activity? GetActivityFromSharedRepositoryByIdentifier(ActivityIdentifiers activityIdentifier)
        {
            ObservableList<Activity> activitiesInRepository = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

            Activity? activityInRepository = activitiesInRepository
                .FirstOrDefault(activity => 
                    string.Equals(activity.ActivityName, activityIdentifier.ActivityName) && 
                    activity.Guid == activityIdentifier.ActivityGuid);

            if (activityInRepository == null)
            {
                activityInRepository = activitiesInRepository.FirstOrDefault(x => x.Guid == activityIdentifier.ActivityGuid);
            }

            if (activityInRepository == null)
            {
                activityInRepository = activitiesInRepository.FirstOrDefault(x => string.Equals(x.ActivityName, activityIdentifier.ActivityName));
            }

            return activityInRepository;
        }

        private TargetBase GetTargetAppFromConsumer(Consumer consumer)
        {
            IEnumerable<TargetBase> targetApps = WorkSpace.Instance.Solution.GetSolutionTargetApplications();
            TargetBase? consumerTargetApp = targetApps.FirstOrDefault(targetApp => string.Equals(targetApp.Guid, consumer.ConsumerGuid));
            if (consumerTargetApp == null)
            {
                throw new BPMNExportException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found for Consumer with Guid '{consumer.ConsumerGuid}'.");
            }
            return consumerTargetApp;
        }

        private TargetBase? GetTargetAppFromTargetAppName(string targetAppName)
        {
            return WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .FirstOrDefault(targetApp => string.Equals(targetApp.Name, targetAppName));
        }

        private Participant GetParticipantForTargetAppName(Collaboration collaboration, string targetAppName)
        {
            Participant? participant = collaboration.Participants.FirstOrDefault(participant => string.Equals(participant.Name, targetAppName));
            if(participant == null)
            {
                throw new BPMNExportException($"No BPMN Participant({GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}) found for {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} name '{targetAppName}'.");
            }
            return participant;
        }

        private Participant GetParticipantForProcessId(Collaboration collaboration, string processId)
        {
            Participant? participant = collaboration.Participants.FirstOrDefault(participant => string.Equals(participant.Process.Id, processId));
            if(participant == null)
            {
                throw new BPMNExportException($"No BPMN Participant({GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}) found for process id '{processId}'.");
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
                throw new BPMNExportException($"No Application Platform found for Activity with {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} '{activity.TargetApplication}'.");
            }

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }

        private class TargetBaseEqualityComparer : IEqualityComparer<TargetBase>
        {
            public bool Equals(TargetBase? x, TargetBase? y)
            {
                return
                    x == null && y == null ||
                    x != null && y != null && x.Guid == y.Guid;
            }

            public int GetHashCode([DisallowNull] TargetBase obj)
            {
                HashCode hashCode = new();
                hashCode.Add(obj.Guid);
                return hashCode.ToHashCode();
            }
        }
    }
}