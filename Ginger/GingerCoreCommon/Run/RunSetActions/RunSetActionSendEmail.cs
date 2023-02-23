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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSendEmail : RunSetActionBase
    {
        public IRunSetActionSendEmailOperations RunSetActionSendEmailOperations;
        public enum eHTMLReportTemplate
        {
            [EnumValueDescription("Free Text")]
            FreeText,
            Summary,
            Detailed,
            Plain,
            Custom
        }

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

        [IsSerializedForLocalRepository]
        public Email Email = new Email();

        //User can attach several templates to the email
        // Attach template + RI
        // Attach its own file
        [IsSerializedForLocalRepository]
        public ObservableList<EmailAttachment> EmailAttachments = new ObservableList<EmailAttachment>();

        [IsSerializedForLocalRepository]
        public eHTMLReportTemplate HTMLReportTemplate { get; set; }
        [IsSerializedForLocalRepository]
        public string CustomHTMLReportTemplate { get; set; }

        public override void Execute(IReportInfo RI)
        {
            RunSetActionSendEmailOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
            //RunSetActionSendEmailEditPage RSAEREP = new RunSetActionSendEmailEditPage(this);
            return "RunSetActionSendEmailEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Send Email"; } }
    }
}
