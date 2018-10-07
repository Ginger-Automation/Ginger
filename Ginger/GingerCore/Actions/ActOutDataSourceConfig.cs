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
using Amdocs.Ginger.Common;

namespace GingerCore.Actions
{
    public class ActOutDataSourceConfig : RepositoryItemBase
    {
        private bool mActive;
        public enum eOutputType
        {    
            Parameter,            
            Path,            
            Actual,
            Parameter_Path
        }

        public new static partial class Fields
        {
            public static string DSName = "DSName";
            public static string DSTable = "DSTable";
            public static string Active = "Active";
            public static string OutputType = "OutputType";
            public static string TableColumn = "TableColumn";
            public static string PossibleValues = "PossibleValues";
            public static string OutParamType = "OutParamMap";
        }

        [IsSerializedForLocalRepository]
        public string DSName { get; set; }

        [IsSerializedForLocalRepository]
        public string DSTable { get; set; }
        
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { mActive = value; OnPropertyChanged(Fields.Active); } }

        [IsSerializedForLocalRepository]
        public string OutputType { get; set; }

        [IsSerializedForLocalRepository]
        public string OutParamMap { get; set; }
        
        public override string ItemName
        {
            get
            {
                return this.OutputType;
            }
            set
            {
                this.OutputType = value;
            }
        }

        ObservableList<string> mPossibleValues = new ObservableList<string>();
        public ObservableList<string> PossibleValues
        {
            get
            {
                return mPossibleValues;
            }
            set
            {
                mPossibleValues = value;
                OnPropertyChanged(Fields.PossibleValues);
            }
        }

        string mTableColumn;
        [IsSerializedForLocalRepository]
        public string TableColumn
        {
            get
            {
                return mTableColumn;
            }
            set
            {
                mTableColumn = value;
                OnPropertyChanged(Fields.TableColumn);
            }
        }
    }
}