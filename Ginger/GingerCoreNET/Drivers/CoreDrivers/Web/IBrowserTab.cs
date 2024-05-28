using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserTab
    {
        public delegate Task OnTabClose(IBrowserTab closedTab);

        public bool IsClosed { get; }

        public Task<string> GetURLAsync();

        public Task GoToURLAsync(string url);

        public Task<string> GetTitleAsync();

        public Task NavigateBackAsync();

        public Task NavigateForwardAsync();

        public Task RefreshAsync();

        public Task<string> GetPageSourceAsync();

        public Task<string> ExecuteJavascriptAsync(string script);

        public Task WaitTillLoadedAsync();

        public Task<string> GetConsoleLogsAsync();

        public Task<string> GetBrowserLogsAsync();

        public Task<bool> SwitchFrame(eLocateBy locatyBy, string locateValue);

        public Task ClickAsync(eLocateBy locateBy, string locateValue);

        public Task DoubleClickAsync(eLocateBy locateBy, string locateValue);

        public Task HoverAsync(eLocateBy locateBy, string locateValue);

        public Task<bool> IsVisibleAsync(eLocateBy locateBy, string locateValue);

        public Task<bool> IsEnabledAsync(eLocateBy locateBy, string locateValue);

        public Task<string?> GetAttributeValueAsync(eLocateBy locateBy, string locateValue, string attributeName);

        public Task<string?> GetTextContentAsync(eLocateBy locateBy, string locateValue);

        public Task<string?> GetInnerTextAsync(eLocateBy locateBy, string locateValue);

        public Task<string?> GetInputValueAsync(eLocateBy locateBy, string locateValue);

        public Task<string?> GetSelectValue(eLocateBy locateBy, string locateValue);

        public Task RightClickAsync(eLocateBy locateBy, string locateValue);

        public Task CloseAsync();
    }
}
