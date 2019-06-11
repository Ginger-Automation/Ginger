using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Ginger.Plugin.Platform.Web.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.Web
{

    /// <summary>
    ///Exposes the Functionality for Implementing a plugin supporting Web Platform On Ginger Automation
    /// </summary>
    // mark this interface to be in the json and will be visible to activities targetting Web platform
    [GingerInterface("IWebPlatform", "Web Platform driver")]
    public interface IWebPlatform: IPlatformService
    {
        /// <summary>
        /// Tells to shift Frames automatically in case of a POM Element
        /// </summary>
        bool AutomaticallyShiftIframe { get; set; }

        /// <summary>
        /// Instance of Executor for Browser Service
        /// </summary>
        IBrowserActions BrowserActions { get;  }
        /// <summary>
        /// Instance of service to Locate element by Different Properties
        /// </summary>
        ILocateWebElement LocateWebElement { get;  }
        /// <summary>
        /// Instance of service providing hanling for Alerts
        /// </summary>
        IAlerts Alerts { get; }


        // TODO: add all Web interfaces
        // ICookiesHandling
    }
}
