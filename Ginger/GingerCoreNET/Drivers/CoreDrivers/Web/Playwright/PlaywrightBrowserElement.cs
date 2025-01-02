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

using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPlaywrightElementHandle = Microsoft.Playwright.IElementHandle;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright
{
    internal sealed class PlaywrightBrowserElement : IBrowserElement
    {
        private readonly IPlaywrightLocator? _playwrightLocator;
        private readonly IPlaywrightElementHandle? _playwrightElementHandle;

        internal PlaywrightBrowserElement(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            _playwrightLocator = playwrightLocator;
        }

        internal PlaywrightBrowserElement(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle);
            _playwrightElementHandle = playwrightElementHandle;
        }

        public Task ClickAsync()
        {
            if (_playwrightLocator != null)
            {
                return ClickAsync(_playwrightLocator);
            }
            else

            {
                return ClickAsync(_playwrightElementHandle!);
            }
        }

        private Task ClickAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ClickAsync(new LocatorClickOptions
            {
                Button = MouseButton.Left
            });
        }

        private Task ClickAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.ClickAsync(new ElementHandleClickOptions
            {
                Button = MouseButton.Left
            });
        }

        public Task ClickAsync(Point point)
        {
            if (_playwrightLocator != null)
            {
                return ClickAsync(_playwrightLocator, point.X, point.Y);
            }
            else
            {
                return ClickAsync(_playwrightElementHandle!, point.X, point.Y);
            }
        }

        public Task ClickAsync(IPlaywrightLocator playwrightLocator, int x, int y)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ClickAsync(new LocatorClickOptions
            {
                Button = MouseButton.Left,
                Position = new()
                {
                    X = x,
                    Y = y
                }
            });
        }

        public Task ClickAsync(IPlaywrightElementHandle playwrightElementHandle, int x, int y)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.ClickAsync(new ElementHandleClickOptions
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
            if (_playwrightLocator != null)
            {
                return DoubleClickAsync(_playwrightLocator);
            }
            else
            {
                return DoubleClickAsync(_playwrightElementHandle!);
            }
        }

        public Task DoubleClickAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.DblClickAsync(new LocatorDblClickOptions()
            {
                Button = MouseButton.Left
            });
        }

        public Task DoubleClickAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.DblClickAsync(new ElementHandleDblClickOptions()
            {
                Button = MouseButton.Left
            });
        }

        public Task DoubleClickAsync(int x, int y)
        {
            if (_playwrightLocator != null)
            {
                return DoubleClickAsync(_playwrightLocator, x, y);
            }
            else
            {
                return DoubleClickAsync(_playwrightElementHandle!, x, y);
            }
        }

        public Task DoubleClickAsync(IPlaywrightLocator playwrightLocator, int x, int y)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.DblClickAsync(new LocatorDblClickOptions
            {
                Button = MouseButton.Left,
                Position = new()
                {
                    X = x,
                    Y = y
                }
            });
        }

        public Task DoubleClickAsync(IPlaywrightElementHandle playwrightElementHandle, int x, int y)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.DblClickAsync(new ElementHandleDblClickOptions
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
            if (_playwrightLocator != null)
            {
                return HoverAsync(_playwrightLocator);
            }
            else
            {
                return HoverAsync(_playwrightElementHandle!);
            }
        }

        public Task HoverAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.HoverAsync();
        }

        public Task HoverAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.HoverAsync();
        }

        public Task<bool> IsVisibleAsync()
        {
            if (_playwrightLocator != null)
            {
                return IsVisibleAsync(_playwrightLocator);
            }
            else
            {
                return IsVisibleAsync(_playwrightElementHandle!);
            }
        }

        public Task<bool> IsVisibleAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.IsVisibleAsync();
        }

        public Task<bool> IsVisibleAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.IsVisibleAsync();
        }

        public Task<bool> IsEnabledAsync()
        {
            if (_playwrightLocator != null)
            {
                return IsEnabledAsync(_playwrightLocator);
            }
            else
            {
                return IsEnabledAsync(_playwrightElementHandle!);
            }
        }

        public Task<bool> IsEnabledAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.IsEnabledAsync();
        }

        public Task<bool> IsEnabledAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.IsEnabledAsync();
        }

        public Task<string> AttributeValueAsync(string name)
        {
            if (_playwrightLocator != null)
            {
                return AttributeValueAsync(_playwrightLocator, name);
            }
            else
            {
                return AttributeValueAsync(_playwrightElementHandle!, name);
            }
        }

        public async Task<string> AttributeValueAsync(IPlaywrightLocator playwrightLocator, string name)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            string? attributeValue = await playwrightLocator.GetAttributeAsync(name);
            if (attributeValue == null)
            {
                return string.Empty;
            }

            return attributeValue;
        }

        public async Task<string> AttributeValueAsync(IPlaywrightElementHandle playwrightElementHandle, string name)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            string? attributeValue = await playwrightElementHandle.GetAttributeAsync(name);
            if (attributeValue == null)
            {
                return string.Empty;
            }

            return attributeValue;
        }

        public async Task<IEnumerable<KeyValuePair<string, string>>> AttributesAsync()
        {
            string script =
                @"element => {
                    let attributes = [];
                    for(let i = 0; i < element.attributes.length; i++) {
                        let attribute = element.attributes[i];
                        attributes.push([
                            attribute.nodeName,
                            attribute.nodeValue,
                        ]);
                    }
                    return attributes;
                }";
            if (_playwrightLocator != null)
            {
                string[][] nameValuePairs = await _playwrightLocator.EvaluateAsync<string[][]>(script);
                if (nameValuePairs != null)
                {
                    return nameValuePairs.Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]));
                }
            }
            else
            {
                string[][] nameValuePairs = await _playwrightElementHandle!.EvaluateAsync<string[][]>(script);
                if (nameValuePairs != null)
                {
                    return nameValuePairs.Select(pair => new KeyValuePair<string, string>(pair[0], pair[1]));
                }
            }
            return [];
        }

        public Task SetAttributeValueAsync(string name, string value)
        {
            if (_playwrightLocator != null)
            {
                return SetAttributeValueAsync(_playwrightLocator, name, value);
            }
            else
            {
                return SetAttributeValueAsync(_playwrightElementHandle!, name, value);
            }
        }

        public Task SetAttributeValueAsync(IPlaywrightElementHandle playwrightElementHandle, string name, string value)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.EvaluateAsync<string>($"element => element.setAttribute('{name}', '{value}')");
        }

        public Task SetAttributeValueAsync(IPlaywrightLocator playwrightLocator, string name, string value)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.EvaluateAsync<string>($"element => element.setAttribute('{name}', '{value}')");
        }

        public Task<Size> SizeAsync()
        {
            if (_playwrightLocator != null)
            {
                return SizeAsync(_playwrightLocator);
            }
            else
            {
                return SizeAsync(_playwrightElementHandle!);
            }
        }

        public async Task<Size> SizeAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            LocatorBoundingBoxResult? boundingBox = await playwrightLocator.BoundingBoxAsync();
            if (boundingBox == null)
            {
                return new Size(width: 0, height: 0);
            }

            return new Size((int)boundingBox.Width, (int)boundingBox.Height);
        }

        public async Task<Size> SizeAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            ElementHandleBoundingBoxResult? boundingBox = await playwrightElementHandle.BoundingBoxAsync();
            if (boundingBox == null)
            {
                return new Size(width: 0, height: 0);
            }

            return new Size((int)boundingBox.Width, (int)boundingBox.Height);
        }

        public Task<Point> PositionAsync()
        {
            if (_playwrightLocator != null)
            {
                return PositionAsync(_playwrightLocator);
            }
            else
            {
                return PositionAsync(_playwrightElementHandle!);
            }
        }

        public async Task<Point> PositionAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            string rect = await playwrightLocator.EvaluateAsync<string>("element => element.getBoundingClientRect().x + 'x' + element.getBoundingClientRect().y");
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

        public async Task<Point> PositionAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            string rect = await playwrightElementHandle.EvaluateAsync<string>("element => element.getBoundingClientRect().x + 'x' + element.getBoundingClientRect().y");
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

        public Task<string> TextContentAsync()
        {
            if (_playwrightLocator != null)
            {
                return TextContentAsync(_playwrightLocator);
            }
            else
            {
                return TextContentAsync(_playwrightElementHandle!);
            }
        }

        public async Task<string> TextContentAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            string? content = await playwrightLocator.TextContentAsync();
            if (content == null)
            {
                return string.Empty;
            }

            return content;
        }

        public async Task<string> TextContentAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            string? content = await playwrightElementHandle.TextContentAsync();
            if (content == null)
            {
                return string.Empty;
            }

            return content;
        }

        public Task<string> ExecuteJavascriptAsync(string script)
        {
            if (_playwrightLocator != null)
            {
                return ExecuteJavascriptAsync(_playwrightLocator, script);
            }
            else
            {
                return ExecuteJavascriptAsync(_playwrightElementHandle!, script);
            }
        }

        public Task<string> ExecuteJavascriptAsync(IPlaywrightLocator playwrightLocator, string script)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.EvaluateAsync<string>(script);
        }

        public Task<string> ExecuteJavascriptAsync(IPlaywrightElementHandle playwrightElementHandle, string script)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.EvaluateAsync<string>(script);
        }

        public Task<string> InnerTextAsync()
        {
            if (_playwrightLocator != null)
            {
                return InnerTextAsync(_playwrightLocator);
            }
            else
            {
                return InnerTextAsync(_playwrightElementHandle!);
            }
        }

        public Task<string> InnerTextAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.InnerTextAsync();
        }

        public Task<string> InnerTextAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.InnerTextAsync();
        }

        public Task<string> InputValueAsync()
        {
            if (_playwrightLocator != null)
            {
                return InputValueAsync(_playwrightLocator);
            }
            else
            {
                return InputValueAsync(_playwrightElementHandle!);
            }
        }

        public Task<string> InputValueAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.InputValueAsync();
        }

        public Task<string> InputValueAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.InputValueAsync();
        }

        public Task RightClickAsync()
        {
            if (_playwrightLocator != null)
            {
                return RightClickAsync(_playwrightLocator);
            }
            else
            {
                return RightClickAsync(_playwrightElementHandle!);
            }
        }

        public Task RightClickAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ClickAsync(new LocatorClickOptions()
            {
                Button = MouseButton.Right
            });
        }

        public Task RightClickAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.ClickAsync(new ElementHandleClickOptions()
            {
                Button = MouseButton.Right
            });
        }

        public Task<string> TagNameAsync()
        {
            if (_playwrightLocator != null)
            {
                return TagNameAsync(_playwrightLocator);
            }
            else
            {
                return TagNameAsync(_playwrightElementHandle!);
            }
        }

        public Task<string> TagNameAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.EvaluateAsync<string>("elem => elem.tagName");
        }

        public Task<string> TagNameAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.EvaluateAsync<string>("elem => elem.tagName");
        }

        public Task ScrollToViewAsync()
        {
            if (_playwrightLocator != null)
            {
                return ScrollToViewAsync(_playwrightLocator);
            }
            else
            {
                return ScrollToViewAsync(_playwrightElementHandle!);
            }
        }

        public Task ScrollToViewAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ScrollIntoViewIfNeededAsync();
        }

        public Task ScrollToViewAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.ScrollIntoViewIfNeededAsync();
        }

        public Task FocusAsync()
        {
            if (_playwrightLocator != null)
            {
                return FocusAsync(_playwrightLocator);
            }
            else
            {
                return FocusAsync(_playwrightElementHandle!);
            }
        }

        public Task FocusAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.FocusAsync();
        }

        public Task FocusAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.FocusAsync();
        }

        public Task ClearAsync()
        {
            if (_playwrightLocator != null)
            {
                return ClearAsync(_playwrightLocator);
            }
            else
            {
                return ClearAsync(_playwrightElementHandle!);
            }
        }

        public Task ClearAsync(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ClearAsync();
        }

        public Task ClearAsync(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.EvaluateAsync("element => element.setAttribute('value', '')");
        }

        public Task SelectByValueAsync(string value)
        {
            if (_playwrightLocator != null)
            {
                return SelectByValueAsync(_playwrightLocator, value);
            }
            else
            {
                return SelectByValueAsync(_playwrightElementHandle!, value);
            }
        }

        public async Task SelectByValueAsync(IPlaywrightLocator playwrightLocator, string value)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Value = value
            });
        }

        public async Task SelectByValueAsync(IPlaywrightElementHandle playwrightElementHandle, string value)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightElementHandle.SelectOptionAsync(new SelectOptionValue()
            {
                Value = value
            });
        }

        public Task SelectByTextAsync(string text)
        {
            if (_playwrightLocator != null)
            {
                return SelectByTextAsync(_playwrightLocator, text);
            }
            else
            {
                return SelectByTextAsync(_playwrightElementHandle!, text);
            }
        }

        public async Task SelectByTextAsync(IPlaywrightLocator playwrightLocator, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Label = text
            });
        }

        public async Task SelectByTextAsync(IPlaywrightElementHandle playwrightElementHandle, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightElementHandle.SelectOptionAsync(new SelectOptionValue()
            {
                Label = text
            });
        }

        public Task SelectByIndexAsync(int index)
        {
            if (_playwrightLocator != null)
            {
                return SelectByIndexAsync(_playwrightLocator, index);
            }
            else
            {
                return SelectByIndexAsync(_playwrightElementHandle!, index);
            }
        }

        public async Task SelectByIndexAsync(IPlaywrightLocator playwrightLocator, int index)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightLocator.SelectOptionAsync(new SelectOptionValue()
            {
                Index = index
            });
        }

        public async Task SelectByIndexAsync(IPlaywrightElementHandle playwrightElementHandle, int index)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            await AssertTagNameAsync(IBrowserElement.SelectTagName);
            await playwrightElementHandle.SelectOptionAsync(new SelectOptionValue()
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
            if (_playwrightLocator != null)
            {
                return SetTextAsync(_playwrightLocator, text);
            }
            else
            {
                return SetTextAsync(_playwrightElementHandle!, text);
            }
        }

        public Task SetTextAsync(IPlaywrightLocator playwrightLocator, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.FillAsync(text);
        }

        public Task SetTextAsync(IPlaywrightElementHandle playwrightElementHandle, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.FillAsync(text);
        }

        public Task TypeTextAsync(string text)
        {
            if (_playwrightLocator != null)
            {
                return SendKeysAsync(_playwrightLocator, text);
            }
            else
            {
                return SendKeysAsync(_playwrightElementHandle!, text);
            }
        }

        private Task SendKeysAsync(IPlaywrightLocator playwrightLocator, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.PressSequentiallyAsync(text);
        }

        private async Task SendKeysAsync(IPlaywrightElementHandle playwrightElementHandle, string text)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            foreach (char c in text)
            {
                await playwrightElementHandle.PressAsync(c.ToString());
            }
        }

        /// <summary>
        /// Presses a sequence of keys asynchronously on the specified Playwright locator or element handle.
        /// If the locator is not null, it uses the locator to press the keys; otherwise, it uses the element handle.
        /// </summary>
        /// <param name="keys">An enumerable collection of keys to be pressed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task PressKeysAsync(IEnumerable<string> keys)
        {
            if (_playwrightLocator != null)
            {
                return PressKeysAsync(_playwrightLocator, keys);
            }
            else
            {
                return PressKeysAsync(_playwrightElementHandle!, keys);
            }
        }

        /// <summary>
        /// Presses a sequence of keys asynchronously on the specified Playwright locator.
        /// Combines the keys into a single string separated by '+' and presses them.
        /// </summary>
        /// <param name="playwrightLocator">The Playwright locator on which to press the keys.</param>
        /// <param name="keys">An enumerable collection of keys to be pressed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task PressKeysAsync(IPlaywrightLocator playwrightLocator, IEnumerable<string> keys)
        {
            string combinedKeys = CombineKeys(keys);
            return playwrightLocator.PressAsync(combinedKeys);
        }

        /// <summary>
        /// Presses a sequence of keys asynchronously on the specified Playwright element handle.
        /// Combines the keys into a single string separated by '+' and presses them.
        /// </summary>
        /// <param name="playwrightElementHandle">The Playwright element handle on which to press the keys.</param>
        /// <param name="keys">An enumerable collection of keys to be pressed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task PressKeysAsync(IPlaywrightElementHandle playwrightElementHandle, IEnumerable<string> keys)
        {
            string combinedKeys = CombineKeys(keys);
            return playwrightElementHandle.PressAsync(combinedKeys);
        }

        /// <summary>
        /// Combines a sequence of keys into a single string separated by '+'.
        /// </summary>
        /// <param name="keys">An enumerable collection of keys to be combined.</param>
        /// <returns>A string representing the combined keys.</returns>
        private string CombineKeys(IEnumerable<string> keys)
        {
            string[] keyArr = keys.ToArray();
            StringBuilder combinedKey = new();
            for (int keyIndex = 0; keyIndex < keyArr.Length; keyIndex++)
            {
                string key = keyArr[keyIndex];
                combinedKey.Append(key);
                if (keyIndex < keyArr.Length - 1)
                {
                    combinedKey.Append('+');
                }
            }
            return combinedKey.ToString();
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
            int timeout = 3000;
            if (_playwrightLocator != null)
            {
                return ScreenshotAsync(_playwrightLocator, timeout);
            }
            else
            {
                return ScreenshotAsync(_playwrightElementHandle!, timeout);
            }
        }

        public Task<byte[]> ScreenshotAsync(IPlaywrightLocator playwrightLocator, int timeout)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.ScreenshotAsync(new LocatorScreenshotOptions()
            {
                Timeout = timeout,
            });
        }

        public Task<byte[]> ScreenshotAsync(IPlaywrightElementHandle playwrightElementHandle, int timeout)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.ScreenshotAsync(new ElementHandleScreenshotOptions()
            {
                Timeout = timeout,
            });
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
            if (_playwrightLocator != null)
            {
                return ShadowRootExists(_playwrightLocator);
            }
            else
            {
                return ShadowRootExists(_playwrightElementHandle!);
            }
        }

        private Task<bool> ShadowRootExists(IPlaywrightLocator playwrightLocator)
        {
            ArgumentNullException.ThrowIfNull(playwrightLocator, nameof(playwrightLocator));
            return playwrightLocator.EvaluateAsync<bool>("element => element.shadowRoot != null");
        }

        private Task<bool> ShadowRootExists(IPlaywrightElementHandle playwrightElementHandle)
        {
            ArgumentNullException.ThrowIfNull(playwrightElementHandle, nameof(playwrightElementHandle));
            return playwrightElementHandle.EvaluateAsync<bool>("element => element.shadowRoot != null");
        }

        public async Task<AxeResult?> TestAccessibilityAsync(AxeRunOptions? options = null)
        {
            if (_playwrightLocator == null)
            {
                throw new Exception("Unable to analyze this page");
            }

            return await _playwrightLocator.RunAxe(options);
        }

        /// <summary>
        /// Sets the file value using the appropriate Playwright locator or element handle.
        /// </summary>
        /// <param name="value">The file path to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task SetFileValueAsync(string[] value)
        {
            if (_playwrightLocator != null)
            {
                return SetFileValueAsync(_playwrightLocator, value);
            }
            else
            {
                return SetFileValueAsync(_playwrightElementHandle!, value);
            }
        }

        /// <summary>
        /// Sets the file value using a Playwright locator.
        /// </summary>
        /// <param name="playwrightLocator">The Playwright locator.</param>
        /// <param name="value">The file path to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task SetFileValueAsync(IPlaywrightLocator playwrightLocator, string[] value)
        {
            return playwrightLocator.SetInputFilesAsync(value);
        }

        /// <summary>
        /// Sets the file value using a Playwright element handle.
        /// </summary>
        /// <param name="playwrightElementHandle">The Playwright element handle.</param>
        /// <param name="value">The file path to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private Task SetFileValueAsync(IPlaywrightElementHandle playwrightElementHandle, string[] value)
        {
            return playwrightElementHandle.SetInputFilesAsync(value);
        }
    }
}
