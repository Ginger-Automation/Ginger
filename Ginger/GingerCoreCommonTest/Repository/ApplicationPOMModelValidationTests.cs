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
using Amdocs.Ginger.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.Repository
{
    [TestClass]
    public class ApplicationPOMModelValidationTests
    {
        [TestMethod]
        public void GetUniqueElementName_DuplicateAcrossMappedAndUnmapped_ReturnsNextAvailableSuffix()
        {
            ApplicationPOMModel pom = new();
            pom.MappedUIElements.Add(new ElementInfo { ElementName = "Submit" });
            pom.UnMappedUIElements.Add(new ElementInfo { ElementName = "submit_2" });

            string uniqueName = pom.GetUniqueElementName(" submit ");

            Assert.AreEqual("submit_3", uniqueName);
        }

        [TestMethod]
        public void GetUniqueElementName_ExcludedElementKeepsItsOwnName_DoesNotRenameOrChangeGroup()
        {
            ApplicationPOMModel pom = new();
            ElementInfo element = new() { ElementName = "Submit", ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped };
            pom.MappedUIElements.Add(element);

            string uniqueName = pom.GetUniqueElementName("Submit", element);

            Assert.AreEqual("Submit", uniqueName);
            Assert.AreEqual(ApplicationPOMModel.eElementGroup.Unmapped, element.ElementGroup);
        }

        [TestMethod]
        public void TryValidateLocator_EmptyValue_ReturnsFalse()
        {
            ObservableList<ElementLocator> locators = [];
            ElementLocator locator = new() { LocateBy = eLocateBy.ByID };
            locators.Add(locator);

            bool isValid = ApplicationPOMModel.TryValidateLocator(
                locators,
                locator,
                locator.LocateBy,
                " ",
                out string validationError);

            Assert.IsFalse(isValid);
            Assert.AreEqual("Locator value cannot be empty.", validationError);
        }

        [TestMethod]
        public void TryValidateLocator_DuplicateTypeAndValueIgnoringCaseAndWhitespace_ReturnsFalse()
        {
            ElementLocator existingLocator = new() { LocateBy = eLocateBy.ByID, LocateValue = " LoginButton " };
            ElementLocator newLocator = new() { LocateBy = eLocateBy.ByID };
            ObservableList<ElementLocator> locators = [existingLocator, newLocator];

            bool isValid = ApplicationPOMModel.TryValidateLocator(
                locators,
                newLocator,
                newLocator.LocateBy,
                "loginbutton",
                out string validationError);

            Assert.IsFalse(isValid);
            Assert.AreEqual("A locator with the same type and value already exists for this element.", validationError);
        }

        [TestMethod]
        public void TryValidateLocator_SameValueWithDifferentType_ReturnsTrue()
        {
            ElementLocator existingLocator = new() { LocateBy = eLocateBy.ByID, LocateValue = "LoginButton" };
            ElementLocator newLocator = new() { LocateBy = eLocateBy.ByName };
            ObservableList<ElementLocator> locators = [existingLocator, newLocator];

            bool isValid = ApplicationPOMModel.TryValidateLocator(
                locators,
                newLocator,
                newLocator.LocateBy,
                "LoginButton",
                out string validationError);

            Assert.IsTrue(isValid);
            Assert.IsNull(validationError);
        }
    }
}
