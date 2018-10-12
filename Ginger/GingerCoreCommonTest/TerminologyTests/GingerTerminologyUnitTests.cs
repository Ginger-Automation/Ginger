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
        public void GingerDefaultTerm_TestActivities()
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
        public void GingerGherkinTerm_TestActivities()
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
        public void GingerTestingTerm_TestActivities()
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
        public void GingerDefaultTerm_TestRunset()
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
        public void GingerGherkinTerm_TestRunset()
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
        public void GingerTestingTerm_TestRunset()
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
        public void GingerDefaultTerm_TestBusinessFlows()
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
        public void GingerGherkinTerm_TestBusinessFlows()
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
        public void GingerTestingTerm_TestBusinessFlows()
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
        public void GingerDefaultTerm_TestVariable()
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
        public void GingerGherkinTerm_TestVariable()
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
        public void GingerTestingTerm_TestVariable()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.Variable;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Parameter", termResValue);
        }

        //ActivitiesGroup

        [TestMethod]
        public void GingerDefaultTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Default;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Activities Group", termResValue);
        }

        [TestMethod]
        public void GingerGherkinTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Gherkin;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Scenario", termResValue);
        }

        [TestMethod]
        public void GingerTestingTerm_TestActivitiesGroup()
        {
            //Arrange
            GingerTerminology.SET_TERMINOLOGY_TYPE = eTerminologyDicsType.Testing;
            eTermResKey termResourceKey = eTermResKey.ActivitiesGroup;

            //Act
            string termResValue = GingerTerminology.getTerminologyValue(termResourceKey);

            //Assert
            Assert.AreEqual("Test Case", termResValue);
        }


    }

} //namespace
