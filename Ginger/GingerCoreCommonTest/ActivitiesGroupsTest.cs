using GingerCore;
using GingerCore.Activities;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GingerCoreCommonTest
{

    [TestClass]
    [Level2]
    public class ActivitiesGroupsTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            //TODO::
        }

        [TestMethod]
        public void AddDefualtGroupTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();
            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            busFlow.AddActivity(activity1);
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            busFlow.AddActivity(activity2);
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            busFlow.AddActivity(activity3);
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            busFlow.AddActivity(activity4);
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            busFlow.AddActivity(activity5);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups.Count == 1, "Validate defualt Activities Group was added");
            Assert.IsTrue(busFlow.ActivitiesGroups[0].ActivitiesIdentifiers.Count == 5, "Validate all Activities are attached to defualt Activities Group");
            Assert.IsTrue(busFlow.ActivitiesGroups[0].ActivitiesIdentifiers[2].ActivityName == activity3.ActivityName, "Validate group has correct activities mapped to it");
            Assert.IsTrue(activity1.ActivitiesGroupID == busFlow.ActivitiesGroups[0].Name, "Validate Activities mapped to defualt group");
            Assert.IsTrue(activity5.ActivitiesGroupID == busFlow.ActivitiesGroups[0].Name, "Validate Activities mapped to defualt group");
        }

        [TestMethod]
        public void GroupUnGroupedActivitiesTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();
            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            busFlow.AddActivity(activity1);
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            busFlow.AddActivity(activity2);
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            busFlow.AddActivity(activity3);
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            busFlow.AddActivity(activity4);
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            busFlow.AddActivity(activity5);
            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            group1.AddActivityToGroup(activity1);
            group2.AddActivityToGroup(activity4);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups.Count == 2, "Validate no extra groups were added");
            Assert.IsTrue(group1.ActivitiesIdentifiers.Count == 3, "Validate first three Activities were attached to first group");
            Assert.IsTrue(group2.ActivitiesIdentifiers.Count == 2, "Validate last two Activities were attached to second group");
            Assert.IsTrue(group1.ActivitiesIdentifiers[2].ActivityName == activity3.ActivityName, "Validate group 1 has correct activities mapped to it");
            Assert.IsTrue(group2.ActivitiesIdentifiers[1].ActivityName == activity5.ActivityName, "Validate group 2 has correct activities mapped to it");
            Assert.IsTrue(activity2.ActivitiesGroupID == group1.Name, "Validate Activities mapped to group 1");
            Assert.IsTrue(activity5.ActivitiesGroupID == group2.Name, "Validate Activities mapped to group 2");
        }

        [TestMethod]
        public void CreateNewGroupToUnSyncedGroupedActivitiesTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();
            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            busFlow.AddActivity(activity1);
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            busFlow.AddActivity(activity2);
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            busFlow.AddActivity(activity3);
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            busFlow.AddActivity(activity4);
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            busFlow.AddActivity(activity5);

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);

            group1.AddActivityToGroup(activity1);
            group1.AddActivityToGroup(activity2);
            group2.AddActivityToGroup(activity3);
            group1.AddActivityToGroup(activity4);
            group1.AddActivityToGroup(activity5);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups.Count == 3, "Validate 1 extra groups were added");
            Assert.IsTrue(group1.ActivitiesIdentifiers.Count == 2, "Validate only 2 Activities are attached to first group");
            Assert.IsTrue(group2.ActivitiesIdentifiers.Count == 1, "Validate 1 Activities were attached to second group");
            Assert.IsTrue(busFlow.ActivitiesGroups.Where(x => x.Name == group1.Name + "_2").FirstOrDefault() != null, "Validate new added group name");           
            Assert.IsTrue(activity4.ActivitiesGroupID == group1.Name + "_2", "Validate Activity was re-grouped to new added group");
            Assert.IsTrue(activity5.ActivitiesGroupID == group1.Name + "_2", "Validate Activity was re-grouped to new added group");
        }


        [TestMethod]
        public void GroupsOrderTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();
            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            busFlow.AddActivity(activity1);
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            busFlow.AddActivity(activity2);
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            busFlow.AddActivity(activity3);
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            busFlow.AddActivity(activity4);
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            busFlow.AddActivity(activity5);

            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);

            group1.AddActivityToGroup(activity1);
            group1.AddActivityToGroup(activity2);
            group2.AddActivityToGroup(activity3);
            group3.AddActivityToGroup(activity4);
            group3.AddActivityToGroup(activity5);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups[0] == group1, "Validate first group is group 1 same as in Activities flow");
            Assert.IsTrue(busFlow.ActivitiesGroups[1] == group2, "Validate first group is group 2 same as in Activities flow");
            Assert.IsTrue(busFlow.ActivitiesGroups[2] == group3, "Validate first group is group 3 same as in Activities flow");
        }

        [TestMethod]
        public void GroupsActivitiesOrderTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();
            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            busFlow.AddActivity(activity1);
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            busFlow.AddActivity(activity2);
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            busFlow.AddActivity(activity3);
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            busFlow.AddActivity(activity4);
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            busFlow.AddActivity(activity5);

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);

            group1.AddActivityToGroup(activity3);
            group1.AddActivityToGroup(activity2);
            group1.AddActivityToGroup(activity1);
            group2.AddActivityToGroup(activity4);
            group2.AddActivityToGroup(activity5);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();

            //Assert
            Assert.IsTrue(group1.ActivitiesIdentifiers[0].IdentifiedActivity == activity1, "Validate group 1 Activity 1 order is same as in Activities flow");
            Assert.IsTrue(group1.ActivitiesIdentifiers[1].IdentifiedActivity == activity2, "Validate group 1 Activity 2 order is same as in Activities flow");
            Assert.IsTrue(group1.ActivitiesIdentifiers[2].IdentifiedActivity == activity3, "Validate group 1 Activity 3 order is same as in Activities flow");
            Assert.IsTrue(group2.ActivitiesIdentifiers[0].IdentifiedActivity == activity4, "Validate group 2 Activity 4 order is same as in Activities flow");
            Assert.IsTrue(group2.ActivitiesIdentifiers[1].IdentifiedActivity == activity5, "Validate group 2 Activity 5 order is same as in Activities flow");
        }
    }
}
