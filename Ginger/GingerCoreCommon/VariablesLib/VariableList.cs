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
using System.Linq;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;

namespace GingerCore.Variables
{
    public class VariableList:VariableBase
    {
        public VariableList()
        {
            ValueList = "";
            CurrentValueIndex = 0;
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " List"; }
        }

        public override string VariableEditPage { get { return "VariableListPage"; } }

        private string mValueList;
        [IsSerializedForLocalRepository]
        public string ValueList { set { mValueList = value; OnPropertyChanged(nameof(this.ValueList)); OnPropertyChanged("Formula"); } get { return mValueList; } }

        private bool mRandomOrder;
        [IsSerializedForLocalRepository]
        public bool RandomOrder
        {
            set
            {
                mRandomOrder = value;
                OnPropertyChanged(nameof(this.RandomOrder));
            }
            get
            {
                return mRandomOrder;
            }
        }

        [IsSerializedForLocalRepository]
        public int CurrentValueIndex { set; get; }

        public override string GetFormula()
        {
            return ValueList.Replace("\r\n", ",");
        }

        public VariableList(string varb, List<string> lst)
        {
            Name = varb;
            ValueList = string.Join("\r\n", lst);
        }

        public override void ResetValue()
        {
            string[] listValues = Formula.Split(',');
            if (listValues.Length == 0)
                Value = string.Empty;
            else
            {
                CurrentValueIndex = 0;
                Value = listValues[CurrentValueIndex];
            }
        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            string[] listValues = Formula.Split(',');
            if (listValues.Length == 0)
            {
                Value = string.Empty;
                return false;
            }

            if (RandomOrder)
            {
                Random rnd = new Random();
                CurrentValueIndex = rnd.Next(listValues.Length);
                Value = listValues[CurrentValueIndex];
            }
            else
            {
                Value = listValues[CurrentValueIndex++];
                if (CurrentValueIndex >= listValues.Length) CurrentValueIndex = 0;
            }
            return true;
        }

        public override eImageType Image { get { return eImageType.VariableList; } }
        public override string VariableType
        {
            get { return "List"; }
        }
        public override bool SupportSetValue { get { return true; } }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.SetValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.AutoGenerateValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return true; } }

        public override bool IsObsolete { get { return true; } }


        public override bool SetValue(string value)
        {
            string[] possibleVals = Formula.Split(',');
            if (possibleVals != null && possibleVals.Contains(value))
            {
                Value = value;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
