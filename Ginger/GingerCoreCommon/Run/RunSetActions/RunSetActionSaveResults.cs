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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Execution;


using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionSaveResults : RunSetActionBase
    {
        public IRunSetActionSaveResultsOperations RunSetActionSaveResultsOperations;

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
        public string SaveResultsInSolutionFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public string SaveResultstoFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public string TemplateName { get; set; }

        [IsSerializedForLocalRepository]
        public bool OpenExecutionResultsFolder { get; set; }

        [IsSerializedForLocalRepository]
        public bool SaveindividualBFReport { get; set; }

        public override void Execute(IReportInfo RI)
        {
            RunSetActionSaveResultsOperations.Execute(RI);
        }
        
        public override string GetEditPage()
        {
            //RunSetActionSaveResultsEditPage p = new RunSetActionSaveResultsEditPage(this);
            return "RunSetActionSaveResultsEditPage";
        }


        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }



        public override string Type { get { return "Save Results"; } }
    }
}
