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
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using IPlaywrightLocator = Microsoft.Playwright.ILocator;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserElement
    {
        public const string AnchorTagName = "a";
        public const string SelectTagName = "select";
        public const string InputTagName = "input";
        public const string ButtonTagName = "button";
        public const string TextAreaTagName = "textarea";

        public Task<string> TagNameAsync();

        public Task<Size> SizeAsync();

        public Task<Point> PositionAsync();

        public Task ClickAsync();

        public Task ClickAsync(Point point);

        public Task DoubleClickAsync();

        public Task DoubleClickAsync(int x, int y);

        public Task HoverAsync();

        public Task<bool> IsVisibleAsync();

        public Task<bool> IsEnabledAsync();

        public Task<string> AttributeValueAsync(string name);

        public Task<IEnumerable<KeyValuePair<string, string>>> AttributesAsync();

        public Task SetAttributeValueAsync(string name, string value);

        public Task<string> TextContentAsync();
        public Task<string> GetSelectedValueAsync();

        public Task<string> InnerTextAsync();

        public Task<string> InputValueAsync();

        public Task RightClickAsync();

        public Task<string> ExecuteJavascriptAsync(string script);

        public Task ScrollToViewAsync();

        public Task FocusAsync();

        public Task ClearAsync();

        public Task SelectByValueAsync(string value);

        public Task SelectByTextAsync(string text);

        public Task SelectByIndexAsync(int index);

        public Task SetCheckboxAsync(bool check);

        public Task SetTextAsync(string text);

        public Task TypeTextAsync(string text);

        public Task PressKeysAsync(IEnumerable<string> keys);

        public Task<byte[]> ScreenshotAsync();

        public Task<IBrowserShadowRoot?> ShadowRootAsync();

        public Task<AxeResult?> TestAccessibilityAsync(AxeRunOptions? options = null);
        public Task SetFileValueAsync(string[] value);

        public Task<bool> ToBeVisibleAsync(float timeOut);

        public Task<bool> AttributeMatchesAsync(string attributeName, string attributeValue, float timeOut);

        public Task<bool> TextMatchesAsync(string textToMatch, float timeOut);

        public Task<bool> ElementToBeClickableAsync(float timeOut);

        public Task<bool> ElementIsSelectedAsync(float timeOut);

        public Task<bool> ToBeNotVisibleAsync(float timeOut);
        public Task<string> GetElementLocator();

    }
}
