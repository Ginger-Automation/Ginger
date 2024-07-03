using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
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
using System.Drawing;
using GingerCore.Actions.VisualTesting;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.ComponentModel;
using System.Runtime.Versioning;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.ActionHandlers;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text.RegularExpressions;
using GingerCore.Drivers.Common;
using System.IO;
using HtmlAgilityPack;
using Protractor;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.IO;
using DocumentFormat.OpenXml.Wordprocessing;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    public sealed class PlaywrightDriver : GingerWebDriver, IVirtualDriver, IIncompleteDriver, IWindowExplorer, IXPath
    {
        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set \"true\" to run the browser in background (headless mode) for faster Execution")]
        public bool HeadlessBrowserMode { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Proxy Server:Port")]
        public string? Proxy { get; set; }

        private PlaywrightBrowser? _browser;
        private IBrowserElement? _lastHighlightedElement;

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
                    ActUIElementHandler actUIElementHandler = new(actUIElement, _browser!, BusinessFlow, Environment);
                    actUIElementHandler.HandleAsync().Wait();
                    break;
                case ActScreenShot actScreenShot:
                    ActScreenShotHandler actScreenShotHandler = new(actScreenShot, _browser!);
                    actScreenShotHandler.HandleAsync().Wait();
                    break;
                default:
                    act.Error = $"Run Action Failed due to unrecognized action type - {act.GetType().Name}";
                    break;
            }
        }

        public bool IsActionSupported(Act act, out string message)
        {
            message = string.Empty;

            if (act is ActWithoutDriver)
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
            else if (act is ActScreenShot)
            {
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
        private void ThrowIfClosed()
        {
            if (!IsRunning())
            {
                throw new InvalidOperationException($"Cannot perform operation on closed driver.");
            }
        }

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

        public void HighLightElement(ElementInfo element, bool locateElementByItLocators = false, IList<ElementInfo> MappedUIElements = null)
        {
            Task.Run(async () =>
            {
                if (_lastHighlightedElement != null)
                {
                    await UnhighlightElementAsync(_lastHighlightedElement);
                }
                await SwitchToFrameOfElementAsync(element);
                IBrowserElement? browserElement = await FindBrowserElementAsync(element);
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

                if (_lastHighlightedElement != null)
                {
                    await UnhighlightElementAsync(_lastHighlightedElement);
                }

                await InjectLiveSpyScriptAsync(currentTab);

                IBrowserElement? browserElement = (await currentTab.GetElementsAsync("GingerLibLiveSpy.ElementFromPoint()")).FirstOrDefault();
                if (browserElement == null)
                {
                    return null!;
                }

                string screenshot = Convert.ToBase64String(await browserElement.ScreenshotAsync());
                string tag = await browserElement.TagNameAsync();
                string xPath = string.Empty;
                if (string.Equals(tag, "iframe", StringComparison.OrdinalIgnoreCase) && string.Equals(tag, "frame", StringComparison.OrdinalIgnoreCase))
                {
                    xPath = await GenerateXPathFromBrowserElementAsync(browserElement);
                    //TODO: create HTMLElementInfo specific for IFrame
                }

                return new HTMLElementInfo()
                {
                    ElementObject = browserElement,
                    ScreenShotImage = screenshot,
                    XPath = xPath,
                };
            }).Result;
        }

        public AppWindow GetActiveWindow()
        {
            ThrowIfClosed();
            return Task.Run(async () =>
            {
                string title = await _browser.CurrentWindow.CurrentTab.TitleAsync();
                return new AppWindow()
                {
                    Title = title,
                };
            }).Result;
        }

        public async Task<List<ElementInfo>> GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null)
        {
            ThrowIfClosed();
            
            if (_lastHighlightedElement != null)
            {
                await UnhighlightElementAsync(_lastHighlightedElement);
            }

            IBrowserTab currentTab = _browser.CurrentWindow.CurrentTab;

            await currentTab.SwitchToMainFrameAsync();
            string pageSource = await _browser.CurrentWindow.CurrentTab.PageSourceAsync();


            POMLearner pomLearner = POMLearner.Create(pageSource, new PlaywrightBrowserElementProvider(currentTab), pomSetting, xpathImpl: this);
            IEnumerable<HTMLElementInfo> htmlElements = await pomLearner.LearnElementsAsync();

            if (foundElementsList != null)
            {
                foundElementsList.AddRange(htmlElements.Cast<ElementInfo>());
            }

            return new(htmlElements);
        }

        private sealed class PlaywrightBrowserElementProvider : POMLearner.IBrowserElementProvider
        {
            private readonly IBrowserTab _browserTab;

            internal PlaywrightBrowserElementProvider(IBrowserTab browserTab)
            {
                _browserTab = browserTab;
            }

            public async Task<IBrowserElement?> GetElementAsync(eLocateBy locateBy, string locateValue)
            {
                return (await _browserTab.GetElementsAsync(locateBy, locateValue)).FirstOrDefault();
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
                throw new NotImplementedException();
            }

            public Task OnShadowDOMExitAsync(HTMLElementInfo shadowHostElement)
            {
                throw new NotImplementedException();
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
                IEnumerable<IBrowserElement> browserElements = await _browser.CurrentWindow.CurrentTab.GetElementsAsync(eLocateBy.ByXPath, xpath);
                List<HTMLElementInfo> htmlElements = [];
                foreach (IBrowserElement browserElement in browserElements)
                {
                    string tag = await browserElement.TagNameAsync();
                    string nameAttributeValue = await browserElement.AttributeValueAsync("name");
                    string idAttributeValue = await browserElement.AttributeValueAsync("id");
                    string valueAttributeValue = await browserElement.AttributeValueAsync("value");
                    string typeAttributeValue = await browserElement.AttributeValueAsync("type");

                    string elementTitle = tag;
                    if (string.Equals(tag, "table", StringComparison.OrdinalIgnoreCase))
                    {
                        elementTitle = "Table";
                    }
                    else if (!string.IsNullOrEmpty(nameAttributeValue))
                    {
                        elementTitle = $"{nameAttributeValue} {tag}";
                    }
                    else if (!string.IsNullOrEmpty(idAttributeValue))
                    {
                        elementTitle = $"{idAttributeValue} {tag}";
                    }
                    else if (!string.IsNullOrEmpty(valueAttributeValue))
                    {
                        elementTitle = $"{(valueAttributeValue.Length > 50 ? valueAttributeValue.Substring(0, 50) + "..." : valueAttributeValue)} {tag}";
                    }

                    string elementName = tag;
                    if (string.IsNullOrEmpty(elementName))
                    {
                        elementName = nameAttributeValue;
                    }

                    string elementId = idAttributeValue;
                    if (string.IsNullOrEmpty(elementId))
                    {
                        elementId = htmlElementInfo.HTMLElementObject.Id;
                    }

                    string elementValue = string.Empty;
                    if (string.Equals(tag, "select", StringComparison.OrdinalIgnoreCase))
                    {
                        elementValue = $"set to {await browserElement.ExecuteJavascriptAsync("element => element.options[element.selectedIndex].text")}";
                    }
                    else if (string.Equals(tag, "span", StringComparison.OrdinalIgnoreCase))
                    {
                        elementValue = $"set to {await browserElement.TextContentAsync()}";
                    }
                    else if (string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase) && string.Equals(typeAttributeValue, "checkbox", StringComparison.OrdinalIgnoreCase)) 
                    {
                        elementValue = $"set to {await browserElement.ExecuteJavascriptAsync("element => element.checked.toString()")}";
                    }
                    else
                    {
                        elementValue = valueAttributeValue;
                    }

                    string elementType = string.Empty;
                    if (string.Equals(tag, "input", StringComparison.OrdinalIgnoreCase))
                    {
                        elementType = $"{tag}.{typeAttributeValue}";
                    }
                    else if (string.Equals(tag, "a", StringComparison.OrdinalIgnoreCase) || string.Equals(tag, "li", StringComparison.OrdinalIgnoreCase))
                    {
                        elementType = "link";
                    }
                    else
                    {
                        elementType = tag;
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

                    HTMLElementInfo newHtmlElement = new()
                    {
                        ElementObject = browserElement,
                        ElementTitle = elementTitle ?? string.Empty,
                        WindowExplorer = this,
                        Name = elementName ?? string.Empty,
                        ID = elementId ?? string.Empty,
                        Value = elementValue ?? string.Empty,
                        Path = elementPath,
                        XPath = await GenerateXPathFromBrowserElementAsync(browserElement),
                        ElementType = elementType ?? string.Empty,
                        ElementTypeEnum = POMLearner.GetElementType(tag, typeAttributeValue),
                    };
                    newHtmlElement.RelXpath = POMLearner.GenerateRelativeXPathFromHTMLElementInfo(newHtmlElement, xpathImpl: this, pomSetting: null);
                    htmlElements.Add(newHtmlElement);
                }
                return htmlElements.Cast<ElementInfo>().ToList();
            }).Result;
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
            htmlElementInfo.OptionalValuesObjectsList.AddRange(POMLearner.GetOptionValues(htmlElementInfo));

            return new(POMLearner.GetProperties(htmlElementInfo));
        }

        public ObservableList<ElementLocator> GetElementLocators(ElementInfo elementInfo, PomSetting pomSetting = null)
        {
            if (elementInfo is not HTMLElementInfo htmlElementInfo)
            {
                return [];
            }

            return new(POMLearner.GenerateLocators(htmlElementInfo, pomSetting));
        }

        public ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            return [];
        }

        public ObservableList<OptionalValue> GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            //copying existing implementation from SeleniumDriver
            throw new NotImplementedException();
        }

        public object GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
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
            return new List<eTabView>() { eTabView.Screenshot, eTabView.GridView, eTabView.PageSource, eTabView.TreeView };
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

        public bool TestElementLocators(ElementInfo Element, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null)
        {
            throw new NotImplementedException();
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            throw new NotImplementedException();
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            throw new NotImplementedException();
        }

        public void StartSpying()
        {
            ThrowIfClosed();
            Task.Run(async () =>
            {
                await InjectLiveSpyScriptAsync(_browser.CurrentWindow.CurrentTab);
            }).Wait();
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI, PomSetting? pomSetting = null)
        {
            throw new NotImplementedException();
        }

        public List<AppWindow> GetWindowAllFrames()
        {
            //copying from SeleniumDriver
            throw new NotImplementedException();
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

        private static readonly IEnumerable<ActUIElement.eElementAction> ActUIElementSupportedOperations = new List<ActUIElement.eElementAction>()
        {
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
        };


        private static readonly IEnumerable<ActBrowserElement.eControlAction> ActBrowserElementSupportedOperations = new List<ActBrowserElement.eControlAction>()
        {
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
        };
    }
}
