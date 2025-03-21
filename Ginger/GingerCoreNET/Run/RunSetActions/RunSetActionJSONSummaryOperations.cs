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

using amdocs.ginger.GingerCoreNET;
using Ginger.Reports;
using System;
using System.IO;

namespace Amdocs.Ginger.CoreNET.Run.RunSetActions
{
    public class RunSetActionJSONSummaryOperations : IRunSetActionJSONSummaryOperations
    {
        public RunSetActionJSONSummary RunSetActionJSONSummary;
        public RunSetActionJSONSummaryOperations(RunSetActionJSONSummary runSetActionJSONSummary)
        {
            this.RunSetActionJSONSummary = runSetActionJSONSummary;
            this.RunSetActionJSONSummary.RunSetActionJSONSummaryOperations = this;
        }
        public void Execute(IReportInfo RI)
        {
            string json = WorkSpace.Instance.RunsetExecutor.CreateSummary();
            string timestamp = DateTime.Now.ToString("MMddyyyy_HHmmss");

            string jsonSummaryFolder = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
            if (!string.IsNullOrEmpty(WorkSpace.Instance.TestArtifactsFolder))
            {
                jsonSummaryFolder = WorkSpace.Instance.TestArtifactsFolder;
            }
            string fileName = Path.Combine(jsonSummaryFolder, WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name + "_" + timestamp + ".json.txt");//why not as .json?                
            System.IO.File.WriteAllText(fileName, json);
        }
    }
}
