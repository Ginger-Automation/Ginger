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
            List<ApplicationPOMModel> currentPOM = null;          
            RecordingManager mngr = new RecordingManager(currentPOM, mBF, Context, mDriver, PlatformInfo);
            if(mngr != null)
            {
                mngr.StartRecording();
                Thread.Sleep(2000);
                mngr.StopRecording();
            }
            TestAction actUI = (TestAction)mBF.Activities[0].Acts[0];
            Assert.IsTrue(actUI.ElementLocateBy == eLocateBy.ByID);
            Assert.IsTrue(actUI.ElementAction == "Click");
            Assert.IsTrue(actUI.ElementType == "Button");
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithPOMTest()
        {
            List<ApplicationPOMModel> lstPOM = new List<ApplicationPOMModel>();
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);
            if (mngr != null)
            {
                mngr.StartRecording();
                Thread.Sleep(3000);
                mngr.StopRecording();
            }
            if (mBF.Activities[0].Acts.Count > 0)
            {
                ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;
                TestAction actUI = (TestAction)mBF.Activities[0].Acts[0];
                Assert.IsTrue(actUI.ElementLocateBy == eLocateBy.POMElement);
                Assert.IsTrue(actUI.ElementAction == "Click");
                Assert.IsTrue(actUI.ElementType == "Button");
                Assert.IsTrue(cPOM.MappedUIElements[0].ElementTypeEnum.ToString() == eElementType.Button.ToString());
                Assert.IsTrue(cPOM.MappedUIElements.Count == mBF.Activities[0].Acts.Count);
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
            List<ApplicationPOMModel> lstPOM = new List<ApplicationPOMModel>();
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, mBF, Context, mDriver, PlatformInfo);
            if (mngr != null)
            {
                mngr.StartRecording();
                Thread.Sleep(3000);
                mngr.StopRecording();
            }
            if (mngr.ListPOMObjectHelper != null && mngr.ListPOMObjectHelper.Count > 0)
            {
                ApplicationPOMModel cPOM = mngr.ListPOMObjectHelper[1].ApplicationPOM;
                Assert.IsTrue(cPOM.PageURL == "www.google.com");
                Assert.IsTrue(mngr.ListPOMObjectHelper.Count == 2);
            }
            else
            {
                Assert.IsTrue(false);
            }
        }
    }
}
