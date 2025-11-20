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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright;
using Amdocs.Ginger.CoreNET.Drivers.WebServicesDriver;
using Amdocs.Ginger.CoreNET.SourceControl;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using Ginger.Repository;
using Ginger.Run.RunSetActions;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Drivers;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SourceControl;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using static GingerCore.Agent;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

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

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, ALMIntegrationEnums.eALMType almType)
        {
            ALMCore aLMCore = null;
            eALMType defaultAlmType = WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(typ => typ.DefaultAlm).AlmType;
            if (aLMCore == null || almType != defaultAlmType)
            {
                aLMCore = (ALMCore)UpdateALMType(almType);
            }
            aLMCore.ConnectALMServer();
            Dictionary<Guid, string> defectsOpeningResults;
            if ((defectsForOpening != null) && (defectsForOpening.Count > 0))
            {
                defectsOpeningResults = aLMCore.CreateNewALMDefects(defectsForOpening, defectsFields);
            }
            else
            {
                return;
            }
            if ((defectsOpeningResults != null) && (defectsOpeningResults.Count > 0))
            {
                foreach (KeyValuePair<Guid, string> defectOpeningResult in defectsOpeningResults)
                {
                    if (defectOpeningResult.Value is not null and not "0")
                    {
                        WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Where(x => x.DefectSuggestionGuid == defectOpeningResult.Key).ToList().ForEach(z => { z.ALMDefectID = defectOpeningResult.Value; z.IsOpenDefectFlagEnabled = false; });
                    }
                }
            }

            //Set back Default Alm
            UpdateALMType(defaultAlmType);
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
            return new GingerCore.ValueExpression(mProjEnvironment, mBusinessFlow);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList)
        {
            return new GingerCore.ValueExpression(mProjEnvironment, mBusinessFlow, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList);
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            return new GingerCore.ValueExpression(obj, attr);
        }

        public void DisplayAsOutlookMail()
        {
            throw new NotImplementedException();
        }


        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            ALMCore aLMCore = GetALMCore();
            aLMCore.ConnectALMServer();
            return aLMCore.ExportBusinessFlowsResultToALM(bfs, ref result, publishToALMConfig, (eALMConnectType)silence);
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string refe, PublishToALMConfig PublishToALMConfig)
        {
            ALMCore aLMCore = GetALMCore();
            aLMCore.ConnectALMServer();
            return aLMCore.ExportBusinessFlowsResultToALM(bfs, ref refe, PublishToALMConfig, eALMConnectType.Silence);
        }

        public Type GetDriverType(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            return zAgent.DriverType switch
            {
                Agent.eDriverType.Selenium => (typeof(SeleniumDriver)),
                eDriverType.Playwright => typeof(PlaywrightDriver),
                Agent.eDriverType.Appium => (typeof(GenericAppiumDriver)),
                Agent.eDriverType.WebServices => (typeof(WebServicesDriver)),
                Agent.eDriverType.UnixShell => typeof(UnixShellDriver),
                _ => throw new Exception("GetDriverType: Unknown Driver type " + zAgent.DriverType),
            };
        }

        public object GetDriverObject(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case Agent.eDriverType.Selenium:
                    return new SeleniumDriver();

                case Agent.eDriverType.Playwright:
                    return new PlaywrightDriver();

                case Agent.eDriverType.Appium:
                    return new GenericAppiumDriver(zAgent.BusinessFlow);
                case Agent.eDriverType.WebServices:
                    return new WebServicesDriver(zAgent.BusinessFlow);
                case Agent.eDriverType.UnixShell:
                    return new UnixShellDriver(zAgent.BusinessFlow);
                default:
                    {
                        throw new Exception("Matching Driver was not found.");
                    }
            }
        }

        public bool GetLatest(string path, SourceControlBase SourceControl, ProgressNotifier progressNotifier = null)
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
                                                                                                                    HTMLReportConfigurations.FirstOrDefault(x => (x.ID == rReport.SelectedHTMLReportTemplateID)),
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

        public void ShowRecoveryItemPage()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool v)
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

        public string GetALMConfig()
        {
            return WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.DefaultAlm).AlmType.ToString();
        }

        public IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow)
        {
            return new WebserviceDriverConsoleReporter();
        }

        private ALMCore GetALMCore()
        {
            string almtype = GetALMConfig();
            Enum.TryParse(almtype, out ALMIntegrationEnums.eALMType AlmType);
            ALMCore almCore = (ALMCore)UpdateALMType(AlmType);
            almCore.GetCurrentAlmConfig();
            ALMCore.SetALMCoreConfigurations(AlmType, almCore);
            return almCore;
        }

        private object UpdateALMType(eALMType almType)
        {
            ALMCore almCore = null;
            ALMConfig CurrentAlmConfigurations = ALMCore.GetCurrentAlmConfig(almType);
            ALMCore.DefaultAlmConfig = CurrentAlmConfigurations;
            //Set ALMRepo
            switch (almType)
            {
                case eALMType.Jira:
                    almCore = new JiraCore();
                    break;
                case eALMType.ZephyrEnterprise:
                    almCore = new ZephyrEntCore();
                    break;
                case eALMType.Octane:
                    almCore = new OctaneCore();
                    break;
                case eALMType.Azure:
                    almCore = new AzureDevOpsCore();
                    break;
                case eALMType.Qtest:
                    almCore = new QtestCore();
                    break;
                case eALMType.QC:
                    if (CurrentAlmConfigurations.UseRest)
                    {
                        almCore = new QCRestAPICore();
                    }
                    else
                    {
                        almCore = new QCCore();
                    }
                    break;
                case eALMType.RQM:
                    almCore = new RQMCore();
                    break;
                default:
                    Reporter.ToLog(eLogLevel.ERROR, $"Invalid ALM Type - {almType}");
                    break;
            }
            return almCore;
        }
        public bool IsSharedRepositoryItem(RepositoryItemBase repositoryItem)
        {
            return SharedRepositoryOperations.IsSharedRepositoryItem(repositoryItem);
        }

        public void DispatcherRun()
        {
            //Not required for GingerConsole
        }

        public bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, eALMConnectType almConnectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            return false;
        }

        public byte[] Capture(Point position, Size size, ImageFormat format)
        {
            throw new NotImplementedException();
        }

        public int ScreenCount()
        {
            throw new NotImplementedException();
        }

        public int PrimaryScreenIndex()
        {
            throw new NotImplementedException();
        }

        public string ScreenName(int screenIndex)
        {
            throw new NotImplementedException();
        }

        public Size ScreenSize(int screenIndex)
        {
            throw new NotImplementedException();
        }

        public Point ScreenPosition(int screenIndex)
        {
            throw new NotImplementedException();
        }

        public Point TaskbarPosition(int screenIndex)
        {
            throw new NotImplementedException();
        }

        public Size TaskbarSize(int screenIndex)
        {
            throw new NotImplementedException();
        }

        public byte[] MergeVertically(IEnumerable<byte[]> images, ImageFormat format)
        {
            throw new NotImplementedException();
        }

        public void Save(string filepath, byte[] image, ImageFormat format)
        {
            throw new NotImplementedException();
        }
    }
}
