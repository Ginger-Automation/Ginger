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



using GingerTest.WizardLib;
using GingerTestHelper;
using GingerWPF.WizardLib;
using GingerWPFUnitTest.POMs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace GingerTest
{
    [TestClass]
    public class WizardTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }


        static GingerAutomator mGingerAutomator;
        Mutex mutex = new Mutex();

        [ClassInitialize]
        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);

            mGingerAutomator = GingerAutomator.StartSession();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();

            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
            mutex.WaitOne();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mutex.ReleaseMutex();
            mTestHelper.TestCleanup();
        }


        WizardPOM ShowMyWizard(string folder, double width = 0)
        {
            Task.Factory.StartNew(() =>
            {
                mGingerAutomator.MainWindowPOM.Execute(() =>
                {
                    MyWizard wiz = new MyWizard(folder);
                    if (width == 0)
                    {
                        WizardWindow.ShowWizard(wiz);
                    }
                    else
                    {
                        WizardWindow.ShowWizard(wiz, width);
                    }
                });
            });

            WizardPOM wizardPOM = WizardPOM.CurrentWizard;
            return wizardPOM;
        }

        [Level3]
        [TestMethod]
        [Timeout(60000)]
        public void VerifyButtonsOnStartThenCancel()
        {
            //Arrange            
            string folder = TestResources.GetTestTempFolder("MyWizardItemsFolder1");

            //Act
            WizardPOM mWizard = ShowMyWizard(folder);
            bool nextButtonEnabled = mWizard.NextButton.IsEnabled;
            bool prevButtonEnabled = mWizard.PrevButton.IsEnabled;
            bool finishButtonEnabled = mWizard.FinishButton.IsEnabled;
            mWizard.CancelButton.Click();
            bool WizardOpen = WizardPOM.IsWizardOpen;
            mGingerAutomator.MainWindowPOM.TakeScreenShot(mTestHelper.GetTempFileName("Wizard Screen Shot.png"));
            //Artifacts


            //Assert
            Assert.IsTrue(nextButtonEnabled, "Next button is enabled");
            Assert.IsFalse(prevButtonEnabled, "Prev button is disabled");
            Assert.IsTrue(finishButtonEnabled, "Finish button is enabled");
            Assert.IsFalse(WizardOpen, "Wizard was closed");
        }

        [Level3]
        [TestMethod]
        [Timeout(60000)]
        public void CreateMyWizardItem()
        {
            //Arrange            
            string folder = TestResources.GetTestTempFolder("MyWizardItemsFolder2");

            //Act
            WizardPOM mWizard = ShowMyWizard(folder);
            mWizard.NextButton.Click();
            mWizard.CurrentPage["Name AID"].SetText("My Wizard Item 1");
            mWizard.NextButton.Click();
            mWizard.FinishButton.Click();

            bool WizardOpen = WizardPOM.IsWizardOpen;

            //Assert

            Assert.IsFalse(WizardOpen, "Wizard was closed");
        }

        [Level3]
        [Ignore] //TODO FIXME
        [TestMethod]
        [Timeout(60000)]
        public void WizardWithWindowWidth()
        {
            //Arrange            

            double width = 1200;
            string folder = TestResources.GetTestTempFolder("MyWizardItemsFolder3");

            //Act
            WizardPOM mWizard = ShowMyWizard(folder, width);
            double w = mWizard.WindowPOM.Width;
            mWizard.CancelButton.Click();

            //Assert
            Assert.AreEqual(width, w, "Wizard width");
        }


        //[TestMethod]  [Timeout(60000)]
        //public void WizardIntro()
        //{
        //    //Arrange                        
        //    string folder = TestResources.getGingerUnitTesterTempFolder("MyWizardItemsFolder4");

        //    //Act
        //    WizardPOM mWizard = ShowMyWizard(folder);
        //    string title = mWizard.CurrentPage["Title AID"].Text;            
        //    mWizard.CancelButton.Click();

        //    //Assert
        //    Assert.AreEqual("MyWizardItem Intro", title, "Wizard Title");
        //}

    }
}
