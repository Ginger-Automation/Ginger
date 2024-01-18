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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Repository;
using GingerCore;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Environments;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using static GingerCore.Agent;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger
{
    public class DotNetFramework : ITargetFrameworkHelper
    {
        public DotNetFramework()
        {
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow, (ObservableList<DataSourceBase>)DSList);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true)
        {
            return new ValueExpression(Env, BF, DSList, bUpdate, UpdateValue, bDone);
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            return new ValueExpression(obj, attr);
        }

        public object GetDriverObject(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case eDriverType.SeleniumFireFox:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.FireFox);
                case eDriverType.SeleniumChrome:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.Chrome);
                case eDriverType.SeleniumIE:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.IE);
                case eDriverType.SeleniumRemoteWebDriver:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.RemoteWebDriver);
                case eDriverType.SeleniumEdge:
                    return new SeleniumDriver(SeleniumDriver.eBrowserType.Edge);

                case eDriverType.Appium:
                    return new GenericAppiumDriver(zAgent.BusinessFlow);
                default:
                    {
                        throw new Exception("Matching Driver was not found.");
                    }

            }
        }

        public Type GetDriverType(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case eDriverType.SeleniumFireFox:
                case eDriverType.SeleniumChrome:
                case eDriverType.SeleniumIE:
                case eDriverType.SeleniumRemoteWebDriver:
                case eDriverType.SeleniumEdge:
                    return (typeof(SeleniumDriver));

                //case Agent.eDriverType.AndroidADB:
                //    return (typeof(AndroidADBDriver));                    

                case eDriverType.Appium:
                    return (typeof(GenericAppiumDriver));

                default:
                    throw new Exception("GetDriverType: Unknown Driver type " + zAgent.DriverType);

            }
        }

        public void ShowAutoRunWindow()
        {
        }

        public void WaitForAutoRunWindowClose()
        {

        }

        bool ITargetFrameworkHelper.Send_Outlook(bool actualSend, string MailTo, string Event, string Subject, string Body, string MailCC, List<string> Attachments, List<KeyValuePair<string, string>> EmbededAttachment)
        {
            return true;
        }

        public void DisplayAsOutlookMail()
        {
        }

        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempFolder)
        {

        }
        public void CreateCustomerLogo(object a, string tempFolder)
        {
            HTMLReportConfiguration currentTemplate = (HTMLReportConfiguration)a;
            System.Drawing.Image CustomerLogo = General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
            CustomerLogo.Save(Path.Combine(tempFolder, "CustomerLogo.png"));
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool captureAllScreens = false)
        {
            return new();
        }

        public Bitmap GetBrowserHeaderScreenshot(Point windowPosition, Size windowSize, Size viewportSize, double devicePixelRatio)
        {
            return new("nofile");
        }

        public Bitmap GetTaskbarScreenshot()
        {
            return new("NoFile");
        }

        public string MergeVerticallyAndSaveBitmaps(params Bitmap[] bitmaps)
        {
            return string.Empty;
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            return true;
        }

        public ITextBoxFormatter CreateTextBoxFormatter(object Textblock)
        {
            return null;
        }

        public string GetALMConfig()
        {
            return WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.DefaultAlm).AlmType.ToString();
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType)
        {
            //update alm type to open defect
        }

        public void HTMLReportAttachment(string extraInformationCalculated, ref string emailReadyHtml, ref string reportsResultFolder, string runSetFolder, object Report, object conf)
        {
        }

        public object CreateNewReportTemplate()
        {
            return null;
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig PublishToALMConfig)
        {
            return true;
        }
        public void ShowRecoveryItemPage()
        {

        }

        public bool GetLatest(string path, SourceControlBase SourceControl)
        {
            return true;
        }

        public bool Revert(string path, SourceControlBase SourceControl)
        {
            return true;
        }

        public SourceControlBase GetNewSVnRepo()
        {
            return null;
        }

        public IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow)
        {
            return null;
        }

        public DbConnection GetOracleConnection(string ConnectionString)
        {
            return null;
        }
        public bool IsSharedRepositoryItem(RepositoryItemBase repositoryItem)
        {
            return SharedRepositoryOperations.IsSharedRepositoryItem(repositoryItem);
        }

        public void DispatcherRun()
        {

        }

        public bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, eALMConnectType almConnectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            return true;
        }
    }
}

