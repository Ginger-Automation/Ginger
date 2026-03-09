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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Utils;
using GingerCore;
using GingerCore.Activities;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Conversion
{
    public sealed class CollaborationFromBusinessFlowCreator
    {
        private readonly BusinessFlow _businessFlow;
        private readonly ISolutionFacadeForBPMN _solutionFacade;

        public CollaborationFromBusinessFlowCreator(BusinessFlow businessFlow) : this(businessFlow, new WorkSpaceToSolutionFacadeAdapter(WorkSpace.Instance)) { }

        public CollaborationFromBusinessFlowCreator(BusinessFlow businessFlow, ISolutionFacadeForBPMN solutionFacade)
        {
            _businessFlow = businessFlow;
            _solutionFacade = solutionFacade;
        }

        public Collaboration Create()
        {
            Collaboration collaboration = CreateCollaboration();

            CreateCallActivitiesForActivityGroups(collaboration);

            return collaboration;
        }

        private Collaboration CreateCollaboration()
        {

            IEnumerable<Participant> participants = CreateParticipants();

            Collaboration collaboration = Collaboration.CreateForUseCase(
                guid: _businessFlow.Guid);
            collaboration.Name = _businessFlow.Name;
            collaboration.Description = _businessFlow.Description;

            foreach (Participant participant in participants)
            {
                collaboration.AddParticipant(participant);
            }

            return collaboration;
        }

        private IEnumerable<Participant> CreateParticipants()
        {
            List<Participant> participants = [];
            IEnumerable<ActivitiesGroup> activityGroups = _businessFlow.ActivitiesGroups.Where(ag => ActivitiesGroupBPMNUtil.IsActive(ag, _solutionFacade));
            if (!activityGroups.Any())
            {
                throw new BPMNConversionException($"No active {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} available in {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}.");
            }

            foreach (ActivitiesGroup activityGroup in activityGroups)
            {
                Participant? participant = CreateParticipant(activityGroup);
                if (participant == null)
                {
                    continue;
                }
                participants.Add(participant);
            }
            return participants;
        }

        private Participant? CreateParticipant(ActivitiesGroup activityGroup)
        {
            bool isEmpty = !activityGroup.ActivitiesIdentifiers.Any();
            if (isEmpty)
            {
                return null;
            }

            IEnumerable<Activity> activities = ActivitiesGroupBPMNUtil.GetActivities(activityGroup, _solutionFacade);
            Activity firstActivity = activities.First();
            Participant participant;
            if (ActivityBPMNUtil.IsWebServicesActivity(firstActivity, _solutionFacade))
            {
                Consumer firstConsumer = ActivityBPMNUtil.GetActivityFirstConsumer(firstActivity);
                TargetBase consumerTargetApp = TargetApplicationBPMNUtil.GetTargetApplicationByGuid(firstConsumer.ConsumerGuid, _solutionFacade);
                participant = new(consumerTargetApp.Guid)
                {
                    Name = consumerTargetApp.Name,
                    SystemRef = consumerTargetApp.Guid.ToString()
                };
            }
            else
            {
                TargetBase targetApp = TargetApplicationBPMNUtil.GetTargetApplicationByName(firstActivity.TargetApplication, _solutionFacade);
                participant = new(targetApp.Guid)
                {
                    Name = targetApp.Name,
                    SystemRef = targetApp.Guid.ToString()
                };
            }
            return participant;
        }

        private void CreateCallActivitiesForActivityGroups(Collaboration collaboration)
        {
            IFlowSource previousFlowSource = null!;
            IEnumerator<ActivitiesGroup> activityGroupEnumerator = _businessFlow.ActivitiesGroups.GetEnumerator();
            bool isFirstActivityGroup = true;
            ActivitiesGroup? lastActivityGroup = null;

            bool allActivityGroupsAreEmpty = true;

            while (activityGroupEnumerator.MoveNext())
            {
                ActivitiesGroup activityGroup = activityGroupEnumerator.Current;

                bool hasNoActivities = !activityGroup.ActivitiesIdentifiers.Any();
                if (hasNoActivities)
                {
                    continue;
                }
                allActivityGroupsAreEmpty = false;

                if (isFirstActivityGroup)
                {
                    StartEvent startEvent = CreateStartEventInParticipantOfActivityGroup(activityGroup, collaboration);
                    previousFlowSource = startEvent;
                    isFirstActivityGroup = false;
                }
                CallActivity callActivity = CreateCallActivityForActivityGroup(activityGroup, collaboration);
                Flow.Create(name: string.Empty, previousFlowSource, callActivity);
                previousFlowSource = callActivity;
                lastActivityGroup = activityGroup;
            }
            if (previousFlowSource != null && lastActivityGroup != null)
            {
                EndEvent endEvent = CreateEndEventInParticipantOfActivityGroup(lastActivityGroup, collaboration);
                Flow.Create(name: string.Empty, previousFlowSource, endEvent);
            }

            if (allActivityGroupsAreEmpty)
            {
                throw new BPMNConversionException($"All {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups)} cannot be empty.");
            }
        }

        private StartEvent CreateStartEventInParticipantOfActivityGroup(ActivitiesGroup activityGroup, Collaboration collaboration)
        {
            Participant participant = GetParticipantOfActivityGroup(activityGroup, collaboration);
            StartEvent startEvent = participant.Process.AddStartEvent(name: string.Empty);
            return startEvent;
        }

        private CallActivity CreateCallActivityForActivityGroup(ActivitiesGroup activityGroup, Collaboration collaboration)
        {
            Participant participant = GetParticipantOfActivityGroup(activityGroup, collaboration);
            CallActivity callActivity = participant.Process.AddCallActivity(name: activityGroup.Name);
            callActivity.ProcessRef = activityGroup.Guid.ToString();
            return callActivity;
        }

        private EndEvent CreateEndEventInParticipantOfActivityGroup(ActivitiesGroup activityGroup, Collaboration collaboration)
        {
            Participant participant = GetParticipantOfActivityGroup(activityGroup, collaboration);
            EndEvent endEvent = participant.Process.AddEndEvent(name: string.Empty, EndEventType.Termination);
            return endEvent;
        }

        private Participant GetParticipantOfActivityGroup(ActivitiesGroup activityGroup, Collaboration collaboration)
        {
            IEnumerable<Activity> activities = ActivitiesGroupBPMNUtil.GetActivities(activityGroup, _solutionFacade);
            Activity firstActivity = activities.First();
            Participant? participantOfFirstActivity;
            if (ActivityBPMNUtil.IsWebServicesActivity(firstActivity, _solutionFacade))
            {
                Consumer firstConsumer = ActivityBPMNUtil.GetActivityFirstConsumer(firstActivity);
                participantOfFirstActivity = collaboration.GetParticipantByGuid(firstConsumer.ConsumerGuid);
            }
            else
            {
                TargetBase targetApp = TargetApplicationBPMNUtil.GetTargetApplicationByName(targetAppName: firstActivity.TargetApplication, _solutionFacade);
                participantOfFirstActivity = collaboration.GetParticipantByGuid(targetApp.Guid);
            }

            if (participantOfFirstActivity == null)
            {
                throw new BPMNConversionException($"No {nameof(Participant)} found for {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{firstActivity.ActivityName}' in {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)} '{activityGroup.Name}'.");
            }

            return participantOfFirstActivity;
        }
    }
}
