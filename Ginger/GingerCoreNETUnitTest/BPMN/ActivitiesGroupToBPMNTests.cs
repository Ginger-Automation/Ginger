using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN.Exceptions;
using Amdocs.Ginger.CoreNET.BPMN.Models;
using Amdocs.Ginger.CoreNET.BPMN.Serialization;
using Amdocs.Ginger.Repository;
using Applitools.Utils;
using DocumentFormat.OpenXml.Wordprocessing;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#nullable enable
namespace GingerCoreNETUnitTest.BPMN
{
    [TestClass]
    public sealed class ActivitiesGroupToBPMNTests
    {
        [TestMethod]
        public void Create_NullActivityGroup_ThrowsArgumentNullException()
        {
            ActivitiesGroup activityGroup = null;
            ISolutionFacadeForBPMN solutionFacade = new Mock<ISolutionFacadeForBPMN>().Object;

            Assert.ThrowsException<ArgumentNullException>(() => new CollaborationFromActivityGroupCreator(activityGroup, solutionFacade));
        }

        [TestMethod]
        public void Create_EmptyActivitesGroup_ThrowsBPMNConversionException()
        {
            ActivitiesGroup activityGroup = new();
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>());
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Create());
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
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>() { inactiveActivity });
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Create());
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
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>());
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            CollaborationFromActivityGroupCreator converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Create());
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

        private void CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            ObservableList<ApplicationPlatform> applicationPlatforms = new()
            {
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
            };
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
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>() { activeActivity, inactiveActivity });
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(new ObservableList<TargetBase>()
            {
                new TargetApplication() { Guid = applicationPlatforms[0].Guid, AppName = applicationPlatforms[0].AppName },
                new TargetApplication() { Guid = applicationPlatforms[1].Guid, AppName = applicationPlatforms[1].AppName }
            });
            solutionFacade = solutionFacadeMock.Object;
            
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

        private void CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade)
        {
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            ObservableList<ApplicationPlatform> applicationPlatforms = new()
            {
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
            };
            ObservableList<TargetBase> targetApplications = new()
            {
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
            };
            activityGroup = new();
            Activity restActivity = new()
            {
                ActivityName = "REST Activity",
                TargetApplication = targetApplications[1].Name,
                Active = true,
                ConsumerApplications = new ObservableList<Consumer>()
                {
                    new Consumer()
                    {
                        ConsumerGuid = targetApplications[0].Guid
                    }
                }
            };
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
            {
                IdentifiedActivity = restActivity
            });
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>() { restActivity });
            solutionFacadeMock.Setup(sf => sf.GetApplicationPlatforms()).Returns(applicationPlatforms);
            solutionFacadeMock.Setup(sf => sf.GetTargetApplications()).Returns(targetApplications);
            solutionFacade = solutionFacadeMock.Object;
            
        }
    }
}
