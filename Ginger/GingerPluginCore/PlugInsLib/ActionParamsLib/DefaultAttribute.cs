using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class DefaultAttribute : Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Default";

        public string DefaultValue { get; set; }

        public DefaultAttribute(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public DefaultAttribute()
        {
        }
    }
  
}
