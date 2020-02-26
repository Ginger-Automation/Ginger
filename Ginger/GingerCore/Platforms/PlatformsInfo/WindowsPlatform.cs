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
            List<ActUIElement.eElementAction> pbTableControlActionlist = base.GetPlatformUIElementActionsList(ElementType);
            
            switch (ElementType)
            {
                case eElementType.Unknown:
                    break;
                case eElementType.Button:
                    pbTableControlActionlist.Add(ActUIElement.eElementAction.ClickAndValidate);
                    break;
                case eElementType.Window:
                    pbTableControlActionlist.Add(ActUIElement.eElementAction.Switch);
                    break;
            }
            return pbTableControlActionlist;
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
            List<ActUIElement.eTableAction> pbTableControlActionlist = base.GetTableControlActions(tableAction);
            switch (tableAction)
            {
                case ActUIElement.eElementAction.TableCellAction:
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.AsyncClick);
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.WinClick);
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.Type);
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.DoubleClick);
                    break;
                case ActUIElement.eElementAction.TableRowAction:
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.SetFocus);
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.IsCellEnabled);
                    break;
                case ActUIElement.eElementAction.TableAction:
                    pbTableControlActionlist.Add(ActUIElement.eTableAction.SetFocus);
                    break;
            }
            return pbTableControlActionlist;
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
