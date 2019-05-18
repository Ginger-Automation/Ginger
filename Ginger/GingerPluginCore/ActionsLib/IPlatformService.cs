using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
   public interface IPlatformService
    {
        IPlatformActionHandler PlatformActionHandler { get; set; }
    }
}
