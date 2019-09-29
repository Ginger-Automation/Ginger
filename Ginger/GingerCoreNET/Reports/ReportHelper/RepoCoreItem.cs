﻿using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using Ginger.Run.RunSetActions;
using Ginger.SolutionAutoSaveAndRecover;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Reports.ReportHelper
{
    public class RepoCoreItem : IRepositoryItemFactory
    {
        private OperationHandler operationHandlerObj;

        public RepoCoreItem()
        {
            operationHandlerObj = new OperationHandler();
        }
        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string title, string tempfolder)
        {
            //operationHandlerObj.CreateChart(y, chartName, title, tempfolder);
        }
        public void CreateCustomerLogo(object a, string tempFolder)
        {
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields)
        {
            throw new NotImplementedException();
        }

        public object CreateNewReportTemplate()
        {
            throw new NotImplementedException();
        }

        public ITextBoxFormatter CreateTextBoxFormatter(object Textblock)
        {
            throw new NotImplementedException();
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow)
        {
            throw new NotImplementedException();
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList)
        {
            throw new NotImplementedException();
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            throw new NotImplementedException();
        }

        public void DisplayAsOutlookMail()
        {
            throw new NotImplementedException();
        }

       

        public void ExecuteActScriptAction(string ScriptFileName, string SolutionFolder)
        {
            throw new NotImplementedException();
        }

        public void ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            throw new NotImplementedException();
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, string refe, PublishToALMConfig PublishToALMConfig)
        {
            throw new NotImplementedException();
        }

        public Type GetDriverType(IAgent agent)
        {
            throw new NotImplementedException();
        }

        public bool GetLatest(string path, SourceControlBase SourceControl)
        {
          return  SourceControlIntegration.GetLatest(path, SourceControl);
        }

        public SourceControlBase GetNewSVnRepo()
        {
            throw new PlatformNotSupportedException("SVN Repositories are not supported yet on Ginger CLI");
        }

        public void HTMLReportAttachment(string extraInformationCalculated, ref string emailReadyHtml, ref string reportsResultFolder, string runSetFolder, object Report, object conf)
        {
            EmailHtmlReportAttachment rReport = (EmailHtmlReportAttachment)Report;
            HTMLReportsConfiguration currentConf = (HTMLReportsConfiguration)conf;
       
                emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
                ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                reportsResultFolder = ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                        false,
                                                                                                                        HTMLReportConfigurations.Where(x => (x.ID == rReport.SelectedHTMLReportTemplateID)).FirstOrDefault(),
                                                                                                                        extraInformationCalculated + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileName(runSetFolder), false, currentConf.HTMLReportConfigurationMaximalFolderSize);
            
        }

        public bool Send_Outlook(bool actualSend = true, string MailTo = null, string Event = null, string Subject = null, string Body = null, string MailCC = null, List<string> Attachments = null, List<KeyValuePair<string, string>> EmbededAttachment = null)
        {
            throw new NotImplementedException();
        }

        public void ShowAutoRunWindow()
        {            
            Reporter.ToLog(eLogLevel.WARN, "Show UI is set to true but not supported when running with GingerConsole");
        }

        public void WaitForAutoRunWindowClose()
        {
            // NA for GingerConsole
        }

        public void ShowRecoveryItemPage(ObservableList<RecoveredItem> recovredItems)
        {
            throw new NotImplementedException();
        }

        public void StartAgentDriver(IAgent IAgent)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool v)
        {
            throw new NotImplementedException();
        }


    }
}
