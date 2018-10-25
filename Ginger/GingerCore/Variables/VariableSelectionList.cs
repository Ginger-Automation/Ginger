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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Variables
{
    public class VariableSelectionList : VariableBase
    {
        public new static partial class Fields
        {
            public static string OptionalValues = "OptionalValues";
            public static string SelectedValue = "SelectedValue";
        }
        
        public VariableSelectionList()
        {
            mOptionalValues = string.Empty;
            mOptionalValuesList = new ObservableList<OptionalValue>();
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Selection List"; }
        }

        public override string VariableEditPage { get { return "VariableSelectionListPage"; } }

        public override eImageType Image { get { return eImageType.List; } }

        public override string VariableType() { return "Selection List"; }

        private string mOptionalValues;
        [IsSerializedForLocalRepository]
        public string OptionalValues
        {
            get
            {
                return mOptionalValues;
            }

            set
            {
                if (value != mOptionalValues)
                {
                    mOptionalValues = value;
                    if (value != ConvertOptionalValuesListToString(OptionalValuesList))//sync with List
                    {
                        OptionalValuesList = ConvertOptionalValuesStringToList(value);
                        OptionalValuesChanged();
                    }
                }
            }
        }

        ObservableList<OptionalValue> mOptionalValuesList;
        public ObservableList<OptionalValue> OptionalValuesList
        {
            get
            {
                return mOptionalValuesList;
            }
            set
            {
                mOptionalValuesList = value;
                if (ConvertOptionalValuesListToString(value) != OptionalValues) //sync with string
                {                    
                    mOptionalValues = ConvertOptionalValuesListToString(value);
                    OptionalValuesChanged();
                }
            }
        }

        public string SelectedValue { set { Value = value; OnPropertyChanged("SelectedValue"); } get { return Value; } }


        public override string GetFormula()
        {
            string form = "Options: ";
            foreach (OptionalValue val in OptionalValuesList)
                form += val.Value + ",";
            form = form.TrimEnd(',');
            return form;
        }
 
        private ObservableList<OptionalValue> ConvertOptionalValuesStringToList(string valsString)
        {
            try
            {
                List<string> valsList = (new List<string>(valsString.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)));
                ObservableList<OptionalValue> valsObservList = new ObservableList<OptionalValue>();
                foreach (string val in valsList)
                    valsObservList.Add(new OptionalValue(val));
                return valsObservList;
            }
            catch
            {
                return new ObservableList<OptionalValue>();
            }
        }

        private string ConvertOptionalValuesListToString(ObservableList<OptionalValue> ValsList)
        {
            try
            {
                string valsString = string.Empty;
                foreach (OptionalValue val in ValsList)
                    valsString += val.Value + "\r\n";
                valsString= valsString.TrimEnd(new char[] { '\r', '\n' });
                return valsString;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void OptionalValuesChanged()
        {
            OnPropertyChanged("Formula");

            //make sure the selected value is valid
            if (OptionalValuesList != null && OptionalValuesList.Count > 0)
            {
                if (SelectedValue == string.Empty
                    || OptionalValuesList.Where(v => v.Value == SelectedValue).FirstOrDefault() == null)
                    SelectedValue = OptionalValuesList[0].Value;
            }
            else
                SelectedValue = string.Empty;
        }

        public void SyncOptionalValuesListAndString()
        {
            if (ConvertOptionalValuesListToString(mOptionalValuesList) != OptionalValues)
            {
                mOptionalValues = ConvertOptionalValuesListToString(mOptionalValuesList);
                OptionalValuesChanged();
            }
        }

        public override void ResetValue()
        {
            if (OptionalValuesList.Count > 0)
                Value = OptionalValuesList[0].Value;
        }

        public override void GenerateAutoValue()
        {
            //NA
        }

        public override bool SupportSetValue { get { return true; } }

        public override List<ActSetVariableValue.eSetValueOptions> GetSupportedOperations()
        {
            List<ActSetVariableValue.eSetValueOptions> supportedOperations = new List<ActSetVariableValue.eSetValueOptions>();
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.SetValue);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.ResetValue);
            return supportedOperations;
        }
    }
}
