#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.Repository;
[TestClass]
public class ActivityEqualityTests
{
    [TestMethod]
    public void AreEqual_SameProperties_ReturnsTrue()
    {
        // Arrange
        var activity1 = new Activity
        {
            ActivityName = "Search and Buy iPhone 15 Device",
            TargetApplication = "Brain_3UK_Self Service",
            Type = eSharedItemType.Regular,
            ActivitiesGroupID = null,
            Acts = [new ActDummy()],
            Variables = [new VariableString()]
        };

        var activity2 = new Activity
        {
            ActivityName = "Search and Buy iPhone 15 Device",
            TargetApplication = "Brain_3UK_Self Service",
            Type = eSharedItemType.Regular,
            ActivitiesGroupID = null,
            Acts = [new ActDummy()],
            Variables = [new VariableString()]
        };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void AreEqual_DifferentActivityName_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { ActivityName = "TestActivity1" };
        var activity2 = new Activity { ActivityName = "TestActivity2" };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreEqual_DifferentTargetApplication_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { TargetApplication = "TestApp1" };
        var activity2 = new Activity { TargetApplication = "TestApp2" };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreEqual_DifferentType_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { Type = eSharedItemType.Regular };
        var activity2 = new Activity { Type = eSharedItemType.Link };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreEqual_DifferentActivitiesGroupID_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { ActivitiesGroupID = "Group1" };
        var activity2 = new Activity { ActivitiesGroupID = "Group2" };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreEqual_DifferentActsCount_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { Acts = [new ActDummy()] };
        var activity2 = new Activity { Acts = [new ActDummy(), new ActDummy()] };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void AreEqual_DifferentVariablesCount_ReturnsFalse()
    {
        // Arrange
        var activity1 = new Activity { Variables = [new VariableString()] };
        var activity2 = new Activity { Variables = [new VariableString(), new VariableString()] };

        // Act
        var result = activity1.AreEqual(activity2);

        // Assert
        Assert.IsFalse(result);
    }
}
