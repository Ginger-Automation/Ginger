using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    internal interface IBrowserDialog
    {
        public delegate Task OnDialogHandle(IBrowserDialog handledDialog);

        public Task<string> GetMessageAsync();
    }
}
