using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.PlugInsLib
{
    public class OnlinePluginPackageReleaseAsset
    {
        public string url { get; set; }
        public string name { get; set; }
        public string content_type { get; set; }
        public int size { get; set; }
        public string download_count { get; set; }
        public string browser_download_url { get; set; }
    }
}
