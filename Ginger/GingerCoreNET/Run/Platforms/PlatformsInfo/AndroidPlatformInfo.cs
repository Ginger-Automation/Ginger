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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class AndroidPlatform : PlatformInfoBase
    {
        public override ePlatformType PlatformType()
        {
            //return ePlatformType.AndroidDevice;
            return ePlatformType.Mobile;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            throw new NotImplementedException();
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            throw new NotImplementedException();
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            throw new NotImplementedException();
        }
        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            throw new NotImplementedException();
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
                ];
            }
            return mElementsTypeList;
        }

        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            // We cache the results
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList =
                [
                    eLocateBy.ByResourceID,
                    eLocateBy.ByXY,
                    eLocateBy.ByXPath,
                    eLocateBy.ByMulitpleProperties,
                    eLocateBy.ByClassName,
                    eLocateBy.ByText,
                    eLocateBy.ByElementsRepository,
                ];
            }
            return mElementLocatorsTypeList;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            // just as example we can override and add action which exist only in Android - like LongClick
            List<ActUIElement.eElementAction> list = base.GetPlatformUIElementActionsList(ElementType);

            // if ElementType
            // switch
            list.Add(ActUIElement.eElementAction.LongClick);
            return list;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type
            List<string> list =
            [
                // add attr which exist for all android elements from PageSource
                "index",
                "text",
                "resource-id",
                "class",
                "package",
                "content-desc",
                "checkable",
                "checked",
                "clickable",
                "enabled",
                "focusable",
                "scrollable",
                "long-clickable",
                "password",
                "selected",
                "bounds",
            ];

            // Per type we can add more
            switch (ElementType)
            {
                case eElementType.TextBox:
                    // list.Add("text");
                    // list.Add("enabled");
                    break;
                case eElementType.RadioButton:
                    //list.Add("selected");
                    break;

            }
            return list;
        }

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            return null;
        }

        public override Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList()
        {
            throw new NotImplementedException();
        }

        public override List<ePosition> GetElementPositionList()
        {
            throw new NotImplementedException();
        }

        // TODO: provide type of values per property - true/false, string, number bounds etc...       
    }
}
