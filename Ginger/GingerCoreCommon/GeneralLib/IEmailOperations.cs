using System.Net.Mail;

namespace GingerCore.GeneralLib
{
    public interface IEmailOperations
    {
        AlternateView alternateView { get; set; }
        bool IsBodyHTML { get; set; }

        void DisplayAsOutlookMail();
        bool Send();
        bool Send_SMTP();
    }
}