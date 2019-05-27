using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
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
        public static List<DriverInfo> GetDriversforPlatform(string platformType)
        {
            List<DriverInfo> driverTypes = new List<DriverInfo>();


            if (platformType == ePlatformType.Service.ToString())
            {

            }

            else
            {
                driverTypes.Add(GetDriver(platformType));

                driverTypes.AddRange(GetServices(platformType));
            }
            return driverTypes;
        }

        private static List<DriverInfo> GetServices(string platformType)
        {
            List<DriverInfo> PlatformServices = new List<DriverInfo>();

            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();

            if (platformType == ePlatformType.Web.ToString())
            {
                foreach (PluginPackage plugin in Plugins) //.Where(x => x.Services!=null && x.Services.Any(a=>a.Interfaces.Contains("IWebPlatform"))))
                {
                    DriverInfo DI = new DriverInfo(plugin.PluginPackageInfo.Id, true);
                    foreach (PluginServiceInfo PI in plugin.Services.Where(a => a.Interfaces.Contains("IWebPlatform")))
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

        private static DriverInfo GetDriver(string platformType)
        {
            DriverInfo DI = new DriverInfo(platformType + " Driver");


            if (platformType == ePlatformType.Web.ToString())
            {

                DI.services.Add(Agent.eDriverType.InternalBrowser);
                DI.services.Add(Agent.eDriverType.SeleniumChrome);
                DI.services.Add(Agent.eDriverType.SeleniumFireFox);
                DI.services.Add(Agent.eDriverType.SeleniumIE);
                DI.services.Add(Agent.eDriverType.SeleniumRemoteWebDriver);
                DI.services.Add(Agent.eDriverType.SeleniumEdge);



            }
            else if (platformType == ePlatformType.Java.ToString())
            {
                DI.services.Add(Agent.eDriverType.JavaDriver);

            }
            else if (platformType == ePlatformType.Mobile.ToString())
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
            else if (platformType == ePlatformType.Windows.ToString())
            {
                DI.services.Add(Agent.eDriverType.WindowsAutomation);
                DI.services.Add(Agent.eDriverType.FlaUIWindow);
            }
            else if (platformType == ePlatformType.PowerBuilder.ToString())
            {
                DI.services.Add(Agent.eDriverType.PowerBuilder);
                DI.services.Add(Agent.eDriverType.FlaUIPB);
            }

            else if (platformType == ePlatformType.Unix.ToString())
            {
                DI.services.Add(Agent.eDriverType.UnixShell);

            }
            else if (platformType == ePlatformType.DOS.ToString())
            {
                DI.services.Add(Agent.eDriverType.DOSConsole);
            }

            else if (platformType == ePlatformType.WebServices.ToString())
            {
                DI.services.Add(Agent.eDriverType.WebServices);
            }

            else if (platformType == ePlatformType.AndroidDevice.ToString())
            {
                DI.services.Add(Agent.eDriverType.AndroidADB);
            }
            else if (platformType == ePlatformType.ASCF.ToString())
            {
                DI.services.Add(Agent.eDriverType.ASCF);
            }

            else if (platformType == ePlatformType.MainFrame.ToString())
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
