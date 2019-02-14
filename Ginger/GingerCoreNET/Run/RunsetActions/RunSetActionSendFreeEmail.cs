using System;
using System.Collections.Generic;
using System.Text;
using amdocs.ginger.GingerCoreNET;
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
                    mValueExpression = new ValueExpression(WorkSpace.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
                }
                return mValueExpression;
            }
        }

        private string mBodytext;
        [IsSerializedForLocalRepository]
        public string Bodytext { get { return mBodytext; } set { if (mBodytext != value) { mBodytext = value; OnPropertyChanged(nameof(Bodytext)); } } }

        private string mMailFrom;
        [IsSerializedForLocalRepository]
        public string MailFrom { get { return mMailFrom; } set { if (mMailFrom != value) { mMailFrom = value; OnPropertyChanged(nameof(MailFrom)); } } }

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
            if (isSuccess == false)
            {
                Errors = Email.Event;
                Reporter.HideStatusMessage();
                Status = RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }

        public override string GetEditPage()
        {
            //RunSetActionSendFreeEmailEditPage RSAEREP = new RunSetActionSendFreeEmailEditPage(this);
            return "RunSetActionSendFreeEmailEditPage";
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
