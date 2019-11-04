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

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionGenerateTestNGReport : RunSetActionBase
    {
        public new static class Fields
        {
            public static string SaveResultstoFolderName = "SaveResultstoFolderName";
            public static string OpenExecutionResultsFolder = "OpenExecutionResultsFolder";
            public static string SaveindividualBFReport = "SaveindividualBFReport";
        }
        [IsSerializedForLocalRepository(false)]
        public bool ConfiguerDynamicParameters { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicParameters = new ObservableList<ActInputValue>();
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

        [IsSerializedForLocalRepository]
        public string SaveResultsInSolutionFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsStatusByActivitiesGroup { get; set; }

        private bool isStatusByActivity = true;
        [IsSerializedForLocalRepository]
        public bool IsStatusByActivity
        {
            get { return isStatusByActivity; }
            set { isStatusByActivity = value; }
        }

        public override void Execute(ReportInfo RI)
        {
            try
            {
                string testNGReportPath = "";

                if (!string.IsNullOrEmpty(SaveResultsInSolutionFolderName))
                {
                    testNGReportPath = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(SaveResultsInSolutionFolderName);
                }
                else
                {
                    testNGReportPath = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                }
                if (!Directory.Exists(testNGReportPath))
                {
                    try
                    {
                        Directory.CreateDirectory(testNGReportPath);
                    }
                    catch(Exception ex)
                    {
                        testNGReportPath = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                    }
                }
                SaveBFResults(RI, testNGReportPath, IsStatusByActivitiesGroup);
            }
            catch (Exception ex)
            {
                Errors = ex.Message.ToString();
                Status = eRunSetActionStatus.Failed;
            }
        }

        //TODO: move to Run SetAction
        private void SaveBFResults(ReportInfo RI, string folder, bool statusByGroupActivity)
        {
            if (DynamicParameters.Count > 0)
            {
                ValueExpression VE = new ValueExpression(RI.Environment, null);
                for(int i = 0;i< DynamicParameters.Count ; i++)
                {
                    DynamicParameters[i].ValueForDriver = VE.Calculate(DynamicParameters[i].Value);
                }
            }
            TestNGResultReport TNGReport = new TestNGResultReport();
            string xml = TNGReport.CreateReport(RI, statusByGroupActivity, DynamicParameters);

            System.IO.File.WriteAllLines(folder + @"\testng-results.xml", new string[] { xml });
        //TODO: let the user select file prefix
        }

        public override string GetEditPage()
        {
            //RunSetActionGenerateTestNGReportEditPage p = new RunSetActionGenerateTestNGReportEditPage(this);
            return "RunSetActionGenerateTestNGReportEditPage";
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Produce TestNG Summary Report"; } }
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound)
            {
                if (name == "SaveResultstoFolderName" || name == "OpenExecutionResultsFolder" || name == "SaveindividualBFReport")
                {
                    return true;
                }
            }
            return false;
        }
    }
}
