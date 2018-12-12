#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using GingerCore.Repository;
using OutLook = Microsoft.Office.Interop.Outlook;
using System.Net;

namespace GingerCore.GeneralLib
{
    public class Email : RepositoryItemBase
    {
        OutLook.MailItem mOutlookMail;
        
        public new static class Fields
        {
            public static string SMTPMailHost = "SMTPMailHost";
            public static string SMTPPort = "SMTPPort";
            public static string SMTPUser = "SMTPUser";
            public static string SMTPPass = "SMTPPass";
            public static string MailFrom = "MailFrom";
            public static string MailTo = "MailTo";
            public static string MailCC = "MailCC";
            public static string Subject = "Subject";
            public static string Body = "Body";
            public static string EmailMethod = "EmailMethod";
            public static string EnableSSL = "EnableSSL";
            public static string ConfigureCredential = "ConfigureCredential";
        }
        
        public enum eEmailMethod
        {
            SMTP,OUTLOOK
        }

        [IsSerializedForLocalRepository]
        public int ID { get; set; }

        [IsSerializedForLocalRepository]
        public string MailFrom { get; set; }

        [IsSerializedForLocalRepository]
        public string MailTo { get; set; }

        [IsSerializedForLocalRepository]
        public string MailCC { get; set; }

        [IsSerializedForLocalRepository]
        public string MailtoName { get; set; }
        
        [IsSerializedForLocalRepository]
        public string Subject { get; set; }
        
        [IsSerializedForLocalRepository]
        public string Body { get; set; }

        [IsSerializedForLocalRepository]
        public List<string> Attachments; // { get; set; } // File names

        public List<KeyValuePair<string, string>> EmbededAttachment = new List<KeyValuePair<string, string>>();

        private string mSMTPMailHost = "";
        [IsSerializedForLocalRepository]
        public string SMTPMailHost
        {
            get { return mSMTPMailHost; }
            set
            {
                if (mSMTPMailHost != value)
                {
                    mSMTPMailHost = value;
                    OnPropertyChanged(Fields.SMTPMailHost);
                }
            }
        }

        private int mSMTPPort = 25;
        [IsSerializedForLocalRepository]
        public int? SMTPPort
        {
            get { return mSMTPPort; }
            set
            {
                if (mSMTPPort != value)
                {
                    mSMTPPort = (int)value;
                    OnPropertyChanged(Fields.SMTPPort);
                }
            }
        }

        private string mSMTPUser;
        [IsSerializedForLocalRepository]
        public string SMTPUser { get { return mSMTPUser; } set { if (mSMTPUser != value) { mSMTPUser = value; OnPropertyChanged(Fields.SMTPUser);  } } }

        private string mSMTPPass;
        [IsSerializedForLocalRepository]
        public string SMTPPass { get { return mSMTPPass; } set { if (mSMTPPass != value) { mSMTPPass = value; OnPropertyChanged(Fields.SMTPPass); } } }

        [IsSerializedForLocalRepository]
        public string Event { get; set; }

        [IsSerializedForLocalRepository]
        public eEmailMethod EmailMethod { get; set; }

        private bool mEnableSSL = true;
        [IsSerializedForLocalRepository(true)]        
        public bool EnableSSL
        {
            get { return mEnableSSL; }
            set
            {
                if (mEnableSSL != value)
                {
                    mEnableSSL = value;                    
                }
            }
        }

        private bool mConfigureCredential = false;
        [IsSerializedForLocalRepository(false)]
        public bool ConfigureCredential
        {
            get { return mConfigureCredential; }
            set
            {
                if (mConfigureCredential != value)
                {
                    mConfigureCredential = value;
                }
            }
        }

        public Email()
        {
            Attachments = new List<string>();
        }
        
        public bool Send()
        {
            //If Outlook Option is selected
            if (EmailMethod == eEmailMethod.OUTLOOK)
            {
              return Send_Outlook();
            }
            //If SMTP is selected
            else
            {
               return Send_SMTP();
            }
            //TODO:Handled Clean up of temporary folder after mail sent.
        }

