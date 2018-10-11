using GingerCore;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreCommonTest.TerminologyTests
{
    [TestClass]
    public class GingerTerminologyUnitTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            //
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
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

        [TestMethod]
        public void GingerTermDefault_TestActivities()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Default;
            eTermResKey termResourceKey = eTermResKey.Activities;
            
            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Activities", termResValue);
        }

        [TestMethod]
        public void GingerTermGherkin_TestActivities()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.Activities;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Steps", termResValue);
        }

        [TestMethod]
        public void GingerTermTesting_TestActivities()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.Activities;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Steps", termResValue);
        }


        [TestMethod]
        public void GingerTermDefault_TestRunset()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Default;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Run Set", termResValue);
        }

        [TestMethod]
        public void GingerTermGherkin_TestRunset()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Run Set", termResValue);
        }

        [TestMethod]
        public void GingerTermTesting_TestRunset()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.RunSet;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Calendar", termResValue);
        }


        [TestMethod]
        public void GingerTermDefault_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Default;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Business Flow Features", termResValue);
        }

        [TestMethod]
        public void GingerTermGherkin_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Business Flow Features", termResValue);
        }

        [TestMethod]
        public void GingerTermTesting_TestBusinessFlows()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.BusinessFlows;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Test Sets", termResValue);
        }

        [TestMethod]
        public void GingerTermDefault_TestVariable()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Default;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Variable", termResValue);
        }

        [TestMethod]
        public void GingerTermGherkin_TestVariable()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Parameter", termResValue);
        }

        [TestMethod]
        public void GingerTermTesting_TestVariable()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Parameter", termResValue);
        }

        [TestMethod]
        public void GingerTerm_NoTermSet_TestVariable()
        {
            //Arrange
            //When terminology is not set, it should pull the default value which is from Default Terminology
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Variable", termResValue);
        }

        [TestMethod]
        public void GingerTerm_NoTermSet_TestBusinessFlow()
        {
            //Arrange
            //When terminology is not set, it should pull the default value which is from Default Terminology
            eTermResKey termResourceKey = eTermResKey.BusinessFlow;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Business Flow Feature", termResValue);
        }

        [TestMethod]
        public void GingerTerm_NoTermSet_TestActivity()
        {
            //Arrange
            //When terminology is not set, it should pull the default value which is from Default Terminology
            eTermResKey termResourceKey = eTermResKey.Activity;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Activity", termResValue);
        }

    }

} //namespace
