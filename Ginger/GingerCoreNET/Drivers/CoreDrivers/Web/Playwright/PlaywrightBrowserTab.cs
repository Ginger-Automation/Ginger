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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using GingerCore.Actions;
using Microsoft.Playwright;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IPlaywrightElementHandle = Microsoft.Playwright.IElementHandle;
using IPlaywrightFrameLocator = Microsoft.Playwright.IFrameLocator;
using IPlaywrightJSHandle = Microsoft.Playwright.IJSHandle;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;
using IPlaywrightPage = Microsoft.Playwright.IPage;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserTab : IBrowserTab
    {
        private static readonly IEnumerable<eLocateBy> SupportedElementLocators =
        [
            eLocateBy.ByID,
            eLocateBy.ByCSS,
            eLocateBy.ByName,
            eLocateBy.ByXPath,
            eLocateBy.ByTagName,
            eLocateBy.ByRelXPath,
            eLocateBy.POMElement,
            eLocateBy.ByAutomationID,
        ];

        private static readonly IEnumerable<eLocateBy> SupportedFrameLocators =
        [
            eLocateBy.ByID,
            eLocateBy.ByTitle,
            eLocateBy.ByUrl,
            eLocateBy.ByXPath,
            eLocateBy.ByRelXPath,
        ];

        private readonly IPlaywrightPage _playwrightPage;
        private readonly IBrowserTab.OnTabClose _onTabClose;
        private readonly LinkedList<string> _consoleMessages = [];
        private IFrame _currentFrame;
        private bool _isClosed = false;
        List<Tuple<string, object>> networkResponseLogList;
        List<Tuple<string, object>> networkRequestLogList;
        ActBrowserElement _act;
        public bool isNetworkLogMonitoringStarted = false;
        public bool isDialogDismiss = true;
        IDialog dialogs;

        public bool IsClosed => _isClosed;
        BrowserHelper _BrowserHelper;

        internal PlaywrightBrowserTab(IPlaywrightPage playwrightPage, IBrowserTab.OnTabClose onTabClose)
        {
            _playwrightPage = playwrightPage;
            _onTabClose = onTabClose;
            _currentFrame = _playwrightPage.MainFrame;
            _playwrightPage.Console += OnConsoleMessage;
            _playwrightPage.Close += OnPlaywrightPageClose;
            _playwrightPage.Dialog += OnPlaywrightDialog;
        }

        private void RemoveEventHandlers()
        {
            _playwrightPage.Console -= OnConsoleMessage;
            _playwrightPage.Close -= OnPlaywrightPageClose;
            _playwrightPage.Dialog -= OnPlaywrightDialog;
        }

        private void OnPlaywrightPageClose(object? sender, IPlaywrightPage e)
        {
            _ = CloseAsync();
        }

        /// <summary>
        /// Handles the console message event and adds the message to the console messages list.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The console message event arguments.</param>
        private void OnConsoleMessage(object? sender, IConsoleMessage e)
        {
            //TODO: Playwright - Selenium console logs contain timestamp and level. Try adding those in these as well so that we can have similar log structure
            _consoleMessages.AddLast(e.Text);
        }

        /// <summary>
        /// Brings the browser tab to the front.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task BringToFrontAsync()
        {
            return _playwrightPage.BringToFrontAsync();
        }

        /// <summary>
        /// Executes the specified JavaScript code on the current page and returns the result as a string.
        /// </summary>
        /// <param name="script">The JavaScript code to execute.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the result of the JavaScript code execution as a string.</returns>
        public Task<string> ExecuteJavascriptAsync(string script)
        {
            ThrowIfClosed();
            return _playwrightPage.EvaluateAsync<string>(script);
        }
        /// <summary>
        /// Executes the specified JavaScript code on the current frame and returns the result as a string.
        /// </summary>
        /// <param name="script">The JavaScript code to execute.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the result of the JavaScript code execution as a string.</returns>

        public Task<string> ExecuteJavascriptIframeAsync(string script)
        {
            ThrowIfClosed();
            return _currentFrame.EvaluateAsync<string>(script);
        }

        /// <summary>
        /// Executes the specified JavaScript code on the current page with the provided argument and returns the result as a string.
        /// </summary>
        /// <param name="script">The JavaScript code to execute.</param>
        /// <param name="arg">The argument to pass to the JavaScript code.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the result of the JavaScript code execution as a string.</returns>
        public Task<string> ExecuteJavascriptAsync(string script, object arg)
        {
            ThrowIfClosed();
            return _playwrightPage.EvaluateAsync<string>(script, arg);
        }

        /// <summary>
        /// Executes the specified JavaScript code on the current frame with the provided argument and returns the result as a string.
        /// </summary>
        /// <param name="script">The JavaScript code to execute.</param>
        /// <param name="arg">The argument to pass to the JavaScript code.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the result of the JavaScript code execution as a string.</returns>

        public Task<string> ExecuteJavascriptIframeAsync(string script, object arg)
        {
            ThrowIfClosed();
            return _currentFrame.EvaluateAsync<string>(script, arg);
        }

        /// <summary>
        /// Injects the specified JavaScript code into the current page.
        /// </summary>
        /// <param name="script">The JavaScript code to inject.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InjectJavascriptAsync(string script)
        {
            ThrowIfClosed();
            return _playwrightPage.AddScriptTagAsync(new PageAddScriptTagOptions()
            {
                Content = script,
            });
        }
        /// <summary>
        /// Injects the specified JavaScript code into the current frame.
        /// </summary>
        /// <param name="script">The JavaScript code to inject.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task InjectJavascriptIframeAsync(string script)
        {
            ThrowIfClosed();
            return _currentFrame.AddScriptTagAsync(new FrameAddScriptTagOptions()
            {
                Content = script,
            });
        }

        /// <summary>
        /// Retrieves the source code of the current page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The task result contains the source code of the page as a string.</returns>
        public Task<string> PageSourceAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.ContentAsync();
        }

        /// <summary>
        /// Performs a left mouse click at the specified point on the page.
        /// </summary>
        /// <param name="point">The coordinates of the point to click.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task MouseClickAsync(Point point)
        {
            ThrowIfClosed();
            return _playwrightPage.Mouse.ClickAsync(point.X, point.Y, new MouseClickOptions()
            {
                Button = MouseButton.Left,
            });
        }

        /// <summary>
        /// Performs a right mouse click at the specified point on the page.
        /// </summary>
        /// <param name="point">The coordinates of the point to click.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task MouseRightClickAsync(Point point)
        {
            ThrowIfClosed();
            return _playwrightPage.Mouse.ClickAsync(point.X, point.Y, new MouseClickOptions()
            {
                Button = MouseButton.Right,
            });
        }

        /// <summary>
        /// Moves the mouse to the specified point on the page.
        /// </summary>
        /// <param name="point">The coordinates of the point to move the mouse to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task MoveMouseAsync(Point point)
        {
            ThrowIfClosed();
            return _playwrightPage.Mouse.MoveAsync(point.X, point.Y);
        }

        /// <summary>
        /// Retrieves the title of the current page.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the title of the page.</returns>
        public Task<string> TitleAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.TitleAsync();
        }

        /// <summary>
        /// Retrieves the URL of the current page.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the URL of the page.</returns>
        public Task<string> URLAsync()
        {
            ThrowIfClosed();
            return Task.FromResult(_playwrightPage.Url);
        }

        /// <summary>
        /// Navigates the browser tab to the specified URL.
        /// </summary>
        /// <param name="url">The URL to navigate to.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task GoToURLAsync(string url)
        {
            ThrowIfClosed();
            await _playwrightPage.GotoAsync(url);
            _currentFrame = _playwrightPage.MainFrame;
        }

        /// <summary>
        /// Navigates the browser tab back to the previous page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task NavigateBackAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.GoBackAsync();
        }

        /// <summary>
        /// Navigates the browser tab forward to the next page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task NavigateForwardAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.GoForwardAsync();
        }

        /// <summary>
        /// Refreshes the current page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task RefreshAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.ReloadAsync();
        }

        /// <summary>
        /// Waits until the page is fully loaded.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task WaitTillLoadedAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.WaitForLoadStateAsync(LoadState.Load);
        }

        /// <summary>
        /// Retrieves the console logs from the current page.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the console logs as a string.</returns>
        public Task<string> ConsoleLogsAsync()
        {
            ThrowIfClosed();
            return Task.FromResult(string.Join('\n', _consoleMessages));
        }

        /// <summary>
        /// Retrieves the browser logs from the current page.
        /// </summary>
        /// <returns>The browser logs as a string.</returns>
        public async Task<string> BrowserLogsAsync()
        {
            ThrowIfClosed();
            string script = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; network;";

            JsonElement? logs = await _playwrightPage.EvaluateAsync(script);
            string rawLogs = string.Empty;
            if (logs.HasValue)
            {
                JsonNode? jsonLogs = JsonNode.Parse(logs.Value.ToString());

                if (jsonLogs != null && jsonLogs.GetValueKind() == JsonValueKind.Array)
                {
                    JsonArray jsonArray = jsonLogs.AsArray();
                    foreach (JsonNode? item in jsonArray)
                    {
                        if (item == null || item.GetValueKind() != JsonValueKind.Object)
                        {
                            continue;
                        }
                        ((JsonObject)item).Remove("$id");
                    }
                }

                if (jsonLogs != null)
                {
                    rawLogs = jsonLogs.ToJsonString();
                }
            }
            return rawLogs;
        }

        /// <summary>
        /// Switches the current frame to the first frame specified by the given locator.
        /// </summary>
        /// <param name="locateBy">The method of locating the frame.</param>
        /// <param name="value">The value used for locating the frame.</param>
        /// <returns>True if the frame was successfully switched, false otherwise.</returns>
        public async Task<bool> SwitchFrameAsync(eLocateBy locateBy, string value)
        {
            ThrowIfClosed();
            if (!IsFrameLocatorSupported(locateBy))
            {
                throw new LocatorNotSupportedException($"Frame locator '{locateBy}' is not supported.");
            }

            IPlaywrightFrameLocator frameLocator = locateBy switch
            {
                eLocateBy.ByID => _currentFrame.FrameLocator($"css=#{value}"),
                eLocateBy.ByTitle => _currentFrame.FrameLocator($"css=iframe[title='{value}']"),
                eLocateBy.ByXPath => _currentFrame.FrameLocator($"xpath={value}"),
                eLocateBy.ByRelXPath => _currentFrame.FrameLocator($"xpath={value}"),
                eLocateBy.ByUrl => _currentFrame.FrameLocator($"css=iframe[src='{value}']"),
                _ => throw new ArgumentException($"Frame locator '{locateBy}' is not supported."),
            };
            bool wasLocated = await frameLocator.Owner.CountAsync() > 0;
            if (!wasLocated)
            {
                return false;
            }

            IPlaywrightJSHandle jsHandle = await frameLocator.Owner.First.EvaluateHandleAsync("element => element");
            IPlaywrightElementHandle? elementHandle = jsHandle.AsElement();
            if (elementHandle == null)
            {
                return false;
            }

            IFrame? frame = await elementHandle.ContentFrameAsync();
            if (frame == null)
            {
                return false;
            }

            _currentFrame = frame;

            return true;
        }

        /// <summary>
        /// Switches the current frame to the main frame.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SwitchToMainFrameAsync()
        {
            ThrowIfClosed();
            _currentFrame = _playwrightPage.MainFrame;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Switches the current frame to the parent frame.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SwitchToParentFrameAsync()
        {
            ThrowIfClosed();
            IFrame? parentFrame = _currentFrame.ParentFrame;
            if (parentFrame != null)
            {
                _currentFrame = parentFrame;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves a collection of browser elements based on the specified locator.
        /// </summary>
        /// <param name="locateBy">The method of locating the elements.</param>
        /// <param name="value">The value used for locating the elements.</param>
        /// <returns>A collection of browser elements.</returns>
        public async Task<IEnumerable<IBrowserElement>> GetElementsAsync(eLocateBy locateBy, string value)
        {
            ThrowIfClosed();
            IPlaywrightLocator locator = await GetElementLocator(locateBy, value);

            int matchedElementCount = await locator.CountAsync();
            IBrowserElement[] elements = new IBrowserElement[matchedElementCount];

            for (int index = 0; index < matchedElementCount; index++)
            {
                elements[index] = new PlaywrightBrowserElement(locator.Nth(index));
            }

            return elements;
        }

        /// <summary>
        /// Retrieves a browser element using the specified JavaScript code.
        /// </summary>
        /// <param name="javascript">The JavaScript code used to locate the element.</param>
        /// <returns>The browser element.</returns>
        public async Task<IBrowserElement?> GetElementAsync(string javascript)
        {
            ThrowIfClosed();
            IPlaywrightJSHandle jsHandle = await _currentFrame.EvaluateHandleAsync(javascript);
            IPlaywrightElementHandle? elementHandle = jsHandle.AsElement();
            if (elementHandle == null)
            {
                return null;
            }
            return new PlaywrightBrowserElement(elementHandle);
        }

        /// <summary>
        /// Takes a screenshot of the current page.
        /// </summary>
        /// <returns>The screenshot image as a byte array.</returns>
        public Task<byte[]> ScreenshotAsync()
        {
            return ScreenshotInternalAsync(fullPage: false);
        }

        /// <summary>
        /// Takes a full-page screenshot of the current page.
        /// </summary>
        /// <returns>The full-page screenshot image as a byte array.</returns>
        public Task<byte[]> ScreenshotFullPageAsync()
        {
            return ScreenshotInternalAsync(fullPage: true);
        }

        private Task<byte[]> ScreenshotInternalAsync(bool fullPage)
        {
            ThrowIfClosed();
            return _playwrightPage.ScreenshotAsync(new PageScreenshotOptions()
            {
                FullPage = fullPage,
            });
        }

        /// <summary>
        /// Retrieves the size of the viewport.
        /// </summary>
        /// <returns>The size of the viewport as a <see cref="Size"/> object.</returns>
        public async Task<Size> ViewportSizeAsync()
        {
            PageViewportSizeResult? sizeResult = _playwrightPage.ViewportSize;
            if (sizeResult != null)
            {
                return new Size(sizeResult.Width, sizeResult.Height);
            }

            try
            {
                int width = int.Parse(await ExecuteJavascriptAsync("window.innerWidth"));
                int height = int.Parse(await ExecuteJavascriptAsync("window.innerHeight"));
                return new Size(width, height);
            }
            catch (Exception)
            {
                throw new PlaywrightException($"Unable to get viewport size");
            }
        }

        /// <summary>
        /// Sets the size of the viewport.
        /// </summary>
        /// <param name="size">The desired size of the viewport.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetViewportSizeAsync(Size size)
        {
            return _playwrightPage.SetViewportSizeAsync(size.Width, size.Height);
        }

        /// <summary>
        /// Get <see cref="IPlaywrightLocator"/> based on the given locate values. The returned locator doesn't guarantee that an element actually exists at that location. <br/>
        /// Supported locators, <br/>
        /// <see cref="eLocateBy.ByID"/>, <see cref="eLocateBy.ByCSS"/>, <see cref="eLocateBy.ByXPath"/>.
        /// </summary>
        /// <param name="locateBy">Locate element based on which property.</param>
        /// <param name="value">The value of the locating property.</param>
        /// <returns><see cref="IPlaywrightLocator"/> pointing to a location in the webpage matching the provided locator values.</returns>
        /// <exception cref="LocatorNotSupportedException"></exception>
        private Task<IPlaywrightLocator> GetElementLocator(eLocateBy locateBy, string value)
        {
            if (!IsElementLocatorSupported(locateBy))
            {
                throw new LocatorNotSupportedException($"Element locator '{locateBy}' is not supported.");
            }

            IPlaywrightLocator locator;
            switch (locateBy)
            {
                case eLocateBy.ByID:
                    value = value.Replace(":", "\\:");
                    locator = _currentFrame.Locator($"css=#{value}");
                    break;
                case eLocateBy.ByCSS:
                    locator = _currentFrame.Locator($"css={value}");
                    break;
                case eLocateBy.ByXPath:
                case eLocateBy.ByRelXPath:
                    locator = _currentFrame.Locator($"xpath={value}");
                    break;
                case eLocateBy.ByName:
                    locator = _currentFrame.Locator($"css=[name='{value}']");
                    break;
                case eLocateBy.ByTagName:
                    locator = _currentFrame.Locator($"css={value}");
                    break;
                case eLocateBy.ByAutomationID:
                    value = value.Replace(":", "\\:");
                    locator = _currentFrame.Locator($"xpath=//*[@data-automation-id=\"{value}\"]");
                    break;
                default:
                    throw new LocatorNotSupportedException($"Element locator '{locateBy}' is not supported.");
            }
            return Task.FromResult(locator);
        }

        /// <summary>
        /// Retrieves the currently focused browser element.
        /// </summary>
        /// <returns>The focused browser element, or null if no element is focused.</returns>
        public async Task<IBrowserElement?> GetFocusedElement()
        {
            ThrowIfClosed();
            IPlaywrightLocator locator = _currentFrame.Locator("css=*:focus");
            int matchedElementCount = await locator.CountAsync();
            if (matchedElementCount <= 0)
            {
                return null;
            }

            return new PlaywrightBrowserElement(locator);
        }

        /// <summary>
        /// Checks if the specified locator exists in the current frame.
        /// </summary>
        /// <param name="locator">The locator to check.</param>
        /// <returns>True if the locator exists, false otherwise.</returns>
        private static async Task<bool> DoesLocatorExistsAsync(IPlaywrightLocator locator)
        {
            return await locator.CountAsync() > 0;
        }

        /// <summary>
        /// Closes the browser tab.
        /// </summary>
        public async Task CloseAsync()
        {
            if (_isClosed)
            {
                return;
            }
            _isClosed = true;
            //_playwrightPage.Dialog -= OnDialog;
            await _playwrightPage.CloseAsync();
            RemoveEventHandlers();
            await _onTabClose(closedTab: this);
        }

        /// <summary>
        /// Throws an exception if the browser tab is closed.
        /// </summary>
        private void ThrowIfClosed()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Cannot perform operation, tab is already closed.");
            }
        }

        /// <summary>
        /// Runs an accessibility test on the current page using Axe.
        /// </summary>
        /// <param name="options">Optional Axe run options.</param>
        /// <returns>The Axe result of the accessibility test.</returns>
        public async Task<AxeResult?> TestAccessibilityAsync(AxeRunOptions? options = null)
        {
            return await _playwrightPage.RunAxe(options);
        }

        /// <summary>
        /// Checks if the provided Playwright page object is equal to the current Playwright page object.
        /// </summary>
        /// <param name="playwrightPage">The Playwright page object to compare.</param>
        /// <returns>True if the page objects are equal, false otherwise.</returns>
        internal bool PlaywrightPageEquals(IPlaywrightPage playwrightPage)
        {
            return _playwrightPage == playwrightPage;
        }

        /// <summary>
        /// Checks if the specified element locator is supported.
        /// </summary>
        /// <param name="locateBy">The element locator to check.</param>
        /// <returns>True if the locator is supported, false otherwise.</returns>
        public static bool IsElementLocatorSupported(eLocateBy locateBy)
        {
            return SupportedElementLocators.Contains(locateBy);
        }

        /// <summary>
        /// Checks if the specified frame locator is supported.
        /// </summary>
        /// <param name="locateBy">The frame locator to check.</param>
        /// <returns>True if the locator is supported, false otherwise.</returns>
        public static bool IsFrameLocatorSupported(eLocateBy locateBy)
        {
            return SupportedFrameLocators.Contains(locateBy);
        }
        /// <summary>
        /// This asynchronous method initiates the process of capturing network logs for the current page. 
        /// It sets up the necessary data structures and event handlers to monitor network requests and responses.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public async Task StartCaptureNetworkLog(ActBrowserElement act)
        {
            _act = act;
            _BrowserHelper = new BrowserHelper(act);
            try
            {
                networkRequestLogList = [];
                networkResponseLogList = [];
                _playwrightPage.Request += OnNetworkRequestSent;
                _playwrightPage.Response += OnNetworkResponseReceived;
                isNetworkLogMonitoringStarted = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

        }
        /// <summary>
        /// This asynchronous method retrieves the captured network logs (requests and responses) for the current browser element and stores them in the act object.
        /// It only performs the action if network log monitoring has been started.
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public async Task GetCaptureNetworkLog(ActBrowserElement act)
        {
            _act = act;
            _BrowserHelper = new BrowserHelper(act);
            try
            {
                await Task.Run(() =>
                {
                    if (isNetworkLogMonitoringStarted)
                    {
                        act.AddOrUpdateReturnParamActual("Raw Request", Newtonsoft.Json.JsonConvert.SerializeObject(networkRequestLogList.Select(x => x.Item2).ToList(), Formatting.Indented));
                        act.AddOrUpdateReturnParamActual("Raw Response", Newtonsoft.Json.JsonConvert.SerializeObject(networkResponseLogList.Select(x => x.Item2).ToList(), Formatting.Indented));
                        foreach (var val in networkRequestLogList.ToList())
                        {
                            act.AddOrUpdateReturnParamActual($"{act.ControlAction.ToString()} {val.Item1}", Convert.ToString(val.Item2));
                        }

                        foreach (var val in networkResponseLogList.ToList())
                        {
                            act.AddOrUpdateReturnParamActual($"{act.ControlAction.ToString()} {val.Item1}", Convert.ToString(val.Item2));
                        }
                    }
                    else
                    {
                        act.ExInfo = $"Action is skipped,{nameof(ActBrowserElement.eControlAction.StartMonitoringNetworkLog)} Action is not started";
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
        /// <summary>
        /// This asynchronous method stops the network log capture by unsubscribing from the network request and response events. 
        /// It then processes and stores the captured logs, saves them to files, and attaches the files as artifacts to the act object.
        /// </summary>
        /// <param name="act"></param>

        public async Task StopCaptureNetworkLog(ActBrowserElement act)
        {
            _act = act;
            _BrowserHelper = new BrowserHelper(act);
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {

                        if (networkRequestLogList.Count != networkResponseLogList.Count)
                        {
                            int timeout = 60;
                            Stopwatch st = Stopwatch.StartNew();
                            if (act.Timeout is not null && act.Timeout != 0)
                            {
                                timeout = act.Timeout.Value;
                            }
                            st.Start();
                            while (timeout > st.Elapsed.TotalSeconds)
                            {
                                if (networkRequestLogList.Count == networkResponseLogList.Count)
                                {
                                    break;
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            st.Stop();
                        }

                        isNetworkLogMonitoringStarted = false;
                        act.AddOrUpdateReturnParamActual("Raw Request", Newtonsoft.Json.JsonConvert.SerializeObject(networkRequestLogList.Select(x => x.Item2).ToList()));
                        act.AddOrUpdateReturnParamActual("Raw Response", Newtonsoft.Json.JsonConvert.SerializeObject(networkResponseLogList.Select(x => x.Item2).ToList()));
                        foreach (var val in networkRequestLogList)
                        {
                            act.AddOrUpdateReturnParamActual($"{act.ControlAction.ToString()} {val.Item1}", Convert.ToString(val.Item2));
                        }
                        foreach (var val in networkResponseLogList)
                        {
                            act.AddOrUpdateReturnParamActual($"{act.ControlAction.ToString()} {val.Item1}", Convert.ToString(val.Item2));
                        }
                        string requestPath = _BrowserHelper.CreateNetworkLogFile("NetworklogRequest", networkRequestLogList);
                        act.ExInfo = $"RequestFile : {requestPath}\n";
                        string responsePath = _BrowserHelper.CreateNetworkLogFile("NetworklogResponse", networkResponseLogList);
                        act.ExInfo = $"{act.ExInfo} ResponseFile : {responsePath}\n";

                        act.AddOrUpdateReturnParamActual("RequestFile", requestPath);
                        act.AddOrUpdateReturnParamActual("ResponseFile", responsePath);

                        Act.AddArtifactToAction(Path.GetFileName(requestPath), act, requestPath);

                        Act.AddArtifactToAction(Path.GetFileName(responsePath), act, responsePath);
                    }


                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            finally
            {
                DetachEvents();
            }

        }

        private void DetachEvents()
        {
            try
            {
                _playwrightPage.Request -= OnNetworkRequestSent;
                _playwrightPage.Response -= OnNetworkResponseReceived;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

        }


        /// <summary>
        /// This method handles the network request event, capturing the details of outgoing network requests. 
        /// It adds the request URL and its serialized data to the network log if the request matches the specified criteria.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="request"></param>
        private async void OnNetworkRequestSent(object? sender, IRequest request)
        {
            try
            {
                if (_BrowserHelper.ShouldMonitorAllUrls() || _BrowserHelper.ShouldMonitorUrl(request.Url))
                {
                    if (_act.GetOrCreateInputParam(nameof(ActBrowserElement.eRequestTypes)).Value == ActBrowserElement.eRequestTypes.FetchOrXHR.ToString())
                    {
                        if (request.ResourceType.Equals("XHR", StringComparison.CurrentCultureIgnoreCase) || request.ResourceType.Equals("FETCH", StringComparison.CurrentCultureIgnoreCase))
                        {
                            networkRequestLogList.Add(new Tuple<string, object>($"RequestUrl:{request.Url}", JsonConvert.SerializeObject(request, Formatting.Indented,
                                                                                                                                new JsonSerializerSettings
                                                                                                                                {
                                                                                                                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                                                                                })));
                        }
                    }
                    else
                    {
                        networkRequestLogList.Add(new Tuple<string, object>($"RequestUrl:{request.Url}", JsonConvert.SerializeObject(request, Formatting.Indented,
                                                                                                                                new JsonSerializerSettings
                                                                                                                                {
                                                                                                                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                                                                                })));
                    }
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
        /// <summary>
        /// This method handles the network response event, capturing details about the response once it's received. 
        /// It checks if the response should be logged based on the configured criteria and adds relevant information to the network response log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="response"></param>
        private async void OnNetworkResponseReceived(object? sender, IResponse response)
        {
            try
            {
                await response.FinishedAsync();
                if (response != null)
                {
                    if (_BrowserHelper.ShouldMonitorAllUrls() || _BrowserHelper.ShouldMonitorUrl(response.Url))
                    {
                        if (_act.GetOrCreateInputParam(nameof(ActBrowserElement.eRequestTypes)).Value == ActBrowserElement.eRequestTypes.FetchOrXHR.ToString())
                        {
                            if (response.Request.ResourceType.Equals("XHR", StringComparison.CurrentCultureIgnoreCase) || response.Request.ResourceType.Equals("FETCH", StringComparison.CurrentCultureIgnoreCase))
                            {
                                networkResponseLogList.Add(new Tuple<string, object>($"ResponseUrl:{response.Url}", JsonConvert.SerializeObject(response, Formatting.Indented,
                                                                                                                                    new JsonSerializerSettings
                                                                                                                                    {
                                                                                                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                                                                                    })
                                ));
                            }
                        }
                        else
                        {
                            networkResponseLogList.Add(new Tuple<string, object>($"ResponseUrl:{response.Url}", JsonConvert.SerializeObject(response, Formatting.Indented,
                                                                                                                                    new JsonSerializerSettings
                                                                                                                                    {
                                                                                                                                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                                                                                    })
                            ));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }





        private async void OnPlaywrightDialog(object? sender, IDialog e)
        {
            try
            {
                if (isDialogDismiss)
                {
                    await e.DismissAsync();
                }
                else
                {
                    dialogs = e;
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
        /// <summary>
        /// This asynchronous method accepts (clicks the "OK" or equivalent) on a message box or dialog.
        /// </summary>
        /// <returns></returns>
        public async Task AcceptMessageBox()
        {
            try
            {
                if (dialogs != null)
                {
                    await dialogs.AcceptAsync();
                    isDialogDismiss = true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "No dialog to accept.");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

        }
        /// <summary>
        /// This asynchronous method dismisses (closes or cancels) a message box or dialog.
        /// </summary>
        /// <returns></returns>
        public async Task DismissMessageBox()
        {
            try
            {
                if (dialogs != null)
                {
                    await dialogs.DismissAsync();
                    isDialogDismiss = true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "No dialog to dismiss.");
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

        }
        /// <summary>
        /// This method retrieves the text content of the current message box or dialog.
        /// </summary>
        /// <returns></returns>
        public string GetMessageBoxText()
        {
            try
            {
                if (dialogs != null)
                {
                    return dialogs.Message;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "No dialog to get message.");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return "Error While Get Message Box Text";
            }

        }
        /// <summary>
        /// This asynchronous method sets the text (usually for a prompt dialog) in the message box.
        /// </summary>
        /// <param name="MessageBoxText"></param>
        /// <returns></returns>
        public async Task SetMessageBoxText(string MessageBoxText)
        {
            try
            {
                if (dialogs != null)
                {
                    await dialogs.AcceptAsync(promptText: MessageBoxText);
                    isDialogDismiss = true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "No dialog to accept.");

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error While Accept Message", ex);
            }

        }

        public async Task StartListenDialogsAsync()
        {
            isDialogDismiss = false;
        }

    }

}
