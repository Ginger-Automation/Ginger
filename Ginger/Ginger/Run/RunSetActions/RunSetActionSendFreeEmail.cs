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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using System;
using System.Windows.Controls;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerCore.Repository;
using System.IO.Compression;
using Ginger.Reports.HTMLReports;
using System.Collections.Generic;
using amdocs.ginger.GingerCoreNET;
using GingerCore.DataSource;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendFreeEmail : RunSetActionBase
    {
        public new static class Fields
        {
            public static string HTMLReportTemplate = "HTMLReportTemplate";
            public static string Bodytext = "Bodytext";
            public static string MailFrom = "MailFrom";
            public static string MailTo = "MailTo";
            public static string MailCC = "MailCC";
            public static string Subject = "Subject";
            public static string MailUser = "MailUser";
            public static string MailHost = "MailHost";
        }

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

        ValueExpression mValueExpression = null;        
        ValueExpression mVE
        {
            get
            {
                if (mValueExpression == null)
                {
                    mValueExpression = new ValueExpression(App.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false, App.UserProfile.Solution.Variables);
                }
                return mValueExpression;
            }
        }

        private string mBodytext;
        [IsSerializedForLocalRepository]
        public string Bodytext { get { return mBodytext; } set { if (mBodytext != value) { mBodytext = value; OnPropertyChanged(Fields.Bodytext); } } }
        
        private string mMailFrom;
        [IsSerializedForLocalRepository]
        public string MailFrom { get { return mMailFrom; } set { if (mMailFrom != value) { mMailFrom = value; OnPropertyChanged(Fields.MailFrom); } } }
        
        private string mMailCC;
        [IsSerializedForLocalRepository]
        public string MailCC { get { return mMailCC; } set { if (mMailCC != value) { mMailCC = value; OnPropertyChanged(Fields.MailCC); } } }

        private string mSubject;
        [IsSerializedForLocalRepository]
        public string Subject { get { return mSubject; } set { if (mSubject != value) { mSubject = value; OnPropertyChanged(Fields.Subject); } } }

        private string mMailTo;
        [IsSerializedForLocalRepository]
        public string MailTo { get { return mMailTo; } set { if (mMailTo != value) { mMailTo = value; OnPropertyChanged(Fields.MailTo); } } }

        private string mMailHost;
        [IsSerializedForLocalRepository]
        public string MailHost { get { return mMailHost; } set { if (mMailHost != value) { mMailHost = value; OnPropertyChanged(Fields.MailHost); } } }

        private string mMailUser;
        [IsSerializedForLocalRepository]
        public string MailUser { get { return mMailUser; } set { if (mMailUser != value) { mMailUser = value; OnPropertyChanged(Fields.MailUser); } } }
     
        public override void Execute(ReportInfo RI)
        {
            Email.Attachments.Clear();
            Email.alternateView = null;

            mVE.Value = MailFrom;
            Email.MailFrom = mVE.ValueCalculated;
            mVE.Value = MailTo;
            Email.MailTo = mVE.ValueCalculated;
            mVE.Value = MailCC;
            Email.MailCC = mVE.ValueCalculated;
            mVE.Value = Subject;
            Email.Subject = mVE.ValueCalculated;
            mVE.Value = Bodytext;
            Email.Body = mVE.ValueCalculated;
            mVE.Value = MailHost;
            Email.SMTPMailHost = mVE.ValueCalculated;
            mVE.Value = MailUser;
            Email.SMTPUser = mVE.ValueCalculated;           
            bool isSuccess;
            isSuccess = Email.Send();
            if(isSuccess == false)
            {
                Errors = Email.Event;
                Reporter.CloseGingerHelper();
                Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }
        
        public override Page GetEditPage()
        {
            RunSetActionSendFreeEmailEditPage RSAEREP = new RunSetActionSendFreeEmailEditPage(this);
            return RSAEREP;
        }

        public static string OverrideHTMLRelatedCharacters(string text)
        {
            text = text.Replace(@"<", "&#60;");
            text = text.Replace(@">", "&#62;");
            text = text.Replace(@"$", "&#36;");
            text = text.Replace(@"%", "&#37;");

            return text;
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send Free Text Email"; } }
    }
}