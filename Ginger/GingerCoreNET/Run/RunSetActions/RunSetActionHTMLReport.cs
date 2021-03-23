#region License
/*
Copyright © 2014-2021 European Support Limited

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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.DataSource;
using Ginger.Reports.GingerExecutionReport;
using Amdocs.Ginger.CoreNET.Logger;
using System.IO;
using Amdocs.Ginger.CoreNET.Utility;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionHTMLReport : RunSetActionBase
    {
        public new static class Fields
        {
            public static string HTMLReportFolderName = "HTMLReportFolderName";
            public static string isHTMLReportFolderNameUsed = "isHTMLReportFolderNameUsed";
            public static string isHTMLReportPermanentFolderNameUsed = "isHTMLReportPermanentFolderNameUsed";
            public static string selectedHTMLReportTemplateID = "selectedHTMLReportTemplateID";
        }

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
        private string mHTMLReportFolderNameCalculated;
        public string HTMLReportFolderNameCalculated
        {
            get
            {
                ValueExpression mVE = new ValueExpression(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
                mVE.Value = HTMLReportFolderName;
                return mHTMLReportFolderNameCalculated = mVE.ValueCalculated;
            }
        }
        [IsSerializedForLocalRepository]
        public int selectedHTMLReportTemplateID { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportFolderNameUsed { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportPermanentFolderNameUsed { get; set; }

        public override void Execute(ReportInfo RI)
        {
            string reportsResultFolder = string.Empty;
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if(WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
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
                    runSetFolder = gr.ExecutionLoggerManager.GetRunSetLastExecutionLogFolderOffline();                    
                }

                string currentHTMLFolderName = string.Empty;
                if (isHTMLReportFolderNameUsed && !String.IsNullOrEmpty(HTMLReportFolderName))
                {
                    if (!isHTMLReportPermanentFolderNameUsed)
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

                if (!string.IsNullOrEmpty(selectedHTMLReportTemplateID.ToString()))
                {
                    ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                        
                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                false,
                                                                                                                                HTMLReportConfigurations.Where(x => (x.ID == selectedHTMLReportTemplateID)).FirstOrDefault(),
                                                                                                                                currentHTMLFolderName,
                                                                                                                                isHTMLReportPermanentFolderNameUsed, currentConf.HTMLReportConfigurationMaximalFolderSize);
                }
                else
                {
                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                                false,
                                                                                                                                null,
                                                                                                                                currentHTMLFolderName,
                                                                                                                                isHTMLReportPermanentFolderNameUsed);
                }
            }
            else
            {
                Errors = "In order to get HTML report, please, perform executions before";
                Reporter.HideStatusMessage();
                Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                return;
            }
        }

        private void ProduceLiteDBReportFolder(HTMLReportsConfiguration currentConf)
        {
            string reportsResultFolder;
            WebReportGenerator webReporterRunner = new WebReportGenerator();
            string reportName = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;
            if (isHTMLReportFolderNameUsed && !String.IsNullOrEmpty(HTMLReportFolderName))
            {
                reportsResultFolder = Path.Combine(HTMLReportFolderName, "Reports", "Ginger-Web-Client");
            }
            else
            {
                reportsResultFolder = Path.Combine(Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder), "Reports", "Ginger-Web-Client");
            }
            if (!String.IsNullOrEmpty(reportsResultFolder))
            {
                webReporterRunner.RunNewHtmlReport(null, null, true);
            }
            string clientAppFolderPath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports", "Ginger-Web-Client");
            if (isHTMLReportPermanentFolderNameUsed)
            {
                if (!Directory.Exists(reportsResultFolder))
                {
                    IoHandler.Instance.CopyFolderRec(clientAppFolderPath, reportsResultFolder, true);
                }
                else
                {
                    webReporterRunner.DeleteFoldersData(Path.Combine(reportsResultFolder, "assets", "Execution_Data"));
                    webReporterRunner.DeleteFoldersData(Path.Combine(reportsResultFolder, "assets", "screenshots"));
                    IoHandler.Instance.CopyFolderRec(Path.Combine(clientAppFolderPath, "assets", "Execution_Data"), Path.Combine(reportsResultFolder, "assets", "Execution_Data"), true);
                    IoHandler.Instance.CopyFolderRec(Path.Combine(clientAppFolderPath, "assets", "screenshots"), Path.Combine(reportsResultFolder, "assets", "screenshots"), true);

                }
            }
            else
            {
                IoHandler.Instance.CopyFolderRec(clientAppFolderPath, $"{reportsResultFolder}_{reportName}_{DateTime.UtcNow.ToString("yyyymmddhhmmss")}", true);
            }
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
