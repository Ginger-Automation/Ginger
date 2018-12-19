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
using System;
using System.Data;
using System.Collections.Generic;
using Amdocs.Ginger.Common.Enums;

namespace GingerCore.DataSource
{    

    public abstract class DataSourceBase : RepositoryItemBase
    {
        public enum eDSType
        {
            // Access
            [EnumValueDescription("MS Access")]
            MSAccess,
        }

        public  static class Fields
        {
            public static string DSType = "DSType";
            public static string Name = "Name";
            public static string FilePath = "FilePath";
            public static string FileFullPath = "FileFullPath";            
            public static DataSourceBase DSC = null;                  
        }

        private string mName { get; set; }
        [IsSerializedForLocalRepository]
        public string Name { get { return mName; } set { if (mName != value) { mName = value; OnPropertyChanged(nameof(Name)); } } }

        [IsSerializedForLocalRepository]
        public new Guid Guid { get; set; }

        private string mFilePath;
        private string mFileFullPath;
        

        [IsSerializedForLocalRepository]
        public new string FilePath
        {
            get { return mFilePath; }
            set
            {
                if (mFilePath != value)
                {
                    mFilePath = value;
                }
            }
        }

        public string FileFullPath
        {
            get
            {
                if (mFileFullPath == null)
                    mFileFullPath = mFilePath;
                return mFileFullPath;
            }
            set
            {
                if (mFileFullPath != value)
                {
                    mFileFullPath = value;
                }
            }
        }

        [IsSerializedForLocalRepository]
        public eDSType DSType { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<DataSourceTable> DSTableList = new ObservableList<DataSourceTable>();
        
        public DataSourceBase DSC { get; set; }


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
        public abstract void Init(string sFilePath, string sMode = "Read");

        public abstract void Close();
        public abstract ObservableList<DataSourceTable> GetTablesList();

        public abstract void UpdateTableList(ObservableList<DataSourceTable> dsTableList);
        public abstract List<string> GetColumnList(string tableName);

        public abstract DataTable GetQueryOutput(string query);

        public abstract void RunQuery(string query);

        public abstract void AddTable(string tableName, string columnList = "");

        public abstract void AddColumn(string tableName, string columnName, string columnType);

        public abstract void RemoveColumn(string tableName, string columnName);

        public abstract void DeleteTable(string tableName);

        public abstract void RenameTable(string tableName, string newTableName);

        public abstract string CopyTable(string tableName);

        public abstract bool IsTableExist(string tableName);

        public abstract void SaveTable(DataTable dataTable);
        public abstract bool ExporttoExcel(string TableName, string sExcelPath,String sSheetName);

        //public override void Save()
        //{
        //    if (DSTableList == null)
        //        return;
        //    foreach (DataSourceTable dsTable in DSTableList)
        //    {
        //        if (dsTable.DataTable != null)
        //        {
        //            dsTable.DSC.SaveTable(dsTable.DataTable);
        //        }
        //    }
        //    base.Save();
        //}

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.DataSource;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }
    }
}
