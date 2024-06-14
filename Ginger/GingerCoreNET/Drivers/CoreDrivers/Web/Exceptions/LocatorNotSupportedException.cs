using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Exceptions
{
    public class LocatorNotSupportedException : Exception
    {
        public LocatorNotSupportedException(string message) : base(message) { }
    }
}
