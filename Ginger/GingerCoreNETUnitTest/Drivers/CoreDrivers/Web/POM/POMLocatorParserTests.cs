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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

#nullable enable
namespace GingerCoreNETUnitTest.Drivers.CoreDrivers.Web.POM
{
    [TestClass]
    [TestCategory(TestCategory.UnitTest)]
    public class POMLocatorParserTests
    {
        [TestMethod]
        public void Create_ValidLocatorValue_ReturnsPOMLocatorParserInstance()
        {
            // Arrange
            Guid pomId = Guid.NewGuid();
            Guid elementId = Guid.NewGuid();
            ApplicationPOMModel pom = new();
            ElementInfo element = new();
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) =>
            {
                if (id == pomId)
                {
                    return pom;
                }
                return null;
            };
            string locatorValue = $"{pomId}_{elementId}";

            // Act
            POMLocatorParser parser = POMLocatorParser.Create(locatorValue, pomProvider);

            // Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void Create_NullLocatorValue_ThrowsArgumentException()
        {
            // Arrange
            string? locatorValue = null;
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) => null;

            // Act & Assert
            Assert.ThrowsException<ArgumentException>(() => POMLocatorParser.Create(locatorValue!, pomProvider));
        }

        [TestMethod]
        public void Create_InvalidLocatorValueFormat_ThrowsFormatException()
        {
            // Arrange
            string locatorValue = "invalidLocatorValue";
            Func<Guid, ApplicationPOMModel?> pomProvider = (Guid id) => null;

            // Act & Assert
            Assert.ThrowsException<FormatException>(() => POMLocatorParser.Create(locatorValue, pomProvider));
        }

        [TestMethod]
        public void Create_InvalidElementId_ThrowsFormatException()
        {
            // Arrange
            Guid pomId = Guid.NewGuid();
            string locatorValue = $"{pomId}_invalidElementId";
            ApplicationPOMModel pom = new();
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) =>
            {
                if (id == pomId)
                {
                    return pom;
                }
                return null;
            };

            // Act & Assert
            Assert.ThrowsException<FormatException>(() => POMLocatorParser.Create(locatorValue, pomProvider));
        }

        [TestMethod]
        public void Create_InvalidPOMId_ThrowsFormatException()
        {
            // Arrange
            Guid elementId = Guid.NewGuid();
            string locatorValue = $"invalidPOMId_{elementId}";
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) => null;

            // Act & Assert
            Assert.ThrowsException<FormatException>(() => POMLocatorParser.Create(locatorValue, pomProvider));
        }

        [TestMethod]
        public void Create_POMHasNoMatchingElementById_ElementInfoIsNull()
        {
            //Arrange
            Guid elementId = Guid.NewGuid();
            Guid pomId = Guid.NewGuid();
            ObservableList<ElementInfo> pomMappedUIElements =
            [
                new ElementInfo() { Guid = Guid.NewGuid() },
                new ElementInfo() { Guid = Guid.NewGuid() },
                new ElementInfo() { Guid = Guid.NewGuid() },
            ];
            ApplicationPOMModel pom = new()
            {
                MappedUIElements = pomMappedUIElements
            };
            string locatorValue = $"{pomId}_{elementId}";
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) =>
            {
                if (id == pomId)
                {
                    return pom;
                }
                return null;
            };

            //Act
            POMLocatorParser parser = POMLocatorParser.Create(locatorValue, pomProvider);

            //Assert
            Assert.IsNull(parser.ElementInfo);
        }

        [TestMethod]
        public void Create_POMHasNoNonNullElement_ElementInfoIsNull()
        {
            //Arrange
            Guid elementId = Guid.NewGuid();
            Guid pomId = Guid.NewGuid();
            ObservableList<ElementInfo> pomMappedUIElements =
            [
                null!,
                null!,
                null!,
            ];
            ApplicationPOMModel pom = new()
            {
                MappedUIElements = pomMappedUIElements
            };
            string locatorValue = $"{pomId}_{elementId}";
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) =>
            {
                if (id == pomId)
                {
                    return pom;
                }
                return null;
            };

            //Act
            POMLocatorParser parser = POMLocatorParser.Create(locatorValue, pomProvider);

            //Assert
            Assert.IsNull(parser.ElementInfo);
        }

        [TestMethod]
        public void Create_POMHasNullMappedUIElements_ElementInfoIsNull()
        {
            //Arrange
            Guid elementId = Guid.NewGuid();
            Guid pomId = Guid.NewGuid();
            ApplicationPOMModel pom = new()
            {
                MappedUIElements = null,
            };
            string locatorValue = $"{pomId}_{elementId}";
            Func<Guid, ApplicationPOMModel?> pomProvider = (id) =>
            {
                if (id == pomId)
                {
                    return pom;
                }
                return null;
            };

            //Act
            POMLocatorParser parser = POMLocatorParser.Create(locatorValue, pomProvider);

            //Assert
            Assert.IsNull(parser.ElementInfo);
        }
    }
}
