using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{

    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class MaxValueAttribute : Attribute, IActionParamProperty
    {        
        // when saved to services json the attr property name will be:
        public string PropertyName => "MaxValue";

        public int MaxValue { get; set; }

        public MaxValueAttribute(int maxValue)
        {
            MaxValue = maxValue;
        }

        public MaxValueAttribute()
        {            
        }
    }
}
