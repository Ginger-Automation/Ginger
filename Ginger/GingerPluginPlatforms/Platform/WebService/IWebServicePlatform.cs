﻿using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.ActionsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin.Platform.WebService
{
    [GingerInterface("IWebServicePlatform", "WebService Platform driver")]
    public interface IWebServicePlatform: IPlatformService
    {
        IHTTPClient RestClient { get; set; }
    }
}
