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
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.SelfHealingLib
{
    public class SelfHealingConfig : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public bool EnableSelfHealing { get; set; } = true;

        [IsSerializedForLocalRepository]
        public bool ReprioritizePOMLocators { get; set; } = true;

        [IsSerializedForLocalRepository]
        public bool AutoFixAnalyzerIssue { get; set; }

        [IsSerializedForLocalRepository]
        public bool AutoUpdateApplicationModel { get; set; }

        [IsSerializedForLocalRepository]
        public bool AutoExecuteInSimulationMode{ get; set; }

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
