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
        private readonly ISolutionFacadeForBPMN _solutionFacade;

        public ActivitiesGroupToBPMNConverter(ActivitiesGroup activityGroup) : this(activityGroup, new WorkSpaceToSolutionFacadeAdapter(WorkSpace.Instance)) { }

        /// <summary>
        /// Create a new <see cref="ActivitiesGroupToBPMNConverter"/>.
        /// </summary>
        /// <param name="activityGroup"><see cref="ActivitiesGroup"/> to be converted.</param>
        /// <param name="solutionFacade">A facade to expose solution data.</param>
        public ActivitiesGroupToBPMNConverter(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            ValidateConstructorArgs(activityGroup, solutionFacade);
            _activityGroup = activityGroup;
            _solutionFacade = solutionFacade;
        }

        /// <summary>
        /// Validate constructor arguments before assigning them to member variables.
        /// </summary>
        /// <param name="activityGroup">Constrcutor argument</param>
        /// <param name="solutionFacade">Constructor argument</param>
        /// <exception cref="ArgumentNullException">If <paramref name="activityGroup"/> or <paramref name="solutionFacade"/> is null.</exception>
        private void ValidateConstructorArgs(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade)
        {
            if(activityGroup == null)
            {
                throw new ArgumentNullException(nameof(activityGroup));
            }
            if(solutionFacade == null)
            {
                throw new ArgumentNullException(nameof(solutionFacade));
            }
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

        /// <summary>
        /// Attach ActivityGroup's ActivityIdentifiers to their relevant Activities from SharedRepository
        /// </summary>
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
            ObservableList<Activity> activitiesInRepository = _solutionFacade.GetActivitiesFromSharedRepository();

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

        /// <summary>
        /// Checks whether the given <paramref name="activity"/> is of WebServices platform or not.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to check the platform of.</param>
        /// <returns><see langword="true"/> if the given <paramref name="activity"/> is of WebServices platform, <see langword="false"/> otherwise.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="ApplicationPlatform"/> is found for the given <paramref name="activity"/> target application name.</exception>
        private bool IsWebServicesActivity(Activity activity)
        {
            IEnumerable<ApplicationPlatform> applicationPlatforms = _solutionFacade.GetApplicationPlatforms();
            ApplicationPlatform? activityAppPlatform = applicationPlatforms.FirstOrDefault(platform => string.Equals(platform.AppName, activity.TargetApplication));
            if (activityAppPlatform == null)
            {
                throw new BPMNConversionException($"No Application Platform found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} with {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} '{activity.TargetApplication}'.");
            }

            return activityAppPlatform.Platform == ePlatformType.WebServices;
        }

        /// <summary>
        /// Get list of <see cref="Activity"/> from <see cref="ActivitiesGroup"/> which are eligible for conversion.
        /// </summary>
        /// <returns>List of <see cref="Activity"/>.</returns>
        /// <exception cref="BPMNConversionException">If <see cref="ActivitiesGroup"/> is empty or no <see cref="Activity"/> is eligible for conversion.</exception>
        private IEnumerable<Activity> GetActivities()
        {
            IEnumerable<Activity> activities = _activityGroup
                    .ActivitiesIdentifiers
                    .Select(identifier => identifier.IdentifiedActivity)
                    .Where(activity => activity != null && activity.Active);

            if(!activities.Any())
            {
                throw new BPMNConversionException($"No eligible {GingerDicser.GetTermResValue(eTermResKey.Activity)} found for creating BPMN.");
            }

            return activities;
        }

        /// <summary>
        /// Create new BPMN <see cref="Collaboration"/>.
        /// </summary>
        /// <returns>Newly created BPMN <see cref="Collaboration"/>.</returns>
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

        /// <summary>
        /// Creates list of new <see cref="Participant"/> for all the involved systems in the <see cref="ActivitiesGroup"/>.
        /// </summary>
        /// <returns>List of newly created <see cref="Participant"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Participant"/> is found.</exception>
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
                throw new BPMNConversionException($"No BPMN Participants (Ginger {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}) found for creating BPMN");
            }

            return participants;
        }

        /// <summary>
        /// Creates list of new <see cref="Participant"/> for all the involved systems in given <paramref name="activity"/>.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> whose involved systems will be used for creating new <see cref="Participant"/> list.</param>
        /// <returns>List of new <see cref="Participant"/>.</returns>
        private IEnumerable<Participant> CreateParticipantsForActivity(Activity activity)
        {
            List<Participant> participants = new();

            if (IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityFirstConsumer(activity);
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

        /// <summary>
        /// Get <see cref="TargetBase"/> whose name matches the given <paramref name="targetAppName"/>.
        /// </summary>
        /// <param name="targetAppName">Name of the <see cref="TargetBase"/> to search for.</param>
        /// <returns><see cref="TargetBase"/> with name matching the given <paramref name="targetAppName"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="TargetBase"/> is found with name matching the given <paramref name="targetAppName"/>.</exception>
        private TargetBase GetTargetApplicationByName(string targetAppName)
        {
            IEnumerable<TargetBase> targetApplications = _solutionFacade.GetTargetApplications();
            TargetBase? targetApp = targetApplications.FirstOrDefault(targetApp => string.Equals(targetApp.Name, targetAppName));

            if (targetApp == null)
            {
                throw new BPMNConversionException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found with name '{targetAppName}'");
            }

            return targetApp;
        }

        /// <summary>
        /// Get <see cref="TargetBase"/> whose Guid matches the given <paramref name="targetAppGuid"/>.
        /// </summary>
        /// <param name="targetAppGuid">Guid of the <see cref="TargetBase"/> to search for.</param>
        /// <returns><see cref="TargetBase"/> with Guid matching the given <paramref name="targetAppGuid"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="TargetBase"/> is found with Guid matching the given <paramref name="targetAppGuid"/>.</exception>
        private TargetBase GetTargetApplicationByGuid(Guid targetAppGuid)
        {
            IEnumerable<TargetBase> targetApplications = _solutionFacade.GetTargetApplications();
            TargetBase? targetApp = targetApplications.FirstOrDefault(targetApp => targetApp.Guid == targetAppGuid);

            if (targetApp == null)
            {
                throw new BPMNConversionException($"No {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found with Guid '{targetAppGuid}'");
            }

            return targetApp;
        }

        /// <summary>
        /// Get the first <see cref="Consumer"/> for the given <paramref name="activity"/>.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to get the Consumer of.</param>
        /// <returns>First <see cref="Consumer"/> for the given <paramref name="activity"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Consumer"/> is found for the given <paramref name="activity"/>.</exception>
        private Consumer GetActivityFirstConsumer(Activity activity)
        {
            Consumer? firstConsumer = activity.ConsumerApplications.FirstOrDefault();

            if (firstConsumer == null)
            {
                throw new BPMNConversionException($"No Consumer found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{activity.ActivityName}'");
            }

            return firstConsumer;
        }

        /// <summary>
        /// Get <see cref="Participant"/> for the first <see cref="Consumer"/> of given <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to get the <see cref="Participant"/> from.</param>
        /// <param name="activity"><see cref="Activity"/> to get the <see cref="Consumer"/> from.</param>
        /// <returns><see cref="Participant"/> for the first <see cref="Consumer"/> of given <paramref name="activity"/>.</returns>
        private Participant GetConsumerParticipant(Collaboration collaboration, Activity activity)
        {
            Consumer consumer = GetActivityFirstConsumer(activity);
            Participant consumerParticipant = GetParticipantByGuid(collaboration, consumer.ConsumerGuid);
            return consumerParticipant;
        }

        /// <summary>
        /// Get the <see cref="Participant"/> for the target application of given <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to get the <see cref="Participant"/> from.</param>
        /// <param name="activity"><see cref="Activity"/> to get the target application from.</param>
        /// <returns><see cref="Participant"/> for the target application of given <paramref name="activity"/>.</returns>
        private Participant GetTargetApplicationParticipant(Collaboration collaboration, Activity activity)
        {
            TargetBase targetApp = GetTargetApplicationByName(activity.TargetApplication);
            Participant targetAppParticipant = GetParticipantByGuid(collaboration, targetApp.Guid);
            return targetAppParticipant;
        }

        /// <summary>
        /// Add <see cref="Task"/> for the given <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="Task"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="Task"/>.</param>
        /// <param name="previousFlowSource">Previous flow source to create the link with the new <see cref="Task"/>.</param>
        /// <returns>New <see cref="IFlowSource"/> which should be used to link with the next entitites.</returns>
        private IFlowSource AddTasksForActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            if (IsWebServicesActivity(activity))
            {
                previousFlowSource = AddTasksForWebServicesActivity(collaboration, activity, previousFlowSource);
            }
            else
            {
                previousFlowSource = AddTasksForUIActivity(collaboration, activity, previousFlowSource);
            }

            return previousFlowSource;
        }

        /// <summary>
        /// Get <see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to get the <see cref="Participant"/> from.</param>
        /// <param name="participantGuid">Guid of the <see cref="Participant"/> to search.</param>
        /// <returns><see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Participant"/> is found with Guid matching the <paramref name="participantGuid"/>.</exception>
        private Participant GetParticipantByGuid(Collaboration collaboration, Guid participantGuid)
        {
            Participant? participant = collaboration
                .Participants
                .FirstOrDefault(participant => string.Equals(participant.Guid, participantGuid.ToString()));

            if (participant == null)
            {
                throw new BPMNConversionException($"No BPMN Participant (Ginger {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found by Guid '{participantGuid}'");
            }

            return participant;
        }

        /// <summary>
        /// Add <see cref="Task"/> for the given WebServices <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="Task"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="Task"/> for.</param>
        /// <param name="previousFlowSource">Previous flow source to create the link with the new <see cref="Task"/>.</param>
        /// <returns>New <see cref="IFlowSource"/> which should be used to link with the next entitites.</returns>
        private IFlowSource AddTasksForWebServicesActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            Participant consumerParticipant = GetConsumerParticipant(collaboration, activity);
            Participant targetAppParticipant = GetTargetApplicationParticipant(collaboration, activity);

            Task requestSourceTask = consumerParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_RequestSource");
            Task requestTargetTask = targetAppParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_RequestTarget");
            Task responseSourceTask = targetAppParticipant.Process.AddTask<SendTask>(name: $"{activity.ActivityName}_ResponseSource");
            Task responseTargetTask = consumerParticipant.Process.AddTask<ReceiveTask>(name: $"{activity.ActivityName}_ResponseTarget");

            Flow.Create(name: string.Empty, previousFlowSource, requestSourceTask);

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

            return responseTargetTask;
        }

        /// <summary>
        /// Add <see cref="Task"/> for the given UI <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="Task"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="Task"/> for.</param>
        /// <param name="previousFlowSource">Previous flow source to create the link with the new <see cref="Task"/>.</param>
        /// <returns>New <see cref="IFlowSource"/> which should be used to link with the next entitites.</returns>
        private UserTask AddTasksForUIActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            Participant participant = GetTargetApplicationParticipant(collaboration, activity);

            UserTask userTask = participant.Process.AddTask<UserTask>(guid: activity.Guid, name: activity.ActivityName);

            Flow.Create(name: string.Empty, previousFlowSource, userTask);

            string activityGuid = activity.Guid.ToString();
            string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "aa");
            userTask.MessageRef = messageRef;

            return userTask;
        }

        /// <summary>
        /// Add <see cref="StartEvent"/> for the the given <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="StartEvent"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="StartEvent"/> for.</param>
        /// <returns>Newly added <see cref="StartEvent"/>.</returns>
        private StartEvent AddStartEventForActivity(Collaboration collaboration, Activity activity)
        {
            Participant participant;
            if (IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityFirstConsumer(activity);
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

        /// <summary>
        /// Add <see cref="EndEvent"/> for the the given <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="EndEvent"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="EndEvent"/> for.</param>
        /// <param name="previousFlowSource">Previous slow source to create the link with the new <see cref="EndEvent"/>.</param>
        /// <returns>Newly added <see cref="EndEvent"/>.</returns>
        private EndEvent AddEndEventForActivity(Collaboration collaboration, Activity activity, IFlowSource previousFlowSource)
        {
            Participant participant;
            if(IsWebServicesActivity(activity))
            {
                Consumer consumer = GetActivityFirstConsumer(activity);
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