using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionAutoSaveAndRecover;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using Amdocs.Ginger.CoreNET.Drivers.WebServicesDriver;
using Ginger.Repository;
using System.Drawing;

namespace GingerCoreNETUnitTest.RunTestslib
{
    public class UnitTestRepositoryItemFactory : ITargetFrameworkHelper
    {
        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempfolder)
        {
          
        }

        public void CreateCustomerLogo(object a, string t)
        {
            
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
            return new ValueExpression(mProjEnvironment, mBusinessFlow, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList);
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            return new ValueExpression(obj, attr);
        }

        public void DisplayAsOutlookMail()
        {
            throw new NotImplementedException();
        }

        public void ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            throw new NotImplementedException();
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs,ref string refe, PublishToALMConfig PublishToALMConfig)
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
            return null;
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
            return true;
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

        public bool GetLatest(string path, SourceControlBase SourceControl)
        {
            throw new NotImplementedException();
        }

        public bool Revert(string path, SourceControlBase SourceControl)
        {
            throw new NotImplementedException();
        }

        public SourceControlBase GetNewSVnRepo()
        {
            throw new NotImplementedException();
        }

        public void WaitForAutoRunWindowClose()
        {
            // NA
        }

        public string GetALMConfig()
        {
            throw new NotImplementedException();
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, ALMIntegrationEnums.eALMType almType)
        {
            throw new NotImplementedException();
        }

        public object GetDriverObject(IAgent agent)
        {
            throw new NotImplementedException();
        }

        public IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow)
        {
            return new WebserviceDriverConsoleReporter();
        }

        public DbConnection GetOracleConnection(string ConnectionString)
        {
            throw new NotImplementedException();
        }

        public bool IsSharedRepositoryItem(RepositoryItemBase item)
        {
            return SharedRepositoryOperations.IsSharedRepositoryItem(item);
        }

        public void DispatcherRun()
        {
            //Not required
        }

        public bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, ALMIntegrationEnums.eALMConnectType almConnectStyle = ALMIntegrationEnums.eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            throw new NotImplementedException();
        }

        public Bitmap GetBrowserHeaderScreenshot(Point windowPosition, Size windowSize, Size viewportSize, double devicePixelRatio)
        {
            throw new NotImplementedException();
        }

        public Bitmap GetTaskbarScreenshot()
        {
            throw new NotImplementedException();
        }

        public string MergeVerticallyAndSaveBitmaps(params Bitmap[] bitmaps)
        {
            throw new NotImplementedException();
        }
    }
}
