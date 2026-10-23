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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace UnitTest
{
    /// <summary>
    /// The tab control and the DataWindow grids of the PowerBuilder test applications.
    /// </summary>
    [Level3]
    [TestClass]
    public class PbTestAppTabAndGridTest
    {
        private static Process mApp;
        private static UIAuto.AutomationElement mWindow;

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void LaunchApp()
        {
            mApp = Process.Start(new ProcessStartInfo
            {
                FileName = "pb_test_app.exe",
                WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp"),
                UseShellExecute = true
            });
            mWindow = WaitForMainWindow(mApp, "Simple Page");
            Assert.IsNotNull(mWindow, "The PowerBuilder test application did not show 'Simple Page'");
        }

        [TestCleanup]
        public void CloseApp()
        {
            Close(mApp);
            mApp = null;
            mWindow = null;
        }

        [TestMethod]
        [Timeout(600000)]
        public void TabControl_SelectsByCaptionAndByNumber_WithoutTheMouse()
        {
            UIAuto.AutomationElement tabbed = OpenTabbedWindow();
            UIAComWrapperHelper helper = CreateHelper(tabbed);
            helper.NonIntrusiveInput = true;
            UIAuto.AutomationElement tab = TabControlOf(tabbed);
            Point cursor = System.Windows.Forms.Cursor.Position;

            Assert.IsTrue(helper.SelectTab(tab, "Data Grid"), "Selecting the tab by its caption failed");
            Assert.AreEqual("Data Grid", PageName(tab));
            helper.SelectControlByIndex(tab, "3");
            Assert.AreEqual("Custom Grid", PageName(tab));
            Assert.AreEqual("True", helper.SetControlValue(tab, "Information"));
            Assert.AreEqual("Information", PageName(tab));
            Assert.AreEqual(cursor, System.Windows.Forms.Cursor.Position, "The mouse was moved");
        }

        [TestMethod]
        [Timeout(600000)]
        public void TabControl_PartlyCoveredByAnotherWindow_SelectsTheTabWithoutClickingThatWindow()
        {
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("The tab strip is clicked with the mouse only on a desktop it can reach: " + InteractiveDesktop.DescribeSession());
            }
            UIAuto.AutomationElement tabbed = OpenTabbedWindow();
            UIAComWrapperHelper helper = CreateHelper(tabbed);
            UIAuto.AutomationElement tab = TabControlOf(tabbed);
            helper.NonIntrusiveInput = true;
            Assert.IsTrue(helper.SelectTab(tab, "1", true), "Could not start from the first tab");
            helper.NonIntrusiveInput = false;

            // Any other window over the application would take the clicks meant for the uncovered part.
            SetWindowPos(GetAncestor(new IntPtr(tabbed.Current.NativeWindowHandle), GA_ROOT), HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            Thread.Sleep(500);
            Rectangle strip = tab.Current.BoundingRectangle;
            Rectangle page = PageOf(tab).Current.BoundingRectangle;
            int stripY = (strip.Y + page.Y) / 2;

            int clicks = 0;
            using System.Windows.Forms.Form cover = new System.Windows.Forms.Form
            {
                Text = "Covering window",
                ShowInTaskbar = false,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                StartPosition = System.Windows.Forms.FormStartPosition.Manual,
                // Over part of the first tab, which the scan then clicks both with the mouse and by window message.
                Bounds = new Rectangle(strip.X + 30, stripY - 10, 30, 20)
            };
            cover.MouseDown += (sender, e) => clicks++;
            cover.Show();
            SetWindowPos(cover.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE);
            System.Windows.Forms.Application.DoEvents();
            UIAuto.AutomationElement atCover = UIAuto.AutomationElement.FromPoint(new Point(strip.X + 45, stripY));
            if (atCover == null || atCover.Current.ProcessId != Environment.ProcessId)
            {
                Assert.Inconclusive("The covering window is not what is over the tab strip: " + Describe(atCover));
            }

            bool byCaption = helper.SelectTab(tab, "Custom Grid");
            string pageByCaption = PageName(tab);
            // From a tab other than the first, whose page the scan must not count as the first tab.
            bool byNumber = helper.SelectTab(tab, "2", true);
            string pageByNumber = PageName(tab);
            System.Windows.Forms.Application.DoEvents();

            Assert.AreEqual(0, clicks, "The window covering the tab strip was clicked");
            Assert.IsTrue(byCaption, "Selecting the tab by its caption failed");
            Assert.AreEqual("Custom Grid", pageByCaption);
            Assert.IsTrue(byNumber, "Selecting the tab by its number failed");
            Assert.AreEqual("Data Grid", pageByNumber);
        }

        [TestMethod]
        [Timeout(600000)]
        public void TypelessGridCell_OfAMinimizedWindow_ReadsEmptyRatherThanWhateverIsOnTheScreen()
        {
            if (!InteractiveDesktop.IsAvailable())
            {
                Assert.Inconclusive("An empty cell is read with the mouse only on a desktop it can reach: " + InteractiveDesktop.DescribeSession());
            }
            Process gridApp = Process.Start(new ProcessStartInfo
            {
                FileName = "csm_grid_target.exe",
                WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp"),
                UseShellExecute = true
            });
            try
            {
                UIAuto.AutomationElement window = WaitForMainWindow(gridApp, null);
                Assert.IsNotNull(window, "csm_grid_target did not show a window");
                UIAuto.AutomationElement cell = null;
                Stopwatch loading = Stopwatch.StartNew();
                while (cell == null && loading.Elapsed < TimeSpan.FromSeconds(20))
                {
                    Thread.Sleep(500);
                    UIAuto.AutomationElement dataWindow = Descendants(window, 4).FirstOrDefault(e => e.Current.ClassName.StartsWith("pbdw", StringComparison.OrdinalIgnoreCase));
                    cell = dataWindow == null ? null : Children(dataWindow).FirstOrDefault(e => e.Current.LocalizedControlType == "" && e.Current.Name == "rate_code_freq");
                }
                Assert.IsNotNull(cell, "The grid has no rate_code_freq cell without a control type: " + Describe(window));
                UIAComWrapperHelper helper = CreateHelper(window);

                ShowWindow(new IntPtr(window.Current.NativeWindowHandle), SW_SHOWMINNOACTIVE);
                Stopwatch clock = Stopwatch.StartNew();
                while (cell.Current.BoundingRectangle.X > -10000 && clock.Elapsed < TimeSpan.FromSeconds(10))
                {
                    Thread.Sleep(200);
                }
                Assert.IsTrue(cell.Current.BoundingRectangle.X <= -10000, "The window did not minimize: " + Describe(cell));

                string value = helper.GetControlValue(cell);

                Assert.AreEqual(string.Empty, value, "A cell of the minimized window was read off whatever is on the screen");
            }
            finally
            {
                Close(gridApp);
            }
        }

        private UIAuto.AutomationElement OpenTabbedWindow()
        {
            UIAuto.AutomationElement opener = Descendants(mWindow, 6).FirstOrDefault(e => e.Current.Name == "Open Tabbed Window");
            Assert.IsNotNull(opener, "Simple Page has no 'Open Tabbed Window' link");
            // Invoke returns once the click handler does, which a window that opens modally never lets it do.
            Task.Run(() => ((UIAuto.InvokePattern)opener.GetCurrentPattern(UIAuto.InvokePattern.Pattern)).Invoke()).Wait(TimeSpan.FromSeconds(10));

            Stopwatch clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(20))
            {
                UIAuto.AutomationElement tabbed = Descendants(mWindow, 2).FirstOrDefault(e => e.Current.Name == "Tabbed Page")
                    ?? Children(UIAuto.AutomationElement.RootElement).FirstOrDefault(e => e.Current.ProcessId == mApp.Id && e.Current.Name == "Tabbed Page");
                if (tabbed != null && TabControlOf(tabbed) != null)
                {
                    return tabbed;
                }
                Thread.Sleep(500);
            }
            Assert.Fail("'Tabbed Page' did not open");
            return null;
        }

        private static UIAuto.AutomationElement TabControlOf(UIAuto.AutomationElement window)
        {
            return Descendants(window, 3).FirstOrDefault(e => e.Current.ClassName.StartsWith("PBTabControl32", StringComparison.Ordinal));
        }

        /// <summary>
        /// The page of the selected tab, the only one in the tree.
        /// </summary>
        private static UIAuto.AutomationElement PageOf(UIAuto.AutomationElement tab)
        {
            return Children(tab).FirstOrDefault(e => e.Current.LocalizedControlType == "pane");
        }

        private static string PageName(UIAuto.AutomationElement tab)
        {
            return PageOf(tab)?.Current.Name;
        }

        private static UIAuto.AutomationElement WaitForMainWindow(Process app, string title)
        {
            Stopwatch clock = Stopwatch.StartNew();
            while (clock.Elapsed < TimeSpan.FromSeconds(60))
            {
                Thread.Sleep(500);
                app.Refresh();
                if (app.MainWindowHandle != IntPtr.Zero && (title == null || app.MainWindowTitle == title))
                {
                    return UIAuto.AutomationElement.FromHandle(app.MainWindowHandle);
                }
            }
            return null;
        }

        private static void Close(Process app)
        {
            try
            {
                app?.Kill();
                app?.WaitForExit(10000);
            }
            catch (InvalidOperationException)
            {
                // already gone
            }
            app?.Dispose();
        }

        private static List<UIAuto.AutomationElement> Children(UIAuto.AutomationElement parent)
        {
            List<UIAuto.AutomationElement> children = [];
            for (UIAuto.AutomationElement child = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(parent); child != null; child = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(child))
            {
                children.Add(child);
            }
            return children;
        }

        private static List<UIAuto.AutomationElement> Descendants(UIAuto.AutomationElement parent, int depth)
        {
            List<UIAuto.AutomationElement> descendants = [];
            foreach (UIAuto.AutomationElement child in Children(parent))
            {
                descendants.Add(child);
                if (depth > 1)
                {
                    descendants.AddRange(Descendants(child, depth - 1));
                }
            }
            return descendants;
        }

        private static string Describe(UIAuto.AutomationElement element)
        {
            if (element == null)
            {
                return "nothing";
            }
            try
            {
                return string.Format("type='{0}' name='{1}' class='{2}' process={3} rect={4}", element.Current.LocalizedControlType, element.Current.Name,
                    element.Current.ClassName, element.Current.ProcessId, element.Current.BoundingRectangle);
            }
            catch (Exception ex)
            {
                return "<unreadable: " + ex.Message + ">";
            }
        }

        private static UIAComWrapperHelper CreateHelper(UIAuto.AutomationElement window)
        {
            UIAComWrapperHelper helper = new UIAComWrapperHelper
            {
                mPlatform = ePlatformType.PowerBuilder,
                CurrentWindow = window,
                CurrentWindowName = window.Current.Name,
                ImplicitWait = 0
            };
            helper.UpdateRootElement();
            helper.WindowExplorer = new PBDriver { mUIAutomationHelper = helper };
            return helper;
        }

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint GA_ROOT = 2;
        private const int SW_SHOWMINNOACTIVE = 7;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr insertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int command);
    }
}
