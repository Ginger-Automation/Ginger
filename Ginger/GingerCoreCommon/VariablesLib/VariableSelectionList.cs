#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableSelectionList : VariableBase
    {
        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Selection List"; }
        }

        public override string VariableEditPage { get { return "VariableSelectionListPage"; } }

        public override eImageType Image { get { return eImageType.VariableList; } }

        public override string VariableType() { return "Selection List"; }

        //DO NOT REMOVE! Used for conversion of old OptionalValues which were kept in one string with delimiter
        public string OptionalValues
        {
            set
            {
                OptionalValuesList = ConvertOptionalValuesStringToList(value);
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesList = new ObservableList<OptionalValue>();

        public string SelectedValue { set { Value = value; OnPropertyChanged(nameof(SelectedValue)); } get { return Value; } }

        public override string GetFormula()
        {
            string formula = "Options: ";
            foreach (OptionalValue val in OptionalValuesList)
            {
                formula += val.Value + ",";
            }
            formula = formula.TrimEnd(',');
            return formula;
        }

        // Support backward compatibility - function when we had the list with delimiter
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
                Reporter.ToLog(eLogLevel.ERROR, "Cannot Convert Optional Values String To List - " + valsString);
                return new ObservableList<OptionalValue>();
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

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.SetValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            return supportedOperations;
        }

    }
}
