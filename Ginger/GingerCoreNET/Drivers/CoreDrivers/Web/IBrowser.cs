using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowser
    {
        public delegate Task OnBrowserClose(IBrowser closedBrowser);

        public IEnumerable<IBrowserWindow> Windows { get; }

        public IBrowserWindow CurrentWindow { get; }

        public bool IsClosed { get; }

        public Task<IBrowserWindow> NewWindowAsync(bool setAsCurrent = true);

        public Task SetWindowAsync(IBrowserWindow window);

        public Task CloseAsync();
    }
}
