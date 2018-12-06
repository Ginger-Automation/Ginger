#region License
/*
Copyright © 2014-2018 European Support Limited

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
            ObservableList<Activity> activities = new ObservableList<Activity>();
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
