using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class GingerActionOutput : IGingerActionOutput
    {
        
        public List<IGingerActionOutputValue> OutputValues { get; set; }

        public object this[string paramName]
        {
            get
            {
                return (from x in OutputValues where x.Param == paramName select x.Value).SingleOrDefault();
            }
        }
    }
}
