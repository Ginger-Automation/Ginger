#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    [TestClass]
    public class RecordingManagerTest
    {
        IRecord mDriver;
        BusinessFlow mBF;
        Context Context;
        IPlatformInfo PlatformInfo;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {
            Context = new Context();
            mDriver = new TestDriver();
            PlatformInfo = new TestPlatform();
            mBF = new BusinessFlow() { Name = "TestRecordingBF", Active = true };
            Activity activity = new Activity();
            mBF.AddActivity(activity);
            Context.BusinessFlow = mBF;

            Context.Target = new Amdocs.Ginger.Common.Repository.TargetBase();
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        [Ignore]   // fail on Azure
        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithoutPOMTest()
        {
            //Arrange
            ObservableList<ApplicationPOMModel> currentPOM = null;
            RecordingManager mngr = new RecordingManager(currentPOM, mBF, Context, mDriver, PlatformInfo);

            //Act
            mngr.StartRecording();
            mngr.StopRecording();
            TestAction actUI = (TestAction)mBF.Activities[0].Acts[1];

            //Assert
            Assert.AreEqual(actUI.ElementLocateBy, eLocateBy.ByID);
            Assert.AreEqual(actUI.ElementAction, "Click");
            Assert.AreEqual(actUI.ElementType, "Button");
        }


        [Ignore]   // fail on Azure
        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithPOMTest()
        {
            //Arrange
            ObservableList<ApplicationPOMModel> lstPOM = [];
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);

            //Act
            mngr.StartRecording();
            mngr.StopRecording();
            ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;
            TestAction actUI = (TestAction)mBF.Activities[0].Acts[1];

            //Assert
            Assert.AreEqual(actUI.ElementLocateBy, eLocateBy.POMElement);
            Assert.AreEqual(actUI.ElementAction, "Click");
            Assert.AreEqual(actUI.ElementType, "Button");
            Assert.AreEqual(cPOM.MappedUIElements[0].ElementTypeEnum.ToString(), eElementType.Button.ToString());
            Assert.AreEqual(cPOM.MappedUIElements.Count, 2);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithMultiplePageHandledTest()
        {
            //Arrange
            ObservableList<ApplicationPOMModel> lstPOM = [];
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);

            //Act
            mngr.StartRecording();
            mngr.StopRecording();
            ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;

            //Assert
            Assert.AreEqual(cPOM.PageURL, "www.google.com");
            Assert.AreEqual(mngr.ListPOMObjectHelper.Count, 2);
        }
    }
}
