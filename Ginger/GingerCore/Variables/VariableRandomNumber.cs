#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System.Collections.Generic;

namespace GingerCore.Variables
{
    public class VariableRandomNumber : VariableBase
    {
        DecimalRandom mDecimalRandom = new DecimalRandom();

        public new static  partial class Fields
        {
            public static string Min = "Min";
            public static string Max = "Max";
            public static string Interval = "Interval";
            public static string isInteger = "IsInteger";     
        }

        public VariableRandomNumber()
        {
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Random Number"; }
        }

        public override string VariableEditPage { get { return "VariableRandomNumberPage"; } }

        private decimal mMin;
        [IsSerializedForLocalRepository]
        public decimal Min { set { mMin = value; OnPropertyChanged("Formula"); } get { return mMin; } }

        private decimal mMax;
        [IsSerializedForLocalRepository]
        public decimal Max { set { mMax = value; OnPropertyChanged("Formula"); } get { return mMax; } }

        private decimal mInterval;
        [IsSerializedForLocalRepository]
        public decimal Interval { set { mInterval = value; OnPropertyChanged("Formula"); } get { return mInterval; } }

        private bool mIsInteger;
        [IsSerializedForLocalRepository]
        public bool IsInteger { set { mIsInteger = value; OnPropertyChanged("Formula"); } get { return mIsInteger; } }        

        public override string GetFormula()
        {
            if (mMin > mMax)
            {
                return "Error: Min>Max";
            }
            string s = mMin + "-" + mMax + " Interval " + mInterval;
            if (mIsInteger) s+= ", Integer";
            return s;
        }

        public override void ResetValue()
        {
            GenerateAutoValue();
        }

        public override void GenerateAutoValue()
        {
            // In case the user is editing the numbers we validate, he will get the err message in the formula
            if (mMin > mMax)
            {
                Value = "Error: Min > Max";
                return;
            }

            if(mMax - mMin < Interval)
            {
                Value = "Error: Max-Min should be greater than Interval";
                return;
            }

            decimal d = mDecimalRandom.NextDecimal(mMin, mMax, IsInteger);
            
            if (Interval != 0)
            {
                // make sure we remove the modulo, so we get nice round numbers per interval request 
                if ((d - d % Interval) < mMin)
                    d = d + (Interval - d%Interval);
                else if ((d - d % Interval) > mMax)
                    d = d - (Interval + d % Interval);
                else
                    d = d - d % Interval;
            }
            
            Value = d.ToString();            
        }

        public override eImageType Image { get { return eImageType.Random; } }

        public override string VariableType() { return "RandomNumber"; }

        public override bool SupportSetValue { get { return false; } }

        public override List<ActSetVariableValue.eSetValueOptions> GetSupportedOperations()
        {
            List<ActSetVariableValue.eSetValueOptions> supportedOperations = new List<ActSetVariableValue.eSetValueOptions>();  
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.AutoGenerateValue);
            return supportedOperations;
        }
    }
}
