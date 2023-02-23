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


using Ginger;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerTest.POMs;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GingerTest.BusinessFlowLib
{
    [Ignore]
    [TestClass]
    [Level3]
    public class BusinessFlowPOMTest
    {
       static GingerAutomator mGingerAutomator;
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions"+ Path.DirectorySeparatorChar +"TestUndoBusinessFlow");
            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(sampleSolutionFolder);

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestMethod]
        [Timeout(60000)]
        public void UndoForSwitchBFTest()
        {
           //Arrange
            BusinessFlowPOM businessflow = mGingerAutomator.MainWindowPOM.SelectBusinessFlow();
            BusinessFlow selectedBusinessFlow = businessflow.selectBusinessFlow("Flow 1");

            businessflow.AutomatePage("Flow 1");
            mGingerAutomator.MainWindowPOM.AddActivityToLIstView();

            BusinessFlow selectedBusinessFlow1 = businessflow.selectBusinessFlow("Flow 2");
            businessflow.AutomatePage("Flow 2");
            mGingerAutomator.MainWindowPOM.ClickOnBackToBFTreeBtn();

            BusinessFlow selectedBusinessFlow2 = businessflow.selectBusinessFlow("Flow 1");
            businessflow.AutomatePage("Flow 1");

            //Act
            int activityCount = mGingerAutomator.MainWindowPOM.ClickOnUndoBtn();

            //Assert
            Assert.AreEqual(1, activityCount);
        }



    }
}




