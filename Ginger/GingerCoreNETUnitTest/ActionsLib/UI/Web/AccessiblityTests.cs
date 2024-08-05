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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.Repository;
using Deque.AxeCore.Selenium;
using Ginger.AnalyzerLib;
using Ginger.Configurations;
using GingerCore;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml.Attributes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace GingerCoreNETUnitTest.ActionsLib.UI.Web
{
    [TestClass]
    [TestCategory(TestCategory.IntegrationTest)]
    public class AccessiblityTests
    {

        private static IWebDriver _driver;
        static ActAccessibilityTesting mact;
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
            mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.ByStandard);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                mact.SeverityList = new ObservableList<OperationValues>();
                mact.StandardList = GetStandardTagslist();
            }
            mact.AnalyzerAccessibility(_driver, e);
            Assert.AreEqual(mact.Status,Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
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
            mact = new ActAccessibilityTesting();
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
            mact = new ActAccessibilityTesting();
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Target, ActAccessibilityTesting.eTarget.Page);
            mact.GetOrCreateInputParam(ActAccessibilityTesting.Fields.Analyzer, ActAccessibilityTesting.eAnalyzer.ByStandard);
            if (mact.GetInputParamValue(ActAccessibilityTesting.Fields.Analyzer) == nameof(ActAccessibilityTesting.eAnalyzer.ByStandard))
            {
                mact.SeverityList = new ObservableList<OperationValues>();
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
            mact = new ActAccessibilityTesting();
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
            ObservableList<AccessibilityRuleData> ruleList = mact.GetRuleList();
            Assert.AreNotEqual(null, ruleList);
            Assert.AreEqual(93, ruleList.Count);
        }

        public static ObservableList<OperationValues> GetStandardTagslist()
        {
            ObservableList<OperationValues> StandardTagList = new ObservableList<OperationValues>();
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2a) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag2aa) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21a) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag21aa) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22a) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.wcag22aa) });
            StandardTagList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eTags.bestpractice) });
            return StandardTagList;
        }

        public static ObservableList<OperationValues> GetSeverityList()
        {
            ObservableList<OperationValues> SeverityList = new ObservableList<OperationValues>();

            SeverityList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Critical) });
            SeverityList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Serious) });
            SeverityList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Moderate) });
            SeverityList.Add(new OperationValues() { Value = nameof(ActAccessibilityTesting.eSeverity.Minor) });
            return SeverityList;
        }
    }
}
