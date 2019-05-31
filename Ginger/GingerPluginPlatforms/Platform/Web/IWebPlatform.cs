using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{
    // mark this interface to be in the json and will be visible to activities targetting Web platform
    [GingerInterface("IWebPlatform", "Web Platform driver")]
    public interface IWebPlatform: IPlatformService
    {
       
        IBrowserActions BrowserActions { get;  }

        ILocateWebElement LocateWebElement { get;  }

        IAlerts Alerts { get; }


        // TODO: add all Web interfaces
        // ICookiesHandling
    }
}
