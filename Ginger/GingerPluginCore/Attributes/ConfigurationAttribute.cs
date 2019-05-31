using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.Attributes
{
    public class ServiceConfigurationAttribute : Attribute, IParamProperty
    {
        public readonly string Name;
        public readonly string Description;
        public ServiceConfigurationAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string PropertyName => "ServiceConfiguration";

        public override string ToString()
        {
            return Name;
        }
    }
}
