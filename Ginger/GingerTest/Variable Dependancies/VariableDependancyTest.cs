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


using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerTest.Variable_Dependancies
{
    [TestClass]
    [Level1]
    public class VariableDependancyTest
    {
        static BusinessFlow BF1;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            

            BF1 = new BusinessFlow();
            BF1.Name = "VariableDependancyTest";
            BF1.Active = true;

            ActivitiesGroup activityGroup = new ActivitiesGroup();

            Activity activity1 = new Activity();
            activity1.Active = true;

            ActDummy actDummy1 = new ActDummy();
            actDummy1.Active = true;
            ActDummy actDummy2 = new ActDummy();
            actDummy2.Active = true;

            activity1.Acts.Add(actDummy1);
            activity1.Acts.Add(actDummy2);
            

            Activity activity2 = new Activity();
            activity2.Active = true;

            ActDummy actDummy3 = new ActDummy();
            actDummy3.Active = true;
            ActDummy actDummy4 = new ActDummy();
            actDummy4.Active = true;

            ActDummy actDummy7 = new ActDummy();
            actDummy7.Active = true;
            ActDummy actDummy8 = new ActDummy();
            actDummy8.Active = true;

            activity2.Acts.Add(actDummy3);
            activity2.Acts.Add(actDummy4);
            activity2.Acts.Add(actDummy7);
            activity2.Acts.Add(actDummy8);

            Activity activity3 = new Activity();
            activity3.Active = true;

            ActDummy actDummy5 = new ActDummy();
            actDummy5.Active = true;
            ActDummy actDummy6 = new ActDummy();
            actDummy6.Active = true;

            activity3.Acts.Add(actDummy5);
            activity3.Acts.Add(actDummy6);

            activityGroup.AddActivityToGroup(activity1);
            activityGroup.AddActivityToGroup(activity2);
            activityGroup.AddActivityToGroup(activity3);

            BF1.Activities.Add(activity1);
            BF1.Activities.Add(activity2);
            BF1.Activities.Add(activity3);

            BF1.ActivitiesGroups.Add(activityGroup);

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            BF1.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
                WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }



            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Name = "Test Runner";
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            ProjEnvironment environment = new ProjEnvironment();
            environment.Name = "Default";
            BF1.Environment = environment.Name;

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();

            mGR.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.Executor.BusinessFlows.Add(BF1);

            Context context1 = new Context();
            context1.BusinessFlow = BF1;
            context1.Activity = BF1.Activities[0];

            mGR.Executor.CurrentBusinessFlow = BF1;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = BF1.Activities[0];
            mGR.Executor.Context = context1;
        }

        [TestMethod]
        //[Timeout(60000)]
        public void ActivityVariablesDependancyTest()
        {
            //Arrange
            ObservableList<Activity> activityList = BF1.Activities;

            BF1.EnableActivitiesVariablesDependenciesControl = true;

            //Added selection list variable in BF
            VariableSelectionList selectionList = new VariableSelectionList();
            selectionList.OptionalValuesList.Add(new OptionalValue("a"));
            selectionList.OptionalValuesList.Add(new OptionalValue("b"));
            selectionList.SelectedValue = selectionList.OptionalValuesList[0].Value;
            selectionList.ItemName = "MyVar";
            BF1.Variables.Add(selectionList);

            //Added dependancies in activities

            string[] variableValues = { "a", "b" };

            VariableDependency actiVD0 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues);
            activityList[0].VariablesDependencies.Add(actiVD0);

            VariableDependency actiVD1 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues);
            activityList[1].VariablesDependencies.Add(actiVD1);

            VariableDependency actiVD2 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues[1]);
            activityList[2].VariablesDependencies.Add(actiVD2);
            
            //Act
            mGR.Executor.RunBusinessFlow(BF1);

            //Assert
            Assert.AreEqual(BF1.Activities[0].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[2].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped);

            //Changed the selected value of selection list 
            ((VariableSelectionList)BF1.Variables[0]).SelectedValue = selectionList.OptionalValuesList[1].Value;

            //Act
            mGR.Executor.RunBusinessFlow(BF1);

            //Assert
            Assert.AreEqual(BF1.Activities[0].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[2].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionVariablesDependancyTest()
        {
            //Arrange
            ObservableList<Activity> activityList = BF1.Activities;

            Activity activity = activityList[1];
            ObservableList<IAct> actionList = activity.Acts;

            activity.EnableActionsVariablesDependenciesControl = true;

            mGR.Executor.CurrentBusinessFlow.CurrentActivity = BF1.Activities[1];
            //Added selection list variable in activity
            VariableSelectionList selectionList = new VariableSelectionList();
            selectionList.OptionalValuesList.Add(new OptionalValue("abc"));
            selectionList.OptionalValuesList.Add(new OptionalValue("xyz"));
            selectionList.SelectedValue = selectionList.OptionalValuesList[1].Value;
            selectionList.ItemName = "MyActVar";
            activity.Variables.Add(selectionList);

            //added action level dependancies

            string[] variableValues = { "abc", "xyz" };

            VariableDependency actiVD0 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues[0]);
            actionList[0].VariablesDependencies.Add(actiVD0);

            VariableDependency actiVD1 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues);
            actionList[1].VariablesDependencies.Add(actiVD1);

            VariableDependency actiVD3 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues);
            actionList[2].VariablesDependencies.Add(actiVD3);

            VariableDependency actiVD4 = new VariableDependency(selectionList.Guid, selectionList.ItemName, variableValues[0]);
            actionList[3].VariablesDependencies.Add(actiVD4);

            //Act
            mGR.Executor.RunActivity(activity);

            //Assert
            Assert.AreEqual(BF1.Activities[1].Acts[0].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped);
            Assert.AreEqual(BF1.Activities[1].Acts[1].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Acts[2].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Acts[3].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped);
            
            //Changed the selected value of selection list
            ((VariableSelectionList)activity.Variables[0]).SelectedValue = selectionList.OptionalValuesList[0].Value;

            //Act
            mGR.Executor.RunActivity(activity);

            //Assert
            Assert.AreEqual(BF1.Activities[1].Acts[0].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Acts[1].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Acts[2].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(BF1.Activities[1].Acts[3].Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }
    }
}