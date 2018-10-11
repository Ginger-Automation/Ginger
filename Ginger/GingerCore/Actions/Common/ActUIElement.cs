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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Helpers;
using GingerCore.Properties;
using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.ComponentModel;
using System.Linq;

namespace GingerCore.Actions.Common
{
    public class ActUIElement : Act
    {
        // --------------------------------------------------------------------------------------------
        // TODO: remove after we take LocateBy, LocateValue from Act.cs
        // Creating dummy enum to avoid confusion until we delete the one in Act.cs

        // Put them back when you want to verify all is good.
        // Do not leave uncomment - since it will create problem when doing backup when 2 attrs are the exact same

        //public new enum eLocatorType
        //{
        //     DUMMY_Locate_By_DO_NOT_USE_The_New_Is_eLocateBy
        //}
        //public new float LocateBy;  // DUMMY_Locate_By_DO_NOT_USE_The_New_Is_ ElementLocateBy on this
        //public new float LocateValue;  // DUMMY_Locate_By_DO_NOT_USE_The_New_I ElementLocateValue on this
        // --------------------------------------------------------------------------------------------

        public const string EElementActionTypeGeneric = "Generic";

        public override string ActionDescription { get { return "UIElement Action"; } }
        public override string ActionUserDescription { get { return "UIElement Action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("UI Element Action");
        }

        public override string ActionEditPage { get { return "_Common.ActUIElementLib.ActUIElementEditPage"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return false; } }

        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);

