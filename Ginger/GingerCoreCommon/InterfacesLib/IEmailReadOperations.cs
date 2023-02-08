using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCore.GeneralLib;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IEmailReadOperations
    {
        Task ReadEmails(EmailReadFilters filters, MSGraphConfig config, Action<ReadEmail> emailProcessor);
    }
}
