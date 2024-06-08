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

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserTab : IBrowserTab
    {
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

        public Task<string> GetPageSourceAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.ContentAsync();
        }

        public Task<string> GetTitleAsync()
        {
            ThrowIfClosed();
            return _playwrightPage.TitleAsync();
        }

        public Task<string> GetURLAsync()
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
            return _playwrightPage.WaitForLoadStateAsync(LoadState.Load);
        }

        public Task<string> GetConsoleLogsAsync()
        {
            return Task.FromResult(string.Join('\n', _consoleMessages));
        }

        public async Task<string> GetBrowserLogsAsync()
        {
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
                case eLocateBy.ByUrl:
                    frameLocator = _currentFrame.FrameLocator($"css=iframe[src='{value}']");
                    break;
                default:
                    throw new ArgumentException($"Frame locator '{locateBy}' is not supported for frames.");
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
            _currentFrame = _playwrightPage.MainFrame;
            return Task.CompletedTask;
        }

        public Task SwitchToParentFrameAsync()
        {
            IFrame? parentFrame = _currentFrame.ParentFrame;
            if (parentFrame != null)
            {
                _currentFrame = parentFrame;
            }

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<IBrowserElement>> GetElementsAsync(eLocateBy locateBy, string value)
        {
            IPlaywrightLocator locator = await LocateElementAsync(locateBy, value);

            int matchedElementCount = await locator.CountAsync();
            IBrowserElement[] elements = new IBrowserElement[matchedElementCount];

            for (int index = 0; index < matchedElementCount; index++)
            {
                elements[index] = new PlaywrightBrowserElement(locator.Nth(index));
            }

            return elements;
        }

        private Task<IPlaywrightLocator> LocateElementAsync(eLocateBy locateBy, string value)
        {
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
                    throw new ArgumentException($"Element locator '{locateBy}' is not supported.");
            }

            return Task.FromResult(locator);
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
            await _onTabClose.Invoke(closedTab: this);
        }

        private void ThrowIfClosed()
        {
            if (_isClosed)
            {
                throw new InvalidOperationException("Cannot perform operation, tab is already closed.");
            }
        }
    }

}
