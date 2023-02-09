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
using Amdocs.Ginger.Repository;
using System.Data;

namespace GingerCore.DataSource
{    

    public class DataSourceTable : RepositoryItemBase
    {        

        public enum eDSTableType
        {
            // Access
            [EnumValueDescription("Ginger Key Value")]
            GingerKeyValue,
            [EnumValueDescription("Customized")]
            Customized,
        }

        public  static class Fields
        {
            public static string DSTableType = "DSTableType";
            public static string Name = "Name";
            public static DataSourceBase DSC = null;
            public static DataTable DataTable = null;
        }

        private string mName { get; set; }
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        [IsSerializedForLocalRepository]
        public eDSTableType DSTableType { get; set; }

        public DataSourceBase DSC { get; set; }

        DataTable mDataTable;
        public DataTable DataTable
        {
            get { return mDataTable; }
            set
            {
                mDataTable = value;
                mDataTable.RowChanged += new DataRowChangeEventHandler(Row_Changed);
                mDataTable.ColumnChanged += new DataColumnChangeEventHandler(Col_Changed);
                mDataTable.RowDeleted += new DataRowChangeEventHandler(Row_Changed);
            }
        }

        private void Row_Changed(object sender, DataRowChangeEventArgs e)
        {
            OnPropertyChanged(nameof(Name));
        }

        private void Col_Changed(object sender, DataColumnChangeEventArgs e)
        {
            OnPropertyChanged(nameof(Name));
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        public override string GetNameForFileName()
        {
            return Name;
        }        
    }
}
