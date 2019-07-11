using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace GingerCoreNETUnitTest.RecordingLibTest
{
    [Ignore]
    [TestClass]
    public class RecordingManagerTest
    {
        IRecord mDriver;
        BusinessFlow mBF;
        Context Context;
        IPlatformInfo PlatformInfo;

        [TestInitialize]
        public void TestInitialize()
        {
            Context = new Context();
            TestPlatform webPlatformInfo = new TestPlatform();
            mDriver = new TestDriver();
            PlatformInfo = new TestPlatform();
            mBF = new BusinessFlow() { Name = "TestRecordingBF", Active = true };
            Activity activity = new Activity();
            mBF.AddActivity(activity);
            Context.BusinessFlow = mBF;
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithoutPOMTest()
        {
            //Arrange
            List<ApplicationPOMModel> currentPOM = null;          
            RecordingManager mngr = new RecordingManager(currentPOM, mBF, Context, mDriver, PlatformInfo);
            if(mngr != null)
            {
                //Act
                mngr.StartRecording();
                Thread.Sleep(2000);
                mngr.StopRecording();
            }

            //Assert
            TestAction actUI = (TestAction)mBF.Activities[0].Acts[0];
            Assert.AreEqual(actUI.ElementLocateBy, eLocateBy.ByID);
            Assert.AreEqual(actUI.ElementAction, "Click");
            Assert.AreEqual(actUI.ElementType, "Button");
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithPOMTest()
        {
            //Arrange
            List<ApplicationPOMModel> lstPOM = new List<ApplicationPOMModel>();
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);
            if (mngr != null)
            {
                //Act
                mngr.StartRecording();
                Thread.Sleep(3000);
                mngr.StopRecording();
            }

            //Assert
            if (mBF.Activities[0].Acts.Count > 0)
            {
                ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;
                TestAction actUI = (TestAction)mBF.Activities[0].Acts[0];
                Assert.AreEqual(actUI.ElementLocateBy, eLocateBy.POMElement);
                Assert.AreEqual(actUI.ElementAction, "Click");
                Assert.AreEqual(actUI.ElementType, "Button");
                Assert.AreEqual(cPOM.MappedUIElements[0].ElementTypeEnum.ToString(), eElementType.Button.ToString());
                Assert.AreEqual(cPOM.MappedUIElements.Count, mBF.Activities[0].Acts.Count);
            }
            else
            {
                Assert.IsTrue(false);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithMultiplePageHandledTest()
        {
            //Arrange
            List<ApplicationPOMModel> lstPOM = new List<ApplicationPOMModel>();
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);
            if (mngr != null)
            {
                //Act
                mngr.StartRecording();
                Thread.Sleep(3000);
                mngr.StopRecording();
            }

            //Assert
            if (mngr.ListPOMObjectHelper != null && mngr.ListPOMObjectHelper.Count > 0)
            {
                ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;
                Assert.AreEqual(cPOM.PageURL, "www.google.com");
                Assert.AreEqual(mngr.ListPOMObjectHelper.Count, 2);
            }
            else
            {
                Assert.IsTrue(false);
            }
        }
    }
}
