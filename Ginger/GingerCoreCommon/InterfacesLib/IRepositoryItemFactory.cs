using System;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using Ginger.Run;
using System.Threading.Tasks;
using System.Collections.Generic;
using GingerCore.ALM;
using GingerCore.Activities;

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

namespace Amdocs.Ginger.Common
{
    public enum eExecutedFrom
    {
        Automation,
        Run
    }
    

    public interface IRepositoryItemFactory
    {        
        //BusinessFlow CreateBusinessFlow();
        //ObservableList<BusinessFlow> GetListofBusinessFlow();

        IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow);

        IValueExpression CreateValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null);

        IValueExpression CreateValueExpression(Object obj, string attr);

        ObservableList<IDatabase> GetDatabaseList();
        ObservableList<VariableBase> GetVariaables();
        ObservableList<IAgent> GetAllIAgents();

        ObservableList<DataSourceBase> GetDatasourceList();

        void StartAgentDriver(IAgent agent);
        Type GetDriverType(IAgent agent);

        Type GetPage(string a);

        Task<int> AnalyzeRunset(Object a, bool b);

        void RunRunSetFromCommandLine();

        bool Send_Outlook(bool actualSend = true, string MailTo=null, string Event=null, string Subject=null, string Body=null, string MailCC=null, List<string> Attachments=null, List<KeyValuePair<string, string>> EmbededAttachment=null);

        void DisplayAsOutlookMail();

        void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempfolder);

        void CreateCustomerLogo(Object a, string t);
        Dictionary<string, string> TakeDesktopScreenShot(bool v);
        
        void ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence);

        string GenerateReportForREportTemplate(string ReportTemplateName, object RI, object RT);

        string GenerateTemplate(string templatename, object o);
        ITextBoxFormatter CreateTextBoxFormatter(object Textblock);

        bool ProcessCommandLineArgs(string[] file);

        void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening);

        void HTMLReportAttachment(string report, string reportsResultFolder, string reportresultfolder, string runsetfolder, object Attachment, object conf);

        object CreateNewReportTemplate();

        void ExecuteActScriptAction(string ScriptFileName, string SolutionFolder);
        bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, string refe, PublishToALMConfig PublishToALMConfig);
    }
}
