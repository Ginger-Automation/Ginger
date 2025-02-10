#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.GeneralLib;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionAutoSaveAndRecover;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Drivers.ASCF;
using GingerCore.Drivers.ConsoleDriverLib;
using GingerCore.Drivers.InternalBrowserLib;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.MainFrame;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Drivers.WindowsLib;
using GingerCore.Environments;
using GingerCore.GeneralLib;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Threading;
using static GingerCore.Agent;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Ginger
{
    public class DotNetFrameworkHelper : ITargetFrameworkHelper
    {
        Outlook.MailItem mOutlookMail;

        public DotNetFrameworkHelper()
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
                case eDriverType.Selenium:
                    return new SeleniumDriver();
                case eDriverType.Playwright:
                    return new PlaywrightDriver();

                case eDriverType.Appium:
                    return new GenericAppiumDriver(zAgent.BusinessFlow);

                case eDriverType.InternalBrowser:
                    return new InternalBrowser(zAgent.BusinessFlow);
                case eDriverType.ASCF:
                    return new ASCFDriver(zAgent.BusinessFlow, agent.Name);
                case eDriverType.DOSConsole:
                    return new DOSConsoleDriver(zAgent.BusinessFlow);
                case eDriverType.UnixShell:
                    return new UnixShellDriver(zAgent.BusinessFlow, zAgent.ProjEnvironment);
                case eDriverType.WebServices:
                    return new WebServicesDriver(zAgent.BusinessFlow);
                case eDriverType.WindowsAutomation:
                    return new WindowsDriver(zAgent.BusinessFlow);
                case eDriverType.PowerBuilder:
                    return new PBDriver(zAgent.BusinessFlow);
                case eDriverType.JavaDriver:
                    return new JavaDriver(zAgent.BusinessFlow);
                case eDriverType.MainFrame3270:
                    return new MainFrameDriver(zAgent.BusinessFlow);

                default:
                    {
                        throw new Exception("Matching Driver was not found.");
                    }
            }
        }

        public Type GetDriverType(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            return zAgent.DriverType switch
            {
                eDriverType.InternalBrowser => (typeof(InternalBrowser)),
                eDriverType.Selenium => (typeof(SeleniumDriver)),
                eDriverType.Playwright => typeof(PlaywrightDriver),
                eDriverType.ASCF => (typeof(ASCFDriver)),
                eDriverType.DOSConsole => (typeof(DOSConsoleDriver)),
                eDriverType.UnixShell => (typeof(UnixShellDriver)),
                eDriverType.PowerBuilder => (typeof(PBDriver)),
                eDriverType.WindowsAutomation => (typeof(WindowsDriver)),
                eDriverType.WebServices => (typeof(WebServicesDriver)),
                eDriverType.JavaDriver => (typeof(JavaDriver)),
                eDriverType.MainFrame3270 => (typeof(MainFrameDriver)),
                //case Agent.eDriverType.AndroidADB:
                //    return (typeof(AndroidADBDriver));                    
                eDriverType.Appium => (typeof(GenericAppiumDriver)),
                _ => throw new Exception("GetDriverType: Unknown Driver type " + zAgent.DriverType),
            };
        }

        AutoRunWindow mAutoRunWindow;
        private AutoResetEvent mAutoRunWindowEven;
        public void ShowAutoRunWindow()
        {
            mAutoRunWindowEven = new AutoResetEvent(false);
            // Run the AutoRunWindow on its own thread 
            Thread mGingerThread = new Thread(() =>
            {
                mAutoRunWindow = new AutoRunWindow();
                mAutoRunWindow.Show();

                GingerCore.General.DoEvents();
                mAutoRunWindowEven.Set();
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
                Dispatcher.Run();
            });

            // Makes the thread support message pumping 
            // Configure the thread            
            mGingerThread.SetApartmentState(ApartmentState.STA);
            mGingerThread.IsBackground = true;
            mGingerThread.Start();

            mAutoRunWindowEven.WaitOne(TimeSpan.FromMinutes(2));
            //Stopwatch stopwatch = Stopwatch.StartNew();
            //while (mAutoRunWindow == null && stopwatch.ElapsedMilliseconds < 5000) // max 5 seconds
            //{
            //    Thread.Sleep(100);
            //}
            if (!WorkSpace.Instance.RunningInExecutionMode)
            {
                mAutoRunWindow.Dispatcher.Invoke(() =>
                {
                    Thread.Sleep(100);  // run something on main window so we know it is active and pumping messages
                });
            }
        }

        public void WaitForAutoRunWindowClose()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            while (mAutoRunWindow.IsVisible && stopwatch.ElapsedMilliseconds < 30000) // max 30 seconds to wait
            {
                Thread.Sleep(500);
            }

        }

        bool ITargetFrameworkHelper.Send_Outlook(bool actualSend, string MailTo, string Event, string Subject, string Body, string MailCC, List<string> Attachments, List<KeyValuePair<string, string>> EmbededAttachment)
        {
            try
            {
                Outlook.Application objOutLook = null;
                // Check whether there is an Outlook process running.
                if (Process.GetProcessesByName("OUTLOOK").Any())
                {
                    // If so, use the GetActiveObject method to obtain the process and cast it to an ApplicatioInstall-Package Microsoft.Office.Interop.Exceln object.

                    objOutLook = MarshalExtension.GetActiveObject("Outlook.Application") as Outlook.Application;
                }
                else
                {
                    // If not, create a new instance of Outlook and log on to the default profile.
                    objOutLook = new Outlook.Application();
                    Outlook.NameSpace nameSpace = objOutLook.GetNamespace("MAPI");
                    nameSpace.Logon("", "", System.Reflection.Missing.Value, System.Reflection.Missing.Value);
                    nameSpace = null;
                }

                mOutlookMail = objOutLook.CreateItem(Outlook.OlItemType.olMailItem) as Outlook.MailItem;

                mOutlookMail.HTMLBody = Body;
                mOutlookMail.Subject = Subject;

                Outlook.AddressEntry currentUser = objOutLook.Session.CurrentUser.AddressEntry;

                if (currentUser.Type == "EX")
                {
                    Outlook.ExchangeUser manager = currentUser.GetExchangeUser();

                    // Add recipient using display name, alias, or smtp address
                    string emails = MailTo;
                    Array arrEmails = emails.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string email in arrEmails)
                    {
                        mOutlookMail.Recipients.Add(email);
                    }

                    //Add CC
                    if (!String.IsNullOrEmpty(MailCC))
                    {
                        Array arrCCEmails = MailCC.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string MailCC1 in arrCCEmails)
                        {
                            mOutlookMail.Recipients.Add(MailCC1);
                        }
                    }

                    mOutlookMail.Recipients.ResolveAll();

                    mOutlookMail.CC = MailCC;
                    mOutlookMail.To = MailTo;

                    //Add Attachment
                    foreach (string AttachmentFileName in Attachments)
                    {
                        if (String.IsNullOrEmpty(AttachmentFileName) == false)
                        {
                            mOutlookMail.Attachments.Add(AttachmentFileName, Type.Missing, Type.Missing, Type.Missing);
                        }
                    }

                    //attachment which is embeded into the email body(images).                       
                    foreach (KeyValuePair<string, string> AttachmentFileName in EmbededAttachment)
                    {
                        if (String.IsNullOrEmpty(AttachmentFileName.Key) == false)
                        {
                            if (File.Exists(AttachmentFileName.Key))
                            {
                                Outlook.Attachment attachment = mOutlookMail.Attachments.Add(AttachmentFileName.Key, Outlook.OlAttachmentType.olEmbeddeditem, null, "");
                                attachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", AttachmentFileName.Value);
                            }
                        }
                    }
                    if (actualSend)
                    {
                        //Send Mail
                        mOutlookMail.Send();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Mailbox Unavailable"))
                {
                    Event = "Failed: Please provide correct sender email address";
                }
                else if (ex.StackTrace.Contains("System.Runtime.InteropServices.Marshal.GetActiveObject"))
                {
                    Event = "Please make sure ginger/outlook opened in same security context (Run as administrator or normal user)";
                }
                else if (ex.StackTrace.Contains("System.Security.Authentication.AuthenticationException") || ex.StackTrace.Contains("System.Net.Sockets.SocketException"))
                {
                    Event = "Please check SSL configuration";
                }
                else
                {
                    Event = "Failed: " + ex.Message;
                }
                return false;
            }
        }

        public void DisplayAsOutlookMail()
        {
            mOutlookMail.Display();
        }

        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempFolder)
        {
            Chart Chart1 = new Chart();
            List<string> x = ["Passed", "Failed", "Stopped", "Other"];
            List<int> yList = (from ylist in y select ylist.Key).ToList();
            int xAxis = 0;
            string total = "";
            Chart1.BackColor = Color.AliceBlue;
            Chart1.BackColor = Color.White;
            Chart1.Series.Add(new Series());
            ChartArea a1 = new ChartArea
            {
                Name = "Area"
            };
            Chart1.ChartAreas.Add(a1);
            a1.InnerPlotPosition = new ElementPosition(12, 10, 78, 78);
            Chart1.Series[0].ChartArea = "Area";
            Chart1.Series[0].Points.DataBindXY(x, yList);
            Chart1.Series["Series1"].Label = "#VALX (#VALY)";
            Chart1.Series[0].ChartType = SeriesChartType.Doughnut;
            Chart1.Series[0]["DoughnutRadius"] = "20";
            Chart1.Series[0]["DoughnutInnerRadius"] = "99";
            Chart1.Series[0]["PieLabelStyle"] = "Outside";
            Chart1.Series[0].BorderWidth = 1;
            Chart1.Series[0].BorderDashStyle = ChartDashStyle.Dot;
            Chart1.Series[0].BorderColor = Color.FromArgb(200, 26, 59, 105);
            foreach (KeyValuePair<int, int> l in y)
            {
                if (l.Key == 0)
                {
                    Chart1.Series[0].Points[l.Value].BorderColor = Color.White;
                    Chart1.Series["Series1"].Points[l.Value].AxisLabel = "";
                    Chart1.Series["Series1"].Points[l.Value].Label = "";
                }
            }
            Chart1.Series[0].Points[0].Color = Chart1.Series[0].Points[0].LabelForeColor = GingerCore.General.makeColor("#008000");
            Chart1.Series[0].Points[1].Color = Chart1.Series[0].Points[1].LabelForeColor = GingerCore.General.makeColor("#FF0000");
            Chart1.Series[0].Points[2].Color = Chart1.Series[0].Points[2].LabelForeColor = GingerCore.General.makeColor("#ff57ab");
            Chart1.Series[0].Points[3].Color = Chart1.Series[0].Points[3].LabelForeColor = GingerCore.General.makeColor("#1B3651");
            Chart1.Series[0].Font = new Font("sans-serif", 9, FontStyle.Bold);
            Chart1.Height = 180;
            Chart1.Width = 310;
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#e3dfdb"));
            System.Drawing.SolidBrush myBrush1 = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#1B3651"));
            Chart1.Titles.Add("NewTitle");
            Chart1.Titles["Title1"].Text = Title;
            Chart1.Titles["Title1"].Font = new Font("sans-serif", 11, FontStyle.Bold);
            Chart1.Titles["Title1"].ForeColor = GingerCore.General.makeColor("#1B3651");
            using (MemoryStream m = new MemoryStream())
            {
                Chart1.SaveImage(m, ChartImageFormat.Png);
                using (Bitmap bitMapImage = new(m))
                {
                    using Graphics graphicImage = Graphics.FromImage(bitMapImage);
                    graphicImage.SmoothingMode = SmoothingMode.AntiAlias;
                    graphicImage.FillEllipse(myBrush, 132, 75, 50, 50);
                    total = yList.Sum().ToString();
                    if (total.Length == 1)
                    {
                        xAxis = 151;
                    }
                    else if (total.Length == 2)
                    {
                        xAxis = 145;
                    }
                    else if (total.Length == 3)
                    {
                        xAxis = 142;
                    }
                    else if (total.Length == 4)
                    {
                        xAxis = 140;
                    }
                    graphicImage.DrawString(yList.Sum().ToString(), new Font("sans-serif", 9, FontStyle.Bold), myBrush1, new System.Drawing.Point(xAxis, 91));
                    bitMapImage.Save(Path.Combine(tempFolder, chartName), System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
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
            HTMLReportTemplatePage.EnchancingLoadedFieldsWithDataAndValidating(currentTemplate);
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool captureAllScreens = false)
        {
            return GingerCore.General.TakeDesktopScreenShot(true);
        }

        public Bitmap GetBrowserHeaderScreenshot(Point windowPosition, Size windowSize, Size viewportSize, double devicePixelRatio)
        {
            return GingerCore.General.GetBrowserHeaderScreenshot(windowPosition, windowSize, viewportSize, devicePixelRatio);
        }

        public Bitmap GetTaskbarScreenshot()
        {
            return GingerCore.General.GetTaskbarScreenshot();
        }

        public string MergeVerticallyAndSaveBitmaps(params Bitmap[] bitmaps)
        {
            return GingerCore.General.MergeVerticallyAndSaveBitmaps(bitmaps);
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            return ALMIntegration.Instance.ExportBusinessFlowsResultToALM(bfs, ref result, publishToALMConfig, eALMConnectType.Silence);
        }

        public ITextBoxFormatter CreateTextBoxFormatter(object Textblock)
        {
            return new TextBoxFormatter(Textblock);
        }

        public string GetALMConfig()
        {
            return WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.DefaultAlm).AlmType.ToString();
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType)
        {
            //update alm type to open defect
            ALMIntegration.Instance.UpdateALMType(almType, true);

            Dictionary<Guid, string> defectsOpeningResults;
            if ((defectsForOpening != null) && (defectsForOpening.Count > 0))
            {
                defectsOpeningResults = ALMIntegration.Instance.CreateNewALMDefects(defectsForOpening, defectsFields);
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
            ALMIntegration.Instance.UpdateALMType(ALMCore.GetDefaultAlmConfig().AlmType);
        }

        public void HTMLReportAttachment(string extraInformationCalculated, ref string emailReadyHtml, ref string reportsResultFolder, string runSetFolder, object Report, object conf)
        {
            EmailHtmlReportAttachment rReport = (EmailHtmlReportAttachment)Report;
            HTMLReportsConfiguration currentConf = (HTMLReportsConfiguration)conf;
            if (!HTMLReportAttachmentConfigurationPage.HasWritePermission(extraInformationCalculated))
            {
                emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->",
                "<b>Full report attachment failed, </b>" +
                "Error: User '" + WindowsIdentity.GetCurrent().Name.ToString() + "' have no write permission on provided alternative folder - " + extraInformationCalculated + ". Attachment in it not saved.");
            }
            else
            {
                emailReadyHtml = emailReadyHtml.Replace("<!--WARNING-->", "");
                ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                reportsResultFolder = Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                        false,
                                                                                                                        HTMLReportConfigurations.FirstOrDefault(x => (x.ID == rReport.SelectedHTMLReportTemplateID)),
                                                                                                                        extraInformationCalculated + "\\" + Path.GetFileName(runSetFolder), false, currentConf.HTMLReportConfigurationMaximalFolderSize);
            }
        }

        public object CreateNewReportTemplate()
        {
            ReportTemplate NewReportTemplate = new ReportTemplate() { Name = "New Report Template", Status = ReportTemplate.eReportStatus.Development };

            ReportTemplateSelector RTS = new ReportTemplateSelector();
            RTS.ShowAsWindow();

            if (RTS.SelectedReportTemplate != null)
            {

                NewReportTemplate.Xaml = RTS.SelectedReportTemplate.Xaml;

                //Make it Generic or Const string for names used for File
                string NewReportName = string.Empty;
                if (GingerCore.General.GetInputWithValidation("Add Report Template", "Report Template Name:", ref NewReportName, Path.GetInvalidFileNameChars(), false, NewReportTemplate))
                {
                    NewReportTemplate.Name = NewReportName;
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);
                }
                return NewReportTemplate;
            }
            return null;
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig PublishToALMConfig)
        {
            return ALMIntegration.Instance.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig, eALMConnectType.Auto, false);
        }





        public void ShowRecoveryItemPage()
        {
            using (RecoverPage recoverPage = new RecoverPage())
            {
                recoverPage.ShowAsWindow();
            }

        }

        public bool GetLatest(string path, SourceControlBase SourceControl, ProgressNotifier progressNotifier = null)
        {
            return SourceControlUI.GetLatest(path, SourceControl, progressNotifier);
        }

        public bool Revert(string path, SourceControlBase SourceControl)
        {
            return SourceControlUI.Revert(path, SourceControl);
        }

        public SourceControlBase GetNewSVnRepo()
        {
            return new SVNSourceControl();
        }

        public IWebserviceDriverWindow GetWebserviceDriverWindow(BusinessFlow businessFlow)
        {
            return new WebServicesDriverWindow(businessFlow);
        }

        public DbConnection GetOracleConnection(string ConnectionString)
        {
            return new OracleConnection(ConnectionString);
        }
        public bool IsSharedRepositoryItem(RepositoryItemBase repositoryItem)
        {
            return SharedRepositoryOperations.IsSharedRepositoryItem(repositoryItem);
        }

        public void DispatcherRun()
        {
            Dispatcher.Run();
        }

        public bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, eALMConnectType almConnectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            return ALMIntegration.Instance.ExportVirtualBusinessFlowToALM(businessFlow, publishToALMConfig, performSaveAfterExport, almConnectStyle, testPlanUploadPath, testLabUploadPath);
        }

        [SupportedOSPlatform("windows")]
        public string ScreenName(int screenIndex)
        {
            return GingerCore.General.ScreenName(screenIndex);
        }

        [SupportedOSPlatform("windows")]
        public int ScreenCount()
        {
            return GingerCore.General.ScreenCount();
        }

        [SupportedOSPlatform("windows")]
        public int PrimaryScreenIndex()
        {
            return GingerCore.General.PrimaryScreenIndex();
        }

        [SupportedOSPlatform("windows")]
        public void Save(string filepath, byte[] image, ImageFormat format)
        {
            GingerCore.General.Save(filepath, image, format);
        }

        [SupportedOSPlatform("windows")]
        public Size ScreenSize(int screenIndex)
        {
            return GingerCore.General.ScreenSize(screenIndex);
        }

        [SupportedOSPlatform("windows")]
        public Point ScreenPosition(int screenIndex)
        {
            return GingerCore.General.ScreenPosition(screenIndex);
        }

        [SupportedOSPlatform("windows")]
        public Point TaskbarPosition(int screenIndex)
        {
            return GingerCore.General.TaskbarPosition(screenIndex);
        }

        [SupportedOSPlatform("windows")]
        public Size TaskbarSize(int screenIndex)
        {
            return GingerCore.General.TaskbarSize(screenIndex);
        }

        [SupportedOSPlatform("windows")]
        public byte[] Capture(Point position, Size size, ImageFormat format)
        {
            return GingerCore.General.Capture(position, size, format);
        }

        [SupportedOSPlatform("windows")]
        public byte[] MergeVertically(IEnumerable<byte[]> images, ImageFormat format)
        {
            return GingerCore.General.MergeVertically(images, format);
        }
    }
}


