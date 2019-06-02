using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    // Enable to define Valid values for int param
    // [InvalidValue(5)]   // 5 is not valid value for this param
    // [InvalidValues(new int[] { 1, 5, 10})]   // 1,5,10 are not valid values for this param
    

    // Can set multiple times per param
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = true)]
    public class InvalidValueAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "InvalidValue";
        
        public List<int> InvalidValue { get; set; }

        public InvalidValueAttribute(int invalidValue)
        {
            if (InvalidValue == null)
            {
                InvalidValue = new List<int>();
            }
            InvalidValue.Add(invalidValue);
        }

        public InvalidValueAttribute(int[] invalidValues)
        {
            if (InvalidValue == null)
            {
                InvalidValue = new List<int>();
            }
            InvalidValue.AddRange(invalidValues);            
        }

        public InvalidValueAttribute()
        {
        }
    }
}
