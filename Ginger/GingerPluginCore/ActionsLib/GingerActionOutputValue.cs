using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class GingerActionOutputValue : IGingerActionOutputValue
    {
        public string Param { get; set; }
        public object Value { get; set; }
        public string Path { get; set; }
    }
}
