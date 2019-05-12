using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class PageChangedEventArgs
    {
        public string PageURL { get; set; }
        public string PageTitle { get; set; }
        public string ScreenShot { get; set; }
    }
}
