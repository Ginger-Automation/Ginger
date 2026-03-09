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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Conversion;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace GingerCoreNETUnitTest.BPMN
{
    [TestClass]
    public sealed class CollaborationFromActivityGroupCreatorTests
    {
        private const string ConditionalTaskNamePrefix = "FC - ";

        [TestMethod]
        public void Create_NullActivityGroup_ThrowsArgumentNullException()
        {
            ActivitiesGroup activityGroup = null!;
            ISolutionFacadeForBPMN solutionFacade = new Mock<ISolutionFacadeForBPMN>().Object;

            Assert.ThrowsException<ArgumentNullException>(() => new CollaborationFromActivityGroupCreator(activityGroup, solutionFacade));
        }

        [TestMethod]
        public void Create_EmptyActivitesGroup_ThrowsBPMNConversionException()
        {
            ActivitiesGroup activityGroup = new();
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([]);
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<NoValidActivityFoundInGroupException>(() => converter.Create());
        }

        [TestMethod]
        public void Create_AllInactiveActivities_ThrowsBPMNConversionException()
        {
            Activity inactiveActivity = new()
            {
                ActivityName = "Inactive Activity",
                Active = false
            };
            ActivitiesGroup activityGroup = new();
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
            {
                IdentifiedActivity = inactiveActivity
            });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([inactiveActivity]);
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<NoValidActivityFoundInGroupException>(() => converter.Create());
        }

        [TestMethod]
        public void Create_AllNonIdentifiedActivities_ThrowsBPMNConversionException()
        {
            ActivitiesGroup activityGroup = new();
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
            {
                IdentifiedActivity = null
            });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([]);
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<NoValidActivityFoundInGroupException>(() => converter.Create());
        }

        [TestMethod]
        public void Create_ActivityGroupWithInActiveActivities_InactiveActivitiesAreIgnored()
        {
            CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Activity inactiveActivity = activityGroup.ActivitiesIdentifiers.First(iden => !iden.IdentifiedActivity.Active).IdentifiedActivity;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Create();

            Assert.IsTrue(collaboration.Participants.Any(), $"{nameof(Collaboration)} has no {nameof(Participant)}");
            Process firstParticipantProcess = collaboration.Participants.ElementAt(0).Process;
            Assert.IsTrue(firstParticipantProcess.GetChildEntitiesByType<Task>().Any(), $"First {nameof(Participant)} {nameof(Process)} is has no {nameof(Task)}");
            Assert.IsFalse(firstParticipantProcess.GetChildEntitiesByType<Task>().Any(task => task.Guid == inactiveActivity.Guid), $"InActive {nameof(Activity)} is not ignored");
        }

        [TestMethod]
        public void Create_ActivityGroupWithInActiveActivities_OnlyActiveActivitiesAreConverted()
        {
            CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Activity activeActivity = activityGroup.ActivitiesIdentifiers.First(iden => iden.IdentifiedActivity.Active).IdentifiedActivity;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Create();

            Assert.IsTrue(collaboration.Participants.Any(), $"{nameof(Collaboration)} has no {nameof(Participant)}");
            Process firstParticipantProcess = collaboration.Participants.ElementAt(0).Process;
            Assert.IsTrue(firstParticipantProcess.GetChildEntitiesByType<Task>().Any(), $"First {nameof(Participant)} {nameof(Process)} is has no {nameof(Task)}");
            Assert.IsTrue(firstParticipantProcess.GetChildEntitiesByType<Task>().Any(task => task.Guid == activeActivity.Guid), $"Active {nameof(Activity)} is not converted");
        }

        [TestMethod]
        public void Create_ActivityGroupWithOnlyOneWebServicesActivity_HasTargetApplicationParticipant()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            TargetBase targetApp = solutionFacade.GetTargetApplications().First(ta => string.Equals(ta.Name, activityGroup.ActivitiesIdentifiers[0].IdentifiedActivity.TargetApplication));
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Create();

            Assert.IsTrue(collaboration.Participants.Any(participant => participant.Guid == targetApp.Guid), $"{nameof(Participant)} for {nameof(Activity)}'s {nameof(TargetApplication)} not found");
        }

        [TestMethod]
        public void Create_ActivityGroupWithOnlyOneWebServicesActivity_HasConsumerParticipant()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Consumer consumer = activityGroup.ActivitiesIdentifiers[0].IdentifiedActivity.ConsumerApplications[0];
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Create();

            Assert.IsTrue(collaboration.Participants.Any(participant => participant.Guid == consumer.ConsumerGuid), $"{nameof(Participant)} for {nameof(Activity)}'s {nameof(Consumer)} not found");
        }

        [TestMethod]
        public void Create_ActivityGroupWithOnlyOneWebServicesActivity_FirstConsumerGuidIsSetAsCollaborationSystemRef()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity restActivity = activityGroup.ActivitiesIdentifiers.First().IdentifiedActivity;
            Consumer restActivityFirstConsumer = restActivity.ConsumerApplications.First();

            Collaboration collaboration = creator.Create();

            Assert.AreEqual(
                expected: restActivityFirstConsumer.ConsumerGuid.ToString(),
                actual: collaboration.SystemRef,
                message: $"{nameof(Collaboration)}'s {nameof(Collaboration.SystemRef)} is not set to REST {nameof(Activity)}'s {nameof(Activity.Guid)}.");
        }

        [TestMethod]
        public void Create_ActivityGroupWithOnlyOneWebServicesActivity_StartEventInFirstConsumerParticipant()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity restActivity = activityGroup.ActivitiesIdentifiers.First().IdentifiedActivity;
            Consumer restActivityFirstConsumer = restActivity.ConsumerApplications.First();

            Collaboration collaboration = creator.Create();

            Participant? participantWithStartEvent = collaboration.Participants.FirstOrDefault(p => p.Process.StartEvent != null);
            Assert.IsNotNull(participantWithStartEvent, message: $"No {nameof(Participant)} with {nameof(StartEvent)} found in {nameof(Collaboration)}.");
            Assert.AreEqual(expected: restActivityFirstConsumer.ConsumerGuid, actual: participantWithStartEvent!.Guid, $"{nameof(Participant)} of REST {nameof(Activity)}'s first {nameof(Consumer)} doesn't contain {nameof(StartEvent)}.");
        }

        [TestMethod]
        public void Create_WithOnlyInactiveFlowControl_HasNoExclusiveGateway()
        {
            CreateActivityGroupWithAllInactiveFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNull(exclusiveGateway, $"{nameof(ExclusiveGateway)} shouldn't exist.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_HasConditionalTaskForActiveFlowControl()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_NoConditionalTaskForInactiveFlowControl()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);
            FlowControl inactiveFlowControl = activityWithFlowControl.Acts.SelectMany(act => act.FlowControls).First(fc => !fc.Active);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t) && string.Equals(t.Name, inactiveFlowControl.Condition));
            Assert.IsNull(conditionalTask, $"Conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithActiveAndInactiveGotoActivityFlowControl_ConditionalTaskDirectsToTargetActivityTask()
        {
            CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First(fc => fc.Active);
            Guid targetActivityGuid = flowControl.GetGuidFromValue();

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == targetActivityGuid);
            Flow? flowFromConditionalTaskToTargetActivityTask = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == targetActivityTask);
            Assert.IsNotNull(flowFromConditionalTaskToTargetActivityTask, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to target {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RerunActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RerunActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRerunActivityFlowControl_ConditionalTaskDirectsToTargetActivityTask()
        {
            CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RerunActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            Flow? flowFromConditionalTaskToTargetActivityTask = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == targetActivityTask);
            Assert.IsNotNull(flowFromConditionalTaskToTargetActivityTask, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to target {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithGoToActivityFlowControl_ConditionalTaskDirectsToTargetActivityTask()
        {
            CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivity);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First();
            Guid targetActivityGuid = flowControl.GetGuidFromValue();

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == targetActivityGuid);
            Flow? flowFromConditionalTaskToTargetActivityTask = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == targetActivityTask);
            Assert.IsNotNull(flowFromConditionalTaskToTargetActivityTask, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to target {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivityByName);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivityByName);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControl_ConditionalTaskDirectsToTargetActivityTask()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.GoToActivityByName);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First();
            string targetActivityName = flowControl.GetNameFromValue();

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => string.Equals(t.Name, targetActivityName));
            Flow? flowFromConditionalTaskToTargetActivityTask = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == targetActivityTask);
            Assert.IsNotNull(flowFromConditionalTaskToTargetActivityTask, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to target {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithGotoActivityByNameFlowControlNoTargetActivityFoundByName_ThrowsBPMNConversionException()
        {
            CreateActivityGroupWithGoToActivityByNameFlowControlWithInvalidTargetName(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Assert.ThrowsException<FlowControlTargetActivityNotFoundException>(() => creator.Create(), $"Expected to throw {nameof(BPMNConversionException)} because no {nameof(Activity)} was found by name in shared repository.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.StopBusinessFlow);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found in process.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.StopBusinessFlow);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            Flow? flowFromFlowControlTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(flowFromFlowControlTaskToGateway, $"No Outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            Flow? flowFromFlowControlTaskToGateway = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromFlowControlTaskToGateway, $"No Outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} is found.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_HasEndEventOfTypeTermination()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            EndEvent? endEvent = process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Termination);
            Assert.IsNotNull(endEvent, $"No {nameof(EndEvent)} found with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Termination)}.");
        }

        [TestMethod]
        public void Create_WithStopBusinessFlowFlowControl_ConditionalTaskDirectsToTerminationEndEvent()
        {
            CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            EndEvent terminationEndEvent = process.EndEvents.First(e => e.EndEventType == EndEventType.Termination);
            Flow? flowFromConditionalTaskToEndEvent = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == terminationEndEvent);
            Assert.IsNotNull(flowFromConditionalTaskToEndEvent, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to {nameof(EndEvent)} with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Termination)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_HasTaskForActivityWithFlowControl()
        {

            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.StopRun);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.StopRun);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(flowFromTaskToGateway, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromTaskToGateway, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.First(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_HasEndEventOfTypeTermination()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            EndEvent? terminationEndEvent = process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Termination);
            Assert.IsNotNull(terminationEndEvent, $"No {nameof(EndEvent)} found with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Termination)}.");
        }

        [TestMethod]
        public void Create_WithStopRunFlowControl_ConditionalTaskDirectsToTerminationEndEvent()
        {
            CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            EndEvent? terminationEndEvent = process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Termination);
            Flow? flowFromConditionalTaskToEndEvent = conditionalTask.OutgoingFlows.First(f => f.Target == terminationEndEvent);
            Assert.IsNotNull(flowFromConditionalTaskToEndEvent, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to {nameof(EndEvent)} with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Termination)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.FailActionAndStopBusinessFlow);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.FailActionAndStopBusinessFlow);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to {nameof(Task)} of next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_HasEndEventOfTypeError()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            EndEvent? errorEndEvent = process.EndEvents.FirstOrDefault(e => e.EndEventType == EndEventType.Error);
            Assert.IsNotNull(errorEndEvent, $"No {nameof(EndEvent)} found with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Error)}.");
        }

        [TestMethod]
        public void Create_WithFailActionAndStopBusinessFlowFlowControl_ConditionalTaskDirectsToErrorEndEvent()
        {
            CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            EndEvent errorEndEvent = process.EndEvents.First(e => e.EndEventType == EndEventType.Error);
            Flow? flowFromConditionalTaskToEndEvent = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == errorEndEvent);
            Assert.IsNotNull(flowFromConditionalTaskToEndEvent, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to {nameof(EndEvent)} with {nameof(EndEvent.EndEventType)} of type {nameof(EndEventType.Error)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_HasExclusiveGateway()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway? exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().FirstOrDefault();
            Assert.IsNotNull(exclusiveGateway, $"No {nameof(ExclusiveGateway)} found.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_HasTaskForActivityWithFlowControl()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RunSharedRepositoryActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == activityWithFlowControl.Guid);
            Assert.IsNotNull(taskForActivityWithFlowControl, $"No {nameof(Task)} found for {nameof(Activity)} with {nameof(FlowControl)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_TaskOfActivityWithFlowControlDirectsToExclusiveGateway()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RunSharedRepositoryActivity);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task taskForActivityWithFlowControl = process.GetChildEntitiesByType<Task>().First(t => t.Guid == activityWithFlowControl.Guid);
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Flow? flowFromTaskToGateway = taskForActivityWithFlowControl.OutgoingFlows.FirstOrDefault(f => f.Target == exclusiveGateway);
            Assert.IsNotNull(flowFromTaskToGateway, $"No outgoing {nameof(Flow)} found from {nameof(Task)} of {nameof(Activity)} with {nameof(FlowControl)} to {nameof(ExclusiveGateway)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_HasTaskForNextActivity()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? nextActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => t.Guid == nextActivity.Guid);
            Assert.IsNotNull(nextActivityTask, $"No {nameof(Task)} found for next {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_ExclusiveGatewayDirectsToNextActivityTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            Flow? flowFromGatewayToNextActivityTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromGatewayToNextActivityTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to next {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_HasConditionalTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? conditionalTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => IsConditionalTask(t));
            Assert.IsNotNull(conditionalTask, $"No conditional {nameof(Task)} found.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_ExclusiveGatewayDirectsToConditionalTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            ExclusiveGateway exclusiveGateway = process.GetChildEntitiesByType<ExclusiveGateway>().First();
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Flow? flowFromGatewayToConditionalTask = exclusiveGateway.OutgoingFlows.FirstOrDefault(f => f.Target == conditionalTask);
            Assert.IsNotNull(flowFromGatewayToConditionalTask, $"No outgoing {nameof(Flow)} found from {nameof(ExclusiveGateway)} to conditional {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_HasTargetActivityTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RunSharedRepositoryActivity);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First();
            string targetActivityName = flowControl.GetNameFromValue();

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task? targetActivityTask = process.GetChildEntitiesByType<Task>().FirstOrDefault(t => string.Equals(t.Name, targetActivityName));
            Assert.IsNotNull(targetActivityTask, $"No {nameof(Task)} found for target {nameof(Activity)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_ConditionalTaskDirectsToTargetActivityTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RunSharedRepositoryActivity);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First();
            string targetActivityName = flowControl.GetNameFromValue();

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task conditionalTask = process.GetChildEntitiesByType<Task>().First(t => IsConditionalTask(t));
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => string.Equals(t.Name, targetActivityName));
            Flow? flowFromConditionalTaskToTargetActivityTask = conditionalTask.OutgoingFlows.FirstOrDefault(f => f.Target == targetActivityTask);
            Assert.IsNotNull(flowFromConditionalTaskToTargetActivityTask, $"No outgoing {nameof(Flow)} found from conditional {nameof(Task)} to target {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControl_TargetActivityTaskDirectsToNextActivityTask()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);
            Activity activityWithFlowControl = GetActivityWithFlowControlActionType(activityGroup, eFlowControlAction.RunSharedRepositoryActivity);
            FlowControl flowControl = activityWithFlowControl.Acts.SelectMany(a => a.FlowControls).First();
            string targetActivityName = flowControl.GetNameFromValue();
            Activity nextActivity = activityGroup.ActivitiesIdentifiers[1].IdentifiedActivity;

            Collaboration collaboration = creator.Create();

            Process process = collaboration.Participants.First().Process;
            Task targetActivityTask = process.GetChildEntitiesByType<Task>().First(t => string.Equals(t.Name, targetActivityName));
            Task nextActivityTask = process.GetChildEntitiesByType<Task>().First(t => t.Guid == nextActivity.Guid);
            Flow? flowFromTargetActivityTaskToNextActivityTask = targetActivityTask.OutgoingFlows.FirstOrDefault(f => f.Target == nextActivityTask);
            Assert.IsNotNull(flowFromTargetActivityTaskToNextActivityTask, $"No outgoing {nameof(Flow)} found from target {nameof(Activity)} {nameof(Task)} to next {nameof(Activity)} {nameof(Task)}.");
        }

        [TestMethod]
        public void Create_WithRunSharedRepositoryActivityFlowControlNoTargetActivityFoundByName_ThrowsBPMNConversionException()
        {
            CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControlWithInvalidTargetName(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            CollaborationFromActivityGroupCreator creator = new(activityGroup, solutionFacade);

            Assert.ThrowsException<FlowControlTargetActivityNotFoundException>(() => creator.Create(), $"Expected to throw {nameof(BPMNConversionException)} because no {nameof(Activity)} was found by name in shared repository.");
        }

        private bool IsConditionalTask(Task task)
        {
            return task.Conditions.Any() || task.Name.StartsWith(ConditionalTaskNamePrefix);
        }

        private void CreateActivityGroupWithRerunActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.RerunActivity,
                                Active = true,
                                Condition = "true",
                                Operator = eFCOperator.CSharp
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithGoToActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivity,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp,
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Activity activity3 = new()
            {
                ActivityName = "Activitiy 3",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity3 });
            activity1.Acts[0].FlowControls[0].Value = activity3.Guid + new FlowControl().GUID_NAME_SEPERATOR + activity3.ActivityName;
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activity3]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithGoToActivityByNameFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivityByName,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Activity activity3 = new()
            {
                ActivityName = "Activitiy 3",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity3 });
            activity1.Acts[0].FlowControls[0].Value = activity3.ActivityName;
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activity3]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithGoToActivityByNameFlowControlWithInvalidTargetName(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivityByName,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp,
                                Value = "NonExistentActivityName"
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Activity activity3 = new()
            {
                ActivityName = "Activitiy 3",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity3 });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activity3]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];

            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.StopBusinessFlow,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });

            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithStopRunFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];

            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.StopRun,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });

            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithFailActionAndStopBusinessFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];

            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.FailActionAndStopBusinessFlow,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });

            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            Activity activityFromSharedRepository = new()
            {
                ActivityName = "ActivityFromSharedRepository",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };

            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.RunSharedRepositoryActivity,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp,
                                Value = activityFromSharedRepository.ActivityName
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });


            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activityFromSharedRepository]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithRunSharedRepositoryActivityFlowFlowControlWithInvalidTargetName(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];

            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.RunSharedRepositoryActivity,
                                Condition = "true",
                                Active = true,
                                Operator = eFCOperator.CSharp,
                                Value = "NonExistentActivityName"
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });


            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithAllInactiveFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivity,
                                Active = false,
                                Condition = "true",
                                Operator = eFCOperator.CSharp,
                            }
                        ]
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Activity activity3 = new()
            {
                ActivityName = "Activitiy 3",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity3 });
            activity1.Acts[0].FlowControls[0].Value = activity3.Guid + new FlowControl().GUID_NAME_SEPERATOR + activity3.ActivityName;
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activity3]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private void CreateActivityGroupWithActiveAndInactiveGotoActivityFlowControl(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                }
            ];
            activityGroup = new();
            Activity activity1 = new()
            {
                ActivityName = "Activity 1",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName,
                Acts =
                [
                    new ActDummy()
                    {
                        FlowControls =
                        [
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivity,
                                Active = false,
                                Condition = "false",
                                Operator = eFCOperator.CSharp,
                            },
                            new FlowControl()
                            {
                                FlowControlAction = eFlowControlAction.GoToActivity,
                                Active = true,
                                Condition = "true",
                                Operator = eFCOperator.CSharp,
                            }
                        ],
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity1 });
            Activity activity2 = new()
            {
                ActivityName = "Activitiy 2",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity2 });
            Activity activity3 = new()
            {
                ActivityName = "Activitiy 3",
                Active = true,
                TargetApplication = applicationPlatforms[0].AppName
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activity3 });
            activity1.Acts[0].FlowControls[0].Value = activity3.Guid + new FlowControl().GUID_NAME_SEPERATOR + activity3.ActivityName;
            activity1.Acts[0].FlowControls[1].Value = activity2.Guid + new FlowControl().GUID_NAME_SEPERATOR + activity2.ActivityName;
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activity1, activity2, activity3]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;
        }

        private Activity GetActivityWithFlowControlActionType(ActivitiesGroup activityGroup, eFlowControlAction flowControlAction)
        {
            IEnumerable<Activity> activitiesInGroup = activityGroup.ActivitiesIdentifiers.Select(iden => iden.IdentifiedActivity);
            return
                activitiesInGroup.First(activity =>
                    activity.Acts.SelectMany(act => act.FlowControls).Any(fc => fc.FlowControlAction == flowControlAction));
        }

        private bool SequenceFlowReachesTargetProcessEntity(SequenceFlow sequenceFlow, IProcessEntity targetProcessEntity)
        {
            return SequenceFlowReachesTargetProcessEntity(sequenceFlow, targetProcessEntity, []);
        }

        private bool SequenceFlowReachesTargetProcessEntity(SequenceFlow sequenceFlow, IProcessEntity targetProcessEntity, Dictionary<SequenceFlow, bool> visitationStatus)
        {
            bool isSequenceFlowAlreadyVisited = visitationStatus.ContainsKey(sequenceFlow) && visitationStatus[sequenceFlow];
            if (isSequenceFlowAlreadyVisited)
            {
                return false;
            }
            else
            {
                visitationStatus[sequenceFlow] = true;
            }

            if (sequenceFlow.Target == targetProcessEntity)
            {
                return true;
            }

            if (sequenceFlow.Target is IFlowSource flowSource)
            {
                IEnumerable<SequenceFlow> nextSequenceFlows = flowSource.OutgoingFlows.Where(of => of is SequenceFlow).Cast<SequenceFlow>();
                return nextSequenceFlows.Any(sf => SequenceFlowReachesTargetProcessEntity(sf, targetProcessEntity, visitationStatus));
            }

            return false;
        }

        private void CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                },
                new ApplicationPlatform()
                {
                    AppName = "MyWebServicesApp",
                    Platform = ePlatformType.WebServices
                }
            ];
            activityGroup = new();
            Activity activeActivity = new()
            {
                ActivityName = "Active Activity",
                TargetApplication = applicationPlatforms[0].AppName,
                Active = true
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = activeActivity });
            Activity inactiveActivity = new()
            {
                ActivityName = "Inactive Activitiy",
                TargetApplication = applicationPlatforms[1].AppName,
                Active = false
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers() { IdentifiedActivity = inactiveActivity });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([activeActivity, inactiveActivity]);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(
            [
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName },
                new TargetApplication() { Guid = applicationPlatforms[1].Guid, AppName = applicationPlatforms[1].AppName }
            ]);
            solutionFacade = solutionFacadeMock.Object;

        }

        private void CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            ObservableList<ApplicationPlatform> applicationPlatforms =
            [
                new ApplicationPlatform()
                {
                    AppName = "MyWebApp",
                    Platform = ePlatformType.Web
                },
                new ApplicationPlatform()
                {
                    AppName = "MyWebServices",
                    Platform = ePlatformType.WebServices
                }
            ];
            ObservableList<TargetBase> targetApplications =
            [
                new TargetApplication()
                {
                    Guid = applicationPlatforms[0].Guid,
                    AppName = applicationPlatforms[0].AppName
                },
                new TargetApplication()
                {
                    Guid = applicationPlatforms[1].Guid,
                    AppName = applicationPlatforms[1].AppName
                }
            ];
            activityGroup = new();
            Activity restActivity = new()
            {
                ActivityName = "REST Activity",
                TargetApplication = targetApplications[1].Name,
                Active = true,
                ConsumerApplications =
                [
                    new Consumer()
                    {
                        ConsumerGuid = targetApplications[0].Guid
                    }
                ]
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
            {
                IdentifiedActivity = restActivity
            });
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns([restActivity]);
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(targetApplications);
            solutionFacade = solutionFacadeMock.Object;

        }
    }
}
