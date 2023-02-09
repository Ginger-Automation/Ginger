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
using System.Collections.Generic;
using System.ComponentModel;

namespace GingerCore.DataSource
{ 
    public class ActDSConditon : INotifyPropertyChanged
    {
        //TODO: Add Param type and valid value, if it is number , date etc... so we can have controls created for it 

        public event PropertyChangedEventHandler PropertyChanged;

        private string mwValue;
        private eCondition mwCond;
        private string mwTableColumn;
        public static partial class Fields
        {
            public static string wCondition = "wCondition";
            public static string wTableColumn = "wTableColumn";
            public static string wOperator = "wOperator";
            public static string wValue = "wValue";
            public static string PossibleCondValues = "PossibleCondValues";
            public static string PossibleColumnValues = "PossibleColumnValues";
        }

        public enum eOperator
        {
            [EnumValueDescription("Equals")]
            Equals,
            [EnumValueDescription("Not Equals")]
            NotEquals,
            [EnumValueDescription("Contains")]
            Contains,
            [EnumValueDescription("Not Contains")]
            NotContains,
            [EnumValueDescription("Starts With")]
            StartsWith,
            [EnumValueDescription("Not Starts With")]
            NotStartsWith,
            [EnumValueDescription("Ends With")]
            EndsWith,
            [EnumValueDescription("Not Ends With")]
            NotEndsWith,
            [EnumValueDescription("Is Null")]
            IsNull,
            [EnumValueDescription("Is Not Null")]
            IsNotNull
        }

        public enum eCondition
        {   
            AND,            
            OR,
            [EnumValueDescription("")]
            EMPTY
        }

        public eCondition wCondition { get { return mwCond; } set { mwCond = value; OnPropertyChanged(Fields.wCondition); } }

        public string wTableColumn { get { return mwTableColumn; }
            set { mwTableColumn = value; OnPropertyChanged(Fields.wTableColumn); } }
    
        public eOperator wOperator { get; set; }

        public string wValue { get { return mwValue; } set { mwValue = value; OnPropertyChanged(Fields.wValue); } }
        
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        ObservableList<string> mPossibleCondValues = new ObservableList<string>();
        public ObservableList<string> PossibleCondValues
        {
            get
            {
                return mPossibleCondValues;
            }
            set
            {
                mPossibleCondValues = value;
                OnPropertyChanged(Fields.PossibleCondValues);
            }
        }

        List<string> mPossibleColumnValues = new List<string>();
        public List<string> PossibleColumnValues
        {
            get
            {
                return mPossibleColumnValues;
            }
            set
            {
                mPossibleColumnValues = value;
                OnPropertyChanged(Fields.PossibleColumnValues);
            }
        }
    }
}
