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

using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Properties;
using System;
using System.Collections.Generic;

namespace GingerCore.Variables
{
    public class VariableList:VariableBase
    {
        public new static partial class Fields
        {
            public static string ValueList = "ValueList";
            public static string RandomOrder = "RandomOrder";                   
        }

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
        public string ValueList { set { mValueList = value; OnPropertyChanged("Formula"); } get { return mValueList; } }
        
        [IsSerializedForLocalRepository]
        public bool RandomOrder { set; get; }

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

        public override void GenerateAutoValue()
        {
            string[] listValues = Formula.Split(',');
            if (listValues.Length == 0)
            {
                Value = string.Empty;
                return;
            }

            if (RandomOrder)
            {
                Random rnd = new Random();
                CurrentValueIndex = rnd.Next(listValues.Length);
                Value = listValues[CurrentValueIndex];
            }
            else
            {
                CurrentValueIndex++;
                if (CurrentValueIndex >= listValues.Length) CurrentValueIndex = 0;
                Value = listValues[CurrentValueIndex];
            }
        }

        public override System.Drawing.Image Image { get { return Resources.List; } }
        public override string VariableType() { return "List"; }

        public override bool SupportSetValue { get { return true; } }

        public override List<ActSetVariableValue.eSetValueOptions> GetSupportedOperations()
        {
            List<ActSetVariableValue.eSetValueOptions> supportedOperations = new List<ActSetVariableValue.eSetValueOptions>();
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.SetValue);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.AutoGenerateValue);
            supportedOperations.Add(ActSetVariableValue.eSetValueOptions.ResetValue);
            return supportedOperations;
        }
    }
}