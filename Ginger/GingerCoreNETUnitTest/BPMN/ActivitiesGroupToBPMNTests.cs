using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.BPMN;
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
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.BPMN
{
    [TestClass]
    public sealed class ActivitiesGroupToBPMNTests
    {
        [TestMethod]
        public void Convert_NullActivityGroup_ThrowsArgumentNullException()
        {
            ActivitiesGroup activityGroup = null;
            ISolutionFacadeForBPMN solutionFacade = new Mock<ISolutionFacadeForBPMN>().Object;

            Assert.ThrowsException<ArgumentNullException>(() => new ActivitiesGroupToBPMNConverter(activityGroup, solutionFacade));
        }

        [TestMethod]
        public void Convert_EmptyActivitesGroup_ThrowsBPMNConversionException()
        {
            ActivitiesGroup activityGroup = new();
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>());
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Convert());
        }

        [TestMethod]
        public void Convert_AllInactiveActivities_ThrowsBPMNConversionException()
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
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Convert());
        }

        [TestMethod]
        public void Convert_AllNonIdentifiedActivities_ThrowsBPMNConversionException()
        {
            ActivitiesGroup activityGroup = new();
            activityGroup.ActivitiesIdentifiers.Add(new ActivityIdentifiers()
            {
                IdentifiedActivity = null
            });
            Mock<ISolutionFacadeForBPMN> solutionFacadeMock = new();
            solutionFacadeMock.Setup(sf => sf.GetActivitiesFromSharedRepository()).Returns(new ObservableList<Activity>());
            ISolutionFacadeForBPMN solutionFacade = solutionFacadeMock.Object;
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Assert.ThrowsException<BPMNConversionException>(() => converter.Convert());
        }

        [TestMethod]
        public void Convert_ActivityGroupWithInActiveActivities_InactiveActivitiesAreIgnored()
        {
            CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Activity inactiveActivity = activityGroup.ActivitiesIdentifiers.First(iden => iden.IdentifiedActivity.Active == false).IdentifiedActivity;
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Convert();

            Assert.IsTrue(collaboration.Participants.Any(), $"{nameof(Collaboration)} has no {nameof(Participant)}");
            Process firstParticipantProcess = collaboration.Participants.ElementAt(0).Process;
            Assert.IsTrue(firstParticipantProcess.Tasks.Any(), $"First {nameof(Participant)} {nameof(Process)} is has no {nameof(Amdocs.Ginger.CoreNET.BPMN.Task)}");
            Assert.IsFalse(firstParticipantProcess.Tasks.Any(task => string.Equals(task.Guid, inactiveActivity.Guid.ToString())), $"InActive {nameof(Activity)} is not ignored");
        }

        [TestMethod]
        public void Convert_ActivityGroupWithInActiveActivities_OnlyActiveActivitiesAreConverted()
        {
            CreateActivityGroupWithActiveAndInactiveActivities(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Activity activeActivity = activityGroup.ActivitiesIdentifiers.First(iden => iden.IdentifiedActivity.Active == true).IdentifiedActivity;
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Convert();

            Assert.IsTrue(collaboration.Participants.Any(), $"{nameof(Collaboration)} has no {nameof(Participant)}");
            Process firstParticipantProcess = collaboration.Participants.ElementAt(0).Process;
            Assert.IsTrue(firstParticipantProcess.Tasks.Any(), $"First {nameof(Participant)} {nameof(Process)} is has no {nameof(Amdocs.Ginger.CoreNET.BPMN.Task)}");
            Assert.IsTrue(firstParticipantProcess.Tasks.Any(task => string.Equals(task.Guid, activeActivity.Guid.ToString())), $"Active {nameof(Activity)} is not converted");
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
        public void Convert_ActivityWithOnlyOneWebServicesActivity_HasTargetApplicationParticipant()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            TargetBase targetApp = solutionFacade.GetTargetApplications().First(ta => string.Equals(ta.Name, activityGroup.ActivitiesIdentifiers[0].IdentifiedActivity.TargetApplication));
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Convert();

            Assert.IsTrue(collaboration.Participants.Any(participant => string.Equals(participant.Guid, targetApp.Guid.ToString())), $"{nameof(Participant)} for {nameof(Activity)}'s {nameof(TargetApplication)} not found");
        }

        [TestMethod]
        public void Convert_ActivityWithOnlyOneWebServicesActivity_HasConsumerParticipant()
        {
            CreateActivityGroupWithOnlyOneWebServicesActivity(out ActivitiesGroup activityGroup, out ISolutionFacadeForBPMN solutionFacade);
            Consumer consumer = activityGroup.ActivitiesIdentifiers[0].IdentifiedActivity.ConsumerApplications[0];
            ActivitiesGroupToBPMNConverter converter = new(activityGroup, solutionFacade);

            Collaboration collaboration = converter.Convert();

            Assert.IsTrue(collaboration.Participants.Any(participant => string.Equals(participant.Guid, consumer.ConsumerGuid.ToString())), $"{nameof(Participant)} for {nameof(Activity)}'s {nameof(Consumer)} not found");
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
