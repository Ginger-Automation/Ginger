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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class WebPlatform : PlatformInfoBase
    {
        public override ePlatformType PlatformType()
        {
            return ePlatformType.Web;
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
                mElementsTypeList.Add(eElementType.TextBox);
                mElementsTypeList.Add(eElementType.HyperLink);

                // TODO: add all HTML elements
            }   
            return mElementsTypeList;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();

            list.Add(ActUIElement.eElementAction.Click);
            list.Add(ActUIElement.eElementAction.JavaScriptClick);
            list.Add(ActUIElement.eElementAction.MouseClick);
            list.Add(ActUIElement.eElementAction.MousePressRelease);
            list.Add(ActUIElement.eElementAction.AsyncClick);

            return list;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = new List<ActUIElement.eElementDragDropType>();

            list.Add(ActUIElement.eElementDragDropType.DragDropJS);
            list.Add(ActUIElement.eElementDragDropType.DragDropSelenium);
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();

            list.Add(ActUIElement.eElementAction.IsEnabled);
            list.Add(ActUIElement.eElementAction.Exist);
            list.Add(ActUIElement.eElementAction.NotExist);
            list.Add(ActUIElement.eElementAction.IsVisible);

            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            switch (ElementType)
            {

                case eElementType.Unknown:
                    foreach (ActUIElement.eElementAction item in Enum.GetValues(typeof(ActUIElement.eElementAction)))
                    {
                        list.Add(item);
                    }
                    break;
                case eElementType.Button:
                    list.Add(ActUIElement.eElementAction.Click);
                    list.Add(ActUIElement.eElementAction.JavaScriptClick);
                    list.Add(ActUIElement.eElementAction.MouseClick);
                    list.Add(ActUIElement.eElementAction.MousePressRelease);
                    list.Add(ActUIElement.eElementAction.ClickAndValidate);
                    list.Add(ActUIElement.eElementAction.Submit);

                    list.Add(ActUIElement.eElementAction.GetValue);
                    list.Add(ActUIElement.eElementAction.IsDisabled);
                    list.Add(ActUIElement.eElementAction.GetFont);
                    list.Add(ActUIElement.eElementAction.IsVisible);
                    list.Add(ActUIElement.eElementAction.GetWidth);
                    list.Add(ActUIElement.eElementAction.GetHeight);
                    list.Add(ActUIElement.eElementAction.GetStyle);
                    break;
                case eElementType.CheckBox:
                    list.Add(ActUIElement.eElementAction.SetValue);
                    list.Add(ActUIElement.eElementAction.Click);
                    list.Add(ActUIElement.eElementAction.JavaScriptClick);
                    list.Add(ActUIElement.eElementAction.MouseClick);
                    list.Add(ActUIElement.eElementAction.MousePressRelease);
                    list.Add(ActUIElement.eElementAction.ClickAndValidate);

                    list.Add(ActUIElement.eElementAction.GetValue);
                    list.Add(ActUIElement.eElementAction.IsDisabled);
                    list.Add(ActUIElement.eElementAction.IsVisible);
                    list.Add(ActUIElement.eElementAction.GetWidth);
                    list.Add(ActUIElement.eElementAction.GetHeight);
                    list.Add(ActUIElement.eElementAction.GetStyle);
                    break;
                case eElementType.TextBox:
                    list.Add(ActUIElement.eElementAction.SetText);
                    list.Add(ActUIElement.eElementAction.SendKeys);
                    list.Add(ActUIElement.eElementAction.ClearValue);
                    list.Add(ActUIElement.eElementAction.IsValuePopulated);
                    list.Add(ActUIElement.eElementAction.GetTextLength);

                    list.Add(ActUIElement.eElementAction.GetValue);
                    list.Add(ActUIElement.eElementAction.IsDisabled);
                    list.Add(ActUIElement.eElementAction.GetFont);
                    list.Add(ActUIElement.eElementAction.IsVisible);
                    list.Add(ActUIElement.eElementAction.GetWidth);
                    list.Add(ActUIElement.eElementAction.GetHeight);
                    list.Add(ActUIElement.eElementAction.GetStyle);
                    list.Add(ActUIElement.eElementAction.GetSize);
                    break;
                case eElementType.ComboBox:
                    list.Add(ActUIElement.eElementAction.SelectByIndex);
                    list.Add(ActUIElement.eElementAction.Select);
                    list.Add(ActUIElement.eElementAction.SelectByText);
                    list.Add(ActUIElement.eElementAction.SetFocus);
                    list.Add(ActUIElement.eElementAction.GetValidValues);
                    list.Add(ActUIElement.eElementAction.GetSelectedValue);
                    list.Add(ActUIElement.eElementAction.IsValuePopulated);
                    list.Add(ActUIElement.eElementAction.GetFont);
                    list.Add(ActUIElement.eElementAction.GetWidth);
                    list.Add(ActUIElement.eElementAction.GetHeight);
                    list.Add(ActUIElement.eElementAction.GetStyle);
                    list.Add(ActUIElement.eElementAction.GetValue);
                    list.Add(ActUIElement.eElementAction.GetAllValues);
                    break;
                case eElementType.Table:
                    list.Add(ActUIElement.eElementAction.TableAction);
                    list.Add(ActUIElement.eElementAction.TableCellAction);
                    list.Add(ActUIElement.eElementAction.TableRowAction);
                    break;
                case eElementType.ScrollBar:
                    list.Add(ActUIElement.eElementAction.ScrollUp);
                    list.Add(ActUIElement.eElementAction.ScrollDown);
                    list.Add(ActUIElement.eElementAction.ScrollLeft);
                    list.Add(ActUIElement.eElementAction.ScrollRight);
                    break;
                case eElementType.HyperLink:
                    list.Add(ActUIElement.eElementAction.Click);
                    list.Add(ActUIElement.eElementAction.JavaScriptClick);
                    list.Add(ActUIElement.eElementAction.MouseClick);
                    list.Add(ActUIElement.eElementAction.MousePressRelease);
                    list.Add(ActUIElement.eElementAction.ClickAndValidate);

                    list.Add(ActUIElement.eElementAction.GetWidth);
                    list.Add(ActUIElement.eElementAction.GetHeight);
                    list.Add(ActUIElement.eElementAction.GetStyle);
                    list.Add(ActUIElement.eElementAction.GetValue);
                    list.Add(ActUIElement.eElementAction.IsVisible);
                    list.Add(ActUIElement.eElementAction.Hover);
                    break;
            }
            return list;
        }

        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            // We cache the results
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.ByModelName);
                mElementLocatorsTypeList.Add(eLocateBy.ByID);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSS);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByXY);
                mElementLocatorsTypeList.Add(eLocateBy.ByMulitpleProperties);

                mElementLocatorsTypeList.Add(eLocateBy.NA);
                mElementLocatorsTypeList.Add(eLocateBy.Unknown);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.Unknown);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByXY);
                mElementLocatorsTypeList.Add(eLocateBy.ByContainerName);
                mElementLocatorsTypeList.Add(eLocateBy.ByHref);
                mElementLocatorsTypeList.Add(eLocateBy.ByLinkText);
                mElementLocatorsTypeList.Add(eLocateBy.ByValue);
                mElementLocatorsTypeList.Add(eLocateBy.ByIndex);
                mElementLocatorsTypeList.Add(eLocateBy.ByAutomationID);
                mElementLocatorsTypeList.Add(eLocateBy.ByLocalizedControlType);
                mElementLocatorsTypeList.Add(eLocateBy.ByBoundingRectangle);
                mElementLocatorsTypeList.Add(eLocateBy.IsEnabled);
                mElementLocatorsTypeList.Add(eLocateBy.IsOffscreen);
                mElementLocatorsTypeList.Add(eLocateBy.ByTitle);
                mElementLocatorsTypeList.Add(eLocateBy.ByCaretPosition);
                mElementLocatorsTypeList.Add(eLocateBy.ByUrl);
                mElementLocatorsTypeList.Add(eLocateBy.ByngModel);
                mElementLocatorsTypeList.Add(eLocateBy.ByngRepeat);
                mElementLocatorsTypeList.Add(eLocateBy.ByngBind);
                mElementLocatorsTypeList.Add(eLocateBy.ByngSelectedOption);
                mElementLocatorsTypeList.Add(eLocateBy.ByResourceID);
                mElementLocatorsTypeList.Add(eLocateBy.ByContentDescription);
                mElementLocatorsTypeList.Add(eLocateBy.ByText);
                mElementLocatorsTypeList.Add(eLocateBy.ByElementsRepository);
                mElementLocatorsTypeList.Add(eLocateBy.ByModelName);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSSSelector);
            }
            return mElementLocatorsTypeList;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type

            List<string> list = new List<string>();

            //TODO: map all missing HTML tags and common attrs

            // add attr which exist for all HTML tags
            list.Add("id");
            list.Add("name");
            list.Add("TagName");
            list.Add("class");        
    
            // Per element add the attr 
            switch (ElementType)
            {
                case eElementType.TextBox:
                    list.Add("text");
                    list.Add("enabled");
                    list.Add("value");
                    break;
                case eElementType.ComboBox:
                    list.Add("Options");
                    break;
                case eElementType.HyperLink:
                    list.Add("href");
                    break;
                case eElementType.Image:
                    list.Add("src");
                    list.Add("width");
                    list.Add("height");
                    break;

            }
            return list;
        }
    }
}