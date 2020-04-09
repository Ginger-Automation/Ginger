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

        private double? mMinValue;
        private double? mMaxValue;

        private int? mPrecisionValue;

        private bool mIsDecimalValue;
        
        [IsSerializedForLocalRepository]
        public bool IsDecimalValue
        {
            set
            {
                mIsDecimalValue = value;
                OnPropertyChanged("IsDecimalValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                return mIsDecimalValue;
            }
        }

        [IsSerializedForLocalRepository]
        public double? MinValue
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
        public double? MaxValue
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
            get { return false; }
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
                if(!mIsDecimalValue)
                {
                    mInitialNumberValue = Math.Round(value);
                }
                else
                {
                    mInitialNumberValue = value;
                }
                
                //Validate();
                Value = value.ToString();
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                return mInitialNumberValue;
            }
        }

        private void Validate()
        {
            //throw new NotImplementedException();
        }

        [IsSerializedForLocalRepository]
        public int? PrecisionValue
        {
            get
            {
                //if(mPrecisionValue == 0)
                //{
                //    return 2;
                //}
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
            var isNumeric = IsNumericValue(mInitialNumberValue);
            if (!mIsDecimalValue)
            {
                return "Initial Value=" + Convert.ToInt32(mInitialNumberValue);
            }
            else
            {
                //var setPrecision = new NumberFormatInfo();
                //setPrecision.NumberDecimalDigits = mPrecisionValue;
                mInitialNumberValue = Math.Round(Convert.ToDouble(mInitialNumberValue),Convert.ToInt32(mPrecisionValue));
                return "Initial Value=" + mInitialNumberValue;//mInitialNumberValue.ToString("N", setPrecision);
            }

        }

        private bool IsNumericValue(object inputValue)
        {
            double retNum;
            bool isNum = Double.TryParse(Convert.ToString(inputValue), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
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
