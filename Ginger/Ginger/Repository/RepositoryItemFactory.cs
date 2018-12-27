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
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.ASCF;
using GingerCore.Drivers.ConsoleDriverLib;
using GingerCore.Drivers.InternalBrowserLib;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.MainFrame;
using GingerCore.Drivers.Mobile.Perfecto;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Drivers.WindowsLib;
using static GingerCore.Agent;
using GingerCore.Drivers.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Ginger.Repository
{
    public class RepositoryItemFactory : IRepositoryItemFactory
    {
        
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


        public IActivitiesGroup CreateActivitiesGroup()
        {
            return new ActivitiesGroup();
        }
        public ObservableList<IDatabase> GetDatabaseList()
        {
            return new ObservableList<IDatabase>();
        }

        public ObservableList<DataSourceBase> GetDatasourceList()
        {
            return new ObservableList<DataSourceBase>();
        }


        public ObservableList<IAgent> GetAllIAgents()
        {
            return new ObservableList<IAgent>( WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().ListItems.ConvertAll(x => (IAgent)x));
        }
        public ObservableList<ProjEnvironment> GetAllEnvironments()
        {
            return new ObservableList<ProjEnvironment>(WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().ListItems.ConvertAll(x => (ProjEnvironment)x));
        }

        //
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
                    Reporter.ToUser(eUserMsgKeys.FailedToConnectAgent, zAgent.Name, e.Message);
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
    }
}
