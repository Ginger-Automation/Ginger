//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.Drivers.CommonLib;
//using GingerCoreNET.Drivers.CommunicationProtocol;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using System;
//using System.Collections.Generic;

//namespace GingerCoreNET.Drivers.CommonActionsLib
//{
//    public class ActUIElement : Acta
//    {
//        // --------------------------------------------------------------------------------------------
//        // TODO: remove after we take LocateBy, LocateValue from Act.cs
//        // Creating dummy enum to avoid confusion until we delete the one in Act.cs

//        // Put them back when you want to verify all is good.
//        // Do not leave uncomment - since it will create problem when doing backup when 2 attrs are the exact same

//        //public new enum eLocatorType
//        //{
//        //     DUMMY_Locate_By_DO_NOT_USE_The_New_Is_eLocateBy
//        //}
//        //public new float LocateBy;  // DUMMY_Locate_By_DO_NOT_USE_The_New_Is_ ElementLocateBy on this
//        //public new float LocateValue;  // DUMMY_Locate_By_DO_NOT_USE_The_New_I ElementLocateValue on this
//        // --------------------------------------------------------------------------------------------

//        public override string ActionDescription { get { return "UIElement Action"; } }
//        public override string ActionUserDescription { get { return "UIElement Action"; } }
        
//        public override string ActionEditPage { get { return "_Common.ActUIElementLib.ActUIElementEditPage"; } }

//        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

//        public override bool ValueConfigsNeeded { get { return false; } }

//        public override List<Platform.ePlatformType> Platforms
//        {
//            get
//            {
//                if (mPlatforms.Count == 0)
//                {
//                    //TODO: FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//                    //mPlatforms.Add(GingerCore.Platforms.Platform.eType.AndroidDevice);
//                    //mPlatforms.Add(GingerCore.Platforms.Platform.eType.Java);
//                    //mPlatforms.Add(GingerCore.Platforms.Platform.eType.PowerBuilder);
//                    //mPlatforms.Add(GingerCore.Platforms.Platform.eType.Windows);

//                    ////TODO: to see the Web impl uncomment
//                    //// DO NOT remove comment before we have Selenium support this action and converter for old action
//                    //mPlatforms.Add(GingerCore.Platforms.Platform.eType.Web);
//                }
//                return mPlatforms;
//            }
//        }
               
//        public static partial class Fields
//        {
//            public static string ElementType = "ElementType";
//            public static string ElementAction = "ElementAction";
//            public static string ElementLocateBy = "ElementLocateBy";
//            public static string ElementLocateValue = "ElementLocateValue";
//            public static string ElementLocateByXValue = "ElementLocateByXValue";
//            public static string ElementLocateByYValue = "ElementLocateByYValue";
//            public static string XCoordinate = "XCoordinate";
//            public static string YCoordinate = "YCoordinate";
//            public static string ValueToSelect = "ValueToSelect";
//            public static string Value = "Value";
//            public static string RowSelectorRadioParam = "RowSelectorRadioParam";

//            //Used for Drag & Drop Action
//            public static string TargetElementType = "TargetElementType";
//            public static string TargetLocateBy = "TargetLocateBy";
//            public static string TargetLocateValue = "TargetLocateValue";
            
//            //used for Java
//            public static string WaitforIdle = "WaitforIdle";

//            //used for TableElementAction
//            public static string ControlAction = "ControlAction";
//            public static string ControlActionValue = "ControlActionValue";
//            public static string RunActionOn = "RunActionOn";
//            public static string ColSelectorValue = "ColSelectorValue";
//            public static string WhereOperator = "WhereOperator";
//            public static string WhereColumnVal = "WhereColumnVal";
//            public static string WhereProperty = "WhereProperty";

//            public static string LocateColTitle = "LocateColTitle";
//            public static string LocateRowType = "LocateRowType";
//            public static string LocateRowValue = "LocateRowValue";

