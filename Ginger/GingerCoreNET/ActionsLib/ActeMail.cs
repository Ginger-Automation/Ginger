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

using System;
using System.Collections.Generic;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.WebServices;
using System.Net.Http;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using System.IO;
using Org.BouncyCastle.X509;
using Ginger.Run;
using System.Reflection;
using OpenQA.Selenium.DevTools.V99.DOM;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Graph;
using Azure.Identity;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using Microsoft.Graph.Extensions;
using System.Linq;
using System.Diagnostics;
using GingerCore.DataSource;
using GingerCore.Environments;

namespace GingerCore.Actions.Communication
{
    public class ActeMail : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Email Action"; } }
        public override string ActionUserDescription { get { return "Email Action"; } }
        HttpClientHandler Handler = null;
        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Email Action let you send email");
            TBH.AddLineBreak();
            TBH.AddText("It is possible to include attachments");
        }

        public override string ActionEditPage { get { return "Communication.ActeMailEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public enum eEmailActionType
        {
            SendEmail = 1,
            ReadEmail = 2
        }

        public new static partial class Fields
        {
            public static string EnableSSL = "EnableSSL";
            public static string ConfigureCredential = "ConfigureCredential";
            public static readonly bool IsValidationRequired = false;
        }
        public static string CertificatePasswordUCValueExpression { get; set; }
        private string mCertificatePath;
        [IsSerializedForLocalRepository]
        public string CertificatePath
        {
            get { return mCertificatePath; }
            set
            {
                if (mCertificatePath != value)
                {
                    mCertificatePath = value;
                }
            }
        }
        private bool mIsValidationRequired = false;
        [IsSerializedForLocalRepository]
        public bool IsValidationRequired
        {
            get { return mIsValidationRequired; }
            set
            {
                if (mIsValidationRequired != value)
                {
                    mIsValidationRequired = value;
                }
            }
        }
        #region Action Fields 
        //These fields were serialized earlier, do not remove it.
        public override string ActionType
        {
            get { return "Email" + eMailActionType.ToString(); }
        }

        public eEmailActionType eMailActionType 
        { 
            get
            {
                return GetOrCreateInputParam(nameof(eMailActionType), eEmailActionType.SendEmail);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(eMailActionType), value.ToString());
                OnPropertyChanged(nameof(eMailActionType));
            }
        }

        public enum eReadAuthenticationType
        {
            OAuth,
            Credentials
        }

        public string ReadAuthenticationType
        {
            get
            {
                return GetOrCreateInputParam(nameof(ReadAuthenticationType), default(eReadAuthenticationType).ToString()).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ReadAuthenticationType), value);
                OnPropertyChanged(nameof(ReadAuthenticationType));
            }
        }

        public string ReadUserEmail
        {
            get
            {
                return GetOrCreateInputParam(nameof(ReadUserEmail), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ReadUserEmail), value);
                OnPropertyChanged(nameof(ReadUserEmail));
            }
        }

        public string ReadUserPassword
        {
            get
            {
                return GetOrCreateInputParam(nameof(ReadUserPassword), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ReadUserPassword), value);
                OnPropertyChanged(nameof(ReadUserPassword));
            }
        }

        public string MSGraphClientId
        {
            get
            {
                return GetOrCreateInputParam(nameof(MSGraphClientId), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MSGraphClientId), value);
                OnPropertyChanged(nameof(MSGraphClientId));
            }
        }
        
        public string MSGraphTenantId
        {
            get
            {
                return GetOrCreateInputParam(nameof(MSGraphTenantId), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(MSGraphTenantId), value);
                OnPropertyChanged(nameof(MSGraphTenantId));
            }
        }

        public EmailReadFilters.eFolderFilter FilterFolder
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterFolder), EmailReadFilters.eFolderFilter.All);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterFolder), value.ToString());
                OnPropertyChanged(nameof(FilterFolder));
            }
        }

        public string FilterFolderNames
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterFolderNames), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterFolderNames), value);
                OnPropertyChanged(nameof(FilterFolderNames));
            }
        }

        public string FilterFrom
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterFrom), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterFrom), value);
                OnPropertyChanged(nameof(FilterFrom));
            }
        }

        public string FilterTo
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterTo), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterTo), value);
                OnPropertyChanged(nameof(FilterTo));
            }
        }

        public string FilterSubject
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterSubject), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterSubject), value);
                OnPropertyChanged(nameof(FilterSubject));
            }
        }

        public string FilterBody
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterBody), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterBody), value);
                OnPropertyChanged(nameof(FilterBody));
            }
        }

        public EmailReadFilters.eHasAttachmentsFilter FilterHasAttachments
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterHasAttachments), EmailReadFilters.eHasAttachmentsFilter.Either);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterHasAttachments), value.ToString());
                OnPropertyChanged(nameof(FilterHasAttachments));
            }
        }

        public string FilterAttachmentContentType
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterAttachmentContentType), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterAttachmentContentType), value);
                OnPropertyChanged(nameof(FilterAttachmentContentType));
            }
        }

        public bool DownloadAttachments
        {
            get
            {
                return bool.Parse(GetOrCreateInputParam(nameof(DownloadAttachments), false.ToString()).Value);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(DownloadAttachments), value.ToString());
                OnPropertyChanged(nameof(DownloadAttachments));
            }
        }

        public string AttachmentDownloadPath
        {
            get
            {
                return GetOrCreateInputParam(nameof(AttachmentDownloadPath)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(AttachmentDownloadPath), value);
                OnPropertyChanged(nameof(AttachmentDownloadPath));
            }
        }

        public string FilterReceivedStartDate
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterReceivedStartDate), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterReceivedStartDate), value);
                OnPropertyChanged(nameof(FilterReceivedStartDate));
            }
        }

        public string FilterReceivedEndDate
        {
            get
            {
                return GetOrCreateInputParam(nameof(FilterReceivedEndDate), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(FilterReceivedEndDate), value);
                OnPropertyChanged(nameof(FilterReceivedEndDate));
            }
        }

        public string Host
        {
            get
            {
                return GetOrCreateInputParam(nameof(Host), "").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Host), value);
                OnPropertyChanged(nameof(Host));
            }
        }

        public string Port
        {
            get
            {
                return GetOrCreateInputParam(nameof(Port), "25").Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(Port), value);
                OnPropertyChanged(nameof(Port));
            }
        }

        public string User
        {
            get { return GetInputParamValue(nameof(User)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(User), value);
                OnPropertyChanged(nameof(User));
            }
        }

        public string Pass
        {
            get { return GetInputParamValue(nameof(Pass)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(Pass), value);
                OnPropertyChanged(nameof(Pass));
            }
        }

        public string MailFrom
        {
            get { return GetInputParamValue(nameof(MailFrom)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(MailFrom), value);
                OnPropertyChanged(nameof(MailFrom));
            }
        }

        public string MailFromDisplayName
        {
            get { return GetInputParamValue(nameof(MailFromDisplayName)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(MailFromDisplayName), value);
                OnPropertyChanged(nameof(MailFromDisplayName));
            }
        }

        public string Mailto
        {
            get { return GetInputParamValue(nameof(Mailto)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(Mailto), value);
                OnPropertyChanged(nameof(Mailto));
            }
        }


        public string Mailcc
        {
            get { return GetInputParamValue(nameof(Mailcc)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(Mailcc), value);
                OnPropertyChanged(nameof(Mailcc));
            }
        }


        public string Subject
        {
            get { return GetInputParamValue(nameof(Subject)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(Subject), value);
                OnPropertyChanged(nameof(Subject));
            }
        }


        public string Body
        {
            get { return GetInputParamValue(nameof(Body)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(Body), value);
                OnPropertyChanged(nameof(Body));
            }
        }


        public string AttachmentFileName
        {
            get { return GetInputParamValue(nameof(AttachmentFileName)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(AttachmentFileName), value);
                OnPropertyChanged(nameof(AttachmentFileName));
            }
        }


        public string MailOption
        {
            get { return GetInputParamValue(nameof(MailOption)); }
            set
            {
                AddOrUpdateInputParamValue(nameof(MailOption), value);
                OnPropertyChanged(nameof(MailOption));
            }
        }


        public bool EnableSSL_Value
        {
            get
            {
                bool returnValue = true;
                if (Boolean.TryParse((GetInputParamValue(Fields.EnableSSL)), out returnValue))
                {
                    return returnValue;
                }
                else
                    return false;
            }
        }
        #endregion  

        public override void Execute()
        {
            if (eMailActionType == eEmailActionType.SendEmail)
            {
                SendEmail();
            }
            else if (eMailActionType == eEmailActionType.ReadEmail)
            {
                ReadEmails();
            }
        }

        private void SendEmail()
        {
            Email email = new Email();
            EmailOperations emailOperations = new EmailOperations(email);
            email.EmailOperations = emailOperations;

            bool isSuccess;
            if (!string.IsNullOrEmpty(Host))
                email.SMTPMailHost = Host;
            try { email.SMTPPort = Convert.ToInt32(this.GetInputParamCalculatedValue(nameof(Port))); }
            catch { email.SMTPPort = 25; }

            email.Subject = this.GetInputParamCalculatedValue(nameof(Subject));
            email.Body = this.GetInputParamCalculatedValue(nameof(Body));
            email.MailFrom = this.GetInputParamCalculatedValue(nameof(MailFrom));
            email.MailTo = this.GetInputParamCalculatedValue(nameof(Mailto));
            email.MailCC = this.GetInputParamCalculatedValue(nameof(Mailcc));

            //add multi attachment files
            if (!string.IsNullOrEmpty(this.GetInputParamCalculatedValue(nameof(AttachmentFileName))))
            {
                String[] fileslist = this.GetInputParamCalculatedValue(nameof(AttachmentFileName)).Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (String filePath in fileslist)
                {
                    email.Attachments.Add(filePath);
                }
            }

            email.EnableSSL = (bool)this.GetInputParamValue<bool>(Fields.EnableSSL);
            email.ConfigureCredential = (bool)this.GetInputParamValue<bool>(Fields.ConfigureCredential);
            email.SMTPUser = this.GetInputParamCalculatedValue(nameof(User));
            email.SMTPPass = this.GetInputParamCalculatedValue(nameof(Pass));
            if (IsValidationRequired)
            {
                string CertificateName = Path.GetFileName(CertificatePath);
                string CertificateKey = CertificatePasswordUCValueExpression;
                string targetPath = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\EmailCertificates");
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
                            ExInfo = "Uploaded certificate is vaslidated";
                            return true;
                        }
                        else
                        {
                            Error = "Uploaded certificate is not validated as it is not matching with base certificate";
                            return false;
                        }
                    };
                }
                else
                {
                    Error = "Request setup Failed because of missing/wrong input";
                    ExInfo = "Certificate path is missing";
                }
            }
            if (email.EmailMethod == Email.eEmailMethod.SMTP)
            {
                email.MailFromDisplayName = this.GetInputParamCalculatedValue(nameof(MailFromDisplayName));
            }
            if (string.IsNullOrEmpty(email.MailTo))
            {
                Error = "Failed: Please provide TO email address.";
                return;
            }
            if (string.IsNullOrEmpty(Subject))
            {
                Error = "Failed: Please provide email subject.";
                return;
            }
            if (this.GetInputParamCalculatedValue(nameof(MailOption)) == Email.eEmailMethod.OUTLOOK.ToString())
            {
                email.EmailMethod = Email.eEmailMethod.OUTLOOK;
            }
            else
            {
                email.EmailMethod = Email.eEmailMethod.SMTP;
                email.MailFromDisplayName = this.GetInputParamCalculatedValue(nameof(MailFromDisplayName));
                if (string.IsNullOrEmpty(email.MailFrom))
                {
                    Error = "Failed: Please provide FROM email address.";
                    return;
                }
            }

            isSuccess = email.EmailOperations.Send();
            if (isSuccess == false)
            {
                Error = email.Event;
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }

            if (email.Event != null && email.Event.IndexOf("Failed") >= 0)
            {
                Error = email.Event;
            }
        }

        private void ReadEmails()
        {
            IEmailReadOperations emailReadOperations = new EmailReadOperations();
            EmailReadFilters filters = CreateEmailReadFilters();
            MSGraphConfig config = CreateMSGraphConfig();
            int index = 1;
            emailReadOperations.ReadEmails(filters, config, email => 
            {
                AddOrUpdateReturnParamActualWithPath(nameof(ReadEmail.From), email.From, index.ToString());
                AddOrUpdateReturnParamActualWithPath(nameof(ReadEmail.Subject), email.Subject, index.ToString());
                AddOrUpdateReturnParamActualWithPath(nameof(ReadEmail.Body), email.Body, index.ToString());
                AddOrUpdateReturnParamActualWithPath(nameof(ReadEmail.HasAttachments), email.HasAttachments.ToString(), index.ToString());
                AddOrUpdateReturnParamActualWithPath(nameof(ReadEmail.ReceivedDateTime), email.ReceivedDateTime.ToString(), index.ToString());
                if (DownloadAttachments && FilterHasAttachments == EmailReadFilters.eHasAttachmentsFilter.Yes)
                {
                    IEnumerable<(string filename, string filepath)> fileNamesAndPaths = DownloadAttachmentFiles(email);
                    foreach((string filename, string filepath) in fileNamesAndPaths)
                    {
                        AddOrUpdateReturnParamActualWithPath(filename, filepath, index.ToString());
                    }
                }
                index++;
            }).Wait();
        }

        private EmailReadFilters CreateEmailReadFilters()
        {
            string calculatedFolderName = GetInputParamCalculatedValue(nameof(FilterFolderNames));
            string calculatedFrom = GetInputParamCalculatedValue(nameof(FilterFrom));
            string calculatedTo = GetInputParamCalculatedValue(nameof(FilterTo));
            string calculatedSubject = GetInputParamCalculatedValue(nameof(FilterSubject));
            string calculatedBody = GetInputParamCalculatedValue(nameof(FilterBody));
            string calculatedAttachmentContentType = GetInputParamCalculatedValue(nameof(FilterAttachmentContentType));
            string calculatedAttachmentDownloadPath = GetInputParamCalculatedValue(nameof(AttachmentDownloadPath));
            string calculatedReceivedStartDate = GetInputParamCalculatedValue(nameof(FilterReceivedStartDate));
            DateTime receivedStartDate = DateTime.MinValue;
            if (!string.IsNullOrEmpty(calculatedReceivedStartDate))
            {
                receivedStartDate = DateTime.Parse(calculatedReceivedStartDate);
            }
            string calculatedReceivedEndDate = GetInputParamCalculatedValue(nameof(FilterReceivedEndDate));
            DateTime receivedEndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(calculatedReceivedEndDate))
            {
                receivedEndDate = DateTime.Parse(calculatedReceivedEndDate);
            }

            EmailReadFilters filters = new()
            {
                Folder = FilterFolder,
                FolderNames = calculatedFolderName,
                From = calculatedFrom,
                To = calculatedTo,
                Subject = calculatedSubject,
                Body = calculatedBody,
                HasAttachments = FilterHasAttachments,
                AttachmentContentType = calculatedAttachmentContentType,
                AttachmentDownloadPath = calculatedAttachmentDownloadPath,
                ReceivedStartDate = receivedStartDate,
                ReceivedEndDate = receivedEndDate
            };

            return filters;
        }

        private MSGraphConfig CreateMSGraphConfig()
        {
            string calculatedUserEmail = GetInputParamCalculatedValue(nameof(ReadUserEmail));
            string calculatedUserPassword = GetInputParamCalculatedValue(nameof(ReadUserPassword));
            if (EncryptionHandler.IsStringEncrypted(calculatedUserPassword))
            {
                calculatedUserPassword = EncryptionHandler.DecryptwithKey(calculatedUserPassword);
            }
            string calculatedMSGraphClientId = GetInputParamCalculatedValue(nameof(MSGraphClientId));
            string calculatedMSGraphTenantId = GetInputParamCalculatedValue(nameof(MSGraphTenantId));

            MSGraphConfig config = new()
            {
                UserEmail = calculatedUserEmail,
                UserPassword = calculatedUserPassword,
                ClientId = calculatedMSGraphClientId,
                TenantId = calculatedMSGraphTenantId
            };

            return config;
        }

        private IEnumerable<(string filename, string filepath)> DownloadAttachmentFiles(ReadEmail email)
        {
            string calculatedAttachmentDownloadPath = GetInputParamCalculatedValue(nameof(AttachmentDownloadPath));
            if (string.IsNullOrEmpty(calculatedAttachmentDownloadPath))
            {
                throw new InvalidOperationException("Invalid attachment download path");
            }

            IEnumerable<string> expectedContentTypes = null;
            string calculatedAttachmentContentType = GetInputParamCalculatedValue(nameof(FilterAttachmentContentType));
            if (!string.IsNullOrEmpty(calculatedAttachmentContentType))
            {
                expectedContentTypes = calculatedAttachmentContentType.Split(";", StringSplitOptions.RemoveEmptyEntries);
            }

            List<(string filename, string filepath)> fileNamesAndPaths = new();

            foreach (ReadEmail.Attachment attachment in email.Attachments)
            {
                if (expectedContentTypes != null && expectedContentTypes.Count() > 0 && 
                    !expectedContentTypes.Any(expectedContentType => expectedContentType.Equals(attachment.ContentType)))
                {
                    continue;
                }
                string downloadFolder = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(calculatedAttachmentDownloadPath);
                if (!System.IO.Directory.Exists(downloadFolder))
                {
                    System.IO.Directory.CreateDirectory(downloadFolder);
                }
                string filePath = Path.Combine(downloadFolder, attachment.Name);
                string uniqueFilePath = GetUniqueFilePath(filePath);
                File.WriteAllBytes(uniqueFilePath, attachment.ContentBytes);
                fileNamesAndPaths.Add((attachment.Name, uniqueFilePath));
            }

            return fileNamesAndPaths;
        }

        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return filePath;
            }

            string directory = Path.GetDirectoryName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            int copyKey = 0;
            string uniqueFilePath;
            do
            {
                copyKey++;
                uniqueFilePath = Path.Combine(directory, $"{fileNameWithoutExtension}_Copy{copyKey}{fileExtension}");
            } while (File.Exists(uniqueFilePath));

            return uniqueFilePath;
        }
    }
}