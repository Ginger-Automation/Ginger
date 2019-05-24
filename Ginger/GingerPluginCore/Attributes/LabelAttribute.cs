using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class LabelAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Label";

        public string Label { get; set; }

        public LabelAttribute(string label)
        {
            Label = label;
        }

        public LabelAttribute()
        {
        }
    }
}
