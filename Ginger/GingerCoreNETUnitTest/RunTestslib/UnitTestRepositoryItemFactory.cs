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
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.RunTestslib
{
    public class UnitTestRepositoryItemFactory : IRepositoryItemFactory
    {
        public Task<int> AnalyzeRunset(object a, bool b)
        {
            throw new NotImplementedException();
        }

        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempfolder)
        {
            throw new NotImplementedException();
        }

        public void CreateCustomerLogo(object a, string t)
        {
            throw new NotImplementedException();
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

        public string GenerateReportForREportTemplate(string ReportTemplateName, object RI, object RT)
        {
            throw new NotImplementedException();
        }

        public string GenerateTemplate(string templatename, object o)
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

        public bool ProcessCommandLineArgs(string[] file)
        {
            throw new NotImplementedException();
        }

        public void ShowAutoRunWindow()
        {
            Console.WriteLine("UnitTestRepositoryItemFactory: Show auto run window");
        }

        public bool Send_Outlook(bool actualSend = true, string MailTo = null, string Event = null, string Subject = null, string Body = null, string MailCC = null, List<string> Attachments = null, List<KeyValuePair<string, string>> EmbededAttachment = null)
        {
            throw new NotImplementedException();
        }

        public void StartAgentDriver(IAgent agent)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool v)
        {
            throw new NotImplementedException();
        }

        public void DownloadSolution(string v)
        {
            throw new NotImplementedException();
        }

        public void ShowRecoveryItemPage(ObservableList<RecoveredItem> recovredItems)
        {
            throw new NotImplementedException();
        }
    }
}
