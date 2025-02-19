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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers
{
    internal sealed class ActScreenShotHandler
    {
        private readonly ActScreenShot _act;
        private readonly IBrowser _browser;
        private readonly IScreenInfo _screenInfo;
        private readonly IScreenCapture _screenCapture;
        private readonly IBitmapOperations _bitmapOperations;

        // For production
        internal ActScreenShotHandler(ActScreenShot act, IBrowser browser) :
            this(act, browser, screenInfo: TargetFrameworkHelper.Helper, screenCapture: TargetFrameworkHelper.Helper, bitmapOperations: TargetFrameworkHelper.Helper)
        { }

        // All dependencies are injected, used for unit testing
        internal ActScreenShotHandler(ActScreenShot act, IBrowser browser, IScreenInfo screenInfo, IScreenCapture screenCapture, IBitmapOperations bitmapOperations)
        {
            _act = act;
            _browser = browser;
            _screenInfo = screenInfo;
            _screenCapture = screenCapture;
            _bitmapOperations = bitmapOperations;
        }

        internal async Task HandleAsync()
        {
            try
            {
                switch (_act.WindowsToCapture)
                {
                    case Act.eWindowsToCapture.OnlyActiveWindow:
                        await HandleOnlyActiveWindowOperationAsync();
                        break;
                    case Act.eWindowsToCapture.DesktopScreen:
                        await HandleDesktopScreenOperationAsync();
                        break;
                    case Act.eWindowsToCapture.AllAvailableWindows:
                        await HandleAllAvailableWindowsOperationAsync();
                        break;
                    case Act.eWindowsToCapture.FullPage:
                        await HandleFullPageOperationAsync();
                        break;
                    case Act.eWindowsToCapture.FullPageWithUrlAndTimestamp:
                        await HandleFullPageWithURLAndTimestampOperationAsync();
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
        }

        private async Task HandleOnlyActiveWindowOperationAsync()
        {
            byte[] screenshot = await _browser.CurrentWindow.CurrentTab.ScreenshotAsync();
            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.TitleAsync();
            _act.AddScreenShot(screenshot, currentTabTitle);
        }

        private Task HandleDesktopScreenOperationAsync()
        {
            var images = CaptureAllDesktopScreens();
            foreach (var (name, filepath) in images)
            {
                _act.ScreenShotsNames.Add(name);
                _act.ScreenShots.Add(filepath);
            }
            return Task.CompletedTask;
        }

        private IEnumerable<(string name, string filepath)> CaptureAllDesktopScreens()
        {
            List<(string name, string filepath)> savedImages = [];

            for (int screenIndex = 0; screenIndex < _screenInfo.ScreenCount(); screenIndex++)
            {
                Point position = _screenInfo.ScreenPosition(screenIndex);
                Size size = _screenInfo.ScreenSize(screenIndex);

                byte[] image = _screenCapture.Capture(position, size, ImageFormat.Png);

                string name = _screenInfo.ScreenName(screenIndex);
                string filepath = Path.GetTempFileName();

                _bitmapOperations.Save(filepath, image, ImageFormat.Png);

                savedImages.Add((name, filepath));
            }

            return savedImages;
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
                    string currentTabTitle = await tab.TitleAsync();
                    _act.AddScreenShot(screenshot, currentTabTitle);
                }
            }
        }

        private async Task HandleFullPageOperationAsync()
        {
            byte[] screenshot = await _browser.CurrentWindow.CurrentTab.ScreenshotFullPageAsync();
            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.TitleAsync();
            _act.AddScreenShot(screenshot, currentTabTitle);
        }

        private async Task HandleFullPageWithURLAndTimestampOperationAsync()
        {
            List<byte[]> images =
                [
                    await GetBrowserHeaderScreenShotAsync(),
                    await GetFullPageScreenShotAsync(),
                    GetTaskbarScreenShot()
                ];

            byte[] mergedImage = _bitmapOperations.MergeVertically(images, ImageFormat.Png);

            string filepath = Path.GetTempFileName();
            _bitmapOperations.Save(filepath, mergedImage, ImageFormat.Png);

            string currentTabTitle = await _browser.CurrentWindow.CurrentTab.TitleAsync();

            _act.ScreenShotsNames.Add(currentTabTitle);
            _act.ScreenShots.Add(filepath);
        }

        private async Task<byte[]> GetBrowserHeaderScreenShotAsync()
        {
            Point browserWindowPosition = await GetBrowserWindowPositionAsync();
            Size browserWindowSize = await GetBrowserWindowSizeAsync();
            Size viewportSize = await GetViewportSizeAsync();
            double devicePixelRatio = await GetDevicePixeRatioAsync();

            Point browserHeaderPosition = new()
            {
                X = (int)(browserWindowPosition.X * devicePixelRatio),
                Y = (int)(browserWindowPosition.Y * devicePixelRatio)
            };
            Size browserHeaderSize = new()
            {
                Width = (int)(browserWindowSize.Width * devicePixelRatio),
                Height = (int)((browserWindowSize.Height - viewportSize.Height) * devicePixelRatio),
            };

            return _screenCapture.Capture(browserHeaderPosition, browserHeaderSize, ImageFormat.Png);
        }

        private async Task<Point> GetBrowserWindowPositionAsync()
        {
            int windowX = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.screenX"));
            int windowY = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.screenY"));
            return new Point(windowX, windowY);
        }

        private async Task<Size> GetBrowserWindowSizeAsync()
        {
            int width = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.outerWidth"));
            int height = int.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.outerHeight"));

            return new Size(width, height);
        }

        private Task<Size> GetViewportSizeAsync()
        {
            return _browser.CurrentWindow.CurrentTab.ViewportSizeAsync();
        }

        private async Task<double> GetDevicePixeRatioAsync()
        {
            double pixelRatio = double.Parse(await _browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync("window.devicePixelRatio"));
            return pixelRatio;
        }

        private Task<byte[]> GetFullPageScreenShotAsync()
        {
            return _browser.CurrentWindow.CurrentTab.ScreenshotFullPageAsync();
        }

        private byte[] GetTaskbarScreenShot()
        {
            int primaryScreenIndex = _screenInfo.PrimaryScreenIndex();
            Point position = _screenInfo.TaskbarPosition(primaryScreenIndex);
            Size size = _screenInfo.TaskbarSize(primaryScreenIndex);
            return _screenCapture.Capture(position, size, ImageFormat.Png);
        }
    }
}
