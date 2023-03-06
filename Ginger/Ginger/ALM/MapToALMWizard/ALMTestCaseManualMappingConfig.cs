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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using System.ComponentModel;
using System.Linq;

namespace Ginger.ALM.MapToALMWizard
{
    public enum TestCaseStepsMappingError { Invalid, Error }
    public enum eMappingStatus
    {
        Mapped,
        Partial,
        UnMapped,
        Unknown
    }
    /// <summary>
    /// Hold Test Case and Steps Mapping data.
    /// Manage changes and user setup.
    /// </summary>
    public class ALMTestCaseManualMappingConfig : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public ActivitiesGroup activitiesGroup = new ActivitiesGroup();
        public ALMTSTest aLMTSTest;
        public ObservableList<ALMTestStepManualMappingConfig> testStepsMappingList = new ObservableList<ALMTestStepManualMappingConfig>();

        public string TestCaseName
        {
            get
            {
                if(aLMTSTest == null)
                {
                    return "";
                }
                return aLMTSTest.TestName;
            }
        }
        public string ActivityGroupName
        {
            get
            {
                return activitiesGroup.Name;
            }
        }

        public void UpdateMappedTestCase(ALMTSTest testCase)
        {
            aLMTSTest = testCase;
            OnPropertyChanged(nameof(TestCaseName));
        }
        public void Clear()
        {
            this.aLMTSTest = null;
        }
        public void UpdateTestCaseMapStatus(int unmappedTestStepsListCount)
        {
            if (testStepsMappingList.Count == 0 || 
                testStepsMappingList.All(ts => ts.almTestStep is null || ts.almTestStep.StepName is null))
            {
                MappingStatus = eMappingStatus.UnMapped; 
                return;
            }
            if (testStepsMappingList.All(ts => ts.almTestStep is not null && ts.almTestStep.StepName is not null)
                && unmappedTestStepsListCount == 0)
            {
                MappingStatus = eMappingStatus.Mapped;
                return;
            }
            MappingStatus = eMappingStatus.Partial;
        }
        private eMappingStatus mMappingStatus;
        public eMappingStatus MappingStatus
        {
            get
            {
                return mMappingStatus;
            }
            set
            {
                mMappingStatus = value;
                OnPropertyChanged(nameof(MappingStatus));
                OnPropertyChanged(nameof(MappingStatusIcon));
            }
        }

        public eImageType MappingStatusIcon
        {
            get
            {
                switch (MappingStatus)
                {
                    case eMappingStatus.Mapped:
                        return eImageType.Mapped;
                    case eMappingStatus.Partial:
                        return eImageType.Partial;
                    case eMappingStatus.UnMapped:
                        return eImageType.UnMapped;
                    default:
                        return eImageType.Unknown;
                }
            }
        }

    }
}
