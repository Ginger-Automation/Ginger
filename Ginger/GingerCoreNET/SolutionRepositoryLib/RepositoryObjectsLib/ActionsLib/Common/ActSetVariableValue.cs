//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Repository;
//using Amdocs.Ginger.Common.Repository;
//using GingerCoreNET.Dictionaries;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using System.Collections.Generic;
//using System.Linq;
//using Amdocs.Ginger.CoreNET.Execution;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common
//{
//    public class ActSetVariableValue : ActWithoutDriver
//    {
//        public override string ActionDescription { get { return "Set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Action"; } }
//        public override string ActionUserDescription { get { return "Allows to set the value of a " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " in run time"; } }
        
//        public override string ActionEditPage { get { return "ActSetVariableValuePage"; } }
//        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
//        public override bool ValueConfigsNeeded { get { return true; } }
        
//        public override List<Platform.ePlatformType> Platforms
//        {
//            get
//            {
//                if (mPlatforms.Count == 0)
//                {
//                    AddAllPlatforms();
//                }
//                return mPlatforms;
//            }
//        }

//        public override string ActionType
//        {
//            get { return "Set Variable"; }
//        }

//        [IsSerializedForLocalRepository]
//        public string VariableName { set; get; }

//        public enum eSetValueOptions {SetValue, ResetValue, AutoGenerateValue  };
//        [IsSerializedForLocalRepository]
//        public eSetValueOptions SetVariableValueOption 
//        { set; get; }

//        public override void Execute()
//        {
//            VariableBase Var = RunOnBusinessFlow.GetHierarchyVariableByName(VariableName);
//            if (Var == null)
//            {
//                Status = eRunStatus.Failed;
//                Error = GingerDicser.GetTermResValue(eTermResKey.Variable) + " was not found.";
//                return;
//            }

//            if (SetVariableValueOption == eSetValueOptions.SetValue)
//            {               
//                ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow,DSList);
//                VE.Value = this.Value;

//                if (Var.GetType() == typeof(VariableString))
//                {
//                    ((VariableString)Var).Value = VE.ValueCalculated;
//                }
//                else if (Var.GetType() == typeof(VariableSelectionList))
//                {
//                    string calculatedValue = VE.ValueCalculated;
//                    if (((VariableSelectionList)Var).OptionalValuesList.Where(pv => pv.Value == calculatedValue).FirstOrDefault() != null)
//                        ((VariableSelectionList)Var).Value = calculatedValue;
//                    else
//                    {
//                        Status = eRunStatus.Failed;
//                        Error = "The value '" + calculatedValue + "' is not part of the possible values of the '" + Var.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".";
//                        return;
//                    }
//                }
//                else if (Var.GetType() == typeof(VariableList))
//                {
//                    string calculatedValue = VE.ValueCalculated;
//                    string[] possibleVals = ((VariableList)Var).Formula.Split(',');
//                    if (possibleVals != null && possibleVals.Contains(calculatedValue))
//                        ((VariableList)Var).Value = calculatedValue;
//                    else
//                    {
//                        Status = eRunStatus.Failed;
//                        Error = "The value '" + calculatedValue + "' is not part of the possible values of the '" + Var.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ".";
//                        return;
//                    }
//                }
//                else if (Var.GetType() == typeof(VariableDynamic))
//                {
//                    ((VariableDynamic)Var).ValueExpression = VE.Value;
//                }
//            }
//            else if (SetVariableValueOption == eSetValueOptions.ResetValue)
//            {
//                ((VariableBase)Var).ResetValue();
//            }
//            else if (SetVariableValueOption == eSetValueOptions.AutoGenerateValue)
//            {
//                ((VariableBase)Var).GenerateAutoValue();
//            }
//            else
//            {
//                Status = eRunStatus.Failed;
//                Error = "Unknown set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value operation.";
//                return;
//            }

//            ExInfo = GingerDicser.GetTermResValue(eTermResKey.Variable) + " '" + Var.Name + "' value was set to: '" + Var.Value + "'";
//        }
//    }
//}
