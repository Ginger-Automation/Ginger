#region License
/*
Copyright © 2014-2024 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using Microsoft.Playwright;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver, IVirtualDriver, IIncompleteDriver, IWindowExplorer, IXPath, IVisualTestingDriver
    {

        private PlaywrightBrowser? _browser;
        private IBrowserElement? _lastHighlightedElement;

        /// <summary>
        /// Gets the name of the driver configuration edit page based on the driver subtype and driver configuration parameters.
        /// </summary>
        /// <param name="driverSubType">The driver subtype.</param>
        /// <param name="driverConfigParams">The driver configuration parameters.</param>
        /// <returns>The name of the driver configuration edit page if the browser type is Chrome, Edge, or FireFox; otherwise, null.</returns>
        public override string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA, IEnumerable<DriverConfigParam> driverConfigParams = null)
        {
            if (driverConfigParams == null)
            {
                return null;
            }
            DriverConfigParam browserTypeParam = driverConfigParams.FirstOrDefault(param => string.Equals(param.Parameter, nameof(BrowserType)));

            if (browserTypeParam == null || !Enum.TryParse(browserTypeParam.Value, out WebBrowserType browserType))
            {
                return null;
            }
            else if (browserType is WebBrowserType.Chrome or WebBrowserType.Edge or WebBrowserType.FireFox)
            {
                return "WebAgentConfigEditPage";
            }
            else
            {
                return null;
            }
        }
        PlaywrightBrowser.Options browserOptions;
        public override void StartDriver()
        {
            ValidateBrowserTypeSupport(BrowserType);

            IPlaywright playwright = Microsoft.Playwright.Playwright.CreateAsync().Result;
            browserOptions = BuildPlaywrightBrowserOptions();
            if (BrowserPrivateMode)
            {
                _browser = new PlaywrightNonPersistentBrowser(playwright, BrowserType, browserOptions, OnBrowserClose);
            }
            else
            {
                _browser = new PlaywrightPersistentBrowser(playwright, BrowserType, browserOptions, OnBrowserClose);
            }
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
                    Server = Proxy,
                };

                if (!string.IsNullOrEmpty(ByPassProxy))
                {
                    options.Proxy.Bypass = string.Join(',', ByPassProxy.Split(';'));
                }
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
            ThrowIfClosed();

            Task.Run(() =>
            {
                switch (act)
                {
                    case ActBrowserElement actBrowserElement:
                        ActBrowserElementHandler actBrowserElementHandler = new(
                            actBrowserElement,
                            _browser,
                            new ActBrowserElementHandler.Context
                            {
                                BusinessFlow = BusinessFlow,
                                Environment = Environment,
                            });
                        actBrowserElementHandler.HandleAsync().Wait();
                        break;
                    case ActUIElement actUIElement:
                        ActUIElementHandler actUIElementHandler = new(
                            actUIElement,
                            _browser.CurrentWindow.CurrentTab,
                            new BrowserElementLocator(
                                _browser.CurrentWindow.CurrentTab,
                                new()
                                {
                                    BusinessFlow = BusinessFlow,
                                    Environment = Environment,
                                    POMExecutionUtils = new POMExecutionUtils(act, actUIElement.ElementLocateValue),
                                    Agent = BusinessFlow.CurrentActivity.CurrentAgent,
                                }));
                        actUIElementHandler.HandleAsync().Wait();
                        break;
                    case ActScreenShot actScreenShot:
                        ActScreenShotHandler actScreenShotHandler = new(actScreenShot, _browser);
                        actScreenShotHandler.HandleAsync().Wait();
                        break;
                    case ActGotoURL actGotoURL:
                        ActGotoURLHandler actGotoURLHandler = new(actGotoURL, _browser);
                        actGotoURLHandler.HandleAsync().Wait();
                        break;
                    case ActVisualTesting actVisualTesting:
                        if (actVisualTesting.VisualTestingAnalyzer != ActVisualTesting.eVisualTestingAnalyzer.Applitools)
                        {
                            actVisualTesting.Execute(this);
                        }
                        else
                        {
                            act.Error = $"{actVisualTesting.VisualTestingAnalyzer} is not supported by Playwright driver, use Selenium driver instead.";
                        }
                        break;
                    case ActAccessibilityTesting actAccessibilityTesting:
                        if (!BrowserPrivateMode)
                        {
                            act.Error = $"Playwright Driver must be in Private mode for using Accessibility actions";
                            break;
                        }
                        ActAccessibilityTestingHandler actAccessibilityTestingHandler;
                        if (actAccessibilityTesting.GetAccessibilityTarget() == ActAccessibilityTesting.eTarget.Element)
                        {
                            actAccessibilityTestingHandler = new(
                            actAccessibilityTesting,
                            _browser.CurrentWindow.CurrentTab,
                            new BrowserElementLocator(
                                _browser.CurrentWindow.CurrentTab,
                                new BrowserElementLocator.Context()
                                {
                                    BusinessFlow = BusinessFlow,
                                    Environment = Environment,
                                    POMExecutionUtils = new(actAccessibilityTesting, actAccessibilityTesting.LocateValueCalculated)
                                }));
                        }
                        else
                        {
                            actAccessibilityTestingHandler = new(
                            actAccessibilityTesting,
                            _browser.CurrentWindow.CurrentTab,
                            browserElementLocator: null);
                        }
                        actAccessibilityTestingHandler.HandleAsync().Wait();
                        break;
                    case ActSmartSync actSmartSync:
                        ActSmartSyncHandler actSmartSyncHandler = new(
                            actSmartSync,
                            _browser.CurrentWindow.CurrentTab,
                            new BrowserElementLocator(
                                _browser.CurrentWindow.CurrentTab,
                                new()
                                {
                                    BusinessFlow = BusinessFlow,
                                    Environment = Environment,
                                    POMExecutionUtils = new POMExecutionUtils(act, act.LocateValue),
                                    Agent = BusinessFlow.CurrentActivity.CurrentAgent,
                                }));
                        float? driverDefaultTimeout = browserOptions.Timeout;
                        try
                        {
                            int smartSyncTimeout = DriverBase.GetMaxTimeout(actSmartSync) * 1000;
                            browserOptions.Timeout = smartSyncTimeout;
                            actSmartSyncHandler.HandleAsync(act, smartSyncTimeout).Wait();
                        }
                        catch (Exception ex)
                        {
                            act.Error = ex.Message;
                        }
                        finally
                        {
                            browserOptions.Timeout = driverDefaultTimeout;
                        }
                        break;
                    case ActWebSmartSync actWebSmartSync:
                        ActWebSmartSyncHandler actWebSmartSyncHandler = new(
                            actWebSmartSync,
                            _browser.CurrentWindow.CurrentTab,
                            new BrowserElementLocator(
                                _browser.CurrentWindow.CurrentTab,
                                new()
                                {
                                    BusinessFlow = BusinessFlow,
                                    Environment = Environment,
                                    POMExecutionUtils = new POMExecutionUtils(actWebSmartSync, actWebSmartSync.ElementLocateValue),
                                    Agent = BusinessFlow.CurrentActivity.CurrentAgent,
                                }));
                        float? driverDefaultTimeout1 = browserOptions.Timeout;
                        float waitUntilTime;
                        if (act.Timeout > 0)
                        {
                            waitUntilTime = act.Timeout.GetValueOrDefault();
                        }
                        else if(browserOptions.Timeout>0)
                        {
                            waitUntilTime = browserOptions.Timeout.Value;
                        }
                        else
                        {
                            waitUntilTime = 5;
                        }
                        browserOptions.Timeout = waitUntilTime;
                        try
                        {                          
                            actWebSmartSyncHandler.HandleAsync(act, waitUntilTime*1000).Wait();
                        }
                        catch (Exception ex)
                        {
                            act.Error = ex.Message;
                        }
                        finally
                        {
                            browserOptions.Timeout = driverDefaultTimeout1;
                        }
                        break;
                    default:
                        act.Error = $"This Action is not supported for Playwright driver";
                        break;
                }
            }).Wait();
        }

        public bool IsActionSupported(Act act, out string message)
        {
            message = string.Empty;

            if (act is ActWithoutDriver or ActScreenShot or ActGotoURL or ActAccessibilityTesting or ActSmartSync or ActWebSmartSync or ActBrowserElement)
            {
                return true;
            }
            if (act is ActUIElement actUIElement)
            {
                bool isLocatorSupported = PlaywrightBrowserTab.IsElementLocatorSupported(actUIElement.ElementLocateBy);
                if (!isLocatorSupported)
                {
                    message = $"Element Locator '{actUIElement.ElementLocateBy}' is not supported by Playwright driver, use Selenium driver instead.";
                }

                bool isOperationSupported = ActUIElementSupportedOperations.Contains(actUIElement.ElementAction);
                if (!isOperationSupported)
                {
                    string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActBrowserElement.eControlAction), actUIElement.ElementAction);
                    if (!string.IsNullOrEmpty(message))
                    {
                        message += '\n';
                    }
                    message += $"'{act.ActionType} - {operationName}' is not supported by Playwright driver, use Selenium driver instead.";
                }

                return isLocatorSupported && isOperationSupported;
            }
            else if (act is ActBrowserElement actBrowserElement)
            {
                bool isLocatorSupported =
                    actBrowserElement.ControlAction != ActBrowserElement.eControlAction.SwitchFrame ||
                    PlaywrightBrowserTab.IsFrameLocatorSupported(act.LocateBy);
                if (!isLocatorSupported)
                {
                    message = $"Frame Locator '{act.LocateBy}' is not supported by Playwright driver, use Selenium driver instead.";
                }

                bool isOperationSupported = ActBrowserElementSupportedOperations.Contains(actBrowserElement.ControlAction);
                if (!isOperationSupported)
                {
                    string operationName = Common.GeneralLib.General.GetEnumValueDescription(typeof(ActBrowserElement.eControlAction), actBrowserElement.ControlAction);
                    if (!string.IsNullOrEmpty(message))
                    {
                        message += '\n';
                    }
                    message += $"'{act.ActionType} - {operationName}' is not supported by Playwright driver, use Selenium driver instead.";
                }
                return isLocatorSupported && isOperationSupported;
            }
            else if (act is ActVisualTesting actVisualTesting)
            {
                if (actVisualTesting.VisualTestingAnalyzer == ActVisualTesting.eVisualTestingAnalyzer.Applitools)
                {
                    message = $"{actVisualTesting.VisualTestingAnalyzer} is not supported by Playwright driver, use Selenium driver instead.";
                    return false;
                }
                return true;
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

            return Task.Run(() => _browser!.CurrentWindow.CurrentTab.URLAsync().Result).Result;
        }

        public override void HighlightActElement(Act act)
        {
            ThrowIfClosed();
            //TODO: implement
        }

        [MemberNotNull(nameof(_browser))]
#pragma warning disable CS8774
        private void ThrowIfClosed()
        {
            if (!IsRunning())
            {
                throw new InvalidOperationException($"Cannot perform operation on closed driver.");
            }
        }
#pragma warning restore CS8774

        [SupportedOSPlatform("windows")]
        public Bitmap? GetScreenShot(Tuple<int, int>? size = null, bool fullPage = false)
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                IBrowserTab tab = _browser!.CurrentWindow.CurrentTab;

                if (size != null)
                {
                    await tab.SetViewportSizeAsync(new Size(width: size.Item1, height: size.Item2));
                }

                byte[] screenshot;
                if (fullPage)
                {
                    screenshot = await tab.ScreenshotAsync();
                }
                else
                {
                    screenshot = await tab.ScreenshotFullPageAsync();
                }

                return BytesToBitmap(screenshot);
            }).Result;
        }

        [SupportedOSPlatform("windows")]
        public Bitmap? GetElementScreenshot(Act act)
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                eLocateBy locateBy;
                string locateValue;
                if (act is ActUIElement actUIElement)
                {
                    locateBy = actUIElement.ElementLocateBy;
                    locateValue = actUIElement.ElementLocateValueForDriver;
                }
                else
                {
                    locateBy = act.LocateBy;
                    locateValue = act.LocateValueCalculated;
                }

                IBrowserElement? element =
                    (await _browser!
                    .CurrentWindow
                    .CurrentTab
                    .GetElementsAsync(locateBy, locateValue))
                    .FirstOrDefault();

                if (element == null)
                {
                    return null;
                }

                byte[] screenshot = await element.ScreenshotAsync();
                return BytesToBitmap(screenshot);
            }).Result;
        }

        [SupportedOSPlatform("windows")]
        private static Bitmap BytesToBitmap(byte[] bytes)
        {
            using MemoryStream memoryStream = new(bytes);
            return new Bitmap(memoryStream);
        }

        private protected override IBrowser GetBrowser()
        {
            ThrowIfClosed();
            return _browser;
        }

        public List<AppWindow> GetAppWindows()
        {
            //TODO: unhighlight last highlighted element
            IEnumerable<AppWindow> appWindows = Task.Run(() => GetAppWindowsAsync().Result).Result;
            return new List<AppWindow>(appWindows);
        }

        public void SwitchWindow(string tabTitle)
        {
            Task.Run(async () =>
            {
                await SwitchTabAsync(filter: async (_, tab) =>
                {
                    string title = await tab.TitleAsync();
                    return string.Equals(title, tabTitle);
                });
            }).Wait();
        }

        public void HighLightElement(ElementInfo element, bool locateElementByItLocators = false, IList<ElementInfo>? MappedUIElements = null)
        {
            Task.Run(async () =>
            {
                if (_lastHighlightedElement != null)
                {
                    await UnhighlightElementAsync(_lastHighlightedElement);
                }
                await SwitchToFrameOfElementAsync(element);
                IBrowserElement? browserElement = null;
                if (element.ElementObject is IBrowserElement)
                {
                    browserElement = (IBrowserElement)element.ElementObject;
                }
                if (browserElement == null)
                {
                    browserElement = await FindBrowserElementAsync(element);
                }
                if (browserElement == null)
                {
                    return;
                }
                _lastHighlightedElement = browserElement;
                await HighlightElementAsync(browserElement);
            }).Wait();
        }

        private protected override async Task<IBrowserElement?> FindBrowserElementAsync(eLocateBy locateBy, string locateValue)
        {
            ThrowIfClosed();
            return (await _browser.CurrentWindow.CurrentTab.GetElementsAsync(locateBy, locateValue)).FirstOrDefault();
        }

        public void UnHighLightElements()
        {
            Task.Run(async () =>
            {
                if (_lastHighlightedElement != null)
                {
                    await UnhighlightElementAsync(_lastHighlightedElement);
                }
            }).Wait();
        }

        public string GetFocusedControl()
        {
            //same implementation in SeleniumDriver so just copying, don't know why?
            return null!;
        }

        public ElementInfo GetControlFromMousePosition()
        {
            ThrowIfClosed();

            return Task.Run(async () =>
            {
                IBrowserTab currentTab = _browser.CurrentWindow.CurrentTab;
                await currentTab.SwitchToParentFrameAsync();
                if (_lastHighlightedElement != null)
                {
                    await UnhighlightElementAsync(_lastHighlightedElement);
                }

                await InjectLiveSpyScriptAsync(currentTab);

                IBrowserElement? browserElement = null;
                try
                {
                    browserElement = await currentTab.GetElementAsync("GingerLibLiveSpy.ElementFromPoint();");
                }
                catch (Exception ex)
                {
                    //when we spy the element for the first time, it throws exception because X,Y point of mouse position is undefined for some reason
                    Reporter.ToLog(eLogLevel.DEBUG, "Failed to get element from point - this is expected on first spy attempt", ex);
                }
                if (browserElement == null)
                {
                    return null!;
                }

                string? screenshot = null;
                try
                {
                    screenshot = Convert.ToBase64String(await browserElement.ScreenshotAsync());
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "error while taking element screenshot", ex);
                }

                string tag = await browserElement.TagNameAsync();
                string xPath = string.Empty;
                if (!string.IsNullOrEmpty(tag) && (string.Equals(tag, "iframe", StringComparison.InvariantCultureIgnoreCase) || string.Equals(tag, "frame", StringComparison.InvariantCultureIgnoreCase)))
                {
                    xPath = await GenerateXPathFromBrowserElementAsync(browserElement);
                    //TODO: create HTMLElementInfo specific for IFrame
                }

                HTMLElementInfo foundElemntInfo = new HTMLElementInfo()
                {
                    ElementObject = browserElement,
                    ScreenShotImage = screenshot,
                    XPath = xPath,
                };

                if (!string.IsNullOrEmpty(tag) && (string.Equals(tag, "iframe", StringComparison.InvariantCultureIgnoreCase) || string.Equals(tag, "frame", StringComparison.InvariantCultureIgnoreCase)))
                {
                    foundElemntInfo.Path = xPath;
                    foundElemntInfo.XPath = xPath;
                    return await GetElementFromIframeAsync(foundElemntInfo, currentTab);
                }
                return foundElemntInfo;

            }).Result;
        }

        private async Task<ElementInfo> GetElementFromIframeAsync(ElementInfo IframeElementInfo, IBrowserTab currentTab)
        {
            await SwitchToFrameOfElementAsync(IframeElementInfo);
            return Task.Run(async () =>
            {
                IBrowserTab IframecurrentTab = _browser.CurrentWindow.CurrentTab;
                await InjectLiveSpyScriptAsync(IframecurrentTab, true);
                IBrowserElement? browserElement = null;
                try
                {
                    browserElement = await currentTab.GetElementAsync("GingerLibLiveSpy.ElementFromPoint();");
                }
                catch (Exception ex)
                {
                    //when we spy the element for the first time, it throws exception because X,Y point of mouse position is undefined for some reason
                    Reporter.ToLog(eLogLevel.DEBUG, "Failed to get element from point - this is expected on first spy attempt", ex);
                }
                if (browserElement == null)
                {
                    return null!;
                }

                string? screenshot = null;
                try
                {
                    screenshot = Convert.ToBase64String(await browserElement.ScreenshotAsync());
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "error while taking element screenshot", ex);
                }

                string tag = await browserElement.TagNameAsync();
                string xPath = string.Empty;
                if (!string.IsNullOrEmpty(tag) && (string.Equals(tag, "iframe", StringComparison.InvariantCultureIgnoreCase) || string.Equals(tag, "frame", StringComparison.InvariantCultureIgnoreCase)))
                {
                    xPath = await GenerateXPathFromBrowserElementAsync(browserElement);
                    //TODO: create HTMLElementInfo specific for IFrame
                }

                HTMLElementInfo foundElemntInfo = new HTMLElementInfo()
                {
                    ElementObject = browserElement,
                    ScreenShotImage = screenshot,
                    XPath = xPath,
                };

                if (!string.IsNullOrEmpty(tag) && (string.Equals(tag, "iframe", StringComparison.InvariantCultureIgnoreCase) || string.Equals(tag, "frame", StringComparison.InvariantCultureIgnoreCase)))
                {
                    foundElemntInfo.Path = xPath;
                    foundElemntInfo.XPath = xPath;
                    return await GetElementFromIframeAsync(foundElemntInfo, currentTab);
                }
                return foundElemntInfo;
            }).Result;
        }
        public AppWindow GetActiveWindow()
        {
            ThrowIfClosed();
            return Task.Run(() => GetActiveWindowAsync().Result).Result;
        }

        public async Task<List<ElementInfo>> GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo>? foundElementsList = null, ObservableList<POMPageMetaData>? PomMetaData = null)
        {
            ThrowIfClosed();

            if (_lastHighlightedElement != null)
            {
                await UnhighlightElementAsync(_lastHighlightedElement);
            }

            IBrowserTab currentTab = _browser.CurrentWindow.CurrentTab;

            await currentTab.SwitchToMainFrameAsync();
            string pageSource = await _browser.CurrentWindow.CurrentTab.PageSourceAsync();

            if (foundElementsList == null)
            {
                foundElementsList = [];
            }

            if (_browser.CurrentWindow.CurrentTab is PlaywrightBrowserTab playwrightBrowserTab)
            {
                await playwrightBrowserTab.BringToFrontAsync();
            }

            POMLearner pomLearner = POMLearner.Create(pageSource, new PlaywrightBrowserElementProvider(currentTab), pomSetting, xpathImpl: this);
            CancellationTokenSource cancellationTokenSource = new();
            Task learnElementsTask = pomLearner.LearnElementsAsync(foundElementsList, cancellationTokenSource.Token);
            _ = Task.Run(() =>
            {
                while (!StopProcess && !learnElementsTask.IsCompleted) ;

                if (StopProcess)
                {
                    cancellationTokenSource.Cancel();
                }
            });
            await learnElementsTask;

            //below part should ideally be handled in POMLearner itself but, when we add the learned element to the observable list, it sets the active status as true again
            foreach (ElementInfo element in foundElementsList)
            {
                HTMLElementInfo htmlElementInfo = (HTMLElementInfo)element;
                if (htmlElementInfo.FriendlyLocators.Count == 0 && htmlElementInfo.Locators.Count >= 1)
                {
                    ElementLocator? byTagNameLocator = htmlElementInfo.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByTagName);
                    if (byTagNameLocator != null)
                    {
                        byTagNameLocator.Active = false;
                    }
                }
            }

            return new(foundElementsList);
        }

        private sealed class PlaywrightBrowserElementProvider : POMLearner.IBrowserElementProvider
        {
            private readonly IBrowserTab _browserTab;
            private int _shadowDOMDepth = 0;

            internal PlaywrightBrowserElementProvider(IBrowserTab browserTab)
            {
                _browserTab = browserTab;
            }

            public async Task<IBrowserElement?> GetElementAsync(eLocateBy locateBy, string locateValue)
            {
                try
                {
                    if (_shadowDOMDepth > 0 && locateBy == eLocateBy.ByXPath)
                    {
                        string cssLocateValue = new ShadowDOM().ConvertXPathToCssSelector(locateValue);
                        return (await _browserTab.GetElementsAsync(eLocateBy.ByCSS, cssLocateValue)).FirstOrDefault();
                    }
                    return (await _browserTab.GetElementsAsync(locateBy, locateValue)).FirstOrDefault();
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public async Task OnFrameEnterAsync(HTMLElementInfo frameElement)
            {
                if (!string.IsNullOrEmpty(frameElement.XPath))
                {
                    bool wasSwitched = await _browserTab.SwitchFrameAsync(eLocateBy.ByXPath, frameElement.XPath);

                    if (!wasSwitched)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"unable to switch to frame with xpath {frameElement.XPath}");
                    }
                }
            }

            public Task OnFrameExitAsync(HTMLElementInfo frameElement)
            {
                return _browserTab.SwitchToParentFrameAsync();
            }

            public Task OnShadowDOMEnterAsync(HTMLElementInfo shadowHostElement)
            {
                _shadowDOMDepth++;
                return Task.CompletedTask;
            }

            public Task OnShadowDOMExitAsync(HTMLElementInfo shadowHostElement)
            {
                _shadowDOMDepth--;
                return Task.CompletedTask;
            }
        }

        public List<ElementInfo> GetElementChildren(ElementInfo elementInfo)
        {
            ThrowIfClosed();
            if (elementInfo == null || elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return [];
            }

            return Task.Run(async () =>
            {
                await SwitchToFrameOfElementAsync(elementInfo);
                string xpath = GenerateXPathFromHTMLElementInfo(htmlElementInfo);
                string childrenXPath = GenerateChildrenXPath(xpath);
                IEnumerable<IBrowserElement> browserElements = await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByXPath, childrenXPath);
                List<HTMLElementInfo> htmlElements = [];
                foreach (IBrowserElement browserElement in browserElements)
                {
                    HTMLElementInfo newHtmlElement = await CreateHtmlElementAsync(browserElement);

                    if (string.IsNullOrEmpty(newHtmlElement.ID) && htmlElementInfo.HTMLElementObject != null)
                    {
                        newHtmlElement.ID = htmlElementInfo.HTMLElementObject.Id;
                    }

                    string elementPath = htmlElementInfo.Path;
                    if (string.IsNullOrEmpty(htmlElementInfo.XPath))
                    {
                        string[] xpathSegments = htmlElementInfo.XPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                        if (xpathSegments.Length > 0)
                        {
                            string lastXPathSegment = xpathSegments[^1].TrimStart('[').TrimEnd(']');
                            if (string.Equals(lastXPathSegment, "frame", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(lastXPathSegment, "iframe", StringComparison.OrdinalIgnoreCase))
                            {
                                elementPath = $"{htmlElementInfo.Path},{htmlElementInfo.XPath}";
                            }
                            elementPath = elementPath.TrimStart(',');
                        }
                    }
                    newHtmlElement.Path = elementPath;
                    newHtmlElement.WindowExplorer = this;
                    newHtmlElement.RelXpath = POMLearner.GenerateRelativeXPathFromHTMLElementInfo(newHtmlElement, xpathImpl: this, pomSetting: null);

                    htmlElements.Add(newHtmlElement);
                }
                return htmlElements.Cast<ElementInfo>().ToList();
            }).Result;
        }

        private string GenerateChildrenXPath(string parentXPath)
        {
            string[] parentXPathSegments = parentXPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
            string elementType = parentXPathSegments[^1];

            int index = elementType.IndexOf('[');
            if (index != -1)
            {
                elementType = elementType.AsSpan(0, index).ToString();
            }

            if (string.Equals(elementType, "iframe") || string.Equals(elementType, "frame"))
            {
                return "/html/*";
            }
            else
            {
                return parentXPath + "/*";
            }
        }

        public ObservableList<ControlProperty> GetElementProperties(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return [];
            }

            if (htmlElementInfo.ElementObject == null)
            {
                htmlElementInfo.ElementObject = Task.Run(async () => await FindBrowserElementAsync(htmlElementInfo)).Result;
            }

            if (htmlElementInfo.OptionalValuesObjectsList == null)
            {
                htmlElementInfo.OptionalValuesObjectsList = [];
            }
            htmlElementInfo.OptionalValuesObjectsList.AddRange(Task.Run(() => POMLearner.GetOptionalValuesAsync(htmlElementInfo).Result).Result);

            return new(Task.Run(() => POMLearner.GetPropertiesAsync(htmlElementInfo).Result).Result);
        }

        public ObservableList<ElementLocator> GetElementLocators(ElementInfo elementInfo, PomSetting? pomSetting = null)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return [];
            }

            IEnumerable<ElementLocator> locators = Task.Run(() => POMLearner.GenerateLocatorsAsync(htmlElementInfo, pomSetting).Result).Result;
            return new(locators);
        }

        public ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting? pomSetting = null)
        {
            return [];
        }

        public ObservableList<OptionalValue> GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            //copying existing implementation from SeleniumDriver
            return [];
        }

        public object? GetElementData(ElementInfo elementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }
            if (htmlElementInfo.ElementObject is not IBrowserElement browserElement)
            {
                return null;
            }

            return null;
        }

        public bool IsRecordingSupported()
        {
            return false;
        }

        public bool IsPOMSupported()
        {
            return true;
        }

        public bool IsLiveSpySupported()
        {
            return true;
        }

        public bool IsWinowSelectionRequired()
        {
            return true;
        }

        public List<eTabView> SupportedViews()
        {
            return [eTabView.Screenshot, eTabView.GridView, eTabView.PageSource, eTabView.TreeView];
        }

        public eTabView DefaultView()
        {
            return eTabView.TreeView;
        }

        public string SelectionWindowText()
        {
            return "Page:";
        }

        public ObservableList<ElementInfo> GetElements(ElementLocator EL)
        {
            //copying existing implementation from SeleniumDriver
            throw new Exception("Not implemented yet for this driver");
        }

        public void UpdateElementInfoFields(ElementInfo elementInfo)
        {
            ThrowIfClosed();

            Task.Run(async () =>
            {
                if (elementInfo == null)
                {
                    return;
                }

                if (elementInfo is not HTMLElementInfo htmlElementInfo)
                {
                    return;
                }

                IBrowserElement? browserElement = (IBrowserElement)elementInfo.ElementObject;
                if (browserElement == null)
                {
                    return;
                }

                if (string.IsNullOrEmpty(elementInfo.XPath))
                {
                    elementInfo.XPath = GenerateXPathFromHTMLElementInfo(htmlElementInfo);
                }

                Point position = await browserElement.PositionAsync();
                Size size = await browserElement.SizeAsync();

                elementInfo.X = position.X;
                elementInfo.Y = position.Y;
                elementInfo.Width = size.Width;
                elementInfo.Height = size.Height;

            }).Wait();
        }

        public bool IsElementObjectValid(object obj)
        {
            //copying from SeleniumDriver, don't know why we return true
            return true;
        }

        public bool TestElementLocators(ElementInfo elementInfo, bool GetOutAfterFoundElement = false, ApplicationPOMModel? mPOM = null)
        {
            return Task.Run(() => TestElementLocatorsAsync(elementInfo, tillFirstPassed: GetOutAfterFoundElement).Result).Result;
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            foreach (ElementInfo elementInfo in originalList)
            {
                elementInfo.ElementStatus = ElementInfo.eElementStatus.Pending;
            }

            Task.Run(async () =>
            {
                foreach (ElementInfo elementInfo in originalList)
                {
                    await SwitchToFrameOfElementAsync(elementInfo);
                    IBrowserElement? browserElement = await FindBrowserElementAsync(elementInfo);
                    if (browserElement != null)
                    {
                        elementInfo.ElementObject = browserElement;
                        elementInfo.ElementStatus = ElementInfo.eElementStatus.Passed;
                    }
                    else
                    {
                        elementInfo.ElementStatus = ElementInfo.eElementStatus.Failed;
                    }
                }
            }).Wait();
        }

        public ElementInfo? GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            if (latestElement == null)
            {
                return null;
            }

            return originalElements
                .Where(original => original.ElementObject != null)
                .Where(original => original.ElementTypeEnum == latestElement.ElementTypeEnum)
                .Where(original =>
                {
                    original.Path ??= string.Empty;
                    latestElement.Path ??= string.Empty;
                    return string.Equals(original.Path, latestElement.Path);
                })
                .Where(original =>
                {
                    ElementLocator? originalByRelXPathLocator = original.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath);
                    if (originalByRelXPathLocator == null)
                    {
                        return false;
                    }

                    ElementLocator? latestByRelXPathLocator = latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath);
                    if (latestByRelXPathLocator == null)
                    {
                        return false;
                    }

                    return string.Equals(originalByRelXPathLocator.LocateValue, latestByRelXPathLocator.LocateValue);
                })
                .FirstOrDefault();
        }

        public void StartSpying()
        {
            ThrowIfClosed();
            Task.Run(async () =>
            {
                await InjectLiveSpyScriptAsync(_browser.CurrentWindow.CurrentTab);
            }).Wait();
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo elementInfo, PomSetting? pomSetting = null)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo || htmlElementInfo.ElementObject is not IBrowserElement browserElement)
            {
                return elementInfo;
            }

            IEnumerable<ControlProperty> properties = [];
            IEnumerable<ElementLocator> locators = [];

            Task.Run(async () =>
            {
                HTMLElementInfo newHtmlElementInfo = await CreateHtmlElementAsync(browserElement);
                htmlElementInfo.ElementTitle = newHtmlElementInfo.ElementTitle ?? string.Empty;
                htmlElementInfo.Name = newHtmlElementInfo.Name ?? string.Empty;
                htmlElementInfo.ID = newHtmlElementInfo.ID ?? string.Empty;
                htmlElementInfo.Value = newHtmlElementInfo.Value ?? string.Empty;
                htmlElementInfo.ElementType = newHtmlElementInfo.ElementType ?? string.Empty;
                htmlElementInfo.XPath = newHtmlElementInfo.XPath ?? string.Empty;
                htmlElementInfo.RelXpath = GetXPathHelper().GetElementRelXPath(htmlElementInfo, pomSetting);

                string tag = await browserElement.TagNameAsync();
                string typeAttributeValue = await browserElement.AttributeValueAsync("type");

                htmlElementInfo.ElementTypeEnum = POMLearner.GetElementType(tag, typeAttributeValue);
                properties = await POMLearner.GetPropertiesAsync(htmlElementInfo);
                locators = await POMLearner.GenerateLocatorsAsync(htmlElementInfo, pomSetting);
            }).Wait();

            //AddRange needs to be called outside of the background thread, since its CollectionChanged event modifies some UI elements
            htmlElementInfo.Properties.Clear();
            htmlElementInfo.Properties.AddRange(properties);
            htmlElementInfo.Locators.Clear();
            htmlElementInfo.Locators.AddRange(locators);

            if (htmlElementInfo.FriendlyLocators.Count == 0 && htmlElementInfo.Locators.Count >= 1)
            {
                ElementLocator? byTagNameLocator = htmlElementInfo.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByTagName);
                if (byTagNameLocator != null)
                {
                    byTagNameLocator.Active = false;
                }
            }

            return htmlElementInfo;
        }

        public List<AppWindow> GetWindowAllFrames()
        {
            //copying from SeleniumDriver
            return [];
        }

        public string GetCurrentPageSourceString()
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                return await _browser.CurrentWindow.CurrentTab.PageSourceAsync();
            }).Result;
        }


        /// <summary>
        /// This field is only used as a cache for <see cref="GetPageSourceDocument(bool)"/> method. Don't rely on this to get the current tab content.
        /// </summary>
        private HtmlDocument? _currentPageDocument;

        public async Task<object> GetPageSourceDocument(bool reload)
        {
            ThrowIfClosed();

            if (reload)
            {
                _currentPageDocument = null;
            }

            if (_currentPageDocument == null)
            {
                _currentPageDocument = new HtmlDocument();
                _currentPageDocument.LoadHtml(await _browser.CurrentWindow.CurrentTab.PageSourceAsync());
            }

            return _currentPageDocument;
        }

        public XPathHelper GetXPathHelper(ElementInfo? elementInfo = null)
        {
            return new XPathHelper(Driver: this, ["PlaywrightDriver", "Web"]);
        }

        public ElementInfo GetRootElement()
        {
            ElementInfo rootElement = new()
            {
                ElementTitle = "html/body",
                ElementType = "root",
                Value = string.Empty,
                Path = string.Empty,
                XPath = "html/body"
            };
            return rootElement;
        }

        public ElementInfo UseRootElement()
        {
            ThrowIfClosed();
            Task.Run(() => _browser.CurrentWindow.CurrentTab.SwitchToMainFrameAsync().Wait()).Wait();
            return GetRootElement();
        }

        public ElementInfo? GetElementParent(ElementInfo elementInfo, PomSetting? pomSetting = null)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            return htmlElementInfo.ParentElement;
        }

        public string? GetElementProperty(ElementInfo elementInfo, string propertyName)
        {
            ThrowIfClosed();
            if (elementInfo.ElementObject == null)
            {
                elementInfo.ElementObject = Task.Run(() => _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByXPath, elementInfo.XPath).Result).Result;
            }
            if (elementInfo.ElementObject is not IBrowserElement browserElement)
            {
                return null;
            }
            return Task.Run(() => browserElement.AttributeValueAsync(propertyName).Result).Result;
        }

        public ElementInfo? FindFirst(ElementInfo elementInfo, List<XpathPropertyCondition> conditions)
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                IEnumerable<IBrowserElement> browserElements = await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByCSS, "*");
                foreach (IBrowserElement browserElement in browserElements)
                {
                    string tag = await browserElement.TagNameAsync();
                    if (!string.Equals(tag, elementInfo.ElementType))
                    {
                        continue;
                    }

                    bool allConditionsPassed = true;
                    foreach (XpathPropertyCondition condition in conditions)
                    {
                        string attributeValue = await browserElement.AttributeValueAsync(condition.PropertyName);
                        switch (condition.Op)
                        {
                            case XpathPropertyCondition.XpathConditionOperator.Equel:
                                allConditionsPassed = string.Equals(elementInfo.Value, attributeValue) && allConditionsPassed;
                                break;
                            case XpathPropertyCondition.XpathConditionOperator.Less:
                                int elementValueAsInt = Convert.ToInt32(elementInfo.Value);
                                int attributeValueAsInt = Convert.ToInt32(attributeValue);
                                allConditionsPassed = elementValueAsInt < attributeValueAsInt && allConditionsPassed;
                                break;
                            case XpathPropertyCondition.XpathConditionOperator.More:
                                elementValueAsInt = Convert.ToInt32(elementInfo.Value);
                                attributeValueAsInt = Convert.ToInt32(attributeValue);
                                allConditionsPassed = elementValueAsInt > attributeValueAsInt && allConditionsPassed;
                                break;
                        }
                    }
                    if (allConditionsPassed)
                    {
                        HTMLElementInfo newHtmlElement = await CreateHtmlElementAsync(browserElement);
                        newHtmlElement.WindowExplorer = this;
                        newHtmlElement.RelXpath = POMLearner.GenerateRelativeXPathFromHTMLElementInfo(newHtmlElement, xpathImpl: this, pomSetting: null);
                        return newHtmlElement;
                    }
                }
                return null;
            }).Result;
        }

        public List<ElementInfo> FindAll(ElementInfo elementInfo, List<XpathPropertyCondition> conditions)
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                List<HTMLElementInfo> foundElements = [];
                IEnumerable<IBrowserElement> browserElements = await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByCSS, "*");
                foreach (IBrowserElement browserElement in browserElements)
                {
                    string tag = await browserElement.TagNameAsync();
                    if (!string.Equals(tag, elementInfo.ElementType))
                    {
                        continue;
                    }

                    bool allConditionsPassed = true;
                    foreach (XpathPropertyCondition condition in conditions)
                    {
                        string attributeValue = await browserElement.AttributeValueAsync(condition.PropertyName);
                        switch (condition.Op)
                        {
                            case XpathPropertyCondition.XpathConditionOperator.Equel:
                                allConditionsPassed = string.Equals(elementInfo.Value, attributeValue) && allConditionsPassed;
                                break;
                            case XpathPropertyCondition.XpathConditionOperator.Less:
                                int elementValueAsInt = Convert.ToInt32(elementInfo.Value);
                                int attributeValueAsInt = Convert.ToInt32(attributeValue);
                                allConditionsPassed = elementValueAsInt < attributeValueAsInt && allConditionsPassed;
                                break;
                            case XpathPropertyCondition.XpathConditionOperator.More:
                                elementValueAsInt = Convert.ToInt32(elementInfo.Value);
                                attributeValueAsInt = Convert.ToInt32(attributeValue);
                                allConditionsPassed = elementValueAsInt > attributeValueAsInt && allConditionsPassed;
                                break;
                        }
                    }
                    if (allConditionsPassed)
                    {
                        HTMLElementInfo newHtmlElement = await CreateHtmlElementAsync(browserElement);
                        newHtmlElement.WindowExplorer = this;
                        newHtmlElement.RelXpath = POMLearner.GenerateRelativeXPathFromHTMLElementInfo(newHtmlElement, xpathImpl: this, pomSetting: null);
                        foundElements.Add(newHtmlElement);
                    }
                }
                return foundElements.Cast<ElementInfo>().ToList();
            }).Result;
        }

        public ElementInfo? GetPreviousSibling(ElementInfo elementInfo)
        {
            if (elementInfo == null)
            {
                return null;
            }

            if (elementInfo.ParentElement == null || elementInfo.ParentElement.ChildElements.Count <= 0)
            {
                return null;
            }

            IList<ElementInfo> childElements = elementInfo.ParentElement.ChildElements;

            for (int i = 0; i < childElements.Count; i++)
            {
                ElementInfo siblingElement = childElements[i];
                if (siblingElement != elementInfo)
                {
                    continue;
                }

                if (i > 0)
                {
                    return childElements[i - 1];
                }
            }

            return null;
        }

        public ElementInfo? GetNextSibling(ElementInfo elementInfo)
        {
            if (elementInfo == null)
            {
                return null;
            }

            if (elementInfo.ParentElement == null || elementInfo.ParentElement.ChildElements.Count <= 0)
            {
                return null;
            }

            IList<ElementInfo> childElements = elementInfo.ParentElement.ChildElements;

            for (int i = 0; i < childElements.Count; i++)
            {
                ElementInfo siblingElement = childElements[i];
                if (siblingElement != elementInfo)
                {
                    continue;
                }

                if (i < childElements.Count - 1)
                {
                    return childElements[i + 1];
                }
            }

            return null;
        }

        public string? GetElementID(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject != null)
            {
                return htmlElementInfo.HTMLElementObject.Id;
            }
            else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
            {
                return Task.Run(() => browserElement.AttributeValueAsync("id").Result).Result;
            }
            return null;
        }

        public string? GetElementTagName(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject != null)
            {
                return htmlElementInfo.HTMLElementObject.Name;
            }
            else if (htmlElementInfo.ElementObject is IBrowserElement browserElement)
            {
                return Task.Run(() => browserElement.TagNameAsync().Result).Result;
            }
            return null;
        }

        public List<object> GetAllElementsByLocator(eLocateBy locateBy, string locateValue)
        {
            ThrowIfClosed();
            return new(Task.Run(() => _browser.CurrentWindow.CurrentTab.GetElementsAsync(locateBy, locateValue).Result).Result);
        }

        public string? GetElementXpath(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            return POMLearner.GenerateXPathFromHtmlElementInfo(htmlElementInfo);
        }

        public string? GetInnerHtml(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject == null)
            {
                return null;
            }

            return htmlElementInfo.HTMLElementObject.InnerHtml;
        }

        public object? GetElementParentNode(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject == null)
            {
                return null;
            }

            return htmlElementInfo.HTMLElementObject.ParentNode;
        }

        public string? GetInnerText(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject == null)
            {
                return null;
            }

            return htmlElementInfo.HTMLElementObject.InnerText;
        }

        public string? GetPreviousSiblingInnerText(ElementInfo elementInfo)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return null;
            }

            if (htmlElementInfo.HTMLElementObject == null)
            {
                return null;
            }

            HtmlNode prevSibling = htmlElementInfo.HTMLElementObject.PreviousSibling;

            var innerText = string.Empty;

            //looking for text till two level up
            if (string.Equals(htmlElementInfo.HTMLElementObject.Name, "input", StringComparison.OrdinalIgnoreCase) && prevSibling == null)
            {
                prevSibling = htmlElementInfo.HTMLElementObject.ParentNode;

                if (string.IsNullOrEmpty(prevSibling.InnerText))
                {
                    prevSibling = prevSibling.PreviousSibling;
                }
            }

            if (prevSibling != null && !string.IsNullOrEmpty(prevSibling.InnerText) && prevSibling.ChildNodes.Count == 1)
            {
                innerText = prevSibling.InnerText;
            }

            return innerText;
        }

        [SupportedOSPlatform("windows")]
        public VisualElementsInfo GetVisualElementsInfo()
        {
            ThrowIfClosed();

            VisualElementsInfo visualElementsInfo = new()
            {
                Bitmap = GetScreenShot()
            };

            Task.Run(async () =>
            {
                //TODO: add function to get all tags available - below is missing some...
                List<IBrowserElement> visualBrowserElements = [];
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "a"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "input"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "select"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "label"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H1"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H2"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H3"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H4"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H5"));
                visualBrowserElements.AddRange(await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByTagName, "H6"));
                List<IBrowserElement> visibleVisualBrowserElements = [];
                foreach (IBrowserElement visualBrowserElement in visualBrowserElements)
                {
                    bool isVisible = await visualBrowserElement.IsVisibleAsync();
                    Size size = await visualBrowserElement.SizeAsync();
                    if (isVisible && size.Width > 0 && size.Height > 0)
                    {
                        visibleVisualBrowserElements.Add(visualBrowserElement);
                    }
                }

                List<VisualElement> visualElements = [];
                foreach (IBrowserElement visibleVisualBrowserElement in visibleVisualBrowserElements)
                {
                    string tag = await visibleVisualBrowserElement.TagNameAsync();
                    string text = await visibleVisualBrowserElement.TextContentAsync();
                    if (string.IsNullOrEmpty(text) && string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase))
                    {
                        text = await visibleVisualBrowserElement.AttributeValueAsync("value");
                        if (string.IsNullOrEmpty(text))
                        {
                            text = await visibleVisualBrowserElement.AttributeValueAsync("outerHTML");
                        }
                    }
                    Size size = await visibleVisualBrowserElement.SizeAsync();
                    Point position = await visibleVisualBrowserElement.PositionAsync();
                    VisualElement VE = new()
                    {
                        ElementType = tag,
                        Text = text,
                        X = position.X,
                        Y = position.Y,
                        Width = size.Width,
                        Height = size.Height
                    };
                    visualElements.Add(VE);
                }

                visualElementsInfo.Elements = visualElements; ;
            }).Wait();

            return visualElementsInfo;
        }

        public void ChangeAppWindowSize(int width, int height)
        {
            ThrowIfClosed();
            if (width <= 0 || height <= 0)
            {
                //for VRT action, it passes widht and height as 0 to maximize which causes issues with Playwright, so ignoring those
                return;
            }
            Size size = new(width, height);
            Task.Run(() => _browser.CurrentWindow.CurrentTab.SetViewportSizeAsync(size).Wait()).Wait();
        }

        public async Task<ElementInfo?> GetElementAtPoint(long ptX, long ptY)
        {
            ThrowIfClosed();
            try
            {
                HTMLElementInfo? elemInfo = null;

                string iframeXPath = string.Empty;
                Point parentElementLocation = new Point(0, 0);

                while (true)
                {
                    string s_Script = $"document.elementFromPoint({ptX}, {ptY});";

                    IBrowserElement? ele = await _browser.CurrentWindow.CurrentTab.GetElementAsync(s_Script);

                    if (ele == null)
                    {
                        return null;
                    }
                    else
                    {
                        HtmlNode? elemNode = null;
                        string elemId;
                        try
                        {
                            elemId = await ele.AttributeValueAsync("id");
                            if (_currentPageDocument == null)
                            {
                                _currentPageDocument = new HtmlDocument();
                                _currentPageDocument.LoadHtml(GetCurrentPageSourceString());
                            }
                            elemNode = _currentPageDocument.DocumentNode.Descendants().FirstOrDefault(x => x.Id.Equals(elemId));
                        }
                        catch (Exception)
                        {
                            elemId = "";
                        }

                        elemInfo = await CreateHtmlElementAsync(ele);
                        elemInfo.Path = iframeXPath;
                    }

                    if (elemInfo.ElementTypeEnum != eElementType.Iframe)    // ele.TagName != "frame" && ele.TagName != "iframe")
                    {
                        await _browser.CurrentWindow.CurrentTab.SwitchToMainFrameAsync();

                        break;
                    }

                    if (string.IsNullOrEmpty(iframeXPath))
                    {
                        iframeXPath = elemInfo.XPath;
                    }
                    else
                    {
                        iframeXPath += "," + elemInfo.XPath;
                    }

                    parentElementLocation.X += elemInfo.X;
                    parentElementLocation.Y += elemInfo.Y;

                    String s_Script2 = "var X, Y; "
                            + "if (window.pageYOffset) " // supported by most browsers 
                            + "{ "
                            + "  X = window.pageXOffset; "
                            + "  Y = window.pageYOffset; "
                            + "} "
                            + "else " // Internet Explorer 6, 7, 8
                            + "{ "
                            + "  var  Elem = document.documentElement; "         // <html> node (IE with DOCTYPE)
                            + "  if (!Elem.clientHeight) Elem = document.body; " // <body> node (IE in quirks mode)
                            + "  X = Elem.scrollLeft; "
                            + "  Y = Elem.scrollTop; "
                            + "} "
                            + "return new Array(X, Y);";

                    IList<Object> i_Coord = (IList<Object>)_browser.CurrentWindow.CurrentTab.ExecuteJavascriptAsync(s_Script2);

                    int s32_ScrollX = Convert.ToInt32(i_Coord[0]);
                    int s32_ScrollY = Convert.ToInt32(i_Coord[1]);

                    Point elePosition = await ele.PositionAsync();
                    Point p_Pos = new Point(elePosition.X - s32_ScrollX,
                                     elePosition.Y - s32_ScrollY);
                    ptX -= p_Pos.X;
                    ptY -= p_Pos.Y;

                    //Driver.SwitchTo().Frame(ele);
                }

                elemInfo.X += parentElementLocation.X;
                elemInfo.Y += parentElementLocation.Y;

                return elemInfo;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Element At Point", ex);
                return null;
            }
        }

        public string GetApplitoolServerURL()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl;
        }

        public string GetApplitoolKey()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
        }

        public ePlatformType GetPlatform()
        {
            return this.Platform;
        }

        public string GetEnvironment()
        {
            return this.BusinessFlow.Environment;
        }

        public OpenQA.Selenium.IWebDriver? GetWebDriver()
        {
            return null;
        }

        public string GetAgentAppName()
        {
            return BrowserType.ToString();
        }

        public string GetViewport()
        {
            ThrowIfClosed();
            return Task.Run(() => _browser.CurrentWindow.CurrentTab.ViewportSizeAsync().Result).Result.ToString();
        }

        private static readonly IEnumerable<ActUIElement.eElementAction> ActUIElementSupportedOperations =
        [
            ActUIElement.eElementAction.Click,
            ActUIElement.eElementAction.DoubleClick,
            ActUIElement.eElementAction.Hover,
            ActUIElement.eElementAction.IsVisible,
            ActUIElement.eElementAction.IsEnabled,
            ActUIElement.eElementAction.GetAttrValue,
            ActUIElement.eElementAction.GetText,
            ActUIElement.eElementAction.MouseRightClick,
            ActUIElement.eElementAction.IsValuePopulated,
            ActUIElement.eElementAction.GetHeight,
            ActUIElement.eElementAction.GetWidth,
            ActUIElement.eElementAction.GetSize,
            ActUIElement.eElementAction.GetStyle,
            ActUIElement.eElementAction.GetValue,
            ActUIElement.eElementAction.GetItemCount,
            ActUIElement.eElementAction.ScrollToElement,
            ActUIElement.eElementAction.SetFocus,
            ActUIElement.eElementAction.IsDisabled,
            ActUIElement.eElementAction.Submit,
            ActUIElement.eElementAction.MultiClicks,
            ActUIElement.eElementAction.ClickXY,
            ActUIElement.eElementAction.DoubleClickXY,
            ActUIElement.eElementAction.ClearValue,
            ActUIElement.eElementAction.Select,
            ActUIElement.eElementAction.SelectByText,
            ActUIElement.eElementAction.SelectByIndex,
            ActUIElement.eElementAction.SetValue,
            ActUIElement.eElementAction.ClickAndValidate,
            ActUIElement.eElementAction.JavaScriptClick,
            ActUIElement.eElementAction.MouseClick,
            ActUIElement.eElementAction.SetText,
            ActUIElement.eElementAction.SendKeys,
            ActUIElement.eElementAction.SendKeysXY,
            ActUIElement.eElementAction.RunJavaScript,
            ActUIElement.eElementAction.AsyncClick,
            ActUIElement.eElementAction.GetCustomAttribute,
            ActUIElement.eElementAction.GetFont,
            ActUIElement.eElementAction.MousePressRelease,
            ActUIElement.eElementAction.GetValidValues,
            ActUIElement.eElementAction.GetTextLength,
            ActUIElement.eElementAction.GetSelectedValue,
            ActUIElement.eElementAction.DragDrop,
            ActUIElement.eElementAction.MultiSetValue,
            ActUIElement.eElementAction.DrawObject,
        ];


        private static readonly IEnumerable<ActBrowserElement.eControlAction> ActBrowserElementSupportedOperations =
        [
            ActBrowserElement.eControlAction.GotoURL,
            ActBrowserElement.eControlAction.OpenURLNewTab,
            ActBrowserElement.eControlAction.GetPageURL,
            ActBrowserElement.eControlAction.GetWindowTitle,
            ActBrowserElement.eControlAction.NavigateBack,
            ActBrowserElement.eControlAction.Refresh,
            ActBrowserElement.eControlAction.DeleteAllCookies,
            ActBrowserElement.eControlAction.RunJavaScript,
            ActBrowserElement.eControlAction.GetPageSource,
            ActBrowserElement.eControlAction.Close,
            ActBrowserElement.eControlAction.CloseTabExcept,
            ActBrowserElement.eControlAction.CloseAll,
            ActBrowserElement.eControlAction.CheckPageLoaded,
            ActBrowserElement.eControlAction.GetConsoleLog,
            ActBrowserElement.eControlAction.GetBrowserLog,
            ActBrowserElement.eControlAction.SwitchFrame,
            ActBrowserElement.eControlAction.SwitchToDefaultFrame,
            ActBrowserElement.eControlAction.SwitchToParentFrame,
            ActBrowserElement.eControlAction.SwitchWindow,
            ActBrowserElement.eControlAction.SwitchToDefaultWindow,
            ActBrowserElement.eControlAction.AcceptMessageBox,
            ActBrowserElement.eControlAction.DismissMessageBox,
            ActBrowserElement.eControlAction.GetMessageBoxText,
            ActBrowserElement.eControlAction.SetAlertBoxText,
            ActBrowserElement.eControlAction.StartMonitoringNetworkLog,
            ActBrowserElement.eControlAction.GetNetworkLog,
            ActBrowserElement.eControlAction.StopMonitoringNetworkLog,
        ];
    }
}
