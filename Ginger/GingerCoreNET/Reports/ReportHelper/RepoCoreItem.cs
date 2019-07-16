using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionAutoSaveAndRecover;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Text;

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

        public void CreateCustomerLogo(object a, string t)
        {
            operationHandlerObj.CreateCustomerLogo(a, t);
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

        public void DownloadSolution(string v)
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

        public void HTMLReportAttachment(string report, ref string emailReadyHtml, ref string reportresultfolder, string runsetfolder, object Attachment, object conf)
        {
            throw new NotImplementedException();
        }

        public bool Send_Outlook(bool actualSend = true, string MailTo = null, string Event = null, string Subject = null, string Body = null, string MailCC = null, List<string> Attachments = null, List<KeyValuePair<string, string>> EmbededAttachment = null)
        {
            throw new NotImplementedException();
        }

        public void ShowAutoRunWindow()
        {
            throw new NotImplementedException();
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
