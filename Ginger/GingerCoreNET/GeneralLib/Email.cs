#region License
/*
Copyright © 2014-2020 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;

namespace GingerCore.GeneralLib
{    

    public class Email : RepositoryItemBase
    { 
        static bool InitSmtpAuthenticationManagerDone = false;
        public Email()
        {
            Attachments = new List<string>();
            if (!InitSmtpAuthenticationManagerDone)
            {
                // For Linux we need to fix the auth
                GingerUtils.OSHelper.Current.InitSmtpAuthenticationManager();
                InitSmtpAuthenticationManagerDone = true;
            }
        }

        public enum eEmailMethod
        {
            SMTP, OUTLOOK
        }

        [IsSerializedForLocalRepository]
        public int ID { get; set; }

        private string mMailFrom;
        [IsSerializedForLocalRepository]
        public string MailFrom
        {
            get
            {
                return mMailFrom;
            }
            set
            {
                if (mMailFrom != value)
                {
                    mMailFrom = value;
                    OnPropertyChanged(nameof(MailFrom));
                }
            }
        }

        private string mMailFromDisplayName;
        [IsSerializedForLocalRepository]
         public string MailFromDisplayName
        {
            get
            {
                return mMailFromDisplayName;
            }
            set
            {
                if (mMailFromDisplayName != value)
                {
                    mMailFromDisplayName = value;
                    OnPropertyChanged(nameof(MailFromDisplayName));
                }
            }
        }

        private string mMailTo;
        [IsSerializedForLocalRepository]
        public string MailTo
        {
            get
            {
                return mMailTo;
            }
            set
            {
                if (mMailTo != value)
                {
                    mMailTo = value;
                    OnPropertyChanged(nameof(MailTo));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string MailCC { get; set; }

        [IsSerializedForLocalRepository]
        public string MailtoName { get; set; }

        private string mSubject;
        [IsSerializedForLocalRepository]
        public string Subject
        {
            get
            {
                return mSubject;
            }
            set
            {
                if (mSubject != value)
                {
                    mSubject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        private string mBody;
        [IsSerializedForLocalRepository]
        public string Body
        {
            get
            {
                return mBody;
            }
            set
            {
                if (mBody != value)
                {
                    mBody = value;
                    OnPropertyChanged(nameof(Body));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public string Comments { get; set; }

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
                    OnPropertyChanged(nameof(SMTPMailHost));
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
                    OnPropertyChanged(nameof(SMTPPort));
                }
            }
        }

        private string mSMTPUser;
        [IsSerializedForLocalRepository]
        public string SMTPUser { get { return mSMTPUser; } set { if (mSMTPUser != value) { mSMTPUser = value; OnPropertyChanged(nameof(SMTPUser)); } } }

        private string mSMTPPass;
        [IsSerializedForLocalRepository]
        public string SMTPPass { get { return mSMTPPass; } set { if (mSMTPPass != value) { mSMTPPass = value; OnPropertyChanged(nameof(SMTPPass)); } } }

        // TODO: why we serialize? is it error report? check/fix
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

        ValueExpression mValueExpression = null;
        ValueExpression mVE
        {
            get
            {
                if (mValueExpression == null)
                {
                    mValueExpression = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
                }
                return mValueExpression;
            }
        }
        public bool IsBodyHTML { get; set; } = true;
        

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
            bool a = TargetFrameworkHelper.Helper.Send_Outlook(actualSend, MailTo, Event, Subject, Body, MailCC, Attachments, EmbededAttachment);
            return a;
        }

        public void DisplayAsOutlookMail()
        {
            Send_Outlook(false);
            TargetFrameworkHelper.Helper.DisplayAsOutlookMail();
            // mOutlookMail.Display();
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
                mVE.Value = MailFrom;
                var fromAddress = new MailAddress(mVE.ValueCalculated, MailFromDisplayName);

                mVE.Value = SMTPMailHost;
                string mailHost = mVE.ValueCalculated;

                if (this.SMTPPort == 0 || this.SMTPPort == null)
                    this.SMTPPort = 25;
                var smtp = new SmtpClient()
                {
                    Host = mailHost,  // amdocs config              
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
                    if (checkValueDecrypt)
                    {
                        smtp.Credentials = new NetworkCredential(SMTPUser, DecryptPass);
                    }
                    else
                    {
                        smtp.Credentials = new NetworkCredential(SMTPUser, SMTPPass);
                    }
                }
                mVE.Value = MailTo;
                string emails = mVE.ValueCalculated;
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
                    foreach (string MailCC1 in arrCCEmails) 
                    {
                        myMail.CC.Add(MailCC1);
                    }
                }

                mVE.Value = Subject;
                string subject = mVE.ValueCalculated;

                mVE.Value = Body;
                string body = mVE.ValueCalculated;

                myMail.From = fromAddress;
                myMail.IsBodyHtml = IsBodyHTML;

                myMail.Subject = subject.Replace('\r', ' ').Replace('\n', ' ');
                myMail.Body = body;

                foreach (string AttachmentFileName in Attachments)
                {
                    if (String.IsNullOrEmpty(AttachmentFileName) == false)
                    {
                        Attachment a = new Attachment(AttachmentFileName);
                        myMail.Attachments.Add(a);
                    }
                }
                if (alternateView != null)
                {
                    myMail.AlternateViews.Add(alternateView);
                }
                
                smtp.Send(myMail);

                //clear and dispose attachments
                if (myMail.Attachments != null)
                {
                    foreach (Attachment attachment in myMail.Attachments)
                    {
                        attachment.Dispose();
                    }
                    myMail.Attachments.Clear();
                    myMail.Attachments.Dispose();
                }
                myMail.Dispose();
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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to send mail", ex);

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
