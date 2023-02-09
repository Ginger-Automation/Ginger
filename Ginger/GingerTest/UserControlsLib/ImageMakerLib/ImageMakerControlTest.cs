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

using Ginger.GeneralLib;
using GingerTestHelper;
using GingerWPF.GeneralLib;
using GingerWPF.UserControlsLib.ImageMakerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GingerTest
{

    [TestClass]
    public class ImageMakerControlTest
    {
        // static GingerAutomator mGingerWPFAutomator; 
        // Mutex mutex = new Mutex();

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            // mGingerAutomator = GingerAutomator.StartSession();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {            
            
        }


        [TestInitialize]
        public void TestInitialize()
        {
          
        }

        [TestCleanup]
        public void TestCleanUp()
        {
          
        }

        //private static Action EmptyDelegate = delegate () { };
        //[Level3]
        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void ImageMakerControlsVisualTest1_50()
        //{
        //    //// Arrange
        //    //bool IsEquel2 = false;
        //    //Page1 p1 = new Page1();
        //    //p1.ShowIcons(1, 50);
        //    //p1.StopSpinners();

        //    //// Act            
        //    //TestWindow TW = new TestWindow(p1);
        //    //TW.Show();
            
        //    //TW.Activate();            
        //    ////mGingerWPFAutomator.SleepWithDoEvents(200);
            
        //    //IsEquel2 = VisualCompare.IsVisualEquel(p1, "ImageMakerControlsVisualTest1_50");

        //    //TW.Close();

        //    ////Assert
        //    //Assert.IsTrue(IsEquel2);
        //}


        //[Level3]
        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void ImageMakerControlsVisualTest51_100()
        //{
        //    //// Arrange
        //    //bool IsEquel2 = false;            
        //    //Page1 p1 = new Page1();
        //    //p1.ShowIcons(51, 100);
        //    //p1.StopSpinners();

        //    //// Act
        //    //TestWindow TW = new TestWindow(p1);
        //    //TW.Show();
        //    ////mGingerWPFAutomator.SleepWithDoEvents(200);                
        //    //IsEquel2 = VisualCompare.IsVisualEquel(p1, "ImageMakerControlsVisualTest51_100");            
        //    //TW.Close();

        //    ////Assert
        //    //Assert.IsTrue(IsEquel2);
        //}


       

    }
}
