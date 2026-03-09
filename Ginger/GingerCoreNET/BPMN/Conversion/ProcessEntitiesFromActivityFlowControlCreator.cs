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

using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Utils;
using GingerCore;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Conversion
{
    internal sealed class ProcessEntitiesFromActivityFlowControlCreator
    {

        internal static readonly IReadOnlySet<eFlowControlAction> SubProcessRelevantFlowControls = new HashSet<eFlowControlAction>()
        {
            eFlowControlAction.GoToActivity,
            eFlowControlAction.RerunActivity,
            eFlowControlAction.GoToActivityByName,
            eFlowControlAction.RunSharedRepositoryActivity,
            eFlowControlAction.StopRun,
            eFlowControlAction.StopBusinessFlow,
            eFlowControlAction.FailActionAndStopBusinessFlow
        };

        private readonly Activity _activity;
        private readonly Collaboration _collaboration;
        private readonly ISolutionFacadeForBPMN _solutionFacade;
        private readonly IDictionary<Activity, IEnumerable<Task>> _activityTasksMap;
        private readonly Participant _activityParticipant;
        private readonly IEnumerable<FlowControl> _flowControls;

        internal ProcessEntitiesFromActivityFlowControlCreator(
            Activity activity, Collaboration collaboration, ISolutionFacadeForBPMN solutionFacade,
            IDictionary<Activity, IEnumerable<Task>> activityTasksMap)
        {
            _activity = activity;
            _collaboration = collaboration;
            _solutionFacade = solutionFacade;
            _activityTasksMap = activityTasksMap;
            _activityParticipant = GetActivityParticipant();
            _flowControls = GetRelevantFlowControls();
        }

        private IEnumerable<FlowControl> GetRelevantFlowControls()
        {
            return ActivityBPMNUtil
                .GetFlowControls(_activity)
                .Where(fc => fc.Active && SubProcessRelevantFlowControls.Contains(fc.FlowControlAction));
        }

        internal IEnumerable<IProcessEntity> Create()
        {
            if (!_flowControls.Any())
            {
                return Array.Empty<Task>();
            }

            ExclusiveGateway gateway = CreateGateway();
            List<Task> conditionalTasks = [];
            foreach (FlowControl flowControl in _flowControls)
            {
                conditionalTasks.AddRange(CreateConditionalTasks(gateway, flowControl));
            }

            List<IProcessEntity> processEntitiesForFlowControl = [.. conditionalTasks, gateway];

            return processEntitiesForFlowControl;
        }


        private ExclusiveGateway CreateGateway()
        {
            return _activityParticipant.Process.AddExclusiveGateway(name: string.Empty);
        }

        private IEnumerable<Task> CreateConditionalTasks(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            switch (flowControl.FlowControlAction)
            {
                case eFlowControlAction.GoToActivity:
                    CreateConditionalTasksForGoToActivityByGuid(exclusiveGateway, flowControl);
                    return Array.Empty<Task>();
                case eFlowControlAction.RerunActivity:
                    CreateConditionalTasksForRerunActivity(exclusiveGateway, flowControl);
                    return Array.Empty<Task>();
                case eFlowControlAction.FailActionAndStopBusinessFlow:
                    CreateConditionalTasksForStopExecutionWithError(exclusiveGateway, flowControl);
                    return Array.Empty<Task>();
                case eFlowControlAction.StopRun:
                case eFlowControlAction.StopBusinessFlow:
                    CreateConditionalTasksForStopExecution(exclusiveGateway, flowControl);
                    return Array.Empty<Task>();
                case eFlowControlAction.GoToActivityByName:
                    CreateConditionalTasksForGoToActivityByName(exclusiveGateway, flowControl);
                    return Array.Empty<Task>();
                case eFlowControlAction.RunSharedRepositoryActivity:
                    return CreateConditionalTasksForGoToSharedRepositoryActivity(exclusiveGateway, flowControl);
                default:
                    return Array.Empty<Task>();
            }
        }

        private void CreateConditionalTasksForGoToActivityByGuid(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            Guid targetActivityGuid = flowControl.GetGuidFromValue();
            IEnumerable<Task> targetActivityTasks = GetTasksForActivityByGuid(targetActivityGuid);
            Task targetActivityFirstTask = targetActivityTasks.First();

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(name: flowControlActionName, source: conditionalTask, target: targetActivityFirstTask);
        }

        private void CreateConditionalTasksForRerunActivity(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            Guid targetActivityGuid = _activity.Guid;
            IEnumerable<Task> targetActivityTasks = GetTasksForActivityByGuid(targetActivityGuid);
            Task targetActivityFirstTask = targetActivityTasks.First();

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(flowControlActionName, source: conditionalTask, target: targetActivityFirstTask);
        }

        private void CreateConditionalTasksForStopExecutionWithError(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            EndEvent? endEvent = null;
            if (_activityParticipant.Process.EndEvents.Any())
            {
                endEvent = _activityParticipant.Process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Error);
            }

            if (endEvent == null)
            {
                endEvent = _activityParticipant.Process.AddEndEvent(name: string.Empty, EndEventType.Error);
            }

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(flowControlActionName, source: conditionalTask, target: endEvent);
        }

        private void CreateConditionalTasksForStopExecution(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            EndEvent? endEvent = null;
            if (_activityParticipant.Process.EndEvents.Any())
            {
                endEvent = _activityParticipant.Process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Termination);
            }

            if (endEvent == null)
            {
                endEvent = _activityParticipant.Process.AddEndEvent(name: string.Empty, EndEventType.Termination);
            }

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(flowControlActionName, source: conditionalTask, target: endEvent);
        }

        private void CreateConditionalTasksForGoToActivityByName(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            string targetActivityName = flowControl.GetNameFromValue();
            IEnumerable<Task> targetActivityTasks = GetTasksForActivityByName(targetActivityName);
            Task targetActivityFirstTask = targetActivityTasks.First();

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(name: flowControlActionName, source: conditionalTask, target: targetActivityFirstTask);
        }

        private IEnumerable<Task> CreateConditionalTasksForGoToSharedRepositoryActivity(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            string targetActivityName = flowControl.GetNameFromValue();
            Activity? targetActivity = _solutionFacade.GetActivitiesFromSharedRepository().FirstOrDefault(a => string.Equals(a.ActivityName, targetActivityName));
            if (targetActivity == null)
            {
                throw new FlowControlTargetActivityNotFoundException($"No target {GingerDicser.GetTermResValue(eTermResKey.Activity)} found in shared repository with name {targetActivityName} for Flow-Control in {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{_activity.ActivityName}'.");
            }

            Task targetActivityTask = _activityParticipant.Process.AddTask<Task>(name: targetActivityName);

            //TODO: BPMN - Get FlowControlAction name from EnumValueDescription attribute
            string flowControlActionName = flowControl.FlowControlAction.ToString();

            string conditionalTaskName = GetConditionalTaskName(flowControl);

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(name: conditionalTaskName);

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(flowControlActionName, conditionalTask, targetActivityTask);

            return [targetActivityTask];
        }

        private string GetConditionalTaskName(FlowControl flowControl)
        {
            string conditionalTaskName;
            string operatorName = flowControl.Operator.ToString();
            if (flowControl.Operator is eFCOperator.CSharp or eFCOperator.Legacy)
            {
                conditionalTaskName = $"FC - {operatorName} - {flowControl.Condition}";
            }
            else
            {
                //TODO: BPMN - Get operator name from EnumValueDescription attribute
                conditionalTaskName = $"FC - {operatorName}";
            }
            return conditionalTaskName;
        }

        private IEnumerable<Task> GetTasksForActivityByGuid(Guid activityGuid)
        {
            KeyValuePair<Activity, IEnumerable<Task>> activityTasksPair = _activityTasksMap.FirstOrDefault(kv => kv.Key.Guid == activityGuid);
            if (activityTasksPair.Value == null || !activityTasksPair.Value.Any())
            {
                throw new FlowControlTargetActivityNotFoundException($"No target {GingerDicser.GetTermResValue(eTermResKey.Activity)} found with Guid '{activityGuid}' for Flow-Control in {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{_activity.ActivityName}'.");
            }

            return activityTasksPair.Value;
        }

        private IEnumerable<Task> GetTasksForActivityByName(string activityName)
        {
            KeyValuePair<Activity, IEnumerable<Task>> activityTasksPair = _activityTasksMap.FirstOrDefault(kv => string.Equals(kv.Key.ActivityName, activityName));
            if (activityTasksPair.Value == null || !activityTasksPair.Value.Any())
            {
                throw new FlowControlTargetActivityNotFoundException($"No target {GingerDicser.GetTermResValue(eTermResKey.Activity)} found with name '{activityName}' for Flow-Control in {GingerDicser.GetTermResValue(eTermResKey.Activity)} '{_activity.ActivityName}'.");
            }

            return activityTasksPair.Value;
        }

        /// <summary>
        /// Get the <see cref="Participant"/> for the target application of the <see cref="Activity"/>.
        /// </summary>
        /// <returns><see cref="Participant"/> for the target application of the <see cref="Activity"/>.</returns>
        private Participant GetActivityParticipant()
        {
            Participant activityParticipant;
            if (ActivityBPMNUtil.IsWebServicesActivity(_activity, _solutionFacade))
            {
                Consumer firstConsumer = ActivityBPMNUtil.GetActivityFirstConsumer(_activity);
                activityParticipant = GetParticipantByGuid(firstConsumer.ConsumerGuid);
            }
            else
            {
                TargetBase targetApp = SolutionBPMNUtil.GetTargetApplicationByName(_activity.TargetApplication, _solutionFacade);
                activityParticipant = GetParticipantByGuid(targetApp.Guid);
            }
            return activityParticipant;
        }

        /// <summary>
        /// Get <see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.
        /// </summary>
        /// <param name="participantGuid">Guid of the <see cref="Participant"/> to search.</param>
        /// <returns><see cref="Participant"/> with Guid matching the given <paramref name="participantGuid"/>.</returns>
        /// <exception cref="BPMNConversionException">If no <see cref="Participant"/> is found with Guid matching the <paramref name="participantGuid"/>.</exception>
        private Participant GetParticipantByGuid(Guid participantGuid)
        {
            Participant? participant = _collaboration.GetParticipantByGuid(participantGuid);

            if (participant == null)
            {
                throw new BPMNConversionException($"No BPMN Participant (Ginger {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} found by Guid '{participantGuid}'");
            }

            return participant;
        }
    }
}
