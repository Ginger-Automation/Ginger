#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [Level3]
    [TestClass]
    public class VisualCompareTest
    {

        BusinessFlow mBF;
        GingerRunner mGR;


        [TestInitialize]
        public void TestInitialize()
        {
            mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF Visual Testing",
                Active = true
            };
            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "VM" });

            mGR = new GingerRunner();
            mGR.Executor.SolutionFolder = TestResources.GetTestTempFolder("");

            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            DriverConfigParam browserTypeParam = a.GetOrCreateParam(parameter: nameof(GingerWebDriver.BrowserType), defaultValue: nameof(WebBrowserType.Chrome));
            browserTypeParam.Value = nameof(WebBrowserType.Chrome);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [a];

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "VM", Agent = a });
            mGR.Executor.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "VM", Platform = ePlatformType.Web, Description = "VM application" },
            ];
            mGR.Executor.BusinessFlows.Add(mBF);
        }

        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }
        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void CreateBaselineAndCompare()
        //{

        //    //Arrange
        //    ResetBusinessFlow();

        //    Activity a1 = new Activity();
        //    a1.Active = true;
        //    mBF.Activities.Add(a1);

        //    ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = Common.getGingerUnitTesterDocumentsFolder() + @"HTML\VisualComapre1.html", Active = true };
        //    a1.Acts.Add(act1);

        //    //Create baseline 
        //    ActVisualTesting act2 = new ActVisualTesting();
        //    act2.Description = "Visual Testing - Test1";
        //    act2.VisualTestingAnalyzer = ActVisualTesting.eVisualTestingAnalyzer.BitmapPixelsComparison;
        //    act2.Active = true;
        //    act2.AddNewReturnParams = true;
        //    act2.ChangeAppWindowSize = ActVisualTesting.eChangeAppWindowSize.Resolution800x600;
        //    act2.CreateBaselineAction = true;

        //    a1.Acts.Add(act2);

        //    // Compare
        //    ActVisualTesting act3 = new ActVisualTesting();
        //    act3.Description = "Visual Testing - Test1";
        //    act3.VisualTestingAnalyzer = ActVisualTesting.eVisualTestingAnalyzer.BitmapPixelsComparison;
        //    act3.ChangeAppWindowSize = ActVisualTesting.eChangeAppWindowSize.Resolution800x600;
        //    act3.Active = true;
        //    act3.AddNewReturnParams = true;
        //    act3.BaseLineFileName = @"~\Documents\ScreenShots\Visual Testing - Test1 - Baseline.png";   // File is created in action 2 with default filename            
        //    a1.Acts.Add(act3);

        //    //Act            
        //    mGR.RunRunner();

        //    //Assert
        //    string percentdiff = act3.GetReturnParam("Percentage Difference");

        //   Assert.AreEqual(percentdiff, "0", "percentdiff=0");
        //   Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
        //   Assert.AreEqual(a1.Status, eRunStatus.Passed);

        //}

        //TODO:
        // Verify 3 screen shots exist

        // TODO: test UIElemtsAnalyzer

    }
}
