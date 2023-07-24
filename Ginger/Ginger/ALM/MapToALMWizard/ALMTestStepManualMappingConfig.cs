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
using GingerCore.Activities;
using GingerCore.ALM.QC;
using System.ComponentModel;

namespace Ginger.ALM.MapToALMWizard
{
    /// <summary>
    /// ALMTestStepManualMappingConfig:
    /// 
    /// </summary>
    public class ALMTestStepManualMappingConfig : INotifyPropertyChanged
    {
        public ActivityIdentifiers activity = null;
        public ALMTSTestStep almTestStep = null;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public string StepName
        {
            get
            {
                if (almTestStep == null)
                {
                    return "";
                }
                return almTestStep.StepName;
            }
        }
        public string ActivityName
        {
            get
            {
                return activity.ActivityName;
            }
        }

        public void UpdateMappedTestStep(ALMTSTestStep testStep)
        {
            almTestStep = testStep;
            OnPropertyChanged(nameof(StepName));
        }

    }
}
