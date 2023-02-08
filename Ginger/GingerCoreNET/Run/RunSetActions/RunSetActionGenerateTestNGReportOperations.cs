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
using Ginger.Run.RunSetActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ginger.Reports;
using Amdocs.Ginger.Common;
using GingerCore;
using amdocs.ginger.GingerCoreNET;
using static Ginger.Run.RunSetActions.RunSetActionBase;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionGenerateTestNGReportOperations : IRunSetActionGenerateTestNGReportOperations
    {
        public RunSetActionGenerateTestNGReport RunSetActionGenerateTestNGReport;
        public RunSetActionGenerateTestNGReportOperations(RunSetActionGenerateTestNGReport runSetActionGenerateTestNGReport)
        {
            this.RunSetActionGenerateTestNGReport = runSetActionGenerateTestNGReport;
            this.RunSetActionGenerateTestNGReport.RunSetActionGenerateTestNGReportOperations = this;
        }
        public void Execute(IReportInfo RI)
        {
            string testNGReportPath = "";
            try
            {
                if (!string.IsNullOrEmpty(RunSetActionGenerateTestNGReport.SaveResultsInSolutionFolderName))
                {
                    testNGReportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(RunSetActionGenerateTestNGReport.SaveResultsInSolutionFolderName);
                }
                else if (!string.IsNullOrEmpty(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.TestArtifactsFolder))
                {
                    testNGReportPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.TestArtifactsFolder;
                }
                else
                {
                    testNGReportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                }
                if (!Directory.Exists(testNGReportPath))
                {
                    try
                    {
                        Directory.CreateDirectory(testNGReportPath);
                    }
                    catch (Exception ex)
                    {
                        testNGReportPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                    }
                }
                SaveBFResults((ReportInfo)RI, testNGReportPath, RunSetActionGenerateTestNGReport.IsStatusByActivitiesGroup);
            }
            catch (Exception ex)
            {
                RunSetActionGenerateTestNGReport.Errors = ex.Message.ToString();
                RunSetActionGenerateTestNGReport.Status = eRunSetActionStatus.Failed;
            }
        }

        //TODO: move to Run SetAction
        private void SaveBFResults(ReportInfo RI, string folder, bool statusByGroupActivity)
        {
            if (RunSetActionGenerateTestNGReport.DynamicParameters.Count > 0)
            {
                ValueExpression VE = new ValueExpression(RI.Environment, null);
                for (int i = 0; i < RunSetActionGenerateTestNGReport.DynamicParameters.Count; i++)
                {
                    RunSetActionGenerateTestNGReport.DynamicParameters[i].ValueForDriver = VE.Calculate(RunSetActionGenerateTestNGReport.DynamicParameters[i].Value);
                }
            }
            TestNGResultReport TNGReport = new TestNGResultReport();
            string xml = TNGReport.CreateReport(RI, statusByGroupActivity, RunSetActionGenerateTestNGReport.DynamicParameters);

            System.IO.File.WriteAllLines(Path.Combine(folder, "testng-results.xml"), new string[] { xml });
            //TODO: let the user select file prefix
        }


    }
}
