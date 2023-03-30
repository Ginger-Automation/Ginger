using Amdocs.Ginger.Common;
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

namespace Amdocs.Ginger.CoreNET
{
    public class ProcessInputVariableRule
    {
        private ObservableList<VariableBase> removedbfInputVariables;

        public BusinessFlow mBusinessFlow;

        public ProcessInputVariableRule(BusinessFlow businessFlow)
        {
            mBusinessFlow = businessFlow;
        }

        public void GetVariablesByRules(ObservableList<VariableBase> variables)
        {
            removedbfInputVariables = new ObservableList<VariableBase>();
            foreach (InputVariableRule variableRule in mBusinessFlow.InputVariableRules)
            {
                if (variableRule.Active)
                {
                    VariableBase sourceVariable = variables.Where(x => x.Guid == variableRule.SourceVariableGuid).FirstOrDefault();
                    VariableBase targetVariable = variables.Where(x => x.Guid == variableRule.TargetVariableGuid).FirstOrDefault();
                    //if (targetVariable == null)
                    //{
                    //    targetVariable = removedbfInputVariables.Where(x => x.Guid == variableRule.TargetVariableGuid).FirstOrDefault();
                    //    variables.Add(targetVariable);
                    //    removedbfInputVariables.Remove(targetVariable);                        
                    //}
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
                            targetVariable.Value = variableRule.OperationValue;
                        }

                        targetVariable.DiffrentFromOrigin = true;
                    }

                    else if (variableRule.OperationType == InputVariableRule.eInputVariableOperation.SetOptionalValues && CalculateOperatorStatus(sourceVariable, variableRule))
                    {
                        if (targetVariable.GetType() == typeof(VariableSelectionList))
                        {
                            ((VariableSelectionList)targetVariable).OptionalValuesList = new ObservableList<OptionalValue>();
                            foreach (OperationValues values in variableRule.OperationValueList)
                            {
                                ((VariableSelectionList)targetVariable).OptionalValuesList.Add(new OptionalValue(values.Value));
                            }
                            //targetVariable.Value = ((VariableSelectionList)targetVariable).OptionalValuesList[0].Value;
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
                            variables.Add(variable);
                            removedbfInputVariables.Remove(targetVariable);
                        }
                    }
                }
            }

            //foreach (InputVariableRule variableRule in mBusinessFlow.InputVariableRules.Where(x => x.OperationType == InputVariableRule.eInputVariableOperation.SetVisibility))
            //{
            //    VariableBase sourceVariable = variables.Where(x => x.Guid == variableRule.SourceVariableGuid).FirstOrDefault();
            //    VariableBase targetVariable = variables.Where(x => x.Guid == variableRule.TargetVariableGuid).FirstOrDefault();
            //}
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
                        status = sourceVariable.Value.Contains(variableRule.TriggerValue);
                        break;
                    case eInputVariableOperator.DoesNotContains:
                        status = !sourceVariable.Value.Contains(variableRule.TriggerValue);
                        break;
                    case eInputVariableOperator.Equals:
                        status = string.Equals(sourceVariable.Value, variableRule.TriggerValue);
                        break;
                    case eInputVariableOperator.Evaluate:
                        Expression = sourceVariable.Value;
                        break;
                    case eInputVariableOperator.GreaterThan:
                        if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                        {
                            status = false;
                        }
                        else
                        {
                            Expression = sourceVariable.Value + ">" + variableRule.TriggerValue;
                        }
                        break;
                    case eInputVariableOperator.GreaterThanEquals:
                        if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                        {
                            status = false;
                        }
                        else
                        {
                            Expression = sourceVariable.Value + ">=" + variableRule.TriggerValue;
                        }
                        break;
                    case eInputVariableOperator.LessThan:
                        if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                        {
                            status = false;
                        }
                        else
                        {
                            Expression = sourceVariable.Value + "<" + variableRule.TriggerValue;
                        }
                        break;
                    case eInputVariableOperator.LessThanEquals:
                        if (!CheckIfValuesCanbecompared(sourceVariable.Value, variableRule.TriggerValue))
                        {
                            status = false;
                        }
                        else
                        {
                            Expression = sourceVariable.Value + "<=" + variableRule.TriggerValue;
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
