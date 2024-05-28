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

        public Task<IEnumerable<IBrowserElement>> GetElementsAsync(eLocateBy locateBy, string locateValue);

        public Task CloseAsync();
    }
}