//            public static string ByRowNum = "ByRowNum";
//            public static string ByRandRow = "ByRandRow";
//            public static string BySelectedRow = "BySelectedRow";
//            public static string ByWhere = "ByWhere";

//            public static string WhereColSelector = "WhereColSelector";
//            public static string WhereColumnTitle = "WhereColumnTitle";
//            public static string WhereColumnValue = "WhereColumnValue";

//            //used for MouseClickAndValidate
//            public static string ClickType = "ClickType";
//            public static string ValidationType = "ValidationType";
//            public static string ValidationElement = "ValidationElement";
//            public static string ValidationElementLocateBy = "ValidationElementLocateBy";
//            public static string ValidationElementLocatorValue = "ValidationElementLocatorValue";
//            public static string LoopThroughClicks = "LoopThroughClicks";
//        }

//        // Fields Helper for specific action, will create AIV with param name based on enum

//        public enum CheckBoxSetValue
//        {
//            Checked,
//            UnChecked
//        }
//        public enum eRadioButtonValueType
//        {
//            [EnumValueDescription("Row Number")]
//            RowNum,
//            [EnumValueDescription("Any Row")]
//            AnyRow,
//            [EnumValueDescription("By Selected Row")]
//            BySelectedRow,
//            [EnumValueDescription("Where")]
//            Where
//        }
         
//        public enum eLocateBy
//        {
//            [EnumValueDescription("NA")]
//            NA,
//            [EnumValueDescription("")]
//            Unknown, 
//            [EnumValueDescription("By ID")]
//            ByID,
//            [EnumValueDescription("By Name")]
//            ByName,
//            [EnumValueDescription("By CSS")]
//            ByCSS,
//            [EnumValueDescription("By XPath")]
//            ByXPath,
//            [EnumValueDescription("By RelXPath")]
//            ByRelXPath,
//            [EnumValueDescription("By X,Y")]
//            ByXY,
//            [EnumValueDescription("By Container Name")]
//            ByContainerName,
//            [EnumValueDescription("By Href")]
//            ByHref,
//            [EnumValueDescription("By Link Text")]
//            ByLinkText,
//            [EnumValueDescription("By Value")]
//            ByValue,
//            [EnumValueDescription("By Index")]
//            ByIndex,
//            [EnumValueDescription("By Class name")]
//            ByClassName, //Android, UI Automation
//            [EnumValueDescription("By AutomationId")]
//            ByAutomationID,
//            [EnumValueDescription("By Localized Control Type")]
//            ByLocalizedControlType,
//            [EnumValueDescription("By Multiple Properties")]
//            ByMulitpleProperties,
//            [EnumValueDescription("By Bounding Rectangle")]
//            ByBoundingRectangle,
//            [EnumValueDescription("Is Enabled")]
//            IsEnabled,
//            [EnumValueDescription("Is Off Screen")]
//            IsOffscreen,
//            [EnumValueDescription("By Title")]
//            ByTitle,
//            [EnumValueDescription("By CaretPosition")]
//            ByCaretPosition,
//            [EnumValueDescription("By URL")]
//            ByUrl,
//            [EnumValueDescription("By ng-model")]
//            ByngModel,
//            [EnumValueDescription("By ng-Repeat")]
//            ByngRepeat,
//            [EnumValueDescription("By ng-Bind")]
//            ByngBind,
//            [EnumValueDescription("By ng-SelectedOption")]
//            ByngSelectedOption,
//            [EnumValueDescription("By Resource ID")]
//            ByResourceID,
//            [EnumValueDescription("By Content Description")]
//            ByContentDescription,
//            [EnumValueDescription("By Text")]
//            ByText,
//            [EnumValueDescription("By Elements Repository")]
//            ByElementsRepository,
//            [EnumValueDescription("By Model Name")]
//            ByModelName,
//        }

