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

//using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using Microsoft.CodeAnalysis.Text;
using System.IO;

namespace GingerCore.GeneralLib
{    

    public class Email : RepositoryItemBase
    {
        public IEmailOperations EmailOperations;

        public Email()
        {
            Attachments = new List<string>();
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
        public static string CertificatePasswordUCValueExpression { get; set; }
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

        public bool IsBodyHTML { get; set; } = true;

        private string mItemName;
        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                mItemName = value;
            }
        }


        

    }
}
