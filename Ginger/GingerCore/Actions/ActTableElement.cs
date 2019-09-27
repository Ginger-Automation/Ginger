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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Enums;

using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.Common;
using System.Linq;

namespace GingerCore.Actions
{
    public class ActTableElement : Act, IObsoleteAction
    {
        public override string ActionDescription { get { return "Table Element Action"; } }
        public override string ActionUserDescription { get { return "Create Java/PowerBulider/Windows Table Action "; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to create Java/Power Builder/Windows Table Action.");
            TBH.AddLineBreak();
            TBH.AddText("This Action will Enable you to Automate Steps related to Java, PB and Windows Tables ");
            TBH.AddLineBreak();
            TBH.AddBoldText("please Note: Table index's starts at 0, first Row/column will be always 0,0 ");
        }

        public new static partial class Fields
        {
            public static string ControlAction = "ControlAction";
            public static string RunActionOn = "RunActionOn";
            public static string ColSelectorValue = "ColSelectorValue";
            public static string WhereOperator = "WhereOperator";
            public static string WhereColumnVal = "WhereColumnVal";
            public static string WhereProperty = "WhereProperty";

            public static string LocateColTitle = "LocateColTitle";
            public static string LocateRowType = "LocateRowType";
            public static string LocateRowValue = "LocateRowValue";

            public static string ByRowNum = "ByRowNum";
            public static string ByRandRow = "ByRandRow";
            public static string BySelectedRow = "BySelectedRow";
            public static string ByWhere = "ByWhere";

            public static string WhereColSelector = "WhereColSelector";
            public static string WhereColumnTitle = "WhereColumnTitle";
            public static string WhereColumnValue = "WhereColumnValue";

            public static string LocateX = "LocateColSelector";
            public static string LocateY = "LocateColTitle";
        }

        public override string ActionEditPage { get { return "ActTableEditPage"; } }        
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                }
                return mPlatforms;
            }
        }

        public enum eTableAction
        {
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Get Value")]
            GetValue,
            GetText,
            Toggle,
            Click,
            ClickXY,
            [EnumValueDescription("Select UIF Date")]
            SelectDate,
            Type,
            WinClick,
            [EnumValueDescription("Async Click")]
            AsyncClick,
            GetRowCount,
            GetSelectedRow,
            IsCellEnabled,
            [EnumValueDescription("Is Cell Visible")]
            IsVisible,
            DoubleClick,     
            SetFocus,
            ActivateRow,
            ActivateCell,
            MousePressAndRelease,
            SendKeys,
            [EnumValueDescription("Is Checked")]
            IsChecked,
            [EnumValueDescription("Set Text")]
            SetText
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
            isSelected,
            [EnumValueDescription("TreePath")]
            TreePath,
            [EnumValueDescription("Text")]
            Text
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
            NotEndsWith
        }

        public enum eRunActionOn
        {
            [EnumValueDescription("On Cell with Row Num & Column Num")]
            OnCellRowNumColNum,
            [EnumValueDescription("On Cell with Row Num & Column Name")]
            OnCellRowNumColName,
            [EnumValueDescription("On Column By Name")]
            OnColumnByName,
            [EnumValueDescription("On Column By Num")]
            OnColumnByNum,
            [EnumValueDescription("On Row By Num")]
            OnRowByNum,
            [EnumValueDescription("On Column Name where matches Column Name")]
            OnColNameWhereColName
        }

        [IsSerializedForLocalRepository]
        public eRunColSelectorValue ColSelectorValue { get; set; }

        [IsSerializedForLocalRepository]
        public eRunColPropertyValue WhereProperty { get; set; }

        [IsSerializedForLocalRepository]
        public eRunColOperator WhereOperator { get; set; }

        [IsSerializedForLocalRepository]
        public eTableAction ControlAction { get; set; }

        [IsSerializedForLocalRepository]
        public eRunActionOn RunActionOn { get; set; }

        [IsSerializedForLocalRepository]
        public bool ByRowNum { get; set; }

        [IsSerializedForLocalRepository]
        public bool ByRandRow { get; set; }

        [IsSerializedForLocalRepository]
        public bool BySelectedRow { get; set; }

        [IsSerializedForLocalRepository]
        public bool ByWhere { get; set; }

        [IsSerializedForLocalRepository]
        public eRunColSelectorValue WhereColSelector { get; set; }

        [IsSerializedForLocalRepository]
        public string WhereColumnTitle { get; set; }
     
        public string WhereColumnValue
        {
             get
            {
                return GetInputParamValue("WhereColumnValue");
            }
            set
            {
                AddOrUpdateInputParamValue("WhereColumnValue", value);
            }    
                
        }            
        public string LocateY
        {
            get
            {
                return GetInputParamValue("ColSelectorValue");
            }
            set
            {
                AddOrUpdateInputParamValue("ColSelectorValue", value);
            }
        }
        [IsSerializedForLocalRepository]
        public string LocateColTitle{ get; set; }        
        public string LocateRowType
        {
            get
            {
                return GetInputParamValue("LocateRowType");
            }
            set
            {
                AddOrUpdateInputParamValue("LocateRowType", value);
            }
        }

     
        public string LocateRowValue 
        {
            get
            {
                return GetInputParamValue("LocateRowValue");
            }
            set
            {
                AddOrUpdateInputParamValue("LocateRowValue", value);
            }

        }       
        
        public string LocateX
        {
            get
            {
                return GetInputParamValue("LocateRowValue1");
            }
            set
            {
                AddOrUpdateInputParamValue("LocateRowValue1", value);
            }
        }
      

        public override String ToString()
        {
            return ControlAction.ToString();
        }

        public override String ActionType
        {
            get
            {
                return  ControlAction.ToString();
            }
        }

        public override List<ePlatformType> LegacyActionPlatformsList { get { return new List<ePlatformType>() { ePlatformType.Java }; } }
        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType actionPlatform)
        {
            if (actionPlatform == ePlatformType.Java)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfigUIElement = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActUIElement>(); });
            ActUIElement newActUIElement = mapConfigUIElement.CreateMapper().Map<Act, ActUIElement>(this);
            
            //TODO: Call below method for Java Platform only
            newActUIElement = ConvertJavaTableToActUITable(newActUIElement);

            return newActUIElement;
        }

        private ActUIElement ConvertJavaTableToActUITable(ActUIElement newActUIElement)
        {
            newActUIElement.ElementLocateBy = this.LocateBy;
            newActUIElement.ElementLocateValue = this.LocateValue;
            newActUIElement.ElementType = Amdocs.Ginger.Common.UIElement.eElementType.Table;

            newActUIElement.ElementAction = GetTableElementActionType(this.ControlAction);
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, this.ControlAction.ToString());
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ControlActionValue, this.Value);

            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.LocateRowType, this.LocateRowType);
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue, this.LocateRowType);

            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ByWhere, this.ByWhere.ToString());
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ByRowNum, this.ByRowNum.ToString());
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ByRandRow, this.ByRandRow.ToString());
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.BySelectedRow, this.BySelectedRow.ToString());
                        

            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle, this.LocateColTitle);
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue, Convert.ToString(this.ColSelectorValue));
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WhereColSelector, Convert.ToString(this.WhereColSelector));
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnTitle, this.WhereColumnTitle);
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, this.WhereColumnValue);
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WhereOperator, Convert.ToString(this.WhereOperator));
            newActUIElement.GetOrCreateInputParam(ActUIElement.Fields.WhereProperty, Convert.ToString(this.WhereProperty));



            if (this.ReturnValues.Count > 0)
            {
                if (this.ReturnValues[0].Param == "Actual")
                {
                    newActUIElement.ReturnValues[0].Param = "Actual0";
                }
            }

            return newActUIElement;
        }

        private string GetCheckedRadioButton()
        {
            var rdbTag = string.Empty;
            if (this.ByRowNum)
            {
                rdbTag = "RowNum";
            }
            else if (this.ByWhere)
            {
                rdbTag = "Where";
            }
            else if (this.ByRandRow)
            {
                rdbTag = "AnyRow";
            }
            else if (this.BySelectedRow)
            {
                rdbTag = this.BySelectedRow.ToString();
            }

            return rdbTag;
        }

        private ActUIElement.eElementAction GetTableElementActionType(eTableAction controlAction)
        {
            ActUIElement.eElementAction elementAction;
            switch (controlAction)
            {
                case eTableAction.SetValue:
                case eTableAction.SetFocus:
                case eTableAction.GetValue:
                case eTableAction.Toggle:
                case eTableAction.Click:
                case eTableAction.SelectDate:
                case eTableAction.Type:
                case eTableAction.WinClick:
                case eTableAction.AsyncClick:
                case eTableAction.IsCellEnabled:
                case eTableAction.IsVisible:
                case eTableAction.DoubleClick:
                case eTableAction.MousePressAndRelease:
                case eTableAction.SendKeys:
                case eTableAction.IsChecked:
                    elementAction = ActUIElement.eElementAction.TableCellAction;
                    break;

                case eTableAction.GetSelectedRow:
                case eTableAction.ActivateRow:
                    elementAction = ActUIElement.eElementAction.TableRowAction;
                    break;

                case eTableAction.GetRowCount:
                    elementAction = ActUIElement.eElementAction.TableAction;
                    break;
                default:
                    elementAction = ActUIElement.eElementAction.Unknown;
                    break;
            }

            return elementAction;
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActUIElement);
        }

        string IObsoleteAction.TargetActionTypeName()
        {
            ActUIElement actUIElement = new ActUIElement();
            return actUIElement.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.Java;
        }

        //TODO: Change icon to Java
        public override eImageType Image { get { return eImageType.Table; } }
    }
}
