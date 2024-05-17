using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserWindow
    {
        public delegate Task OnWindowClose(IBrowserWindow closedWindow);

        public IEnumerable<IBrowserTab> Tabs { get; }

        public IBrowserTab CurrentTab { get; }

        public bool IsClosed { get; }

        public Task<IBrowserTab> NewTabAsync(bool setAsCurrent = true);

        public Task DeleteCookiesAsync();

        public Task CloseAsync();
    }
}
