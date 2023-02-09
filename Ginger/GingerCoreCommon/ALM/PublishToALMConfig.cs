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
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace GingerCore.ALM
{
    public class PublishToALMConfig : INotifyPropertyChanged
    {
        public enum eALMTestSetLevel
        {
            [EnumValueDescription("Business Flow")]
            BusinessFlow,
            [EnumValueDescription("Run Set")]
            RunSet
        }
        public enum eExportType
        {
            [EnumValueDescription("Results Only")]
            ResultsOnly,
            [EnumValueDescription("Entities and Results")]
            EntitiesAndResults
        }
        public bool IsVariableInTCRunUsed { get; set; }

        private string mVariableForTCRunName;        
        public string VariableForTCRunName
        {
            get
            {
                return mVariableForTCRunName;
            }
            set
            {
                if (mVariableForTCRunName != value)
                {
                    mVariableForTCRunName = value;
                    OnPropertyChanged(nameof(VariableForTCRunName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
      
        public string VariableForTCRunNameCalculated { get; set; }   
                       
        public bool ToAttachActivitiesGroupReport { get; set; }
             
        public FilterByStatus FilterStatus { get; set; }
        public eALMType PublishALMType { get; set; }
        public eALMTestSetLevel ALMTestSetLevel { get; set; } 
        public eExportType ExportType { get; set; }
        public ObservableList<ExternalItemFieldBase> AlmFields { get; set; }
        public string TestSetFolderDestination { get; set; }
        public string TestCaseFolderDestination { get; set; }
        public void CalculateTCRunName(IValueExpression ve)
        {          
            if (IsVariableInTCRunUsed && (VariableForTCRunName != null) && (VariableForTCRunName != string.Empty))
            {
                ve.Value = VariableForTCRunName;
                VariableForTCRunNameCalculated = ve.ValueCalculated;
            }
            else
            {
                ve.Value = "GingerRun_{CS Exp=DateTime.Now}";
                VariableForTCRunNameCalculated = ve.ValueCalculated;
            }
        }
    }
}
