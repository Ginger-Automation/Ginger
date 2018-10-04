using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackageInfo
    {
        public static string cInfoFile = "Ginger.PluginPackage.json";

        public string Id { get; set; }
        public string Version { get; set; }    
        public string ProjectUrl { get; set; }
        public string  Description { get; set; }
        public string Summary { get; set; }
        public string StartupDLL { get; set; }
    }
}
