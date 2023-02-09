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
    public class RunSetActionSendFreeEmailOperations : IRunSetActionSendFreeEmailOperations
    {

        public RunSetActionSendFreeEmail RunSetActionSendFreeEmail;
        public RunSetActionSendFreeEmailOperations(RunSetActionSendFreeEmail runSetActionSendFreeEmail)
        {
            this.RunSetActionSendFreeEmail = runSetActionSendFreeEmail;
            this.RunSetActionSendFreeEmail.RunSetActionSendFreeEmailOperations = this;
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


        public void Execute(IReportInfo RI)
        {
            EmailOperations emailOperations = new EmailOperations(RunSetActionSendFreeEmail.Email);
            RunSetActionSendFreeEmail.Email.EmailOperations = emailOperations;

            RunSetActionSendFreeEmail.Email.Attachments.Clear();
            RunSetActionSendFreeEmail.Email.EmailOperations.alternateView = null;

            mVE.Value = RunSetActionSendFreeEmail.MailFrom;
            RunSetActionSendFreeEmail.Email.MailFrom = mVE.ValueCalculated;

            mVE.Value = RunSetActionSendFreeEmail.MailFromDisplayName;
            RunSetActionSendFreeEmail.Email.MailFromDisplayName = mVE.ValueCalculated;

            mVE.Value = RunSetActionSendFreeEmail.MailTo;
            RunSetActionSendFreeEmail.Email.MailTo = mVE.ValueCalculated;
            mVE.Value = RunSetActionSendFreeEmail.MailCC;
            RunSetActionSendFreeEmail.Email.MailCC = mVE.ValueCalculated;
            mVE.Value = RunSetActionSendFreeEmail.Subject;
            RunSetActionSendFreeEmail.Email.Subject = mVE.ValueCalculated;
            mVE.Value = RunSetActionSendFreeEmail.Bodytext;
            RunSetActionSendFreeEmail.Email.Body = mVE.ValueCalculated;
            mVE.Value = RunSetActionSendFreeEmail.MailHost;
            RunSetActionSendFreeEmail.Email.SMTPMailHost = mVE.ValueCalculated;
            mVE.Value = RunSetActionSendFreeEmail.MailUser;
            RunSetActionSendFreeEmail.Email.SMTPUser = mVE.ValueCalculated;
            bool isSuccess;
            isSuccess = RunSetActionSendFreeEmail.Email.EmailOperations.Send();
            if (isSuccess == false)
            {
                RunSetActionSendFreeEmail.Errors = RunSetActionSendFreeEmail.Email.Event;
                Reporter.HideStatusMessage();
                RunSetActionSendFreeEmail.Status = RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }

        public static string OverrideHTMLRelatedCharacters(string text)
        {
            text = text.Replace(@"<", "&#60;");
            text = text.Replace(@">", "&#62;");
            text = text.Replace(@"$", "&#36;");
            text = text.Replace(@"%", "&#37;");

            return text;
        }

    }
}
