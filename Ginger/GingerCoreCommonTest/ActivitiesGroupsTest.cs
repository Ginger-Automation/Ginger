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

        [TestMethod]
        public void MoveActivitiesGroupUpTest()
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
            Activity activity6 = new Activity() { ActivityName = "Activity6" };
            busFlow.AddActivity(activity6);
            Activity activity7 = new Activity() { ActivityName = "Activity7" };
            busFlow.AddActivity(activity7);

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            group1.AddActivityToGroup(activity1);
            group1.AddActivityToGroup(activity2);
            group2.AddActivityToGroup(activity3);
            group2.AddActivityToGroup(activity4);
            group2.AddActivityToGroup(activity5);
            group3.AddActivityToGroup(activity6);
            group3.AddActivityToGroup(activity7);

            //Act
            busFlow.MoveActivitiesGroupUp(group3);

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups[1] == group3, "Validate group 3 moved to place 2");
            Assert.IsTrue(busFlow.ActivitiesGroups[2] == group2, "Validate group 2 moved to place 3");
            Assert.IsTrue(busFlow.Activities[2] == activity6, "Validate group 3 Activities moved up");
            Assert.IsTrue(busFlow.Activities[3] == activity7, "Validate group 3 Activities moved up");
            Assert.IsTrue(busFlow.Activities[4] == activity3, "Validate group 2 Activities moved down");
            Assert.IsTrue(busFlow.Activities[5] == activity4, "Validate group 2 Activities moved down");
            Assert.IsTrue(busFlow.Activities[6] == activity5, "Validate group 2 Activities moved down");
        }

        [TestMethod]
        public void MoveActivitiesGroupDownTest()
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
            Activity activity6 = new Activity() { ActivityName = "Activity6" };
            busFlow.AddActivity(activity6);

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            group1.AddActivityToGroup(activity1);
            group1.AddActivityToGroup(activity2);
            group2.AddActivityToGroup(activity3);
            group2.AddActivityToGroup(activity4);
            group2.AddActivityToGroup(activity5);
            group3.AddActivityToGroup(activity6);

            //Act
            busFlow.MoveActivitiesGroupDown(group1);

            //Assert
            Assert.IsTrue(busFlow.ActivitiesGroups[0] == group2, "Validate group 2 moved to place 1");
            Assert.IsTrue(busFlow.ActivitiesGroups[1] == group1, "Validate group 1 moved to place 2");
            Assert.IsTrue(busFlow.Activities[0] == activity3, "Validate group 2 Activities moved up");
            Assert.IsTrue(busFlow.Activities[1] == activity4, "Validate group 2 Activities moved up");
            Assert.IsTrue(busFlow.Activities[2] == activity5, "Validate group 2 Activities moved up");
            Assert.IsTrue(busFlow.Activities[3] == activity1, "Validate group 1 Activities moved down");
            Assert.IsTrue(busFlow.Activities[4] == activity2, "Validate group 1 Activities moved down");
        }

        [TestMethod]
        public void AddActivityWithNewGroupTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };           
            Activity activity2 = new Activity() { ActivityName = "Activity2" };           
            Activity activity3 = new Activity() { ActivityName = "Activity3" };            
            Activity activity4 = new Activity() { ActivityName = "Activity4" };          
            Activity activity5 = new Activity() { ActivityName = "Activity5" };                      

            busFlow.AddActivity(activity1, group1);
            busFlow.AddActivity(activity2, group1);
            busFlow.AddActivity(activity3, group1);

            busFlow.AddActivity(activity4, group2);
            busFlow.AddActivity(activity5, group2);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);
            Activity activity6 = new Activity() { ActivityName = "Activity6" };
            busFlow.AddActivity(activity6, group3);

            //Assert
            Assert.IsTrue(busFlow.Activities[5] == activity6, "Validate new Activity added in last");
            Assert.IsTrue(busFlow.ActivitiesGroups[2] == group3, "Validate new group was added to BF");
            Assert.IsTrue(activity6.ActivitiesGroupID == group3.Name, "Validate new Activity is mapped to new group");           
            Assert.IsTrue(group3.ActivitiesIdentifiers[0].IdentifiedActivity == activity6, "Validate new Activity is mapped to new group");
        }

        [TestMethod]
        public void AddActivityToExistingGroupTest()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };

            busFlow.AddActivity(activity1, group1);
            busFlow.AddActivity(activity2, group1);
            busFlow.AddActivity(activity3, group1);

            busFlow.AddActivity(activity4, group2);
            busFlow.AddActivity(activity5, group2);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            Activity activity6 = new Activity() { ActivityName = "Activity6" };
            busFlow.AddActivity(activity6, group1);

            //Assert
            Assert.IsTrue(busFlow.Activities[3] == activity6, "Validate new Activity added in last of existing group Activities");            
            Assert.IsTrue(activity6.ActivitiesGroupID == group1.Name, "Validate new Activity is mapped to existing group");
            Assert.IsTrue(group1.ActivitiesIdentifiers[group1.ActivitiesIdentifiers.Count -1].IdentifiedActivity == activity6, "Validate new Activity is mapped to existing group");
        }

        [TestMethod]
        public void MoveActivityBetweenGroupsTest1()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            Activity activity6 = new Activity() { ActivityName = "Activity6" };

            busFlow.AddActivity(activity1, group1);
            busFlow.AddActivity(activity2, group1);

            busFlow.AddActivity(activity3, group2);
            busFlow.AddActivity(activity4, group2);

            busFlow.AddActivity(activity5, group3);
            busFlow.AddActivity(activity6, group3);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            busFlow.MoveActivityBetweenGroups(activity2, group2);

            //Assert
            Assert.IsTrue(group1.ActivitiesIdentifiers.Where(x=>x.IdentifiedActivity== activity2).FirstOrDefault() == null, "Validate Activity removed from original group");
            Assert.IsTrue(activity2.ActivitiesGroupID == group2.Name, "Validate Activity moved to target group");
            Assert.IsTrue(busFlow.Activities.IndexOf(activity2) == 3, "Validate Activity moved to correct index in Activities list");            
            Assert.IsTrue(group2.ActivitiesIdentifiers.IndexOf(group2.ActivitiesIdentifiers.Where(x=>x.IdentifiedActivity == activity2).First()) == 2, "Validate Activity moved to correct indx in target group");
        }

        [TestMethod]
        public void MoveActivityBetweenGroupsTest2()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            Activity activity6 = new Activity() { ActivityName = "Activity6" };

            busFlow.AddActivity(activity1, group1);
            busFlow.AddActivity(activity2, group1);

            busFlow.AddActivity(activity3, group2);
            busFlow.AddActivity(activity4, group2);

            busFlow.AddActivity(activity5, group3);
            busFlow.AddActivity(activity6, group3);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            busFlow.MoveActivityBetweenGroups(activity2, group2, 2);

            //Assert
            Assert.IsTrue(group1.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity2).FirstOrDefault() == null, "Validate Activity removed from original group");
            Assert.IsTrue(activity2.ActivitiesGroupID == group2.Name, "Validate Activity moved to target group");
            Assert.IsTrue(busFlow.Activities.IndexOf(activity2) == 1, "Validate Activity moved to correct index in Activities list");
            Assert.IsTrue(group2.ActivitiesIdentifiers.IndexOf(group2.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity2).First()) == 0, "Validate Activity moved to correct indx in target group");
        }

        [TestMethod]
        public void MoveActivityBetweenGroupsTest3()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            Activity activity6 = new Activity() { ActivityName = "Activity6" };

            busFlow.AddActivity(activity1, group1);
            busFlow.AddActivity(activity2, group1);

            busFlow.AddActivity(activity3, group2);
            busFlow.AddActivity(activity4, group2);

            busFlow.AddActivity(activity5, group3);
            busFlow.AddActivity(activity6, group3);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            busFlow.MoveActivityBetweenGroups(activity6, group2, 2);

            //Assert
            Assert.IsTrue(group3.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity6).FirstOrDefault() == null, "Validate Activity removed from original group");
            Assert.IsTrue(activity6.ActivitiesGroupID == group2.Name, "Validate Activity moved to target group");
            Assert.IsTrue(busFlow.Activities.IndexOf(activity6) == 2, "Validate Activity moved to correct index in Activities list");
            Assert.IsTrue(group2.ActivitiesIdentifiers.IndexOf(group2.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity6).First()) == 0, "Validate Activity moved to correct indx in target group");
        }


        [TestMethod]
        public void MoveActivityInGroupsTest1()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            Activity activity6 = new Activity() { ActivityName = "Activity6" };

            busFlow.AddActivity(activity1, group1);

            busFlow.AddActivity(activity2, group2);
            busFlow.AddActivity(activity3, group2);
            busFlow.AddActivity(activity4, group2);
            busFlow.AddActivity(activity5, group2);

            busFlow.AddActivity(activity6, group3);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            busFlow.MoveActivityInGroup(activity2, 3);

            //Assert
            Assert.IsTrue(busFlow.Activities.IndexOf(activity2) == 3, "Validate Activity moved to correct index in Activities list");
            Assert.IsTrue(group2.ActivitiesIdentifiers.IndexOf(group2.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity2).First()) == 2, "Validate Activity moved to correct indx in target group");
        }

        [TestMethod]
        public void MoveActivityInGroupsTest2()
        {
            //Arrange
            BusinessFlow busFlow = new BusinessFlow();

            ActivitiesGroup group1 = new ActivitiesGroup() { Name = "Group1" };
            busFlow.AddActivitiesGroup(group1);
            ActivitiesGroup group2 = new ActivitiesGroup() { Name = "Group2" };
            busFlow.AddActivitiesGroup(group2);
            ActivitiesGroup group3 = new ActivitiesGroup() { Name = "Group3" };
            busFlow.AddActivitiesGroup(group3);

            Activity activity1 = new Activity() { ActivityName = "Activity1" };
            Activity activity2 = new Activity() { ActivityName = "Activity2" };
            Activity activity3 = new Activity() { ActivityName = "Activity3" };
            Activity activity4 = new Activity() { ActivityName = "Activity4" };
            Activity activity5 = new Activity() { ActivityName = "Activity5" };
            Activity activity6 = new Activity() { ActivityName = "Activity6" };

            busFlow.AddActivity(activity1, group1);

            busFlow.AddActivity(activity2, group2);
            busFlow.AddActivity(activity3, group2);
            busFlow.AddActivity(activity4, group2);
            busFlow.AddActivity(activity5, group2);

            busFlow.AddActivity(activity6, group3);

            //Act
            busFlow.AttachActivitiesGroupsAndActivities();
            busFlow.MoveActivityInGroup(activity5, 1);

            //Assert
            Assert.IsTrue(busFlow.Activities.IndexOf(activity5) == 1, "Validate Activity moved to correct index in Activities list");
            Assert.IsTrue(group2.ActivitiesIdentifiers.IndexOf(group2.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity == activity5).First()) == 0, "Validate Activity moved to correct indx in target group");
        }
    }
}
