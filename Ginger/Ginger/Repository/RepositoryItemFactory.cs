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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.ALM;
using Ginger.AnalyzerLib;
using Ginger.GeneralLib;
using Ginger.Reports;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.ASCF;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.ConsoleDriverLib;
using GingerCore.Drivers.InternalBrowserLib;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.MainFrame;
using GingerCore.Drivers.Mobile.Perfecto;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Drivers.WindowsLib;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.DataVisualization.Charting;
using System.Windows.Threading;
using static GingerCore.Agent;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Ginger.Repository
{
    public class RepositoryItemFactory : IRepositoryItemFactory
    {
        Outlook.MailItem mOutlookMail;
        
        public RepositoryItemFactory()
        {
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment mProjEnvironment, BusinessFlow mBusinessFlow, object DSList)
        {
            return new ValueExpression(mProjEnvironment, mBusinessFlow, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList);
        }

        public IValueExpression CreateValueExpression(ProjEnvironment Env, BusinessFlow BF, ObservableList<DataSourceBase> DSList = null, bool bUpdate = false, string UpdateValue = "", bool bDone = true, ObservableList<VariableBase> solutionVariables = null)
        {
            return new ValueExpression(Env, BF, (ObservableList<GingerCore.DataSource.DataSourceBase>)DSList, bUpdate, UpdateValue, bDone, solutionVariables);            
        }

        public IValueExpression CreateValueExpression(object obj, string attr)
        {
            return new ValueExpression(obj, attr);
        }


 
        public ObservableList<IDatabase> GetDatabaseList()
        {
            return new ObservableList<IDatabase>();
        }

        public ObservableList<DataSourceBase> GetDatasourceList()
        {
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
        }


        public ObservableList<IAgent> GetAllIAgents()
        {
            return new ObservableList<IAgent>( WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().ListItems.ConvertAll(x => (IAgent)x));
        }
  
        public void StartAgentDriver(IAgent agent)
        {
            Agent zAgent = (Agent)agent;
            BusinessFlow BusinessFlow = zAgent.BusinessFlow;
            ProjEnvironment ProjEnvironment = zAgent.ProjEnvironment;
            bool Remote = zAgent.Remote;
            
            DriverBase Driver = null; 
            zAgent.mIsStarting = true;
            zAgent.OnPropertyChanged(Fields.Status);
            try
            {
                try
                {
                    if (Remote)
                    {
                        throw new Exception("Remote is Obsolete, use GingerGrid");
                        //We pass the agent info
                    }
                    else
                    {
                        switch (zAgent.DriverType)
                        {
                            case eDriverType.InternalBrowser:
                                Driver = new InternalBrowser(BusinessFlow);
                                break;
                            case eDriverType.SeleniumFireFox:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.FireFox);
                                break;
                            case eDriverType.SeleniumChrome:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
                                break;
                            case eDriverType.SeleniumIE:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.IE);
                                break;
                            case eDriverType.SeleniumRemoteWebDriver:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.RemoteWebDriver);
                                // set capabilities
                                if (zAgent.DriverConfiguration == null) zAgent.DriverConfiguration = new ObservableList<DriverConfigParam>();
                                ((SeleniumDriver)Driver).RemoteGridHub = zAgent.GetParamValue(SeleniumDriver.RemoteGridHubParam);
                                ((SeleniumDriver)Driver).RemoteBrowserName = zAgent.GetParamValue(SeleniumDriver.RemoteBrowserNameParam);
                                ((SeleniumDriver)Driver).RemotePlatform = zAgent.GetParamValue(SeleniumDriver.RemotePlatformParam);
                                ((SeleniumDriver)Driver).RemoteVersion = zAgent.GetParamValue(SeleniumDriver.RemoteVersionParam);
                                break;
                            case eDriverType.SeleniumEdge:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Edge);
                                break;
                            case eDriverType.SeleniumPhantomJS:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.PhantomJS);
                                break;
                            case eDriverType.ASCF:                                
                                Driver = new ASCFDriver(BusinessFlow, zAgent.Name);
                                break;
                            case eDriverType.DOSConsole:                                
                                Driver = new DOSConsoleDriver(BusinessFlow);
                                break;
                            case eDriverType.UnixShell:                                
                                 Driver = new UnixShellDriver(BusinessFlow, ProjEnvironment);
                                ((UnixShellDriver)Driver).SetScriptsFolder(System.IO.Path.Combine(zAgent.SolutionFolder, @"Documents\sh\"));
                                break;
                            case eDriverType.MobileAppiumAndroid:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.Android, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumIOS:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOS, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumAndroidBrowser:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumIOSBrowser:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileAndroid:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.NativeAndroid, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileAndroidWeb:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.WebAndroid, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileIOS:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.NativeIOS, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileIOSWeb:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.WebIOS, BusinessFlow);
                                break;
                            case eDriverType.WebServices:
                                WebServicesDriver WebServicesDriver = new WebServicesDriver(BusinessFlow);
                                Driver = WebServicesDriver;
                                break;
                            case eDriverType.WindowsAutomation:
                                Driver = new WindowsDriver(BusinessFlow);
                                break;
                            case eDriverType.FlaUIWindow:
                                Driver = new WindowsDriver(BusinessFlow, UIAutomationDriverBase.eUIALibraryType.FlaUI);
                                break;
                            case eDriverType.PowerBuilder:
                                Driver = new PBDriver(BusinessFlow);
                                break;
                            case eDriverType.FlaUIPB:
                                Driver = new PBDriver(BusinessFlow, UIAutomationDriverBase.eUIALibraryType.FlaUI);
                                break;
                            case eDriverType.JavaDriver:
                                Driver = new JavaDriver(BusinessFlow);
                                break;
                            case eDriverType.MainFrame3270:
                                Driver = new MainFrameDriver(BusinessFlow);
                                break;
                            case eDriverType.AndroidADB:
                                string DeviceConfigFolder = zAgent.GetOrCreateParam("DeviceConfigFolder").Value;
                                if (!string.IsNullOrEmpty(DeviceConfigFolder))
                                {
                                    Driver = new AndroidADBDriver(BusinessFlow, System.IO.Path.Combine(zAgent.SolutionFolder, @"Documents\Devices", DeviceConfigFolder, @"\"));
                                }
                                else
                                {
                                    //TODO: Load create sample folder/device, or start the wizard
                                    throw new Exception("Please set device config folder");
                                }
                                break;
                                //TODO: default mess
                        }
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, zAgent.Name, e.Message);
                }

                if (zAgent.AgentType == eAgentType.Service)
                {
                    zAgent.StartPluginService();
                    zAgent.OnPropertyChanged(Fields.Status);
                }
                else
                {
                    zAgent.Driver = Driver;
                    Driver.BusinessFlow = zAgent.BusinessFlow;
                    zAgent.SetDriverConfiguration();

                    //if STA we need to start it on seperate thread, so UI/Window can be refreshed: Like IB, Mobile, Unix
                    if (Driver.IsSTAThread())
                    {
                        zAgent.CTS = new CancellationTokenSource();

                        zAgent.MSTATask = new Task(() => { Driver.StartDriver(); }, zAgent.CTS.Token, TaskCreationOptions.LongRunning);
                        zAgent.MSTATask.Start();
                    }
                    else
                    {
                        Driver.StartDriver();
                    }
                }
            }
            finally
            {
                if (zAgent.AgentType == eAgentType.Service)
                {
                    zAgent.mIsStarting = false;
                }
                else
                {
                    // Give the driver time to start            
                    Thread.Sleep(500);
                    zAgent.mIsStarting = false;
                    Driver.IsDriverRunning = true;
                    zAgent.OnPropertyChanged(Fields.Status);
                    Driver.driverMessageEventHandler += zAgent.driverMessageEventHandler;
                    zAgent.OnPropertyChanged(Fields.IsWindowExplorerSupportReady);
                }
            }


            //return Driver;
        }

        public Type GetDriverType(IAgent agent)
        {
            Agent zAgent = (Agent)agent;

            switch (zAgent.DriverType)
            {
                case Agent.eDriverType.InternalBrowser:
                    return(typeof(InternalBrowser));                    
                case Agent.eDriverType.SeleniumFireFox:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.SeleniumChrome:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.SeleniumIE:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.SeleniumRemoteWebDriver:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.SeleniumEdge:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.SeleniumPhantomJS:
                    return (typeof(SeleniumDriver));                    
                case Agent.eDriverType.ASCF:
                    return (typeof(ASCFDriver));                    
                case Agent.eDriverType.DOSConsole:
                    return (typeof(DOSConsoleDriver));                    
                case Agent.eDriverType.UnixShell:
                    return (typeof(UnixShellDriver));                    
                case Agent.eDriverType.MobileAppiumAndroid:
                    return (typeof(SeleniumAppiumDriver));                    
                case Agent.eDriverType.MobileAppiumIOS:
                    return (typeof(SeleniumAppiumDriver));                    
                case Agent.eDriverType.MobileAppiumAndroidBrowser:
                case Agent.eDriverType.MobileAppiumIOSBrowser:
                    return (typeof(SeleniumAppiumDriver));                    
                case Agent.eDriverType.PowerBuilder:
                    return (typeof(PBDriver));                    
                case Agent.eDriverType.WindowsAutomation:
                    return (typeof(WindowsDriver));                    
                case Agent.eDriverType.WebServices:
                    return (typeof(WebServicesDriver));                    
                case Agent.eDriverType.JavaDriver:
                    return (typeof(JavaDriver));                    
                case Agent.eDriverType.MainFrame3270:
                    return (typeof(MainFrameDriver));                    
                case Agent.eDriverType.AndroidADB:
                    return (typeof(AndroidADBDriver));                    
                case Agent.eDriverType.PerfectoMobileAndroid:
                case Agent.eDriverType.PerfectoMobileAndroidWeb:
                case Agent.eDriverType.PerfectoMobileIOS:
                case Agent.eDriverType.PerfectoMobileIOSWeb:
                    return (typeof(PerfectoDriver));
                    
                default:
                    throw new Exception("GetDriverType: Unknow Driver type " + zAgent.DriverType);
                    
            }
        }

        public ObservableList<VariableBase> GetVariaables()
        {
            return  WorkSpace.UserProfile.Solution.Variables;
        }

        public Type GetPage(string a)
        {
            throw new NotImplementedException();
        }

        public async Task<int> AnalyzeRunset(object a, bool runInSilentMode)
        {
            try
            {
                AnalyzerPage analyzerPage = new AnalyzerPage();
                Dispatcher.CurrentDispatcher.Invoke(() => 
                {
                    RunSetConfig runSetConfig = (RunSetConfig)a;
                    analyzerPage.Init( WorkSpace.UserProfile.mSolution, runSetConfig);
                });
                await analyzerPage.AnalyzeWithoutUI();


                if (analyzerPage.TotalHighAndCriticalIssues > 0)
                {
                    if (!runInSilentMode)
                    {
                        Reporter.ToUser(eUserMsgKey.AnalyzerFoundIssues);
                        analyzerPage.ShowAsWindow();
                    }
                    return 1;
                }
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
            return 0;
        }

        public void RunRunSetFromCommandLine()
        {
            App.MainWindow.Hide();
            App.AppSplashWindow.Close();
            AutoRunWindow RP = new AutoRunWindow();
            RP.Show();
        }

        bool IRepositoryItemFactory.Send_Outlook(bool actualSend, string MailTo, string Event, string Subject, string Body, string MailCC, List<string> Attachments, List<KeyValuePair<string, string>> EmbededAttachment)
        {
            try
            {
                Outlook.Application objOutLook = null;
                if (string.IsNullOrEmpty(MailTo))
                {
                    Event = "Failed: Please provide TO email address.";
                    return false;
                }
                if (string.IsNullOrEmpty(Subject))
                {
                    Event = "Failed: Please provide email subject.";
                    return false;
                }
                // Check whether there is an Outlook process running.
                if (System.Diagnostics.Process.GetProcessesByName("OUTLOOK").Count() > 0)
                {
                    // If so, use the GetActiveObject method to obtain the process and cast it to an ApplicatioInstall-Package Microsoft.Office.Interop.Exceln object.

                    objOutLook = Marshal.GetActiveObject("Outlook.Application") as Outlook.Application;
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
                            mOutlookMail.Attachments.Add(AttachmentFileName, Type.Missing, Type.Missing, Type.Missing);
                    }

                    //attachment which is embeded into the email body(images).                       
                    foreach (KeyValuePair<string, string> AttachmentFileName in EmbededAttachment)
                    {
                        if (String.IsNullOrEmpty(AttachmentFileName.Key) == false)
                        {
                            if (System.IO.File.Exists(AttachmentFileName.Key))
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
                if (ex.Message.Contains("Mailbox Unavailabel"))
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
            List<string> x = new List<string>() { "Passed", "Failed", "Stopped", "Other" };
            List<int> yList = (from ylist in y select ylist.Key).ToList();
            int xAxis = 0;
            string total = "";
            Chart1.BackColor = System.Drawing.Color.AliceBlue;
            Chart1.BackColor = System.Drawing.Color.White;
            Chart1.Series.Add(new Series());
            ChartArea a1 = new ChartArea();
            a1.Name = "Area";
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
            Chart1.Series[0].BorderColor = System.Drawing.Color.FromArgb(200, 26, 59, 105);
            foreach (KeyValuePair<int, int> l in y)
            {
                if (l.Key == 0)
                {
                    Chart1.Series[0].Points[l.Value].BorderColor = System.Drawing.Color.White;
                    Chart1.Series["Series1"].Points[l.Value].AxisLabel = "";
                    Chart1.Series["Series1"].Points[l.Value].Label = "";
                }
            }
            Chart1.Series[0].Points[0].Color = Chart1.Series[0].Points[0].LabelForeColor = GingerCore.General.makeColor("#008000");
            Chart1.Series[0].Points[1].Color = Chart1.Series[0].Points[1].LabelForeColor = GingerCore.General.makeColor("#FF0000");
            Chart1.Series[0].Points[2].Color = Chart1.Series[0].Points[2].LabelForeColor = GingerCore.General.makeColor("#ff57ab");
            Chart1.Series[0].Points[3].Color = Chart1.Series[0].Points[3].LabelForeColor = GingerCore.General.makeColor("#1B3651");
            Chart1.Series[0].Font = new Font("sans-serif", 9, System.Drawing.FontStyle.Bold);
            Chart1.Height = 180;
            Chart1.Width = 310;
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#e3dfdb"));
            System.Drawing.SolidBrush myBrush1 = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#1B3651"));
            Chart1.Titles.Add("NewTitle");
            Chart1.Titles["Title1"].Text = Title;
            Chart1.Titles["Title1"].Font = new Font("sans-serif", 11, System.Drawing.FontStyle.Bold);
            Chart1.Titles["Title1"].ForeColor = GingerCore.General.makeColor("#1B3651");
            MemoryStream m = new MemoryStream();
            Chart1.SaveImage(m, ChartImageFormat.Png);
            Bitmap bitMapImage = new System.Drawing.Bitmap(m);
            Graphics graphicImage = Graphics.FromImage(bitMapImage);
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
            graphicImage.DrawString(yList.Sum().ToString(), new Font("sans-serif", 9, System.Drawing.FontStyle.Bold), myBrush1, new System.Drawing.Point(xAxis, 91));
            m = new MemoryStream();
            bitMapImage.Save(tempFolder + "\\" + chartName, ImageFormat.Jpeg);
            graphicImage.Dispose();
            bitMapImage.Dispose();
        }

        public void CreateCustomerLogo(object a, string tempFolder)
        {
            HTMLReportConfiguration currentTemplate = (HTMLReportConfiguration)a;
            System.Drawing.Image CustomerLogo = Ginger.General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
            CustomerLogo.Save(tempFolder + "/CustomerLogo.png");
           
            Ginger.Reports.HTMLReportTemplatePage.EnchancingLoadedFieldsWithDataAndValidating(currentTemplate);
        }

        public Dictionary<string, string> TakeDesktopScreenShot(bool captureAllScreens = false)
        {            
            return GingerCore.General.TakeDesktopScreenShot(true);
        }

        public void ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, ref string result, PublishToALMConfig publishToALMConfig, object silence)
        {
            ALM.ALMIntegration.Instance.ExportBusinessFlowsResultToALM(bfs, ref result, publishToALMConfig, ALM.ALMIntegration.eALMConnectType.Silence);
        }

        public ITextBoxFormatter CreateTextBoxFormatter(object Textblock)
        {
            return new TextBoxFormatter(Textblock);
        }

        public string GenerateTemplate(string templatename, object o)
        {
            ReportInfo reportInfo = (ReportInfo)o;
            return ReportTemplate.GenerateReport(templatename, reportInfo);
        }

        public bool ProcessCommandLineArgs(string[] lines)
        {
            string scURL = null;
            string scUser = null;
            string scPswd = null;

            foreach (string arg in lines)
            {
                int i = arg.IndexOf('=');
                string param = arg.Substring(0, i).Trim();
                string value = arg.Substring(i + 1).Trim();

                switch (param)
                {
                    case "SourceControlType":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlType: '" + value + "'");
                        if (value.Equals("GIT"))
                             WorkSpace.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.GIT;
                        else if (value.Equals("SVN"))
                             WorkSpace.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.SVN;
                        else
                             WorkSpace.UserProfile.SourceControlType = SourceControlBase.eSourceControlType.None;
                        break;

                    case "SourceControlUrl":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUrl: '" + value + "'");
                        if ( WorkSpace.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                        {
                            if (!value.ToUpper().Contains("/SVN") && !value.ToUpper().Contains("/SVN/"))
                                value = value + "svn/";
                            if (!value.ToUpper().EndsWith("/"))
                                value = value + "/";
                        }
                         WorkSpace.UserProfile.SourceControlURL = value;
                        scURL = value;
                        break;

                    case "SourceControlUser":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlUser: '" + value + "'");
                        if ( WorkSpace.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && value == "")
                            value = "Test";
                         WorkSpace.UserProfile.SourceControlUser = value;
                        scUser = value;
                        break;

                    case "SourceControlPassword":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlPassword: '" + value + "'");
                         WorkSpace.UserProfile.SourceControlPass = value;
                        scPswd = value;
                        break;

                    case "PasswordEncrypted":
                        Reporter.ToLog(eLogLevel.DEBUG, "PasswordEncrypted: '" + value + "'");
                        string pswd =  WorkSpace.UserProfile.SourceControlPass;
                        if (value == "Y")
                            pswd = EncryptionHandler.DecryptwithKey( WorkSpace.UserProfile.SourceControlPass, App.ENCRYPTION_KEY);
                        if ( WorkSpace.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && pswd == "")
                            pswd = "Test";
                         WorkSpace.UserProfile.SourceControlPass = pswd;
                        break;

                    case "SourceControlProxyServer":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected SourceControlProxyServer: '" + value + "'");
                        if (value == "")
                             WorkSpace.UserProfile.SolutionSourceControlConfigureProxy = false;
                        else
                             WorkSpace.UserProfile.SolutionSourceControlConfigureProxy = true;
                        if (value != "" && !value.ToUpper().StartsWith("HTTP://"))
                            value = "http://" + value;
                         WorkSpace.UserProfile.SolutionSourceControlProxyAddress = value;
                        break;

                    case "SourceControlProxyPort":
                        if (value == "")
                             WorkSpace.UserProfile.SolutionSourceControlConfigureProxy = false;
                        else
                             WorkSpace.UserProfile.SolutionSourceControlConfigureProxy = true;
                        Reporter.ToLog(eLogLevel.INFO, "Selected SourceControlProxyPort: '" + value + "'");
                         WorkSpace.UserProfile.SolutionSourceControlProxyPort = value;
                        break;

                    case "Solution":
                        if (scURL != null && scUser != "" && scPswd != null)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Downloading Solution from source control");
                            if (value.IndexOf(".git") != -1)
                                App.DownloadSolution(value.Substring(0, value.IndexOf(".git") + 4));
                            else
                                App.DownloadSolution(value);
                        }
                        Reporter.ToLog(eLogLevel.DEBUG, "Loading the Solution: '" + value + "'");
                        try
                        {
                            if (App.SetSolution(value) == false)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to load the Solution");
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                            return false;
                        }
                        break;

                    case "Env":
                        Reporter.ToLog(eLogLevel.DEBUG, "Selected Environment: '" + value + "'");
                        ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                        if (env != null)
                        {
                           App.RunsetExecutor.RunsetExecutionEnvironment = env;
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to find matching Environment in the Solution");
                            return false;
                        }
                        break;

                    case "RunSet":
                        Reporter.ToLog(eLogLevel.DEBUG, string.Format("Selected {0}: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), value));
                        ObservableList<RunSetConfig> RunSets = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>();
                        RunSetConfig runSetConfig = RunSets.Where(x => x.Name.ToLower().Trim() == value.ToLower().Trim()).FirstOrDefault();
                        if (runSetConfig != null)
                        {
                            WorkSpace.RunsetExecutor.RunSetConfig = runSetConfig;
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to find matching {0} in the Solution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                            return false;
                        }
                        break;

                    default:
                        Reporter.ToLog(eLogLevel.ERROR, "Un Known argument: '" + param + "'");
                        return false;
                }
               
            }
            return true;
        }

        public void CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening)
        {
            Dictionary<Guid, string> defectsOpeningResults;
            if ((defectsForOpening != null) && (defectsForOpening.Count > 0))
            {
                defectsOpeningResults = ALMIntegration.Instance.CreateNewALMDefects(defectsForOpening);
            }
            else
                return;

            if ((defectsOpeningResults != null) && (defectsOpeningResults.Count > 0))
            {
                foreach (KeyValuePair<Guid, string> defectOpeningResult in defectsOpeningResults)
                {
                    if ((defectOpeningResult.Value != null) && (defectOpeningResult.Value != "0"))
                    {
                        WorkSpace.RunsetExecutor.DefectSuggestionsList.Where(x => x.DefectSuggestionGuid == defectOpeningResult.Key).ToList().ForEach(z => { z.ALMDefectID = defectOpeningResult.Value; z.IsOpenDefectFlagEnabled = false; });
                    }
                }
            }
        }

        public void HTMLReportAttachment(string extraInformationCalculated, string emailReadyHtml, string reportsResultFolder, string runSetFolder, object Report, object conf)
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
                reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder),
                                                                                                                        false,
                                                                                                                        HTMLReportConfigurations.Where(x => (x.ID == rReport.SelectedHTMLReportTemplateID)).FirstOrDefault(),
                                                                                                                        extraInformationCalculated + "\\" + System.IO.Path.GetFileName(runSetFolder), false, currentConf.HTMLReportConfigurationMaximalFolderSize);
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
                if (GingerCore.General.GetInputWithValidation("Add Report Template", "Report Template Name:", ref NewReportName, System.IO.Path.GetInvalidFileNameChars()))
                {
                    NewReportTemplate.Name = NewReportName;
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);
                }
                return NewReportTemplate;
            }
            return null;
        }

        public string GenerateReportForREportTemplate(string ReportTemplateName, object RIf, object RTs )
        {
            ReportInfo RI = (ReportInfo)RIf;
            ReportTemplate RT = (ReportTemplate)RTs;
            ReportPage RP = new ReportPage(RI, RT.Xaml);
            string FileName = Path.GetTempPath() + ReportTemplateName + ".rtf";

            if (System.IO.File.Exists(FileName))
                FileName = Path.GetTempPath() + " " + DateTime.Now.ToString("dMMMyyyy_HHmmss_fff") + "_" + ReportTemplateName + ".rtf";

            GC.Collect();
            RP.SaveReport(FileName);

            string PDFFileName = FileName.Replace(".rtf", ".pdf");

            RTFtoPDF.Convert(FileName, PDFFileName);

            return PDFFileName;
        }

        public void ExecuteActScriptAction(string ScriptFileName, string SolutionFolder)
        {
            //TODO: Remove from here and execute it in actual RunSetActionScript.cs (Not perticularly tested)
            ActScript act = new ActScript();
            string FileName = ScriptFileName.Replace(@"~\", SolutionFolder);

            Ginger.Run.RunSetActions.RunSetActionScript actionScript = new RunSetActionScript();
            actionScript.VerifySolutionFloder(SolutionFolder, FileName);
            
            act.ScriptName = FileName;
            act.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            act.Execute();
            //this.Errors = act.Error;
        }

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> bfs, string result, PublishToALMConfig PublishToALMConfig)
        {
            return ALMIntegration.Instance.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig, ALMIntegration.eALMConnectType.Auto, false);
        }
    }
    
}
