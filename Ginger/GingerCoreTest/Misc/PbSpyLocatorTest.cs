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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.PBDriver;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        private const string EditBoxValue = "www.google.com";

        private Form mForm;
        private Button[] mRepeatedButtons;
        private Button mBracketButton;
        private Button mMnemonicButton;
        private CheckBox mCheckedBox;
        private TextBox mValueBox;
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

            mValueBox = new TextBox { Name = "sle_url", Text = EditBoxValue, Location = new Point(12, 172), Width = 200 };
            mForm.Controls.Add(mValueBox);
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
        public void PowerBuilderNameLocator_OfAnUnnamedEditBox_IsNotItsValue()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement element = ElementOf(mValueBox);
            Assert.AreEqual(string.Empty, element.Current.Name, "The edit box needs to have no accessible name");
            Assert.AreEqual("sle_url", element.Current.AutomationId);

            ElementLocator byName = helper.GetElementLocators(helper.GetElementInfoFor(element)).Single(l => l.LocateBy == eLocateBy.ByName);

            Assert.AreEqual(element.Current.LocalizedControlType + " (" + element.Current.AutomationId + ")", byName.LocateValue);
            Assert.AreEqual(element, helper.FindElementByLocator(eLocateBy.ByName, byName.LocateValue));
        }

        [TestMethod]
        public void PowerBuilderLocators_ThatFindTheElement_AreActiveAndListedFirst()
        {
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement element = ElementOf(mRepeatedButtons[1]);

            List<ElementLocator> locators = helper.GetElementLocators(helper.GetElementInfoFor(element)).ToList();

            ElementLocator byName = locators.Single(l => l.LocateBy == eLocateBy.ByName);
            Assert.IsFalse(byName.Active, "ByName finds the first of the buttons sharing the name");
            Assert.AreEqual(ElementLocator.eLocateStatus.Failed, byName.LocateStatus);
            Assert.IsTrue(locators[0].Active, "A new action takes the first locator");
            Assert.AreEqual(ElementLocator.eLocateStatus.Passed, locators[0].LocateStatus);
            Assert.AreEqual(element, helper.FindElementByLocator(locators[0].LocateBy, locators[0].LocateValue));
            Assert.IsTrue(locators.IndexOf(byName) > locators.FindLastIndex(l => l.Active), "ByName is listed above a locator that finds the element");
            Assert.AreEqual(eLocateBy.ByXY, locators[^1].LocateBy);
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

        [TestMethod]
        public void TableAction_OnARowThatCannotBeFound_SaysWhyRatherThanFailingOnAnIndex()
        {
            Panel grid = new Panel { AccessibleName = "dw_stand_in", Location = new Point(250, 120), Size = new Size(320, 90) };
            grid.Controls.Add(new Label { AccessibleName = "name_t", Text = "Name", Location = new Point(0, 0), Width = 100 });
            grid.Controls.Add(new Label { AccessibleName = "address_t", Text = "Address", Location = new Point(110, 0), Width = 200 });
            for (int row = 0; row < 3; row++)
            {
                grid.Controls.Add(new TextBox { AccessibleName = "name", Text = "Name " + row, Location = new Point(0, 20 + (row * 22)), Width = 100 });
                grid.Controls.Add(new TextBox { AccessibleName = "address", Text = "Address " + row, Location = new Point(110, 20 + (row * 22)), Width = 200 });
            }
            mForm.Controls.Add(grid);
            Application.DoEvents();
            UIAComWrapperHelper helper = CreateHelper(ePlatformType.PowerBuilder);
            PBDriver driver = (PBDriver)helper.WindowExplorer;
            TableElementInfo table = helper.GetTableElementInfoForGrid(ElementOf(grid));
            CollectionAssert.AreEqual(new[] { "name", "address" }, table.ColumnNames, "The stand-in has to read as a grid of two columns");
            Assert.AreEqual(3, table.RowCount);

            ActTableElement negativeRow = GridGetValue("Row Number", "-1");
            driver.RunAction(negativeRow);
            ActTableElement selectedRow = GridGetValue("By Selected Row", null);
            driver.RunAction(selectedRow);

            Assert.AreEqual("Given Row Number -1 is not present in Column", negativeRow.Error);
            StringAssert.Contains(selectedRow.Error, "'By Selected Row' is not supported");
        }

        private static ActTableElement GridGetValue(string rowType, string rowValue)
        {
            ActTableElement action = new ActTableElement
            {
                LocateBy = eLocateBy.ByName,
                LocateValueCalculated = "dw_stand_in",
                ControlAction = ActTableElement.eTableAction.GetValue,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                LocateColTitle = "address",
                LocateRowType = rowType,
                LocateRowValue = rowValue
            };
            // What the runner does before handing the action to the driver.
            foreach (ActInputValue input in action.InputValues)
            {
                input.ValueForDriver = input.Value;
            }
            return action;
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
