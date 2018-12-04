using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.GeneralLib;

namespace Amdocs.Ginger.CoreNET.Run.RunsetActions
{
    public class RunSetActionSendSMS : RunSetActionBase
    {
        [IsSerializedForLocalRepository]
        public Email SMSEmail = new Email();

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionStart);
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        public override void Execute(IReportInfo RI)
        {
            //TODO: check number of chars and show err if more or update Errors field
            SMSEmail.Send();
        }

        public override string GetEditPage()
        {
           // RunSetActionSendSMSEditPage p = new RunSetActionSendSMSEditPage(this);
            return "RunSetActionSendSMSEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<IGingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send SMS"; } }
    }
}
