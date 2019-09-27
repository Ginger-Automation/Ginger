#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    /// <summary>
    /// A class tohold the info of available services from a specific driver. 
    /// A lsit of driver info will be created during agent creation which will hold all the available drivers/services from different plugins 
    /// </summary>
    public class DriverInfo
    {

        public readonly bool isDriverPlugin;

        // ObservableList<PluginPackage> Plugins;

        public List<object> services = new List<object>();

        public readonly string Name;


        public DriverInfo(string Name, bool isPlugin = false)
        {
            isDriverPlugin = isPlugin;

            this.Name = Name;
            if (isDriverPlugin)
            {
                ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            }
        }

        public override string ToString()
        {
            return Name;
        }
        public static List<DriverInfo> GetDriversforPlatform(ePlatformType platformType)
        {
            List<DriverInfo> driverTypes = new List<DriverInfo>();


            if (platformType == ePlatformType.Service)
            {

            }

            else
            {
                driverTypes.Add(GetDriver(platformType));

                driverTypes.AddRange(GetServices(platformType));
            }
            return driverTypes;
        }

        private static List<DriverInfo> GetServices(ePlatformType platformType)
        {
            List<DriverInfo> PlatformServices = new List<DriverInfo>();

            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            string PlatformInterface = string.Empty;


          
   
            switch(platformType)
            {
                case ePlatformType.Web:
                    PlatformInterface = "IWebPlatform";

                    break;
                case ePlatformType.WebServices:
                    PlatformInterface = "IWebServicePlatform";

                    break;

            }


            if(!string.IsNullOrEmpty(PlatformInterface))
            {
                foreach (PluginPackage plugin in Plugins) 
                {
                    DriverInfo DI = new DriverInfo(plugin.PluginPackageInfo.Id, true);
                    foreach (PluginServiceInfo PI in plugin.Services.Where(a => a.Interfaces.Contains(PlatformInterface)))
                    {

                        DI.services.Add(PI.ServiceId);

                    }
                    if (DI.services.Count > 0)
                    {
                        PlatformServices.Add(DI);
                    }
                }
            }
            return PlatformServices;
        }

        private static DriverInfo GetDriver(ePlatformType platformType)
        {
            DriverInfo DI = new DriverInfo(platformType + " Driver");


            if (platformType == ePlatformType.Web)
            {

                DI.services.Add(Agent.eDriverType.InternalBrowser);
                DI.services.Add(Agent.eDriverType.SeleniumChrome);
                DI.services.Add(Agent.eDriverType.SeleniumFireFox);
                DI.services.Add(Agent.eDriverType.SeleniumIE);
                DI.services.Add(Agent.eDriverType.SeleniumRemoteWebDriver);
                DI.services.Add(Agent.eDriverType.SeleniumEdge);



            }
            else if (platformType == ePlatformType.Java)
            {
                DI.services.Add(Agent.eDriverType.JavaDriver);

            }
            else if (platformType == ePlatformType.Mobile)
            {
                DI.services.Add(Agent.eDriverType.MobileAppiumAndroid);
                DI.services.Add(Agent.eDriverType.MobileAppiumIOS);
                DI.services.Add(Agent.eDriverType.PerfectoMobileAndroid);
                DI.services.Add(Agent.eDriverType.PerfectoMobileAndroidWeb);
                DI.services.Add(Agent.eDriverType.PerfectoMobileIOS);
                DI.services.Add(Agent.eDriverType.PerfectoMobileIOSWeb);
                DI.services.Add(Agent.eDriverType.MobileAppiumAndroidBrowser);
                DI.services.Add(Agent.eDriverType.MobileAppiumIOSBrowser);
            }
            else if (platformType == ePlatformType.Windows)
            {
                DI.services.Add(Agent.eDriverType.WindowsAutomation);                
            }
            else if (platformType == ePlatformType.PowerBuilder)
            {
                DI.services.Add(Agent.eDriverType.PowerBuilder);
            }

            else if (platformType == ePlatformType.Unix)
            {
                DI.services.Add(Agent.eDriverType.UnixShell);

            }
            else if (platformType == ePlatformType.DOS)
            {
                DI.services.Add(Agent.eDriverType.DOSConsole);
            }

            else if (platformType == ePlatformType.WebServices)
            {
                DI.services.Add(Agent.eDriverType.WebServices);
            }

            //else if (platformType == ePlatformType.AndroidDevice.ToString())
            //{
            //    DI.services.Add(Agent.eDriverType.AndroidADB);
            //}
            else if (platformType == ePlatformType.ASCF)
            {
                DI.services.Add(Agent.eDriverType.ASCF);
            }

            else if (platformType == ePlatformType.MainFrame)
            {
                DI.services.Add(Agent.eDriverType.MainFrame3270);
            }
            else
            {
                DI = null;
            }

            return DI;
        }

    }
}
