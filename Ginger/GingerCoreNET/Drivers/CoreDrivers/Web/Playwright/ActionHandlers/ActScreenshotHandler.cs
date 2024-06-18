using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using GingerCore.Actions;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V125.Page;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers
{
    internal sealed class ActScreenShotHandler
    {
        private readonly ActScreenShot _act;
        private readonly IBrowser _browser;

        internal ActScreenShotHandler(ActScreenShot actScreenShot, IBrowser browser)
        {
            _act = actScreenShot;
            _browser = browser;
        }

        [SupportedOSPlatform("windows")]
        internal Task HandleAsync()
        {
            Task operationTask = Task.CompletedTask;
            try
            {
                switch (_act.WindowsToCapture)
                {
                    case Act.eWindowsToCapture.OnlyActiveWindow:
                        operationTask = HandleOnlyActiveWindowOperationAsync();
                        break;
                    case Act.eWindowsToCapture.DesktopScreen:
                        operationTask = HandleDesktopScreenOperationAsync();
                        break;
                    case Act.eWindowsToCapture.AllAvailableWindows:
                        operationTask = HandleAllAvailableWindowsOperationAsync();
                        break;
                    case Act.eWindowsToCapture.FullPage:
                        operationTask = HandleFullPageOperationAsync();
                        break;
                    case Act.eWindowsToCapture.FullPageWithUrlAndTimestamp:
                        operationTask = HandleFullPageWithURLAndTimestampOperationAsync();
                        break;
                    default:
                        string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(Act.eWindowsToCapture), _act.WindowsToCapture);
                        _act.Error = $"Operation '{operationName}' is not supported";
                        break;
                }
            }
            catch (Exception ex)
            {
                _act.Error = ex.Message;
            }
            return operationTask;
        }

        private async Task HandleOnlyActiveWindowOperationAsync()
        {
            byte[] screenshot = await _browser.CurrentWindow.CurrentTab.ScreenshotAsync();
            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.GetTitleAsync();
            _act.AddScreenShot(screenshot, currentTabTitle);
        }

        private Task HandleDesktopScreenOperationAsync()
        {
            bool captureAllScreens = true;
            Dictionary<string, string> screenshots = TargetFrameworkHelper.Helper.TakeDesktopScreenShot(captureAllScreens);
            foreach (KeyValuePair<string, string> entry in screenshots)
            {
                string name = entry.Key;
                string path = entry.Value;
                _act.ScreenShotsNames.Add(name);
                _act.ScreenShots.Add(path);
            }
            return Task.CompletedTask;
        }

        private async Task HandleAllAvailableWindowsOperationAsync()
        {
            List<IBrowserWindow> windows = new(_browser.Windows);
            foreach (IBrowserWindow window in windows)
            {
                List<IBrowserTab> tabs = new(window.Tabs);
                foreach (IBrowserTab tab in tabs)
                {
                    byte[] screenshot = await tab.ScreenshotAsync();
                    string currentTabTitle = await tab.GetTitleAsync();
                    _act.AddScreenShot(screenshot, currentTabTitle);
                }
            }
        }

        private async Task HandleFullPageOperationAsync()
        {
            byte[] screenshot = await _browser.CurrentWindow.CurrentTab.ScreenshotFullPageAsync();
            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.GetTitleAsync();
            _act.AddScreenShot(screenshot, currentTabTitle);
        }

        [SupportedOSPlatform("windows")]
        private async Task HandleFullPageWithURLAndTimestampOperationAsync()
        {
            List<Bitmap?> images =
                [
                    await GetBrowserHeaderScreenshotAsync(),
                    await GetFullPageScreenshotAsync(),
                    GetTaskbarScreenshot()
                ];
            string filepath = TargetFrameworkHelper.Helper.MergeVerticallyAndSaveBitmaps(images.Where(img => img != null).ToArray());
            foreach (Bitmap? img in images)
            {
                img?.Dispose();
            }

            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.GetTitleAsync();
            _act.ScreenShotsNames.Add(currentTabTitle);
            _act.ScreenShots.Add(filepath);
        }

        private async Task<Bitmap?> GetBrowserHeaderScreenshotAsync()
        {
            Point? browserWindowPosition = await GetBrowserWindowPositionAsync();
            if (browserWindowPosition == null)
            {
                return null;
            }

            Size? browserWindowSize = await GetBrowserWindowSizeAsync();
            if (browserWindowSize == null)
            {
                return null;
            }

            Size viewportSize = await GetViewportSizeAsync();
            double? devicePixelRatio = await GetDevicePixeRatioAsync();
            if (devicePixelRatio == null)
            {
                return null;
            }

            return TargetFrameworkHelper.Helper.GetBrowserHeaderScreenshot(browserWindowPosition.Value, browserWindowSize.Value, viewportSize, devicePixelRatio.Value);
        }

        private async Task<Point?> GetBrowserWindowPositionAsync()
        {
            int windowX;
            int windowY;
            try
            {
                windowX = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.screenX"));
                windowY = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.screenY"));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to get browser window position", ex);
                return null;
            }
            return new Point(windowX, windowY);
        }

        private async Task<Size?> GetBrowserWindowSizeAsync()
        {
            int width;
            int height;
            try
            {
                width = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.outerWidth"));
                height = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.outerHeight"));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to get browser window size", ex);
                return null;
            }
            return new Size(width, height);
        }

        private Task<Size> GetViewportSizeAsync()
        {
            return _browser.CurrentWindow.CurrentTab.ViewportSizeAsync();
        }

        private async Task<double?> GetDevicePixeRatioAsync()
        {
            try
            {
                double pixelRatio = double.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.devicePixelRatio"));
                return pixelRatio;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to get device pixel ratio", ex);
                return null;
            }
        }

        private async Task<Bitmap?> GetFullPageScreenshotAsync()
        {
            byte[] screenshot = await _browser.CurrentWindow.CurrentTab.ScreenshotFullPageAsync();
            TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(Bitmap));
            return (Bitmap?)typeConverter.ConvertFrom(screenshot);
        }

        private Bitmap GetTaskbarScreenshot()
        {
            return TargetFrameworkHelper.Helper.GetTaskbarScreenshot();
        }
    }
}
