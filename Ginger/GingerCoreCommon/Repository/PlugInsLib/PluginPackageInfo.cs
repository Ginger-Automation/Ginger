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

        //TODO: Add Author, owner license, url, tags etc..
    }
}
