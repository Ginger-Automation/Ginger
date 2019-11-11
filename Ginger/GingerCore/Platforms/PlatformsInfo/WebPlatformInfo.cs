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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Linq;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core;
using System.Reflection;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;

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

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList = new List<ActBrowserElement.eControlAction>();

            browserActElementList.Add(ActBrowserElement.eControlAction.GotoURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.AcceptMessageBox);
            browserActElementList.Add(ActBrowserElement.eControlAction.OpenURLNewTab);
            browserActElementList.Add(ActBrowserElement.eControlAction.Refresh);
            browserActElementList.Add(ActBrowserElement.eControlAction.RunJavaScript);
            browserActElementList.Add(ActBrowserElement.eControlAction.Maximize);
            browserActElementList.Add(ActBrowserElement.eControlAction.Close);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToDefaultFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToParentFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchWindow);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetWindowTitle);
            browserActElementList.Add(ActBrowserElement.eControlAction.DeleteAllCookies);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageSource);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.InjectJS);
            browserActElementList.Add(ActBrowserElement.eControlAction.CheckPageLoaded);
            browserActElementList.Add(ActBrowserElement.eControlAction.CloseTabExcept);
            browserActElementList.Add(ActBrowserElement.eControlAction.CloseAll);
            browserActElementList.Add(ActBrowserElement.eControlAction.NavigateBack);
            browserActElementList.Add(ActBrowserElement.eControlAction.DismissMessageBox);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetMessageBoxText);
            browserActElementList.Add(ActBrowserElement.eControlAction.SetAlertBoxText);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetBrowserLog);
            return browserActElementList;
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
            //list.Add(ActUIElement.eElementAction.Exist);
            //list.Add(ActUIElement.eElementAction.NotExist);
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

        public override Act GetPlatformActionByElementInfo(ElementInfo elementInfo, ElementActionCongifuration actConfig)
        {
            var pomExcutionUtil = new POMExecutionUtils();
            Act elementAction = null;
            if (elementInfo != null)
            {
                ElementTypeData elementTypeOperations = GetPlatformElementTypesData().Where(x => x.ElementType == elementInfo.ElementTypeEnum).FirstOrDefault();
                if(actConfig != null)
                {
                    if (string.IsNullOrWhiteSpace(actConfig.Operation))
                        actConfig.Operation = GetDefaultElementOperation(elementInfo.ElementTypeEnum);
                }
                if ((elementTypeOperations != null) && ((elementTypeOperations.ElementOperationsList != null)) && (elementTypeOperations.ElementOperationsList.Count > 0))
                {
                    if (elementTypeOperations.ActionType == typeof(ActBrowserElement))
                    {
                        elementAction = new ActBrowserElement()
                        {
                            Description = string.IsNullOrWhiteSpace(actConfig.Description) ? "Browser Action : " + actConfig.Operation + " - " + elementInfo.ItemName : actConfig.Description,
                            ControlAction = (ActBrowserElement.eControlAction)System.Enum.Parse(typeof(ActBrowserElement.eControlAction), actConfig.Operation),
                            LocateBy = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), Convert.ToString(actConfig.LocateBy)),
                            Value = actConfig.ElementValue,
                            LocateValue = actConfig.LocateValue
                        };
                    }
                    else if (elementTypeOperations.ActionType == typeof(ActUIElement))
                    {
                        elementAction = new ActUIElement()
                        {
                            Description = string.IsNullOrWhiteSpace(actConfig.Description) ? "UI Element Action : " + actConfig.Operation + " - " + elementInfo.ItemName : actConfig.Description,
                            ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), actConfig.Operation),
                            ElementLocateValue = actConfig.LocateValue,
                            Value = actConfig.ElementValue
                        };

                        pomExcutionUtil.SetPOMProperties(elementAction, elementInfo, actConfig);
                    }
                } 
            }
            else
            {
                elementAction = new ActUIElement()
                {
                    Description = string.IsNullOrWhiteSpace(actConfig.Description) ? "UI Element Action : " + actConfig.Operation + " - " + elementInfo.ItemName : actConfig.Description,
                    ElementLocateBy = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), Convert.ToString(actConfig.LocateBy)),
                    ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), actConfig.Operation),
                    ElementLocateValue = actConfig.LocateValue,
                    ElementType = (eElementType)System.Enum.Parse(typeof(eElementType), Convert.ToString(actConfig.Type)),
                    Value = actConfig.ElementValue
                };
            }
            return elementAction;
        }
        public override string GetDefaultElementOperation(eElementType ElementTypeEnum)
        {
            switch (ElementTypeEnum)
            {
                case eElementType.Button:
                case eElementType.CheckBox:
                case eElementType.RadioButton:
                case eElementType.HyperLink:
                case eElementType.Span:
                case eElementType.Div:
                    //actConfig.Operation = ActUIElement.eElementAction.Click.ToString();
                    return ActUIElement.eElementAction.Click.ToString();

                case eElementType.TextBox:
                    //actConfig.Operation = ActUIElement.eElementAction.SetText.ToString();
                    return ActUIElement.eElementAction.SetText.ToString();

                case eElementType.Iframe:
                    //actConfig.Operation = ActUIElement.eElementAction.SetText.ToString();
                    return ActBrowserElement.eControlAction.SwitchFrame.ToString();

                case eElementType.ComboBox:
                    return ActUIElement.eElementAction.SelectByText.ToString();

                default:
                    //actConfig.Operation = ActUIElement.eElementAction.NotExist.ToString();
                    return ActUIElement.eElementAction.NotExist.ToString();
            }
        }
        /// <summary>
        /// This method is used to check if the paltform supports POM
        /// </summary>
        /// <returns></returns>
        public override bool IsPlatformSupportPOM()
        {
            return true;
        }

        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = new List<ElementTypeData>();

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Button,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>(){   ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.DoubleClick,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.MultiClicks,
                                                                ActUIElement.eElementAction.Submit,
                                                                ActUIElement.eElementAction.GetValue,                                                                
                                                                },

                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.CheckBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.DoubleClick,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.MultiClicks,
                                                                ActUIElement.eElementAction.GetValue
                                                                 },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TextBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.SetValue,
                                                                ActUIElement.eElementAction.SetText,
                                                                ActUIElement.eElementAction.SendKeys,                                                                
                                                                ActUIElement.eElementAction.MultiSetValue,
                                                                ActUIElement.eElementAction.ClearValue,
                                                                ActUIElement.eElementAction.IsValuePopulated,
                                                                ActUIElement.eElementAction.GetText,
                                                                ActUIElement.eElementAction.GetTextLength,
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.GetFont,
                                                                },
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ComboBox,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Select,
                                                                ActUIElement.eElementAction.SelectByIndex,                                                               
                                                                ActUIElement.eElementAction.SelectByText,
                                                                ActUIElement.eElementAction.SetValue,
                                                                ActUIElement.eElementAction.ClearValue,                                                                                                                                
                                                                ActUIElement.eElementAction.GetValidValues,
                                                                ActUIElement.eElementAction.GetSelectedValue,
                                                                ActUIElement.eElementAction.IsValuePopulated,
                                                                ActUIElement.eElementAction.GetText,                                                                                                                               
                                                                }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ScrollBar,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  //ActUIElement.eElementAction.ScrollUp, //not yet supported by SelenuimDriver
                                                                //ActUIElement.eElementAction.ScrollDown,
                                                                //ActUIElement.eElementAction.ScrollLeft,
                                                                //ActUIElement.eElementAction.ScrollRight
                    }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.HyperLink,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.DoubleClick,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.MultiClicks,
                                                                ActUIElement.eElementAction.GetValue}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Label,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue,
                                                               ActUIElement.eElementAction.GetText,
                                                               ActUIElement.eElementAction.GetFont}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Text,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue,
                                                               ActUIElement.eElementAction.GetText,
                                                               ActUIElement.eElementAction.GetFont}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.List,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {
                                                               ActUIElement.eElementAction.Select,
                                                               ActUIElement.eElementAction.SelectByIndex,
                                                               ActUIElement.eElementAction.SelectByText,
                                                               ActUIElement.eElementAction.ClearValue,
                                                               ActUIElement.eElementAction.GetValidValues,
                                                               ActUIElement.eElementAction.GetSelectedValue,
                                                               ActUIElement.eElementAction.IsValuePopulated,
                                                               ActUIElement.eElementAction.GetValue}
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TableItem,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  
                                                                ActUIElement.eElementAction.GetValue,
                                                                ActUIElement.eElementAction.SetValue,
                                                                ActUIElement.eElementAction.Click,
                                                              }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Div,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue,
                                                               }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Span,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() { ActUIElement.eElementAction.GetValue,
                                                               ActUIElement.eElementAction.SetValue,
                                                               ActUIElement.eElementAction.Click,
                                                               }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.RadioButton,
                    IsCommonElementType = true,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Click,
                                                                ActUIElement.eElementAction.DoubleClick,
                                                                ActUIElement.eElementAction.JavaScriptClick,
                                                                ActUIElement.eElementAction.MouseClick,
                                                                ActUIElement.eElementAction.MousePressRelease,
                                                                ActUIElement.eElementAction.ClickAndValidate,
                                                                ActUIElement.eElementAction.MultiClicks,
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
                    IsCommonElementType = true,
                    ActionType = typeof(ActBrowserElement),
                    ElementOperationsList = new List<Enum>() {  ActBrowserElement.eControlAction.SwitchFrame,
                                                                ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                                                                ActBrowserElement.eControlAction.SwitchToParentFrame }
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Canvas,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.ClickXY,
                                                                ActUIElement.eElementAction.DrawObject,
                                                                ActUIElement.eElementAction.DoubleClickXY,
                                                                ActUIElement.eElementAction.SendKeysXY,
                                                               }
                });

                // adding generic/common actions per each ElementType
                List<Enum> ElementCommonActionsList = new List<Enum>() {                                                                            
                                                                            ActUIElement.eElementAction.IsVisible,
                                                                            ActUIElement.eElementAction.IsDisabled,
                                                                            ActUIElement.eElementAction.IsEnabled,
                                                                            ActUIElement.eElementAction.Hover,                                                                            
                                                                            ActUIElement.eElementAction.GetHeight,
                                                                            ActUIElement.eElementAction.GetStyle,
                                                                            ActUIElement.eElementAction.GetWidth,                                                                           
                                                                            ActUIElement.eElementAction.SetFocus,
                                                                            ActUIElement.eElementAction.RunJavaScript,
                                                                            ActUIElement.eElementAction.GetSize,
                                                                            ActUIElement.eElementAction.GetAttrValue,                                                                           
                                                                            ActUIElement.eElementAction.GetItemCount,
                                                                            ActUIElement.eElementAction.DragDrop,
                                                                            ActUIElement.eElementAction.MouseRightClick,
                                                                            ActUIElement.eElementAction.ScrollToElement,
                };

                mPlatformElementTypeOperations.Where(y => y.ActionType == typeof(ActUIElement)).ToList()
                                                 .ForEach(z => z.ElementOperationsList = z.ElementOperationsList.Union(ElementCommonActionsList).ToList());


                //Adding to support switch window in UIelement
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Window,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = new List<Enum>() {  ActUIElement.eElementAction.Switch }
                });

                //------Must be last one for calculating all supported Element operations
                List<Enum> allSupportedOperations = new List<Enum>();
                foreach (ElementTypeData elemData in mPlatformElementTypeOperations)
                {
                    foreach (ActUIElement.eElementAction operation in elemData.ElementOperationsList)
                    {
                        if (!allSupportedOperations.Contains(operation))
                        {
                            allSupportedOperations.Add(operation);
                        }
                    }
                }
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Unknown,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = allSupportedOperations
                });
            }

            return mPlatformElementTypeOperations;
        }

        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            // We cache the results
            if (mElementLocatorsTypeList == null)
            {
                //Arrange locator on priority basis
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.POMElement);
                mElementLocatorsTypeList.Add(eLocateBy.ByID);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSS);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByXY);
                mElementLocatorsTypeList.Add(eLocateBy.ByMulitpleProperties);                                
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
                mElementLocatorsTypeList.Add(eLocateBy.ByCSSSelector);
                mElementLocatorsTypeList.Add(eLocateBy.ByModelName);//???
                mElementLocatorsTypeList.Add(eLocateBy.NA);
                mElementLocatorsTypeList.Add(eLocateBy.Unknown);
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

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            ObservableList<ElementLocator> learningLocatorsList = new ObservableList<ElementLocator>();
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Recommended (sensitive to page design changes)" });

            return learningLocatorsList;
        }

    }
}