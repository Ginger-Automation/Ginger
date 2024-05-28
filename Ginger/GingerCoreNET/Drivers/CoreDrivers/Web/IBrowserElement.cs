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

        public Task<string> GetTagNameAsync();

        public Task<Size> GetSizeAsync();

        public Task ClickAsync();

        public Task DoubleClickAsync();

        public Task HoverAsync();

        public Task<bool> IsVisibleAsync();

        public Task<bool> IsEnabledAsync();

        public Task<string> GetAttributeValueAsync(string attributeName);

        public Task<string> GetTextContentAsync();

        public Task<string> GetInnerTextAsync();

        public Task<string> GetInputValueAsync();

        public Task RightClickAsync();

        public Task<string> ExecuteJavascriptAsync(string script);

        public Task ScrollToViewAsync();
    }
}
