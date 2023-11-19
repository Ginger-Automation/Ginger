using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.FlowControlLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    internal sealed class ProcessEntitiesFromActivityFlowControlCreator
    {
        internal static readonly IReadOnlySet<eFlowControlAction> SubProcessRelevantFlowControls = new HashSet<eFlowControlAction>()
        {
            eFlowControlAction.GoToActivity,
            eFlowControlAction.StopBusinessFlow,
            eFlowControlAction.RerunActivity,
            eFlowControlAction.StopRun,
            eFlowControlAction.RunSharedRepositoryActivity,
            eFlowControlAction.FailActionAndStopBusinessFlow
        };

        private readonly Activity _activity;
        private readonly Collaboration _collaboration;
        private readonly ISolutionFacadeForBPMN _solutionFacade;
        private readonly Participant _activityParticipant;
        private readonly IEnumerable<FlowControl> _flowControls;

        internal ProcessEntitiesFromActivityFlowControlCreator(Activity activity, Collaboration collaboration, ISolutionFacadeForBPMN solutionFacade)
        {
            _activity = activity;
            _collaboration = collaboration;
            _solutionFacade = solutionFacade;
            _activityParticipant = GetActivityTargetApplicationParticipant();
            _flowControls = GetRelevantFlowControls();
        }

        private IEnumerable<FlowControl> GetRelevantFlowControls()
        {
            return ActivityBPMNUtil
                .GetFlowControls(_activity)
                .Where(fc => SubProcessRelevantFlowControls.Contains(fc.FlowControlAction));
        }

        internal ExclusiveGateway? Create()
        {
            if(!_flowControls.Any())
            {
                return null;
            }

            ExclusiveGateway gateway = CreateGateway();
            foreach(FlowControl flowControl in _flowControls)
            {
                CreateConditionalTasks(gateway, flowControl);
            }
            return gateway;
        }


        private ExclusiveGateway CreateGateway()
        {
            return _activityParticipant.Process.AddExclusiveGateway(name: string.Empty);
        }

        private void CreateConditionalTasks(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            switch(flowControl.FlowControlAction)
            {
                case eFlowControlAction.GoToActivity:
                    CreateConditionalTasksForGoToActivity(exclusiveGateway, flowControl);
                    break;
                case eFlowControlAction.RerunActivity:
                    CreateConditionalTasksForRerunActivity(exclusiveGateway, flowControl);
                    break;
                default:
                    throw new NotSupportedException($"Creating conditional {nameof(Task)} for {nameof(eFlowControlAction)} of type {flowControl.FlowControlAction} is not supported.");
            }
        }

        private void CreateConditionalTasksForGoToActivity(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            Guid targetActivityGuid = flowControl.GetGuidFromValue();
            Task? targetActivityTask = _activityParticipant.Process.GetChildEntitiesByType<Task>().FirstOrDefault(task => task.Guid == targetActivityGuid);
            if(targetActivityTask == null)
            {
                throw new BPMNConversionException($"No BPMN {nameof(Task)} found for {GingerDicser.GetTermResValue(eTermResKey.Activity)}.");
            }

            string conditionalTaskName;
            if(flowControl.Operator == eFCOperator.CSharp || flowControl.Operator == eFCOperator.Legacy)
            {
                conditionalTaskName = flowControl.Condition;
            }
            else
            {
                string operatorName = flowControl.Operator.ToString();
                //TODO: BPMN - Get operator name from EnumValueDescription attribute
                conditionalTaskName = $"Operator - {operatorName}";
            }

            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(
                name: conditionalTaskName, 
                conditions: new List<Task.Condition>() 
                { 
                    new Task.FieldValueCondition(nameFieldTag: $"FC_{flowControl.Guid}", valueFieldTag: $"FC_PASSED_FIELD_TAG") 
                });

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(name: string.Empty, source: conditionalTask, target: targetActivityTask);
        }

        private void CreateConditionalTasksForRerunActivity(ExclusiveGateway exclusiveGateway, FlowControl flowControl)
        {
            Guid targetActivityGuid = _activity.Guid;
            Task? targetActivityTask = _activityParticipant.Process.GetChildEntitiesByType<Task>().FirstOrDefault(task => task.Guid == targetActivityGuid);
            if (targetActivityTask == null)
            {
                throw new BPMNConversionException($"No BPMN {nameof(Task)} found for {GingerDicser.GetTermResValue(eTermResKey.Activity)}.");
            }

            string conditionalTaskName;
            if (flowControl.Operator == eFCOperator.CSharp || flowControl.Operator == eFCOperator.Legacy)
            {
                conditionalTaskName = flowControl.Condition;
            }
            else
            {
                string operatorName = flowControl.Operator.ToString();
                //TODO: BPMN - Get operator name from EnumValueDescription attribute
                conditionalTaskName = $"Operator - {operatorName}";
            }
            Task conditionalTask = _activityParticipant.Process.AddTask<Task>(
                name: conditionalTaskName,
                conditions: new List<Task.Condition>()
                {
                    new Task.FieldValueCondition(nameFieldTag: $"FC_{flowControl.Guid}", valueFieldTag: $"FC_PASSED_FIELD_TAG")
                });

            Flow.Create(name: string.Empty, source: exclusiveGateway, target: conditionalTask);
            Flow.Create(name: string.Empty, source: exclusiveGateway, target: targetActivityTask);
        }

        /// <summary>
        /// Get the <see cref="Participant"/> for the target application of the <see cref="Activity"/>.
        /// </summary>
        /// <returns><see cref="Participant"/> for the target application of the <see cref="Activity"/>.</returns>
        private Participant GetActivityTargetApplicationParticipant()
        {
            TargetBase targetApp = SolutionBPMNUtil.GetTargetApplicationByName(_activity.TargetApplication, _solutionFacade);
            Participant targetAppParticipant = GetParticipantByGuid(targetApp.Guid);
            return targetAppParticipant;
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
