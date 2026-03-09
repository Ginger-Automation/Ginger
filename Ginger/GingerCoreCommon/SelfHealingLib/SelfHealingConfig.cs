#region License
/*
Copyright © 2014-2026 European Support Limited

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

namespace Amdocs.Ginger.Common.SelfHealingLib
{
    public class SelfHealingConfig : RepositoryItemBase
    {
        private bool mEnableSelfHealing;
        [IsSerializedForLocalRepository(true)]
        public bool EnableSelfHealing
        {
            get { return mEnableSelfHealing; }
            set
            {
                if (mEnableSelfHealing != value)
                {
                    mEnableSelfHealing = value;
                    OnPropertyChanged(nameof(EnableSelfHealing));
                }
            }
        }
        private bool mReprioritizePOMLocators;
        [IsSerializedForLocalRepository(true)]
        public bool ReprioritizePOMLocators
        {
            get { return mReprioritizePOMLocators; }
            set
            {
                if (mReprioritizePOMLocators != value)
                {
                    mReprioritizePOMLocators = value;
                    OnPropertyChanged(nameof(ReprioritizePOMLocators));
                }
            }
        }
        private bool mAutoFixAnalyzerIssue;
        [IsSerializedForLocalRepository]
        public bool AutoFixAnalyzerIssue
        {
            get { return mAutoFixAnalyzerIssue; }
            set
            {
                if (mAutoFixAnalyzerIssue != value)
                {
                    mAutoFixAnalyzerIssue = value;
                    OnPropertyChanged(nameof(AutoFixAnalyzerIssue));
                }
            }
        }
        private bool mAutoUpdateApplicationModel;
        [IsSerializedForLocalRepository]
        public bool AutoUpdateApplicationModel
        {
            get { return mAutoUpdateApplicationModel; }
            set
            {
                if (mAutoUpdateApplicationModel != value)
                {
                    mAutoUpdateApplicationModel = value;
                    OnPropertyChanged(nameof(AutoUpdateApplicationModel));
                }
            }
        }
        private bool mForceUpdateApplicationModel;
        [IsSerializedForLocalRepository]
        public bool ForceUpdateApplicationModel
        {
            get { return mForceUpdateApplicationModel; }
            set
            {
                if (mForceUpdateApplicationModel != value)
                {
                    mForceUpdateApplicationModel = value;
                    OnPropertyChanged(nameof(ForceUpdateApplicationModel));
                }
            }
        }
        private bool mAutoExecuteInSimulationMode;
        [IsSerializedForLocalRepository]
        public bool AutoExecuteInSimulationMode
        {
            get { return mAutoExecuteInSimulationMode; }
            set
            {
                if (mAutoExecuteInSimulationMode != value)
                {
                    mAutoExecuteInSimulationMode = value;
                    OnPropertyChanged(nameof(AutoExecuteInSimulationMode));
                }
            }
        }
        public bool SaveChangesInSourceControl { get; set; }

        public override string ItemName { get { return string.Empty; } set { } }

        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound)
            {
                if (name == "AutoExecuteInSimulateionMode")
                {
                    bool.TryParse(value, out bool res);
                    this.AutoExecuteInSimulationMode = res;
                    return true;
                }
            }
            return false;
        }

    }
}
