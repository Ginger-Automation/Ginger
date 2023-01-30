using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public interface IEmailReadOperations
    {
        Task ReadEmails(EmailReadFilters filters, MSGraphConfig config, Action<ReadEmail> emailProcessor);
    }
}
