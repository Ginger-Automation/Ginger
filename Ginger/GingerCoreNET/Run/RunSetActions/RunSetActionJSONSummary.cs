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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Reports;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunSetActions
{
    public class RunSetActionJSONSummary : RunSetActionBase
    {
        public override bool SupportRunOnConfig
        {
            get { return true; }
        }

        public override string Type { get { return "Save JSON Summary"; } }

        public override void Execute(ReportInfo RI)
        {
            string json = WorkSpace.Instance.RunsetExecutor.CreateSummary();
            string timestamp = DateTime.Now.ToString("MMddyyyy_HHmmss");
            string fileName = Path.Combine(WorkSpace.Instance.Solution.Folder, "ExecutionResults", WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "_" + timestamp + ".json.txt");
            System.IO.File.WriteAllText(fileName, json);
        }

        public override string GetEditPage()
        {
            return "RunSetActionSummaryJSONPage";
        }

        public override List<eRunAt> GetRunOptions()
        {
            List<eRunAt> list = new List<eRunAt>();            
            list.Add(eRunAt.ExecutionEnd);
            return list;
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }
    }
}
