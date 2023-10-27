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
        private readonly ActivitiesGroup _activityGroup;

        public ActivitiesGroupToBPMNConverter(ActivitiesGroup activityGroup)
        {
            _activityGroup = activityGroup;
        }

        /// <summary>
        /// Convert <see cref="ActivitiesGroup"/> to a BPMN <see cref="Collaboration"/>.
        /// </summary>
        /// <returns>BPMN <see cref="Collaboration"/>.</returns>
        public Collaboration Convert()
        {
            AttachIdentifiersToActivities();

            Collaboration collaboration = CreateCollaboration();

            IEnumerable<Activity> activities = GetActivities();

            Activity firstActivity = activities.First();
            IFlowSource previousFlowSource = AddStartEventForActivity(collaboration, firstActivity);

            foreach (Activity activity in activities)
            {
                previousFlowSource = AddTasksForActivity(collaboration, activity, previousFlowSource);
            }

            Activity lastActivity = activities.Last();
            AddEndEventForActivity(collaboration, lastActivity, previousFlowSource);

            return collaboration;
        }

        private void AttachIdentifiersToActivities()
        {
            foreach (ActivityIdentifiers identifier in _activityGroup.ActivitiesIdentifiers)
            {
                identifier.IdentifiedActivity = GetActivityFromSharedRepositoryByIdentifier(identifier);

                if (identifier.IdentifiedActivity == null)
                {
                    identifier.ExistInRepository = false;
                }
            }
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

        private bool IsWebServicesActivity(Activity activity)
        {
            ApplicationPlatform? activityAppPlatform = WorkSpace.Instance.Solution
                .ApplicationPlatforms
                .FirstOrDefault(platform => string.Equals(platform.AppName, activity.TargetApplication));
            if (activityAppPlatform == null)
            {
                throw new BPMNExportException($"No Application Platform found for Activity with {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} '{activity.TargetApplication}'.");
            }

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }

        private IEnumerable<Activity> GetActivities()
        {
            IEnumerable<Activity> activities = _activityGroup
                    .ActivitiesIdentifiers
                    .Select(identifier => identifier.IdentifiedActivity)
                    .Where(activity => activity != null && activity.Active);

            if(!activities.Any())
            {
                throw new BPMNExportException($"No valid {GingerDicser.GetTermResValue(eTermResKey.Activity)} found for creating BPMN.");
            }

            return activities;
        }

        private Collaboration CreateCollaboration()
        {
            IEnumerable<Participant> participants = CreateParticipants();

            Participant firstParticipant = participants.First();

            Collaboration collaboration = new(_activityGroup.Guid, CollaborationType.SubProcess)
            {
                Name = _activityGroup.Name,
                SystemRef = firstParticipant.Guid,
                Description = _activityGroup.Description
            };

            foreach (Participant participant in participants)
            {
                collaboration.AddParticipant(participant);
            }

            return collaboration;
        }

        private IEnumerable<Participant> CreateParticipants()
        {
            List<Participant> participants = new();
            IEnumerable<Activity> activities = GetActivities();
            foreach (Activity activity in activities)
            {
                participants.AddRange(CreateParticipantsForActivity(activity));
            }

            if(!participants.Any())
            {
                throw new BPMNExportException($"No BPMN Participants (Ginger {GingerDicser.GetTermResValue(eTermResKey.Activity)}) found for creating BPMN");
            }

            return participants;
        }

        private IEnumerable<Participant> CreateParticipantsForActivity(Activity activity)
        {
            List<Participant> participants = new();

            if (IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityConsumer(activity);
                TargetBase consumerTargetApp = GetTargetApplicationByGuid(consumer.ConsumerGuid);
                Participant participantForConsumer = new(consumerTargetApp.Guid)
                {
                    Name = consumerTargetApp.Name,
                    SystemRef = consumerTargetApp.Guid.ToString()
                };
                participants.Add(participantForConsumer);
            }

            TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);
            Participant participantForTargetApp = new(targetApp.Guid)
            {
                Name = targetApp.Name,
                SystemRef = targetApp.Guid.ToString()
            };
            participants.Add(participantForTargetApp);

            return participants;
        }

        private TargetBase GetTargetApplicationByName(string targetAppName)
        {
            TargetBase? targetApp = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .FirstOrDefault(targetApp => string.Equals(targetApp.Name, targetAppName));

            if (targetApp == null)
            {
                throw new BPMNExportException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found with name '{targetAppName}'");
            }

            return targetApp;
        }

        private TargetBase GetTargetApplicationByGuid(Guid targetAppGuid)
        {
            TargetBase? targetApp = WorkSpace.Instance.Solution
                .GetSolutionTargetApplications()
                .FirstOrDefault(targetApp => targetApp.Guid == targetAppGuid);

            if (targetApp == null)
            {
                throw new BPMNExportException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found with Guid '{targetAppGuid}'");
            }

            return targetApp;
        }

        private Consumer GetActivityConsumer(Activity activity)
        {
            Consumer? firstConsumer = activity.ConsumerApplications.FirstOrDefault();

            if (firstConsumer == null)
            {
                throw new BPMNExportException($"No Consumer found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{activity.ActivityName}'");
            }

            return firstConsumer;
        }

        private IFlowSource AddTasksForActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            if (IsWebServicesActivity(activity))
            {
                IEnumerable<Task> tasks = AddTaskForWebServicesActivity(collaboration, activity);

                Task firstTask = tasks.First();
                Flow.Create(name: string.Empty, previousFlowSource, firstTask);

                Task lastTask = tasks.Last();
                previousFlowSource = lastTask;
            }
            else
            {
                Task task = AddTaskForUIActivity(collaboration, activity);

                Flow.Create(name: string.Empty, previousFlowSource, task);
                previousFlowSource = task;
            }

            return previousFlowSource;
        }

        private Participant GetParticipantByGuid(Collaboration collaboration, Guid participantGuid)
        {
            Participant? participant = collaboration
                .Participants
                .FirstOrDefault(participant => string.Equals(participant.Guid, participantGuid.ToString()));

            if (participant == null)
            {
                throw new BPMNExportException($"No BPMN Participant (Ginger {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found by Guid '{participantGuid}'");
            }

            return participant;
        }

        private IEnumerable<Task> AddTaskForWebServicesActivity(Collaboration collaboration, Activity activity)
        {
            Consumer consumer = GetActivityConsumer(activity);
            TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);

            Participant consumerParticipant = GetParticipantByGuid(collaboration, consumer.ConsumerGuid);
            Participant targetAppParticipant = GetParticipantByGuid(collaboration, targetApp.Guid);

            Task requestSourceTask = consumerParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_RequestSource");
            Task requestTargetTask = targetAppParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_RequestTarget");
            Task responseSourceTask = targetAppParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_ResponseSource");
            Task responseTargetTask = consumerParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_ResponseTarget");

            Flow requestFlow = Flow.Create(name: $"{activity.ActivityName}_IN", requestSourceTask, requestTargetTask);

            if (requestFlow is MessageFlow requestMessageFlow)
            {
                string activityGuid = activity.Guid.ToString();
                string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "aa");
                requestMessageFlow.MessageRef = messageRef;
            }

            Flow.Create(name: string.Empty, requestTargetTask, responseSourceTask);

            Flow responseFlow = Flow.Create(name: $"{activity.ActivityName}_OUT", responseSourceTask, responseTargetTask);

            if (responseFlow is MessageFlow responseMessageFlow)
            {
                string activityGuid = activity.Guid.ToString();
                string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "bb");
                responseMessageFlow.MessageRef = messageRef;
            }

            List<Task> tasks = new()
            {
                requestSourceTask,
                requestTargetTask,
                responseSourceTask,
                responseTargetTask
            };

            return tasks;
        }

        private UserTask AddTaskForUIActivity(Collaboration collaboration, Activity activity)
        {
            TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);
            Participant participant = GetParticipantByGuid(collaboration, targetApp.Guid);

            UserTask userTask = participant.Process.AddTask<UserTask>(guid: activity.Guid, name: activity.ActivityName);

            string activityGuid = activity.Guid.ToString();
            string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "aa");
            userTask.MessageRef = messageRef;

            return userTask;
        }

        private StartEvent AddStartEventForActivity(Collaboration collaboration, Activity activity)
        {
            Participant participant;
            if (IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityConsumer(activity);
                participant = GetParticipantByGuid(collaboration, consumer.ConsumerGuid);
            }
            else
            {
                TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);
                participant = GetParticipantByGuid(collaboration, targetApp.Guid);
            }

            StartEvent startEvent = participant.Process.AddStartEvent(name: string.Empty);

            return startEvent;
        }

        private EndEvent AddEndEventForActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            Participant participant;
            if(IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityConsumer(activity);
                participant = GetParticipantByGuid(collaboration, consumer.ConsumerGuid);
            }
            else
            {
                TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);
                participant = GetParticipantByGuid(collaboration, targetApp.Guid);
            }

            EndEvent endEvent = participant.Process.AddEndEvent(name: string.Empty);
            Flow.Create(name: string.Empty, previousFlowSource, endEvent);

            return endEvent;
        }
    }
}