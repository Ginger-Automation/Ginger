using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public sealed class EmailReadGmailOperations : IEmailReadOperations
    {
        public Task ReadEmails(EmailReadFilters filters, EmailReadConfig config, Action<ReadEmail> emailProcessor)
        {
            throw new NotImplementedException();
        }
    }
}
