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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace GingerCoreNETUnitTest.Drivers.CoreDrivers.Web.ActionHandlers
{
    [TestClass]
    [TestCategory(TestCategory.UnitTest)]
    public class ActScreenShotHandlerTests
    {
        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindow_OneScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindow_OneScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindow_ScreenShotNameIsTabTitle()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            IBrowser browser = MockBrowser().Object;
            string tabTitle = await browser.CurrentWindow.CurrentTab.TitleAsync();
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: tabTitle, actual: act.ScreenShotsNames.First());
        }

        [TestMethod]
        public async Task HandleAsync_OnlytActiveWindowErrorGettingScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlytActiveWindowErrorGettingScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindowErrorGettingScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindowErrorGettingTabTitle_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindowErrorGettingTabTitle_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_OnlyActiveWindowErrorGettingTitle_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenSingleScreen_OneScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenSingleScreen_OneScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenSingleScreen_ScreenShotNameIsScreenName()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            string screenName = "Mock-Screen-Name";
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenName(0)).Returns(screenName);
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: screenName, actual: act.ScreenShotsNames.First());
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenMultipleScreen_MultipleScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            int screenCount = 3;
            IScreenInfo screenInfo = MockMultipleScreenInfo(screenCount).Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: screenCount, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenMultipleScreen_MultipleScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            int screenCount = 3;
            IScreenInfo screenInfo = MockMultipleScreenInfo(screenCount).Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: screenCount, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenMultipleScreen_ScreenShotNamesAreScreenNames()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            int screenCount = 3;
            IScreenInfo screenInfo = MockMultipleScreenInfo(screenCount).Object;
            string[] screenNames = new string[screenCount];
            for (int index = 0; index < screenCount; index++)
            {
                screenNames[index] = screenInfo.ScreenName(index);
            }
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            for (int index = 0; index < screenCount; index++)
            {
                string screenName = screenNames[index];
                string screenShotName = act.ScreenShotsNames[index];
                Assert.AreEqual(expected: screenName, actual: screenShotName);
            }
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenPosition_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenPosition(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenPosition_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenPosition(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenPosition_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenPosition(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenSize_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenSize(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenSize_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenSize(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenSize_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenSize(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenName_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenName(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenName_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenName(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenName_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.ScreenName(0)).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorGettingScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorSavingBitmap_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            Mock<IBitmapOperations> mockBitmapOperations = MockBitmapOperations();
            mockBitmapOperations.Setup(o => o.Save(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IBitmapOperations bitmapOperations = mockBitmapOperations.Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorSavingBitmap_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            Mock<IBitmapOperations> mockBitmapOperations = MockBitmapOperations();
            mockBitmapOperations.Setup(o => o.Save(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IBitmapOperations bitmapOperations = mockBitmapOperations.Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_DesktopScreenErrorSavingBitmap_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.DesktopScreen,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            Mock<IBitmapOperations> mockBitmapOperations = MockBitmapOperations();
            mockBitmapOperations.Setup(o => o.Save(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IBitmapOperations bitmapOperations = mockBitmapOperations.Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsSingleWindow_OneScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsSingleWindow_OneScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsSingleWindow_ScreenShotNameIsTabTitle()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            string tabTitle = await browser.CurrentWindow.CurrentTab.TitleAsync();
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: tabTitle, actual: act.ScreenShotsNames.First());
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsMultipleWindow_MultipleScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            int windowCount = 3;
            IBrowser browser = MockBrowser(windowCount, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: windowCount, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsMultipleWindow_MultipleScreenShotNamesAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            int windowCount = 3;
            IBrowser browser = MockBrowser(windowCount, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: windowCount, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsMultipleWindow_ScreenShotNamesAreTabTitles()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            int windowCount = 3;
            IBrowser browser = MockBrowser(windowCount, tabCount: 1).Object;
            string[] tabTitles = new string[windowCount];
            for (int index = 0; index < windowCount; index++)
            {
                tabTitles[index] = await browser.Windows.ElementAt(index).CurrentTab.TitleAsync();
            }
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            for (int index = 0; index < windowCount; index++)
            {
                string screenShotName = act.ScreenShotsNames[index];
                string tabTitle = tabTitles[index];
                Assert.AreEqual(expected: tabTitle, actual: screenShotName);
            }
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingTabTitle_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingTabTitle_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_AllAvailableWindowsErrorGettingTabTitle_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.AllAvailableWindows,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotAsync()).ThrowsAsync(new Exception("Mock-Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPage_OneScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPage_OneScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPage_ScreenShotNameIsTabTitle()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            IBrowser browser = MockBrowser().Object;
            string tabTitle = await browser.CurrentWindow.CurrentTab.TitleAsync();
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: tabTitle, actual: act.ScreenShotsNames.First());
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingTitle_ErrorAddedToAction()
        {

            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingTitle_ScreenShotNotAddedToAction()
        {

            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageErrorGettingTitle_ScreenShotNameNotAddedToAction()
        {

            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPage,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.TitleAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStamp_OneScreenShotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStamp_OneScreenShotNameAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser().Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 1, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStamp_ScreenShotNameIsTabTitle()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser().Object;
            string tabTitle = await browser.CurrentWindow.CurrentTab.TitleAsync();
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: tabTitle, actual: act.ScreenShotsNames.First());
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorExecutingJavascript_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ExecuteJavascriptAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorExecutingJavascript_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ExecuteJavascriptAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorExecutingJavascript_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ExecuteJavascriptAsync(It.IsAny<string>())).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingFullPageScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingFullPageScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingFullPageScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            Mock<IBrowserTab> mockTab = MockBrowserTab();
            mockTab.Setup(o => o.ScreenshotFullPageAsync()).ThrowsAsync(new Exception("Mock Exception"));
            IBrowserTab tab = mockTab.Object;
            IBrowserWindow window = MockBrowserWindow(tab).Object;
            IBrowser browser = MockBrowser(window).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingPrimaryScreenIndex_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.PrimaryScreenIndex()).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingPrimaryScreenIndex_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.PrimaryScreenIndex()).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingPrimaryScreenIndex_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.PrimaryScreenIndex()).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarPosition_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarPosition(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarPosition_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarPosition(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarPosition_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarPosition(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarSize_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarSize(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarSize_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarSize(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarSize_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            Mock<IScreenInfo> mockScreenInfo = MockSingleScreenInfo();
            mockScreenInfo.Setup(o => o.TaskbarSize(It.IsAny<int>())).Throws(new Exception("Mock-Exception"));
            IScreenInfo screenInfo = mockScreenInfo.Object;
            IScreenCapture screenCapture = MockScreenCapture().Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarScreenShot_ErrorAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.IsFalse(string.IsNullOrEmpty(act.Error));
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarScreenShot_ScreenShotNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShots.Count);
        }

        [TestMethod]
        public async Task HandleAsync_FullPageWithUrlAndTimeStampErrorGettingTaskbarScreenShot_ScreenShotNameNotAddedToAction()
        {
            ActScreenShot act = new()
            {
                WindowsToCapture = Act.eWindowsToCapture.FullPageWithUrlAndTimestamp,
            };
            IBrowser browser = MockBrowser(windowCount: 1, tabCount: 1).Object;
            IScreenInfo screenInfo = MockSingleScreenInfo().Object;
            Mock<IScreenCapture> mockScreenCapture = MockScreenCapture();
            mockScreenCapture.Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>())).Throws(new Exception("Mock-Exception"));
            IScreenCapture screenCapture = mockScreenCapture.Object;
            IBitmapOperations bitmapOperations = MockBitmapOperations().Object;
            ActScreenShotHandler handler = new(act, browser, screenInfo, screenCapture, bitmapOperations);

            await handler.HandleAsync();

            Assert.AreEqual(expected: 0, actual: act.ScreenShotsNames.Count);
        }

        private static Mock<IBrowser> MockBrowser(int windowCount = 1, int tabCount = 1)
        {
            IBrowserWindow[] windows = new IBrowserWindow[windowCount];
            for (int windowIndex = 0; windowIndex < windowCount; windowIndex++)
            {
                Mock<IBrowserWindow> window = MockBrowserWindow(tabCount);
                windows[windowIndex] = window.Object;
            }

            return MockBrowser(windows);
        }

        private static Mock<IBrowser> MockBrowser(params IBrowserWindow[] windows)
        {
            Mock<IBrowser> browser = new(MockBehavior.Strict);

            browser
                .Setup(o => o.Windows)
                .Returns(windows);
            browser
                .Setup(o => o.CurrentWindow)
                .Returns(windows[0]);

            return browser;
        }

        private static Mock<IBrowserWindow> MockBrowserWindow(int tabCount)
        {
            IBrowserTab[] tabs = new IBrowserTab[tabCount];
            for (int tabIndex = 0; tabIndex < tabCount; tabIndex++)
            {
                Mock<IBrowserTab> tab = MockBrowserTab(title: $"Mock-Tab-{tabIndex + 1}");
                tabs[tabIndex] = tab.Object;
            }

            return MockBrowserWindow(tabs);
        }

        private static Mock<IBrowserWindow> MockBrowserWindow(params IBrowserTab[] tabs)
        {
            Mock<IBrowserWindow> window = new(MockBehavior.Strict);

            window
                .Setup(o => o.Tabs)
                .Returns(tabs);
            window
                .Setup(o => o.CurrentTab)
                .Returns(tabs[0]);

            return window;
        }

        private static Mock<IBrowserTab> MockBrowserTab(string title = "")
        {
            if (string.IsNullOrEmpty(title))
            {
                title = $"Mock-Tab-{Guid.NewGuid()}";
            }

            Mock<IBrowserTab> tab = new(MockBehavior.Strict);

            tab
                .Setup(o => o.TitleAsync())
                .ReturnsAsync(title);
            tab
                .Setup(o => o.ScreenshotAsync())
                .ReturnsAsync(new byte[1]);
            tab
                .Setup(o => o.ScreenshotFullPageAsync())
                .ReturnsAsync(new byte[1]);
            tab
                .Setup(o => o.ExecuteJavascriptAsync("window.screenX"))
                .ReturnsAsync("0");
            tab
                .Setup(o => o.ExecuteJavascriptAsync("window.screenY"))
                .ReturnsAsync("0");
            tab
                .Setup(o => o.ExecuteJavascriptAsync("window.outerWidth"))
                .ReturnsAsync("1918");
            tab
                .Setup(o => o.ExecuteJavascriptAsync("window.outerHeight"))
                .ReturnsAsync("1038");
            tab
                .Setup(o => o.ExecuteJavascriptAsync("window.devicePixelRatio"))
                .ReturnsAsync("1");
            tab
                .Setup(o => o.ViewportSizeAsync())
                .ReturnsAsync(new Size(width: 1394, height: 919));

            return tab;
        }

        private static Mock<IScreenInfo> MockSingleScreenInfo()
        {
            Mock<IScreenInfo> screenInfo = new();

            screenInfo
                .Setup(o => o.ScreenCount())
                .Returns(1);
            screenInfo
                .Setup(o => o.PrimaryScreenIndex())
                .Returns(0);
            screenInfo
                .Setup(o => o.ScreenName(0))
                .Returns("Mock-Primary-Screen");
            screenInfo
                .Setup(o => o.ScreenPosition(0))
                .Returns(new Point(x: 0, y: 0));
            screenInfo
                .Setup(o => o.ScreenSize(0))
                .Returns(new Size(width: 1280, height: 720));
            screenInfo
                .Setup(o => o.TaskbarPosition(0))
                .Returns(new Point(x: 0, y: 670));
            screenInfo
                .Setup(o => o.TaskbarSize(0))
                .Returns(new Size(width: 1280, height: 50));

            return screenInfo;
        }

        private static Mock<IScreenInfo> MockMultipleScreenInfo(int count)
        {
            Mock<IScreenInfo> screenInfo = new();

            screenInfo
                .Setup(o => o.ScreenCount())
                .Returns(count);
            screenInfo
                .Setup(o => o.PrimaryScreenIndex())
                .Returns(0);

            for (int index = 0; index < count; index++)
            {
                screenInfo
                    .Setup(o => o.ScreenName(index))
                    .Returns($"Mock-Screen-{index + 1}");
                screenInfo
                    .Setup(o => o.ScreenPosition(index))
                    .Returns(new Point(x: 1280 * index, y: 0));
                screenInfo
                    .Setup(o => o.ScreenSize(index))
                    .Returns(new Size(width: 1280, height: 720));
                screenInfo
                    .Setup(o => o.TaskbarPosition(index))
                    .Returns(new Point(x: 1280 * index, y: 670));
                screenInfo
                    .Setup(o => o.TaskbarSize(index))
                    .Returns(new Size(width: 1280, height: 50));
            }

            return screenInfo;
        }

        private static Mock<IScreenCapture> MockScreenCapture()
        {
            Mock<IScreenCapture> screenCapture = new();

            screenCapture
                .Setup(o => o.Capture(It.IsAny<Point>(), It.IsAny<Size>(), It.IsAny<ImageFormat>()))
                .Returns(new byte[1]);

            return screenCapture;
        }

        private static Mock<IBitmapOperations> MockBitmapOperations()
        {
            Mock<IBitmapOperations> bitmapOperations = new();

            bitmapOperations
                .Setup(o => o.MergeVertically(It.IsAny<IEnumerable<byte[]>>(), It.IsAny<ImageFormat>()))
                .Returns(new byte[1]);
            bitmapOperations
                .Setup(o => o.Save(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<ImageFormat>()));

            return bitmapOperations;
        }
    }
}
