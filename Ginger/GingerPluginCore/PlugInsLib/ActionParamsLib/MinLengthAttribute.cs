using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinLengthAttribute: Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "MinLength";

        public int MinLength { get; set; }

        public MinLengthAttribute(int minLength)
        {
            MinLength = minLength;
        }

        public MinLengthAttribute()
        {
        }
    }
}