                    //TODO: to see the Web impl uncommnet
                    // DO NOT remove comment before we have Selenium support this action and converter for old action
                    mPlatforms.Add(ePlatformType.Web);
                }
                return mPlatforms;
            }
        }

        public new static partial class Fields
        {
            public static string ElementType = "ElementType";
            public static string ElementAction = "ElementAction";
            public static string ElementLocateBy = "ElementLocateBy";
            public static string SubElementType = "SubElementType";
            public static string SubElementAction = "SubElementAction";
            public static string ElementLocateValue = "ElementLocateValue";
            public static string ElementLocateByXValue = "ElementLocateByXValue";
            public static string ElementLocateByYValue = "ElementLocateByYValue";
            public static string XCoordinate = "XCoordinate";
            public static string YCoordinate = "YCoordinate";
            public static string ValueToSelect = "ValueToSelect";
            public static string Value = "Value";
            public static string RowSelectorRadioParam = "RowSelectorRadioParam";

            //Used for Drag & Drop Action
            public static string TargetElementType = "TargetElementType";
            public static string TargetLocateBy = "TargetLocateBy";
            public static string TargetLocateValue = "TargetLocateValue";
            public static string SourceDragXY = "SourceDragXY";
            public static string TargetDropXY = "TargetDropXY";

            public static string DragDropType = "DragDropType";

            //used for Java
            public static string WaitforIdle = "WaitforIdle";

            //used for TableElementAction
            public static string ControlAction = "ControlAction";
            public static string ControlActionValue = "ControlActionValue";
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

            //used for MouseClickAndValidate
            public static string ClickType = "ClickType";
            public static string ValidationType = "ValidationType";
            public static string ValidationElement = "ValidationElement";
            public static string ValidationElementLocateBy = "ValidationElementLocateBy";
            public static string ValidationElementLocatorValue = "ValidationElementLocatorValue";
            public static string LoopThroughClicks = "LoopThroughClicks";

            //used for SendKeysAndValidate
            public static string DefineHandleAction = "DefineHandleAction";
            public static string HandleActionType = "HandleActionType";
            public static string HandleElementType = "HandleElementType";
            public static string HandleElementLocateBy = "HandleElementLocateBy";
            public static string HandleElementLocatorValue = "HandleElementLocatorValue";
            public static string ValidationElementValue = "ValidationElementValue";
        }

        // Fields Helper for specific action, will create AIV with param name based on enum

        public enum CheckBoxSetValue
        {
            Checked,
            UnChecked
        }
        public enum eRadioButtonValueType
        {
            [EnumValueDescription("Row Number")]
            RowNum,
            [EnumValueDescription("Any Row")]
            AnyRow,
            [EnumValueDescription("By Selected Row")]
            BySelectedRow,
            [EnumValueDescription("Where")]
            Where
        }

        public enum eSubElementType
        {
            [EnumValueDescription("HTML Table")]
            HTMLTable,
        }

        public enum eElementProperty
        {
            [EnumValueDescription("Enabled")]
            Enabled,
            [EnumValueDescription("Visible")]
            Visible,
            [EnumValueDescription("Color")]
            Color,
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("Tool Tip")]
            ToolTip,
            [EnumValueDescription("Type")]
            Type,
            [EnumValueDescription("Value")]
            Value,
            [EnumValueDescription("Date Time Value")]
            DateTimeValue,
            [EnumValueDescription("HTML")]
            HTML,
            [EnumValueDescription("List")]
            List,
        }

        public enum eElementAction
        {
            #region Generic Action Types
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("")]
            Unknown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Activate Hover")]
            Hover,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set as Visible")]
            Visible,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click")]
            Click,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Custom Attribute")]
            GetCustomAttribute,//keeping for backward support
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Async Click")]
            AsyncClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Win Click")]             // not here at all ?
            WinClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Mouse Click")]
            MouseClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Mouse Press/Release")]
            MousePressRelease,                              // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Double Click")]
            DoubleClick,                                     // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Java Script Click")]
            JavaScriptClick,                                 // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click and Validate")]
            ClickAndValidate,                                // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Keys and Validate")]
            SendKeysAndValidate,                             // JAVA ?
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Click X,Y")]
            ClickXY,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Text")]
            SetText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Text")]
            GetText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Value")]
            SetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Value")]
            GetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get X,Y")]
            GetXY,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Size")]
            GetSize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Open Drop Down")]
            OpenDropDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Close Drop Down")]
            CloseDropDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get All Values")]
            GetAllValues,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Attribute Value")]
            GetAttrValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Focus")]
            SetFocus,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Enabled")]
            IsEnabled,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Toggle")]
            Toggle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select")]
            Select,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Visible")]
            IsVisible,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Mandatory")]
            IsMandatory,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Exist")]
            Exist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Does not exist")]
            NotExist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Enabled")]
            Enabled,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Name")]
            GetName,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Dialog Text")]
            GetDialogText,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Accept Dialog")]
            AcceptDialog,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Dismiss Dialog")]
            DismissDialog,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Date")]
            SetDate,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Up")]
            ScrollUp,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Down")]
            ScrollDown,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Left")]
            ScrollLeft,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Scroll Right")]
            ScrollRight,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select By Index")]
            SelectByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Value By Index")]
            GetValueByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Item Count")]
            GetItemCount,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Keys")]
            SendKeys,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Send Key Press Release")]
            SendKeyPressRelease,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait Until Display")]
            [EnumValueExtDescriptionAttribute("Wait Until Display")]
            WaitUntilDisplay,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait")]
            Wait,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Wait Until Disappear")]
            WaitUntilDisappear,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Drag and Drop")]
            DragDrop,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Expand")]
            Expand,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Collapse")]
            Collapse,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Close")]
            CloseWindow,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Is Exist")]
            IsExist,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Maximize")]
            Maximize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Minimize")]
            Minimize,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Control Property")]
            GetControlProperty,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Initialize Editor Pane")]
            InitializeJEditorPane,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("JEditor Pane Element Action")]
            JEditorPaneElementAction,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Refresh")]
            Refresh,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Contexts")]
            GetContexts,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set Contexts")]
            SetContext,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Select From Dijit List")]
            SelectFromDijitList,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Right Click")]
            MouseRightClick,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Font")]
            GetFont,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Width")]
            GetWidth,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Height")]
            GetHeight,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Style")]
            GetStyle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Async Select From Drop Down (By Index)")]
            AsyncSelectFromDropDownByIndex,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Multi Clicks")]
            MultiClicks,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Multi Set Value")]
            MultiSetValue,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Get Window Title")]
            GetWindowTitle,
            [Description(EElementActionTypeGeneric)]
            [EnumValueDescription("Set As Disabled")]
            IsDisabled,
            [EnumValueDescription("Switch")]
            Switch,
            #endregion Generic Action Types

            #region TextBox Action Types
            [EnumValueDescription("Clear Value")]
            ClearValue,
            [EnumValueDescription("Get Text Length")]
            GetTextLength,
            #endregion TextBox Action Types

            #region Table Action Types
            [EnumValueDescription("Table Row Action")]
            TableRowAction,
            [EnumValueDescription("Table Cell Action")]
            TableCellAction,
            [EnumValueDescription("Table Action")]
            TableAction,
            [EnumValueDescription("Click Using Invoke Pattern")]
            InvokeClick,
            [EnumValueDescription("Click Using Legacy Pattern")]
            LegacyClick,
            [EnumValueDescription("Get Row Count")]
            GetRowCount,
            [EnumValueDescription("Get Selected Row")]
            GetSelectedRow,
            [EnumValueDescription("Draw Object")]
            DrawObject,
            [EnumValueDescription("Win Double Click")]
            winDoubleClick,
            [EnumValueDescription("Async Select")]
            AsyncSelect,
            [EnumValueDescription("Scroll to Element")]
            ScrollToElement,
            #endregion Table Action Types

            #region ComboBox related Types
            [EnumValueDescription("Set Selected Value By Index")]
            SetSelectedValueByIndex,
            [EnumValueDescription("Select By Text")]
            SelectByText,
            [EnumValueDescription("Get Valid Values")]
            GetValidValues,
            [EnumValueDescription("Get Selected Value")]
            GetSelectedValue,
            [EnumValueDescription("Is Value Populated")]
            IsValuePopulated,
            #endregion Usable Action Types

            [EnumValueDescription("Submit")]
            Submit,
            [EnumValueDescription("Run Java Script")]
            RunJavaScript,

            //Below should NOT be used- only kept for old action types support
            #region NOT TO USE Action Types
            [EnumValueDescription("Simple Click")]
            SimpleClick,
            [EnumValueDescription("Click At")]
            ClickAt,
            [EnumValueDescription("Long Click")]  // For Android
            LongClick,
            #endregion NOT TO USE Action Types
        }
        public enum eElementDragDropType
        {
            [EnumValueDescription("Using JavaScript")]
            DragDropJS,
            [EnumValueDescription("Using Selenium")]
            DragDropSelenium,
            [EnumValueDescription("Mouse Drag Drop")]
            MouseDragDrop,
        }

        eElementType mElementType;
        [IsSerializedForLocalRepository]
        public eElementType ElementType
        {
            get { return mElementType; }
            set
            {
                if (mElementType != value)
                {
                    mElementType = value;
                    OnPropertyChanged(nameof(ActUIElement.ElementType));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public eElementAction ElementAction { get; set; }

        [IsSerializedForLocalRepository]
        public eLocateBy ElementLocateBy { get; set; }

        [IsSerializedForLocalRepository]
        public eLocateBy TargetLocateBy { get; set; }

        [IsSerializedForLocalRepository]
        public eElementType TargetElementType { get; set; }

        [IsSerializedForLocalRepository]
        public eElementType HandleElementType { get; set; }

        [IsSerializedForLocalRepository]
        public eElementAction HandleActionType { get; set; }

        #region TableElementConfigs
        public enum eTableElementRunColSelectorValue
        {
            [EnumValueDescription("Column Title")]
            ColTitle,
            [EnumValueDescription("Column Name")]
            ColName,
            [EnumValueDescription("Column Number")]
            ColNum
        }

        public enum eTableElementRunColPropertyValue
        {
            [EnumValueDescription("Value")]
            Value,
            [EnumValueDescription("Path")]
            Path,
            [EnumValueDescription("HTML")]
            HTML,
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("ToolTip")]
            ToolTip,
            [EnumValueDescription("Enabled")]
            Enabled,
            [EnumValueDescription("Visible")]
            Visible,
            [EnumValueDescription("Type")]
            Type,
            [EnumValueDescription("List")]
            List,
            [EnumValueDescription("DateTimeValue")]
            DateTimeValue,
            [EnumValueDescription("isSelected")]
            isSelected
        }

        public enum eTableElementRunColOperator
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

        public enum eTableElementRunActionOn
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
        #endregion TableElementConfigs

        // The simple will be: Value = 'abc';
        // but since it is list can also support: X=100, Y=200
        // or multiple attrs: ID='a123' Text='abc' class='TBC'
        // We keep it seperated from Params, but will process Value for driver for each
        public ObservableList<UIElementPropertyValueLocator> PropertyValueLocatrs = new ObservableList<UIElementPropertyValueLocator>();
        public enum eWaitForIdle
        {
            [EnumValueDescription("0. None - default and recommended for most actions")]
            None,
            [EnumValueDescription("1. Short - Response expected in less than 1 second (test for idle every 0.1 second, max 30 seconds wait)")]
            Short,
            [EnumValueDescription("2. Medium - Between 1 to 5 seconds (test for idle every 0.5 second , max 60 seconds wait)")]
            Medium,
            [EnumValueDescription("3. Long - Between 5 to 30 seconds (test for idle every 1 second, max 2 Minutes wait)")]
            Long,
            [EnumValueDescription("4. Very Long - more than 30 seconds (test for idle every 5 seconds , max 5 minutes wait)")]
            VeryLong,
        }

        [IsSerializedForLocalRepository]
        public string ElementLocateValue
        {
            get
            {
                return GetOrCreateInputParam(Fields.ElementLocateValue).Value;
            }
            set
            {
                GetOrCreateInputParam(Fields.ElementLocateValue).Value = value;
            }
        }

        public enum eTableAction
        {
            [EnumValueDescription("Set Value")]
            SetValue,
            [EnumValueDescription("Get Value")]
            GetValue,
            Toggle,
            Click,
            Type,
            [EnumValueDescription("Win Click")]
            WinClick,
            [EnumValueDescription("Async Click")]
            AsyncClick,
            [EnumValueDescription("Get Row Count")]
            GetRowCount,
            [EnumValueDescription("Get Selected Row")]
            GetSelectedRow,
            [EnumValueDescription("Is Cell Enabled")]
            IsCellEnabled,
            [EnumValueDescription("Is Cell Visible")]
            IsVisible,
            [EnumValueDescription("Double Click")]
            DoubleClick,
            [EnumValueDescription("Set Focus")]
            SetFocus
        }

        // TODO: move Locate Value to here and remove from Act.cs

        // When recording we will collect some more info on the element to use as hint
        public string LocatorHint { get; set; }  // make it list of loc by loc value - which can find the elem or one string - will be used for auto fix

        public override String ActionType
        {
            get
            {
                return "UI Element Action. " + ElementAction;
            }
        }

        public override System.Drawing.Image Image
        {
            get
            {
                switch (ElementType)
                {
                    case eElementType.Button:
                        return Resources.ActButton;
                    case eElementType.TextBox:
                        return Resources.TextBox_16x16;
                    case eElementType.ComboBox:
                        return Resources.DropDownList_16x16;
                    case eElementType.List:
                        return Resources.List_16x16;
                    case eElementType.CheckBox:
                        return Resources.CheckBox_16x16;
                    case eElementType.Image:
                        return Resources.Image_16x16;
                    case eElementType.Label:
                        return Resources.Label_16x16;
                    case eElementType.MenuItem:
                        return Resources.MenuItem_16x16;
                    case eElementType.MenuBar:
                        return Resources.MenuBar_16x16;
                    case eElementType.RadioButton:
                        return Resources.RadioButton_16x16;
                    case eElementType.TreeView:
                        return Resources.TreeView_16x16;
                    case eElementType.Window:
                        return Resources.Window_16x16;
                    case eElementType.Table:
                        return Resources.Table;
                    default:
                        return Resources.Window_16x16;  // FIXME
                }
            }
        }

        // ------------------------------------------------------

        // For each special Action/Locator which have more than just value we create function to get the data so driver can use it easily, see sample of By X Y in Selenium
        // Locate By XY

        //TODO: use the AIV list of items
        public void GetLocateByXYValuesForDriver(out double X, out double Y)
        {
            // split the Value, do not create new param
            // all locate value need to be combined into string
            if (string.IsNullOrEmpty(ElementLocateValueForDriver))
            {
                X = 0;
                Y = 0;
            }
            else
            {
                string[] xy = ElementLocateValueForDriver.Split(',');
                if ((xy != null) && (xy.Count() > 1))
                {
                    if (!double.TryParse(xy[0].Split('=')[1], out X))
                        X = 0;
                    if (!double.TryParse(xy[1].Split('=')[1], out Y))
                        Y = 0;
                }
                else
                {
                    X = 0;
                    Y = 0;
                }
            }
        }

        //Pack
        public void GetLocateByXYValues(out double X, out double Y)
        {
            // split the Value, do not create new param
            // all locate value need to be combined into string
            if (string.IsNullOrEmpty(ElementLocateValue))
            {
                X = 0;
                Y = 0;
            }
            else
            {
                string[] xy = ElementLocateValue.Split(',');
                if ((xy != null) && (xy.Count() > 1))
                {
                    if (!double.TryParse(xy[0].Split('=')[1], out X))
                        X = 0;
                    if (!double.TryParse(xy[1].Split('=')[1], out Y))
                        Y = 0;
                }
                else
                {
                    X = 0;
                    Y = 0;
                }
            }
        }

        //Parse
        public void SetLocateByXYValues(double X, double Y)
        {
            ElementLocateValue = "X=" + X + ",Y=" + Y;
        }

        //TOOD: impl in ActionEditPage to show the output grid or show no output values
        // return per action if to display output values grid
        //Move to Act.cs base
        public bool GetDisplayOutputValue()
        {

            //TODO: fill for all
            switch (ElementAction)
            {
                case eElementAction.Click:
                case eElementAction.Collapse:
                case eElementAction.Expand:
                    return false;
                case eElementAction.GetValue:
                    return true;
                default:
                    return false;
            }
        }

        internal Drivers.CommunicationProtocol.PayLoad GetPayLoad()
        {
            PayLoad PL = new PayLoad("UIElementAction");
            PL.AddValue(this.ElementLocateBy.ToString());
            PL.AddValue(GetOrCreateInputParam(Fields.ElementLocateValue).ValueForDriver); // Need Value for driver
            PL.AddValue(this.ElementType.ToString());
            PL.AddValue(this.ElementAction.ToString());
            // Make it generic function in Act.cs to be used by other actions
            List<PayLoad> PLParams = new List<PayLoad>();
            foreach (ActInputValue AIV in this.InputValues)
            {
                if (!string.IsNullOrEmpty(AIV.Value))
                {
                    PayLoad AIVPL = new PayLoad("AIV", AIV.Param, AIV.ValueForDriver);
                    PLParams.Add(AIVPL);
                }
            }
            PL.AddListPayLoad(PLParams);
            PL.ClosePackage();

            return PL;
        }

        public override ActionDetails Details
        {
            //TODO: create sepeare class ActUIElementDetails - so it will do lazy loading/calc
            // for now below will do, just to show the concept in ActionGrid
            get
            {
                // We create a customized user friendly action details for actions grid and report
                ActionDetails d = base.Details;
                if (this.ElementLocateBy == eLocateBy.ByXY)
                {
                    double X;
                    double Y;
                    this.GetLocateByXYValuesForDriver(out X, out Y);
                    d.Info = "Locate By X,Y: X= " + X + ", Y=" + Y;
                }
                else
                {
                    d.Info = "Locate By: " + ElementLocateBy + " - " + ElementLocateValue;
                }

                if (Value != null)
                {
                    d.Info += " Value: " + Value;
                }

                // return params order by priority and improtence
                d.Params.Clear();
                d.Params.Add(new ActionParamInfo() { Param = "Type", Value = this.ElementType.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Locate By", Value = this.ElementLocateBy.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Action", Value = this.ElementAction.ToString() });   // get the neum desc
                d.Params.Add(new ActionParamInfo() { Param = "Val", Value = this.Value });   // get the neum desc

                // TODO: push others
                return d;
            }
        }

        public string ElementLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.ElementLocateValue);
            }
        }
        public string TargetLocateValue
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
            }
        }
        public string TargetLocateValueForDriver
        {
            get
            {
                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
            }
        }
    }
}