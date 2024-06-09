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

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserElement : IBrowserElement
    {
        private readonly IPlaywrightLocator _locator;

        internal PlaywrightBrowserElement(IPlaywrightLocator locator)
        {
            _locator = locator;
        }

        public Task ClickAsync()
        {
            return _locator.ClickAsync(new LocatorClickOptions
            {
                Button = MouseButton.Left
            });
        }

        public Task ClickAsync(int x, int y)
        {
            return _locator.ClickAsync(new LocatorClickOptions
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
            return _locator.DblClickAsync(new LocatorDblClickOptions()
            {
                Button = MouseButton.Left
            });
        }

        public Task DoubleClickAsync(int x, int y)
        {
            return _locator.DblClickAsync(new LocatorDblClickOptions
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
            return _locator.HoverAsync();
        }

        public Task<bool> IsVisibleAsync()
        {
            return _locator.IsVisibleAsync();
        }

        public Task<bool> IsEnabledAsync()
        {
            return _locator.IsEnabledAsync();
        }

        public async Task<string> AttributeValueAsync(string name)
        {
            string? attributeValue = await _locator.GetAttributeAsync(name);
            if (attributeValue == null)
            {
                return string.Empty;
            }

            return attributeValue;
        }

        public Task SetAttributeValueAsync(string name, string value)
        {
            return _locator.EvaluateAsync<string>($"element => element.setAttribute('{name}', '{value}')");
        }

        public async Task<Size> SizeAsync()
        {
            LocatorBoundingBoxResult? boundingBox = await _locator.BoundingBoxAsync();
            if (boundingBox == null)
            {
                return new Size(width: 0, height: 0);
            }

            return new Size((int)boundingBox.Width, (int)boundingBox.Height);
        }

        public async Task<string> TextContentAsync()
        {
            string? content = await _locator.TextContentAsync();
            if (content == null)
            {
                return string.Empty;
            }

            return content;
        }

        public Task<string> ExecuteJavascriptAsync(string script)
        {
            return _locator.EvaluateAsync<string>(script);
        }

        public Task<string> InnerTextAsync()
        {
            return _locator.InnerTextAsync();
        }

        public Task<string> InputValueAsync()
        {
            return _locator.InputValueAsync();
        }

        public Task RightClickAsync()
        {
            return _locator.ClickAsync(new LocatorClickOptions()
            {
                Button = MouseButton.Right
            });
        }

        public Task<string> TagNameAsync()
        {
            return _locator.EvaluateAsync<string>("elem => elem.tagName");
        }

        public Task ScrollToViewAsync()
        {
            return _locator.ScrollIntoViewIfNeededAsync();
        }

        public Task FocusAsync()
        {
            return _locator.FocusAsync();
        }

        public Task ClearAsync()
        {
            return _locator.ClearAsync();
        }

        public async Task SelectByValueAsync(string value)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _locator.SelectOptionAsync(new SelectOptionValue()
            {
                Value = value
            });
        }

        public async Task SelectByTextAsync(string text)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _locator.SelectOptionAsync(new SelectOptionValue()
            {
                Label = text
            });
        }

        public async Task SelectByIndexAsync(int index)
        {
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await _locator.SelectOptionAsync(new SelectOptionValue()
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
            return _locator.FillAsync(text);
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
    }
}
