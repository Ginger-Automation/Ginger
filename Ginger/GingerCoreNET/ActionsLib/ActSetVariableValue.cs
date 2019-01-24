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

using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Linq;

namespace GingerCore.Actions
{
    public class ActSetVariableValue : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Action"; } }
        public override string ActionUserDescription { get { return "Allows to set the value of a " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " in run time"; } }

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
            get { return "Set Variable"; }
        }

        [IsSerializedForLocalRepository]
        public string VariableName { set; get; }


        [IsSerializedForLocalRepository]
        public VariableBase.eSetValueOptions SetVariableValueOption { get; set; }
               
        public override void Execute()
        {
            VariableBase Var = RunOnBusinessFlow.GetHierarchyVariableByName(VariableName);
            if (Var == null)
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = GingerDicser.GetTermResValue(eTermResKey.Variable) + " was not found.";
                return;
            }

            string calculatedValue = ValueExpression.Calculate(this.Value);
            if (SetVariableValueOption == VariableBase.eSetValueOptions.SetValue)
            {                
                if (Var.GetType() == typeof(VariableString))
                {
                    ((VariableString)Var).Value = calculatedValue;
                }
                else if (Var.GetType() == typeof(VariableSelectionList))
                {                     
                    if (((VariableSelectionList)Var).OptionalValuesList.Where(pv => pv.Value == calculatedValue).SingleOrDefault() != null)
                    {
                        ((VariableSelectionList)Var).Value = calculatedValue;
                    }
                    else
                    {
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
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
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        Error = "The value '" + calculatedValue + "' is not part of the possible values of the '" + Var.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".";
                        return;
                    }
                }
                else if (Var.GetType() == typeof(VariableDynamic))
                {
                    ((VariableDynamic)Var).ValueExpression = this.Value;
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.ResetValue)
            {                  
                    (Var).ResetValue();
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.ClearSpecialChar)
            {
                string specChar = ValueExpression.Calculate(this.Value);
                if(string.IsNullOrEmpty(specChar))
                {
                    specChar = @"{}(),\""";
                }
                if(!string.IsNullOrEmpty(((VariableString)Var).Value))
                {
                    foreach (char c in specChar)
                    {
                        ((VariableString)Var).Value = ((VariableString)Var).Value.Replace(c.ToString(), "");
                    }
                }
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.AutoGenerateValue)
            {
                ((VariableBase)Var).GenerateAutoValue();
            }
            else if (SetVariableValueOption == VariableBase.eSetValueOptions.StartTimer)
            {
                if (Var.GetType() == typeof(VariableTimer))
                {
                    ((VariableTimer)Var).StartTimer();                   
                }
                else
                {
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();
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
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();

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
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    Error = "Operation type " + SetVariableValueOption + " is not supported for variable of type " + Var.GetType();
                    return;
                }
            }
            else
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = "Unknown set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value operation.";
                return;
            }

            ExInfo = GingerDicser.GetTermResValue(eTermResKey.Variable) + " '" + Var.Name + "' value was set to: '" + Var.Value + "'";
        }
    }
}
