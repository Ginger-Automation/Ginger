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

using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.ActionsLib
{
    public class ActInfo
    {

        public enum eLocateBy
        {

            NA,
            Unknown,
            POMElement,
            ByID,
            ByName,
            ByCSS,
            ByXPath,
            ByRelXPath,
            ByXY,
            ByContainerName,
            ByHref,
            ByLinkText,
            ByValue,
            ByIndex,
            ByClassName,
            ByAutomationID,
            ByLocalizedControlType,
            ByMulitpleProperties,
            ByBoundingRectangle,
            IsEnabled,
            IsOffscreen,
            ByTitle,
            ByCaretPosition,
            ByUrl,
            ByngModel,
            ByngRepeat,
            ByngBind,
            ByngSelectedOption,
            ByResourceID,
            ByContentDescription,
            ByText,
            ByElementsRepository,
            ByModelName,
            ByCSSSelector,
        }

        public enum eElementDragDropType
        {
        
            DragDropJS,
            
            DragDropSelenium,
           
            MouseDragDrop,
        }
        public enum eElementAction
        {
            #region Generic Action Types

            Unknown,

            Hover,

            Visible,

            Click,

            GetCustomAttribute,//keeping for backward support

            AsyncClick,
            // not here at all ?
            WinClick,

            MouseClick,

            MousePressRelease,                              // JAVA ?

            DoubleClick,                                     // JAVA ?

            JavaScriptClick,                                 // JAVA ?

            ClickAndValidate,                                // JAVA ?

            SendKeysAndValidate,                             // JAVA ?

            ClickXY,

            SetText,

            GetText,

            SetValue,

            GetValue,

            GetXY,
            GetSize,
            OpenDropDown,
            SelectandValidate,
            CloseDropDown,
            GetAllValues,
            GetAttrValue,
            SetFocus,
            IsEnabled,
            Toggle,
            Select,
            IsVisible,
            IsMandatory,
            Exist,
            NotExist,
            Enabled,
            GetName,
            GetDialogText,
            AcceptDialog,
            DismissDialog,
            SetDate, ScrollUp,
            ScrollDown,
            ScrollLeft,
            ScrollRight,
            SelectByIndex,
            GetValueByIndex,
            GetItemCount,
            SendKeys,
            SendKeyPressRelease,
            WaitUntilDisplay,
            Wait,
            WaitUntilDisappear,
            DragDrop,
            Expand,
            Collapse,
            CloseWindow,
            IsExist,
            Maximize,
            Minimize,
            GetControlProperty,
            InitializeJEditorPane,
            JEditorPaneElementAction,
            Refresh,
            GetContexts,
            SetContext,
            SelectFromDijitList,
            MouseRightClick,
            GetFont,
            GetWidth,
            GetHeight,
            GetStyle,
            AsyncSelectFromDropDownByIndex,
            MultiClicks,
            MultiSetValue,
            GetWindowTitle,
            IsDisabled,
            Switch,
            DoubleClickXY,
            SendKeysXY,
            #endregion Generic Action Types

            #region TextBox Action Types

            ClearValue,
            GetTextLength,
            #endregion TextBox Action Types

            #region Table Action Types
            TableRowAction,
            TableCellAction,
            TableAction,
            InvokeClick,
            LegacyClick,
            GetRowCount,
            GetSelectedRow,
            DrawObject,
            winDoubleClick,
            AsyncSelect,
            ScrollToElement,
            #endregion Table Action Types

            #region ComboBox related Types
            SetSelectedValueByIndex,
            SelectByText,
            GetValidValues,
            GetSelectedValue,
            IsValuePopulated,
            #endregion Usable Action Types

            Submit,
            RunJavaScript,
           
        }


        public enum eElementType
        {
            
            Unknown,
            TextBox,
            Button,
            Dialog,
            ComboBox,
            ComboBoxOption,    // HTML Input Select
            List,
            ListItem,
            TableItem,
            RadioButton,
            Table,
            CheckBox,
            Image,
            Label,
            MenuItem,           
            MenuBar,
            TreeView,
            Window,
            HyperLink,
            ScrollBar,
            Iframe,
            Canvas,
            Text,
            Tab,
            EditorPane,
            EditorTable,
            //HTML Elements
            Div,
            Span,
            Form
        }

        public enum eControlAction
        {
         
            InitializeBrowser,
            
            GetPageSource,
          
            GetPageURL,
           
            SwitchFrame,
           
            SwitchToDefaultFrame,
         
            SwitchToParentFrame,
           
            Maximize,
            
            Close,
            
            SwitchWindow,
            
            SwitchToDefaultWindow,
           
            InjectJS,
        
            CheckPageLoaded,
            
            OpenURLNewTab,
           
            GotoURL,
           
            CloseTabExcept,
            
            CloseAll,
        
            Refresh,
            
           
            DismissMessageBox,
          
            DeleteAllCookies,
           
            AcceptMessageBox,
           
            GetWindowTitle,
            
            GetMessageBoxText,
           
            SetAlertBoxText,
           
            RunJavaScript,
            NavigateBack
        }

     

        public enum eGotoURLType
        {

            Current,
            NewTab,
            NewWindow,
        }
    }
}
