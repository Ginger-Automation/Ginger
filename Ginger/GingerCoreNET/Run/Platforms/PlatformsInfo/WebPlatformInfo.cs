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
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

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
            List<ActUIElement.eElementAction> list =
            [
                ActUIElement.eElementAction.Click,
                ActUIElement.eElementAction.JavaScriptClick,
                ActUIElement.eElementAction.MouseClick,
                ActUIElement.eElementAction.MousePressRelease,
                ActUIElement.eElementAction.AsyncClick,
            ];

            return list;
        }

        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList =
            [
                ActBrowserElement.eControlAction.GotoURL,
                ActBrowserElement.eControlAction.AcceptMessageBox,
                ActBrowserElement.eControlAction.OpenURLNewTab,
                ActBrowserElement.eControlAction.Refresh,
                ActBrowserElement.eControlAction.RunJavaScript,
                ActBrowserElement.eControlAction.Maximize,
                ActBrowserElement.eControlAction.Close,
                ActBrowserElement.eControlAction.SwitchFrame,
                ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                ActBrowserElement.eControlAction.SwitchToParentFrame,
                ActBrowserElement.eControlAction.SwitchWindow,
                ActBrowserElement.eControlAction.SwitchToShadowDOM,
                ActBrowserElement.eControlAction.SwitchToDefaultDOM,
                ActBrowserElement.eControlAction.GetWindowTitle,
                ActBrowserElement.eControlAction.DeleteAllCookies,
                ActBrowserElement.eControlAction.GetPageSource,
                ActBrowserElement.eControlAction.GetPageURL,
                ActBrowserElement.eControlAction.InjectJS,
                ActBrowserElement.eControlAction.CheckPageLoaded,
                ActBrowserElement.eControlAction.CloseTabExcept,
                ActBrowserElement.eControlAction.CloseAll,
                ActBrowserElement.eControlAction.NavigateBack,
                ActBrowserElement.eControlAction.DismissMessageBox,
                ActBrowserElement.eControlAction.GetMessageBoxText,
                ActBrowserElement.eControlAction.SetAlertBoxText,
                ActBrowserElement.eControlAction.GetBrowserLog,
                ActBrowserElement.eControlAction.GetConsoleLog,
                ActBrowserElement.eControlAction.StartMonitoringNetworkLog,
                ActBrowserElement.eControlAction.GetNetworkLog,
                ActBrowserElement.eControlAction.StopMonitoringNetworkLog,
                ActBrowserElement.eControlAction.SetBlockedUrls,
                ActBrowserElement.eControlAction.UnblockeUrls,
                ActBrowserElement.eControlAction.ClearExistingNetworkLog,
            ];
            return browserActElementList;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            List<ActUIElement.eElementDragDropType> list = [ActUIElement.eElementDragDropType.DragDropJS, ActUIElement.eElementDragDropType.DragDropSelenium];
            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            List<ActUIElement.eElementAction> list =
            [
                ActUIElement.eElementAction.IsEnabled,
                //list.Add(ActUIElement.eElementAction.Exist);
                //list.Add(ActUIElement.eElementAction.NotExist);
                ActUIElement.eElementAction.IsVisible,
            ];

            return list;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> list = [];
            switch (ElementType)
            {
                default:
                    ElementTypeData elementTypeOperations = GetPlatformElementTypesData().FirstOrDefault(x => x.ElementType == ElementType);
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
            ObservableList<Act> UIElementsActionsList = [];

            ElementTypeData elementTypeOperations = GetPlatformElementTypesData().FirstOrDefault(x => x.ElementType == elementInfo.ElementTypeEnum);
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
                ElementTypeData elementTypeOperations = GetPlatformElementTypesData().FirstOrDefault(x => x.ElementType == elementInfo.ElementTypeEnum);
                if (actConfig != null)
                {
                    if (string.IsNullOrWhiteSpace(actConfig.Operation))
                    {
                        actConfig.Operation = GetDefaultElementOperation(elementInfo.ElementTypeEnum);
                    }
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
                            //LocateBy = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), Convert.ToString(actConfig.LocateBy)),
                            ElementLocateBy = (eLocateBy)System.Enum.Parse(typeof(eLocateBy), Convert.ToString(actConfig.LocateBy)),
                            ElementLocateValue = actConfig.LocateValue,
                            //LocateValue = actConfig.LocateValue,
                            ElementType = (eElementType)System.Enum.Parse(typeof(eElementType), Convert.ToString(actConfig.Type)),
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
            return ElementTypeEnum switch
            {
                eElementType.Button or eElementType.CheckBox or eElementType.RadioButton or eElementType.HyperLink or eElementType.Span or eElementType.Div or eElementType.Image => ActUIElement.eElementAction.Click.ToString(),//actConfig.Operation = ActUIElement.eElementAction.Click.ToString();
                eElementType.TextBox => ActUIElement.eElementAction.SetText.ToString(),//actConfig.Operation = ActUIElement.eElementAction.SetText.ToString();
                eElementType.Iframe => ActBrowserElement.eControlAction.SwitchFrame.ToString(),//actConfig.Operation = ActUIElement.eElementAction.SetText.ToString();
                eElementType.ComboBox => ActUIElement.eElementAction.SelectByText.ToString(),
                eElementType.Label or eElementType.Text => ActUIElement.eElementAction.GetValue.ToString(),
                _ => ActUIElement.eElementAction.NotExist.ToString(),//actConfig.Operation = ActUIElement.eElementAction.NotExist.ToString();
            };
        }

        /// <summary>
        /// This method is used to check if the paltform supports POM
        /// </summary>
        /// <returns></returns>
        public override bool IsPlatformSupportPOM()
        {
            return true;
        }

        /// <summary>
        /// This method is used to return possible POM elements categories per platform
        /// </summary>
        /// <returns></returns>
        public override List<ePomElementCategory> GetPlatformPOMElementCategories()
        {
            return [ePomElementCategory.Web];
        }

        /// <summary>
        /// This method is used to check if the paltform includes GUI based Application Instance and thus, supports Sikuli based operations
        /// </summary>
        /// <returns></returns>
        public override bool IsSikuliSupported()
        {
            return true;
        }

        public override Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList()
        {
            ObservableList<UIElementFilter> uIBasicElementFilters = [];
            ObservableList<UIElementFilter> uIAdvancedElementFilters = [];

            foreach (PlatformInfoBase.ElementTypeData elementTypeOperation in GetPlatformElementTypesData())
            {
                if (elementTypeOperation.IsCommonElementType)
                {
                    uIBasicElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, true));
                }
                else
                {
                    if(elementTypeOperation.ElementType.Equals(eElementType.Svg))
                    {
                        uIAdvancedElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, "It includes key SVG child elements such as SvgGroup, SvgLine, SvgUse, SvgText, SvgCircle, and SvgPath.", false));
                    }
                    else
                    {
                        uIAdvancedElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, string.Empty, false));
                    }                    
                }
            }

            Dictionary<string, ObservableList<UIElementFilter>> elementListDic = new Dictionary<string, ObservableList<UIElementFilter>>
            {
                { "Basic", new ObservableList<UIElementFilter>(uIBasicElementFilters) },
                { "Advanced", new ObservableList<UIElementFilter>(uIAdvancedElementFilters) }
            };

            return elementListDic;
        }

        public virtual List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations =
                [
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Button,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                    ActUIElement.eElementAction.DoubleClick,
                                                                    ActUIElement.eElementAction.JavaScriptClick,
                                                                    ActUIElement.eElementAction.MouseClick,
                                                                    ActUIElement.eElementAction.MousePressRelease,
                                                                    ActUIElement.eElementAction.ClickAndValidate,
                                                                    ActUIElement.eElementAction.MultiClicks,
                                                                    ActUIElement.eElementAction.Submit,
                                                                    ActUIElement.eElementAction.GetValue,
                                                                    ],

                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.CheckBox,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                    ActUIElement.eElementAction.DoubleClick,
                                                                    ActUIElement.eElementAction.JavaScriptClick,
                                                                    ActUIElement.eElementAction.MouseClick,
                                                                    ActUIElement.eElementAction.MousePressRelease,
                                                                    ActUIElement.eElementAction.ClickAndValidate,
                                                                    ActUIElement.eElementAction.MultiClicks,
                                                                    ActUIElement.eElementAction.GetValue
                                                                     ],
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.TextBox,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [  ActUIElement.eElementAction.SetValue,
                                                                    ActUIElement.eElementAction.SetText,
                                                                    ActUIElement.eElementAction.SendKeys,
                                                                    ActUIElement.eElementAction.MultiSetValue,
                                                                    ActUIElement.eElementAction.ClearValue,
                                                                    ActUIElement.eElementAction.IsValuePopulated,
                                                                    ActUIElement.eElementAction.GetText,
                                                                    ActUIElement.eElementAction.GetTextLength,
                                                                    ActUIElement.eElementAction.GetValue,
                                                                    ActUIElement.eElementAction.GetFont,
                                                                    ],
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.ComboBox,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [  ActUIElement.eElementAction.Select,
                                                                    ActUIElement.eElementAction.SelectByIndex,
                                                                    ActUIElement.eElementAction.SelectByText,
                                                                    ActUIElement.eElementAction.SetValue,
                                                                    ActUIElement.eElementAction.ClearValue,
                                                                    ActUIElement.eElementAction.GetValidValues,
                                                                    ActUIElement.eElementAction.GetSelectedValue,
                                                                    ActUIElement.eElementAction.IsValuePopulated,
                                                                    ActUIElement.eElementAction.GetText,
                                                                    ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.ScrollBar,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = []
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.HyperLink,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                    ActUIElement.eElementAction.DoubleClick,
                                                                    ActUIElement.eElementAction.JavaScriptClick,
                                                                    ActUIElement.eElementAction.MouseClick,
                                                                    ActUIElement.eElementAction.MousePressRelease,
                                                                    ActUIElement.eElementAction.ClickAndValidate,
                                                                    ActUIElement.eElementAction.MultiClicks,
                                                                    ActUIElement.eElementAction.GetValue]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Label,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [ ActUIElement.eElementAction.GetValue,
                                                                   ActUIElement.eElementAction.GetText,
                                                                   ActUIElement.eElementAction.GetFont]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Text,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [ ActUIElement.eElementAction.GetValue,
                                                                   ActUIElement.eElementAction.GetText,
                                                                   ActUIElement.eElementAction.GetFont]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.List,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                   ActUIElement.eElementAction.Select,
                                                                   ActUIElement.eElementAction.SelectByIndex,
                                                                   ActUIElement.eElementAction.SelectByText,
                                                                   ActUIElement.eElementAction.ClearValue,
                                                                   ActUIElement.eElementAction.GetValidValues,
                                                                   ActUIElement.eElementAction.GetSelectedValue,
                                                                   ActUIElement.eElementAction.IsValuePopulated,
                                                                   ActUIElement.eElementAction.GetValue]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.TableItem,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                    ActUIElement.eElementAction.GetValue,
                                                                    ActUIElement.eElementAction.SetValue,

                                                                  ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Div,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [ ActUIElement.eElementAction.GetValue,
                                                                   ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Svg,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [ ActUIElement.eElementAction.GetValue,
                                                                   ActUIElement.eElementAction.SetValue,
                                                                   ActUIElement.eElementAction.DoubleClick,
                                                                    ActUIElement.eElementAction.JavaScriptClick,
                                                                    ActUIElement.eElementAction.MouseClick,
                                                                    ActUIElement.eElementAction.MousePressRelease,
                                                                    ActUIElement.eElementAction.ClickAndValidate,
                                                                    ActUIElement.eElementAction.MultiClicks
                                                                   ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Span,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [ ActUIElement.eElementAction.GetValue,
                                                                   ActUIElement.eElementAction.SetValue,

                                                                   ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.RadioButton,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [
                                                                    ActUIElement.eElementAction.DoubleClick,
                                                                    ActUIElement.eElementAction.JavaScriptClick,
                                                                    ActUIElement.eElementAction.MouseClick,
                                                                    ActUIElement.eElementAction.MousePressRelease,
                                                                    ActUIElement.eElementAction.ClickAndValidate,
                                                                    ActUIElement.eElementAction.MultiClicks,
                                                                    ActUIElement.eElementAction.GetValue ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Image,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = []
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Iframe,
                        IsCommonElementType = true,
                        ActionType = typeof(ActBrowserElement),
                        ElementOperationsList = [  ActBrowserElement.eControlAction.SwitchFrame,
                                                                    ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                                                                    ActBrowserElement.eControlAction.SwitchToParentFrame ]
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Canvas,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = [  ActUIElement.eElementAction.ClickXY,
                                                                    ActUIElement.eElementAction.DrawObject,
                                                                    ActUIElement.eElementAction.DoubleClickXY,
                                                                    ActUIElement.eElementAction.SendKeysXY,
                                                                   ]
                    },
                ];

                // adding generic/common actions per each ElementType
                List<Enum> ElementCommonActionsList = [
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
                                                                            ActUIElement.eElementAction.Click,
                ];

                mPlatformElementTypeOperations.Where(y => y.ActionType == typeof(ActUIElement)).ToList()
                                                 .ForEach(z => z.ElementOperationsList = z.ElementOperationsList.Union(ElementCommonActionsList).ToList());


                //Adding to support switch window in UIelement
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Window,
                    ActionType = typeof(ActUIElement),
                    ElementOperationsList = [ActUIElement.eElementAction.Switch]
                });

                //------Must be last one for calculating all supported Element operations
                List<Enum> allSupportedOperations = [];
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
                mElementLocatorsTypeList =
                [
                    eLocateBy.POMElement,
                    eLocateBy.ByID,
                    eLocateBy.ByName,
                    eLocateBy.ByXPath,
                    eLocateBy.ByCSS,
                    eLocateBy.ByClassName,
                    eLocateBy.ByXY,
                    eLocateBy.ByMulitpleProperties,
                    eLocateBy.ByRelXPath,
                    eLocateBy.ByContainerName,
                    eLocateBy.ByHref,
                    eLocateBy.ByLinkText,
                    eLocateBy.ByValue,
                    eLocateBy.ByIndex,
                    eLocateBy.ByAutomationID,
                    eLocateBy.ByLocalizedControlType,
                    eLocateBy.ByBoundingRectangle,
                    eLocateBy.IsEnabled,
                    eLocateBy.IsOffscreen,
                    eLocateBy.ByTitle,
                    eLocateBy.ByCaretPosition,
                    eLocateBy.ByUrl,
                    eLocateBy.ByngModel,
                    eLocateBy.ByngRepeat,
                    eLocateBy.ByngBind,
                    eLocateBy.ByngSelectedOption,
                    eLocateBy.ByResourceID,
                    eLocateBy.ByContentDescription,
                    eLocateBy.ByText,
                    eLocateBy.ByElementsRepository,
                    eLocateBy.ByCSSSelector,
                    eLocateBy.ByModelName,//???
                    eLocateBy.NA,
                    eLocateBy.Unknown,
                    eLocateBy.ByTagName,
                    eLocateBy.ByLabel,
                    eLocateBy.ByPlaceholder,
                    eLocateBy.ByAltText,
                    eLocateBy.ByTestID,
                    eLocateBy.Chained,
                    eLocateBy.ByAriaLabel,
                    eLocateBy.ByPartialLinkText,
                    eLocateBy.ByCustomXPath,
                    eLocateBy.ByDataAttribute,
                    eLocateBy.ByDataTestId

                ];
            }
            return mElementLocatorsTypeList;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            //TODO: cache in hashmap per elem type

            List<string> list =
            [
                //TODO: map all missing HTML tags and common attrs

                // add attr which exist for all HTML tags
                "id",
                "name",
                "TagName",
                "class",
            ];

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
            ObservableList<ElementLocator> learningLocatorsList =
            [
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID, Help = "Highly Recommended (usually unique)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Highly Recommended (usually unique)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath, Help = "Highly Recommended (usually unique)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByDataTestId, Help = "Highly Recommended (usually unique)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByCSSSelector, Help = "Highly Recommended (usually unique)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByPlaceholder, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByClassName, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByLinkText, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByHref, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByAriaLabel, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByTitle, Help = "Less Recommended (sensitive to page design changes)", EnableFriendlyLocator = false },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByTagName, Help = "Less Recommended", EnableFriendlyLocator = true },
            ];
            return learningLocatorsList;
        }

        public override List<ePosition> GetElementPositionList()
        {
            if (mElementPositionList == null)
            {
                //Arrange Position on priority basis
                mElementPositionList =
                [
                    ePosition.left,
                    ePosition.right,
                    ePosition.above,
                    ePosition.below,
                    ePosition.near,
                ];
            }
            return mElementPositionList;
        }
    }
}