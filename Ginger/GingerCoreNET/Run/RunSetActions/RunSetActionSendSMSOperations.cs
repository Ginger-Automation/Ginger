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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendSMSOperations : IRunSetActionSendSMSOperations
    {
        public RunSetActionSendSMS RunSetActionSendSMS;
        public RunSetActionSendSMSOperations(RunSetActionSendSMS runSetActionSendSMS)
        {
            this.RunSetActionSendSMS = runSetActionSendSMS;
            this.RunSetActionSendSMS.RunSetActionSendSMSOperations = this;
        }

        public void Execute(IReportInfo RI)
        {
            EmailOperations emailOperations = new EmailOperations(RunSetActionSendSMS.SMSEmail);
            RunSetActionSendSMS.SMSEmail.EmailOperations = emailOperations;

            //TODO: check number of chars and show err if more or update Errors field           
            RunSetActionSendSMS.SMSEmail.IsBodyHTML = false;

            bool isSuccess;
            isSuccess = RunSetActionSendSMS.SMSEmail.EmailOperations.Send();
            if (isSuccess == false)
            {
                RunSetActionSendSMS.Errors = RunSetActionSendSMS.SMSEmail.Event;
                Reporter.HideStatusMessage();
                RunSetActionSendSMS.Status = RunSetActionBase.eRunSetActionStatus.Failed;
            }
        }

    }
}
