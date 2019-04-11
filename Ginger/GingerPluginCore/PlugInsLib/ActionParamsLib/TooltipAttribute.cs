using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class TooltipAttribute : Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Tooltip";

        public string mTooltip { get; set; }

        public TooltipAttribute(string tooltip)
        {
            mTooltip = tooltip;
        }

        public TooltipAttribute()
        {
        }
    }
}
