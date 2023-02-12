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
using System.Collections.Generic;
using System.Globalization;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableNumber : VariableBase
    {
        private string mInitialNumberValue;

        private string mMinValue;
        private string mMaxValue;

        private string mPrecisionValue;

        private bool? mIsDecimalValue;
        
        [IsSerializedForLocalRepository]
        public bool IsDecimalValue
        {
            set
            {
                mIsDecimalValue = value;
                OnPropertyChanged("IsDecimalValue");
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                if (mIsDecimalValue == null)
                {
                    return false;
                }
                    
                return Convert.ToBoolean(mIsDecimalValue);
            }
        }

        [IsSerializedForLocalRepository]
        public string MinValue
        {
            get 
            {
                if (string.IsNullOrEmpty(mMinValue))
                {
                    mMinValue = GetMinValue();
                }
                return mMinValue; 
            }
            set 
            {
                mMinValue = value;
                OnPropertyChanged("MinValue");
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
        }

        private string GetMinValue()
        {
            if (!IsDecimalValue)
            {
                return Int32.MinValue.ToString();
            }
            else
            {
                return float.MinValue.ToString();
            }
        }

        [IsSerializedForLocalRepository]
        public string MaxValue
        {
            get 
            { 
                if(string.IsNullOrEmpty(mMaxValue))
                {
                    mMaxValue = GetMaxValue();
                }
                return mMaxValue;             
            }
            set
            { 
                mMaxValue = value;
                OnPropertyChanged("MaxValue");
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
        }

        private string GetMaxValue()
        {
            if(!IsDecimalValue)
            {
                return Int32.MaxValue.ToString();
            }
            else
            {
                return float.MaxValue.ToString();
            }
        }

        [IsSerializedForLocalRepository("2")]
        public string PrecisionValue
        {
            get
            {
                return mPrecisionValue;
            }
            set
            {
                mPrecisionValue = value;
                OnPropertyChanged("PrecisionValue");
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
        }

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
        public string InitialNumberValue
        {
            set
            {
                if(mIsDecimalValue == null)
                {
                    mInitialNumberValue = value;
                }
                else if(!Convert.ToBoolean(mIsDecimalValue))
                {
                    mInitialNumberValue = GetValidInteger(value);
                }
                else
                {
                    mInitialNumberValue = GetFloatWithPrecisionValue(value);
                }

                Value = mInitialNumberValue;
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                if(string.IsNullOrEmpty(mInitialNumberValue))
                {
                    mInitialNumberValue = "0";
                }
                return mInitialNumberValue;
            }
        }

        private string GetFloatWithPrecisionValue(string value)
        {
            if (!string.IsNullOrEmpty(mPrecisionValue) && value.Contains(".") && value.Split('.')[1].Length > 0)
            {
                var validFloat = ConvertStringToNumber(value);
                return Math.Round(Convert.ToDouble(validFloat), Convert.ToInt32(mPrecisionValue)).ToString();
            }
            else if (Convert.ToInt32(mPrecisionValue) > 0 && !value.Contains("."))
            {
                string val = value;
                val += ".";
                for (int i = 0; i < Convert.ToInt32(mPrecisionValue); i++)
                    val += "0";
                return val;
            }
            else
            {
                return value;
            }
        }

        private float ConvertStringToNumber(string value)
        {
           bool isValidNumber = float.TryParse(value, out var validFloat);

            if (!isValidNumber)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Not a valid Number : {value}");
            }
            if ((!string.IsNullOrEmpty(mMinValue) || !string.IsNullOrEmpty(mMaxValue)) && !CheckNumberInRange(validFloat))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Input Number is not in range: Min. Value: [{mMinValue}] - Max. Value: [{mMaxValue}].");
            }


            return validFloat;
        }

        private  string GetValidInteger(string value)
        {
            try
            {
                float validInteger = ConvertStringToNumber(value);

                return Convert.ToInt32(validInteger).ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred during GetValidInteger..", ex);
                return mInitialNumberValue;
            }

        }



        public override bool GenerateAutoValue(ref string errorMsg)
        {
            //NA
            errorMsg = "Generate Auto Value is not supported";
            return false;
        }

        public override string GetFormula()
        {
            if (!Convert.ToBoolean(mIsDecimalValue))
            {
                 mInitialNumberValue = GetValidInteger(mInitialNumberValue);
            }
            else
            {
                mInitialNumberValue = GetFloatWithPrecisionValue(mInitialNumberValue);
            }
            return "Initial Value=" + mInitialNumberValue;
        }

        public override List<eSetValueOptions> GetSupportedOperations()
        {
            var supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(eSetValueOptions.SetValue);
            supportedOperations.Add(eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override bool SetValue(string value)
        {
            bool isValidationPass = false;
            bool isValidNumber = float.TryParse(value, out var number);

            if (isValidNumber && CheckNumberInRange(number))
            {
                //integer type validation
                if (!IsDecimalValue && !IsIntegerValue(value))
                {
                    isValidationPass = false;
                }
                else
                {
                    isValidationPass = true;
                }

            }

            if (isValidationPass)
            {
                this.Value = value;
            }
            return isValidationPass;
        }

        public override void ResetValue()
        {
            Value = mInitialNumberValue;
        }
        private bool IsIntegerValue(string value)
        {
            return int.TryParse(value, out var result);
        }

        public bool CheckNumberInRange(float number)
        {
            var minValue = float.Parse(this.MinValue);
            var maxValue = float.Parse(this.MaxValue);
            if ((number < minValue) || (number > maxValue))
            {
                return  false;
            }
            return true;
        }
    }
}
