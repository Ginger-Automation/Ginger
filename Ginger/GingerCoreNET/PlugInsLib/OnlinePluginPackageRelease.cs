using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    public class OnlinePluginPackageRelease
    {
        public string tag_name { get; set; }
        public string Version
        {
            get
            {
                string v;
                if (tag_name.ToLower().StartsWith("v"))
                {
                    v = tag_name.Substring(1);
                }
                else
                {
                    v = tag_name;
                }
                return v;
            }
        }
        public string name { get; set; }        
        public string prerelease { get; set; } 
        public string published_at { get; set; } 
        public ObservableList<OnlinePluginPackageReleaseAsset> assets { get; set; }                
    }
}
