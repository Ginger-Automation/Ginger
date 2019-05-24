using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = true)]

    public class ValidValueAttribute : Attribute, IParamProperty
    {
        public string PropertyName => "ValidValue";

        public List<int> ValidValue { get; set; }

        public ValidValueAttribute(int validValue)
        {
            if(ValidValue == null)
            {
                ValidValue = new List<int>();
            }
            ValidValue.Add(validValue);
        }

        public ValidValueAttribute(int[] validValues)
        {
            if(ValidValue == null)
            {
                ValidValue = new List<int>();
            }
            ValidValue.AddRange(validValues);
        }

        public ValidValueAttribute()
        {

        }
    }
}
