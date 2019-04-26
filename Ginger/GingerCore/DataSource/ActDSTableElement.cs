#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Collections.Generic;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCore.DataSource;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.InterfacesLib;
namespace GingerCore.Actions
{
    public class ActDSTableElement : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Data Source Action"; } }
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


        public override System.Drawing.Image Image { get { return Resources.Act; } }

        public override void Execute()
        {
            string outVal = "";
            switch (ControlAction)
            {
                case eControlAction.GetValue:
                    ValueExpression VE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    VE.Value = ValueExp;
                    // VE.ReplaceDataSource(ValueExp);
                    AddOrUpdateReturnParamActual("Output", VE.ValueCalculated);
                    break;
                case eControlAction.SetValue:
                    ValueExpression SVE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    SVE.Value = this.Value;

                    ValueExpression VEUpdate = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList, true, SVE.ValueCalculated);
                    VEUpdate.Value = ValueExp;
                    outVal = VEUpdate.ValueCalculated;
                    //VEUpdate.ReplaceDataSource(ValueExp);
                    break;
                case eControlAction.MarkAsDone:
                    ValueExpression VEMAD = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    VEMAD.Value = ValueExp;
                    outVal = VEMAD.ValueCalculated;
                    break;
                case eControlAction.RowCount:
                case eControlAction.AvailableRowCount:
                    ValueExpression VERC = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    VERC.Value = ValueExp;
                    // VE.ReplaceDataSource(ValueExp);
                    AddOrUpdateReturnParamActual("Output", VERC.ValueCalculated);
                    break;
                case eControlAction.ExportToExcel:
                    ValueExpression EVE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    EVE.Value = this.Value;

                    ValueExpression ETERC = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList,false, EVE.ValueCalculated);
                    ETERC.Value = ValueExp;
                    // VE.ReplaceDataSource(ValueExp);                    
                    if(ETERC.ValueCalculated=="The Export Excel can be *.xlsx only")
                    {
                        this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        Error = "The Export Excel can be *.xlsx only";
                    }
                    else
                        outVal = ETERC.ValueCalculated;
                    break;
                case eControlAction.DeleteRow:
                    ValueExpression veDel = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    veDel.Value = ValueExp;
                    outVal = veDel.ValueCalculated;
                    int rowCount = 0;
                    if(!string.IsNullOrEmpty(outVal) && !int.TryParse(outVal, out rowCount))
                    {
                        this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        Error = outVal;
                    }
                    break;
                default:
                    ValueExpression VEDR = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
                    VEDR.Value = ValueExp;
                    outVal = VEDR.ValueCalculated;
                    break;
            }
        }
        public new static partial class Fields
        {
            public static string DSName = "DSName";
            public static string DSTableName = "DSTableName";

            
            public static string ControlAction = "ControlAction";
            public static string Identifier = "Identifier";

            public static string KeyName = "KeyName";

            public static string iCustomized = "Customized";
            public static string iByQuery = "ByQuery";

            public static string QueryValue = "QueryValue";
            public static string ColSelectorValue = "ColSelectorValue";
            public static string WhereOperator = "WhereOperator";
            //public static string WhereColumnVal = "WhereColumnVal";
            public static string WhereProperty = "WhereProperty";

            public static string LocateColTitle = "LocateColTitle";
            public static string LocateRowType = "LocateRowType";
            public static string LocateRowValue = "LocateRowValue";

            public static string ByRowNum = "ByRowNum";
            public static string ByNextAvailable = "ByNextAvailable";            
            public static string ByWhere = "ByWhere";

            public static string WhereColSelector = "WhereColSelector";
            public static string WhereColumnTitle = "WhereColumnTitle";
            public static string WhereColumnValue = "WhereColumnValue";
            

            public static string ValueExp = "ValueExpression";

            public static string WhereConditions = "WhereConditions";

            public static string ExcelPath = "ExcelPath";
            public static string ExcelSheetName = "ExcelSheetName";

            //public static string InputValue = "InputValue";
            //public static string LocateY = "LocateColTitle";
        }

        [IsSerializedForLocalRepository]
        public eControlAction ControlAction { get; set; }

        [IsSerializedForLocalRepository]
        public string ValueExp { get; set; }

        //[IsSerializedForLocalRepository]        
        public ObservableList<ActDSConditon> WhereConditions = new ObservableList<ActDSConditon>();

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
            ExportToExcel
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


        //[IsSerializedForLocalRepository]
        public string QueryValue { get; set; }

        //public string InputValue { get; set; }
        //[IsSerializedForLocalRepository]
        public string ColSelectorValue { get; set; }

        //[IsSerializedForLocalRepository]
        public string DSName { get; set; }


        //[IsSerializedForLocalRepository]
        public string DSTableName { get; set; }

        //[IsSerializedForLocalRepository]
        public eRunColPropertyValue WhereProperty { get; set; }

        //[IsSerializedForLocalRepository]
        public eRunColOperator WhereOperator { get; set; }
                
        //[IsSerializedForLocalRepository]
        //public eRunActionOn RunActionOn { get; set; }
    

        //[IsSerializedForLocalRepository]
        public bool ByRowNum { get; set; }

        //[IsSerializedForLocalRepository]
        public bool ByNextAvailable { get; set; }
        
        //[IsSerializedForLocalRepository]
        public bool ByWhere { get; set; }

        //[IsSerializedForLocalRepository]
        public eRunColSelectorValue WhereColSelector { get; set; }

        //[IsSerializedForLocalRepository]
        public string WhereColumnTitle { get; set; }
        //[IsSerializedForLocalRepository]
        public string WhereColumnValue { get; set; }       
       
        //[IsSerializedForLocalRepository]
        public string LocateColTitle{ get; set; }
        //[IsSerializedForLocalRepository]
        public string LocateRowType { get; set; }
        //[IsSerializedForLocalRepository]
        public string LocateRowValue { get; set; }

        string mExcelPath;
        public string ExcelPath
        {
            get { return mExcelPath;  }
            set {

                if (mExcelPath != value)
                {
                    mExcelPath = value;
                    OnPropertyChanged(nameof(ExcelPath)); 
                }
            }
        }

        string mExcelSheetName;
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
            ObservableList<string> Condition = new ObservableList<string>();
            if (wCond != ActDSConditon.eCondition.EMPTY)
                foreach (ActDSConditon.eCondition item in Enum.GetValues(typeof(ActDSConditon.eCondition)))
                    if (item.ToString() != "EMPTY")
                        Condition.Add(item.ToString());
            List<string> colNames = new List<string>();
            foreach (string sColName in mColName)
                colNames.Add(sColName);
            ADSC.PossibleCondValues = Condition;
            ADSC.PossibleColumnValues = colNames;
            WhereConditions.Add(ADSC);
               
            ADSC.wCondition = wCond;            
            ADSC.wTableColumn = wColName;            
            ADSC.wOperator = wOper;
            ADSC.wValue = wValue;                          
        }

        public void UpdateDSConditionColumns(List<string> mColName)
        {
            foreach(ActDSConditon ADSC in WhereConditions)
            {
                ADSC.PossibleColumnValues = mColName;
                if (!mColName.Contains(ADSC.wTableColumn))
                    ADSC.wTableColumn = mColName[0];
            }            
        }
    }
}
