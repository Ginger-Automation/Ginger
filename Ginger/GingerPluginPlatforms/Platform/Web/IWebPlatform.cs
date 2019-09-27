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
