#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class PowerBuilderPlatform : PlatformInfoBase
    {
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList =
                [
                    eLocateBy.ByAutomationID,
                    eLocateBy.ByRelXPath,
                    eLocateBy.ByXPath,
                    eLocateBy.ByClassName,
                    eLocateBy.ByName,
                    eLocateBy.ByAutomationID,
                    eLocateBy.ByTitle,
                ];
            }
            return mElementLocatorsTypeList;
        }

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList =
            [
                ActBrowserElement.eControlAction.InitializeBrowser,
                ActBrowserElement.eControlAction.GetPageSource,
                ActBrowserElement.eControlAction.GetPageURL,
                ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                ActBrowserElement.eControlAction.SwitchFrame,
            ];

            return browserActElementList;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            //TOOD: Currently we support only drag and drop for PB. So avoiding the call to get actions list from base
            List<ActUIElement.eElementAction> controlActionlist = [];

            switch (ElementType)
            {
                case eElementType.Unknown:
                    controlActionlist.Add(ActUIElement.eElementAction.DragDrop);
                    break;

                case eElementType.Button:
                    controlActionlist.Add(ActUIElement.eElementAction.ClickAndValidate);
                    break;
                case eElementType.List:
                case eElementType.ComboBox:
                    controlActionlist.Add(ActUIElement.eElementAction.SendKeysAndValidate);
                    controlActionlist.Add(ActUIElement.eElementAction.SelectandValidate);
                    break;
                case eElementType.TextBox:
                case eElementType.Text:
                    controlActionlist.Add(ActUIElement.eElementAction.SendKeysAndValidate);
                    break;
                case eElementType.Dialog:
                    controlActionlist.Add(ActUIElement.eElementAction.AcceptDialog);
                    controlActionlist.Add(ActUIElement.eElementAction.DismissDialog);
                    break;
                case eElementType.Window:
                    controlActionlist.Add(ActUIElement.eElementAction.Switch);
                    break;
            }
            return controlActionlist;
        }

        public override string GetPlatformGenericElementEditControls()
        {
            return null;
        }

        /// <summary>
        /// This method is used to return possible POM elements categories per platform
        /// </summary>
        /// <returns></returns>
        public override List<ePomElementCategory> GetPlatformPOMElementCategories()
        {
            return [ePomElementCategory.PowerBuilder];
        }

        public override List<ActUIElement.eSubElementType> GetSubElementType(eElementType elementType)
        {
            List<ActUIElement.eSubElementType> list = [];
            if (elementType is eElementType.List or eElementType.ComboBox)
            {
                list.Add(ActUIElement.eSubElementType.Pane);
            }
            return list;
        }
        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type

            List<string> list =
            [
                //TODO: map all missing HTML tags and common attrs

                // add attr which exist for all HTML tags
                "Id",
                "Name",
                "AutomationId",
                "XPath",
            ];

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
            return ePlatformType.PowerBuilder;
        }

        public override List<ActUIElement.eTableAction> GetTableControlActions(ActUIElement.eElementAction tableAction)
        {
            List<ActUIElement.eTableAction> pbTableControlActionlist = base.GetTableControlActions(tableAction);
            return pbTableControlActionlist;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list =
            [
                ActUIElement.eElementAction.IsEnabled,
                ActUIElement.eElementAction.Exist,
                ActUIElement.eElementAction.NotExist,
                ActUIElement.eElementAction.GetValue,
            ];
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            List<ActUIElement.eElementAction> list =
            [
                ActUIElement.eElementAction.InvokeClick,
                ActUIElement.eElementAction.LegacyClick,
                ActUIElement.eElementAction.MouseClick,
            ];

            return list;
        }

        public override List<eElementType> GetPlatformUIElementsType()
        {
            // We cache the results
            if (mElementsTypeList == null)
            {
                mElementsTypeList =
                [
                    eElementType.Unknown,
                    eElementType.Button,
                    eElementType.ComboBox,
                    eElementType.RadioButton,
                    eElementType.TextBox,
                    eElementType.CheckBox,
                    eElementType.Image,
                    eElementType.Label,
                    eElementType.List,
                    eElementType.Table,
                    eElementType.Text,
                    eElementType.Dialog,
                    eElementType.Window,
                ];

            }
            return mElementsTypeList;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = [ActUIElement.eElementDragDropType.MouseDragDrop];
            return list;
        }

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            return null;
        }

        public override Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList()
        {
            throw new System.NotImplementedException();
        }

        public override List<ePosition> GetElementPositionList()
        {
            throw new System.NotImplementedException();
        }
    }
}
