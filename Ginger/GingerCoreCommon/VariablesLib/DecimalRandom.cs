#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Threading;

namespace GingerCore.Variables
{
    public class DecimalRandom 
    {
        Random rnd = new Random(Guid.NewGuid().GetHashCode());
    
        public decimal NextDecimal(bool ZeroToOne = false)
        {
            // Improve fare distribution as rnd is not really random
            Thread.Sleep(1);          
            //The low 32 bits of a 96-bit integer. 
            int lo = rnd.Next(int.MinValue, int.MaxValue);

            Thread.Sleep(1);          
            //The middle 32 bits of a 96-bit integer. 
            int mid = rnd.Next(int.MinValue, int.MaxValue);

            Thread.Sleep(1);          
            //The high 32 bits of a 96-bit integer. 
            int hi = rnd.Next(int.MinValue, int.MaxValue);
            //The sign of the number; 1 is negative, 0 is positive. 
            bool isNegative;
            //A power of 10 ranging from 0 to 28.      
            byte scale;            
            // Can have fraction up to 28 digits after the .
            if (ZeroToOne)
            {
                isNegative = false;
                scale = Convert.ToByte(28);
            }
            else
            {
                isNegative = (rnd.Next(2) == 0);
                scale = Convert.ToByte(rnd.Next(29));
            }

            Decimal randomDecimal = new Decimal(lo, mid, hi, isNegative, scale);
            if (ZeroToOne)
            {
                while (randomDecimal > 1) { randomDecimal--; };                
            }            
            return randomDecimal;
        }

        public decimal NextDecimal(decimal Min, decimal Max, bool integer)
        {
            //Get random decimal 
            decimal d1 = NextDecimal(true);

            //Make sure it is a decimal between 0-1
            decimal d0_1 = Math.Abs(d1);            
            while (d0_1 > 1)
            {
                d0_1 = d0_1 /10M;
            }
            
            decimal d = d0_1 * (decimal)Math.Abs(Max-Min) + Min;
            if (integer)
            {
                d = Math.Round(d,0);
            }
            return d;
        }
    }
}