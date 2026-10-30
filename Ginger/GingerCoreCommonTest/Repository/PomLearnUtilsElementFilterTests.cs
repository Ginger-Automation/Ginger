#region License
/*
Copyright © 2014-2026 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License");
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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.Repository
{
    [TestClass]
    public class PomLearnUtilsElementFilterTests
    {
        [TestMethod]
        public void SelectElementsToList_NullFilter_KeepsDefaultSelection()
        {
            PomLearnUtils learnUtils = new(new ApplicationPOMModel());
            ObservableList<UIElementFilter> elements =
            [
                new UIElementFilter(eElementType.Button, string.Empty, isSelected: true),
                new UIElementFilter(eElementType.Div, string.Empty, isSelected: false),
            ];

            learnUtils.SelectElementsToList(elements, null);

            Assert.IsTrue(elements[0].Selected);
            Assert.IsFalse(elements[1].Selected);
        }

        [TestMethod]
        public void SelectElementsToList_SavedFilter_AppliesSavedSelection()
        {
            PomLearnUtils learnUtils = new(new ApplicationPOMModel());
            ObservableList<UIElementFilter> elements =
            [
                new UIElementFilter(eElementType.Button, string.Empty, isSelected: true),
                new UIElementFilter(eElementType.Div, string.Empty, isSelected: false),
            ];
            ObservableList<UIElementFilter> savedFilter =
            [
                new UIElementFilter(eElementType.Div, string.Empty, isSelected: true),
            ];

            learnUtils.SelectElementsToList(elements, savedFilter);

            Assert.IsFalse(elements[0].Selected);
            Assert.IsTrue(elements[1].Selected);
        }
    }
}
