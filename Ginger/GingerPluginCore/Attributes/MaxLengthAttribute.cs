using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter , AllowMultiple = false)]
    public class MaxLengthAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "MaxLength";

        public int MaxLength { get; set; }

        public MaxLengthAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }

        public MaxLengthAttribute()
        {

        }
    }
}

