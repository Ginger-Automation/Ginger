#region License
/*
Copyright © 2014-2025 European Support Limited

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
using MongoDB.Driver.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Conversion
{
    //TODO: BPMN - How to handle WithoutPlatformApps
    /// <summary>
    /// Creates a BPMN <see cref="Collaboration"/> from an <see cref="ActivitiesGroup"/>.
    /// </summary>
    public sealed class CollaborationFromActivityGroupCreator
    {
        public sealed class Options
        {
            public bool IgnoreInterActivityFlowControls { get; set; } = false;
        }

        private readonly ActivitiesGroup _activityGroup;
        private readonly Options _options;
        private readonly ISolutionFacadeForBPMN _solutionFacade;

        /// <summary>
        /// Create a new <see cref="CollaborationFromActivityGroupCreator"/>.
        /// </summary>
        /// <param name="activityGroup"><see cref="ActivitiesGroup"/> which will be used to create.</param>
        public CollaborationFromActivityGroupCreator(ActivitiesGroup activityGroup) : this(activityGroup, new Options()) { }

        public CollaborationFromActivityGroupCreator(ActivitiesGroup activityGroup, Options options) : this(activityGroup, options, new WorkSpaceToSolutionFacadeAdapter(WorkSpace.Instance)) { }

        public CollaborationFromActivityGroupCreator(ActivitiesGroup activityGroup, ISolutionFacadeForBPMN solutionFacade) : this(activityGroup, new Options(), solutionFacade) { }

        /// <summary>
        /// Create a new <see cref="CollaborationFromActivityGroupCreator"/>.
        /// </summary>
        /// <param name="activityGroup"><see cref="ActivitiesGroup"/> which will be used to create.</param>
        /// <param name="solutionFacade">A facade to expose solution data.</param>
        public CollaborationFromActivityGroupCreator(ActivitiesGroup activityGroup, Options options, ISolutionFacadeForBPMN solutionFacade)
        {
            ValidateConstructorArgs(activityGroup, solutionFacade);
            _activityGroup = activityGroup;
            _options = options;
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
            if (activityGroup == null)
            {
                throw new ArgumentNullException(nameof(activityGroup));
            }
            if (solutionFacade == null)
            {
                throw new ArgumentNullException(nameof(solutionFacade));
            }
        }

        /// <summary>
        /// Create a BPMN <see cref="Collaboration"/> from <see cref="ActivitiesGroup"/>.
        /// </summary>
        /// <returns>BPMN <see cref="Collaboration"/>.</returns>
        public Collaboration Create()
        {
            IEnumerable<Activity> activities = ActivitiesGroupBPMNUtil.GetActivities(_activityGroup, _solutionFacade);

            Collaboration collaboration = CreateCollaboration(activities);

            CreateProcessEntitiesForActivities(activities, collaboration);

            return collaboration;
        }

        private void CreateProcessEntitiesForActivities(IEnumerable<Activity> activities, Collaboration collaboration)
        {
            if (!activities.Any())
            {
                return;
            }

            HistoricalEnumerator<Activity> activitiesEnumerator = new(activities.GetEnumerator());
            Dictionary<Activity, IEnumerable<Task>> activityTasksMap = [];
            CreateProcessEntitiesForActivities(activitiesEnumerator, collaboration, activityTasksMap);
        }

        private IFlowTarget CreateProcessEntitiesForActivities(HistoricalEnumerator<Activity> activitiesEnumerator, Collaboration collaboration, IDictionary<Activity, IEnumerable<Task>> activityTaskMap)
        {
            bool isFirstActivity = activitiesEnumerator.Current == null;

            bool noMoreActivitiesLeft = !activitiesEnumerator.MoveNext();
            if (noMoreActivitiesLeft)
            {
                Participant participantWithStartEvent = collaboration
                    .Participants
                    .First(p => p.Process.StartEvent != null);
                EndEvent endEvent = participantWithStartEvent
                    .Process
                    .AddEndEvent(string.Empty);
                return endEvent;
            }

            Activity currentActivity = activitiesEnumerator.Current!;

            StartEvent? startEvent = null;
            if (isFirstActivity)
            {
                startEvent = CreateStartEventInParticipantOfActivity(currentActivity, collaboration);
            }

            IEnumerable<Task> tasksForCurrentActivity = CreateTasksForActivity(currentActivity, collaboration);
            activityTaskMap.Add(currentActivity, tasksForCurrentActivity);

            Task firstTaskForCurrentActivity = tasksForCurrentActivity.First();
            Task lastTaskForCurrentActivity = tasksForCurrentActivity.Last();

            IFlowTarget firstEntityForNextActivity = CreateProcessEntitiesForActivities(activitiesEnumerator, collaboration, activityTaskMap);

            IEnumerable<IProcessEntity> processEntitiesForFlowControls = CreateProcessEntitiesForActivityFlowControls(currentActivity, collaboration, activityTaskMap);

            if (startEvent != null)
            {
                Flow.Create(name: string.Empty, source: startEvent, firstTaskForCurrentActivity);
            }

            if (processEntitiesForFlowControls.Any())
            {
                ExclusiveGateway exclusiveGateway = (ExclusiveGateway)processEntitiesForFlowControls.First(pe => pe is ExclusiveGateway);
                IEnumerable<IFlowSource> conditionalTasks = processEntitiesForFlowControls.Where(pe => pe is IFlowSource and not ExclusiveGateway).Cast<IFlowSource>();

                Flow.Create(name: string.Empty, source: lastTaskForCurrentActivity, target: exclusiveGateway);

                foreach (IFlowSource conditionalTask in conditionalTasks)
                {
                    Flow.Create(name: string.Empty, source: conditionalTask, target: firstEntityForNextActivity);
                }

                Flow.Create(name: "default", source: exclusiveGateway, target: firstEntityForNextActivity);
            }
            else
            {
                Flow.Create(name: string.Empty, source: lastTaskForCurrentActivity, target: firstEntityForNextActivity);
            }

            return firstTaskForCurrentActivity;
        }

        private IEnumerable<Task> CreateTasksForActivity(Activity activity, Collaboration collaboration)
        {
            ActivityTaskCreator activityTaskCreator = new(activity, collaboration, _solutionFacade);
            IEnumerable<Task> tasksForActivity = activityTaskCreator.Create();

            if (!tasksForActivity.Any())
            {
                throw new BPMNConversionException($"No BPMN {nameof(Task)} were created for {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{activity.ActivityName}'.");
            }

            return tasksForActivity;
        }

        private StartEvent CreateStartEventInParticipantOfActivity(Activity activity, Collaboration collaboration)
        {
            Participant participant;
            if (ActivityBPMNUtil.IsWebServicesActivity(activity, _solutionFacade))
            {
                Consumer consumer = ActivityBPMNUtil.GetActivityFirstConsumer(activity);
                participant = GetParticipantByGuid(consumer.ConsumerGuid, collaboration);
            }
            else
            {
                TargetBase targetApp = SolutionBPMNUtil.GetTargetApplicationByName(activity.TargetApplication, _solutionFacade);
                participant = GetParticipantByGuid(targetApp.Guid, collaboration);
            }

            StartEvent startEvent = participant.Process.AddStartEvent(name: string.Empty);

            return startEvent;
        }

        private IEnumerable<IProcessEntity> CreateProcessEntitiesForActivityFlowControls(Activity activity, Collaboration collaboration, IDictionary<Activity, IEnumerable<Task>> activityTasksMap)
        {
            if (_options.IgnoreInterActivityFlowControls)
            {
                return Array.Empty<IProcessEntity>();
            }

            ProcessEntitiesFromActivityFlowControlCreator processEntitiesFromActivityFlowControlCreator = new(
                activity,
                collaboration,
                _solutionFacade,
                activityTasksMap);
            return processEntitiesFromActivityFlowControlCreator.Create();
        }

        /// <summary>
        /// Get <see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.
        /// </summary>
        /// <param name="participantGuid">Guid of the <see cref="Participant"/> to search.</param>
        /// <returns><see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Participant"/> is found with Guid matching the <paramref name="participantGuid"/>.</exception>
        private Participant GetParticipantByGuid(Guid participantGuid, Collaboration collaboration)
        {
            Participant? participant = collaboration.GetParticipantByGuid(participantGuid);

            if (participant == null)
            {
                throw new BPMNConversionException($"No BPMN Participant (Ginger {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found by Guid '{participantGuid}'");
            }

            return participant;
        }

        /// <summary>
        /// Create new BPMN <see cref="Collaboration"/>.
        /// </summary>
        /// <returns>Newly created BPMN <see cref="Collaboration"/>.</returns>
        private Collaboration CreateCollaboration(IEnumerable<Activity> activities)
        {
            IEnumerable<Participant> participants = CreateParticipants(activities);

            Participant firstParticipant = participants.First();

            Collaboration collaboration = Collaboration.CreateForSubProcess(
                guid: _activityGroup.Guid,
                systemRef: firstParticipant.Guid.ToString());
            collaboration.Name = _activityGroup.Name;
            collaboration.Description = _activityGroup.Description;

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
        private IEnumerable<Participant> CreateParticipants(IEnumerable<Activity> activities)
        {
            List<Participant> participants = [];
            foreach (Activity activity in activities)
            {
                participants.AddRange(CreateParticipantsForActivity(activity));
            }

            if (!participants.Any())
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
            List<Participant> participants = [];

            if (ActivityBPMNUtil.IsWebServicesActivity(activity, _solutionFacade))
            {
                Consumer consumer = ActivityBPMNUtil.GetActivityFirstConsumer(activity);
                TargetBase consumerTargetApp = TargetApplicationBPMNUtil.GetTargetApplicationByGuid(consumer.ConsumerGuid, _solutionFacade);
                Participant participantForConsumer = new(consumerTargetApp.Guid)
                {
                    Name = consumerTargetApp.Name,
                    SystemRef = consumerTargetApp.Guid.ToString()
                };
                participants.Add(participantForConsumer);
            }

            TargetBase targetApp = SolutionBPMNUtil.GetTargetApplicationByName(activity.TargetApplication, _solutionFacade);
            Participant participantForTargetApp = new(targetApp.Guid)
            {
                Name = targetApp.Name,
                SystemRef = targetApp.Guid.ToString()
            };
            participants.Add(participantForTargetApp);

            return participants;
        }

        private sealed class HistoricalEnumerator<T> : IEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            /// <summary>
            /// Gets the element in the collection at the previous position of the enumerator.
            /// </summary>
            public T Previous { get; private set; }

            public T Current => _enumerator.Current;

            object IEnumerator.Current => ((IEnumerator)_enumerator).Current;

            public HistoricalEnumerator(IEnumerator<T> enumerator)
            {
                _enumerator = enumerator;
                Previous = default!;
            }

            public void Dispose()
            {
                _enumerator.Dispose();
            }

            public bool MoveNext()
            {
                Previous = Current;
                return _enumerator.MoveNext();
            }

            public void Reset()
            {
                Previous = default!;
                _enumerator.Reset();
            }
        }
    }
}