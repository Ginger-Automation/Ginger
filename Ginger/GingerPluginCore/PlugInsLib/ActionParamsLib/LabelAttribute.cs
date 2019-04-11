using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class LabelAttribute : Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Label";

        public string mLabel { get; set; }

        public LabelAttribute(string label)
        {
            mLabel = label;
        }

        public LabelAttribute()
        {
        }
    }
}
