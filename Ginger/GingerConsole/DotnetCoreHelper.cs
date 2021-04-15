#region License
/*
Copyright © 2014-2020 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Drivers.WebServicesDriver;
using Amdocs.Ginger.CoreNET.SourceControl;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using Ginger.Run.RunSetActions;
using Ginger.SolutionAutoSaveAndRecover;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Drivers;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Mobile.Perfecto;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SourceControl;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Reports.ReportHelper
{
    public class DotnetCoreHelper : ITargetFrameworkHelper
    {
        private OperationHandler operationHandlerObj;

        public DotnetCoreHelper()
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

        public DbConnection GetOracleConnection(string ConnectionString)
        {
            return new OracleConnection(ConnectionString);
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, ALMIntegration.eALMType almType)
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

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string refe, PublishToALMConfig PublishToALMConfig)
        {
            throw new NotImplementedException();
        }

        public Type GetDriverType(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case Agent.eDriverType.SeleniumFireFox:                
                case Agent.eDriverType.SeleniumChrome:                    
                case Agent.eDriverType.SeleniumIE:                   
                case Agent.eDriverType.SeleniumRemoteWebDriver:                    
                case Agent.eDriverType.SeleniumEdge:
                    return (typeof(SeleniumDriver));
                case Agent.eDriverType.MobileAppiumAndroid:
                case Agent.eDriverType.MobileAppiumIOS:
                case Agent.eDriverType.MobileAppiumAndroidBrowser:
                case Agent.eDriverType.MobileAppiumIOSBrowser:
                    return (typeof(SeleniumAppiumDriver));
                case Agent.eDriverType.PerfectoMobileAndroid:
                case Agent.eDriverType.PerfectoMobileAndroidWeb:
                case Agent.eDriverType.PerfectoMobileIOS:
                case Agent.eDriverType.PerfectoMobileIOSWeb:
                    return (typeof(PerfectoDriver));
                case Agent.eDriverType.Appium:
                    return (typeof(GenericAppiumDriver));

                default:
                    throw new Exception("GetDriverType: Unknown Driver type " + zAgent.DriverType);
            }
        }

        public object GetDriverObject(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case Agent.eDriverType.SeleniumFireFox:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.FireFox);
                case Agent.eDriverType.SeleniumChrome:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.Chrome);
                case Agent.eDriverType.SeleniumIE:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.IE);
                case Agent.eDriverType.SeleniumRemoteWebDriver:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.RemoteWebDriver);
                case Agent.eDriverType.SeleniumEdge:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.Edge);
                case Agent.eDriverType.MobileAppiumAndroid:
                    return new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.Android, zAgent.BusinessFlow);
                case Agent.eDriverType.MobileAppiumIOS:
                    return new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOS, zAgent.BusinessFlow);
                case Agent.eDriverType.MobileAppiumAndroidBrowser:
                    return new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser, zAgent.BusinessFlow);                    
                case Agent.eDriverType.MobileAppiumIOSBrowser:
                    return new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser, zAgent.BusinessFlow);
                case Agent.eDriverType.PerfectoMobileAndroid:
                    return new PerfectoDriver(PerfectoDriver.eContextType.NativeAndroid, zAgent.BusinessFlow);                    
                case Agent.eDriverType.PerfectoMobileAndroidWeb:
                    return new PerfectoDriver(PerfectoDriver.eContextType.WebAndroid, zAgent.BusinessFlow);                    
                case Agent.eDriverType.PerfectoMobileIOS:
                    return new PerfectoDriver(PerfectoDriver.eContextType.NativeIOS, zAgent.BusinessFlow);
                case Agent.eDriverType.PerfectoMobileIOSWeb:
                    return new PerfectoDriver(PerfectoDriver.eContextType.WebIOS, zAgent.BusinessFlow);
                case Agent.eDriverType.Appium:
                    return new GenericAppiumDriver(zAgent.BusinessFlow);                           
                default:
                    {
                        throw new Exception("Matching Driver was not found.");
                    }
            }        
        }

        public bool GetLatest(string path, SourceControlBase SourceControl)
        {
            return SourceControlIntegration.GetLatest(path, SourceControl);
        }

        public bool Revert(string path, SourceControlBase SourceControl)
        {
            return SourceControlIntegration.Revert(SourceControl, path);
        }

        public SourceControlBase GetNewSVnRepo()
        {
            return new SVNSourceControlShellWrapper();
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

        public Dictionary<string, string> TakeDesktopScreenShot(bool v)
        {
            throw new NotImplementedException();
        }

        public string GetALMConfig()
        {
            throw new NotImplementedException();
        }

        public DbConnection GetMSAccessConnection()
        {
            throw new NotImplementedException("MS Acess is not supported on Ginger Console");
        }

        public IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow)
        {
            return new WebserviceDriverConsoleReporter();
        }

  
    }
}
