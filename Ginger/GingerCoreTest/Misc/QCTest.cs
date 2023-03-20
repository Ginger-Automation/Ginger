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


//using GingerCore.ALM.QC;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace UnitTests.NonUITests
//{
//    [TestClass]
//    [Ignore]
//    public class QCTest 
//    {
//        [TestInitialize]
//        public void TestInitialize()
//        {
//            //PUT HERE ONLY INIT CODE RUN ONCE - so first we connect and do login, all other test method should assume we have successfull conenction       
//            bool bConnected = QCConnect.ConnectQCServer("http://qcstlyphdmz:8080/qcbin", UserControls , "");
//            bool bLogin = false;
//            if (bConnected)
//            {
//                bLogin = QCConnect.ConnectQCProject("DEFAULT", "NEWGEN_SYS_TEST");
//                if (!bLogin)
//                {
//                    throw new Exception("Cannot Login to QC");
//                }
//            }
//            else
//            {
//                throw new Exception("Cannot Connect to QC");
//            }
//        }


//        [TestMethod]  [Timeout(60000)]
//        public void CheckConnected()
//        {
//            //Arrange

//            //Act
//            bool b = QCConnect.IsServerConnected;

//            //Assert
//            Assert.IsTrue(b);
//        }


//        [TestMethod]  [Timeout(60000)]
//        public void GetDomains()
//        {
//            // Arrange

//            //Act
//            List<string> Domains = QCConnect.GetQCDomains();

//            //Assert
//            Assert.IsTrue(Domains.Count() > 0);
//           Assert.AreEqual(Domains[0], "Dom1");
//           Assert.AreEqual(Domains[1], "Dom2");
//            //...
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void GetDomainProjects()
//        {
//            // Arrange

//            //Act
//            List<string> DomainProject = QCConnect.GetQCDomainProjects("aaa");

//            //Assert
//            Assert.IsTrue(DomainProject.Count() > 0);
//           Assert.AreEqual(DomainProject[0], "Proj1");
//           Assert.AreEqual(DomainProject[1], "Proj2");
//            //...
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void AddTestPlan()
//        {
//            // Arrange
//            //string TesCaseName = "Create AutoRenew";
//            //string Subject = "Subject\\Automation";            

//            //Act
//            //int Testid = QCConnect.TestPlanAddTest(Subject, TesCaseName);

//            //Assert
//            //Assert.IsTrue(TPID > 0);            
//            //...
//        }


//        //[TestMethod]  [Timeout(60000)]
//        //public void AddTestToTestLab()
//        //{
//        //    //TODO:
//        //    // Arrange
//        //    string TesCaseName = "Create Account";
//        //    string Subject = "Subject\\Automation"; // need a UI design - Work in progress
//        //    string FolderPath = "Root\\Automation";
//        //    string FolderName = "GingerQC";
//        //    string TestCaseStatus = "Passed";
//        //    string TestStepStatus = "Passed";
//        //    String StepName = "Login";
//        //    String StepDesc = "Enter URL";
//        //    String StepExpected = "Login Succesfull";

//        //    // AddTestPlan
//        //    int Testid = ExportToQC.TestPlanAddTest(Subject, TesCaseName);

//        //    //Add Activities
//        //    int StepID = ExportToQC.AddDesignSteps(Subject, Testid, StepName, StepDesc, StepExpected);

//        //    //Upload Test Cases to Test lab & its statuses         
//        //   // bool bUploaded = QCConnect.TestLabAddTCStatus(FolderPath, FolderName, TestCaseStatus, TestStepStatus, Testid);

//        //    //Assert
//        //   // Assert.IsTrue(bUploaded);
//        //}

//        [TestMethod]  [Timeout(60000)]
//        public void CreateDefect()
//        {
//            // Arrange
//            String Status = "New";
//            String Summary = "GingerTest";
//            String DetectedBy = "Swapnak";
//            String Version = "V1";

//            //Act
//            int BugID = ExportToQC.CreateDefect(Status, Summary, DetectedBy, Version);

//            //Assert
//            Assert.IsTrue(BugID > 0);
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void GetTestPlanExplorer()
//        {
//            // Arrange           

//            //Act
//            List<string> testPlanPathList = QCConnect.GetTestPlanExplorer("Subject");

//            //Assert            
//            Assert.IsTrue(testPlanPathList.Count() > 0);

//            //...
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void GetTestLabExplorer()
//        {
//            // Arrange           

//            //Act
//            List<string> testlabPathList = QCConnect.GetTestLabExplorer("Root");

//            //Assert            
//            Assert.IsTrue(testlabPathList.Count() > 0);

//            //...
//        }
//    }
//}

