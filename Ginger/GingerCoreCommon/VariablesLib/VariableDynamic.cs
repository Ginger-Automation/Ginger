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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;

namespace GingerCore.Variables
{
    public class VariableDynamic : VariableBase
    {
        // Do not serialize being set at runtime by Ginger Runner
        ProjEnvironment mProjEnvironment;
        BusinessFlow mBusinessFlow;

        private string mValueExpression;
        [IsSerializedForLocalRepository]
        public string ValueExpression
        {
            set { mValueExpression = value; OnPropertyChanged("Formula"); }
            get { return mValueExpression; }
        }

        public VariableDynamic()
        {
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Dynamic"; }
        }

        public void Init(ProjEnvironment ProjEnvironment, BusinessFlow BusinessFlow)
        {
            mProjEnvironment = ProjEnvironment;
            mBusinessFlow = BusinessFlow;
        }

        public override string VariableEditPage { get { return "VariableDynamicPage"; } }

        public override string GetFormula()
        {
            return ValueExpression;
        }

        public override string Value
        {
            get
            {
                return GenerateVEValue();
            }
            set
            {
                base.Value = value;
                OnPropertyChanged("Value");
            }
        }

        public override void ResetValue()
        {
            Value = GenerateVEValue();
        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            //NA
            errorMsg = "Generate Auto Value is not supported";
            return false;
        }

        private string GenerateVEValue()
        {
            try
            {
                if (mProjEnvironment == null && mBusinessFlow == null)
                {
                    return "Value will be calculated during execution.";
                }

                IValueExpression Ve = TargetFrameworkHelper.Helper.CreateValueExpression(mProjEnvironment, mBusinessFlow);
                Ve.Value = ValueExpression;

                if (Ve.Value != null && Ve.Value.Contains("{Var Name=" + Name + "}"))
                {
                    return "ERROR: " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " value cannot point to itself. ";
                }

                return Ve.ValueCalculated;
            }
            catch (Exception ex)//Env and BF objects were not set by Ginger Runner
            {
                return ex.Message;
            }
        }

        public override eImageType Image { get { return eImageType.CSS3Text; } }
        public override string VariableType { get { return "Dynamic"; } }

        public override bool SupportSetValue { get { return true; } }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return false; } }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            throw new NotImplementedException();
        }

        public override bool SetValue(string value)
        {
            ValueExpression = value;
            return true;
        }
    }
}
