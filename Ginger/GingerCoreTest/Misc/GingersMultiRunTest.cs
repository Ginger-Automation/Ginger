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


//using Amdocs.Ginger.Common;
//using Ginger.Run;
//using GingerCore;
//using GingerCore.Actions;
//using GingerCore.Actions.Common;
//using GingerCore.Platforms;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.Common.UIElement;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

//namespace UnitTests.NonUITests
//{

//    [TestClass]
//    public class GingersMultiRunTest
//    {


//        RunsetExecutor mGMR;

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            mGMR = new RunsetExecutor();
//            mGMR.RunSetConfig = new RunSetConfig();
//            mGMR.RunSetConfig.mRunWithAnalyzer = false;

//            AddGinger(Agent.eDriverType.SeleniumFireFox);
//            AddGinger(Agent.eDriverType.SeleniumFireFox);
//            AddGinger(Agent.eDriverType.SeleniumFireFox);
//            //AddGinger(Agent.eDriverType.SeleniumFireFox);
//            //AddGinger(Agent.eDriverType.SeleniumFireFox);

//            AddGinger(Agent.eDriverType.SeleniumChrome);
//            AddGinger(Agent.eDriverType.SeleniumChrome);
//            AddGinger(Agent.eDriverType.SeleniumChrome);
//            AddGinger(Agent.eDriverType.SeleniumChrome);
//            AddGinger(Agent.eDriverType.SeleniumChrome);
//            AddGinger(Agent.eDriverType.InternalBrowser);
//        }

//        [TestCleanup()]
//        public void TestCleanUp()
//        {
//            foreach (GingerRunner GR in mGMR.Runners)
//            {
//                GR.StopAgents();
//            }
//        }

//        private void AddGinger(Agent.eDriverType DriverType)
//        {
//            GingerRunner mGR2 = new GingerRunner();
//            Platform p22 = new Platform() { PlatformType = ePlatformType.Web };
//            Agent a2 = new Agent();
//            a.Driver = new InternalBrowser(mBF);
//            a2.DriverType = DriverType;
//            p22.Agent = a2;
//            mGR2.SolutionAgents = new ObservableList<IAgent>();
//            mGR2.SolutionAgents.Add(a2);
//            mGR2.Platforms.Add(p22);
//            for (int i = 1; i < 3; i++)
//            {
//                AddBusinessFlow(mGR2);
//            }

//            mGR2.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a2 });
//            mGMR.Runners.Add(mGR2);
//        }


//        [TestMethod]  [Timeout(60000)]
//        public void RunGingersParallel_FFandChrome_X3()
//        {
//            Act
//            mGMR.RunRunset();

//            Assert
//            foreach (GingerRunner GR in mGMR.Runners)
//            {
//                foreach (BusinessFlow bf in GR.BusinessFlows)
//                {
//                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
//                }

//            }
//        }

//        [TestMethod]  [Timeout(60000)]
//        public void SpeedTest_FFx2_Chrome_X1()
//        {
//            Arrange
//            mGMR = new RunsetExecutor();
//            AddGinger(Agent.eDriverType.SeleniumFireFox);
//            AddGinger(Agent.eDriverType.SeleniumFireFox);
//            AddGinger(Agent.eDriverType.SeleniumChrome);

//            for (int i = 0; i < 5; i++)
//            {
//                AddBusinessFlow(mGMR.Runners[0]);
//                AddBusinessFlow(mGMR.Runners[1]);
//                AddBusinessFlow(mGMR.Runners[2]);
//            }


//            Act
//            mGMR.RunRunset();

//            Assert
//            foreach (GingerRunner GR in mGMR.Runners)
//            {
//                foreach (BusinessFlow bf in GR.BusinessFlows)
//                {
//                    Assert.AreEqual(bf.RunStatus, eRunStatus.Passed);
//                }

//            }


//        }



//        private void AddBusinessFlow(GingerRunner GR)
//        {
//            BusinessFlow mBF;
//            mBF = new BusinessFlow();
//            mBF.Activities = new ObservableList<Activity>();
//            mBF.Name = "BF Test Fire Fox";
//            mBF.Active = true;
//            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });
//            Platform p = new Platform();
//            p.PlatformType = ePlatformType.Web;
//            mBF.Platforms = new ObservableList<Platform>();
//            mBF.Platforms.Add(p);
//            Activity a1 = new Activity();
//            a1.Active = true;
//            mBF.Activities.Add(a1);

//            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "http://:8099/", Active = true };
//            a1.Acts.Add(act1);

//            ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
//            a1.Acts.Add(act2);

//            ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
//            a1.Acts.Add(act3);

//            ActSubmit act4 = new ActSubmit() { LocateBy = eLocateBy.ByValue, LocateValue = "Log in", Active = true };
//            a1.Acts.Add(act4);

//            ActLink act5 = new ActLink() { LocateBy = eLocateBy.ByLinkText, Wait = 1, LocateValue = "Manage Customer", LinkAction = ActLink.eLinkAction.Click, Active = true };
//            a1.Acts.Add(act5);

//            GR.BusinessFlows.Add(mBF);
//        }



//    }
//}
