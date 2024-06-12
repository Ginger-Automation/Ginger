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
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver, IVirtualDriver, IIncompleteDriver
    {
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
            PlaywrightBrowser.Options browserOptions = BuildPlaywrightBrowserOptions();
            _browser = new(playwright, BrowserType, browserOptions, OnBrowserClose);
        }

        private void ValidateBrowserTypeSupport(WebBrowserType browserType)
        {
            IEnumerable<WebBrowserType> supportedBrowserTypes = GetSupportedBrowserTypes(Agent.eDriverType.Playwright);
            if (!supportedBrowserTypes.Contains(browserType))
            {
                throw new InvalidOperationException($"Browser type '{browserType}' is not supported for Playwright driver.");
            }
        }

        private PlaywrightBrowser.Options BuildPlaywrightBrowserOptions()
        {
            PlaywrightBrowser.Options options = new()
            {
                Args = new[] { "--start-maximized" },
                Headless = HeadlessBrowserMode,
                Timeout = DriverLoadWaitingTime * 1000, //convert to milliseconds
            };

            if (!string.IsNullOrEmpty(Proxy))
            {
                options.Proxy = new Proxy()
                {
                    Server = Proxy
                };
            }

            return options;
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

        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public override Act GetCurrentElement()
        {
            ThrowIfClosed();

            async Task<Act?> getCurrentElementAsync()
            {
                IBrowserElement? element = await ((PlaywrightBrowserTab)_browser!.CurrentWindow.CurrentTab).GetFocusedElement();
                if (element == null)
                {
                    return null;
                }

                string tagName = await element.TagNameAsync();
                Act? act = null;
                switch (tagName)
                {
                    case "text":
                        act = new ActTextBox()
                        {
                            TextBoxAction = ActTextBox.eTextBoxAction.SetValue,
                        };
                        await SetElementLocatorToActionAsync(element, act);
                        act.AddOrUpdateInputParamValue("Value", await element.AttributeValueAsync(name: "value"));
                        act.AddOrUpdateReturnParamActual("Actual", $"Tag Name = {tagName}");
                        break;
                    case "button":
                        string idAttrValue = await element.AttributeValueAsync(name: "id");
                        
                        act = new ActButton()
                        {
                            LocateBy = eLocateBy.ByID,
                            LocateValue = idAttrValue
                        };
                        break;
                    case "submit":
                        idAttrValue = await element.AttributeValueAsync(name: "id");

                        act = new ActButton()
                        {
                            LocateBy = eLocateBy.ByID,
                            LocateValue = idAttrValue
                        };
                        break;
                    case "reset":
                        //TODO: add missing Act get() method
                        break;
                    case "file":
                        //TODO: add missing Act get() method
                        break;
                    case "hidden": // does type this apply?
                                   //TODO: add missing Act get() method
                        break;
                    case "password":
                        act = new ActPassword()
                        {
                            PasswordAction = ActPassword.ePasswordAction.SetValue,
                        };
                        await SetElementLocatorToActionAsync(element, act);
                        act.AddOrUpdateInputParamValue("Value", await element.AttributeValueAsync(name: "value"));
                        act.AddOrUpdateReturnParamActual("Actual", $"Tag Name = {tagName}");
                        break;
                    case "checkbox":
                        idAttrValue = await element.AttributeValueAsync(name: "id");
                        act = new ActCheckbox()
                        {
                            LocateBy = eLocateBy.ByID,
                            LocateValue = idAttrValue,
                        };
                        break;
                    case "radio":
                        idAttrValue = await element.AttributeValueAsync(name: "id");
                        act = new ActRadioButton()
                        {
                            LocateBy = eLocateBy.ByID,
                            LocateValue = idAttrValue
                        };
                        break;

                }
                return act;
            }

            return Task.Run(getCurrentElementAsync).Result!;
            
        }

        private async Task SetElementLocatorToActionAsync(IBrowserElement element, Act act)
        {
            //order by priority

            // By ID
            string locatorValue = await element.AttributeValueAsync(name: "id");
            if (locatorValue != "")
            {
                act.LocateBy = eLocateBy.ByID;
                act.LocateValue = locatorValue;
                return;
            }

            // By name
            locatorValue = await element.AttributeValueAsync(name: "name");
            if (locatorValue != "")
            {
                act.LocateBy = eLocateBy.ByName;
                act.LocateValue = locatorValue;
                return;
            }

            //TODO: CSS....

            //By href
            locatorValue = await element.AttributeValueAsync(name: "href");
            if (locatorValue != "")
            {
                act.LocateBy = eLocateBy.ByHref;
                act.LocateValue = locatorValue;
                return;
            }

            //By Value
            locatorValue = await element.AttributeValueAsync(name: "value");
            if (locatorValue != "")
            {
                act.LocateBy = eLocateBy.ByValue;
                act.LocateValue = locatorValue;
                return;
            }

            // by text
            locatorValue = await element.TextContentAsync();
            if (locatorValue != "")
            {
                act.LocateBy = eLocateBy.ByLinkText;
                act.LocateValue = locatorValue;
                return;
            }
            //TODO: add XPath
        }

        public override string GetURL()
        {
            ThrowIfClosed();

            return Task.Run(() => _browser!.CurrentWindow.CurrentTab.GetURLAsync().Result).Result;
        }

        public override void HighlightActElement(Act act)
        {
            ThrowIfClosed();
            //TODO: implement
        }

        private void ThrowIfClosed()
        {
            if (!IsRunning())
            {
                throw new InvalidOperationException($"Cannot perform operation on closed driver.");
            }
        }
    }
}
