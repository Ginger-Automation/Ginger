#region License
/*
Copyright © 2014-2020 European Support Limited

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
using System.Collections.Generic;
using System.Globalization;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableNumber : VariableBase
    {
        private double mInitialNumberValue;

        private double mMinValue;
        private double mMaxValue;

        private int mPrecisionValue;

        private bool mIsIntegerValue;
        
        [IsSerializedForLocalRepository]
        public bool IsIntegerValue
        {
            set
            {
                mIsIntegerValue = value;
                if (IsIntegerValue)
                {
                    mMinValue = Int32.MinValue;
                    mMaxValue = Int32.MaxValue;
                }
                else
                {
                    mMinValue = float.MinValue;
                    mMaxValue = float.MaxValue;
                    mPrecisionValue = 2; //default
                }
                mInitialNumberValue = MinValue;
            }
            get
            {
                return mIsIntegerValue;
            }
        }

        [IsSerializedForLocalRepository]
        public double MinValue
        {
            get 
            {
                return mMinValue; 
            }
            set 
            {
                mMinValue = value;
                OnPropertyChanged("MinValue");
                OnPropertyChanged("Formula");
            }
        }

        [IsSerializedForLocalRepository]
        public double MaxValue
        {
            get 
            { 
                return mMaxValue;
            }
            set
            { 
                mMaxValue = value;
                OnPropertyChanged("MaxValue");
                OnPropertyChanged("Formula");
            }
        }

        public VariableNumber() { }
        public override string VariableType
        {
            get { return "Number"; }
        }

        public override string VariableEditPage
        {
            get
            {
                return "VariableNumberPage";
            }
        }
        public override eImageType Image
        {
            get { return eImageType.SequentialExecution; }
        }

        public override bool SupportResetValue
        {
            get { return true; }
        }

        public override bool SupportAutoValue
        {
            get { return true; }
        }

        public override bool SupportSetValue
        {
            get { return true; }
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Number"; }
        }

        [IsSerializedForLocalRepository]
        public double InitialNumberValue
        {
            set
            {
                mInitialNumberValue = value;
                Value = value.ToString();
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                return mInitialNumberValue;
            }
        }

        [IsSerializedForLocalRepository]
        public int PrecisionValue
        {
            get
            {
                if(mPrecisionValue ==0)
                {
                    mPrecisionValue = 2;
                }
                return mPrecisionValue;
            }
            set
            {
                mPrecisionValue = value;
                OnPropertyChanged("Formula");
                OnPropertyChanged("PrecisionValue");
                OnPropertyChanged("InitialNumberValue");
            }
        }

        public override void GenerateAutoValue()
        {

        }

        public override string GetFormula()
        {
            var isIntegerNumber = IsNumericValue(mInitialNumberValue);
            if (isIntegerNumber)
            {
                return "Initial Value=" + mInitialNumberValue;
            }
            else
            {
                //var setPrecision = new NumberFormatInfo();
                //setPrecision.NumberDecimalDigits = PrecisionValue;
                mInitialNumberValue = Math.Round(mInitialNumberValue, PrecisionValue);
                return "Initial Value=" + mInitialNumberValue;//mInitialNumberValue.ToString("N", setPrecision);
            }

        }

        private bool IsNumericValue(object inputValue)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(inputValue), System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }

        public override List<eSetValueOptions> GetSupportedOperations()
        {
            var supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(eSetValueOptions.SetValue);
            supportedOperations.Add(eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override void ResetValue()
        {
            Value = mInitialNumberValue.ToString();
        }
    }
}
