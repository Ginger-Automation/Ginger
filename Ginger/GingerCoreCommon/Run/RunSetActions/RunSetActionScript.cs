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
using Ginger.Reports;
using GingerCore.Actions;
using Amdocs.Ginger.Common.InterfacesLib;
using System.Runtime.InteropServices;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionScript : RunSetActionBase
    {
        public IRunSetActionScriptOperations RunSetActionScriptOperations;
        [IsSerializedForLocalRepository]
        public string ScriptFileName { get; set; }

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
            RunSetActionScriptOperations.Execute(RI);
        }

        public override string GetEditPage()
        {
            //RunSetActionScriptEditPage p = new RunSetActionScriptEditPage(this);
            return "RunSetActionScriptEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Run Script"; } }
        
    }
}
