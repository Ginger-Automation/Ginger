#region License
/*
Copyright © 2014-2018 European Support Limited
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
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class ScriptActionTest
    {
        static GingerRunner mGR = null;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            BusinessFlow mBF;
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Script Action Test";
            mBF.Active = true;
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;
            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.CurrentBusinessFlow = mBF;
            mGR.BusinessFlows.Add(mBF);
            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }

        [TestMethod]
        public void RunScriptAction()
        {
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand= ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(@"Script\SampleScript.vbs");
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Arrange
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(2, actScript.ReturnValues.Count);

        }
    }

}
