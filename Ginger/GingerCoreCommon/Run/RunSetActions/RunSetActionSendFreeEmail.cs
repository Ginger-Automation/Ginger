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
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;

using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using GingerCore;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendFreeEmail : RunSetActionBase
    {
        public IRunSetActionSendFreeEmailOperations RunSetActionSendFreeEmailOperations;
        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionStart);
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        [IsSerializedForLocalRepository]
        public Email Email = new Email();

        private string mBodytext;
        [IsSerializedForLocalRepository]
        public string Bodytext { get { return mBodytext; } set { if (mBodytext != value) { mBodytext = value; OnPropertyChanged(nameof(Bodytext)); } } }

        private string mMailFrom;
        [IsSerializedForLocalRepository]
        public string MailFrom { get { return mMailFrom; } set { if (mMailFrom != value) { mMailFrom = value; OnPropertyChanged(nameof(MailFrom)); } } }

        private string mMailFromDisplayName;
        [IsSerializedForLocalRepository]
        public string MailFromDisplayName { get { return mMailFromDisplayName; } set { if (mMailFromDisplayName != value) { mMailFromDisplayName = value; OnPropertyChanged(nameof(mMailFromDisplayName)); } } }

        private string mMailCC;
        [IsSerializedForLocalRepository]
        public string MailCC { get { return mMailCC; } set { if (mMailCC != value) { mMailCC = value; OnPropertyChanged(nameof(MailCC)); } } }

        private string mSubject;
        [IsSerializedForLocalRepository]
        public string Subject { get { return mSubject; } set { if (mSubject != value) { mSubject = value; OnPropertyChanged(nameof(Subject)); } } }

        private string mMailTo;
        [IsSerializedForLocalRepository]
        public string MailTo { get { return mMailTo; } set { if (mMailTo != value) { mMailTo = value; OnPropertyChanged(nameof(MailTo)); } } }
       
        public string MailHost
        {
            get
            {
                return Email.SMTPMailHost;
            }
            set
            {
                if (Email.SMTPMailHost != value)
                {
                    Email.SMTPMailHost = value;                    
                }
            }
        }
       
        public string MailUser
        {
            get
            {
                return Email.SMTPUser;
            }
            set
            {
                if (Email.SMTPUser != value)
                {
                    Email.SMTPUser = value;                   
                }
            }
        }

        public override void Execute(IReportInfo RI)
        {
            RunSetActionSendFreeEmailOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
            //RunSetActionSendFreeEmailEditPage RSAEREP = new RunSetActionSendFreeEmailEditPage(this);
            return "RunSetActionSendFreeEmailEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send Text Email"; } }
    }
}
