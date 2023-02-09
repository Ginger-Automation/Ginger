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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions
{
    public class ActSetVariableValue : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Action"; } }
        public override string ActionUserDescription { get { return "Allows to set the value of a " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " in run time"; } }

        private bool isAutoGenerateValuesucceed;
        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("1- Select the " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " to modify it value");
            TBH.AddLineBreak();
            TBH.AddText("2- Select the type of operation which required, the options are Reset Value\\Generate Auto Value\\Set Value");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("In case 'Set Value' operation is selected, user is able to configure the value for the " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " (String and List variables type only) using value expression.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("For List type " + GingerDicser.GetTermResValue(eTermResKey.Variables) + ", the value to set must be one of the " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " possible values.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("The 'Value' field is relevant only for 'SetValue' operation.");
        }


        public override string ActionEditPage { get { return "ActSetVariableValuePage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override string ActionType
        {
            get { return "Set " + GingerDicser.GetTermResValue(eTermResKey.Variable); }
        }

        public string VariableName
        {
            get
            {
                return GetOrCreateInputParam(nameof(VariableName)).Value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(VariableName), value);
            }
        }


        public VariableBase.eSetValueOptions SetVariableValueOption
        {
            get
            {
                return (VariableBase.eSetValueOptions)GetOrCreateInputParam<VariableBase.eSetValueOptions>(nameof(SetVariableValueOption), VariableBase.eSetValueOptions.SetValue);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetVariableValueOption), value.ToString());
            }
        }

        public override void Execute()
        {
            VariableBase Var = RunOnBusinessFlow.GetHierarchyVariableByName(VariableName);
            if (Var == null)
            {
                Error = GingerDicser.GetTermResValue(eTermResKey.Variable) + " was not found.";
                return;
            }
            string calculatedValue = string.Empty;
            if (ValueExpression != null)
            {
                ValueExpression.DecryptFlag = false;
                calculatedValue = ValueExpression.Calculate(this.Value);
            }
            else
            {
                calculatedValue = this.Value;
            }

            if (SetVariableValueOption == VariableBase.eSetValueOptions.SetValue)
            {
                if (Var.GetType() == typeof(VariableString))
                {
                    ((VariableString)Var).Value = calculatedValue;
                }
                else if (Var.GetType() == typeof(VariableSelectionList))
                {

                    bool isSetValue = Var.SetValue(calculatedValue);
                    if (!isSetValue)
                    {
                        Error = "The value '" + calculatedValue + "' is not part of the possible values of the '" + Var.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".";
                        return;
                    }
                }
                else if (Var.GetType() == typeof(VariableList))
                {
                    string[] possibleVals = ((VariableList)Var).Formula.Split(',');
                    if (possibleVals != null && possibleVals.Contains(calculatedValue))
                    {
                        ((VariableList)Var).Value = calculatedValue;
                    }
                    else
                    {
                        Error = "The value '" + calculatedValue + "' is not part of the possible values of the '" + Var.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".";
                        return;
                    }
                }
                else if (Var.GetType() == typeof(VariableDynamic))
                {
                    ((VariableDynamic)Var).ValueExpression = this.Value;
                }
                else if (Var.GetType() == typeof(VariableNumber))
                {
                    try
                    {
                        var varNumber = ((VariableNumber)Var);
                        if (varNumber.CheckNumberInRange(float.Parse(calculatedValue)))
                        {
                            varNumber.Value = calculatedValue;
                        }
                        else
                        {
                            Error = $"The value {calculatedValue} is not in the range, {Var.Name}:-[Min value: {varNumber.MinValue}, Max value: {varNumber.MaxValue}]   {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                            return;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Error = $"Error occurred during SetValue for Variable number type..:- {ex.Message}";
                        return;
                    }

                }
                else if (Var.GetType() == typeof(VariableDateTime))
                {
                    var varDateTime = ((VariableDateTime)Var);
                    try
                    {
                        if (!varDateTime.IsValidDateTimeFormat(calculatedValue))
                        {
                            Error = $"The Value [{calculatedValue}] is not in correct format, expected format is [{varDateTime.DateTimeFormat}], {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                            return;
                        }

                        if (varDateTime.CheckDateTimeWithInRange(calculatedValue))
                        {
                            varDateTime.Value = calculatedValue;
                        }
                        else
                        {
                            Error = $"The value {calculatedValue} is not in the date range {Var.Name}:-[Min value:{varDateTime.MinDateTime}, Max value:{varDateTime.MaxDateTime}] {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Error = $"Invalid DateTimeFormat,{Var.Name}:-[DateTimeFormat:{varDateTime.DateTimeFormat}] :- {ex.Message}";
                        return;
                    }
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.ResetValue)
            {
                (Var).ResetValue();
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.ClearSpecialChar)
            {
                string specChar = ValueExpression.Calculate(this.Value);
                if (string.IsNullOrEmpty(specChar))
                {
                    specChar = @"{}(),\""";
                }
                if (!string.IsNullOrEmpty(((VariableString)Var).Value))
                {
                    foreach (char c in specChar)
                    {
                        ((VariableString)Var).Value = ((VariableString)Var).Value.Replace(c.ToString(), "");
                    }
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.AutoGenerateValue)
            {
                string errorMsg = String.Empty;
                isAutoGenerateValuesucceed = ((VariableBase)Var).GenerateAutoValue(ref errorMsg);
                if (!isAutoGenerateValuesucceed)
                {
                    Error = errorMsg;
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.StartTimer)
            {
                if (Var.GetType() == typeof(VariableTimer))
                {
                    ((VariableTimer)Var).StartTimer();
                }
                else
                {
                    Error = "Operation type " + SetVariableValueOption + " is not supported for " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " of type " + Var.GetType();
                    return;
                }
            }

            else if (SetVariableValueOption == VariableBase.eSetValueOptions.StopTimer)
            {
                if (Var.GetType() == typeof(VariableTimer))
                {
                    ((VariableTimer)Var).StopTimer();
                }
                else
                {
                    Error = "Operation type " + SetVariableValueOption + " is not supported for " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " of type " + Var.GetType();

                    return;
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.ContinueTimer)
            {
                if (Var.GetType() == typeof(VariableTimer))
                {
                    ((VariableTimer)Var).ContinueTimer();
                }
                else
                {
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();
                    return;
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.DynamicValueDeletion)
            {
                if (Var.GetType() == typeof(VariableSelectionList))
                {
                    string errorMsg = String.Empty;
                    ((VariableSelectionList)Var).DynamicDeleteValue(calculatedValue, ref errorMsg);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Error = errorMsg;
                    }
                }
                else
                {
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();
                    return;
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.DeleteAllValues)
            {
                if (Var.GetType() == typeof(VariableSelectionList))
                {
                    string errorMsg = String.Empty;
                    ((VariableSelectionList)Var).DeleteAllValues(ref errorMsg);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        Error = errorMsg;
                    }
                    else
                    {
                        calculatedValue = string.Empty;
                    }
                }
                else
                {
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();
                    return;
                }
            }
            else
            {
                Error = "Unknown set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value operation.";
                return;
            }

            ExInfo = GingerDicser.GetTermResValue(eTermResKey.Variable) + " '" + Var.Name + "' value was set to: '" + Var.Value + "'";
        }
    }
}
