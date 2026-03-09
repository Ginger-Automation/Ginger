#region License
/*
Copyright © 2014-2026 European Support Limited

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
        private bool mIsLoopEnabled = true;
        [IsSerializedForLocalRepository(true)]
        public bool IsLoopEnabled
        {
            get { return mIsLoopEnabled; }
            set
            {
                if (mIsLoopEnabled != value)
                {
                    mIsLoopEnabled = value;
                    OnPropertyChanged(nameof(IsLoopEnabled));
                }
            }
        }

        private bool mIsDynamicValueModificationEnabled;

        [IsSerializedForLocalRepository(false)]
        public bool IsDynamicValueModificationEnabled
        {
            get { return mIsDynamicValueModificationEnabled; }
            set
            {
                if (mIsDynamicValueModificationEnabled != value)
                {
                    mIsDynamicValueModificationEnabled = value;
                    OnPropertyChanged(nameof(IsDynamicValueModificationEnabled));
                }
            }
        }

        //DO NOT REMOVE! Used for conversion of old OptionalValues which were kept in one string with delimiter
        public string OptionalValues
        {
            set
            {
                OptionalValuesList = ConvertOptionalValuesStringToList(value);
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<OptionalValue> OptionalValuesList = [];

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
            if (OptionalValuesList.FirstOrDefault(pv => pv.Value == value) != null)
            {
                Value = value;
                return true;
            }
            else if (IsDynamicValueModificationEnabled)
            {
                OptionalValuesList.Add(new OptionalValue(value));
                Value = OptionalValuesList[^1].Value;
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
                if (OptionalValuesList.FirstOrDefault(pv => pv.Value == value) != null)
                {
                    op = OptionalValuesList.FirstOrDefault(pv => pv.Value == value);
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
                ObservableList<OptionalValue> valsObservList = [];
                foreach (string val in valsList)
                    valsObservList.Add(new OptionalValue(val));
                return valsObservList;
            }
            catch
            {
                Reporter.ToLog(eLogLevel.ERROR, "Cannot Convert Optional Values String To List - " + valsString);
                return [];
            }
        }

        public override List<string> GetExtraParamsList()
        {
            List<string> extraParamsDescription =
            [
                "Index=1",
                "GetLength=True",
                "IsContains=",
            ];
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
                                Reporter.ToLog(eLogLevel.ERROR, "Error!! variable " + Name + ": index is out of bounds");
                                return "Error!! variable index is out of bounds";
                            }
                        }
                    case "GetLength":
                        {
                            if (keyValuePair.Value is "True" or "true")
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
                            if (OptionalValuesList.FirstOrDefault(x => x.Value == keyValuePair.Value) != null)
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
            OptionalValue currentOptionalValue = OptionalValuesList.FirstOrDefault(op => op.Value == Value);
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
            List<VariableBase.eSetValueOptions> supportedOperations =
            [
                VariableBase.eSetValueOptions.SetValue,
                VariableBase.eSetValueOptions.AutoGenerateValue,
                VariableBase.eSetValueOptions.ResetValue,
            ];
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

        public override void SetInitialValue(string InitialValue)
        {
            SetValue(InitialValue);
        }
    }
}
