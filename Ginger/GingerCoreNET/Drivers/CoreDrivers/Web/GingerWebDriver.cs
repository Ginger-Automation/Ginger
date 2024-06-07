using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using GingerCore;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web
{
    public abstract class GingerWebDriver : DriverBase
    {
        private static readonly IEnumerable<WebBrowserType> SeleniumSupportedBrowserTypes = new List<WebBrowserType>()
        {
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge,
            WebBrowserType.Brave,
            WebBrowserType.InternetExplorer,
            WebBrowserType.RemoteWebDriver,
        };
        private static readonly IEnumerable<WebBrowserType> PlaywrightSupportedBrowserTypes = new List<WebBrowserType>()
        {
            WebBrowserType.Chrome,
            WebBrowserType.FireFox,
            WebBrowserType.Edge
        };

        [UserConfigured]
        [UserConfiguredEnumType(typeof(WebBrowserType))]
        [UserConfiguredDefault("Chrome")]
        [UserConfiguredDescription("Browser Type")]
        public virtual WebBrowserType BrowserType { get; set; }

        public override ePlatformType Platform => ePlatformType.Web;

        public static IEnumerable<WebBrowserType> GetSupportedBrowserTypes(Agent.eDriverType driverType)
        {
            if (driverType == Agent.eDriverType.Selenium)
            {
                return SeleniumSupportedBrowserTypes;
            }
            else if (driverType == Agent.eDriverType.Playwright)
            {
                return PlaywrightSupportedBrowserTypes;
            }
            else
            {
                throw new ArgumentException($"Unknown web driver type '{driverType}'");
            }
        }
    }
}
