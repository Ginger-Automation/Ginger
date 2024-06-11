using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright.ActionHandlers;
using Amdocs.Ginger.CoreNET.RunLib;
using GingerCore;
using GingerCore.Actions;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using GingerCore.Actions.Common;
using System.Threading;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver, IVirtualDriver, IIncompleteDriver
    {
        private const string BrowserExecutableNotFoundErrorMessage = "Executable doesn't exist at";

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Only for Chrome & Firefox | Set \"true\" to run the browser in background (headless mode) for faster Execution")]
        public bool HeadlessBrowserMode { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Proxy Server:Port")]
        public string? Proxy { get; set; }

        private PlaywrightBrowser? _browser;

        public override void StartDriver()
        {
            ValidateBrowserTypeSupport(BrowserType);

            IPlaywright playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;

            BrowserTypeLaunchOptions launchOptions = BuildLaunchOptions();
            IPlaywrightBrowser playwrightBrowser = LaunchBrowserWithInstallationAsync(playwright, BrowserType, launchOptions).Result;

            _browser = new(playwrightBrowser, OnBrowserClose);
        }

        private void ValidateBrowserTypeSupport(WebBrowserType browserType)
        {
            IEnumerable<WebBrowserType> supportedBrowserTypes = GetSupportedBrowserTypes(Agent.eDriverType.Playwright);
            if (!supportedBrowserTypes.Contains(browserType))
            {
                throw new InvalidOperationException($"Browser type '{browserType}' is not supported for Playwright driver.");
            }
        }

        private BrowserTypeLaunchOptions BuildLaunchOptions()
        {
            BrowserTypeLaunchOptions launchOptions = new()
            {
                Args = new[] { "--start-maximized" },
                Headless = HeadlessBrowserMode,
                Timeout = DriverLoadWaitingTime * 1000, //convert to milliseconds
            };

            if (!string.IsNullOrEmpty(Proxy))
            {
                launchOptions.Proxy = new Proxy()
                {
                    Server = Proxy
                };
            }

            if (BrowserType == WebBrowserType.Chrome)
            {
                launchOptions.Channel = "chrome";
            }
            else if (BrowserType == WebBrowserType.Edge)
            {
                launchOptions.Channel = "msedge";
            }

            return launchOptions;
        }

        private async Task<IPlaywrightBrowser> LaunchBrowserWithInstallationAsync(IPlaywright playwright, WebBrowserType browserType, BrowserTypeLaunchOptions? launchOptions = null)
        {
            IPlaywrightBrowser browser;
            try
            {
                browser = await LaunchBrowserAsync(playwright, browserType, launchOptions);
            }
            catch (PlaywrightException ex)
            {
                if (ex.Message.Contains(BrowserExecutableNotFoundErrorMessage))
                {
                    ExecutePlaywrightInstallationCommand(browserType);
                    browser = await LaunchBrowserAsync(playwright, browserType, launchOptions);
                }
                else
                {
                    throw;
                }
            }
            return browser;
        }

        private async Task<IPlaywrightBrowser> LaunchBrowserAsync(IPlaywright playwright, WebBrowserType browserType, BrowserTypeLaunchOptions? launchOptions = null)
        {
            IPlaywrightBrowser browser;
            if (browserType == WebBrowserType.Chrome)
            {
                browser = await playwright.Chromium.LaunchAsync(launchOptions);
            }
            else if (browserType == WebBrowserType.FireFox)
            {
                browser = await playwright.Firefox.LaunchAsync(launchOptions);
            }
            else if (browserType == WebBrowserType.Edge)
            {
                browser = await playwright.Chromium.LaunchAsync(launchOptions);
            }
            else
            {
                throw new ArgumentException($"Unknown browser type '{BrowserType}'");
            }
            return browser;
        }

        private static void ExecutePlaywrightInstallationCommand(WebBrowserType browserType)
        {
            string browserTypeString;
            switch (browserType)
            {
                case WebBrowserType.Chrome:
                case WebBrowserType.Edge:
                    browserTypeString = Microsoft.Playwright.BrowserType.Chromium;
                    break;
                case WebBrowserType.FireFox:
                    browserTypeString = Microsoft.Playwright.BrowserType.Firefox;
                    break;
                default:
                    throw new ArgumentException($"Unknown browser type '{browserType}'");
            }

            int exitCode = Program.Main(new[] { "install", browserTypeString });
            if (exitCode != 0)
            {
                throw new Exception($"Error occurred while executing playwright installation command, exited with code {exitCode}");
            }
        }

        public override bool IsRunning()
        {
            if (_browser == null)
            {
                return false;
            }

            if (_browser.IsClosed)
            {
                _browser = null;
                return false;
            }

            return true;
        }

        public override void CloseDriver()
        {
            if (!IsRunning())
            {
                return;
            }

            _browser?.CloseAsync().Wait();
            _browser = null;
        }

        private Task OnBrowserClose(IBrowser closedBrowser)
        {
            if (closedBrowser == _browser)
            {
                _browser = null;
            }

            return Task.CompletedTask;
        }

        public override Act GetCurrentElement()
        {
            throw new NotImplementedException();
        }

        public override string GetURL()
        {
            throw new NotImplementedException();
        }

        public override void HighlightActElement(Act act)
        {
            throw new NotImplementedException();
        }

        public override void RunAction(Act act)
        {
            if (!IsRunning())
            {
                throw new InvalidOperationException("cannot run action when driver is not started");
            }

            switch (act)
            {
                case ActBrowserElement actBrowserElement:
                    ActBrowserElementHandler actBrowserElementHandler = new(actBrowserElement, _browser!, new ActBrowserElementHandler.Context
                    {
                        BusinessFlow = BusinessFlow,
                        Environment = Environment,
                    });
                    actBrowserElementHandler.HandleAsync().Wait();
                    break;
                case ActUIElement actUIElement:
                    ActUIElementHandler actUIElementHandler = new(actUIElement, _browser!);
                    actUIElementHandler.HandleAsync().Wait();
                    break;
                default:
                    act.Error = $"Run Action Failed due to unrecognized action type - {act.GetType().Name}";
                    break;
            }
        }

        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public bool IsActionSupported(Act act, out string message)
        {
            if (act is ActWithoutDriver)
            {
                message = string.Empty;
                return true;
            }
            if (act is ActUIElement actUIElement)
            {
                bool isSupported = ActUIElementHandler.IsOperationSupported(actUIElement.ElementAction);
                if (!isSupported)
                {
                    string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActBrowserElement.eControlAction), actUIElement.ElementAction);
                    message = $"'{act.ActionType} - {operationName}' is not supported by Playwright driver, use Selenium driver instead.";
                }
                else
                {
                    message = string.Empty;
                }
                return isSupported;
            }
            else if (act is ActBrowserElement actBrowserElement)
            {
                bool isSupported = ActBrowserElementHandler.IsOperationSupported(actBrowserElement.ControlAction);
                if (!isSupported)
                {
                    string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActBrowserElement.eControlAction), actBrowserElement.ControlAction);
                    message = $"'{act.ActionType} - {operationName}' is not supported by Playwright driver, use Selenium driver instead.";
                }
                else
                {
                    message = string.Empty;
                }
                return isSupported;
            }
            else
            {
                message = $"'{act.ActionType}' is not supported by Playwright driver, use Selenium driver instead.";
                return false;
            }
        }
    }
}
