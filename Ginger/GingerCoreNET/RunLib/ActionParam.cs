using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class ActionParam
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public Type ParamType { get; set; }

        public object Value { get; set; }

        public string GetValueAsString()
        {
            if (Value != null)
            {
                return Value.ToString();
            }
            else
            {
                return null;
            }
        }

        public void SetValueFromString(string s)
        {

        }

        // Add also int for nullable int
        internal int GetValueAsInt()
        {
            return int.Parse(Value.ToString());
        }
    }
}
