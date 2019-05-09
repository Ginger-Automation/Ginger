using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET
{
    public interface IPlatformInfo
    {
        Act GetPlatformAction(ElementInfo eInfo, ElementActionCongifuration actConfig);
    }
}
