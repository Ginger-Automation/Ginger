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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Reports;
using Amdocs.Ginger;
//using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.DataSource;
//using Ginger.Reports.GingerExecutionkReport;
//using Amdocs.Ginger.CoreNET.Logger;
using System.IO;
//using Amdocs.Ginger.CoreNET.Utility;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionHTMLReport : RunSetActionBase
    {
        public IRunSetActionHTMLReportOperations RunSetActionHTMLReportOperations;

        public override List<RunSetActionBase.eRunAt> GetRunOptions()
        {
            List<RunSetActionBase.eRunAt> list = new List<RunSetActionBase.eRunAt>();
            list.Add(RunSetActionBase.eRunAt.ExecutionEnd);
            return list;
        }

        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        private string mHTMLReportFolderName;
        [IsSerializedForLocalRepository]
        public string HTMLReportFolderName
        {
            get
            {
                return mHTMLReportFolderName;
            }
            set
            {
                mHTMLReportFolderName = value;
                OnPropertyChanged(nameof(HTMLReportFolderName));
            }
        }
        
        [IsSerializedForLocalRepository]
        public int selectedHTMLReportTemplateID { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportFolderNameUsed { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportPermanentFolderNameUsed { get; set; }

        public override void Execute(IReportInfo RI)
        {
            RunSetActionHTMLReportOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
           // RunSetActionHTMLReportEditPage p = new RunSetActionHTMLReportEditPage(this);
            return "RunSetActionHTMLReportEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Produce HTML Report"; } }
    }
}
