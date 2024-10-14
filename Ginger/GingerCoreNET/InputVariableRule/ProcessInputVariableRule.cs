#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.VariablesLib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.Variables;
using GingerCore;
using GingerCore.Variables;
using GingerCoreNET.RosLynLib;
using System;
using System.Linq;
using static Ginger.Variables.InputVariableRule;

namespace Amdocs.Ginger.CoreNET
{
    public class ProcessInputVariableRule
    {
        private ObservableList<VariableBase> removedbfInputVariables;

        public BusinessFlow mBusinessFlow;
        public GingerRunner mGingerRunner;

        public ProcessInputVariableRule(BusinessFlow businessFlow, GingerRunner gingerRunner)
        {
            mBusinessFlow = businessFlow;
            mGingerRunner = gingerRunner;
        }

        public void GetVariablesByRules(ObservableList<VariableBase> variables)
        {
            removedbfInputVariables = [];
            foreach (InputVariableRule variableRule in mBusinessFlow.InputVariableRules)
            {
                try
                {
                    if (variableRule.Active)
                    {
                        VariableBase sourceVariable = variables.FirstOrDefault(x => x.Guid == variableRule.SourceVariableGuid);
                        VariableBase targetVariable = variables.FirstOrDefault(x => x.Guid == variableRule.TargetVariableGuid);
                        string originalFormula = targetVariable.Formula;
                        string originalValue = targetVariable.Value;
                        if (sourceVariable != null && targetVariable != null)
                        {
                            if (variableRule.OperationType == InputVariableRule.eInputVariableOperation.SetValue && CalculateOperatorStatus(sourceVariable, variableRule))
                            {
                                if (targetVariable.GetType() == typeof(VariableSelectionList))
                                {
                                    OptionalValue optionalValue = ((VariableSelectionList)targetVariable).OptionalValuesList.FirstOrDefault(x => x.Value == variableRule.OperationValue);
                                    if (optionalValue != null)
                                    {
                                        targetVariable.Value = variableRule.OperationValue;
                                    }
                                }
                                else
                                {
                                    if (targetVariable.GetType() == typeof(VariableString))
                                    {
                                        ((VariableString)targetVariable).InitialStringValue = variableRule.OperationValue;
                                    }
                                    else if (targetVariable.GetType() == typeof(VariableNumber))
                                    {
                                        ((VariableNumber)targetVariable).InitialNumberValue = variableRule.OperationValue;
                                    }
                                    else if (targetVariable.GetType() == typeof(VariableDateTime))
                                    {
                                        ((VariableDateTime)targetVariable).InitialDateTime = variableRule.OperationValue;
                                    }
                                }
                            }

                            else if (variableRule.OperationType == InputVariableRule.eInputVariableOperation.SetOptionalValues)
                            {
                                if (CalculateOperatorStatus(sourceVariable, variableRule))
                                {
                                    if (targetVariable.GetType() == typeof(VariableSelectionList))
                                    {
                                        ((VariableSelectionList)targetVariable).OptionalValuesList = [];
                                        foreach (OperationValues values in variableRule.OperationValueList)
                                        {
                                            ((VariableSelectionList)targetVariable).OptionalValuesList.Add(new OptionalValue(values.Value));
                                        }
                                        if (((VariableSelectionList)targetVariable).OptionalValuesList != null && ((VariableSelectionList)targetVariable).OptionalValuesList.Count > 0)
                                        {
                                            OptionalValue op = ((VariableSelectionList)targetVariable).OptionalValuesList.FirstOrDefault(x => x.Value == ((VariableSelectionList)targetVariable).Value);
                                            if (op == null)
                                            {
                                                ((VariableSelectionList)targetVariable).Value = ((VariableSelectionList)targetVariable).OptionalValuesList[0].Value;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (variableRule.Active && variableRule.OperationType == InputVariableRule.eInputVariableOperation.SetVisibility && CalculateOperatorStatus(sourceVariable, variableRule))
                            {
                                if (variableRule.OperationValue == eVisibilityOptions.Hide.ToString())
                                {
                                    variables.Remove(targetVariable);
                                    if (removedbfInputVariables.FirstOrDefault(x => x.Guid != targetVariable.Guid) == null)
                                    {
                                        removedbfInputVariables.Add(targetVariable);
                                    }
                                }
                                else if (variableRule.OperationValue == eVisibilityOptions.Show.ToString())
                                {
                                    VariableBase variable = removedbfInputVariables.FirstOrDefault(x => x.Guid == variableRule.TargetVariableGuid);
                                    if (variable != null)
                                    {
                                        variables.Add(variable);
                                        removedbfInputVariables.Remove(targetVariable);
                                    }
                                }
                            }
                        }

                        if (targetVariable.Formula != originalFormula || targetVariable.Value != originalValue)//variable was changed
                        {
                            targetVariable.VarValChanged = true;
                            targetVariable.DiffrentFromOrigin = true;
                            //TODO : Raise event to mark runset dirty status as modified. 
                            mGingerRunner.Executor.UpdateBusinessFlowsRunList();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to process rule"), ex);
                }
            }
        }

        private bool CalculateOperatorStatus(VariableBase sourceVariable, InputVariableRule variableRule)
        {
            bool? status = null;

            string Expression = string.Empty;

            if (sourceVariable != null)
            {
                switch (variableRule.Operator)
                {
                    case eInputVariableOperator.Contains:
                        if (sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            status = sourceVariable.Value.Contains(variableRule.TriggerValue);
                        }
                        break;
                    case eInputVariableOperator.DoesNotContains:
                        if (sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            status = !sourceVariable.Value.Contains(variableRule.TriggerValue);
                        }
                        break;
                    case eInputVariableOperator.Equals:
                        status = string.Equals(sourceVariable.Value, variableRule.TriggerValue);
                        break;
                    case eInputVariableOperator.Evaluate:
                        Expression = sourceVariable.Value;
                        break;
                    case eInputVariableOperator.GreaterThan:
                        if (sourceVariable.GetType() == typeof(VariableNumber) || sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                            {
                                status = false;
                            }
                            else
                            {
                                Expression = sourceVariable.Value + ">" + variableRule.TriggerValue;
                            }
                        }
                        else if (sourceVariable.GetType() == typeof(VariableDateTime))
                        {
                            status = ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
                        }

                        break;
                    case eInputVariableOperator.GreaterThanEquals:
                        if (sourceVariable.GetType() == typeof(VariableNumber) || sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                            {
                                status = false;
                            }
                            else
                            {
                                Expression = sourceVariable.Value + ">=" + variableRule.TriggerValue;
                            }
                        }
                        else if (sourceVariable.GetType() == typeof(VariableDateTime))
                        {
                            status = ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
                        }
                        break;
                    case eInputVariableOperator.LessThan:
                        if (sourceVariable.GetType() == typeof(VariableNumber) || sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                            {
                                status = false;
                            }
                            else
                            {
                                Expression = sourceVariable.Value + "<" + variableRule.TriggerValue;
                            }
                        }
                        else if (sourceVariable.GetType() == typeof(VariableDateTime))
                        {
                            status = ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
                        }
                        break;
                    case eInputVariableOperator.LessThanEquals:
                        if (sourceVariable.GetType() == typeof(VariableNumber) || sourceVariable.GetType() == typeof(VariableSelectionList) || sourceVariable.GetType() == typeof(VariableString))
                        {
                            if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                            {
                                status = false;
                            }
                            else
                            {
                                Expression = sourceVariable.Value + "<=" + variableRule.TriggerValue;
                            }
                        }
                        else if (sourceVariable.GetType() == typeof(VariableDateTime))
                        {
                            status = ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
                        }
                        break;
                    case eInputVariableOperator.NotEquals:
                        status = !string.Equals(sourceVariable.Value, variableRule.TriggerValue);
                        break;
                    default:
                        status = false;
                        break;
                }
                if (status == null)
                {
                    status = CodeProcessor.EvalCondition(Expression);
                }
            }

            return Convert.ToBoolean(status);
        }

        private bool ComparerDateTime(string variableValue, string triggerValue, string dateFormat, eInputVariableOperator voperator)
        {
            bool status = false;
            try
            {
                if (!string.IsNullOrEmpty(variableValue) && !string.IsNullOrEmpty(triggerValue) && !string.IsNullOrEmpty(dateFormat))
                {
                    DateTime dtVariable = DateTime.Parse(Convert.ToDateTime(variableValue).ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture));

                    DateTime triggerDate = DateTime.Parse(Convert.ToDateTime(triggerValue).ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture));

                    switch (voperator)
                    {
                        case eInputVariableOperator.GreaterThan:
                            if (dtVariable > triggerDate)
                            {
                                status = true;
                            }
                            break;
                        case eInputVariableOperator.GreaterThanEquals:
                            if (dtVariable >= triggerDate)
                            {
                                status = true;
                            }
                            break;
                        case eInputVariableOperator.LessThan:
                            if (dtVariable < triggerDate)
                            {
                                status = true;
                            }
                            break;
                        case eInputVariableOperator.LessThanEquals:
                            if (dtVariable <= triggerDate)
                            {
                                status = true;
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                status = false;
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to compare datetime"), ex);
            }
            return status;
        }

        private static bool CheckIfValuesCanbecompared(string actual, string Expected)
        {
            try
            {
                double.Parse(actual);
                double.Parse(actual);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
