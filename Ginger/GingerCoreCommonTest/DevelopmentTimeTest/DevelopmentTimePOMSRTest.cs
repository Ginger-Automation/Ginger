using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreCommonTest.DevelopmentTimeTest
{
    [TestClass]
    public class DevelopmentTimePOMSRTest
    {


        [TestMethod]
        public void CheckLearnPOMDevTime()
        {
            ApplicationPOMModel model = new ApplicationPOMModel();
            model.Name = "Test";
            model.Description = "Description";

            PomLearnUtils pomLearn = new PomLearnUtils(model);
            TimeSpan prevTime = TimeSpan.Zero;
            pomLearn.StartLearningTime();
            Thread.Sleep(1000);
            pomLearn.StopLearningTime();

            Assert.AreNotEqual(prevTime, model.DevelopmentTime);

        }
    }
}
