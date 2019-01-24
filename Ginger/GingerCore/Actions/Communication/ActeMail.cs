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
using GingerCore.GeneralLib;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCore.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions.Communication
{
    public class ActeMail : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Email Action"; } }
        public override string ActionUserDescription { get { return "Email Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Email Action let you send email");
            TBH.AddLineBreak();
            TBH.AddText("It is possible to include attachments");
        }        

        public override string ActionEditPage { get { return "Communication.ActeMailEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }
        
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
        }

        public new static partial class Fields
        {
            public static string Host = "Host";
            public static string Port = "Port";
            public static string MailFrom = "MailFrom";
            public static string Mailto = "Mailto";
            public static string Mailcc = "Mailcc";
            public static string Subject = "Subject";
            public static string Body = "Body";
            public static string AttachmentFileName = "AttachmentFileName";
            public static string eMailActionType = "eMailActionType";
            public static string User = "User";
            public static string Pass = "Passed";
            public static string MailOption = "MailOption";
            public static string EnableSSL = "EnableSSL";
        }

        public override string ActionType
        {
            get { return "Email" + eMailActionType.ToString(); }
        }

        [IsSerializedForLocalRepository]
        public eEmailActionType eMailActionType { get; set; }

        [IsSerializedForLocalRepository]
        public string Host
        {
            get
            {                
                return GetOrCreateInputParam(Fields.Host, "").Value;
            }
            set { AddOrUpdateInputParamValue(Fields.Host, value); }
        }

        [IsSerializedForLocalRepository]
        public string Port
        {
            get
            {               
                return GetOrCreateInputParam(Fields.Port, "25").Value;
            }
            set { AddOrUpdateInputParamValue(Fields.Port, value); }
        }
        [IsSerializedForLocalRepository]
        public string User
        {
            get { return GetInputParamValue(Fields.User); }
            set { AddOrUpdateInputParamValue(Fields.User, value); }
        }
        [IsSerializedForLocalRepository]
        public string Pass
        {
            get { return GetInputParamValue(Fields.Pass); }
            set { AddOrUpdateInputParamValue(Fields.Pass, value); }
        }
        [IsSerializedForLocalRepository]
        public string MailFrom
        {
            get { return GetInputParamValue(Fields.MailFrom); }
            set { AddOrUpdateInputParamValue(Fields.MailFrom, value); }
        }

        [IsSerializedForLocalRepository]
        public string Mailto
        {
            get { return GetInputParamValue(Fields.Mailto); }
            set { AddOrUpdateInputParamValue(Fields.Mailto, value); }
        }

        [IsSerializedForLocalRepository]
        public string Mailcc
        {
            get { return GetInputParamValue(Fields.Mailcc); }
            set { AddOrUpdateInputParamValue(Fields.Mailcc, value); }
        }

        [IsSerializedForLocalRepository]
        public string Subject
        {
            get { return GetInputParamValue(Fields.Subject); }
            set { AddOrUpdateInputParamValue(Fields.Subject, value); }
        }

        [IsSerializedForLocalRepository]
        public string Body
        {
            get { return GetInputParamValue(Fields.Body); }
            set { AddOrUpdateInputParamValue(Fields.Body, value); }
        }

        [IsSerializedForLocalRepository]
        public string AttachmentFileName
        {
            get { return GetInputParamValue(Fields.AttachmentFileName); }
            set { AddOrUpdateInputParamValue(Fields.AttachmentFileName, value);
                OnPropertyChanged(nameof(AttachmentFileName));
            }
        }

        [IsSerializedForLocalRepository]
        public string MailOption
        {
            get { return GetInputParamValue(Fields.MailOption); }
            set { AddOrUpdateInputParamValue(Fields.MailOption, value); }
        }

        [IsSerializedForLocalRepository]
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

        public override void Execute()
        {
            Email e = new Email();
            bool isSuccess;
            if (!string.IsNullOrEmpty(Host))
                e.SMTPMailHost = Host;
            try { e.SMTPPort = Convert.ToInt32(this.GetInputParamCalculatedValue(Fields.Port)); }
            catch { e.SMTPPort = 25; }

            e.Subject = this.GetInputParamCalculatedValue(Fields.Subject);
            e.Body = this.GetInputParamCalculatedValue(Fields.Body);
            e.MailFrom = this.GetInputParamCalculatedValue(Fields.MailFrom);
            e.MailTo = this.GetInputParamCalculatedValue(Fields.Mailto);
            e.MailCC = this.GetInputParamCalculatedValue(Fields.Mailcc);
            e.Attachments.Add(this.GetInputParamCalculatedValue(Fields.AttachmentFileName));
            e.EnableSSL = EnableSSL_Value;
            if (string.IsNullOrEmpty(e.MailTo))
            {
                Error = "Failed: Please provide TO email address.";
                return;
            }
            if (string.IsNullOrEmpty(Subject))
            {
                Error = "Failed: Please provide email subject.";
                return;
            }
            if (this.GetInputParamCalculatedValue(Fields.MailOption) == Email.eEmailMethod.OUTLOOK.ToString())
                e.EmailMethod = Email.eEmailMethod.OUTLOOK;
            else
            {
                e.EmailMethod = Email.eEmailMethod.SMTP;
                if (string.IsNullOrEmpty(e.MailFrom))
                {
                    Error = "Failed: Please provide FROM email address.";
                    return;
                }
            }                
            
            isSuccess = e.Send();
            if (isSuccess == false)
            {
                Error = e.Event;                
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }

            if (e.Event != null && e.Event.IndexOf("Failed") >= 0)
                Error = e.Event;
        }
    }
}