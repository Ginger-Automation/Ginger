using System;
using System.Collections.Generic;
using System.Text;
using static Amdocs.Ginger.Plugin.Core.ActionsLib.ActInfo;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
   public struct ActBrowserInfo
    {
        public eControlAction ControlAction { get; set; }
        public string LocateValue { get; set; }

        public eLocateBy LocateBy { get; set; }

        public string Value { get; set; }

        public eGotoURLType GotoURLType { get; set; }

    }
}
