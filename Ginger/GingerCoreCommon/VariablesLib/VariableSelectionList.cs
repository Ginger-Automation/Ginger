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
using System.Linq;
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

        public override eImageType Image { get { return eImageType.MinusSquare; } }

        public override string VariableType
        {
            get { return "Selection List"; }
        }

        [IsSerializedForLocalRepository(true)]
        public bool IsLoopEnabled { get; set; } = true;

        [IsSerializedForLocalRepository(false)]
        public bool IsDynamicValueModificationEnabled { get; set; } = false;


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

        private string mValue;
        [IsSerializedForLocalRepository]
        public override string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                OnPropertyChanged("Value");
            }
        }

        public override bool SetValue(string value)
        {
            if (OptionalValuesList.Where(pv => pv.Value == value).FirstOrDefault() != null)
            {
                Value = value;
                return true;
            }
            else if (IsDynamicValueModificationEnabled)
            {
                OptionalValuesList.Add(new OptionalValue(value));
                Value = OptionalValuesList[OptionalValuesList.Count - 1].Value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void DynamicDeleteValue(string value, ref string errorMsg)
        {
            if (IsDynamicValueModificationEnabled)
            {
                OptionalValue op;
                if (OptionalValuesList.Where(pv => pv.Value == value).FirstOrDefault() != null)
                {
                    op = OptionalValuesList.Where(pv => pv.Value == value).FirstOrDefault();
                }
                else
                {
                    errorMsg = "Could not found the value entered for deletion";
                    return;
                }

                OptionalValuesList.Remove(op);

                if (OptionalValuesList.Count == 0)
                {
                    Value = string.Empty;
                }
                else if (Value == op.Value)
                {
                    Value = OptionalValuesList[0].Value;
                }
            }
            else
            {
                errorMsg = "Dynamic value modification is not allowed";
            }
        }

        public void DeleteAllValues(ref string errorMsg)
        {
            if (IsDynamicValueModificationEnabled)
            {
                OptionalValuesList.ClearAll();
                if (OptionalValuesList.Count > 0)
                {
                    errorMsg = "Could not delete all optional values, please try again.";
                    return;
                }
                Value = string.Empty;
            }
            else
            {
                errorMsg = "Dynamic value modification is not allowed";
            }
        }

        public override void PostDeserialization()
        {
            //Note: we need to reset all variables postserialization except variableSelectionList, thats why empty overriden method. 
        }

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

        public override List<string> GetExtraParamsList() 
        {
            List<string> extraParamsDescription = new List<string>();
            extraParamsDescription.Add("Index=1");
            extraParamsDescription.Add("GetLength=True");
            extraParamsDescription.Add("IsContains=");
            return extraParamsDescription;
        }


        public override string GetValueWithParam(Dictionary<string, string> extraParamDict)
        {
            string param = string.Empty;
            int index;
            if (extraParamDict.Count == 0)
            {
                return Value;
            }
            foreach (KeyValuePair<string, string> keyValuePair in extraParamDict)
            {
                switch (keyValuePair.Key)
                {
                    case "Index":
                        {
                            bool isNumber = Int32.TryParse(keyValuePair.Value, out index);
                            if (isNumber && index > 0 && OptionalValuesList.Count >= index)
                            {
                                return OptionalValuesList[index - 1].Value;
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Error!! variable " + Name +": index is out of bounds");
                                return "Error!! variable index is out of bounds";
                            }
                        }
                    case "GetLength":
                        {
                            if (keyValuePair.Value == "True" || keyValuePair.Value == "true")
                            {
                                return OptionalValuesList.Count.ToString();
                            }
                            else
                            {
                                return Value;
                            }
                        }
                    case "IsContains":
                        {
                            if (OptionalValuesList.Where<OptionalValue>(x => x.Value == keyValuePair.Value).FirstOrDefault() != null)
                            {
                                return bool.TrueString;
                            }
                            else
                            {
                                return bool.FalseString;
                            }
                        }
                    default:
                        continue;
                }
            }

            return param;
        }



        public override void ResetValue()
        {
            if (OptionalValuesList.Count > 0)
            {
                Value = OptionalValuesList[0].Value;
            }

        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            if (OptionalValuesList.Count == 0)
            {
                Value = string.Empty;
                errorMsg = "Generate Auto Value is not possible because Selection List is empty";
                return false;
            }

            //Finding the index of the current Optional Value
            OptionalValue currentOptionalValue = OptionalValuesList.Where<OptionalValue>(op => op.Value == Value).FirstOrDefault();
            if (currentOptionalValue == null)
            {
                errorMsg = "Failed to generate auto value because current value is not part of the list of values";
                return false;
            }
            int index = OptionalValuesList.IndexOf(currentOptionalValue);

            if (index == -1)
            {
                errorMsg = "Failed to generate auto value because current value is not part of the list of values";
                return false;
            }

            //Check if the current OptionalValue is last
            if (index == OptionalValuesList.Count - 1)
            {
                //If loop chechbox is disabled so return a proper message.
                if (!IsLoopEnabled)
                {
                    errorMsg = "Generate Auto Value is not possible because current value is last and looping is not allowed";
                    return false;
                }
                else
                {
                    Value = OptionalValuesList[0].Value;
                    errorMsg = string.Empty;
                    return true;
                }
            }
            else
            {
                Value = OptionalValuesList[++index].Value;
                errorMsg = string.Empty;
                return true;
            }


        }

        public override bool SupportSetValue { get { return true; } }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.SetValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.AutoGenerateValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            if (IsDynamicValueModificationEnabled)
            {
                supportedOperations.Add(VariableBase.eSetValueOptions.DynamicValueDeletion);
                supportedOperations.Add(VariableBase.eSetValueOptions.DeleteAllValues);
            }
            return supportedOperations;
        }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return true; } }

        public override void SetInitialSetup()
        {
            base.SetInitialSetup();

            OptionalValue newVal1 = new OptionalValue("Value1");
            OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Value2");
            OptionalValuesList.Add(newVal2);
        }
    }
}
