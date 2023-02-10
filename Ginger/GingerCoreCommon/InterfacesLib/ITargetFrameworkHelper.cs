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

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionAutoSaveAndRecover;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SourceControl;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Amdocs.Ginger.Common
{
    public enum eExecutedFrom
    {
        Automation,
        Run
    }


    public interface ITargetFrameworkHelper
    {
        IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow);
        IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList);
        IValueExpression CreateValueExpression(Object obj, string attr);

        Type GetDriverType(IAgent agent);

        object GetDriverObject(IAgent agent);

        void ShowAutoRunWindow();

        bool Send_Outlook(bool actualSend = true, string MailTo = null, string Event = null, string Subject = null, string Body = null, string MailCC = null, List<string> Attachments = null, List<KeyValuePair<string, string>> EmbededAttachment = null);

        void DisplayAsOutlookMail();

        void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempfolder);

        void CreateCustomerLogo(Object a, string t);
        Dictionary<string, string> TakeDesktopScreenShot(bool v);
        Bitmap GetBrowserHeaderScreenshot(Point windowPosition, Size windowSize, Size viewportSize, double devicePixelRatio);
        Bitmap GetTaskbarScreenshot();
        string MergeVerticallyAndSaveBitmaps(params Bitmap[] bitmaps);

        void ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence);

        // string GenerateReportForREportTemplate(string ReportTemplateName, object RI, object RT);

        // string GenerateTemplate(string templatename, object o);
        ITextBoxFormatter CreateTextBoxFormatter(object Textblock);

        string GetALMConfig();

        void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType);

        void HTMLReportAttachment(string report, ref string emailReadyHtml, ref string reportresultfolder, string runsetfolder, object Attachment, object conf);

        object CreateNewReportTemplate();

        bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string refe, PublishToALMConfig PublishToALMConfig);



        bool GetLatest(string path, SourceControlBase SourceControl);

        bool Revert(string path, SourceControlBase SourceControl);

        void ShowRecoveryItemPage(ObservableList<RecoveredItem> recovredItems);

        SourceControlBase GetNewSVnRepo();
        void WaitForAutoRunWindowClose();

        IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow);

        void DispatcherRun();

        DbConnection GetOracleConnection(string ConnectionString);

        bool IsSharedRepositoryItem(RepositoryItemBase item);
        bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, eALMConnectType almConnectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null);
    }
}
