using Amdocs.Ginger.Common.UIElement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserElementLocator
    {
        public Task<IEnumerable<IBrowserElement>> FindMatchingElements(eLocateBy locateBy, string locateValue);

        public Task<IBrowserElement?> FindFirstMatchingElement(eLocateBy locateBy, string locateValue);
    }
}
