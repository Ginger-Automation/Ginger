﻿using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightBrowserType = Microsoft.Playwright.IBrowserType;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal class PlaywrightNonPersistentBrowser : PlaywrightBrowser
    {
        private readonly IPlaywrightBrowser _browser;
        private IBrowserWindow _currentWindow;
        
        public override IBrowserWindow CurrentWindow => _currentWindow;

        internal PlaywrightNonPersistentBrowser(IPlaywright playwright, WebBrowserType browserType, Options? options = null, IBrowser.OnBrowserClose? onBrowserClose = null) :
            base(playwright, browserType, options, onBrowserClose)
        {
            //Remove synchronous object creation if it is not possible. instead have a async Creator method
            _browser = Task.Run(async () =>
            {
                try
                {
                    return await LaunchBrowserWithInstallationAsync();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while creating {nameof(IBrowserWindow)}", ex);
                    return null!;
                }
            }).Result;

            if (_browser == null)
            {
                throw new Exception($"Error occurred while creating {nameof(IBrowserWindow)}");
            }
            
            if (_browser.Contexts.Count <= 0)
            {
                _currentWindow = Task.Run(async () => await NewWindowAsync()).Result;
            }
            else
            {
                _currentWindow = new PlaywrightBrowserWindow(_browser.Contexts[0], OnWindowClose);
                _windows.AddLast(_currentWindow);
            }
        }

        private async Task<IPlaywrightBrowser> LaunchBrowserWithInstallationAsync()
        {
            IPlaywrightBrowser browser;
            try
            {
                browser = await LaunchBrowserAsync();
            }
            catch (PlaywrightException ex)
            {
                if (ex.Message.Contains(BrowserExecutableNotFoundErrorMessage))
                {
                    ExecutePlaywrightInstallationCommand(_browserType);
                    browser = await LaunchBrowserAsync();
                }
                else
                {
                    throw;
                }
            }
            return browser;
        }

        private async Task<IPlaywrightBrowser> LaunchBrowserAsync()
        {
            IPlaywrightBrowserType playwrightBrowserType = GetPlaywrightBrowserType();
            BrowserTypeLaunchOptions? launchOptions = BuildBrowserTypeLaunchOptions();

            return await playwrightBrowserType.LaunchAsync(launchOptions);
        }
        private BrowserTypeLaunchOptions? BuildBrowserTypeLaunchOptions()
        {
            if (_options == null)
            {
                return null;
            }

            BrowserTypeLaunchOptions launchOptions = new()
            {
                Args = _options.Args,
                Headless = _options.Headless,
                Timeout = _options.Timeout,
                Proxy = _options.Proxy,
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

        public override async Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true)
        {
            ThrowIfClosed();

            IPlaywrightBrowserContext context = await _browser.NewContextAsync(new BrowserNewContextOptions()
            {
                ViewportSize = ViewportSize.NoViewport,
            });

            PlaywrightBrowserWindow window = new(context, OnWindowClose);
            _windows.AddLast(window);

            if (setAsCurrent)
            {
                _currentWindow = window;
            }

            return window;
        }

        private IPlaywrightBrowserType GetPlaywrightBrowserType()
        {
            switch (_browserType)
            {
                case WebBrowserType.Chrome:
                case WebBrowserType.Edge:
                    return _playwright.Chromium;
                case WebBrowserType.FireFox:
                    return _playwright.Firefox;
                default:
                    throw new InvalidOperationException();
            }
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

            await _browser.CloseAsync();
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