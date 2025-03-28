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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Logger;
using Ginger.Reports;
using GingerCore;
using GingerCore.DataSource;
using System;
using System.IO;
using System.Linq;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionHTMLReportOperations : IRunSetActionHTMLReportOperations
    {
        public RunSetActionHTMLReport RunSetActionHTMLReport;
        public RunSetActionHTMLReportOperations(RunSetActionHTMLReport runSetActionHTMLReport)
        {
            RunSetActionHTMLReport = runSetActionHTMLReport;
            runSetActionHTMLReport.RunSetActionHTMLReportOperations = this;

        }
        private string mHTMLReportFolderNameCalculated;
        public string HTMLReportFolderNameCalculated
        {
            get
            {
                ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false)
                {
                    Value = RunSetActionHTMLReport.HTMLReportFolderName
                };
                return mHTMLReportFolderNameCalculated = mVE.ValueCalculated;
            }
        }

        public void Execute(IReportInfo RI)
        {
            if (!WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Any(htmlRC => htmlRC.IsDefault))
            {
                Reporter.ToLog(eLogLevel.ERROR, "No Default HTML Report template available to generate report. Please set a default template in Configurations -> Reports -> Reports Template.");
                RunSetActionHTMLReport.Errors = "No Default HTML Report template available to generate report. Please set a default template in Configurations -> Reports -> Reports Template.";
                Reporter.HideStatusMessage();
                RunSetActionHTMLReport.Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                return;
            }

            string reportsResultFolder = string.Empty;
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                ProduceLiteDBReportFolder(currentConf);
                return;
            }

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
            {
                string runSetFolder = string.Empty;
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
                {
                    runSetFolder = WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                }
                else
                {
                    GingerRunner gr = new GingerRunner();
                    runSetFolder = gr.Executor.ExecutionLoggerManager.GetRunSetLastExecutionLogFolderOffline();
                }

                string currentHTMLFolderName = string.Empty;
                if (RunSetActionHTMLReport.isHTMLReportFolderNameUsed && !String.IsNullOrEmpty(RunSetActionHTMLReport.HTMLReportFolderName))
                {
                    if (!RunSetActionHTMLReport.isHTMLReportPermanentFolderNameUsed)
                    {
                        currentHTMLFolderName = Path.Combine(HTMLReportFolderNameCalculated, System.IO.Path.GetFileName(runSetFolder));
                    }
                    else
                    {
                        currentHTMLFolderName = HTMLReportFolderNameCalculated;
                    }
                }
                if (String.IsNullOrEmpty(currentHTMLFolderName))
                {
                    if (!string.IsNullOrEmpty(WorkSpace.Instance.TestArtifactsFolder))
                    {
                        currentHTMLFolderName = Path.Combine(WorkSpace.Instance.TestArtifactsFolder, System.IO.Path.GetFileName(runSetFolder));
                    }
                }

                if (!string.IsNullOrEmpty(RunSetActionHTMLReport.selectedHTMLReportTemplateID.ToString()))
                {
                    ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();

                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                false,
                                                                                                                                HTMLReportConfigurations.FirstOrDefault(x => (x.ID == RunSetActionHTMLReport.selectedHTMLReportTemplateID)),
                                                                                                                                currentHTMLFolderName,
                                                                                                                                RunSetActionHTMLReport.isHTMLReportPermanentFolderNameUsed, currentConf.HTMLReportConfigurationMaximalFolderSize);
                }
                else
                {
                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                false,
                                                                                                                                null,
                                                                                                                                currentHTMLFolderName,
                                                                                                                                RunSetActionHTMLReport.isHTMLReportPermanentFolderNameUsed);
                }
            }
            else
            {
                RunSetActionHTMLReport.Errors = "In order to get HTML report, please, perform executions before";
                Reporter.HideStatusMessage();
                RunSetActionHTMLReport.Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                return;
            }
        }

        private void ProduceLiteDBReportFolder(HTMLReportsConfiguration currentConf)
        {
            string reportsResultFolder;
            WebReportGenerator webReporterRunner = new WebReportGenerator();
            string reportName = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;
            if (RunSetActionHTMLReport.isHTMLReportFolderNameUsed && !String.IsNullOrEmpty(RunSetActionHTMLReport.HTMLReportFolderName))
            {
                reportsResultFolder = Path.Combine(HTMLReportFolderNameCalculated, "Reports");
            }
            else
            {
                reportsResultFolder = Path.Combine(Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder), "Reports");
            }
            if (RunSetActionHTMLReport.isHTMLReportPermanentFolderNameUsed)
            {
                webReporterRunner.RunNewHtmlReport(Path.Combine(reportsResultFolder, "Ginger-Web-Client"), null, null, false);
            }
            else
            {
                webReporterRunner.RunNewHtmlReport(Path.Combine(reportsResultFolder, $"{reportName}_{DateTime.UtcNow:yyyymmddhhmmssfff}"), null, null, false);
            }
        }


    }
}
