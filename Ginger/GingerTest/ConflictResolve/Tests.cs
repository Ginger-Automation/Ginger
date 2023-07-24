using Ginger;
using Ginger.ConflictResolve;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.ConflictResolve
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void Test()
        {
            BusinessFlow bf1 = new()
            {
                Name = "BusinessFlow",
                Activities = new()
                {
                    new Activity()
                    {
                        ActivityName = "Activity",
                        Acts = new()
                        {
                            new ActBrowserElement()
                            {
                                ControlAction = ActBrowserElement.eControlAction.GotoURL
                            }
                        }
                    }
                }
            };
            
            BusinessFlow bf2 = new()
            {
                Guid = bf1.Guid,
                Name = "BusinessFlow",
                Activities = new()
                {
                    new Activity()
                    {
                        Guid = bf1.Activities[0].Guid,
                        ActivityName = "Activity",
                        Acts = new()
                        {
                            new ActBrowserElement()
                            {
                                Guid = bf1.Activities[0].Acts[0].Guid,
                                ControlAction = ActBrowserElement.eControlAction.GetPageURL
                            }
                        }
                    }
                }
            };

            //TODO: Null values are considered as no value
            //TODO: While comparing RepositoryItemBase subclasses, if the GUID is different then the values should be considered as Deleted/Added

            ICollection<Comparison> comparisonResult = RIBCompare.Compare(bf1.GetType().Name, bf1, bf2);
            //comparisonResult.ChildComparisons.First().ChildComparisons.First(c => string.Equals(c.Name, nameof(BusinessFlow.Name))).Selected = true;
            BusinessFlow? mergedFlow = RIBMerge.Merge<BusinessFlow>(comparisonResult);
            //ICollection<ComparisonResult> notUnmodified = comparisonResult.ChildComparisons.Where(childCR => childCR.State != State.Unmodified).ToList();
            //TreeViewComparisonWindow window = new();
            //TreeViewComparisonPage page = new(comparisonResult);
        }

    }
}
