using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Utils
{
    public class StringManager
    {
        public static bool IsNumeric(string sValue)
        {
            // simple method to check is strign is number
            // there are many other alternatives, just keep it simple and make sure it run fast as it is going to be used a lot, for every return value calc   
            // regec and other are more expensive

            foreach (char c in sValue)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }
    }
}
