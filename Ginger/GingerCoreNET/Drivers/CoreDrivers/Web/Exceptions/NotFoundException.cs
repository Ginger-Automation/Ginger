using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions
{
    internal sealed class NotFoundException : Exception
    {
        internal NotFoundException(string message) : base(message) { }
    }
}
