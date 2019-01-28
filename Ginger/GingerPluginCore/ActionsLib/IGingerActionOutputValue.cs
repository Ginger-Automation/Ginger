using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public interface IGingerActionOutputValue
    {
        string Param { get; set; }
        object Value { get; set; }
        string Path { get; set; }
    }
}
