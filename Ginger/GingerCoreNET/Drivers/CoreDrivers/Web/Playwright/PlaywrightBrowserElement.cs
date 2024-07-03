using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightBrowser = Microsoft.Playwright.IBrowser;
using IPlaywrightBrowserContext = Microsoft.Playwright.IBrowserContext;
using IPlaywrightPage = Microsoft.Playwright.IPage;
using IPlaywrightDialog = Microsoft.Playwright.IDialog;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;
using System.Drawing;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserElement : IBrowserElement
    {
        //TODO: rename it to _playwrightLocator to maintain the naming format
        private readonly IPlaywrightLocator _playwrightLocator;

        internal PlaywrightBrowserElement(IPlaywrightLocator locator)
        {
            _playwrightLocator = locator;
        }

        public Task ClickAsync()
        {
            return _playwrightLocator.ClickAsync(new LocatorClickOptions
            {
                Button = MouseButton.Left
            });
        }

        public Task ClickAsync(int x, int y)
        {
            return _playwrightLocator.ClickAsync(new LocatorClickOptions
            {
                Button = MouseButton.Left,
                Position = new()
                {
                    X = x,
                    Y = y
                }
            });
        }

        public Task DoubleClickAsync()
        {
            return _playwrightLocator.DblClickAsync(new LocatorDblClickOptions()
            {
                Button = MouseButton.Left
            });
        }

        public Task DoubleClickAsync(int x, int y)
        {
            return _playwrightLocator.DblClickAsync(new LocatorDblClickOptions
            {
                Button = MouseButton.Left,
                Position = new()
                {
                    X = x,
                    Y = y
                }
            });
        }

        public Task HoverAsync()
        {
            return _playwrightLocator.HoverAsync();
        }

        public Task<bool> IsVisibleAsync()
        {
            return _playwrightLocator.IsVisibleAsync();
        }

        public Task<bool> IsEnabledAsync()
        {
            return _playwrightLocator.IsEnabledAsync();
        }

        public async Task<string> AttributeValueAsync(string name)
        {
            string? attributeValue = await _playwrightLocator.GetAttributeAsync(name);
            if (attributeValue == null)
            {
                return string.Empty;
            }

            return attributeValue;
        }

        public Task SetAttributeValueAsync(string name, string value)
        {
            return _playwrightLocator.EvaluateAsync<string>($"element => element.setAttribute('{name}', '{value}')");
        }

        public async Task<Size> SizeAsync()
        {
            LocatorBoundingBoxResult? boundingBox = await _playwrightLocator.BoundingBoxAsync();
            if (boundingBox == null)
            {
                return new Size(width: 0, height: 0);
            }

            return new Size((int)boundingBox.Width, (int)boundingBox.Height);
        }

        public async Task<Point> PositionAsync()
        {
            string rect = await _playwrightLocator.EvaluateAsync<string>("element => element.getBoundingClientRect().x + 'x' + element.getBoundingClientRect().y");
            if (string.IsNullOrEmpty(rect))
            {
                throw new Exception("Unable to get element position");
            }
            
            string[] coordinates = rect.Split('x');
            if (coordinates.Length != 2)
            {
                throw new Exception("Unable to get element position");
            }

            if (!double.TryParse(coordinates[0], out double x))
            {
                throw new Exception($"Unable to get element position, invalid x coordinate ");
            }
            if (!double.TryParse(coordinates[1], out double y))
            {
                throw new Exception($"Unable to get element position, invalid y coordinate {coordinates[1]}");
            }

            return new Point((int)x, (int)y);
        }

        public async Task<string> TextContentAsync()
        {
            string? content = await _playwrightLocator.TextContentAsync();
            if (content == null)
            {
                return string.Empty;
            }

            return content;
        }

        public Task<string> ExecuteJavascriptAsync(string script)
        {
            return _playwrightLocator.EvaluateAsync<string>(script);
        }

        public Task<string> InnerTextAsync()
        {
            return _playwrightLocator.InnerTextAsync();
        }

        public Task<string> InputValueAsync()
        {
            return _playwrightLocator.InputValueAsync();
        }

        public Task RightClickAsync()
        {
            return _playwrightLocator.ClickAsync(new LocatorClickOptions()
            {
                Button = MouseButton.Right
            });
        }

        public Task<string> TagNameAsync()
        {
            return _playwrightLocator.EvaluateAsync<string>("elem => elem.tagName");
        }

        public Task ScrollToViewAsync()
        {
            return _playwrightLocator.ScrollIntoViewIfNeededAsync();
        }

        public Task FocusAsync()
        {
            return _playwrightLocator.FocusAsync();
        }

        public Task ClearAsync()
        {
            return _playwrightLocator.ClearAsync();
        }

        public async Task SelectByValueAsync(string value)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Value = value
            });
        }

        public async Task SelectByTextAsync(string text)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Label = text
            });
        }

        public async Task SelectByIndexAsync(int index)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Index = index
            });
        }

        public async Task SetCheckboxAsync(bool check)
        {
            await AssertTagNameAsync(IBrowserElement.InputTagName);
            await AssertTypeAttributeAsync("checkbox");

            await ExecuteJavascriptAsync($"element => element.checked={check.ToString().ToLower()}");
        }

        public Task SetTextAsync(string text)
        {
            return _playwrightLocator.FillAsync(text);
        }

        private async Task AssertTagNameAsync(string expected)
        {
            string tagName = await TagNameAsync();
            if (!string.Equals(tagName, expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected '{expected}' element but found '{tagName}'");
            }
        }

        public async Task AssertTypeAttributeAsync(string expected)
        {
            string type = await AttributeValueAsync(name: "type");
            if (!string.Equals(type, expected, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Expected '{expected}' type but found '{type}'");
            }
        }

        public Task<byte[]> ScreenshotAsync()
        {
            return _playwrightLocator.ScreenshotAsync();
        }

        public async Task<IBrowserShadowRoot?> ShadowRootAsync()
        {
            if (!await ShadowRootExists())
            {
                return null;
            }

            return new PlaywrightBrowserShadowRoot(_playwrightLocator);
        }

        private Task<bool> ShadowRootExists()
        {
            return _playwrightLocator.EvaluateAsync<bool>("element => element.shadowRoot != null");
        }
    }
}
