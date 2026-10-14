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

extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.PBDriver;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCoreTest.Misc
{
    /// <summary>
    /// What the Spy reports for an element has to find that same element again.
    /// </summary>
    [TestClass]
    public class PbSpyLocatorTest
    {
        private const string RepeatedName = "Same";
        private const string BracketName = "Amount [USD]";
        private const string MnemonicCaption = "&Save Customer Record And Continue To The Next Screen Now";
        private const string MnemonicAccessibleName = "Save Customer Record And Continue To The Next Screen Now";

        private Form mForm;
        private Button[] mRepeatedButtons;
        private Button mBracketButton;
        private Button mMnemonicButton;
        private CheckBox mCheckedBox;
        [TestInitialize]
        public void CreateWindow()
        {
            mForm = new Form
            {
                Text = "PB spy host",
                Width = 600,
                Height = 260,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000)
            };

            mRepeatedButtons = new Button[3];
            for (int i = 0; i < mRepeatedButtons.Length; i++)
            {
                mRepeatedButtons[i] = new Button { Text = RepeatedName, Location = new Point(12 + (i * 90), 12), Width = 80 };
                mForm.Controls.Add(mRepeatedButtons[i]);
            }

            mBracketButton = new Button { Text = BracketName, Location = new Point(12, 52), Width = 120 };
            mForm.Controls.Add(mBracketButton);

            mMnemonicButton = new Button { Text = MnemonicCaption, Location = new Point(12, 92), Width = 560 };
            mForm.Controls.Add(mMnemonicButton);

            mCheckedBox = new CheckBox { Text = "Active", Checked = true, Location = new Point(12, 132) };
            mForm.Controls.Add(mCheckedBox);
            mForm.Show();
            Application.DoEvents();
        }

        [TestCleanup]
        public void CloseWindow()
        {
            mForm?.Close();
            mForm?.Dispose();
        }

        [TestMethod]
        public void PowerBuilderXPath_FindsEachOfTheRepeatedlyNamedSiblings()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);

            foreach (Button button in mRepeatedButtons)
            {
                UIAuto.AutomationElement expected = ElementOf(button);
                string xpath = helper.GetElementAbsoluteXPath(expected);

                object found = helper.FindElementByLocator(eLocateBy.ByXPath, xpath);

                Assert.IsNotNull(found, "XPath '" + xpath + "' found nothing");
                Assert.AreEqual(expected, found, "XPath '" + xpath + "' landed on a different button than the one it was created for");
            }
        }

        [TestMethod]
        public void PowerBuilderXPath_FindsAnElementWhoseNameHasBrackets()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement expected = ElementOf(mBracketButton);
            string xpath = helper.GetElementAbsoluteXPath(expected);

            object found = helper.FindElementByLocator(eLocateBy.ByXPath, xpath);

            Assert.AreEqual(expected, found, "XPath '" + xpath + "' did not find the button it was created for");
        }

        [TestMethod]
        public void WindowsXPath_KeepsTheXPathItProducedBefore()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.Windows);

            string secondButtonXPath = helper.GetElementAbsoluteXPath(ElementOf(mRepeatedButtons[1]));

            // Windows POM delta matches elements by XPath string, so its output must not move.
            Assert.AreEqual("/" + RepeatedName, secondButtonXPath);
        }

        [TestMethod]
        public void PowerBuilderNameLocator_IsTheAccessibleNameAndFindsTheElement()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement expected = ElementOf(mMnemonicButton);

            ElementLocator byName = helper.GetElementLocators(helper.GetElementInfoFor(expected)).Single(l => l.LocateBy == eLocateBy.ByName);

            Assert.AreEqual(MnemonicAccessibleName, byName.LocateValue);
            Assert.AreEqual(expected, helper.FindElementByLocator(eLocateBy.ByName, byName.LocateValue));
        }

        [TestMethod]
        public void PowerBuilderLocators_IncludeXYAtTheElementCentre()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement element = ElementOf(mBracketButton);

            ElementLocator byXY = helper.GetElementLocators(helper.GetElementInfoFor(element)).Single(l => l.LocateBy == eLocateBy.ByXY);

            Rectangle window = helper.CurrentWindow.Current.BoundingRectangle;
            Rectangle target = element.Current.BoundingRectangle;
            string expected = (target.X + (target.Width / 2) - window.X) + "," + (target.Y + (target.Height / 2) - window.Y);
            Assert.AreEqual(expected, byXY.LocateValue);
        }

        [TestMethod]
        public void WindowsNameLocator_KeepsTheWindowText()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.Windows);
            UIAuto.AutomationElement element = ElementOf(mMnemonicButton);

            ElementLocator byName = helper.GetElementLocators(helper.GetElementInfoFor(element)).Single(l => l.LocateBy == eLocateBy.ByName);

            Assert.AreEqual(helper.GetElementTitle(element), byName.LocateValue);
        }

        [TestMethod]
        public void PowerBuilderElementInfo_ReadsToggleStateAndOffscreen()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);

            UIAElementInfo info = (UIAElementInfo)helper.GetElementInfoFor(ElementOf(mCheckedBox));

            Assert.AreEqual("ToggleState_On", info.ToggleState);
            Assert.AreEqual(ElementOf(mCheckedBox).Current.IsOffscreen, info.IsOffscreen);
            Assert.IsTrue(bool.TryParse(helper.GetControlPropertyValue(ElementOf(mCheckedBox), "IsOffScreen"), out _),
                "The PowerBuilder element info reads IsOffscreen through this key, so it has to yield a value");
        }

        [TestMethod]
        public void PowerBuilderTestElementLocators_ReportsEachLocatorWithoutWaiting()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            PBDriver driver = (PBDriver)helper.WindowExplorer;
            helper.ImplicitWait = 30;

            ElementLocator existing = new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = BracketName, Active = true };
            ElementLocator missing = new ElementLocator { LocateBy = eLocateBy.ByName, LocateValue = "No such control", Active = true };
            ElementInfo element = new ElementInfo { Locators = [existing, missing] };

            Stopwatch clock = Stopwatch.StartNew();
            bool anyPassed = driver.TestElementLocators(element);
            clock.Stop();

            Assert.IsTrue(anyPassed);
            Assert.AreEqual(ElementLocator.eLocateStatus.Passed, existing.LocateStatus);
            Assert.AreEqual(ElementLocator.eLocateStatus.Failed, missing.LocateStatus);
            Assert.IsTrue(clock.Elapsed < TimeSpan.FromSeconds(10), "The missing locator waited out the implicit wait: " + clock.Elapsed);
            Assert.AreEqual(30, helper.ImplicitWait, "The agent's implicit wait has to be put back after the test");
        }

        private UIAComWrapperHelper CreateHelper(ePlatformType platform)
        {
            UIAComWrapperHelper helper = new UIAComWrapperHelper
            {
                mPlatform = platform,
                CurrentWindow = UIAuto.AutomationElement.FromHandle(mForm.Handle),
                CurrentWindowName = mForm.Text,
                ImplicitWait = 0
            };
            helper.UpdateRootElement();

            PBDriver driver = new PBDriver { mUIAutomationHelper = helper };
            helper.WindowExplorer = driver;
            return helper;
        }

        private static UIAuto.AutomationElement ElementOf(Control control)
        {
            return UIAuto.AutomationElement.FromHandle(control.Handle);
        }
    }
}
