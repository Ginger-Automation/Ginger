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
using Amdocs.Ginger.Common;
using System;
using System.Data;
using System.Collections.Generic;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.Actions;
using System.Linq;
using System.Reflection;

namespace GingerCore.DataSource
{    

    public abstract class DataSourceBase : RepositoryItemBase 
    {
        public enum eDSType
        {
            [EnumValueDescription("LiteDataBase")]
            LiteDataBase,
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

        //Do not use this Observable List
        public ObservableList<DataSourceTable> DSTableList = new ObservableList<DataSourceTable>();
        

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
       
        public abstract ObservableList<DataSourceTable> GetTablesList();

        public abstract void UpdateTableList(ObservableList<DataSourceTable> dsTableList);
        public abstract List<string> GetColumnList(string tableName);

        public abstract DataTable GetQueryOutput(string query);

        public abstract void InitConnection();
        public abstract void AddRow(List<string> mColumnNames, DataSourceTable mDSTableDetails);
       
        public abstract void DeleteAll(List<object> AllItemsList, string TName=null);
        public abstract DataTable GetKeyName(string mDSTableName);

        public abstract void DuplicateRow(List<string> mColumnNames, List<object> SelectedItemsList,  DataSourceTable mDSTableDetails);
        
        public abstract DataTable GetTable(string TableName);
        
        public abstract string AddNewCustomizedTableQuery();

        public abstract string AddColumnName(string ColunmName);
        public abstract string UpdateDSReturnValues(string Name, string sColList, string sColVals);
        public abstract string GetExtension();
        public abstract string AddNewKeyValueTableQuery();
        public abstract bool RunQuery(string query);

        public abstract int GetRowCount(string TableName);
        public abstract void AddTable(string tableName, string columnList = "");

        public abstract void AddColumn(string tableName, string columnName, string columnType);

        public abstract void RemoveColumn(string tableName, string columnName);

        public abstract void DeleteTable(string tableName);
        
        public abstract void RenameTable(string tableName, string newTableName);

        public abstract string CopyTable(string tableName);

        public abstract bool IsTableExist(string tableName);

        public abstract void SaveTable(DataTable dataTable);
        public abstract bool ExporttoExcel(string TableName, string sExcelPath,String sSheetName,string sTableQueryValue="");

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

        public override string GetItemType()
        {
            return "DataSource";
        }

        public void UpdateDSNameChangeInItem(object item, string prevVarName, string newVarName, ref bool namechange)
        {
            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (Amdocs.Ginger.Common.GeneralLib.General.IsFieldToAvoidInVeFieldSearch(mi.Name))
                {
                    continue;
                }

                //Get the attr value
                PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                dynamic value = null;
                try
                {
                    if (mi.MemberType == MemberTypes.Property)
                    {
                        if (PI.CanWrite)
                        {
                            value = PI.GetValue(item);
                        }
                    }
                    else if (mi.MemberType == MemberTypes.Field)
                    {
                        value = item.GetType().GetField(mi.Name).GetValue(item);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception during UpdateVariableNameChangeInItem", ex);
                }

                if (value is IObservableList)
                {
                    List<dynamic> list = new List<dynamic>();
                    foreach (object o in value)
                    {
                        UpdateDSNameChangeInItem(o, prevVarName, newVarName, ref namechange);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        if (mi.Name == "VariableName")
                        {
                            if (value == prevVarName)
                            {
                                PI.SetValue(item, newVarName);
                            }
                            namechange = true;
                        }
                        else if (mi.Name == "StoreToValue")
                        {
                            if (value == prevVarName)
                            {
                                PI.SetValue(item, newVarName);
                            }
                            else if (value.IndexOf("{Var Name=" + prevVarName + "}") > 0)
                            {
                                PI.SetValue(item, value.Replace("{Var Name=" + prevVarName + "}", "{Var Name=" + newVarName + "}"));
                            }
                            namechange = true;
                        }
                        else
                        {
                            try
                            {
                                if (PI.CanWrite)
                                {
                                    string stringValue = value.ToString();
                                    string variablePlaceHoler = "{Var Name=xx}";
                                    if (stringValue.Contains(variablePlaceHoler.Replace("xx", prevVarName)))
                                    {
                                        PI.SetValue(item, stringValue.Replace(variablePlaceHoler.Replace("xx", prevVarName), variablePlaceHoler.Replace("xx", newVarName)));
                                        namechange = true;
                                    }
                                }
                            }
                            catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
                        }
                    }
                }
            }
        }
    }
}
