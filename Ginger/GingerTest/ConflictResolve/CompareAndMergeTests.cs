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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerTest.ConflictResolve
{
    [TestClass]
    public class CompareAndMergeTests
    {
        //#region Generic Comparison Tests

        //[TestMethod]
        //public void Compare_NoChange_AllComparisonsShouldBeUnmodified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF",
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF",
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    AssertAllComparisonsAreUnmodified(comparisons);

        //    static void AssertAllComparisonsAreUnmodified(ICollection<Comparison> comparisons)
        //    {
        //        foreach(Comparison comparison in comparisons)
        //        {
        //            Assert.AreEqual(expected: State.Unmodified, comparison.State);
        //            if (comparison.HasChildComparisons)
        //                AssertAllComparisonsAreUnmodified(comparison.ChildComparisons);
        //        }
        //    }
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_HasOnlyOneComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Assert.AreEqual(expected: 1, actual: comparisons.Count, "More comparisons than expected.");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_ComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    Assert.AreEqual(expected: State.Modified, actual: comparison.State, $"Comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_HasChangedPropertyComparisons()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    Assert.IsTrue(comparison.HasChildComparisons, "No child comparison found.");
        //    IEnumerable<Comparison> namePropertyComparisons = comparison.ChildComparisons.Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)));
        //    Assert.IsTrue(namePropertyComparisons.Any(), $"No comparison found for property {nameof(BusinessFlow.Name)}");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_HasDeletedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> namePropertyComparisons = comparison.ChildComparisons.Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)));
        //    Comparison? deletedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedChildComparison, $"No child comparison as '{nameof(State.Deleted)}' found for property {nameof(BusinessFlow.Name)}.");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_HasAddedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> namePropertyComparisons = comparison.ChildComparisons.Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)));
        //    Comparison? addedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedChildComparison, $"No child comparison as '{nameof(State.Added)}' found for property {nameof(BusinessFlow.Name)}.");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_DeletedComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> namePropertyComparisons = comparison.ChildComparisons.Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)));
        //    Comparison? addedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Comparison? deletedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Name,
        //        actual: (string?)deletedChildComparison?.Data,
        //        "Local BusinessFlow name not equal to deleted comparison's data.");
        //}

        //[TestMethod]
        //public void Compare_SimplePropertyChange_AddedComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "My BF"
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "My BF New"
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> namePropertyComparisons = comparison.ChildComparisons.Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)));
        //    Comparison? addedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Comparison? deletedChildComparison = namePropertyComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: remoteBF.Name,
        //        actual: (string?)addedChildComparison?.Data,
        //        "Remote BusinessFlow name not equal to added comparison's data.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_HasOnlyOneComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Assert.AreEqual(expected: 1, actual: comparisons.Count, "More comparisons than expected.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_ComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    Assert.AreEqual(expected: State.Modified, actual: comparison.State, $"Comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_HasChangedPropertyComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Assert.IsTrue(activitiesPropertyComparisons.Any(), $"No comparison found for property {nameof(BusinessFlow.Activities)}");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_ChangedPropertyComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Assert.AreEqual(
        //        expected: State.Modified,
        //        actual: activitiesPropertyComparisons.ElementAt(0).State,
        //        $"Activity property comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_ItemComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Assert.AreEqual(
        //        expected: State.Modified,
        //        actual: activityComparison.State,
        //        $"Activity property comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_HasItemChangedPropertyComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    IEnumerable<Comparison> activityNamePropertyComparisons = activityComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(Activity.ActivityName)));
        //    Assert.IsTrue(activityNamePropertyComparisons.Any(), $"No comparisons found for property '{nameof(Activity.ActivityName)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_HasAddedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    IEnumerable<Comparison> activityNamePropertyComparisons = activityComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(Activity.ActivityName)));
        //    Comparison? addedActivityNameComparison = activityNamePropertyComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(
        //        addedActivityNameComparison,
        //        $"No child comparison as '{State.Added} found' for property {nameof(Activity.ActivityName)}.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_HasDeletedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    IEnumerable<Comparison> activityNamePropertyComparisons = activityComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(Activity.ActivityName)));
        //    Comparison? deletedActivityNameComparison = activityNamePropertyComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(
        //        deletedActivityNameComparison,
        //        $"No child comparison as '{State.Deleted} found' for property {nameof(Activity.ActivityName)}.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_DeletedComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    IEnumerable<Comparison> activityNamePropertyComparisons = activityComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(Activity.ActivityName)));
        //    Comparison? deletedActivityNameComparison = activityNamePropertyComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Activities[0].ActivityName,
        //        actual: (string?)deletedActivityNameComparison?.Data,
        //        "Local Activity name not equal to deleted comparison's data.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemChange_AddedComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "My Activity"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = "My Activity New"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison activityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    IEnumerable<Comparison> activityNamePropertyComparisons = activityComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(Activity.ActivityName)));
        //    Comparison? addedActivityNameComparison = activityNamePropertyComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Activities[0].ActivityName,
        //        actual: (string?)addedActivityNameComparison?.Data,
        //        "Remote Activity name not equal to added comparison's data.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_HasOnlyOneComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Assert.AreEqual(expected: 1, actual: comparisons.Count, "More comparisons than expected.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_ComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    Assert.AreEqual(expected: State.Modified, actual: comparison.State, $"Comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_HasChangedPropertyComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Assert.IsTrue(activitiesPropertyComparisons.Any(), $"No comparison found for property {nameof(BusinessFlow.Activities)}");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_UnchangedItemComparisonIsUnmodified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Assert.AreEqual(
        //        expected: State.Unmodified,
        //        actual: noChangeActivityComparison.State,
        //        $"Unchanged Activity's comparison is not '{nameof(State.Unmodified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_HasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Comparison? addedActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons
        //        .FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedActivityComparison, $"No comparison found as {nameof(State.Added)}' for added activity.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemAdded_AddedComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Comparison? addedActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons
        //        .FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.Activities[1],
        //        actual: (Activity?)addedActivityComparison?.Data,
        //        "Added Activity is not same as comparison's data");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_HasOnlyOneComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Assert.AreEqual(expected: 1, actual: comparisons.Count, "More comparisons than expected.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_ComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    Assert.AreEqual(expected: State.Modified, actual: comparison.State, $"Comparison is not '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_HasChangedPropertyComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Assert.IsTrue(activitiesPropertyComparisons.Any(), $"No comparison found for property {nameof(BusinessFlow.Activities)}");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_UnchangedItemComparisonIsUnmodified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Assert.AreEqual(
        //        expected: State.Unmodified,
        //        actual: noChangeActivityComparison.State,
        //        $"Unchanged Activity's comparison is not '{nameof(State.Unmodified)}'.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_HasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Comparison? deletedActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons
        //        .FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedActivityComparison, $"No comparison found as {nameof(State.Deleted)}' for deleted activity.");
        //}

        //[TestMethod]
        //public void Compare_CollectionPropertyItemDeleted_DeletedComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                ActivityName = "Activity 1"
        //            },
        //            new Activity()
        //            {
        //                ActivityName = "Activity 2"
        //            }
        //        }
        //    };
        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new()
        //        {
        //            new Activity()
        //            {
        //                Guid = localBF.Activities[0].Guid,
        //                ActivityName = localBF.Activities[0].ActivityName
        //            }
        //        }
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison comparison = comparisons.ElementAt(0);
        //    IEnumerable<Comparison> activitiesPropertyComparisons = comparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)));
        //    Comparison noChangeActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons.ElementAt(0);
        //    Comparison? deletedActivityComparison = activitiesPropertyComparisons.ElementAt(0).ChildComparisons
        //        .FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.Activities[1],
        //        actual: (Activity?)deletedActivityComparison?.Data,
        //        "Deleted Activity is not same as comparison's data");
        //}

        //#endregion Generic Comparison Tests

        //#region BusinessFlow Comparison Tests

        //[TestMethod]
        //public void Compare_BusinessFlow_BusinessFlowComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new();

        //    BusinessFlow remoteBF = new();

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Assert.AreEqual(expected: 2, actual: comparisons.Count, "BusinessFlow comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasBusinessFlowDeletedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new();
        //    BusinessFlow remoteBF = new();

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison? bfDeletedComparison = comparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(bfDeletedComparison, $"No BusinessFlow comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasBusinessFlowAddedComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new();
        //    BusinessFlow remoteBF = new();

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison? bfAddedComparison = comparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(bfAddedComparison, $"No BusinessFlow comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BusinessFlowDeletedComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new();
        //    BusinessFlow remoteBF = new();

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison? bfDeletedComparison = comparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF,
        //        actual: (BusinessFlow?)bfDeletedComparison?.Data,
        //        $"BusinessFlow '{nameof(State.Deleted)}' comparison's Data not same as Local BusinessFlow.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BusinessFlowAddedComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new();
        //    BusinessFlow remoteBF = new();

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison? bfAddedComparison = comparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF,
        //        actual: (BusinessFlow?)bfAddedComparison?.Data,
        //        $"BusinessFlow '{nameof(State.Deleted)}' comparison's Data not same as Remote BusinessFlow.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_NameComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "Name Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "Name New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] nameComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: nameComparisons.Length, $"{nameof(BusinessFlow.Name)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedNameComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "Name Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "Name New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] nameComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)))
        //        .ToArray();
        //    Comparison? deletedNameComparison = nameComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedNameComparison, $"No {nameof(BusinessFlow.Name)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedNameComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "Name Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "Name New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] nameComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)))
        //        .ToArray();
        //    Comparison? addedNameComparison = nameComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedNameComparison, $"No {nameof(BusinessFlow.Name)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedNameComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "Name Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "Name New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] nameComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)))
        //        .ToArray();
        //    Comparison? deletedNameComparison = nameComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.Name,
        //        actual: (string?)deletedNameComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Name)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Name)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedNameComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Name = "Name Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Name = "Name New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] nameComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Name)))
        //        .ToArray();
        //    Comparison? addedNameComparison = nameComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.Name,
        //        actual: (string?)addedNameComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Name)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Name)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DescriptionComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Description = "Description Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Description = "Description New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] descriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Description)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: descriptionComparisons.Length, $"{nameof(BusinessFlow.Description)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedDescriptionComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Description = "Description Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Description = "Description New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] descriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Description)))
        //        .ToArray();
        //    Comparison? deletedDescriptionComparison = descriptionComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedDescriptionComparison, $"No {nameof(BusinessFlow.Description)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedDescriptionComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Description = "Description Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Description = "Description New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] descriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Description)))
        //        .ToArray();
        //    Comparison? addedDescriptionComparison = descriptionComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedDescriptionComparison, $"No {nameof(BusinessFlow.Description)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedDescriptionComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Description = "Description Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Description = "Description New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] descriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Description)))
        //        .ToArray();
        //    Comparison? deletedDescriptionComparison = descriptionComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.Description,
        //        actual: (string?)deletedDescriptionComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Description)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Description)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedDescriptionComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Description = "Description Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Description = "Description New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] descriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Description)))
        //        .ToArray();
        //    Comparison? addedDescriptionComparison = descriptionComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.Description,
        //        actual: (string?)addedDescriptionComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Description)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Description)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_RunDescriptionComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        RunDescription = "RunDescription Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        RunDescription = "RunDescription New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] runDescriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.RunDescription)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: runDescriptionComparisons.Length, $"{nameof(BusinessFlow.RunDescription)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedRunDescriptionComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        RunDescription = "RunDescription Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        RunDescription = "RunDescription New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] runDescriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.RunDescription)))
        //        .ToArray();
        //    Comparison? deletedRunDescriptionComparison = runDescriptionComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedRunDescriptionComparison, $"No {nameof(BusinessFlow.RunDescription)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedRunDescriptionComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        RunDescription = "RunDescription Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        RunDescription = "RunDescription New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] runDescriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.RunDescription)))
        //        .ToArray();
        //    Comparison? addedRunDescriptionComparison = runDescriptionComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedRunDescriptionComparison, $"No {nameof(BusinessFlow.RunDescription)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedRunDescriptionComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        RunDescription = "RunDescription Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        RunDescription = "RunDescription New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] runDescriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.RunDescription)))
        //        .ToArray();
        //    Comparison? deletedRunDescriptionComparison = runDescriptionComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.RunDescription,
        //        actual: (string?)deletedRunDescriptionComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.RunDescription)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.RunDescription)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedRunDescriptionComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        RunDescription = "RunDescription Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        RunDescription = "RunDescription New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] runDescriptionComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.RunDescription)))
        //        .ToArray();
        //    Comparison? addedRunDescriptionComparison = runDescriptionComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.RunDescription,
        //        actual: (string?)addedRunDescriptionComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.RunDescription)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.RunDescription)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_StatusComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Status = BusinessFlow.eBusinessFlowStatus.Suspended,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Status = BusinessFlow.eBusinessFlowStatus.Candidate,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] statusComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Status)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: statusComparisons.Length, $"{nameof(BusinessFlow.Status)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedStatusComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Status = BusinessFlow.eBusinessFlowStatus.Suspended,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Status = BusinessFlow.eBusinessFlowStatus.Candidate,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] statusComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Status)))
        //        .ToArray();
        //    Comparison? deletedStatusComparison = statusComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedStatusComparison, $"No {nameof(BusinessFlow.Status)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedStatusComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Status = BusinessFlow.eBusinessFlowStatus.Suspended,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Status = BusinessFlow.eBusinessFlowStatus.Candidate,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] statusComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Status)))
        //        .ToArray();
        //    Comparison? addedStatusComparison = statusComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedStatusComparison, $"No {nameof(BusinessFlow.Status)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedStatusComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Status = BusinessFlow.eBusinessFlowStatus.Suspended,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Status = BusinessFlow.eBusinessFlowStatus.Candidate,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] statusComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Status)))
        //        .ToArray();
        //    Comparison? deletedStatusComparison = statusComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Status,
        //        actual: (BusinessFlow.eBusinessFlowStatus?)deletedStatusComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Status)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Status)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedStatusComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Status = BusinessFlow.eBusinessFlowStatus.Suspended,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Status = BusinessFlow.eBusinessFlowStatus.Candidate,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] statusComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Status)))
        //        .ToArray();
        //    Comparison? addedStatusComparison = statusComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Status,
        //        actual: (BusinessFlow.eBusinessFlowStatus?)addedStatusComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Status)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Status)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActiveComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Active = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Active = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activeComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Active)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: activeComparisons.Length, $"{nameof(BusinessFlow.Active)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedActiveComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Active = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Active = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activeComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Active)))
        //        .ToArray();
        //    Comparison? deletedActiveComparison = activeComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedActiveComparison, $"No {nameof(BusinessFlow.Active)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedActiveComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Active = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Active = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activeComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Active)))
        //        .ToArray();
        //    Comparison? addedActiveComparison = activeComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedActiveComparison, $"No {nameof(BusinessFlow.Active)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedActiveComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Active = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Active = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activeComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Active)))
        //        .ToArray();
        //    Comparison? deletedActiveComparison = activeComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Active,
        //        actual: (bool?)deletedActiveComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Active)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Active)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedActiveComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Active = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Active = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activeComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Active)))
        //        .ToArray();
        //    Comparison? addedActiveComparison = activeComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Active,
        //        actual: (bool?)addedActiveComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Active)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Active)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_MandatoryComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Mandatory = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Mandatory = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] mandatoryComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Mandatory)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: mandatoryComparisons.Length, $"{nameof(BusinessFlow.Mandatory)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedMandatoryComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Mandatory = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Mandatory = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] mandatoryComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Mandatory)))
        //        .ToArray();
        //    Comparison? deletedMandatoryComparison = mandatoryComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedMandatoryComparison, $"No {nameof(BusinessFlow.Mandatory)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedMandatoryComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Mandatory = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Mandatory = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] mandatoryComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Mandatory)))
        //        .ToArray();
        //    Comparison? addedMandatoryComparison = mandatoryComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedMandatoryComparison, $"No {nameof(BusinessFlow.Mandatory)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedMandatoryComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Mandatory = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Mandatory = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] mandatoryComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Mandatory)))
        //        .ToArray();
        //    Comparison? deletedMandatoryComparison = mandatoryComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Mandatory,
        //        actual: (bool?)deletedMandatoryComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Mandatory)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Mandatory)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedMandatoryComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Mandatory = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Mandatory = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] mandatoryComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Mandatory)))
        //        .ToArray();
        //    Comparison? addedMandatoryComparison = mandatoryComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Mandatory,
        //        actual: (bool?)addedMandatoryComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Mandatory)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Mandatory)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_SourceComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Source = BusinessFlow.eSource.Gherkin,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Source = BusinessFlow.eSource.QTP,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] sourceComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Source)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: sourceComparisons.Length, $"{nameof(BusinessFlow.Source)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedSourceComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Source = BusinessFlow.eSource.Gherkin,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Source = BusinessFlow.eSource.QTP,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] sourceComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Source)))
        //        .ToArray();
        //    Comparison? deletedSourceComparison = sourceComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedSourceComparison, $"No {nameof(BusinessFlow.Source)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedSourceComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Source = BusinessFlow.eSource.Gherkin,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Source = BusinessFlow.eSource.QTP,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] sourceComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Source)))
        //        .ToArray();
        //    Comparison? addedSourceComparison = sourceComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedSourceComparison, $"No {nameof(BusinessFlow.Source)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedSourceComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Source = BusinessFlow.eSource.Gherkin,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Source = BusinessFlow.eSource.QTP,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] sourceComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Source)))
        //        .ToArray();
        //    Comparison? deletedSourceComparison = sourceComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Source,
        //        actual: (BusinessFlow.eSource?)deletedSourceComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Source)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.Source)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedSourceComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Source = BusinessFlow.eSource.Gherkin,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Source = BusinessFlow.eSource.QTP,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] sourceComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Source)))
        //        .ToArray();
        //    Comparison? addedSourceComparison = sourceComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Source,
        //        actual: (BusinessFlow.eSource?)addedSourceComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Source)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.Source)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 1, actual: activitiesComparisons.Length, $"{nameof(BusinessFlow.Activities)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison? modifiedActivitiesComparison = activitiesComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(modifiedActivitiesComparison, $"No {nameof(BusinessFlow.Activities)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison[] activitiesItemComparisons = activitiesComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2, 
        //        actual: activitiesItemComparisons.Length, 
        //        $"{nameof(BusinessFlow.Activities)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison[] activitiesItemComparisons = activitiesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = activitiesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.Activities)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison[] activitiesItemComparisons = activitiesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = activitiesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.Activities)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison[] activitiesItemComparisons = activitiesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = activitiesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.Activities[0],
        //        actual: (Activity?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Activities)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.Activities)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Activities = new() { new Activity() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Activities = new() { new Activity() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Activities)))
        //        .ToArray();
        //    Comparison[] activitiesItemComparisons = activitiesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = activitiesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.Activities[0],
        //        actual: (Activity?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Activities)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.Activities)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AlmDataComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        AlmData = "AlmData Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        AlmData = "AlmData New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] almDataComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.AlmData)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 2, actual: almDataComparisons.Length, $"{nameof(BusinessFlow.AlmData)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedAlmDataComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        AlmData = "AlmData Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        AlmData = "AlmData New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] almDataComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.AlmData)))
        //        .ToArray();
        //    Comparison? deletedAlmDataComparison = almDataComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedAlmDataComparison, $"No {nameof(BusinessFlow.AlmData)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedAlmDataComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        AlmData = "AlmData Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        AlmData = "AlmData New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] almDataComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.AlmData)))
        //        .ToArray();
        //    Comparison? addedAlmDataComparison = almDataComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedAlmDataComparison, $"No {nameof(BusinessFlow.AlmData)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedAlmDataComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        AlmData = "AlmData Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        AlmData = "AlmData New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] almDataComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.AlmData)))
        //        .ToArray();
        //    Comparison? deletedAlmDataComparison = almDataComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.AlmData,
        //        actual: (string?)deletedAlmDataComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.AlmData)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.AlmData)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedAlmDataComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        AlmData = "AlmData Old",
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        AlmData = "AlmData New",
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] almDataComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.AlmData)))
        //        .ToArray();
        //    Comparison? addedAlmDataComparison = almDataComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.AlmData,
        //        actual: (string?)addedAlmDataComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.AlmData)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.AlmData)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Assert.AreEqual(
        //        expected: 1, 
        //        actual: targetApplicationsComparisons.Length, 
        //        $"{nameof(BusinessFlow.TargetApplications)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison? modifiedTargetApplicationsComparison = targetApplicationsComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(
        //        modifiedTargetApplicationsComparison, 
        //        $"No {nameof(BusinessFlow.TargetApplications)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison[] targetApplicationsItemComparisons = targetApplicationsComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: targetApplicationsItemComparisons.Length,
        //        $"{nameof(BusinessFlow.TargetApplications)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison[] targetApplicationsItemComparisons = targetApplicationsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = targetApplicationsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.TargetApplications)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison[] targetApplicationsItemComparisons = targetApplicationsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = targetApplicationsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.TargetApplications)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison[] targetApplicationsItemComparisons = targetApplicationsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = targetApplicationsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.TargetApplications[0],
        //        actual: (TargetApplication?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.TargetApplications)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.TargetApplications)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TargetApplicationsAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        TargetApplications = new() { new TargetApplication() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] targetApplicationsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.TargetApplications)))
        //        .ToArray();
        //    Comparison[] targetApplicationsItemComparisons = targetApplicationsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = targetApplicationsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.TargetApplications[0],
        //        actual: (TargetApplication?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.TargetApplications)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.TargetApplications)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Assert.AreEqual(
        //        expected: 1,
        //        actual: variablesComparisons.Length,
        //        $"{nameof(BusinessFlow.Variables)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison? modifiedVariablesComparison = variablesComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(
        //        modifiedVariablesComparison,
        //        $"No {nameof(BusinessFlow.Variables)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison[] variablesItemComparisons = variablesComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: variablesItemComparisons.Length,
        //        $"{nameof(BusinessFlow.Variables)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison[] variablesItemComparisons = variablesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = variablesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.Variables)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison[] variablesItemComparisons = variablesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = variablesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.Variables)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison[] variablesItemComparisons = variablesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = variablesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.Variables[0],
        //        actual: (VariableString?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Variables)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.Variables)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_VariablesAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Variables = new() { new VariableString() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Variables = new() { new VariableString() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] variablesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Variables)))
        //        .ToArray();
        //    Comparison[] variablesItemComparisons = variablesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = variablesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.Variables[0],
        //        actual: (VariableString?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Variables)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.Variables)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_EnableActivitiesVariablesDependenciesControlComparisonCountIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        EnableActivitiesVariablesDependenciesControl = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        EnableActivitiesVariablesDependenciesControl = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] enableActivitiesVariablesDependenciesControlComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)))
        //        .ToArray();
        //    Assert.AreEqual(
        //        expected: 2, 
        //        actual: enableActivitiesVariablesDependenciesControlComparisons.Length, 
        //        $"{nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)} comparison count is not 2");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasDeletedEnableActivitiesVariablesDependenciesControlComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        EnableActivitiesVariablesDependenciesControl = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        EnableActivitiesVariablesDependenciesControl = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] enableActivitiesVariablesDependenciesControlComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)))
        //        .ToArray();
        //    Comparison? deletedEnableActivitiesVariablesDependenciesControlComparison = 
        //        enableActivitiesVariablesDependenciesControlComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(
        //        deletedEnableActivitiesVariablesDependenciesControlComparison, 
        //        $"No {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)} comparison found as '{nameof(State.Deleted)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_HasAddedEnableActivitiesVariablesDependenciesControlComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        EnableActivitiesVariablesDependenciesControl = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        EnableActivitiesVariablesDependenciesControl = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] enableActivitiesVariablesDependenciesControlComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)))
        //        .ToArray();
        //    Comparison? addedEnableActivitiesVariablesDependenciesControlComparison = 
        //        enableActivitiesVariablesDependenciesControlComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(
        //        addedEnableActivitiesVariablesDependenciesControlComparison, 
        //        $"No {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)} comparison found as '{nameof(State.Added)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_DeletedEnableActivitiesVariablesDependenciesControlComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        EnableActivitiesVariablesDependenciesControl = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        EnableActivitiesVariablesDependenciesControl = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] enableActivitiesVariablesDependenciesControlComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)))
        //        .ToArray();
        //    Comparison? deletedEnableActivitiesVariablesDependenciesControlComparison = 
        //        enableActivitiesVariablesDependenciesControlComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.EnableActivitiesVariablesDependenciesControl,
        //        actual: (bool?)deletedEnableActivitiesVariablesDependenciesControlComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)} comparison's data not same as Local BusinessFlow's {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_AddedEnableActivitiesVariablesDependenciesControlComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        EnableActivitiesVariablesDependenciesControl = true,
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        EnableActivitiesVariablesDependenciesControl = false,
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] enableActivitiesVariablesDependenciesControlComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)))
        //        .ToArray();
        //    Comparison? addedEnableActivitiesVariablesDependenciesControlComparison = 
        //        enableActivitiesVariablesDependenciesControlComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.EnableActivitiesVariablesDependenciesControl,
        //        actual: (bool?)addedEnableActivitiesVariablesDependenciesControlComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)} comparison's data not same as Remote BusinessFlow's {nameof(BusinessFlow.EnableActivitiesVariablesDependenciesControl)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 1, actual: activitiesGroupsComparisons.Length, $"{nameof(BusinessFlow.ActivitiesGroups)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison? modifiedActivitiesGroupsComparison = activitiesGroupsComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(modifiedActivitiesGroupsComparison, $"No {nameof(BusinessFlow.ActivitiesGroups)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison[] activitiesGroupsItemComparisons = activitiesGroupsComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: activitiesGroupsItemComparisons.Length,
        //        $"{nameof(BusinessFlow.ActivitiesGroups)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison[] activitiesGroupsItemComparisons = activitiesGroupsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = activitiesGroupsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.ActivitiesGroups)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison[] activitiesGroupsItemComparisons = activitiesGroupsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = activitiesGroupsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.ActivitiesGroups)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison[] activitiesGroupsItemComparisons = activitiesGroupsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = activitiesGroupsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.ActivitiesGroups[0],
        //        actual: (ActivitiesGroup?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.ActivitiesGroups)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.ActivitiesGroups)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_ActivitiesGroupsAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        ActivitiesGroups = new() { new ActivitiesGroup() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] activitiesGroupsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.ActivitiesGroups)))
        //        .ToArray();
        //    Comparison[] activitiesGroupsItemComparisons = activitiesGroupsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = activitiesGroupsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.ActivitiesGroups[0],
        //        actual: (ActivitiesGroup?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.ActivitiesGroups)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.ActivitiesGroups)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 1, actual: tagsComparisons.Length, $"{nameof(BusinessFlow.Tags)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison? modifiedTagsComparison = tagsComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(modifiedTagsComparison, $"No {nameof(BusinessFlow.Tags)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison[] tagsItemComparisons = tagsComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: tagsItemComparisons.Length,
        //        $"{nameof(BusinessFlow.Tags)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison[] tagsItemComparisons = tagsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = tagsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.Tags)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison[] tagsItemComparisons = tagsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = tagsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.Tags)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison[] tagsItemComparisons = tagsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = tagsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreEqual(
        //        expected: localBF.Tags[0],
        //        actual: (Guid?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.Tags)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.Tags)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_TagsAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        Tags = new() { Guid.NewGuid() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] tagsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.Tags)))
        //        .ToArray();
        //    Comparison[] tagsItemComparisons = tagsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = tagsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreEqual(
        //        expected: remoteBF.Tags[0],
        //        actual: (Guid?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.Tags)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.Tags)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Assert.AreEqual(expected: 1, actual: bFFlowControlsComparisons.Length, $"{nameof(BusinessFlow.BFFlowControls)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison? modifiedBFFlowControlsComparison = bFFlowControlsComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(modifiedBFFlowControlsComparison, $"No {nameof(BusinessFlow.BFFlowControls)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison[] bFFlowControlsItemComparisons = bFFlowControlsComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: bFFlowControlsItemComparisons.Length,
        //        $"{nameof(BusinessFlow.BFFlowControls)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison[] bFFlowControlsItemComparisons = bFFlowControlsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = bFFlowControlsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.BFFlowControls)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison[] bFFlowControlsItemComparisons = bFFlowControlsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = bFFlowControlsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.BFFlowControls)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison[] bFFlowControlsItemComparisons = bFFlowControlsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = bFFlowControlsItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.BFFlowControls[0],
        //        actual: (FlowControl?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.BFFlowControls)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.BFFlowControls)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_BFFlowControlsAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        BFFlowControls = new() { new FlowControl() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] bFFlowControlsComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.BFFlowControls)))
        //        .ToArray();
        //    Comparison[] bFFlowControlsItemComparisons = bFFlowControlsComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = bFFlowControlsItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.BFFlowControls[0],
        //        actual: (FlowControl?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.BFFlowControls)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.BFFlowControls)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesComparisonCountIs1()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Assert.AreEqual(
        //        expected: 1,
        //        actual: inputVariableRulesComparisons.Length,
        //        $"{nameof(BusinessFlow.InputVariableRules)} comparison count is not 1");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesComparisonIsModified()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison? modifiedInputVariableRulesComparison = inputVariableRulesComparisons.FirstOrDefault(c => c.State == State.Modified);
        //    Assert.IsNotNull(
        //        modifiedInputVariableRulesComparison,
        //        $"No {nameof(BusinessFlow.InputVariableRules)} comparison found as '{nameof(State.Modified)}'.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesItemComparisonsIs2()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison[] inputVariableRulesItemComparisons = inputVariableRulesComparisons[0].ChildComparisons.ToArray();
        //    Assert.AreEqual(
        //        expected: 2,
        //        actual: inputVariableRulesItemComparisons.Length,
        //        $"{nameof(BusinessFlow.InputVariableRules)} item comparisons count is not 2.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesHasDeletedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison[] inputVariableRulesItemComparisons = inputVariableRulesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = inputVariableRulesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.IsNotNull(deletedItemComparison, $"No {nameof(BusinessFlow.InputVariableRules)} item comparison found as {nameof(State.Deleted)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesHasAddedItemComparison()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison[] inputVariableRulesItemComparisons = inputVariableRulesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = inputVariableRulesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.IsNotNull(addedItemComparison, $"No {nameof(BusinessFlow.InputVariableRules)} item comparison found as {nameof(State.Added)}.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesDeletedItemComparisonHasLocalData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison[] inputVariableRulesItemComparisons = inputVariableRulesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? deletedItemComparison = inputVariableRulesItemComparisons.FirstOrDefault(c => c.State == State.Deleted);
        //    Assert.AreSame(
        //        expected: localBF.InputVariableRules[0],
        //        actual: (InputVariableRule?)deletedItemComparison?.Data,
        //        $"{nameof(State.Deleted)} {nameof(BusinessFlow.InputVariableRules)} item comparison's data is not same as Local BusinessFlow's {nameof(BusinessFlow.InputVariableRules)} item.");
        //}

        //[TestMethod]
        //public void Compare_BusinessFlow_InputVariableRulesAddedItemComparisonHasRemoteData()
        //{
        //    //arrange
        //    BusinessFlow localBF = new()
        //    {
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    BusinessFlow remoteBF = new()
        //    {
        //        Guid = localBF.Guid,
        //        InputVariableRules = new() { new InputVariableRule() },
        //    };

        //    //act
        //    ICollection<Comparison> comparisons = new ConflictResolver().Compare(localBF, remoteBF);

        //    //assert
        //    Comparison bfComparison = comparisons.ToArray()[0];
        //    Comparison[] inputVariableRulesComparisons = bfComparison.ChildComparisons
        //        .Where(c => string.Equals(c.Name, nameof(BusinessFlow.InputVariableRules)))
        //        .ToArray();
        //    Comparison[] inputVariableRulesItemComparisons = inputVariableRulesComparisons[0].ChildComparisons.ToArray();
        //    Comparison? addedItemComparison = inputVariableRulesItemComparisons.FirstOrDefault(c => c.State == State.Added);
        //    Assert.AreSame(
        //        expected: remoteBF.InputVariableRules[0],
        //        actual: (InputVariableRule?)addedItemComparison?.Data,
        //        $"{nameof(State.Added)} {nameof(BusinessFlow.InputVariableRules)} item comparison's data is not same as Remote BusinessFlow's {nameof(BusinessFlow.InputVariableRules)} item.");
        //}

        //#endregion BusinessFlow Comparison Tests
    }
}