//        public enum eElementType
//        {
//            [EnumValueDescription("")]
//            Unknown,  
//            [EnumValueDescription("Text Box")]
//            TextBox,       
//            Button,
//            [EnumValueDescription("Combo Box/Drop Down")]
//            ComboBox,     // HTML Input Select
//            [EnumValueDescription("Combo Box Item/Drop Down Option")]
//            ComboBoxOption,    // HTML Input Select
//            List,
//            ListItem, 
//            [EnumValueDescription("Table Item")]
//            TableItem,
//            [EnumValueDescription("Radio Button")]
//            RadioButton,
//            Table,  
//            CheckBox,
//            Image,
//            Label,
//            [EnumValueDescription("Menu Item")]
//            MenuItem,
//            [EnumValueDescription("Menu Bar")]
//            MenuBar,
//            TreeView,
//            Window,
//            HyperLink,
//            ScrollBar,
//            Iframe,
//            Text,
//            //HTML Elements
//            Div,
//            Span,
//            Form,
//        }

//        public enum eElementAction
//        {
//            //those are all available UI Control actions, each driver will filter what can be done based on the type and what it is supporting

//            //Below should be used
//            #region Usable Action Types
//            [EnumValueDescription("")]
//            Unknown,
//            [EnumValueDescription("Click")]
//            Click,
//            [EnumValueDescription("Async Click")]
//            AsyncClick,
//            [EnumValueDescription("Win Click")]
//            WinClick,
//            [EnumValueDescription("Mouse Click")]
//            MouseClick,
//            [EnumValueDescription("Mouse Press/Release")]
//            MousePressRelease,
//            [EnumValueDescription("Double Click")]
//            DoubleClick,
//            [EnumValueDescription("Java Script Click")]
//            JavaScriptClick,
//            [EnumValueDescription("Click and Validate")]
//            ClickAndValidate,
//            [EnumValueDescription("Click X,Y")]
//            ClickXY,
//            [EnumValueDescription("Set Text")]
//            SetText,
//            [EnumValueDescription("Get Text")]
//            GetText,
//            [EnumValueDescription("Set Value")]
//            SetValue,
//            [EnumValueDescription("Get Value")]
//            GetValue,
//            [EnumValueDescription("Get X,Y")]
//            GetXY,
//            [EnumValueDescription("Get Size")]
//            GetSize,
//            [EnumValueDescription("Open Drop Down")]
//            OpenDropDown,
//            [EnumValueDescription("Close Drop Down")]
//            CloseDropDown,
//            [EnumValueDescription("Get All Values")]
//            GetAllValues,
//            [EnumValueDescription("Get Attribute Value")]
//            GetAttrValue,           
//            [EnumValueDescription("Set Focus")]
//            SetFocus,
//            [EnumValueDescription("Is Enabled")]
//            IsEnabled,
//            [EnumValueDescription("Toggle")]
//            Toggle,
//            [EnumValueDescription("Select")]
//            Select,
//            [EnumValueDescription("Is Visible")]
//            IsVisible,
//            [EnumValueDescription("Exist")]
//            Exist,
//            [EnumValueDescription("Does not exist")]
//            NotExist,
//            [EnumValueDescription("Enabled")]
//            Enabled,
//            [EnumValueDescription("Get Name")]
//            GetName,
//            [EnumValueDescription("Get Dialog Text")]
//            GetDialogText,
//            [EnumValueDescription("Accept Dialog")]
//            AcceptDialog,
//            [EnumValueDescription("Dismiss Dialog")]
//            DismissDialog,
//            [EnumValueDescription("Select UIF Date")]
//            SetDate,
//            [EnumValueDescription("Scroll Up")]
//            ScrollUp,
//            [EnumValueDescription("Scroll Down")]
//            ScrollDown,
//            [EnumValueDescription("Scroll Left")]
//            ScrollLeft,
//            [EnumValueDescription("Scroll Right")]
//            ScrollRight,
//            [EnumValueDescription("Select By Index")]
//            SelectByIndex,
//            [EnumValueDescription("Get Value By Index")]
//            GetValueByIndex,
//            [EnumValueDescription("Get Item Count")]
//            GetItemCount,
//            [EnumValueDescription("Send Keys")]
//            SendKeys,
//            [EnumValueDescription("Send Key Press Release")]
//            SendKeyPressRelease,
//            [EnumValueDescription("Wait Until Display")]
//            WaitUntilDisplay,
//            [EnumValueDescription("Wait Until Disappear")]
//            WaitUntilDisappear,
//            [EnumValueDescription("Drag and Drop")]
//            DragDrop,
//            Expand,
//            Collapse,
//            Hover,
//            [EnumValueDescription("Switch")]
//            Switch,
//            [EnumValueDescription("Close")]
//            CloseWindow,
//            [EnumValueDescription("Is Exist")]
//            IsExist,
//            [EnumValueDescription("Maximize")]
//            Maximize,
//            [EnumValueDescription("Minimize")]
//            Minimize,
//            [EnumValueDescription("Get Control Property")]
//            GetControlProperty,
//            // Table Actions
//            [EnumValueDescription("Table Row Action")]
//            TableRowAction,
//            [EnumValueDescription("Table Cell Action")]
//            TableCellAction,
//            [EnumValueDescription("Table Action")]
//            TableAction,
//            [EnumValueDescription("Click Using Invoke Pattern")]
//            InvokeClick,
//            [EnumValueDescription("Click Using Legacy Pattern")]
//            LegacyClick,
//            #endregion Usable Action Types