        public AlternateView alternateView { get; set; }
        private bool Send_Outlook(bool actualSend = true)
        {
            try
            {
                OutLook.Application objOutLook = null;
                if (string.IsNullOrEmpty(MailTo))
                {
                    Event = "Failed: Please provide TO email address.";
                    return false;
                }
                if (string.IsNullOrEmpty(Subject))
                {
                    Event = "Failed: Please provide email subject.";
                    return false;
                }
                // Check whether there is an Outlook process running.
                if (System.Diagnostics.Process.GetProcessesByName("OUTLOOK").Count() > 0)
                {
                    // If so, use the GetActiveObject method to obtain the process and cast it to an Application object.
                    objOutLook = System.Runtime.InteropServices.Marshal.GetActiveObject("Outlook.Application") as OutLook.Application;
                }
                else
                {
                    // If not, create a new instance of Outlook and log on to the default profile.
                    objOutLook = new OutLook.Application();
                    OutLook.NameSpace nameSpace = objOutLook.GetNamespace("MAPI");
                    nameSpace.Logon("", "", System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                    nameSpace = null;
                }

                mOutlookMail = objOutLook.CreateItem(OutLook.OlItemType.olMailItem) as OutLook.MailItem;

                mOutlookMail.HTMLBody = this.Body;
                mOutlookMail.Subject = this.Subject;

                OutLook.AddressEntry currentUser = objOutLook.Session.CurrentUser.AddressEntry;

                if (currentUser.Type == "EX")
                {
                    OutLook.ExchangeUser manager = currentUser.GetExchangeUser();

                    // Add recipient using display name, alias, or smtp address
                    string emails = MailTo;
                    Array arrEmails = emails.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string email in arrEmails)
                    {
                        mOutlookMail.Recipients.Add(email);
                    }

                    //Add CC
                    if (!String.IsNullOrEmpty(MailCC))
                    {
                        Array arrCCEmails = MailCC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string MailCC1 in arrCCEmails)
                        {
                            mOutlookMail.Recipients.Add(MailCC1);
                        }
                    }

                    mOutlookMail.Recipients.ResolveAll();

                    mOutlookMail.CC = this.MailCC;
                    mOutlookMail.To = this.MailTo;

                    //Add Attachment
                    foreach (string AttachmentFileName in Attachments)
                    {
                        if (String.IsNullOrEmpty(AttachmentFileName) == false)
                            mOutlookMail.Attachments.Add(AttachmentFileName, Type.Missing, Type.Missing, Type.Missing);
                    }

                    //attachment which is embeded into the email body(images).                       
                    foreach (KeyValuePair<string, string> AttachmentFileName in EmbededAttachment)
                    {
                        if (String.IsNullOrEmpty(AttachmentFileName.Key) == false)
                        {
                            OutLook.Attachment attachment = mOutlookMail.Attachments.Add(AttachmentFileName.Key, OutLook.OlAttachmentType.olEmbeddeditem, null, "");
                            attachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", AttachmentFileName.Value);
                        }
                    }
                    if (actualSend)
                    {
                        //Send Mail
                        mOutlookMail.Send();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Mailbox Unavailabel"))
                {
                    Event = "Failed: Please provide correct sender email address";
                }
                else if(ex.StackTrace.Contains("System.Runtime.InteropServices.Marshal.GetActiveObject"))
                {
                    Event = "Please make sure ginger/outlook opened in same security context (Run as administrator or normal user)";
                }
                else if (ex.StackTrace.Contains("System.Security.Authentication.AuthenticationException") || ex.StackTrace.Contains("System.Net.Sockets.SocketException"))
                {
                    Event = "Please check SSL configuration";
                }
                else
                {
                    Event = "Failed: " + ex.Message;
                }                
                return false;
            }
        }

        public void DisplayAsOutlookMail()
        {
            Send_Outlook(false);
            mOutlookMail.Display();
        }

        public bool Send_SMTP()
        {
            try
            {
                if (string.IsNullOrEmpty(MailFrom))
                {
                    Event = "Failed: Please provide FROM email address.";
                    return false;
                }
                if (string.IsNullOrEmpty(MailTo))
                {
                    Event = "Failed: Please provide TO email address.";
                    return false;
                }
                if (string.IsNullOrEmpty(Subject))
                {
                    Event = "Failed: Please provide email subject.";
                    return false;
                }

                if (string.IsNullOrEmpty(SMTPMailHost))
                {
                    Event = "Failed: Please provide Mail Host";
                    return false;
                }

                var fromAddress = new MailAddress(MailFrom, "_Amdocs Ginger Automation");

                if (this.SMTPPort == 0 || this.SMTPPort == null)
                    this.SMTPPort = 25;
                var smtp = new SmtpClient()
                {                    
                    Host = SMTPMailHost,  // amdocs config              
                    Port = (int)this.SMTPPort,
                    EnableSsl = EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = !ConfigureCredential   
                };

                if (ConfigureCredential)
                {
                    bool checkValueDecrypt;
                    checkValueDecrypt = true;
                    string DecryptPass = EncryptionHandler.DecryptString(SMTPPass, ref checkValueDecrypt);
                    if(checkValueDecrypt)
                    {
                        smtp.Credentials = new NetworkCredential(SMTPUser, DecryptPass);
                    }
                    else
                    {
                        smtp.Credentials = new NetworkCredential(SMTPUser, SMTPPass);
                    }
                }

                string emails = MailTo; 
                Array arrEmails = emails.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                System.Net.Mail.MailMessage myMail = new System.Net.Mail.MailMessage();
                foreach (string email in arrEmails)
                {
                    myMail.To.Add(email);
                }

                //Add CC
                if (!String.IsNullOrEmpty(MailCC))
                {
                    Array arrCCEmails = MailCC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string MailCC1 in arrCCEmails) //arrEmails) // Updated by Preeti for defect 2464
                    {
                        myMail.CC.Add(MailCC1);
                    }
                }    
                
                myMail.From = fromAddress;
                myMail.IsBodyHtml = true;
                myMail.Subject = this.Subject.Replace('\r', ' ').Replace('\n', ' ');
                myMail.Body = this.Body;

                foreach (string AttachmentFileName in Attachments)
                {
                    if (String.IsNullOrEmpty(AttachmentFileName) == false)
                    {
                        Attachment a = new Attachment(AttachmentFileName);
                        myMail.Attachments.Add(a);
                    }
                }
                if(alternateView!=null)
                {
                    myMail.AlternateViews.Add(alternateView);
                }
                smtp.Send(myMail);

                return true;
            }
            catch (Exception ex)
            {                
                if (ex.Message.Contains("Mailbox unavailable"))
                {
                    Event = "Failed: Please provide correct FROM email address";
                }
                else if (ex.StackTrace.Contains("System.Runtime.InteropServices.Marshal.GetActiveObject"))
                {
                    Event = "Please make sure ginger/outlook opened in same security context (Run as administrator or normal user)";
                }
                else if (ex.StackTrace.Contains("System.Security.Authentication.AuthenticationException") || ex.StackTrace.Contains("System.Net.Sockets.SocketException"))
                {
                    Event = "Please check SSL configuration";
                }
                else
                {
                    Event = "Failed: " + ex.Message;
                }
                return false;
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
    }
}
