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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.DataSource;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

namespace GingerCore.Actions
{
    public class ActDSTableElement : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Data Source Action"; } }

        public override eImageType Image { get { return eImageType.DataTable; } }

        public override string ActionUserDescription { get { return "Data Source Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to Read/Write Data from a common place for all Business Flows/Activities/Actions.");
        }

        public override string ActionEditPage { get { return "ActDataSourcePage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override String ActionType
        {
            get
            {
                return "Data Source Manipulation";
            }
        }

        public override void Execute()
        {
            DataSourceBase DataSource = null;
            string outVal = "";
            foreach (DataSourceBase ds in DSList)
            {
                if (DSName == null)
                {
                    string[] Token = ValueExp.Split(new[] { "{DS Name=", " " }, StringSplitOptions.None);
                    DSName = Token[1];
                }

                if (ds.Name == DSName)
                {
                    DataSource = ds;
                }
            }
            if (DataSource.DSType == DataSourceBase.eDSType.LiteDataBase)
            {
                GingerCoreNET.DataSource.GingerLiteDB liteDB = new GingerCoreNET.DataSource.GingerLiteDB();
                string value = GetInputParamValue("Value");
                if (!string.IsNullOrEmpty(value))
                {
                    ValueExpression mValueExpression = new(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, RunOnBusinessFlow, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>())
                    {
                        Value = value
                    };
                    ValueUC = mValueExpression.ValueCalculated;
                }

                LiteDBSQLTranslator liteDBSQLTranslator = new(this);
                this.ValueExp = liteDBSQLTranslator.CreateValueExpression();

                string Querystring = this.ValueExp;
                Regex rxvarPattern = new Regex(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
                MatchCollection matcheslist = rxvarPattern.Matches(ValueExp);
                for (int i = 0; i < matcheslist.Count; i++)
                {
                    if (!string.IsNullOrEmpty(matcheslist[i].ToString()))
                    {
                        ValueExpression mValueExpression = new(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, RunOnBusinessFlow, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>())
                        {
                            Value = matcheslist[i].ToString()
                        };
                        string ValueUCdata = mValueExpression.ValueCalculated;
                        Querystring = Querystring.Replace(matcheslist[i].ToString(), ValueUCdata);
                    }
                }

                string Query = Querystring.Substring(Querystring.IndexOf("QUERY=") + 6, Querystring.Length - (Querystring.IndexOf("QUERY=") + 7));
                liteDB.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DataSource.FileFullPath);

                if (this.ExcelConfig != null)
                {
                    var orgExcelConfig = this.ExcelConfig;
                    var tempExp = new ExportToExcelConfig();
                    try
                    {
                        ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ExcelConfig.ExcelPath
                        };
                        tempExp.ExcelPath = VE.ValueCalculated;

                        VE.Value = ExcelConfig.ExcelSheetName;
                        tempExp.ExcelSheetName = VE.ValueCalculated;

                        VE.Value = ExcelConfig.ExportQueryValue;
                        tempExp.ExportQueryValue = VE.ValueCalculated;
                        if (ExcelConfig.IsCustomExport)
                        {
                            tempExp.ExportQueryValue = this.ExcelConfig.CreateQueryWithWhereList(ExcelConfig.ColumnList.ToList().FindAll(x => x.IsSelected), ExcelConfig.WhereConditionStringList, DSTableName, DataSourceBase.eDSType.LiteDataBase);
                        }
                        this.ExcelConfig = tempExp;
                        liteDB.Execute(this, Query);
                    }
                    finally
                    {
                        this.ExcelConfig = orgExcelConfig;
                    }
                }
                else
                {
                    liteDB.Execute(this, Query);
                }
            }
            else if (DataSource.DSType == DataSourceBase.eDSType.MSAccess)
            {
                switch (ControlAction)
                {
                    case eControlAction.GetValue:
                        ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ValueExp
                        };
                        AddOrUpdateReturnParamActual("Output", VE.ValueCalculated);
                        break;
                    case eControlAction.SetValue:
                        ValueExpression SVE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = this.Value
                        };

                        ValueExpression VEUpdate = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList, true, SVE.ValueCalculated)
                        {
                            Value = ValueExp
                        };
                        outVal = VEUpdate.ValueCalculated;
                        break;
                    case eControlAction.MarkAsDone:
                        ValueExpression VEMAD = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ValueExp
                        };
                        outVal = VEMAD.ValueCalculated;
                        break;
                    case eControlAction.RowCount:
                    case eControlAction.AvailableRowCount:
                        ValueExpression VERC = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ValueExp
                        };
                        AddOrUpdateReturnParamActual("Output", VERC.ValueCalculated);
                        break;
                    case eControlAction.ExportToExcel:
                        ValueExpression VEETE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);

                        var excelSheetName = string.Empty;
                        var query = string.Empty;
                        var excelFilePath = string.Empty;
                        if (ExcelConfig == null)
                        {
                            VEETE.Value = ExcelPath;
                            excelFilePath = VEETE.ValueCalculated;

                            VEETE.Value = ExcelSheetName;
                            excelSheetName = VEETE.ValueCalculated;
                        }
                        else
                        {
                            VEETE.Value = ExcelConfig.ExcelPath;
                            excelFilePath = VEETE.ValueCalculated;

                            VEETE.Value = ExcelConfig.ExcelSheetName;
                            excelSheetName = VEETE.ValueCalculated;

                            VEETE.Value = ExcelConfig.ExportQueryValue;
                            query = VEETE.ValueCalculated;

                            if (ExcelConfig.IsCustomExport)
                            {
                                query = this.ExcelConfig.CreateQueryWithWhereList(ExcelConfig.ColumnList.ToList().FindAll(x => x.IsSelected), ExcelConfig.WhereConditionStringList, DSTableName, DataSourceBase.eDSType.MSAccess);
                            }
                        }

                        if (excelFilePath.ToLower().EndsWith(".xlsx"))
                        {
                            DataSource.ExporttoExcel(DSTableName, excelFilePath, excelSheetName, query.ToLower());
                        }
                        else
                        {
                            this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            Error = "The Export Excel can be *.xlsx only";
                        }
                        break;
                    case eControlAction.DeleteRow:
                        ValueExpression veDel = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ValueExp
                        };
                        outVal = veDel.ValueCalculated;
                        int rowCount = 0;
                        if (!string.IsNullOrEmpty(outVal) && !int.TryParse(outVal, out rowCount))
                        {
                            this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            Error = outVal;
                        }
                        break;
                    case eControlAction.AddRow:
                        DataSourceTable DSTable = null;
                        ObservableList<DataSourceTable> dstTables = DataSource.GetTablesList();
                        foreach (DataSourceTable dst in dstTables)
                        {
                            if (dst.Name == DSTableName)
                            {
                                DSTable = dst;
                                DSTable.DataTable = dst.DSC.GetTable(DSTableName);
                                break;
                            }
                        }
                        if (DSTable != null)
                        {
                            List<string> mColumnNames = DataSource.GetColumnList(DSTableName);
                            DataSource.AddRow(mColumnNames, DSTable);
                            DataSource.SaveTable(DSTable.DataTable);
                            //Get GingerId
                            DataTable dt = DataSource.GetTable(DSTableName);
                            DataRow row = dt.Rows[^1];
                            string GingerId = Convert.ToString(row["GINGER_ID"]);
                            AddOrUpdateReturnParamActual("GINGER_ID", GingerId);
                        }
                        else
                        {
                            Error = "No table present in the DataSource with the name =" + DSTableName;
                        }
                        break;
                    default:
                        ValueExpression VEDR = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList)
                        {
                            Value = ValueExp
                        };
                        outVal = VEDR.ValueCalculated;
                        break;
                }
            }
            return;
        }
        public new static partial class Fields
        {
            public static readonly string DSName = "DSName";
            public static readonly string DSTableName = "DSTableName";


            public static readonly string ControlAction = "ControlAction";
            public static readonly string Identifier = "Identifier";

            public static readonly string KeyName = "KeyName";

            public static readonly string Customized = "Customized";
            public static readonly string ByQuery = "ByQuery";

            public static readonly string QueryValue = "QueryValue";
            public static readonly string ColSelectorValue = "ColSelectorValue";
            public static readonly string WhereOperator = "WhereOperator";

            public static readonly string WhereProperty = "WhereProperty";

            public static readonly string LocateColTitle = "LocateColTitle";
            public static readonly string LocateRowType = "LocateRowType";
            public static readonly string LocateRowValue = "LocateRowValue";

            public static readonly string ByRowNum = "ByRowNum";
            public static readonly string ByNextAvailable = "ByNextAvailable";
            public static readonly string ByWhere = "ByWhere";

            public static readonly string WhereColSelector = "WhereColSelector";
            public static readonly string WhereColumnTitle = "WhereColumnTitle";
            public static readonly string WhereColumnValue = "WhereColumnValue";


            public static readonly string ValueExp = "ValueExpression";

            public static readonly string WhereConditions = "WhereConditions";

            public static readonly string ExcelPath = "ExcelPath";
            public static readonly string ExcelSheetName = "ExcelSheetName";


        }


        private string mKeyName;
        [IsSerializedForLocalRepository]
        public string KeyName
        {
            get
            {
                return mKeyName;
            }
            set
            {
                if (!string.Equals(mKeyName, value))
                {
                    mKeyName = value;
                    OnPropertyChanged(nameof(KeyName));
                }
            }
        }
        private eControlAction mControlAction;
        [IsSerializedForLocalRepository]
        public eControlAction ControlAction
        {
            get
            {
                return mControlAction;
            }
            set
            {
                if (mControlAction != value)
                {
                    mControlAction = value;
                    OnPropertyChanged(nameof(ControlAction));
                }
            }
        }

        private string mValueExp;
        [IsSerializedForLocalRepository]
        public string ValueExp
        {
            get
            {
                return mValueExp;
            }
            set
            {
                if (mValueExp != value)
                {
                    mValueExp = value;
                    OnPropertyChanged(nameof(ValueExp));
                }
            }
        }

        public string VarName { get; set; }

        [IsSerializedForLocalRepository]
        public string ValueUC { get; set; }

        private ObservableList<ActDSConditon> mWhereConditions;
        [IsSerializedForLocalRepository]
        public ObservableList<ActDSConditon> WhereConditions
        {
            get
            {
                return mWhereConditions;
            }
            set
            {
                if (mWhereConditions != value)
                {
                    mWhereConditions = value;
                    OnPropertyChanged(nameof(WhereConditions));
                }
            }
        }

        private ExportToExcelConfig mExcelConfig;
        [IsSerializedForLocalRepository]
        public ExportToExcelConfig ExcelConfig
        {
            get
            {
                return mExcelConfig;
            }
            set
            {
                if (mExcelConfig != value)
                {
                    if (mExcelConfig != null)
                    {
                        mExcelConfig.OnDirtyStatusChanged -= this.RaiseDirtyChanged;
                    }
                    mExcelConfig = value;
                    mExcelConfig.StartDirtyTracking();
                    mExcelConfig.OnDirtyStatusChanged += this.RaiseDirtyChanged;
                    OnPropertyChanged(nameof(ExcelConfig));
                }
            }
        }

        public enum eControlAction
        {
            [EnumValueDescription("Get Value")]
            GetValue,
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Mark Row As Used")]
            MarkAsDone,
            [EnumValueDescription("Delete Row")]
            DeleteRow,
            [EnumValueDescription("Mark All As UnUsed")]
            MarkAllUnUsed,
            [EnumValueDescription("Mark All As Used")]
            MarkAllUsed,
            [EnumValueDescription("Delete All")]
            DeleteAll,
            [EnumValueDescription("Get Row Count")]
            RowCount,
            [EnumValueDescription("Get Available Row Count")]
            AvailableRowCount,
            [EnumValueDescription("Export to Excel")]
            ExportToExcel,
            [EnumValueDescription("Add Row")]
            AddRow
        }

        public enum eRunColSelectorValue
        {
            [EnumValueDescription("Column Title")]
            ColTitle,
            [EnumValueDescription("Column Number")]
            ColNum
        }

        public enum eRunColPropertyValue
        {
            [EnumValueDescription("Value")]
            Value,
            [EnumValueDescription("isSelected")]
            isSelected
        }

        public enum eRunColOperator
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


        private string mQueryValue;
        [IsSerializedForLocalRepository]
        public string QueryValue
        {
            get
            {
                return mQueryValue;
            }
            set
            {
                if (mQueryValue != value)
                {
                    mQueryValue = value;
                    OnPropertyChanged(nameof(QueryValue));
                }
            }
        }

        private string mColSelectorValue;
        [IsSerializedForLocalRepository]
        public string ColSelectorValue
        {
            get
            {
                return mColSelectorValue;
            }
            set
            {
                if (mColSelectorValue != value)
                {
                    mColSelectorValue = value;
                    OnPropertyChanged(nameof(ColSelectorValue));
                }
            }
        }

        private string mDSName;
        [IsSerializedForLocalRepository]
        public string DSName
        {
            get
            {
                return mDSName;
            }
            set
            {
                if (mDSName != value)
                {
                    mDSName = value;
                    OnPropertyChanged(nameof(DSName));
                }
            }
        }

        private bool mCustomized;
        [IsSerializedForLocalRepository]
        public bool Customized
        {
            get
            {
                return mCustomized;
            }
            set
            {
                if (mCustomized != value)
                {
                    mCustomized = value;
                    OnPropertyChanged(nameof(Customized));
                }
            }
        }

        private bool mByQuery;
        [IsSerializedForLocalRepository]
        public bool ByQuery
        {
            get
            {
                return mByQuery;
            }
            set
            {
                mByQuery = value;
                OnPropertyChanged(nameof(ByQuery));
            }
        }

        private string mDSTableName;
        [IsSerializedForLocalRepository]
        public string DSTableName
        {
            get
            {
                return mDSTableName;
            }
            set
            {
                if (mDSTableName != value)
                {
                    mDSTableName = value;
                    OnPropertyChanged(nameof(DSTableName));
                }
            }
        }

        private eRunColPropertyValue mWhereProperty;
        [IsSerializedForLocalRepository]
        public eRunColPropertyValue WhereProperty
        {
            get
            {
                return mWhereProperty;
            }
            set
            {
                if (mWhereProperty != value)
                {
                    mWhereProperty = value;
                    OnPropertyChanged(nameof(WhereProperty));
                }
            }
        }

        private eRunColOperator mWhereOperator;
        [IsSerializedForLocalRepository]
        public eRunColOperator WhereOperator
        {
            get { return mWhereOperator; }
            set
            {

                if (mWhereOperator != value)
                {
                    mWhereOperator = value;
                    OnPropertyChanged(nameof(WhereOperator));
                }
            }
        }

        private bool mByRowNum;
        [IsSerializedForLocalRepository]
        public bool ByRowNum
        {
            get
            {
                return mByRowNum;
            }
            set
            {
                if (mByRowNum != value)
                {
                    mByRowNum = value;
                    OnPropertyChanged(nameof(ByRowNum));
                }
            }
        }

        private int mRowVal;
        [IsSerializedForLocalRepository]
        public int RowVal
        {
            get
            {
                return mRowVal;
            }
            set
            {
                if (mRowVal != value)
                {
                    mRowVal = value;
                    OnPropertyChanged(nameof(RowVal));
                }
            }
        }


        [IsSerializedForLocalRepository]
        public bool ByNextAvailable { get; set; }

        [IsSerializedForLocalRepository]
        public bool ByWhere { get; set; }
        [IsSerializedForLocalRepository]
        public bool IsKeyValueTable { get; set; }
        public bool MarkUpdate { get; set; }
        [IsSerializedForLocalRepository]
        public eRunColSelectorValue WhereColSelector { get; set; }

        [IsSerializedForLocalRepository]
        public bool BySelectedCell { get; set; }

        [IsSerializedForLocalRepository]
        public string WhereColumnTitle { get; set; }
        [IsSerializedForLocalRepository]
        public string WhereColumnValue { get; set; }

        [IsSerializedForLocalRepository]
        public string LocateColTitle { get; set; }
        [IsSerializedForLocalRepository]
        public string LocateRowType { get; set; }
        [IsSerializedForLocalRepository]
        public string LocateRowValue
        {
            get;
            set;
        }

        string mExcelPath;
        [IsSerializedForLocalRepository]
        public string ExcelPath
        {
            get { return mExcelPath; }
            set
            {

                if (mExcelPath != value)
                {
                    mExcelPath = value;
                    OnPropertyChanged(nameof(ExcelPath));
                }
            }
        }

        string mExcelSheetName;
        [IsSerializedForLocalRepository]
        public string ExcelSheetName
        {
            get { return mExcelSheetName; }
            set
            {

                if (mExcelSheetName != value)
                {
                    mExcelSheetName = value;
                    OnPropertyChanged(nameof(ExcelSheetName));
                }
            }
        }

        public ObservableList<ActDSConditon> ActDSConditions
        {
            get
            {
                return WhereConditions;
            }
        }

        public void AddDSCondition(ActDSConditon.eCondition wCond, string wColName, ActDSConditon.eOperator wOper, string wValue, List<string> mColName)
        {

            ActDSConditon ADSC = new ActDSConditon();
            ObservableList<string> Condition = [];
            if (wCond != ActDSConditon.eCondition.EMPTY)
            {
                foreach (ActDSConditon.eCondition item in Enum.GetValues(typeof(ActDSConditon.eCondition)))
                {
                    if (item.ToString() != "EMPTY")
                    {
                        Condition.Add(item.ToString());
                    }
                }
            }

            List<string> colNames = [.. mColName];

            ADSC.PossibleCondValues = Condition;
            ADSC.PossibleColumnValues = colNames;
            WhereConditions.Add(ADSC);

            ADSC.wCondition = wCond;
            ADSC.wTableColumn = wColName;
            ADSC.wOperator = wOper;
            ADSC.wValue = wValue.Replace("\"", string.Empty);
        }

        public void UpdateDSConditionColumns(List<string> mColName)
        {
            if (WhereConditions == null)
            {
                WhereConditions = [];
            }
            foreach (ActDSConditon ADSC in WhereConditions)
            {
                ADSC.PossibleColumnValues = mColName;
                if (!mColName.Contains(ADSC.wTableColumn))
                {
                    ADSC.wTableColumn = mColName[0];
                }
            }
        }
    }
}
