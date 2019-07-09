using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]

    public class ValidValueAttribute : Attribute, IParamProperty
    {
        public string PropertyName => "ValidValue";

        public List<object> ValidValue { get; set; } = new List<object>();

        public ValidValueAttribute()
        {

        }

        public ValidValueAttribute(int validValue)
        {
            
            ValidValue.Add(validValue);
        }

        public ValidValueAttribute(int[] validValues)
        { 
            ValidValue.AddRange(validValues.Cast<object>());
        }

        public ValidValueAttribute(string[] validValues)
        {
            ValidValue.AddRange(validValues.Cast<object>());
        }
        public ValidValueAttribute(bool[] validValues)
        {
            ValidValue.AddRange(validValues.Cast<object>());
        }        
    }
}
