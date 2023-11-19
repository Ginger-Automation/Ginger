﻿using Amdocs.Ginger.Common.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    internal sealed class ActivityTaskCreator
    {
        private readonly Activity _activity;
        private readonly Collaboration _collaboration;
        private readonly ISolutionFacadeForBPMN _solutionFacade;

        internal ActivityTaskCreator(Activity activity, Collaboration collaboration, ISolutionFacadeForBPMN solutionFacade)
        {
            _activity = activity;
            _collaboration = collaboration;
            _solutionFacade = solutionFacade;
        }

        internal IEnumerable<Task> Create()
        {
            IEnumerable<Task> tasksForActivity;
            if (ActivityBPMNUtil.IsWebServicesActivity(_activity, _solutionFacade))
            {
                tasksForActivity = CreateTasksForWebServicesActivity();
            }
            else
            {
                tasksForActivity = new List<Task>() { CreateTasksForUIActivity() };
            }

            return tasksForActivity;
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

        /// <summary>
        /// Add <see cref="Task"/> for the given WebServices <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="Task"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="Task"/> for.</param>
        /// <returns>New <see cref="IFlowSource"/> which should be used to link with the next entitites.</returns>
        private IEnumerable<Task> CreateTasksForWebServicesActivity()
        {
            Participant consumerParticipant = GetConsumerParticipant();
            Participant targetAppParticipant = GetActivityTargetApplicationParticipant();

            Task requestSourceTask = consumerParticipant.Process.AddTask<SendTask>(name: $"{_activity.ActivityName}_RequestSource");
            Task requestTargetTask = targetAppParticipant.Process.AddTask<ReceiveTask>(name: $"{_activity.ActivityName}_RequestTarget");
            Task responseSourceTask = targetAppParticipant.Process.AddTask<SendTask>(name: $"{_activity.ActivityName}_ResponseSource");
            Task responseTargetTask = consumerParticipant.Process.AddTask<ReceiveTask>(name: $"{_activity.ActivityName}_ResponseTarget");

            IEnumerable<Task> tasksForActivity = new List<Task>()
            {
                requestSourceTask,
                requestTargetTask,
                responseSourceTask,
                responseTargetTask
            };

            Flow requestFlow = Flow.Create(name: $"{_activity.ActivityName}_IN", requestSourceTask, requestTargetTask);

            if (requestFlow is MessageFlow requestMessageFlow)
            {
                string activityGuid = _activity.Guid.ToString();
                string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "aa");
                requestMessageFlow.MessageRef = messageRef;
            }

            Flow.Create(name: string.Empty, requestTargetTask, responseSourceTask);

            Flow responseFlow = Flow.Create(name: $"{_activity.ActivityName}_OUT", responseSourceTask, responseTargetTask);

            if (responseFlow is MessageFlow responseMessageFlow)
            {
                string activityGuid = _activity.Guid.ToString();
                string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "bb");
                responseMessageFlow.MessageRef = messageRef;
            }

            return tasksForActivity;
        }

        /// <summary>
        /// Add <see cref="Task"/> for the given UI <paramref name="activity"/>.
        /// </summary>
        /// <param name="collaboration"><see cref="Collaboration"/> to add the <see cref="Task"/> to.</param>
        /// <param name="activity"><see cref="Activity"/> to create the <see cref="Task"/> for.</param>
        /// <returns>New <see cref="IFlowSource"/> which should be used to link with the next entitites.</returns>
        private UserTask CreateTasksForUIActivity()
        {
            Participant participant = GetActivityTargetApplicationParticipant();

            UserTask userTask = participant.Process.AddTask<UserTask>(guid: _activity.Guid, name: _activity.ActivityName);

            string activityGuid = _activity.Guid.ToString();
            string messageRef = string.Concat(activityGuid.AsSpan(0, activityGuid.Length - 2), "aa");
            userTask.MessageRef = messageRef;

            return userTask;
        }

        /// <summary>
        /// Get <see cref="Participant"/> for the first <see cref="Consumer"/> of given <paramref name="activity"/>.
        /// </summary>
        /// <param name="activity"><see cref="Activity"/> to get the <see cref="Consumer"/> from.</param>
        /// <returns><see cref="Participant"/> for the first <see cref="Consumer"/> of given <paramref name="activity"/>.</returns>
        private Participant GetConsumerParticipant()
        {
            Consumer consumer = ActivityBPMNUtil.GetActivityFirstConsumer(_activity);
            Participant consumerParticipant = GetParticipantByGuid(consumer.ConsumerGuid);
            return consumerParticipant;
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
    }
}
