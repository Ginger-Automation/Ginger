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
using System.Text;
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

        public override void GenerateAutoValue()
        {
           //NA
        }

        [IsSerializedForLocalRepository]
        public string InitialDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(mInitialDateTime))
                {
                    var currentDate = DateTime.Today.Date;
                    return currentDate.ToString("dd-MMM-yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                }
                return mInitialDateTime;
            }
            set
            {
                mInitialDateTime = value;
                Value = mInitialDateTime;
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
                   return DateTime.Now.AddYears(-1).ToString();
                }
                return mMinDateTime;
            }
            set
            {
                mMinDateTime = value;
                OnPropertyChanged("MinDateTime");
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
                    return DateTime.Now.AddYears(1).ToString();
                }
                return mMaxDateTime;
            }
            set
            {
                mMaxDateTime = value;
                OnPropertyChanged("MaxDateTime");
                OnPropertyChanged("Formula");
            }
        }


        [IsSerializedForLocalRepository]
        public string DateTimeFormat
        {
            get
            {
                if(string.IsNullOrEmpty(mDateTimeFormat))
                {
                    return @"dd-MMM-yyyy hh:mm:ss tt";
                }
                return mDateTimeFormat;
            }
            set
            {
                mDateTimeFormat = value;
                OnPropertyChanged("DateTimeFormat");
                OnPropertyChanged("InitialDateTime");
            }
        }

        public override string GetFormula()
        {
            mInitialDateTime = Convert.ToDateTime(mInitialDateTime).ToString(DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
            Value = mInitialDateTime;
            return "Initial DateTime : "+ mInitialDateTime;
        }

        
        public override List<eSetValueOptions> GetSupportedOperations()
        {
            var supportedOperations = new List<eSetValueOptions>();
            supportedOperations.Add(eSetValueOptions.SetValue);
            supportedOperations.Add(eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override void ResetValue()
        {
            Value = mInitialDateTime;
        }
    }
}
