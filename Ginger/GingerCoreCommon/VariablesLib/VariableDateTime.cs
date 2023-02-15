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
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableDateTime : VariableBase
    {
        private string mDateTimeFormat;
        private string mInitialDateTime;
        private string mMinDateTime;
        private string mMaxDateTime;

        public override string VariableType
        {
            get { return "DateTime"; }
        }

        public override string VariableEditPage
        {
            get { return "VariableDateTimePage"; }
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
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " DateTime"; }
        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            //NA
            errorMsg = "Generate Auto Value is not supported";
            return false;
        }

        [IsSerializedForLocalRepository]
        public string InitialDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(mInitialDateTime))
                {
                    var currentDate = DateTime.Today.Date;
                    return currentDate.ToString("MM/dd/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                }
                return ConvertDateTimeToSpecificFormat(mDateTimeFormat, mInitialDateTime);
            }
            set
            {
                if (mMinDateTime != null && mMaxDateTime != null && !CheckDateTimeWithInRange(value))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Initial DateTime[{value}] is not in Range:- Min.DateTime [{MinDateTime}], Max.DateTime [{MaxDateTime}]");
                }
                else
                {
                    mInitialDateTime = value;
                    Value = ConvertDateTimeToSpecificFormat(mDateTimeFormat, mInitialDateTime);
                }

                OnPropertyChanged("InitialDateTime");
                OnPropertyChanged("Formula");
            }
        }

        [IsSerializedForLocalRepository]
        public string MinDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(mMinDateTime))
                {
                    return DateTime.Now.AddYears(-1).ToString(mDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                }
                return ConvertDateTimeToSpecificFormat(mDateTimeFormat, mMinDateTime);
            }
            set
            {
                mMinDateTime = value;
                OnPropertyChanged("MinDateTime");
                OnPropertyChanged("InitialDateTime");
                OnPropertyChanged("Formula");
            }
        }

        [IsSerializedForLocalRepository]
        public string MaxDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(mMaxDateTime))
                {
                    return DateTime.Now.AddYears(1).ToString(mDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                }
                return ConvertDateTimeToSpecificFormat(mDateTimeFormat, mMaxDateTime);
            }
            set
            {
                mMaxDateTime = value;
                OnPropertyChanged("MaxDateTime");
                OnPropertyChanged("InitialDateTime");
                OnPropertyChanged("Formula");
            }
        }


        [IsSerializedForLocalRepository]
        public string DateTimeFormat
        {
            get
            {
                if (string.IsNullOrEmpty(mDateTimeFormat))
                {
                    return @"MM/dd/yyyy hh:mm:ss tt";
                }
                return mDateTimeFormat;
            }
            set
            {
                mDateTimeFormat = value;

                if (mInitialDateTime != null)
                {
                    InitialDateTime = ConvertDateTimeToSpecificFormat(mDateTimeFormat, mInitialDateTime);
                }
                OnPropertyChanged("DateTimeFormat");
                OnPropertyChanged("InitialDateTime");
                OnPropertyChanged("Formula");
            }
        }

        public override string GetFormula()
        {
            return "Initial DateTime : " + ConvertDateTimeToSpecificFormat(mDateTimeFormat, mInitialDateTime);
        }



        public override List<eSetValueOptions> GetSupportedOperations()
        {
            var supportedOperations = new List<eSetValueOptions>();
            supportedOperations.Add(eSetValueOptions.SetValue);
            supportedOperations.Add(eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override eImageType Image
        {
            get { return eImageType.DatePicker; }
        }

        public override bool SetValue(string value)
        {
            try
            {
                if (IsValidDateTimeFormat(value) && CheckDateTimeWithInRange(value))
                {
                    InitialDateTime = value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }

        public override void ResetValue()
        {
            Value = mInitialDateTime;
        }

        public string ConvertDateTimeToSpecificFormat(string format, string datetimeToFormat = "")
        {
            if (!string.IsNullOrEmpty(datetimeToFormat))
            {
                return Convert.ToDateTime(datetimeToFormat).ToString(format, System.Globalization.CultureInfo.InvariantCulture);
            }
            return Convert.ToDateTime(this.mInitialDateTime).ToString(format, System.Globalization.CultureInfo.InvariantCulture);
        }

        public bool CheckDateTimeWithInRange(string dateTimeValue)
        {
            if (DateTime.Parse(ConvertDateTimeToSpecificFormat(DateTimeFormat, dateTimeValue)) >= DateTime.Parse(ConvertDateTimeToSpecificFormat(DateTimeFormat, MinDateTime)) &&
                DateTime.Parse(ConvertDateTimeToSpecificFormat(DateTimeFormat, dateTimeValue)) <= DateTime.Parse(ConvertDateTimeToSpecificFormat(DateTimeFormat, MaxDateTime)))
            {
                return true;
            }
            return false;
        }
        public bool IsValidDateTimeFormat(string value)
        {
            string[] formats = { this.DateTimeFormat };
            if (DateTime.TryParseExact(value, formats, System.Globalization.CultureInfo.InvariantCulture,
                                      System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
