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
    class PowerBuilderPlatform : PlatformInfoBase
    {
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.ByAutomationID);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.ByAutomationID);
                mElementLocatorsTypeList.Add(eLocateBy.ByTitle);
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
            //TOOD: Currently we support only drag and drop for PB. So avoiding the call to get actions list from base
            List<ActUIElement.eElementAction> controlActionlist = new List<ActUIElement.eElementAction>();

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

        public override List<ActUIElement.eSubElementType> GetSubElementType(eElementType elementType)
        {
            List<ActUIElement.eSubElementType> list = new List<ActUIElement.eSubElementType>();
            if (elementType== eElementType.List || elementType == eElementType.ComboBox)
            {                
                list.Add(ActUIElement.eSubElementType.Pane);                
            }
            return list;
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
            return ePlatformType.PowerBuilder;
        }

        public override List<ActUIElement.eTableAction> GetTableControlActions(ActUIElement.eElementAction tableAction)
        {
            List<ActUIElement.eTableAction> pbTableControlActionlist = base.GetTableControlActions(tableAction);
            return pbTableControlActionlist;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {            
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            list.Add(ActUIElement.eElementAction.IsEnabled);
            list.Add(ActUIElement.eElementAction.Exist);
            list.Add(ActUIElement.eElementAction.NotExist);
            list.Add(ActUIElement.eElementAction.GetValue);
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
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
                mElementsTypeList.Add(eElementType.Text);
                mElementsTypeList.Add(eElementType.Dialog);
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
