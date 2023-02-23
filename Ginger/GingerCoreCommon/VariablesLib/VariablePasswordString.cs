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

using System.Collections.Generic;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariablePasswordString : VariableBase
    {
        public VariablePasswordString()
        {       
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Password String"; }
        }

        public override string VariableEditPage { get { return "VariablePasswordStringPage"; } }

        private string mPassword;
        [IsSerializedForLocalRepository]
        public string Password 
        {
            set { mPassword = value; Value = value; OnPropertyChanged(nameof(this.Password)); OnPropertyChanged("Formula"); }
            get 
            {
                if (!string.IsNullOrEmpty(mPassword))
                {
                    return mPassword;
                }
                else if (!string.IsNullOrEmpty(Value))
                {
                    mPassword = Value;
                }
                return mPassword;
            } 
        }

        public override string GetFormula()
        {
            return Password;
        }

        public override void ResetValue()
        {
            Value = Password; 
        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            //NA
            errorMsg = "Generate Auto Value is not supported";
            return false;
        }

        public override eImageType Image { get { return eImageType.Password; } }
        public override string VariableType
        {
            get { return "PasswordString"; }
        }
        public override bool SupportSetValue { get { return false; } }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            throw new System.NotImplementedException();
        }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return false; } }
    }
}
