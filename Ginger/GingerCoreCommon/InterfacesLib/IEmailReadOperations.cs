using System;
using System.Threading.Tasks;
using GingerCore.GeneralLib;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IEmailReadOperations
    {
        Task ReadEmails(EmailReadFilters filters, EmailReadConfig config, Action<ReadEmail> emailProcessor);
    }
}
