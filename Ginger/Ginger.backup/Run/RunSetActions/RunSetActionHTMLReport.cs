#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Ginger.Reports;
using GingerCore;
using GingerCore.Repository;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;

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

        [IsSerializedForLocalRepository]
        public string HTMLReportFolderName { get; set; }

        [IsSerializedForLocalRepository]
        public int selectedHTMLReportTemplateID { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportFolderNameUsed { get; set; }

        [IsSerializedForLocalRepository]
        public bool isHTMLReportPermanentFolderNameUsed { get; set; }

        public override void Execute(ReportInfo RI)
        {
            string reportsResultFolder = string.Empty;
            HTMLReportsConfiguration currentConf = App.UserProfile.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (App.RunsetExecutor.RunSetConfig.RunsetExecLoggerPopulated)
            {
                string runSetFolder = string.Empty;
                if (App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
                { 
                    runSetFolder = App.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                    AutoLogProxy.UserOperationStart("Online Report");
                }
                else
                {
                    runSetFolder = ExecutionLogger.GetRunSetLastExecutionLogFolderOffline();
                    AutoLogProxy.UserOperationStart("Offline Report");
                }
                if (!string.IsNullOrEmpty(selectedHTMLReportTemplateID.ToString()))
                {
                    ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                    if ((isHTMLReportFolderNameUsed) && (HTMLReportFolderName != null) && (HTMLReportFolderName != string.Empty))
                    {
                        string currentHTMLFolderName = string.Empty;
                        if (!isHTMLReportPermanentFolderNameUsed)
                        {
                            currentHTMLFolderName = HTMLReportFolderName + "\\" + System.IO.Path.GetFileName(runSetFolder);
                        }
                        else
                        {
                            currentHTMLFolderName = HTMLReportFolderName;
                        }
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
                                                                                                                                HTMLReportConfigurations.Where(x => (x.ID == selectedHTMLReportTemplateID)).FirstOrDefault(),
                                                                                                                                null,
                                                                                                                                isHTMLReportPermanentFolderNameUsed);
                    }
                }
                else
                {
                    reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder), 
                                                                                                                                false,
                                                                                                                                null,
                                                                                                                                null,
                                                                                                                                isHTMLReportPermanentFolderNameUsed);
                }
            }
            else
            {
                Errors = "In order to get HTML report, please, perform executions before";
                Reporter.CloseGingerHelper();
                Status = Ginger.Run.RunSetActions.RunSetActionBase.eRunSetActionStatus.Failed;
                return;
            }
        }

        public override Page GetEditPage()
        {
            RunSetActionHTMLReportEditPage p = new RunSetActionHTMLReportEditPage(this);
            return p;
        }

        public override void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers)
        {
            throw new NotImplementedException();
        }

        public override string Type { get { return "Produce HTML Report"; } }
    }
}
