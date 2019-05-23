using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class ElementActionCongifuration
    {
        public object LocateBy { get; set; }
        public string LocateValue { get; set; }
        public string ElementValue { get; set; }
        public string Operation { get; set; }
        public object Type { get; set; }
        public string Description { get; set; }
        public object LearnedElementInfo { get; set; }

        public bool AddPOMToAction { get; set; }
        public string POMGuid { get; set; }
        public string ElementGuid { get; set; }        
    }
}
