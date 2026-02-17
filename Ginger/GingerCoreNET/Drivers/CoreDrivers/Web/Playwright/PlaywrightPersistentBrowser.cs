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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightBrowserType = Microsoft.Playwright.IBrowserType;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightPersistentBrowser : PlaywrightBrowser
    {
        private IBrowserWindow _currentWindow;

        public override IBrowserWindow CurrentWindow => _currentWindow;

        internal PlaywrightPersistentBrowser(IPlaywright playwright, WebBrowserType browserType, Options? options = null, IBrowser.OnBrowserClose? onBrowserClose = null) :
            base(playwright, browserType, options, onBrowserClose)
        {
            //Remove synchronous object creation if it is not possible. instead have a async Creator method
            IBrowserWindow? newWindow = Task.Run(() =>
            {
                try
                {
                    //this code needs to be executed in a separate Task otherwise, it will cause a deadlock and freeze the calling thread
                    //check this for an example https://stackoverflow.com/a/43912280/12190808
                    return NewWindowAsync();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while creating {nameof(IBrowserWindow)}", ex);
                    return null!;
                }
            }).Result;

            if (newWindow == null)
            {
                throw new Exception($"Error occurred while creating {nameof(IBrowserWindow)}");
            }
            _currentWindow = newWindow;
        }

        public override async Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true)
        {
            ThrowIfClosed();

            IPlaywrightBrowserContext context = await LaunchBrowserContextWithInstallationAsync();

            PlaywrightBrowserWindow window = new(context, OnWindowClose);
            _windows.AddLast(window);

            if (setAsCurrent)
            {
                _currentWindow = window;
            }

            return window;
        }

        private async Task<IPlaywrightBrowserContext> LaunchBrowserContextWithInstallationAsync()
        {
            IPlaywrightBrowserContext browserContext;
            try
            {
                if (_options.EnableVideoRecording)
                {
                    try
                    {
                        ExecutePlaywrightInstallation("ffmpeg");
                    }
                    catch (PlaywrightException ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Unable to install Playwright ffmpeg dependency, so disabling Video Recording.", ex);
                        _options.EnableVideoRecording = false;
                    }
                }

                browserContext = await LaunchBrowserContextAsync();
            }
            catch (PlaywrightException ex)
            {
                if (ex.Message.Contains(BrowserExecutableNotFoundErrorMessage))
                {
                    ExecutePlaywrightInstallationCommand(_browserType);
                    browserContext = await LaunchBrowserContextAsync();
                }
                else
                {
                    throw;
                }
            }
            return browserContext;
        }

        private async Task<IPlaywrightBrowserContext> LaunchBrowserContextAsync()
        {
            IPlaywrightBrowserType playwrightBrowserType = GetPlaywrightBrowserType();
            BrowserTypeLaunchPersistentContextOptions? launchOptions = BuildBrowserContextLaunchOptions();

            IPlaywrightBrowserContext context = await playwrightBrowserType.LaunchPersistentContextAsync(userDataDir: string.Empty, launchOptions);
            return context;
        }

        private BrowserTypeLaunchPersistentContextOptions? BuildBrowserContextLaunchOptions()
        {
            if (_options == null)
            {
                return null;
            }

            RecordVideoSize recordVideoSize = null;
            string recordVideoDir = null;

            if (_options != null && _options.EnableVideoRecording)
            {
                recordVideoSize = _options.RecordVideoSize;
                recordVideoDir = _options.RecordVideoDir;
            }

            BrowserTypeLaunchPersistentContextOptions launchOptions = new()
            {
                Args = _options.Args,
                Headless = _options.Headless,
                Timeout = _options.Timeout,
                Proxy = _options.Proxy,
                ViewportSize = ViewportSize.NoViewport,
                BypassCSP = true,
                RecordVideoDir = recordVideoDir,
                RecordVideoSize = recordVideoSize
            };

            if (_browserType == WebBrowserType.Chrome)
            {
                launchOptions.Channel = "chrome";
            }
            else if (_browserType == WebBrowserType.Edge)
            {
                launchOptions.Channel = "msedge";
            }

            return launchOptions;
        }

        private IPlaywrightBrowserType GetPlaywrightBrowserType()
        {
            return _browserType switch
            {
                WebBrowserType.Chrome or WebBrowserType.Edge => _playwright.Chromium,
                WebBrowserType.FireFox => _playwright.Firefox,
                _ => throw new InvalidOperationException(),
            };
        }

        public override async Task SetWindowAsync(IBrowserWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException(paramName: nameof(window));
            }

            IEnumerable<IBrowserWindow> windows = new List<IBrowserWindow>(_windows);

            IBrowserWindow? windowToSwitch = null;
            foreach (IBrowserWindow currentWindow in windows)
            {
                if (currentWindow == window)
                {
                    windowToSwitch = currentWindow;
                    break;
                }
            }

            if (windowToSwitch == null)
            {
                throw new ArgumentException($"No matching window found in the list of browser windows");
            }

            _currentWindow = windowToSwitch;
            await ((PlaywrightBrowserWindow)_currentWindow).BringToFrontAsync();
        }

        private Task OnWindowClose(IBrowserWindow closedWindow)
        {
            _windows.Remove(closedWindow);

            if (_windows.Count <= 0)
            {
                return ClosePlaywrightBrowser();
            }

            if (closedWindow == _currentWindow)
            {
                _currentWindow = _windows.Last!.Value;
            }

            return Task.CompletedTask;
        }

        public override Task CloseAsync()
        {
            return CloseAllWindows();
        }

        private Task CloseAllWindows()
        {
            Task[] windowCloseTasks = new Task[_windows.Count];
            List<IBrowserWindow> windowsToClose = new(_windows);
            for (int index = 0; index < windowsToClose.Count; index++)
            {
                windowCloseTasks[index] = windowsToClose[index].CloseAsync();
            }
            return Task.WhenAll(windowCloseTasks);
        }

        private async Task ClosePlaywrightBrowser()
        {
            if (_isClosed)
            {
                return;
            }

            _isClosed = true;

            _playwright.Dispose();
            if (_onBrowserClose != null)
            {
                await _onBrowserClose.Invoke(closedBrowser: this);
            }
        }

        private void ThrowIfClosed()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Cannot perform operation, browser is already closed.");
            }
        }
    }
}
