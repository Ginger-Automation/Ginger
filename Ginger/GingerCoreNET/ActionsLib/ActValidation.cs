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

using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.GeneralLib;
using GingerCoreNET.RosLynLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// This class is for dummy act - good for agile, and to be replace later on when real
//  act is available, so tester can write the step to be.
namespace GingerCore.Actions
{
    [Serializable]
    public class ActValidation : ActWithoutDriver
    {
        public enum eCalcEngineType
        {
            CS,
            VBS
        }
        
        public eCalcEngineType CalcEngineType
        {
            get
            {
                return (eCalcEngineType)GetOrCreateInputParam<eCalcEngineType>(nameof(CalcEngineType),eCalcEngineType.VBS);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(CalcEngineType), value.ToString());
                OnPropertyChanged(nameof(CalcEngineType));
            }
        }
        public override string ActionDescription { get { return "Condition Validation Action"; } }
        public override string ActionUserDescription { get { return "use this action to perform validation using value expression edit"; } }
        string ConditionCalculated = String.Empty;
        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to perform validations Using Value Expression editor ");

        }
        public string Condition
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = value;
                OnPropertyChanged(nameof(Condition));
            }
        }


        public override string ActionEditPage { get { return "ActionEditPages.ActValidationEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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

        public override String ActionType
        {
            get
            {
                return "Condition Validation Action";
            }
        }

        //public override System.Drawing.Image Image { get { return Resources.Act; } }
        public override eImageType Image { get { return eImageType.Check; } }

        public override void Execute()
        {
            // in case of CS need to deferantionate failure reason compilation issue/ bool issue/false result and show info ontop act.exinfo
            string csharpError = "";
            CalculateCondition(this.RunOnBusinessFlow, RunOnEnvironment, this);
            string rc = String.Empty;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && CalcEngineType.Equals(eCalcEngineType.VBS))
            {
                rc = VBS.ExecuteVBSEval(ConditionCalculated.Trim());
            }
            else
            {
                Boolean parsedValue;
                rc = CodeProcessor.GetEvaluteResult(ConditionCalculated.Trim(), out csharpError);
                if (Boolean.TryParse(rc.Trim(), out parsedValue))
                {
                    if (parsedValue)
                    {
                        rc = "-1";
                    }
                }
            }
            if (rc == "-1")
            {
                ConditionCalculated += " is True";
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                this.ExInfo = ConditionCalculated;
            }
            else
            { 
                ConditionCalculated += " is False";
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                this.Error = ConditionCalculated;
                if(!string.IsNullOrEmpty(csharpError))
                {
                    this.ExInfo = csharpError;
                }
            }
            return;
        }

        public void CalculateCondition(BusinessFlow BusinessFlow, Environments.ProjEnvironment ProjEnvironment, Act act)
        {
            if (Condition == null)
            {
                ConditionCalculated = "";
                return;
            }

            ValueExpression.Value = Condition;

            ValueExpression.Value = ValueExpression.Value.Replace("{ActionStatus}", act.Status.ToString());

            ConditionCalculated = ValueExpression.ValueCalculated;
        }
        public override void DoNewActionSetup()
        {
            CalcEngineType = eCalcEngineType.CS;
        }
    }
}
