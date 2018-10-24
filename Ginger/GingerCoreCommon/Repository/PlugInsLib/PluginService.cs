using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.Common
{
    public class PluginService
    {
        public List<MethodInfo> mStandAloneMethods = new List<MethodInfo>();

        public string ServiceId { get; internal set; }
    }
}
