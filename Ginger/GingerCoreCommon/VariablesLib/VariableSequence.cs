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
    public class VariableSequence : VariableBase
    {
        public VariableSequence()
        {
            Min = 0;
            Max = 999;
            Interval = 1;
            CurrentValueIndex = 0;
        }

        public override string VariableUIType
        {
            get { return GingerDicser.GetTermResValue(eTermResKey.Variable) + " Sequence"; }
        }

        public override string VariableEditPage { get { return "VariableSequencePage"; } }

        private int mMin;
        [IsSerializedForLocalRepository]
        public int Min { set { mMin = value; OnPropertyChanged(nameof(this.Min)); OnPropertyChanged("Formula"); } get { return mMin; } }

        private int mMax;
        [IsSerializedForLocalRepository]
        public int Max { set { mMax = value; OnPropertyChanged(nameof(this.Max));  OnPropertyChanged("Formula"); } get { return mMax; } }

        private int mInterval;
        [IsSerializedForLocalRepository]
        public int Interval { set { mInterval = value; OnPropertyChanged(nameof(this.Interval));  OnPropertyChanged("Formula"); } get { return mInterval; } }

        [IsSerializedForLocalRepository]
        public bool IsInteger { set; get; }

        [IsSerializedForLocalRepository]
        public int CurrentValueIndex { set; get; }
       
        public override string GetFormula()
        {
            return Min + "-" + Max + " Interval " + Interval;
        }

        public override void ResetValue()
        {
            CurrentValueIndex = 0;
            Value = Min.ToString();
        }

        public override bool GenerateAutoValue(ref string errorMsg)
        {
            CurrentValueIndex++;
            int delta = CurrentValueIndex * Interval;
            int val = Min + delta;
            if (val > Max)
            {
                val = Min;
                CurrentValueIndex = 0;
            }
            Value = val.ToString();
            return true;
        }
    
        public override eImageType Image { get { return eImageType.SequentialExecution; } }
        public override string VariableType
        {
            get { return "Sequence"; }
        }
        public override bool SupportSetValue { get { return false; } }

        public override List<VariableBase.eSetValueOptions> GetSupportedOperations()
        {
            List<VariableBase.eSetValueOptions> supportedOperations = new List<VariableBase.eSetValueOptions>();
            supportedOperations.Add(VariableBase.eSetValueOptions.AutoGenerateValue);
            supportedOperations.Add(VariableBase.eSetValueOptions.ResetValue);
            return supportedOperations;
        }

        public override bool SupportResetValue { get { return true; } }

        public override bool SupportAutoValue { get { return true; } }
    }
}
