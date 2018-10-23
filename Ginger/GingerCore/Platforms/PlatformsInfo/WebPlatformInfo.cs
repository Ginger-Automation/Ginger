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
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Linq;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;

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
                mElementsTypeList = GetPlatformElementTypesData().Where(y => y.ActionType == typeof(ActUIElement)).Select(z => z.ElementType).ToList();
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
                default:
                    ElementTypeData elementTypeOperations = GetPlatformElementTypesData().Where(x => x.ElementType == ElementType).FirstOrDefault();
                    if (elementTypeOperations != null)
                    {
                        if (elementTypeOperations.ActionType == typeof(ActUIElement))
                        {
                            elementTypeOperations.ElementOperationsList.ForEach(z => list.Add((ActUIElement.eElementAction)(object)z));
                        }
                    }
                    break;
            }
            return list;
        }

        public override ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            ObservableList<Act> UIElementsActionsList = new ObservableList<Act>();

            ElementTypeData elementTypeOperations = GetPlatformElementTypesData().Where(x => x.ElementType == elementInfo.ElementTypeEnum).FirstOrDefault();
            if ((elementTypeOperations != null) && ((elementTypeOperations.ElementOperationsList != null)) && (elementTypeOperations.ElementOperationsList.Count > 0))
            {
                if (elementTypeOperations.ActionType == typeof(ActBrowserElement))
                {
                    elementTypeOperations.ElementOperationsList.ForEach(z => UIElementsActionsList.Add
                        (new ActBrowserElement()
                        {
                            Description = ((EnumValueDescriptionAttribute[])typeof(ActBrowserElement.eControlAction).GetField(z.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false)).ToString() + elementInfo.ElementTitle,
                            ControlAction = (ActBrowserElement.eControlAction)System.Enum.Parse(typeof(ActBrowserElement.eControlAction), z.ToString()),
                            Value = "true"
                        }));
                }
                else
                {
                    if (elementTypeOperations.ActionType == typeof(ActUIElement))
                    {
                        elementTypeOperations.ElementOperationsList.ForEach(z => UIElementsActionsList.Add
                            (new ActUIElement()
                            {
                                Description = ((EnumValueDescriptionAttribute[])typeof(ActUIElement.eElementAction).GetField(z.ToString()).GetCustomAttributes(typeof(EnumValueDescriptionAttribute), false))[0].ValueDescription + " : " + elementInfo.ElementTitle,
                                ElementAction = (ActUIElement.eElementAction)z,
                                ElementType = elementInfo.ElementTypeEnum,
                            }));
                    }
                }
            }
            return UIElementsActionsList;
        }

        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = new List<ElementTypeData>();

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Unknown,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>(Enum.GetValues(typeof(ActUIElement.eElementAction)).Cast<Enum>())
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Button,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>(){   ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.Submit,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.IsDisabled,
                                                                ActUIElement.eElementAction.GetFont },
                    
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.CheckBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SetValue,
                                                                ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.IsDisabled },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TextBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SetText,
                                                                ActUIElement.eElementAction.SendKeys,
                                                                ActUIElement.eElementAction.ClearValue,
                                                                ActUIElement.eElementAction.IsValuePopulated,
                                                                ActUIElement.eElementAction.GetTextLength,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.IsDisabled,
                                                                ActUIElement.eElementAction.IsEnabled,
                                                                ActUIElement.eElementAction.GetFont,
                                                                ActUIElement.eElementAction.GetSize },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ComboBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SelectByIndex,
                                                                ActUIElement.eElementAction.Select,
                                                                ActUIElement.eElementAction.SelectByText,
                                                                ActUIElement.eElementAction.GetValidValues,
                                                                ActUIElement.eElementAction.GetSelectedValue,
                                                                ActUIElement.eElementAction.IsValuePopulated,
                                                                ActUIElement.eElementAction.GetFont,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.GetAllValues }
                    });

                //mWebPlatformElementTypeOperations.Add(new ElementTypeOperations()
                //{
                //    ElementType = eElementType.Table,
                //    ActionType = typeof(ActUIElement),
                //    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.TableAction,
                //                                                ActUIElement.eElementAction.TableCellAction,
                //                                                ActUIElement.eElementAction.TableRowAction }
                //});

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ScrollBar,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.ScrollUp,
                                                                ActUIElement.eElementAction.ScrollDown,
                                                                ActUIElement.eElementAction.ScrollLeft,
                                                                ActUIElement.eElementAction.ScrollRight }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.HyperLink,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.GetValue }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Label,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.List,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetAllValues,
                                                               ActUIElement.eElementAction.GetValue}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TableItem,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.IsEnabled }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Div,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>()
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Span,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SetValue,
                                                                ActUIElement.eElementAction.GetValue }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.RadioButton,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.GetValue }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Image,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>()
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Iframe,
                    ActionType = typeof(ActBrowserElement),
                    ElementOperationsList = new List<Enum>() {  ActBrowserElement.eControlAction.SwitchFrame,
                                                                ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                                                                ActBrowserElement.eControlAction.SwitchToParentFrame }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Canvas,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.ClickXY}
                });

                // adding generic/common actions per each ElementType
                List<Enum> ElementCommonActionsList = new List<Enum>() {    ActUIElement.eElementAction.Hover,
                                                                            ActUIElement.eElementAction.IsVisible,
                                                                            ActUIElement.eElementAction.GetHeight,
                                                                            ActUIElement.eElementAction.GetStyle,
                                                                            ActUIElement.eElementAction.GetWidth,
                                                                            // ActUIElement.eElementAction.NotExist,   //
                                                                            // ActUIElement.eElementAction.IsExist,    // currently not implemented in Web
                                                                            ActUIElement.eElementAction.SetFocus,
                                                                            ActUIElement.eElementAction.RunJavaScript};

                mPlatformElementTypeOperations.Where( y => y.ActionType == typeof(ActUIElement)).ToList()
                                                 .ForEach(z => z.ElementOperationsList = z.ElementOperationsList.Union(ElementCommonActionsList).ToList());
            }

            return mPlatformElementTypeOperations;
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
                if (WorkSpace.Instance.BetaFeatures.ShowPOMInResourcesTab)
                {
                    mElementLocatorsTypeList.Add(eLocateBy.POMElement);
                }
                mElementLocatorsTypeList.Add(eLocateBy.NA);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.Unknown);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
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