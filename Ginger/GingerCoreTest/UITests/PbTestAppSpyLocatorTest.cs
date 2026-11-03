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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers;
using GingerCore.Drivers.Common.LegacyAutomation;
using GingerCore.Drivers.PBDriver;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace UnitTest
{
    /// <summary>
    /// The Spy's locators, checked against the PowerBuilder test application rather than a stand-in.
    /// </summary>
    [Level3]
    [TestClass]
    public class PbTestAppSpyLocatorTest
    {
        private const string WindowTitle = "Simple Page";
        private const string DataWindowName = "dw_acc_grd";

        private static Process mApp;
        private static UIAuto.AutomationElement mWindow;

        public TestContext TestContext { get; set; }

        /// <summary>
        /// One instance per test: class cleanup runs only when the whole assembly is done, and
        /// a second "Simple Page" left open meanwhile is found by the other PowerBuilder tests.
        /// </summary>
        [TestInitialize]
        public void LaunchApp()
        {
            mApp = Process.Start(new ProcessStartInfo
            {
                FileName = "pb_test_app.exe",
                WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp"),
                UseShellExecute = true
            });

            Stopwatch clock = Stopwatch.StartNew();
            while (mWindow == null && clock.Elapsed < TimeSpan.FromSeconds(60))
            {
                Thread.Sleep(500);
                mApp.Refresh();
                if (mApp.MainWindowHandle != IntPtr.Zero && mApp.MainWindowTitle == WindowTitle)
                {
                    mWindow = UIAuto.AutomationElement.FromHandle(mApp.MainWindowHandle);
                }
            }
            Assert.IsNotNull(mWindow, "The PowerBuilder test application did not show '" + WindowTitle + "'");
        }

        [TestCleanup]
        public void CloseApp()
        {
            try
            {
                mApp?.Kill();
                mApp?.WaitForExit(10000);
            }
            catch (InvalidOperationException)
            {
                // already gone
            }
            mApp?.Dispose();
            mWindow = null;
        }

        [TestMethod]
        [Timeout(600000)]
        public void SpyLocators_FindEveryControlOfTheWindowAgain()
        {
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);
            UIAComWrapperHelper windowsHelper = CreateHelper(ePlatformType.Windows);

            List<UIAuto.AutomationElement> elements = [];
            CollectDescendants(mWindow, 0, elements);
            Assert.IsTrue(elements.Count > 20, "Expected the test window's controls, got " + elements.Count);

            List<SpiedElement> spied = elements.Select(e => Spy(pbHelper, windowsHelper, e)).ToList();

            foreach (SpiedElement s in spied)
            {
                TestContext.WriteLine(s.ToString());
            }

            List<string> xpathFailures = spied.Where(s => !s.PbXPathFound).Select(s => s.Describe("XPath " + s.PbXPath)).ToList();
            List<string> nameFailures = spied.Where(s => s.PbName != null && !s.PbNameFound && spied.Count(o => o.PbName == s.PbName) == 1)
                .Select(s => s.Describe("Name " + s.PbName)).ToList();
            List<string> idFailures = spied.Where(s => s.AutomationId != null && !s.AutomationIdFound && spied.Count(o => o.AutomationId == s.AutomationId) == 1)
                .Select(s => s.Describe("AutomationId " + s.AutomationId)).ToList();
            List<SpiedElement> notShown = spied.Where(s => s.XY != null && !IsShownInWindow(s.Element)).ToList();
            List<string> xyFailures = spied.Where(s => s.XY != null && !s.XYFound && !notShown.Contains(s)).Select(s => s.Describe("XY " + s.XY)).ToList();
            List<string> oldXPathFailures = spied.Where(s => !s.WindowsXPathFound).Select(s => s.Describe("old XPath " + s.WindowsXPath)).ToList();

            TestContext.WriteLine("");
            TestContext.WriteLine("Controls spied: " + spied.Count);
            TestContext.WriteLine("XPath, new generation, not found again: " + xpathFailures.Count);
            TestContext.WriteLine("XPath, old generation, not found again: " + oldXPathFailures.Count);
            TestContext.WriteLine("Unique Name not found again: " + nameFailures.Count);
            TestContext.WriteLine("Unique AutomationId not found again: " + idFailures.Count);
            TestContext.WriteLine("XY not landing on the control: " + xyFailures.Count);
            TestContext.WriteLine("XY not checked, the control is not on screen inside the window: " + notShown.Count);
            foreach (SpiedElement s in notShown)
            {
                TestContext.WriteLine("  not shown: " + s.Describe("XY " + s.XY));
            }
            foreach (string line in oldXPathFailures)
            {
                TestContext.WriteLine("  old: " + line);
            }

            Assert.AreEqual(0, xpathFailures.Count, string.Join(Environment.NewLine, xpathFailures));
            Assert.AreEqual(0, nameFailures.Count, string.Join(Environment.NewLine, nameFailures));
            Assert.AreEqual(0, idFailures.Count, string.Join(Environment.NewLine, idFailures));
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("XY was not checked, a point lookup cannot reach the application: " + InteractiveDesktop.DescribeSession());
            }
            Assert.AreEqual(0, xyFailures.Count, string.Join(Environment.NewLine, xyFailures));
        }

        [TestMethod]
        [Timeout(600000)]
        public void SpyInsideTheDataWindow_FindsWhatItPickedAgain()
        {
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("The Spy picks by screen point, which cannot reach the application: " + InteractiveDesktop.DescribeSession());
            }
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);
            UIAComWrapperHelper windowsHelper = CreateHelper(ePlatformType.Windows);

            UIAuto.AutomationElement dataWindow = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, DataWindowName);
            Assert.IsNotNull(dataWindow, DataWindowName + " was not found");
            TestContext.WriteLine("DataWindow: " + Describe(dataWindow));

            List<UIAuto.AutomationElement> children = [];
            CollectDescendants(dataWindow, 0, children);
            TestContext.WriteLine("Raw descendants: " + children.Count);
            foreach (UIAuto.AutomationElement child in children)
            {
                TestContext.WriteLine("  " + Describe(child));
            }

            // a point lookup finds the topmost window under the point
            mWindow.SetFocus();
            Thread.Sleep(1000);

            pbHelper.taskFinished = false;
            Rectangle area = dataWindow.Current.BoundingRectangle;
            List<SpiedElement> picked = [];
            for (int row = 1; row <= 4; row++)
            {
                for (int column = 1; column <= 4; column++)
                {
                    Point point = new Point(area.X + (area.Width * column / 5), area.Y + (area.Height * row / 5));
                    TestContext.WriteLine("Point " + point + ": UI Automation returns " + Describe(UIAuto.AutomationElement.FromPoint(point)));
                    UIAuto.AutomationElement element = pbHelper.GetElementAtPoint(point) as UIAuto.AutomationElement;
                    if (element == null)
                    {
                        TestContext.WriteLine("Point " + point + ": nothing in the window (" + pbHelper.LastElementFromPointMessage + ")");
                        continue;
                    }
                    if (picked.Any(p => p.Element.Equals(element)))
                    {
                        continue;
                    }
                    picked.Add(Spy(pbHelper, windowsHelper, element));
                }
            }

            foreach (SpiedElement s in picked)
            {
                TestContext.WriteLine(s.ToString());
            }

            List<string> xpathFailures = picked.Where(s => !s.PbXPathFound).Select(s => s.Describe("XPath " + s.PbXPath)).ToList();
            Assert.IsTrue(picked.Count > 0, "The Spy picked nothing inside " + DataWindowName);
            Assert.AreEqual(0, xpathFailures.Count, string.Join(Environment.NewLine, xpathFailures));
        }

        [TestMethod]
        [Timeout(600000)]
        public void SpyAtTheCentreOfADataWindowCell_PicksTheCell()
        {
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("The Spy picks by screen point, which cannot reach the application: " + InteractiveDesktop.DescribeSession());
            }
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);

            UIAuto.AutomationElement dataWindow = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, DataWindowName);
            Assert.IsNotNull(dataWindow, DataWindowName + " was not found");
            List<UIAuto.AutomationElement> descendants = [];
            CollectDescendants(dataWindow, 0, descendants);
            List<UIAuto.AutomationElement> cells = descendants.Where(e => e.Current.NativeWindowHandle == 0).ToList();
            Assert.IsTrue(cells.Count > 0, "No cells under " + DataWindowName);

            // a point lookup finds the topmost window under the point
            mWindow.SetFocus();
            Thread.Sleep(1000);

            pbHelper.taskFinished = false;
            List<string> misses = [];
            foreach (UIAuto.AutomationElement cell in cells)
            {
                Rectangle rect = cell.Current.BoundingRectangle;
                Point centre = new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
                UIAuto.AutomationElement picked = pbHelper.GetElementAtPoint(centre) as UIAuto.AutomationElement;
                TestContext.WriteLine(Describe(cell) + " picked " + (picked == null ? "nothing (" + pbHelper.LastElementFromPointMessage + ")" : Describe(picked)));
                if (picked == null || !picked.Equals(cell))
                {
                    misses.Add(Describe(cell) + " picked " + (picked == null ? "nothing" : Describe(picked)));
                }
            }
            Assert.AreEqual(0, misses.Count, string.Join(Environment.NewLine, misses));
        }

        [TestMethod]
        [Timeout(600000)]
        public void XYLocator_UnderAnotherApplicationsWindow_FindsNothing()
        {
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("A point lookup cannot reach the application: " + InteractiveDesktop.DescribeSession());
            }
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);
            // Otherwise the lookup raises the application above the covering window first.
            pbHelper.NonIntrusiveInput = true;
            UIAuto.AutomationElement target = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, "sle_acc_text");
            Assert.IsNotNull(target, "sle_acc_text was not found");
            if (!IsShownInWindow(target))
            {
                Assert.Inconclusive("sle_acc_text is not on screen: " + Describe(target));
            }

            // Any other window over the application would answer for the point before the covering window is up.
            SetWindowPos(mApp.MainWindowHandle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            Thread.Sleep(500);
            Rectangle window = mWindow.Current.BoundingRectangle;
            Rectangle area = target.Current.BoundingRectangle;
            Point centre = new Point(area.X + (area.Width / 2), area.Y + (area.Height / 2));
            string xy = (centre.X - window.X) + "," + (centre.Y - window.Y);
            if (!FindsAtPoint(pbHelper, xy, target))
            {
                Assert.Inconclusive("XY " + xy + " did not find sle_acc_text before it was covered: " + Describe(UIAuto.AutomationElement.FromPoint(centre)));
            }

            using System.Windows.Forms.Form cover = new System.Windows.Forms.Form
            {
                Text = "Covering window",
                ShowInTaskbar = false,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                Bounds = new Rectangle(centre.X - 40, centre.Y - 20, 80, 40)
            };
            cover.Show();
            SetWindowPos(cover.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            System.Windows.Forms.Application.DoEvents();
            UIAuto.AutomationElement atPoint = UIAuto.AutomationElement.FromPoint(centre);
            if (atPoint == null || atPoint.Current.ProcessId != Environment.ProcessId)
            {
                Assert.Inconclusive("The covering window is not what is at " + centre + ": " + Describe(atPoint));
            }

            pbHelper.taskFinished = false;
            object found = pbHelper.FindElementByLocator(eLocateBy.ByXY, xy);

            Assert.IsNull(found, "XY " + xy + " found " + (found is UIAuto.AutomationElement element ? Describe(element) : found?.ToString()) + " in the window covering the application");
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [TestMethod]
        [Timeout(600000)]
        public void DataWindowEditor_SpiedWhileEditing_IsLocatedAfterFocusLeaves()
        {
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);
            UIAComWrapperHelper windowsHelper = CreateHelper(ePlatformType.Windows);

            UIAuto.AutomationElement dataWindow = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, DataWindowName);
            UIAuto.AutomationElement otherControl = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, "sle_acc_text");
            Assert.IsNotNull(dataWindow, DataWindowName + " was not found");
            Assert.IsNotNull(otherControl, "sle_acc_text was not found");

            otherControl.SetFocus();
            Thread.Sleep(1000);
            List<UIAuto.AutomationElement> idle = [];
            CollectDescendants(dataWindow, 0, idle);
            TestContext.WriteLine("DataWindow without focus, " + idle.Count + " descendants:");
            idle.ForEach(e => TestContext.WriteLine("  " + Describe(e)));

            dataWindow.SetFocus();
            Thread.Sleep(1000);
            List<UIAuto.AutomationElement> editing = [];
            CollectDescendants(dataWindow, 0, editing);
            TestContext.WriteLine("DataWindow with focus, " + editing.Count + " descendants:");
            editing.ForEach(e => TestContext.WriteLine("  " + Describe(e)));

            UIAuto.AutomationElement editor = editing.FirstOrDefault(e => !idle.Any(i => i.Equals(e)));
            if (editor == null)
            {
                Assert.Inconclusive("Focusing the DataWindow did not add an editor to its tree. " + DescribeForeground());
            }

            SpiedElement spied = Spy(pbHelper, windowsHelper, editor);
            TestContext.WriteLine("Spied while editing: " + spied);

            Assert.IsTrue(spied.PbXPathFound, "While editing, the XPath " + spied.PbXPath + " did not find the editor it was created for");
            Rectangle editedCell = editor.Current.BoundingRectangle;
            UIAuto.AutomationElement cell = idle.Where(e => e.Current.BoundingRectangle.IntersectsWith(editedCell))
                .OrderByDescending(e => Area(Rectangle.Intersect(e.Current.BoundingRectangle, editedCell)))
                .First();

            otherControl.SetFocus();
            Thread.Sleep(1000);
            List<UIAuto.AutomationElement> afterwards = [];
            CollectDescendants(dataWindow, 0, afterwards);
            TestContext.WriteLine("After focus left, editor still in the tree: " + afterwards.Any(e => e.Equals(editor)));

            pbHelper.taskFinished = false;
            UIAuto.AutomationElement located = pbHelper.FindElementByLocator(eLocateBy.ByXPath, spied.PbXPath) as UIAuto.AutomationElement;
            Assert.IsNotNull(located, "After focus left the DataWindow, " + spied.PbXPath + " found nothing");
            TestContext.WriteLine("Located after focus left: " + Describe(located));
            Assert.AreEqual(spied.AutomationId, located.Current.AutomationId);
            Assert.AreEqual(editedCell, located.Current.BoundingRectangle, "The editor came back over a different cell");

            string value = AcceptedValue();
            TestContext.WriteLine("Session: " + InteractiveDesktop.DescribeSession());
            string result = pbHelper.SetControlValue(located, value);
            TestContext.WriteLine("SetControlValue: " + result);
            Assert.IsTrue(result.StartsWith("True"), "Set Value through " + spied.PbXPath + " failed: " + result);

            otherControl.SetFocus();
            Thread.Sleep(1000);
            string cellValue = pbHelper.GetControlPropertyValue(cell, "Value");
            TestContext.WriteLine("Cell " + Describe(cell) + " now holds '" + cellValue + "'");
            Assert.AreEqual(value, cellValue, "The value did not reach the DataWindow cell under the editor");
        }

        /// <summary>
        /// The test application's DataWindow opens a "DataWindow Error" message box for a value
        /// that fails the name column's validation rule, and keeps the cell's old value.
        /// </summary>
        [TestMethod]
        [Timeout(600000)]
        public void DataWindowEditor_ValueFailingValidation_ReportsThePowerBuilderMessage()
        {
            UIAComWrapperHelper pbHelper = CreateHelper(ePlatformType.PowerBuilder);
            UIAuto.AutomationElement otherControl = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, "sle_acc_text");
            Assert.IsNotNull(otherControl, "sle_acc_text was not found");
            otherControl.SetFocus();
            Thread.Sleep(1000);

            string xpath = "/" + DataWindowName + "/[AutomationId:10]";
            pbHelper.taskFinished = false;
            UIAuto.AutomationElement editor = pbHelper.FindElementByLocator(eLocateBy.ByXPath, xpath) as UIAuto.AutomationElement;
            if (editor == null)
            {
                Assert.Inconclusive("Focusing the DataWindow did not bring up its editor. " + DescribeForeground());
            }
            TestContext.WriteLine(xpath + " found " + Describe(editor));

            UIAuto.AutomationElement dataWindow = (UIAuto.AutomationElement)pbHelper.FindElementByLocator(eLocateBy.ByName, DataWindowName);
            for (int step = 0; step < 2 && CellUnder(dataWindow, editor) != "name"; step++)
            {
                string stepped = pbHelper.SetControlValue(editor, AcceptedValue());
                Assert.IsTrue(stepped.StartsWith("True"), "Could not move the editor on from the " + CellUnder(dataWindow, editor) + " column: " + stepped);
                pbHelper.taskFinished = false;
                editor = pbHelper.FindElementByLocator(eLocateBy.ByXPath, xpath) as UIAuto.AutomationElement;
                Assert.IsNotNull(editor, xpath + " found nothing after the editor moved on");
                TestContext.WriteLine("Moved on, " + xpath + " found " + Describe(editor) + " over " + CellUnder(dataWindow, editor));
            }
            Assert.AreEqual("name", CellUnder(dataWindow, editor), "The editor did not reach the column with the validation rule");

            string message = null;
            try
            {
                string result = pbHelper.SetControlValue(editor, "Ginger" + DateTime.Now.ToString("HHmmss"));
                TestContext.WriteLine("SetControlValue returned: " + result);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                CloseApplicationMessageBoxes();
            }
            TestContext.WriteLine("SetControlValue failed with: " + message);
            Assert.IsNotNull(message, "A value the DataWindow rejected was not reported as an error");
            StringAssert.Contains(message, "does not pass the validation test");

            string accepted = AcceptedValue();
            string retry = pbHelper.SetControlValue(editor, accepted);
            Assert.IsTrue(retry.StartsWith("True"), "After the message box was closed, " + accepted + " was not set: " + retry);
        }

        /// <summary>
        /// The name of the DataWindow cell the editor covers most of.
        /// </summary>
        private static string CellUnder(UIAuto.AutomationElement dataWindow, UIAuto.AutomationElement editor)
        {
            Rectangle editorArea = editor.Current.BoundingRectangle;
            List<UIAuto.AutomationElement> cells = [];
            CollectDescendants(dataWindow, 0, cells);
            return cells.Where(c => c.Current.NativeWindowHandle == 0 && c.Current.BoundingRectangle.IntersectsWith(editorArea))
                .OrderByDescending(c => Area(Rectangle.Intersect(c.Current.BoundingRectangle, editorArea)))
                .Select(c => c.Current.Name)
                .FirstOrDefault();
        }

        /// <summary>
        /// Short enough for the name column's validation rule, which rejects "Ginger" + HHmmss.
        /// </summary>
        private static string AcceptedValue()
        {
            return "G" + DateTime.Now.ToString("HHmmss");
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        /// <summary>
        /// The DataWindow only opens its editor while its window is active, which Windows
        /// refuses while another application is being used.
        /// </summary>
        private static string DescribeForeground()
        {
            GetWindowThreadProcessId(GetForegroundWindow(), out uint processId);
            string owner;
            try
            {
                owner = Process.GetProcessById((int)processId).ProcessName;
            }
            catch (ArgumentException)
            {
                owner = "process " + processId;
            }
            return "Foreground: " + owner + ". " + InteractiveDesktop.DescribeSession();
        }

        private static void CloseApplicationMessageBoxes()
        {
            UIAuto.TreeWalker walker = UIAuto.TreeWalker.ControlViewWalker;
            for (UIAuto.AutomationElement window = walker.GetFirstChild(UIAuto.AutomationElement.RootElement); window != null; window = walker.GetNextSibling(window))
            {
                if (window.Current.ProcessId != mApp.Id || window.Current.ClassName != "#32770")
                {
                    continue;
                }
                for (UIAuto.AutomationElement part = walker.GetFirstChild(window); part != null; part = walker.GetNextSibling(part))
                {
                    if (part.Current.Name == "OK" && part.TryGetCurrentPattern(UIAuto.InvokePattern.Pattern, out object invoke))
                    {
                        ((UIAuto.InvokePattern)invoke).Invoke();
                        Thread.Sleep(500);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// A screen point can only land on a control drawn inside its window and on the screen.
        /// The test window opens partly above the top of the screen, and a closed combo box's
        /// list reports a rectangle that is not in the window at all.
        /// </summary>
        private static bool IsShownInWindow(UIAuto.AutomationElement element)
        {
            Rectangle area = element.Current.BoundingRectangle;
            Rectangle visible = Rectangle.Intersect(mWindow.Current.BoundingRectangle, System.Windows.Forms.SystemInformation.VirtualScreen);
            return area.Width > 0 && area.Height > 0 && visible.Contains(area);
        }

        private static int Area(Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        private static SpiedElement Spy(UIAComWrapperHelper pbHelper, UIAComWrapperHelper windowsHelper, UIAuto.AutomationElement element)
        {
            SpiedElement s = new SpiedElement { Element = element, Description = Describe(element) };

            ElementInfo info = pbHelper.GetElementInfoFor(element);
            List<ElementLocator> locators = pbHelper.GetElementLocators(info).ToList();

            s.PbXPath = locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByXPath)?.LocateValue;
            s.PbXPathFound = s.PbXPath != null && Finds(pbHelper, eLocateBy.ByXPath, s.PbXPath, element);

            s.PbName = locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByName)?.LocateValue;
            s.PbNameFound = s.PbName != null && Finds(pbHelper, eLocateBy.ByName, s.PbName, element);

            s.AutomationId = locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByAutomationID)?.LocateValue;
            s.AutomationIdFound = s.AutomationId != null && Finds(pbHelper, eLocateBy.ByAutomationID, s.AutomationId, element);

            s.XY = locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByXY)?.LocateValue;
            s.XYFound = s.XY != null && FindsAtPoint(pbHelper, s.XY, element);

            s.WindowsXPath = windowsHelper.GetElementAbsoluteXPath(element);
            s.WindowsXPathFound = Finds(windowsHelper, eLocateBy.ByXPath, s.WindowsXPath, element);
            return s;
        }

        private static bool Finds(UIAComWrapperHelper helper, eLocateBy locateBy, string value, UIAuto.AutomationElement expected)
        {
            try
            {
                helper.taskFinished = false;
                return expected.Equals(helper.FindElementByLocator(locateBy, value));
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// The point can land on a part of the control, such as a combo box's button.
        /// </summary>
        private static bool FindsAtPoint(UIAComWrapperHelper helper, string xy, UIAuto.AutomationElement expected)
        {
            try
            {
                helper.taskFinished = false;
                UIAuto.AutomationElement found = helper.FindElementByLocator(eLocateBy.ByXY, xy) as UIAuto.AutomationElement;
                for (int depth = 0; found != null && depth < 10; depth++)
                {
                    if (expected.Equals(found))
                    {
                        return true;
                    }
                    found = UIAuto.TreeWalker.RawViewWalker.GetParent(found);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void CollectDescendants(UIAuto.AutomationElement parent, int depth, List<UIAuto.AutomationElement> into)
        {
            if (depth > 4)
            {
                return;
            }
            UIAuto.AutomationElement child = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(parent);
            while (child != null)
            {
                into.Add(child);
                CollectDescendants(child, depth + 1, into);
                child = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(child);
            }
        }

        private static string Describe(UIAuto.AutomationElement element)
        {
            try
            {
                return string.Format("type='{0}' name='{1}' id='{2}' class='{3}' rect={4}", element.Current.LocalizedControlType, element.Current.Name,
                    element.Current.AutomationId, element.Current.ClassName, element.Current.BoundingRectangle);
            }
            catch (Exception ex)
            {
                return "<unreadable: " + ex.Message + ">";
            }
        }

        private static UIAComWrapperHelper CreateHelper(ePlatformType platform)
        {
            UIAComWrapperHelper helper = new UIAComWrapperHelper
            {
                mPlatform = platform,
                CurrentWindow = mWindow,
                CurrentWindowName = WindowTitle,
                ImplicitWait = 0
            };
            helper.UpdateRootElement();

            PBDriver driver = new PBDriver { mUIAutomationHelper = helper };
            helper.WindowExplorer = driver;
            return helper;
        }

        private sealed class SpiedElement
        {
            public UIAuto.AutomationElement Element;
            public string Description;
            public string PbXPath;
            public bool PbXPathFound;
            public string PbName;
            public bool PbNameFound;
            public string AutomationId;
            public bool AutomationIdFound;
            public string XY;
            public bool XYFound;
            public string WindowsXPath;
            public bool WindowsXPathFound;

            public string Describe(string locator)
            {
                return locator + "  ->  " + Description;
            }

            public override string ToString()
            {
                return Description + Environment.NewLine
                    + "    XPath=" + Mark(PbXPathFound) + PbXPath + "  Name=" + Mark(PbNameFound) + PbName
                    + "  AutomationId=" + Mark(AutomationIdFound) + AutomationId + "  XY=" + Mark(XYFound) + XY
                    + "  oldXPath=" + Mark(WindowsXPathFound) + WindowsXPath;
            }

            private static string Mark(bool found)
            {
                return found ? "[ok]" : "[MISS]";
            }
        }
    }
}
