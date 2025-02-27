#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.ALM;
using GingerCore.GeneralLib;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendSMS : RunSetActionBase
    {
        public IRunSetActionSendSMSOperations RunSetActionSendSMSOperations;
        [IsSerializedForLocalRepository]
        public Email SMSEmail = new Email();

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = [RunSetActionBase.eRunAt.ExecutionStart, RunSetActionBase.eRunAt.ExecutionEnd];
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        public override void Execute(IReportInfo RI)
        {
            RunSetActionSendSMSOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
            // RunSetActionSendSMSEditPage p = new RunSetActionSendSMSEditPage(this);
            return "RunSetActionSendSMSEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override PublishToALMConfig.eALMTestSetLevel GetAlMTestSetLevel()
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send SMS"; } }
    }
}
