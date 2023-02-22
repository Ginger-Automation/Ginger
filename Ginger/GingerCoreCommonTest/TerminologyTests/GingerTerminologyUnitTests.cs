#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.TerminologyTests
{
    [TestClass]
    public class GingerTerminologyUnitTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // before every test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //after every test
        }

        #region Business Flows

        [TestMethod]  [Timeout(60000)]
        public void GingerDefaultTerm_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Business Flows", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Business Flow Features", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Test Sets", termValue);
        }

        #endregion

        #region ActivitiesGroup

        [TestMethod]  [Timeout(60000)]
        public void GingerDefaultTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Activities Group", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Scenario", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Test Case", termValue);
        }
        #endregion

        #region Activities

        [TestMethod]  [Timeout(60000)]
        public void GingerDefaultTerm_TestActivities()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            eTermResKey termResourceKey = eTermResKey.Activities;
            
            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Activities", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivities()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.Activities;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Steps", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestActivities()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;
            eTermResKey termResourceKey = eTermResKey.Activities;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Steps", termValue);
        }
        #endregion

        #region Variable

        [TestMethod]  [Timeout(60000)]
        public void GingerDefaultTerm_TestVariable()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Variable", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestVariable()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Parameter", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestVariable()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Parameter", termValue);
        }
        #endregion
        
        #region Runset

        [TestMethod]  [Timeout(60000)]
        public void GingerDefaultTerm_TestRunset()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Run Set", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestRunset()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Run Set", termValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestRunset()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termValue = GingerTerminology.GetTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Calendar", termValue);
        }
        #endregion
                
        #region Prefix and Suffix Tests for Business Flow - Gherkin Terminology

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlowPrefix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre");

            //Assert
            Assert.AreEqual("TestPre Business Flow Feature", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlowSuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "", "TestSuffix");

            //Assert
            Assert.AreEqual("Business Flow Feature TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlowCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "", "", true);

            //Assert
            Assert.AreEqual("Business Flow Feature".ToUpper(), termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlowPrefixSuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre", "TestSuffix", false);

            //Assert
            Assert.AreEqual("TestPre Business Flow Feature TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestBusinessFlowPrefixSuffixCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre", "TestSuffix", true);

            //Assert
            Assert.AreEqual("TestPre Business Flow Feature TestSuffix".ToUpper(), termResValue);
        }
        #endregion

        #region Prefix and Suffix Tests for Business Flow - Testing Terminology

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlowPrefix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre");

            //Assert
            Assert.AreEqual("TestPre Test Set", termResValue);
        }


        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlowSuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "", "TestSuffix");

            //Assert
            Assert.AreEqual("Test Set TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlowCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "", "", true);

            //Assert
            Assert.AreEqual("Test Set".ToUpper(), termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlowPrefixSuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre", "TestSuffix", false);

            //Assert
            Assert.AreEqual("TestPre Test Set TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerTestingTerm_TestBusinessFlowPrefixSuffixCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, "TestPre", "TestSuffix", true);

            //Assert
            Assert.AreEqual("TestPre Test Set TestSuffix".ToUpper(), termResValue);
        }
        #endregion

        #region Prefix and Suffix Tests for Activity - Gherkin Terminology

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivityPrefix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Testing;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.Activity, "TestPre");

            //Assert
            Assert.AreEqual("TestPre Step", termResValue);
        }


        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivitySuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.Activity, "", "TestSuffix");

            //Assert
            Assert.AreEqual("Step TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivityCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.Activity, "", "", true);

            //Assert
            Assert.AreEqual("Step".ToUpper(), termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivityPrefixSuffix()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.Activity, "TestPre", "TestSuffix", false);

            //Assert
            Assert.AreEqual("TestPre Step TestSuffix", termResValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void GingerGherkinTerm_TestActivityPrefixSuffixCase()
        {
            //Arrange
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Gherkin;

            //Act
            string termResValue = GingerDicser.GetTermResValue(eTermResKey.Activity, "TestPre", "TestSuffix", true);

            //Assert
            Assert.AreEqual("TestPre Step TestSuffix".ToUpper(), termResValue);
        }
        #endregion
        
    }

} //namespace
