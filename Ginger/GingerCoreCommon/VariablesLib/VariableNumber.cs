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
        private string mInitialNumberValue;

        private float? mMinValue;
        private float? mMaxValue;

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
        public float? MinValue
        {
            get 
            {
                return mMinValue; 
            }
            set 
            {
                mMinValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        [IsSerializedForLocalRepository]
        public float? MaxValue
        {
            get 
            { 
                return mMaxValue;
            }
            set
            { 
                mMaxValue = value;
                OnPropertyChanged("MaxValue");
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
        public string InitialNumberValue
        {
            set
            {
                if(!mIsDecimalValue)
                {
                    mInitialNumberValue = GetValidInteger(value).ToString();
                }
                else
                {
                    mInitialNumberValue = GetFloatWithPrecisionValue(value);
                }

                Value = value.ToString();
                OnPropertyChanged("InitialNumberValue");
                OnPropertyChanged("Formula");
            }
            get
            {
                if(mInitialNumberValue == null)
                {
                    mInitialNumberValue = "0";
                }
                return mInitialNumberValue;
            }
        }

        private string GetFloatWithPrecisionValue(string value)
        {
            if (value.Contains(".") && value.Split('.')[1].Length > 0)
            {
                float validFloat;
                float.TryParse(value, out validFloat);
                if (validFloat < mMinValue || validFloat > mMaxValue)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Input value is out of range!");
                    return mInitialNumberValue;
                }
                return Math.Round(Convert.ToDouble(validFloat), Convert.ToInt32(mPrecisionValue)).ToString();
            }
            else
            {
                return value;
            }
        }

        private  string GetValidInteger(string value)
        {
            try
            {
                float validInteger;
                float.TryParse(value, out validInteger);
                if (validInteger < mMinValue || validInteger > mMaxValue)
                {
                    Reporter.ToLog(eLogLevel.WARN, "Input value is out of range!");
                    return mInitialNumberValue;
                }
                return Convert.ToInt32(validInteger).ToString();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured during GetValidInteger..", ex);
                return mInitialNumberValue;
            }

        }

        [IsSerializedForLocalRepository]
        public int? PrecisionValue
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

        public override void GenerateAutoValue()
        {

        }

        public override string GetFormula()
        {
            if (!mIsDecimalValue)
            {
                return "Initial Value=" + GetValidInteger(mInitialNumberValue).ToString();
            }
            else
            {
                mInitialNumberValue = GetFloatWithPrecisionValue(mInitialNumberValue);
                return "Initial Value=" + mInitialNumberValue;
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
            Value = mInitialNumberValue;
        }
    }
}
