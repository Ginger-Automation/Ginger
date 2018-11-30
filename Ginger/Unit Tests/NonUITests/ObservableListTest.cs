using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class ObservableListTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            //TODO::
        }




        [TestMethod]
        public void ObservableListCast()
        {
            //Arrange
            ObservableList<IActivity> activities = new ObservableList<IActivity>();
            activities.Add(new Activity() { ActivityName = "a1" });
            activities.Add(new Activity() { ActivityName = "a2" });

            //Act
            ObservableList<RepositoryItemBase> list = activities.ListItemsCast<RepositoryItemBase>();

            //Assert
            Assert.IsTrue(activities[0] == list[0], "a1 equel a1");
            Assert.IsTrue(activities[1] == list[1], "a2 equel a2");
        }


    }
}