//            //Below should NOT be used- only kept for old action types support
//            #region NOT TO USE Action Types
//            [EnumValueDescription("Simple Click")]
//            SimpleClick,
//            [EnumValueDescription("Click At")]
//            ClickAt,
//            [EnumValueDescription("Long Click")]  // For Android
//            LongClick,
//            #endregion NOT TO USE Action Types
//        }
        
//        public eElementType ElementType { get; set; }
        
//        public eElementAction ElementAction { get; set; }
        
//        public eLocateBy ElementLocateBy { get; set; }
        
//        public eLocateBy TargetLocateBy { get; set; }
        
//        public eElementType TargetElementType { get; set; }
        
//        #region TableElementConfigs
//        public enum eTableElementRunColSelectorValue
//        {
//            [EnumValueDescription("Column Title")]
//            ColTitle,
//            [EnumValueDescription("Column Name")]
//            ColName,
//            [EnumValueDescription("Column Number")]
//            ColNum
//        }
        
//        public enum eTableElementRunColPropertyValue
//        {
//            [EnumValueDescription("Value")]
//            Value,
//            [EnumValueDescription("Path")]
//            Path,
//            [EnumValueDescription("HTML")]
//            HTML,
//            [EnumValueDescription("Text")]
//            Text,
//            [EnumValueDescription("ToolTip")]
//            ToolTip,
//            [EnumValueDescription("Enabled")]
//            Enabled,
//            [EnumValueDescription("Visible")]
//            Visible,
//            [EnumValueDescription("Type")]
//            Type,
//            [EnumValueDescription("List")]
//            List,
//            [EnumValueDescription("DateTimeValue")]
//            DateTimeValue,
//            [EnumValueDescription("isSelected")]
//            isSelected
//        }

//        public enum eTableElementRunColOperator
//        {
//            [EnumValueDescription("Equals")]
//            Equals,
//            [EnumValueDescription("Not Equals")]
//            NotEquals,
//            [EnumValueDescription("Contains")]
//            Contains,
//            [EnumValueDescription("Not Contains")]
//            NotContains,
//            [EnumValueDescription("Starts With")]
//            StartsWith,
//            [EnumValueDescription("Not Starts With")]
//            NotStartsWith,
//            [EnumValueDescription("Ends With")]
//            EndsWith,
//            [EnumValueDescription("Not Ends With")]
//            NotEndsWith
//        }

