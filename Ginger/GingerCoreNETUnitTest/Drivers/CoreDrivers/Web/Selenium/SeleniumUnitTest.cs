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
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCoreNETUnitTest.Drivers.CoreDrivers.Web.Selenium
{
    [TestClass]
    [TestCategory(TestCategory.UnitTest)]
    public class SeleniumUnitTest
    {
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method returns the correct ActLocator when the ElementLocateBy is set to eLocateBy.ByID.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_LocateByPrimitive_ReturnsActLocator()
        {
            // Arrange
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.ByID,
                ElementLocateValue = "expectedId",
            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;
            var pomExecutionUtil = new POMExecutionUtils();
            var selenium = new SeleniumDriver();

            // Act
            var result = selenium.GetLocatorsForWebSmartSync(act, pomExecutionUtil);

            // Assert
            Assert.AreEqual(eLocateBy.ByID, result[0].LocateBy);
            Assert.AreEqual("expectedId", result[0].LocateValue);
        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method uses the first active locator when the ElementLocateBy is set to eLocateBy.POMElement.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_POMElement_UseFirstActiveLocator()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            var locators = new ObservableList<ElementLocator>
            {
                new ElementLocator { LocateBy = eLocateBy.ByID, LocateValue = "testId", Active = false },
                new ElementLocator { LocateBy = eLocateBy.ByXPath, LocateValue = "//div[@id='test']", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = "testName", Active = false }
            };

            var currentPOMElementInfo = new ElementInfo { Locators = locators };
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null)).Returns(currentPOMElementInfo);

            var yourClass = new SeleniumDriver();

            // Act
            var result = yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object);

            // Assert
            Assert.AreEqual(locators[1].LocateBy, result[0].LocateBy);
            Assert.AreEqual(locators[1].LocateValue, result[0].LocateValue);
        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method uses the first supported locator when the ElementLocateBy is set to eLocateBy.POMElement.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_POMElement_UseFirstSupportedLocator()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            var locators = new ObservableList<ElementLocator>
            {
                new ElementLocator { LocateBy = eLocateBy.ByModelName, LocateValue = "testId", Active = false },
                new ElementLocator { LocateBy = eLocateBy.ByResourceID, LocateValue = "//div[@id='test']", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = "testName", Active = true }
            };

            var currentPOMElementInfo = new ElementInfo { Locators = locators };
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null)).Returns(currentPOMElementInfo);

            var yourClass = new SeleniumDriver();

            // Act
            List<ElementLocator> result = yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object);

            // Assert
            Assert.AreEqual(locators[2].LocateValue, result[0].LocateValue);
            Assert.AreEqual(locators[2].LocateBy, result[0].LocateBy);
        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method throws an exception when there is no active locator for the POM element.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_NoActiveLocatorForPOMElement_ThrowException()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            var locators = new ObservableList<ElementLocator>
            {
                new ElementLocator { LocateBy = eLocateBy.ByModelName, LocateValue = "testId", Active = false },
                new ElementLocator { LocateBy = eLocateBy.ByResourceID, LocateValue = "//div[@id='test']", Active = false },
                new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = "testName", Active = false }
            };

            var currentPOMElementInfo = new ElementInfo { Locators = locators };
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null)).Returns(currentPOMElementInfo);

            var yourClass = new SeleniumDriver();
            //Act/Assert
            var ex = Assert.ThrowsException<Exception>(() => yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object));
            Assert.AreEqual("No active or supported locators found in the current POM. Verify the POM configuration.", ex.Message);

        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method throws an exception when there is no supported locator for the POM element.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_NoSupportedLocatorForPOMElement_ThrowException()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            var locators = new ObservableList<ElementLocator>
            {
                new ElementLocator { LocateBy = eLocateBy.ByModelName, LocateValue = "testId", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByResourceID, LocateValue = "//div[@id='test']", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByContainerName, LocateValue = "testName", Active = true }
            };

            var currentPOMElementInfo = new ElementInfo { Locators = locators };
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null)).Returns(currentPOMElementInfo);

            var yourClass = new SeleniumDriver();
            //Act/Assert
            var ex = Assert.ThrowsException<Exception>(() => yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object));
            Assert.AreEqual("No active or supported locators found in the current POM. Verify the POM configuration.", ex.Message);

        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method throws an exception when there is no current POM.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_NoCurrentPOM_ThrowException()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM());

            var yourClass = new SeleniumDriver();
            //Act/Assert
            var ex = Assert.ThrowsException<Exception>(() => yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object));
            Assert.AreEqual("Relevant POM not found. Ensure that the POM context is correctly initialized before invoking this operation.", ex.Message);

        }

        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method throws an exception when there is no current POM element info.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_NoCurrentPOMElementInfo_ThrowException()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null));

            var yourClass = new SeleniumDriver();
            //Act/Assert
            var ex = Assert.ThrowsException<Exception>(() => yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object));
            Assert.AreEqual("Unable to find details about the POM. Check if the POM element information is correctly set.", ex.Message);

        }


        /// <summary>
        /// This unit test method verifies that the GetElementLocatorForWebSmartSync method throws an exception when the LocateBy value is unsupported.
        /// </summary>
        [TestMethod]
        public void GetElementLocatorForWebSmartSync_UnsupportedLocateBy_ThrowException()
        {
            // Arrange
            var ElementLocateBy = eLocateBy.ByModelName;
            var ElementLocateValue = "expectedId";
            //Act/Assert
            var ex = Assert.ThrowsException<Exception>(() => SeleniumDriver.GetElementLocatorForWebSmartSync(ElementLocateBy, ElementLocateValue));
            Assert.AreEqual("Unsupported locator type. Supported locator types include: ByXPath, ByID, ByName, ByClassName, ByCssSelector, ByLinkText, ByRelativeXpath, and ByTagName.", ex.Message);

        }
        /// <summary>
        /// This unit test method verifies that the GetLocatorsForWebSmartSync method uses the active and supported locators when the ElementLocateBy is set to eLocateBy.POMElement and UseAllLocators is true.
        /// </summary>
        [TestMethod]
        public void GetLocatorsForWebSmartSync_POMElement_UseAllActiveandSupportedLocator()
        {
            // Arrange
            var mockPOMExecutionUtil = new Mock<POMExecutionUtils>();
            var act = new ActWebSmartSync
            {
                ElementLocateBy = eLocateBy.POMElement,
                ElementLocateValue = "expectedId",
                UseAllLocators = true,

            };
            act.InputValues.First(iv => string.Equals(iv.Param, nameof(ActWebSmartSync.ElementLocateValue))).ValueForDriver = act.ElementLocateValue;

            var locators = new ObservableList<ElementLocator>
            {
                new ElementLocator { LocateBy = eLocateBy.ByID, LocateValue = "testId", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByXPath, LocateValue = "//div[@id='test']", Active = true },
                new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = "testName", Active = true }
            };

            var currentPOMElementInfo = new ElementInfo { Locators = locators };
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOM()).Returns(new ApplicationPOMModel());
            mockPOMExecutionUtil.Setup(p => p.GetCurrentPOMElementInfo(null)).Returns(currentPOMElementInfo);

            var yourClass = new SeleniumDriver();

            // Act
            List<ElementLocator> result = yourClass.GetLocatorsForWebSmartSync(act, mockPOMExecutionUtil.Object);

            // Assert
            Assert.AreEqual(locators[0].LocateValue, result[0].LocateValue);
            Assert.AreEqual(locators[0].LocateBy, result[0].LocateBy);
            Assert.AreEqual(locators[1].LocateValue, result[1].LocateValue);
            Assert.AreEqual(locators[1].LocateBy, result[1].LocateBy);
            Assert.AreEqual(locators[2].LocateValue, result[2].LocateValue);
            Assert.AreEqual(locators[2].LocateBy, result[2].LocateBy);
        }

        [TestMethod]
        public void GetElementLocatorForWebSmartSync_AllSupportedMappings()
        {
            // XPath
            var by1 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByXPath, "//div[@id='a']");
            Assert.AreEqual("By.XPath: //div[@id='a']", by1.ToString());

            // Rel XPath
            var by2 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByRelXPath, "//span");
            Assert.AreEqual("By.XPath: //span", by2.ToString());

            // ID
            var by3 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByID, "user");
            Assert.AreEqual("By.Id: user", by3.ToString());

            // Name
            var by4 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByName, "country");
            Assert.AreEqual("By.Name: country", by4.ToString());

            // ClassName
            var by5 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByClassName, "mainCls");
            Assert.AreEqual("By.ClassName[Contains](" + "mainCls)", by5.ToString().Replace("By.ClassName[Contains]: mainCls", "By.ClassName[Contains](mainCls)")); // some drivers format differently

            // CSS
            var by6 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByCSSSelector, "div.panel");
            Assert.AreEqual("By.CssSelector: div.panel", by6.ToString());

            // LinkText
            var by7 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByLinkText, "Login");
            Assert.AreEqual("By.LinkText: Login", by7.ToString());

            // TagName
            var by8 = SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.ByTagName, "input");
            Assert.AreEqual("By.TagName: input", by8.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(System.Exception))]
        public void GetElementLocatorForWebSmartSync_UnsupportedLocator_Throws()
        {
            // eLocateBy.NA should trigger exception
            SeleniumDriver.GetElementLocatorForWebSmartSync(eLocateBy.NA, "x");
        }

        [TestMethod]
        public void WebSmartSyncGetMaxTimeout_UsesActionTimeout()
        {
            var driver = new SeleniumDriver();
            driver.ImplicitWait = 25;

            var act = new ActWebSmartSync
            {
                Timeout = 7,
                SyncOperations = ActWebSmartSync.eSyncOperation.ElementExists,
                ElementLocateBy = eLocateBy.ByXPath,
                ElementLocateValue = "//div"
            };

            int timeout = driver.WebSmartSyncGetMaxTimeout(act);
            Assert.AreEqual(7, timeout);
        }

        [TestMethod]
        public void WebSmartSyncGetMaxTimeout_FallbackToImplicitWait()
        {
            var driver = new SeleniumDriver();
            driver.ImplicitWait = 18;

            var act = new ActWebSmartSync
            {
                Timeout = 0,
                SyncOperations = ActWebSmartSync.eSyncOperation.ElementExists,
                ElementLocateBy = eLocateBy.ByXPath,
                ElementLocateValue = "//div"
            };

            int timeout = driver.WebSmartSyncGetMaxTimeout(act);
            Assert.AreEqual(18, timeout);
        }

        [TestMethod]
        public void GetSearchedWinTitle_ForAct()
        {
            var driver = new SeleniumDriver();
            var act = new ActUIElement
            {
                ElementLocateBy = eLocateBy.ByTitle,
                ElementLocateValue = "Main App"
            };
            string title = driver.GetSearchedWinTitle(act);
            Assert.AreEqual("Main App", title);
        }

        [TestMethod]
        public void GetSearchedWinTitle_ForActUIElement()
        {
            var driver = new SeleniumDriver();
            var actUI = new ActUIElement
            {
                ElementLocateValue = "Popup Dialog",
                ElementLocateBy = eLocateBy.ByTitle
            };
            string title = driver.GetSearchedWinTitle(actUI);
            Assert.AreEqual("Popup Dialog", title);
        }

        [TestMethod]
        public void GetElementTypeEnum_InputText_ReturnsTextBox()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body><input id='u1' type='text' /></body></html>");
            var node = doc.DocumentNode.SelectSingleNode("//input");
            var tup = SeleniumDriver.GetElementTypeEnum(htmlNode: node);
            Assert.AreEqual("input", tup.Item1.ToLower());
            Assert.AreEqual(eElementType.TextBox, tup.Item2);
        }

        [TestMethod]
        public void GetElementTypeEnum_SvgRoot_ReturnsSvg()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body><svg id='chart'></svg></body></html>");
            var node = doc.DocumentNode.SelectSingleNode("//svg");
            var tup = SeleniumDriver.GetElementTypeEnum(htmlNode: node);
            Assert.AreEqual(eElementType.Svg, tup.Item2);
        }

        [TestMethod]
        public void GetElementTypeEnum_SvgChild_GWithTypeAttribute_ReturnsSvg()
        {
            // Simulate a <g> element flagged as SVG via TypeAtt
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body><svg><g id='groupA'></g></svg></body></html>");
            var node = doc.DocumentNode.SelectSingleNode("//g");
            var tup = SeleniumDriver.GetElementTypeEnum(htmlNode: node, TypeAtt: "Svg");
            Assert.AreEqual(eElementType.Svg, tup.Item2);
        }

        [TestMethod]
        public void GetElementTypeEnum_UnknownTag_ReturnsUnknown()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body><custom-widget data-x='1'></custom-widget></body></html>");
            var node = doc.DocumentNode.SelectSingleNode("//custom-widget");
            var tup = SeleniumDriver.GetElementTypeEnum(htmlNode: node);
            Assert.AreEqual(eElementType.Unknown, tup.Item2);
        }
    }
}
