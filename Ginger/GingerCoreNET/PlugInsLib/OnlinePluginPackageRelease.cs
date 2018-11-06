using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    public class OnlinePluginPackageRelease
    {
        public string tag_name;
        public string Version { get { return tag_name; } }
        public string name { get; set; }        
        public string prerelease { get; set; } 
        public string published_at { get; set; } 
        public ObservableList<OnlinePluginPackageReleaseAsset> assets { get; set; }                
    }
}