//        public enum eTableElementRunActionOn
//        {
//            [EnumValueDescription("On Cell with Row Num & Column Num")]
//            OnCellRowNumColNum,
//            [EnumValueDescription("On Cell with Row Num & Column Name")]
//            OnCellRowNumColName,
//            [EnumValueDescription("On Column By Name")]
//            OnColumnByName,
//            [EnumValueDescription("On Column By Num")]
//            OnColumnByNum,
//            [EnumValueDescription("On Row By Num")]
//            OnRowByNum,
//            [EnumValueDescription("On Column Name where matches Column Name")]
//            OnColNameWhereColName

//        }
//        #endregion TableElementConfigs

//        // The simple will be: Value = 'abc';
//        // but since it is list can also support: X=100, Y=200
//        // or multiple attrs: ID='a123' Text='abc' class='TBC'
//        // We keep it seperated from Params, but will process Value for driver for each
//        public ObservableList<UIElementPropertyValueLocator> PropertyValueLocatrs = new ObservableList<UIElementPropertyValueLocator>();
//        public enum eWaitForIdle
//        {
//            [EnumValueDescription("0. None - default and recommended for most actions")]
//            None,
//            [EnumValueDescription("1. Short - Response expected in less than 1 second (test for idle every 0.1 second, max 30 seconds wait)")]
//            Short,
//            [EnumValueDescription("2. Medium - Between 1 to 5 seconds (test for idle every 0.5 second , max 60 seconds wait)")]
//            Medium,
//            [EnumValueDescription("3. Long - Between 5 to 30 seconds (test for idle every 1 second, max 2 Minutes wait)")]
//            Long,
//            [EnumValueDescription("4. Very Long - more than 30 seconds (test for idle every 5 seconds , max 5 minutes wait)")]
//            VeryLong,
//        }
        
//        public string ElementLocateValue
//        {
//            get
//            {
//                return GetOrCreateInputParam(Fields.ElementLocateValue).Value;
//            }
//            set
//            {
//                GetOrCreateInputParam(Fields.ElementLocateValue).Value = value;
//            }
//        }

//        public enum eTableAction
//        {
//            SetValue,            
//            GetValue,
//            Toggle,
//            Click,
//            Type,
//            WinClick,           
//            AsyncClick,
//            GetRowCount,
//            GetSelectedRow,
//            [EnumValueDescription("Is Cell Enabled")]
//            IsCellEnabled,
//            [EnumValueDescription("Is Cell Visible")]
//            IsVisible,
//            DoubleClick,
//            SetFocus
//        }
        
//        // TODO: move Locate Value to here and remove from Act.cs
        
//        // When recording we will collect some more info on the element to use as hint
//        public string LocatorHint { get; set; }  // make it list of loc by loc value - which can find the elem or one string - will be used for auto fix
        
//        public override String ActionType
//        {
//            get
//            {
//                return "UI Element Action. " + ElementAction;
//            }
//        }

//        // ------------------------------------------------------

//        // For each special Action/Locator which have more than just value we create function to get the data so driver can use it easily, see sample of By X Y in Selenium
//        // Locate By XY

//        //TODO: use the AIV list of items
//        public void GetLocateByXYValuesForDriver(out double X, out double Y)
//        {
//            // split the Value, do not create new param
//            // all locate value need to be combined into string
//            if (string.IsNullOrEmpty(ElementLocateValueForDriver))
//            {
//                X = 0;
//                Y = 0;
//            }
//            else
//            {
//                string[] xy = ElementLocateValueForDriver.Split(',');

//                X = int.Parse(xy[0].Replace("X=", ""));
//                Y = int.Parse(xy[1].Replace("Y=", ""));
//            }
//        }

//        //Pack
//        public void GetLocateByXYValues(out double X, out double Y)
//        {
//            // split the Value, do not create new param
//            // all locate value need to be combined into string
//            if (string.IsNullOrEmpty(ElementLocateValue))
//            {
//                X = 0;
//                Y = 0;
//            }
//            else
//            {
//                string[] xy = ElementLocateValue.Split(',');

