#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNETUnitTest.RunTestslib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace UnitTests.NonUITests.GingerRunnerTests
{

    [TestClass]
    [Level1]
    public class GingerVariableTests
    {

        // static WorkspaceLocker mWorkspaceLocker = new WorkspaceLocker("GingerVariableTests");

        static BusinessFlow mBF;
        static GingerRunner mGR;
        static Solution solution;
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test Fire Fox";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;            
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString busFlowV1 = new VariableString() { Name = "BFV1", InitialStringValue = "1" };
            mBF.AddVariable(busFlowV1);

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent a = new Agent();            
            a.AgentType = Agent.eAgentType.Service; // Simple agent which anyhow we don't need to start for this test and will work on Linux

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.Executor.BusinessFlows.Add(mBF);

            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple"));
            string solutionFile = System.IO.Path.Combine(path, @"Ginger.Solution.xml");
            solution = SolutionOperations.LoadSolution(solutionFile);
            WorkSpace.Instance.SolutionRepository.Open(path);
            WorkSpace.Instance.Solution = solution;
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }

        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            
        }


        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }

        [TestMethod]  [Timeout(60000)]
        public void TestVariable_StringSetValue()
        {
            //Arrange
            string initialValue = "123";
            string variableName = "V1";
            string newValue = "abc";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);

            VariableString v1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.SetValue, Value = newValue, Active=true };
            activity1.Acts.Add(actSetVariableValue);
            
            //Act            
            mGR.Executor.RunRunner();

            //Assert
            
            Assert.AreEqual(eRunStatus.Passed, actSetVariableValue.Status);
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.AreEqual(newValue, v1.Value );
        }

        [TestMethod]  [Timeout(60000)]
        public void TestVariable_StringResetValue()
        {
            //Arrange
            string initialValue = "123";
            string variableName = "V1";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);

            VariableString v1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.ResetValue, Value = "123", Active = true };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus );
            Assert.AreEqual(eRunStatus.Passed, activity1.Status );
            Assert.AreEqual(initialValue, v1.Value );
        }

        [TestMethod]  [Timeout(60000)]
        public void TestVariable_StringClearSpecialChar()
        {
            //Arrange
            string initialValue = "S$tr!ing@123#";
            string variableName = "V1";
            string specialCharacters = "$@#!";
            string expectedValue = RemoveSpecialCharacters(initialValue, specialCharacters.ToCharArray());
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);

            VariableString v1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.ClearSpecialChar, Active = true, Value = specialCharacters };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.AreEqual(expectedValue, mBF.Activities[0].Variables[0].Value);
            Assert.AreEqual(expectedValue, v1.Value);
        }

        private string RemoveSpecialCharacters(string value, char[] specialCharacters)
        {
            return new String(value.Except(specialCharacters).ToArray());
        }

        [TestMethod]  [Timeout(60000)]
        public void TestVariable_PasswordStringSetValue()
        {
            //Arrange
            string initialValue = "123";
            string variableName = "V1";
            string newValue = "abc";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);

            VariablePasswordString v1 = new VariablePasswordString() { Name = variableName, Password = initialValue };
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.SetValue, Value = newValue, Active = true };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.AreEqual(initialValue, v1.Value);
        }


        [TestMethod]  [Timeout(60000)]
        public void TestVariable_RandomNumberSetValue()
        {
            //Arrange
            int min = 1;
            int max = 5;
            string variableName = "V1";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);


            VariableRandomNumber v1 = new VariableRandomNumber() { Name = variableName, Interval = 1, Min = min, Max = max };
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.AutoGenerateValue, Active = true };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            decimal num1 = decimal.Parse(v1.Value);
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.IsTrue(num1 >= min && num1 <= max, "num1 >= " + min + " && num1 <= " + max);
        }


        [TestMethod]  [Timeout(60000)]
        public void TestVariable_RandomStringSetValue()
        {
            //Arrange
            int maxChars = 5;
            string variableName = "V1";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);


            VariableRandomString v1 = new VariableRandomString() { Name = variableName, Max = maxChars, IsUpperCase = true};
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.AutoGenerateValue, Active = true };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.AreEqual(v1.Value.ToUpper(), v1.Value);
        }


        [TestMethod]  [Timeout(60000)]
        public void TestVariable_SelectionListSetValue()
        {
            //Arrange
            string variableName = "V1";
            ResetBusinessFlow();

            Activity activity1 = new Activity() { Active = true };
            mBF.Activities.Add(activity1);

            VariableSelectionList v1 = new VariableSelectionList() { Name = variableName};
            v1.OptionalValuesList.Add(new OptionalValue("Jupiter"));
            v1.OptionalValuesList.Add(new OptionalValue("Saturn"));
            activity1.AddVariable(v1);

            ActSetVariableValue actSetVariableValue = new ActSetVariableValue() { VariableName = variableName, SetVariableValueOption = VariableBase.eSetValueOptions.SetValue, Active = true, Value="Jupiter" };
            activity1.Acts.Add(actSetVariableValue);

            //Act            
            mGR.Executor.RunRunner();

            //Assert
            Assert.IsTrue(string.IsNullOrEmpty(actSetVariableValue.Error));
            Assert.AreEqual(eRunStatus.Passed, mBF.RunStatus);
            Assert.AreEqual(eRunStatus.Passed, activity1.Status);
            Assert.AreEqual("Jupiter", v1.Value);
        }
    }
}
