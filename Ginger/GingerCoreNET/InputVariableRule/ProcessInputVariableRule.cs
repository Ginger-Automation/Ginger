﻿using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ginger.Variables;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.VariablesLib;
using static Ginger.Variables.InputVariableRule;
using GingerCoreNET.RosLynLib;
using Ginger.Run;
using amdocs.ginger.GingerCoreNET;

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
            removedbfInputVariables = new ObservableList<VariableBase>();
            foreach (InputVariableRule variableRule in mBusinessFlow.InputVariableRules)
            {
                try
                {
                    if (variableRule.Active)
                    {
                        VariableBase sourceVariable = variables.Where(x => x.Guid == variableRule.SourceVariableGuid).FirstOrDefault();
                        VariableBase targetVariable = variables.Where(x => x.Guid == variableRule.TargetVariableGuid).FirstOrDefault();
                        string originalFormula = targetVariable.Formula;
                        string originalValue = targetVariable.Value;
                        if (sourceVariable !=null && targetVariable!=null)
                        {
                            if (variableRule.OperationType == InputVariableRule.eInputVariableOperation.SetValue && CalculateOperatorStatus(sourceVariable, variableRule))
                            {
                                if (targetVariable.GetType() == typeof(VariableSelectionList))
                                {
                                    OptionalValue optionalValue = ((VariableSelectionList)targetVariable).OptionalValuesList.Where(x => x.Value == variableRule.OperationValue).FirstOrDefault();
                                    if (optionalValue !=null)
                                    {
                                        targetVariable.Value = variableRule.OperationValue;
                                    }
                                }
                                else
                                {
                                    if(targetVariable.GetType() == typeof(VariableString))
                                    {
                                       ((VariableString)targetVariable).InitialStringValue = variableRule.OperationValue;
                                    }
                                    else if(targetVariable.GetType() == typeof(VariableNumber))
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
                                        ((VariableSelectionList)targetVariable).OptionalValuesList = new ObservableList<OptionalValue>();
                                        foreach (OperationValues values in variableRule.OperationValueList)
                                        {
                                            ((VariableSelectionList)targetVariable).OptionalValuesList.Add(new OptionalValue(values.Value));
                                        }
                                        if (((VariableSelectionList)targetVariable).OptionalValuesList != null && ((VariableSelectionList)targetVariable).OptionalValuesList.Count > 0)
                                        {
                                           OptionalValue op = ((VariableSelectionList)targetVariable).OptionalValuesList.Where(x => x.Value == ((VariableSelectionList)targetVariable).Value).FirstOrDefault();
                                            if(op == null)
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
                                    if (removedbfInputVariables.Where(x => x.Guid != targetVariable.Guid).FirstOrDefault() == null)
                                    {
                                        removedbfInputVariables.Add(targetVariable);
                                    }
                                }
                                else if (variableRule.OperationValue == eVisibilityOptions.Show.ToString())
                                {
                                    VariableBase variable = removedbfInputVariables.Where(x => x.Guid == variableRule.TargetVariableGuid).FirstOrDefault();
                                    if(variable != null)
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

            if (sourceVariable !=null)
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
                            status =   ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
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
                            status =  ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
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
                           status =  ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
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
                            status =  ComparerDateTime(sourceVariable.Value, variableRule.TriggerValue, ((VariableDateTime)sourceVariable).DateTimeFormat, variableRule.Operator);
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

        private bool ComparerDateTime(string variableValue, string triggerValue,string dateFormat, eInputVariableOperator voperator)
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