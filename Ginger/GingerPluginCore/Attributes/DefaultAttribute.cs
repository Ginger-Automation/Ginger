using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class DefaultAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Default";

        public object DefaultValue { get; set; }

        public DefaultAttribute(object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public DefaultAttribute()
        {
        }
    }
  
}
