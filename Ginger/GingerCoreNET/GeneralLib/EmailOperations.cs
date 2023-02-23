#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.Run;
using NPOI.HPSF;
using Org.BouncyCastle.Asn1.X509;
using Array = System.Array;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Applitools;
using Ginger.Run.RunSetActions;
using GingerCore.Actions.WebServices;
using System.Net.Security;
using System.Reflection;
using System.Windows;

namespace GingerCore.GeneralLib
{
    public class EmailOperations : IEmailOperations
    {
        public Email Email;

        static bool InitSmtpAuthenticationManagerDone = false;
        public EmailOperations(Email email)
        {
            //Attachments = new List<string>();
            if (!InitSmtpAuthenticationManagerDone)
            {
                // For Linux we need to fix the auth
                WorkSpace.Instance.OSHelper.InitSmtpAuthenticationManager();
                InitSmtpAuthenticationManagerDone = true;
            }
            this.Email = email;
            this.Email.EmailOperations = this;
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
            if (Email.EmailMethod == Email.eEmailMethod.OUTLOOK)
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
            bool a = CheckRequiredEmailFields(true);
            if (a)
            {
                a = TargetFrameworkHelper.Helper.Send_Outlook(actualSend, Email.MailTo, Email.Event, Email.Subject, Email.Body, Email.MailCC, Email.Attachments, Email.EmbededAttachment);
            }
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
                if (!CheckRequiredEmailFields())
                {
                    return false;
                }
                mVE.Value = Email.MailFrom;
                var fromAddress = new MailAddress(mVE.ValueCalculated, Email.MailFromDisplayName);

                mVE.Value = Email.SMTPMailHost;
                string mailHost = mVE.ValueCalculated;

                if (Email.SMTPPort == 0 || Email.SMTPPort == null)
                {
                    Email.SMTPPort = 25;
                }
                var smtp = new SmtpClient()
                {
                    Host = mailHost,  // amdocs config              
                    Port = (int)Email.SMTPPort,
                    EnableSsl = Email.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = !Email.ConfigureCredential
                };

                if (Email.ConfigureCredential)
                {
                    string DecryptPass = EncryptionHandler.DecryptwithKey(Email.SMTPPass);
                    mVE.Value = Email.SMTPUser;
                    string smtpUser = mVE.ValueCalculated;
                    if (!string.IsNullOrEmpty(DecryptPass))
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, DecryptPass);
                    }
                    else
                    {
                        smtp.Credentials = new NetworkCredential(smtpUser, Email.SMTPPass);
                    }
                }
                mVE.Value = Email.MailTo;
                string emails = mVE.ValueCalculated;
                Array arrEmails = emails.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                System.Net.Mail.MailMessage myMail = new System.Net.Mail.MailMessage();
                foreach (string email in arrEmails)
                {
                    myMail.To.Add(email);
                }
                if (Email.IsValidationRequired)
                {
                    string CertificateName = Path.GetFileName(Email.CertificatePath);
                    string CertificateKey = Email.CertificatePasswordUCValueExpression;
                    string targetPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\EmailCertificates");//certificate is present in this folder //used this since relative path cannot be used during execution
                    string Certificatepath = Path.Combine(targetPath, CertificateName);
                    if (!string.IsNullOrEmpty(Certificatepath))
                    {
                        GingerRunner.eActionExecutorType ActionExecutorType = GingerRunner.eActionExecutorType.RunWithoutDriver;
                        X509Certificate2 customCertificate = new X509Certificate2();
                        ServicePointManager.ServerCertificateValidationCallback = delegate (object s, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
                        {
                            X509Certificate2 actualCertificate;
                            if (!string.IsNullOrEmpty(CertificateKey))
                            {
                                actualCertificate = new X509Certificate2(Certificatepath, CertificateKey);
                            }
                            else
                            {
                                actualCertificate = new X509Certificate2(Certificatepath);
                            }
                            if (certificate.Equals(actualCertificate))
                            {
                                Email.Attachments.Add(Certificatepath);
                                Email.Event = "Uploaded certificate is vaslidated";
                                return true;
                            }
                            else
                            {
                                Email.Event = "Uploaded certificate is not validated as it is not matching with base certificate";
                                return false;
                            }
                        };
                    }
                    else
                    {
                        Email.Event = "Request setup Failed because of missing/wrong input";
                        return false;
                    }
                }
                //Add CC
                if (!String.IsNullOrEmpty(Email.MailCC))
                {
                    Array arrCCEmails = Email.MailCC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string MailCC1 in arrCCEmails)
                    {
                        myMail.CC.Add(MailCC1);
                    }
                }

                mVE.Value = Email.Subject;
                string subject = mVE.ValueCalculated;

                mVE.Value = Email.Body;
                string body = mVE.ValueCalculated;

                myMail.From = fromAddress;
                myMail.IsBodyHtml = IsBodyHTML;

                myMail.Subject = subject.Replace('\r', ' ').Replace('\n', ' ');
                myMail.Body = body;

                foreach (string AttachmentFileName in Email.Attachments)
                {
                    if (String.IsNullOrEmpty(AttachmentFileName) == false)
                    {
                        Attachment a = new Attachment(WorkSpace.Instance.OSHelper.AdjustFilePath(AttachmentFileName));
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
                    Email.Event = "Failed: Please provide correct FROM email address";
                }
                else if (ex.StackTrace.Contains("System.Runtime.InteropServices.Marshal.GetActiveObject"))
                {
                    Email.Event = "Please make sure ginger/outlook opened in same security context (Run as administrator or normal user)";
                }
                else if (ex.StackTrace.Contains("System.Security.Authentication.AuthenticationException") || ex.StackTrace.Contains("System.Net.Sockets.SocketException"))
                {
                    Email.Event = "Please check SSL configuration";
                }
                else
                {
                    Email.Event = "Failed: " + ex.Message;
                }
                Reporter.ToLog(eLogLevel.ERROR, "Failed to send mail", ex);

                return false;
            }
        }
        private bool CheckRequiredEmailFields(bool outlookEmail = false)
        {
            if (!outlookEmail)
            {
                if (string.IsNullOrEmpty(Email.MailFrom))
                {
                    Email.Event = "Failed: Please provide FROM email address.";
                    return false;
                }
                if (string.IsNullOrEmpty(Email.SMTPMailHost))
                {
                    Email.Event = "Failed: Please provide Mail Host";
                    return false;
                }
            }
            if (string.IsNullOrEmpty(Email.MailTo))
            {
                Email.Event = "Failed: Please provide TO email address.";
                return false;
            }
            if (string.IsNullOrEmpty(Email.Subject))
            {
                Email.Event = "Failed: Please provide email subject.";
                return false;
            }
            return true;
        }
    }
}
