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

using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Environments;
using Ginger;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;

namespace UnitTests.NonUITests
{

    [TestClass]
    public class GingersMultiRunTest
    {
        RunsetExecutor mGMR;
        ProjEnvironment mEnv;
        BusinessFlow mBF;
        GingerRunner mGR;

        [TestInitialize]
        public void TestInitialize()
        {
            mGMR = new RunsetExecutor();
            
            mEnv = new ProjEnvironment();
            
            mGMR.RunSetConfig = new RunSetConfig();
            
            mGMR.RunsetExecutionEnvironment = new ProjEnvironment();
            mEnv = mGMR.RunsetExecutionEnvironment;
            WorkSpace.UserProfile = new Ginger.UserProfile();
            WorkSpace.UserProfile.Solution = new Ginger.SolutionGeneral.Solution();

            mGMR.RunSetConfig.mRunWithAnalyzer = false;

            AddGinger(Agent.eDriverType.SeleniumChrome);
        }

        [TestCleanup()]
        public void TestCleanUp()
        {
            foreach (GingerRunner GR in mGMR.Runners)
            {
                GR.StopAgents();
            }
        }
        
        private void AddGinger(Agent.eDriverType DriverType)
        {
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Status Result Test";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.Platforms = new ObservableList<Platform>();
            //mBF.Platforms.Add(p);
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });


            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            //mGR.GiveUserFeedback = true;

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;
            mGR.SolutionAgents = new ObservableList<IAgent>();
            mGR.SolutionAgents.Add(a);

            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web });
            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });

            mGR.BusinessFlows.Add(mBF);
        }


        [TestMethod]
        public void RunSingleGinger()
        {
            //Act
            mGMR.RunRunset();

            //Assert
            foreach (GingerRunner GR in mGMR.Runners)
            {
                foreach (BusinessFlow bf in GR.BusinessFlows)
                {
                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
                }
            }
        }

        [TestMethod]
        public void RunGingersInParallel()
        {
            //Act
            AddGinger(Agent.eDriverType.SeleniumChrome);
            mGMR.RunRunset();
            
            //Assert
            foreach (GingerRunner GR in mGMR.Runners)
            {
                foreach (BusinessFlow bf in GR.BusinessFlows)
                {
                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
                }
            }
        }

        [TestMethod]
        public void DuplicateGingersInParallelAndRun()
        {
            //Act
            AddGinger(Agent.eDriverType.SeleniumChrome);

            //Assert
            foreach (GingerRunner GR in mGMR.Runners)
            {
                foreach (BusinessFlow bf in GR.BusinessFlows)
                {
                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
                }
            }
        }

        [TestMethod]
        public void RunGingersSequentially()
        {
            //Act
            AddGinger(Agent.eDriverType.SeleniumChrome);
            mGMR.RunSetConfig.RunModeParallel = false;
            mGMR.RunRunset();

            //Assert
            foreach (GingerRunner GR in mGMR.Runners)
            {
                foreach (BusinessFlow bf in GR.BusinessFlows)
                {
                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
                }
            }
        }

        private void AddBusinessFlow(GingerRunner GR)
        {
            BusinessFlow mBF;
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "RUNSET TEST";
            mBF.Active = true;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.Platforms = new ObservableList<Platform>();
           // mBF.Platforms.Add(p);

            Activity a1 = new Activity();
            a1.Active = true;
            a1.TargetApplication = "WebApp";
            
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            a1.Acts.Add(act1);

            ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act2);

            ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act3);

            GR.BusinessFlows.Add(mBF);
        }



    }
}
