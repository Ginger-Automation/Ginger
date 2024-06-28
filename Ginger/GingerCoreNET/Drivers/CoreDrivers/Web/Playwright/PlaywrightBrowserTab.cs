using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightPage = Microsoft.Playwright.IPage;
using IPlaywrightDialog = Microsoft.Playwright.IDialog;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;
using System.Drawing;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserTab : IBrowserTab
    {
        private static readonly IEnumerable<eLocateBy> SupportedElementLocators = new List<eLocateBy>()
        {
            eLocateBy.ByID,
            eLocateBy.ByCSS,
            eLocateBy.ByXPath,
            eLocateBy.POMElement,
        };

        private static readonly IEnumerable<eLocateBy> SupportedFrameLocators = new List<eLocateBy>()
        {
            eLocateBy.ByID,
            eLocateBy.ByTitle,
            eLocateBy.ByUrl,
            eLocateBy.ByXPath,
            eLocateBy.ByRelXPath,
        };

        private readonly IPlaywrightPage _playwrightPage;
        private readonly IBrowserTab.OnTabClose _onTabClose;
        private readonly LinkedList<string> _consoleMessages = [];
        private IFrame _currentFrame;
        private bool _isClosed = false;

        public bool IsClosed => _isClosed;

        internal PlaywrightBrowserTab(IPlaywrightPage playwrightPage, IBrowserTab.OnTabClose onTabClose)
        {
            _playwrightPage = playwrightPage;
            _onTabClose = onTabClose;
            _currentFrame = _playwrightPage.MainFrame;
            _playwrightPage.Console += OnConsoleMessage;
            _playwrightPage.Close += OnPlaywrightPageClose;
        }

        private void RemoveEventHandlers()
        {
            _playwrightPage.Console -= OnConsoleMessage;
            _playwrightPage.Close -= OnPlaywrightPageClose;
        }

        private void OnPlaywrightPageClose(object? sender, IPlaywrightPage e)
        {
            _ = CloseAsync();
        }

        private void OnConsoleMessage(object? sender, IConsoleMessage e)
        {
            //TODO: Playwright - Selenium console logs contain timestamp and level. Try adding those in these as well so that we can have similar log structure
            _consoleMessages.AddLast(e.Text);
        }

        public Task BringToFrontAsync()
        {
            return _playwrightPage.BringToFrontAsync();
        }

        public Task<string> ExecuteJavascriptAsync(string script)
        {
            ThrowIfClosed();
            return _playwrightPage.EvaluateAsync<string>(script);
        }

        public Task<string> PageSourceAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.ContentAsync();
        }

        public Task<string> TitleAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.TitleAsync();
        }

        public Task<string> URLAsync()
        {
            ThrowIfClosed();
            return Task.FromResult(_playwrightPage.Url);
        }

        public async Task GoToURLAsync(string url)
        {
            ThrowIfClosed();
            await _playwrightPage.GotoAsync(url);
            _currentFrame = _playwrightPage.MainFrame;
        }

        public Task NavigateBackAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.GoBackAsync();
        }

        public Task NavigateForwardAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.GoForwardAsync();
        }

        public Task RefreshAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.ReloadAsync();
        }

        public Task WaitTillLoadedAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.WaitForLoadStateAsync(LoadState.Load);
        }

        public Task<string> ConsoleLogsAsync()
        {
            ThrowIfClosed();
            return Task.FromResult(string.Join('\n', _consoleMessages));
        }

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

        public async Task<bool> SwitchFrameAsync(eLocateBy locateBy, string value)
        {
            ThrowIfClosed();
            if (!IsFrameLocatorSupported(locateBy))
            {
                throw new LocatorNotSupportedException($"Frame locator '{locateBy}' is not supported.");
            }
            
            IFrameLocator frameLocator;
            switch (locateBy)
            {
                case eLocateBy.ByID:
                    frameLocator = _currentFrame.FrameLocator($"css=#{value}");
                    break;
                case eLocateBy.ByTitle:
                    frameLocator = _currentFrame.FrameLocator($"css=iframe[title='{value}']");
                    break;
                case eLocateBy.ByXPath:
                    frameLocator = _currentFrame.FrameLocator($"xpath={value}");
                    break;
                case eLocateBy.ByRelXPath:
                    frameLocator = _currentFrame.FrameLocator($"xpath={value}");
                    break;
                case eLocateBy.ByUrl:
                    frameLocator = _currentFrame.FrameLocator($"css=iframe[src='{value}']");
                    break;
                default:
                    throw new ArgumentException($"Frame locator '{locateBy}' is not supported.");
            }

            bool wasLocated = await frameLocator.Owner.CountAsync() > 0;
            if (!wasLocated)
            {
                return false;

            }

            IJSHandle jsHandle = await frameLocator.Owner.EvaluateHandleAsync("element => element");
            IElementHandle? elementHandle = jsHandle.AsElement();
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

        public Task SwitchToMainFrameAsync()
        {
            ThrowIfClosed();
            _currentFrame = _playwrightPage.MainFrame;
            return Task.CompletedTask;
        }

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

        public Task<byte[]> ScreenshotAsync()
        {
            return ScreenshotInternalAsync(fullPage: false);
        }

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
                    locator = _currentFrame.Locator($"css=#{value}");
                    break;
                case eLocateBy.ByCSS:
                    locator = _currentFrame.Locator($"css={value}");
                    break;
                case eLocateBy.ByXPath:
                    locator = _currentFrame.Locator($"xpath={value}");
                    break;
                default:
                    throw new LocatorNotSupportedException($"Element locator '{locateBy}' is not supported.");
            }

            return Task.FromResult(locator);
        }

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

        private static async Task<bool> DoesLocatorExistsAsync(IPlaywrightLocator locator)
        {
            return await locator.CountAsync() > 0;
        }

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

        private void ThrowIfClosed()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Cannot perform operation, tab is already closed.");
            }
        }

        internal bool PlaywrightPageEquals(IPlaywrightPage playwrightPage)
        {
            return _playwrightPage == playwrightPage;
        }

        public static bool IsElementLocatorSupported(eLocateBy locateBy)
        {
            return SupportedElementLocators.Contains(locateBy);
        }

        public static bool IsFrameLocatorSupported(eLocateBy locateBy)
        {
            return SupportedFrameLocators.Contains(locateBy);
        }
    }

}
