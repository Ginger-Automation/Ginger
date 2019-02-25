using System;
using System.Collections.Generic;
using System.Text;
using Ginger.Run;
using GingerCore;

namespace Amdocs.Ginger.Common
{
    public class Context
    {
        public BusinessFlow BusinessFlow { get; set; }
        public GingerRunner Runner { get; set; }
    }
}
