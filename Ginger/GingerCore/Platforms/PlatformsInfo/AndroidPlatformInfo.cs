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

using System;
using System.Collections.Generic;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class AndroidPlatform : PlatformInfoBase
    {        
        public override ePlatformType PlatformType()
        {
            return ePlatformType.AndroidDevice;
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
            }
            return mElementsTypeList;
        }

        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            // We cache the results
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.ByResourceID);
                mElementLocatorsTypeList.Add(eLocateBy.ByXY);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByMulitpleProperties);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByText);
                mElementLocatorsTypeList.Add(eLocateBy.ByElementsRepository);
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
            List<string> list = new List<string>();
            // add attr which exist for all android elements from PageSource
            list.Add("index");
            list.Add("text");
            list.Add("resource-id");            
            list.Add("class");
            list.Add("package");
            list.Add("content-desc");
            list.Add("checkable");
            list.Add("checked");
            list.Add("clickable"); 
            list.Add("enabled");
            list.Add("focusable");
            list.Add("scrollable");
            list.Add("long-clickable");
            list.Add("password");
            list.Add("selected");
            list.Add("bounds");

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
        // TODO: provide type of values per property - true/false, string, number bounds etc...       
    }
}
