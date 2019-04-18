using Amdocs.Ginger.Plugin.Core.ActionsLib;
using Amdocs.Ginger.Plugin.Core.PlugInsLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Drivers
{
    [ServiceFeature]
   public interface IWebPlatform
    {
        void HandleBrowserElementAction(GingerAction GA, ActBrowserInfo act);
    }
}
