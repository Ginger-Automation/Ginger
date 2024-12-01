#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Deque.AxeCore.Selenium;
using Ginger.Configurations;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace GingerCoreNETUnitTest.ActionsLib.UI.Web
{
    [TestClass]
    [TestCategory(TestCategory.IntegrationTest)]
    public class AccessiblityTests
    {

        private static IWebDriver _driver;
        IWebElement e = null;

        [ClassInitialize()]
        public static void ClassInit(TestContext TC)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            _driver = new ChromeDriver();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _driver.Quit();
        }

        public static void InitDriver()
        {
            //// Initialize the WebDriver (Chrome in this case)
            _driver = new ChromeDriver();
        }
        public static void CleanDriver()
        {
            _driver.Quit();
        }

        [TestMethod]
        [TestProperty("ExecutionOrder", "1")]
        public void TestAnalyzerAccessibility_Standard_Failed()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            _driver.Navigate().GoToUrl($"https://ginger.amdocs.com/");
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.ByStandard);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                mact.SeverityList = [];
                mact.StandardList = GetStandardTagslist();
            }
            mact.AnalyzerAccessibility(_driver, e);
            Assert.AreEqual(mact.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }

        [TestMethod]
        [TestProperty("ExecutionOrder", "2")]
        public void TestAnalyzerAccessibility_Severity_Failed()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            _driver.Navigate().GoToUrl($"https://ginger.amdocs.com/");
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.BySeverity);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.BySeverity))
            {
                mact.SeverityList = GetSeverityList();
                mact.SeverityList.RemoveAt(1);
            }
            mact.AnalyzerAccessibility(_driver, e);
            Assert.AreEqual(mact.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }
        [TestMethod]
        [TestProperty("ExecutionOrder", "3")]
        public void TestAnalyzerAccessibility_Standard_Pass()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            string htmlFilePath = Path.Combine(TestResources.GetTestResourcesFolder("Html"), "TestAccessiblity.html");
            _driver.Navigate().GoToUrl(htmlFilePath);
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.ByStandard);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                mact.SeverityList = [];
                mact.StandardList = GetStandardTagslist();
            }
            mact.AnalyzerAccessibility(_driver, e);
            Assert.AreEqual(mact.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }
        [TestMethod]
        [TestProperty("ExecutionOrder", "4")]
        public void TestAnalyzerAccessibility_Severity_Pass()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            string htmlFilePath = Path.Combine(TestResources.GetTestResourcesFolder("Html"), "TestAccessiblity.html");
            _driver.Navigate().GoToUrl(htmlFilePath);
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.BySeverity);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.BySeverity))
            {
                mact.SeverityList = GetSeverityList();
                mact.SeverityList.RemoveAt(1);
            }
            mact.AnalyzerAccessibility(_driver, e);
            Assert.AreEqual(mact.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }
        [TestMethod]
        [TestCategory(TestCategory.UnitTest)]
        public void TestCreateAxeBuilder_IsNotNull()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            AxeBuilder axeBuilder = mact.CreateAxeBuilder(_driver);
            Assert.AreNotEqual(null, axeBuilder);
        }
        [TestMethod]
        [TestCategory(TestCategory.UnitTest)]
        public void TestGetRuleList_isNotNull()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            ActAccessibilityTesting mact = new ActAccessibilityTesting();
            ObservableList<AccessibilityRuleData> ruleList = mact.GetRuleList();
            Assert.AreNotEqual(null, ruleList);
            Assert.AreEqual(92, ruleList.Count);
        }

        public static ObservableList<OperationValues> GetStandardTagslist()
        {
            ObservableList<OperationValues> StandardTagList =
            [
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2a) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2aa) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21a) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21aa) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22a) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22aa) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.bestpractice) },
            ];
            return StandardTagList;
        }

        public static ObservableList<OperationValues> GetSeverityList()
        {
            ObservableList<OperationValues> SeverityList =
            [
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Critical) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Serious) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Moderate) },
                new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Minor) },
            ];
            return SeverityList;
        }
    }
}
