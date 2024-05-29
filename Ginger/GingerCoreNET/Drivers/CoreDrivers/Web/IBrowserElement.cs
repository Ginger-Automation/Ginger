using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserElement
    {
        public const string AnchorTagName = "a";
        public const string SelectTagName = "select";
        public const string InputTagName = "input";
        public const string ButtonTagName = "button";

        public Task<string> TagNameAsync();

        public Task<Size> SizeAsync();

        public Task ClickAsync();

        public Task ClickAsync(int x, int y);

        public Task DoubleClickAsync();

        public Task DoubleClickAsync(int x, int y);

        public Task HoverAsync();

        public Task<bool> IsVisibleAsync();

        public Task<bool> IsEnabledAsync();

        public Task<string> AttributeValueAsync(string attributeName);

        public Task<string> TextContentAsync();

        public Task<string> InnerTextAsync();

        public Task<string> InputValueAsync();

        public Task RightClickAsync();

        public Task<string> ExecuteJavascriptAsync(string script);

        public Task ScrollToViewAsync();

        public Task FocusAsync();
    }
}