//                X = int.Parse(xy[0].Split('=')[1]);
//                Y = int.Parse(xy[1].Split('=')[1]);
//            }

//        }

//        //Parse
//        public void SetLocateByXYValues(double X, double Y)
//        {
//            ElementLocateValue = "X=" + X + ",Y=" + Y;
//        }

//        // ------------------------------------------------------
        
//        //TOOD: impl in ActionEditPage to show the output grid or show no output values
//        // return per action if to display output values grid
//        //Move to Act.cs base
//        public bool GetDisplayOutputValue()
//        {
//            //TODO: fill for all
//            switch (ElementAction)
//            {
//                case eElementAction.Click:
//                case eElementAction.Collapse:
//                case eElementAction.Expand:
//                    return false;
//                case eElementAction.GetValue:
//                    return true;
//                default:
//                    return false;
//            }
//        }

//        internal Drivers.CommunicationProtocol.NewPayLoad GetPayLoad()
//        {
//            NewPayLoad PL = new NewPayLoad("UIElementAction");
//            PL.AddValue(this.ElementLocateBy.ToString());
//            PL.AddValue(GetOrCreateInputParam(Fields.ElementLocateValue).ValueForDriver); // Need Value for driver
//            PL.AddValue(this.ElementType.ToString());
//            PL.AddValue(this.ElementAction.ToString());
//            // Make it generic function in Act.cs to be used by other actions
//            List<NewPayLoad> PLParams = new List<NewPayLoad>();
//            foreach (ActInputValue AIV in this.InputValues)
//            {
//                if (!string.IsNullOrEmpty(AIV.Value))
//                {
//                    NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.ValueForDriver);                                        
//                    PLParams.Add(AIVPL);
//                }
//            }
//            PL.AddListPayLoad(PLParams);
//            PL.ClosePackage();
//            return PL;
//        }

//        public override ActionDetails Details
//        {
//            //TODO: create sepeare class ActUIElementDetails - so it will do lazy loading/calc
//            // for now below will do, just to show the concept in ActionGrid
//            get
//            {
//                // We create a customized user friendly action details for actions grid and report
//                ActionDetails d = base.Details;
//                if (this.ElementLocateBy == eLocateBy.ByXY)
//                {
//                    double X;
//                    double Y;
//                    this.GetLocateByXYValuesForDriver(out X, out Y);
//                    d.Info = "Locate By X,Y: X= " + X + ", Y=" + Y;
//                }
//                else
//                {
//                    d.Info = "Locate By: " + ElementLocateBy + " - " + ElementLocateValue;
//                }

//                if (Value != null)
//                {
//                    d.Info += " Value: " + Value;
//                }

//                // return params order by priority and improtence
//                d.Params.Clear();
//                d.Params.Add(new ActionParamInfo() { Param = "Type", Value = this.ElementType.ToString() });   // get the neum desc
//                d.Params.Add(new ActionParamInfo() { Param = "Locate By", Value = this.ElementLocateBy.ToString() });   // get the neum desc
//                d.Params.Add(new ActionParamInfo() { Param = "Action", Value = this.ElementAction.ToString() });   // get the neum desc
//                d.Params.Add(new ActionParamInfo() { Param = "Val", Value = this.Value });   // get the neum desc

//                // TODO: push others

//                return d;
//            }
//        }
        
//        public string ElementLocateValueForDriver
//        {
//            get
//            {
//                return this.GetInputParamCalculatedValue(Fields.ElementLocateValue);
//            }
//        }
//        public string TargetLocateValue
//        {
//            get
//            {
//                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
//            }
//        }
//        public string TargetLocateValueForDriver
//        {
//            get
//            {
//                return this.GetInputParamCalculatedValue(Fields.TargetLocateValue);
//            }
//        }
//    }
//}