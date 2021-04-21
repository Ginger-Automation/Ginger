#region License
/*
Copyright © 2014-2020 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms.PlatformsInfo
{
    class WindowsPlatform : PlatformInfoBase
    {
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.ByAutomationID);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByTitle);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.ByText);
                mElementLocatorsTypeList.Add(eLocateBy.ByID);
            }
            return mElementLocatorsTypeList;
        }

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList = new List<ActBrowserElement.eControlAction>();

            browserActElementList.Add(ActBrowserElement.eControlAction.InitializeBrowser);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageSource);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToDefaultFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchFrame);

            return browserActElementList;
        }
        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> windowsPlatformElementActionslist = base.GetPlatformUIElementActionsList(ElementType);
            
            switch (ElementType)
            {
                case eElementType.Unknown:
                    break;
                case eElementType.Button:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickAndValidate);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    break;
                case eElementType.Window:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetWindowTitle);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Maximize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Minimize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.IsExist);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.CloseWindow);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Switch);
                    break;
                case eElementType.MenuItem:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Collapse);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Expand);
                    break;
            }
            return windowsPlatformElementActionslist;
        }

        public override string GetPlatformGenericElementEditControls()
        {
            return null;
        }
        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type

            List<string> list = new List<string>();

            //TODO: map all missing HTML tags and common attrs

            // add attr which exist for all HTML tags
            list.Add("Id");
            list.Add("Name");
            list.Add("AutomationId");
            list.Add("XPath");

            // Per element add the attr 
            switch (ElementType)
            {
                case eElementType.Unknown:
                    break;
            }
            return list;
        }

        public override ePlatformType PlatformType()
        {
            return ePlatformType.Windows;
        }

        public override List<ActUIElement.eTableAction> GetTableControlActions(ActUIElement.eElementAction tableAction)
        {
            List<ActUIElement.eTableAction> windowsTableControlActionlist = base.GetTableControlActions(tableAction);
            switch (tableAction)
            {
                case ActUIElement.eElementAction.TableCellAction:
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.IsCellEnabled);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.IsVisible);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetValue);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.SetValue);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.SetFocus);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.Click);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.AsyncClick);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.WinClick);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.Toggle);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.DoubleClick);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.Type);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.MousePressAndRelease);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.IsChecked);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.RightClick);
                    break;
                case ActUIElement.eElementAction.TableRowAction:
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetSelectedRow);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.ActivateRow);
                    break;
                case ActUIElement.eElementAction.TableAction:
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetRowCount);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.SelectAllRows);
                    //list.Add(ActUIElement.eTableAction.RightClick);
                    break;
                case ActUIElement.eElementAction.Unknown:
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.IsCellEnabled);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.IsVisible);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetValue);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.SetValue);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.SetFocus);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.Click);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.AsyncClick);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.WinClick);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.Toggle);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetRowCount);
                    windowsTableControlActionlist.Add(ActUIElement.eTableAction.GetSelectedRow);
                    break;
            }
            return windowsTableControlActionlist;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            list.Add(ActUIElement.eElementAction.IsEnabled);
            list.Add(ActUIElement.eElementAction.Exist);
            list.Add(ActUIElement.eElementAction.NotExist);
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            list.Add(ActUIElement.eElementAction.DragDrop);
            list.Add(ActUIElement.eElementAction.InvokeClick);
            list.Add(ActUIElement.eElementAction.LegacyClick);
            list.Add(ActUIElement.eElementAction.MouseClick);

            return list;
        }

        public override List<eElementType> GetPlatformUIElementsType()
        {
            // We cache the results
            if (mElementsTypeList == null)
            {
                mElementsTypeList = new List<eElementType>();

                mElementsTypeList.Add(eElementType.Unknown);
                mElementsTypeList.Add(eElementType.Button);
                mElementsTypeList.Add(eElementType.ComboBox);
                mElementsTypeList.Add(eElementType.RadioButton);
                mElementsTypeList.Add(eElementType.TextBox);
                mElementsTypeList.Add(eElementType.CheckBox);
                mElementsTypeList.Add(eElementType.Image);
                mElementsTypeList.Add(eElementType.Label);
                mElementsTypeList.Add(eElementType.List);
                mElementsTypeList.Add(eElementType.Table);
                mElementsTypeList.Add(eElementType.Window);
                mElementsTypeList.Add(eElementType.MenuItem);
            }
            return mElementsTypeList;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = new List<ActUIElement.eElementDragDropType>();

            list.Add(ActUIElement.eElementDragDropType.MouseDragDrop);            
            return list;
        }

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            return null;
        }

    }
}
