using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers
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

        public Task CloseAsync();
    }
}
