using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class TooltipAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Tooltip";

        public string Tooltip { get; set; }

        public TooltipAttribute(string tooltip)
        {
            Tooltip = tooltip;
        }

        public TooltipAttribute()
        {
        }
    }
}
