using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinValueAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "MinValue";

        public int MinValue { get; set; }

        public MinValueAttribute(int minValue)
        {
            MinValue = minValue; 
        }

        public MinValueAttribute()
        {
        }
    }
}
