using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreCommonTest.DevelopmentTimeTest
{
    [TestClass]
    public class DevelopmentTimeForBFTest
    {
        [TestMethod]
        public void CheckStopWatchTimerStartBF()
        {
            BusinessFlow businessFlow = new BusinessFlow("BF1");

            TimeSpan stopwatch = TimeSpan.Zero ;
            
            Thread.Sleep(1000);
            //Checking that Time has not started until any change
            Assert.AreEqual(stopwatch, businessFlow.DevelopmentTimeCalc);


            businessFlow.StartDirtyTracking();
            businessFlow.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            //Checking if after dirty status changed to modified, tracking of time started
            Assert.AreNotEqual(stopwatch, businessFlow.DevelopmentTimeCalc);


        }

        [TestMethod]
        public void CheckStopWatchTimerStopBF()
        {
            BusinessFlow businessFlow = new BusinessFlow("BF2");
            businessFlow.StartDirtyTracking();
            businessFlow.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            TimeSpan timeNoted = businessFlow.DevelopmentTimeCalc;
            businessFlow.StopTimer();

            Thread.Sleep(2300);
            Assert.AreEqual(timeNoted, businessFlow.DevelopmentTimeCalc);
        }

        [TestMethod]
        public void CheckStopWatchTimerStartActivity()
        {
            GingerCore.Activity act = new GingerCore.Activity();

            TimeSpan stopwatch = TimeSpan.Zero;

            Assert.AreEqual(stopwatch, act.DevelopmentTimeCalc);



            act.StartDirtyTracking();
            act.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);

            Assert.AreNotEqual(stopwatch, act.DevelopmentTimeCalc);
        }

        [TestMethod]
        public void CheckStopWatchTimerStopActivity()
        {
            GingerCore.Activity act = new GingerCore.Activity();
            act.StartDirtyTracking();
            act.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            TimeSpan timeNoted = act.DevelopmentTimeCalc;
            act.StopTimer();

            Thread.Sleep(2300);
            Assert.AreEqual(timeNoted, act.DevelopmentTimeCalc);
        }
    }
}
