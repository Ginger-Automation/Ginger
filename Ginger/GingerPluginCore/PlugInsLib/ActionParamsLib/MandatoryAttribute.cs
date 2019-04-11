using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    // Define param propert is Mandatory
    // [Mandatory] 

    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class MandatoryAttribute : Attribute, IActionParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "Mandatory";
    }
}
