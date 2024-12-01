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
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using GingerCore;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTest.AnalyzerTests
{
    [TestClass]
    [Level1]
    public class AnalyzerTest
    {
        static SolutionRepository SR;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            string path = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution");
            SR = WorkspaceHelper.CreateWorkspaceAndOpenSolution(path);
            SR.StopAllRepositoryFolderWatchers();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            SR.StopAllRepositoryFolderWatchers();
        }

        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzeBusinessFlowTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Demo Flow 01.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);

            ObservableList<AnalyzerItemBase> mIssues = [];
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreNotEqual(0, mIssues.Count);

        }

        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzerVariableUsedOnlyInSetVariableTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "MissingVariableUsedOnlyInSetVariable.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);


            ObservableList<AnalyzerItemBase> mIssues = [];
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreEqual(0, mIssues.Count);

        }


        [TestMethod]
        [Timeout(60000)]
        public void MissingVariableUsedOnlyInSetVariableActionTest()
        {
            //Arrange            
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "MissingVariableUsedOnlyInSetVariable.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);


            ObservableList<AnalyzerItemBase> mIssues = [];
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            businessFlow.Variables.Remove(businessFlow.GetVariable("username"));


            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreEqual(1, mIssues.Count);
            Assert.AreEqual(AnalyzerItemBase.eSeverity.High, mIssues[0].Severity);
            Assert.AreEqual("The Variable 'username' is missing", mIssues[0].Description);
            Assert.AreEqual(AnalyzerItemBase.eType.Error, mIssues[0].IssueType);
            Assert.AreEqual(AnalyzerItemBase.eCanFix.Yes, mIssues[0].CanAutoFix, "Auto Fix validation when missing variable is used only in Set variable action");

        }


        [TestMethod]
        [Timeout(60000)]
        public void MissingVariableUsedinMultipleActionsTest()
        {
            //Arrange          
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "MissingVariableUsedInMultipleActions.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);


            ObservableList<AnalyzerItemBase> mIssues = [];
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            businessFlow.Variables.Remove(businessFlow.GetVariable("username"));


            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreEqual(4, mIssues.Count);
            AnalyzerItemBase missingVariableIssue = mIssues.FirstOrDefault(x => x.IssueCategory == AnalyzerItemBase.eIssueCategory.MissingVariable);

            Assert.AreEqual(AnalyzerItemBase.eSeverity.High, missingVariableIssue.Severity);
            Assert.AreEqual("The Variable 'username' is missing", missingVariableIssue.Description);
            Assert.AreEqual(AnalyzerItemBase.eType.Error, missingVariableIssue.IssueType);
            Assert.AreEqual(AnalyzerItemBase.eCanFix.No, missingVariableIssue.CanAutoFix, "Auto Fix validation when missing variable is used in multiple actions");

        }


    }

}
