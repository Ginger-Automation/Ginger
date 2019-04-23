using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

using GingerCore.Platforms.PlatformsInfo;
using System.Threading;
using GingerCore.Actions.Common;

namespace UnitTests.RecordingLibTest
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
            WebPlatform webPlatformInfo = new WebPlatform();
            mDriver = new TestDriver();
            PlatformInfo = new WebPlatform();
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
            RecordingManager mngr = new RecordingManager(currentPOM, Context, mDriver, PlatformInfo);
            if(mngr != null)
            {
                mngr.StartRecording();
                Thread.Sleep(2000);
                mngr.StopRecording();
            }
            ActUIElement actUI = (ActUIElement)Context.BusinessFlow.Activities[0].Acts[0];
            Assert.IsTrue(actUI.ElementLocateBy == Amdocs.Ginger.Common.UIElement.eLocateBy.ByID);
            Assert.IsTrue(actUI.ElementAction == ActUIElement.eElementAction.Click);
            Assert.IsTrue(actUI.ElementType == Amdocs.Ginger.Common.UIElement.eElementType.Button);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoRecordingWithPOMTest()
        {
            List<ApplicationPOMModel> lstPOM = new List<ApplicationPOMModel>();
            ApplicationPOMModel currentPOM = new ApplicationPOMModel();
            lstPOM.Add(currentPOM);
            RecordingManager mngr = new RecordingManager(lstPOM, Context, mDriver, PlatformInfo);
            if (mngr != null)
            {
                mngr.StartRecording();
                Thread.Sleep(2000);
                mngr.StopRecording();
            }
            if (Context.BusinessFlow.Activities[0].Acts.Count > 0)
            {
                ActUIElement actUI = (ActUIElement)Context.BusinessFlow.Activities[0].Acts[0];
                Assert.IsTrue(actUI.ElementLocateBy == Amdocs.Ginger.Common.UIElement.eLocateBy.POMElement);
                Assert.IsTrue(actUI.ElementAction == ActUIElement.eElementAction.Click);
                Assert.IsTrue(actUI.ElementType == Amdocs.Ginger.Common.UIElement.eElementType.Button);
            }
            else
            {
                Assert.IsTrue(false);
            }
        }
    }
}
