#region License
/*
Copyright © 2014-2024 European Support Limited

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

using GingerCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace GingerCoreCommonTest.DevelopmentTimeTest
{
    [TestClass]
    public class DevelopmentTimeForBFTest
    {
        [TestMethod]
        public void CheckStopWatchTimerStartBF()
        {
            BusinessFlow businessFlow = new BusinessFlow("BF1");

            TimeSpan stopwatch = TimeSpan.Zero;

            Thread.Sleep(1000);
            //Checking that Time has not started until any change
            Assert.AreEqual(stopwatch, businessFlow.DevelopmentTime);


            businessFlow.StartDirtyTracking();
            businessFlow.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            //Checking if after dirty status changed to modified, tracking of time started
            Assert.AreNotEqual(stopwatch, businessFlow.DevelopmentTime);


        }

        [TestMethod]
        public void CheckStopWatchTimerStopBF()
        {
            BusinessFlow businessFlow = new BusinessFlow("BF2");
            businessFlow.StartDirtyTracking();
            businessFlow.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            TimeSpan timeNoted = businessFlow.DevelopmentTime;
            businessFlow.StopTimer();

            Thread.Sleep(2300);
            Assert.AreEqual(timeNoted, businessFlow.DevelopmentTime);
        }

        [TestMethod]
        public void CheckStopWatchTimerStartActivity()
        {
            GingerCore.Activity act = new GingerCore.Activity();

            TimeSpan stopwatch = TimeSpan.Zero;

            Assert.AreEqual(stopwatch, act.DevelopmentTime);



            act.StartDirtyTracking();
            act.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);

            Assert.AreNotEqual(stopwatch, act.DevelopmentTime);
        }

        [TestMethod]
        public void CheckStopWatchTimerStopActivity()
        {
            GingerCore.Activity act = new GingerCore.Activity();
            act.StartDirtyTracking();
            act.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

            Thread.Sleep(1000);
            TimeSpan timeNoted = act.DevelopmentTime;
            act.StopTimer();

            Thread.Sleep(2300);
            Assert.AreEqual(timeNoted, act.DevelopmentTime);
        }
    }
}
