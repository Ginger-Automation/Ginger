using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public interface IGingerActionOutput
    {
        List<IGingerActionOutputValue> OutputValues { get; set; }        
    }
}
