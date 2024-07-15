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

using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Task ClickAsync(int x, int y);

        public Task DoubleClickAsync();

        public Task DoubleClickAsync(int x, int y);

        public Task HoverAsync();

        public Task<bool> IsVisibleAsync();

        public Task<bool> IsEnabledAsync();

        public Task<string> AttributeValueAsync(string name);

        public Task<IEnumerable<KeyValuePair<string, string>>> AttributesAsync();

        public Task SetAttributeValueAsync(string name, string value);

        public Task<string> TextContentAsync();

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

        public Task<byte[]> ScreenshotAsync();

        public Task<IBrowserShadowRoot?> ShadowRootAsync();
    }
}
