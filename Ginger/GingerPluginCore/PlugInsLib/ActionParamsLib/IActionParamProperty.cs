using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    // interface to be implemented by param property attribute
    public interface IActionParamProperty
    {
        string PropertyName { get; }    // set the property name to be written to json
    }
}
