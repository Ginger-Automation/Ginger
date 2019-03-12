#region License
/*
Copyright © 2014-2019 European Support Limited

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
        }

        public new static partial class Fields
        {            
            public static string EnableSSL = "EnableSSL";
            public static string ConfigureCredential = "ConfigureCredential";
        }

        #region Action Fields 
        //These fields were serialized earlier, do not remove it.
        public override string ActionType
        {
            get { return "Email" + eMailActionType.ToString(); }
        }
        
        public eEmailActionType eMailActionType { get; set; }
        
        public string Host
        {
            get
            {                
                return GetOrCreateInputParam(nameof(Host), "").Value;
            }
            set { AddOrUpdateInputParamValue(nameof(Host), value); }
        }
        
        public string Port
        {
            get
            {               
                return GetOrCreateInputParam(nameof(Port), "25").Value;
            }
            set { AddOrUpdateInputParamValue(nameof(Port), value); }
        }
        
        public string User
        {
            get { return GetInputParamValue(nameof(User)); }
            set { AddOrUpdateInputParamValue(nameof(User), value); }
        }
        
        public string Pass
        {
            get { return GetInputParamValue(nameof(Pass)); }
            set { AddOrUpdateInputParamValue(nameof(Pass), value); }
        }
        
        public string MailFrom
        {
            get { return GetInputParamValue(nameof(MailFrom)); }
            set { AddOrUpdateInputParamValue(nameof(MailFrom), value); }
        }

        
        public string Mailto
        {
            get { return GetInputParamValue(nameof(Mailto)); }
            set { AddOrUpdateInputParamValue(nameof(Mailto), value); }
        }

        
        public string Mailcc
        {
            get { return GetInputParamValue(nameof(Mailcc)); }
            set { AddOrUpdateInputParamValue(nameof(Mailcc), value); }
        }

        
        public string Subject
        {
            get { return GetInputParamValue(nameof(Subject)); }
            set { AddOrUpdateInputParamValue(nameof(Subject), value); }
        }

        
        public string Body
        {
            get { return GetInputParamValue(nameof(Body)); }
            set { AddOrUpdateInputParamValue(nameof(Body), value); }
        }

        
        public string AttachmentFileName
        {
            get { return GetInputParamValue(nameof(AttachmentFileName)); }
            set { AddOrUpdateInputParamValue(nameof(AttachmentFileName), value);
                OnPropertyChanged(nameof(AttachmentFileName));
            }
        }

        
        public string MailOption
        {
            get { return GetInputParamValue(nameof(MailOption)); }
            set { AddOrUpdateInputParamValue(nameof(MailOption), value); }
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
            Email e = new Email();
            bool isSuccess;
            if (!string.IsNullOrEmpty(Host))
                e.SMTPMailHost = Host;
            try { e.SMTPPort = Convert.ToInt32(this.GetInputParamCalculatedValue(nameof(Port))); }
            catch { e.SMTPPort = 25; }

            e.Subject = this.GetInputParamCalculatedValue(nameof(Subject));
            e.Body = this.GetInputParamCalculatedValue(nameof(Body));
            e.MailFrom = this.GetInputParamCalculatedValue(nameof(MailFrom));
            e.MailTo = this.GetInputParamCalculatedValue(nameof(Mailto));
            e.MailCC = this.GetInputParamCalculatedValue(nameof(Mailcc));
            e.Attachments.Add(this.GetInputParamCalculatedValue(nameof(AttachmentFileName)));
            e.EnableSSL = (bool)this.GetInputParamValue<bool>(Fields.EnableSSL);
            e.ConfigureCredential = (bool)this.GetInputParamValue<bool>(Fields.ConfigureCredential);
            e.SMTPUser = this.GetInputParamCalculatedValue(nameof(User));
            e.SMTPPass = this.GetInputParamCalculatedValue(nameof(Pass));
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
            if (this.GetInputParamCalculatedValue(nameof(MailOption)) == Email.eEmailMethod.OUTLOOK.ToString())
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