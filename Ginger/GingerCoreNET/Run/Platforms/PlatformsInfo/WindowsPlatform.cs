#region License
/*
Copyright © 2014-2023 European Support Limited

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
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class WindowsPlatform : PlatformInfoBase
    {
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.POMElement);
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
            List<ActUIElement.eElementAction> windowsPlatformElementActionslist = new List<ActUIElement.eElementAction>();

            switch (ElementType)
            {
                case eElementType.Unknown:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.RightClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetSelectedValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Toggle);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Expand);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Collapse);

                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Maximize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Minimize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.DragDrop);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickAndValidate);
                    break;

                case eElementType.Button:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickAndValidate);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                
                    break;

                case eElementType.TextBox:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);                  
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    break;

                case eElementType.Label:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    break;

                case eElementType.ComboBox:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    break;

                case eElementType.Tab:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);

                    break;

                case eElementType.TabItem:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    break;


                case eElementType.CheckBox:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Toggle);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    break;

                case eElementType.RadioButton:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);

                    break;

                case eElementType.List:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetSelectedValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    break;

                case eElementType.ListItem:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);

                    break;

                case eElementType.TreeView:                    
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    break;

                case eElementType.TreeItem:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Expand);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);

                    break;

          

                case eElementType.DatePicker:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    break;

                //case eElementType.ScrollBar:
                //    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                //    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                //    break;

                case eElementType.Dialog:
                case eElementType.Window:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Switch);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.CloseWindow);
                    //windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.IsExist);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Minimize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Maximize);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetWindowTitle);
                    break;

                case eElementType.MenuBar:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    //windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    break;

                case eElementType.MenuItem:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Collapse);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Expand);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);

                    break;
                case eElementType.HyperLink:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);

                    break;
                case eElementType.Document:
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SetText);
                    windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);

                    break;
            }

            windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.IsExist);
            if(ElementType != eElementType.Window)
            {
                windowsPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
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
            list.Add(ActUIElement.eElementAction.Click);
            list.Add(ActUIElement.eElementAction.MouseClick);
            list.Add(ActUIElement.eElementAction.AsyncClick);

            return list;
        }
        public override List<ActUIElement.eElementProperty> GetPlatformElementProperties()
        {
            List<ActUIElement.eElementProperty> elementPropertyList = new List<ActUIElement.eElementProperty>();
            elementPropertyList.Add(ActUIElement.eElementProperty.Name);
            elementPropertyList.Add(ActUIElement.eElementProperty.Value);
            elementPropertyList.Add(ActUIElement.eElementProperty.Enabled);
            //elementPropertyList.Add(ActUIElement.eElementProperty.IsSelected);
            elementPropertyList.Add(ActUIElement.eElementProperty.ClassName);
            elementPropertyList.Add(ActUIElement.eElementProperty.Type);
            elementPropertyList.Add(ActUIElement.eElementProperty.AutomationId);
            elementPropertyList.Add(ActUIElement.eElementProperty.ProcessId);
            elementPropertyList.Add(ActUIElement.eElementProperty.XCoordinate);
            elementPropertyList.Add(ActUIElement.eElementProperty.YCoordinate);
            return elementPropertyList;
        }

        /// <summary>
        /// Return list of element Types supported for this platform
        /// </summary>
        /// <returns></returns>
        public override List<eElementType> GetPlatformUIElementsType()
        {
            // We cache the results
            if (mElementsTypeList == null)
            {
                mElementsTypeList = new List<eElementType>();

                mElementsTypeList.Add(eElementType.Unknown);
                mElementsTypeList.Add(eElementType.Button);
                mElementsTypeList.Add(eElementType.TextBox);
                mElementsTypeList.Add(eElementType.Label);
                mElementsTypeList.Add(eElementType.ComboBox);
                mElementsTypeList.Add(eElementType.Tab);
                mElementsTypeList.Add(eElementType.TabItem);
                mElementsTypeList.Add(eElementType.CheckBox);
                mElementsTypeList.Add(eElementType.RadioButton);
                mElementsTypeList.Add(eElementType.List);
                mElementsTypeList.Add(eElementType.ListItem);
                mElementsTypeList.Add(eElementType.TreeView);
                mElementsTypeList.Add(eElementType.TreeItem);
                mElementsTypeList.Add(eElementType.DatePicker);
                mElementsTypeList.Add(eElementType.HyperLink);
                mElementsTypeList.Add(eElementType.Document);
                mElementsTypeList.Add(eElementType.Window);
                mElementsTypeList.Add(eElementType.Dialog);
                mElementsTypeList.Add(eElementType.MenuItem);
                mElementsTypeList.Add(eElementType.MenuBar);
                mElementsTypeList.Add(eElementType.Image);
                //mElementsTypeList.Add(eElementType.Browser);
                //mElementsTypeList.Add(eElementType.ScrollBar);
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
            ObservableList<ElementLocator> learningLocatorsList = new ObservableList<ElementLocator>();
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByAutomationID, Help = "Recommended (usually stable)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Recommended (sensitive to page design changes)" });

            return learningLocatorsList;
        }

        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = new List<ElementTypeData>();
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Unknown,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Button,
                    IsCommonElementType = true
                });

                //mPlatformElementTypeOperations.Add(new ElementTypeData()
                //{
                //    ElementType = eElementType.ScrollBar,
                //    IsCommonElementType = false
                //});

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ComboBox,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.RadioButton,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TextBox,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.CheckBox,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Label,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.List,
                    IsCommonElementType = true
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.ListItem,
                    IsCommonElementType = false
                });

                //mPlatformElementTypeOperations.Add(new ElementTypeData()
                //{
                //    ElementType = eElementType.MenuItem,
                //    IsCommonElementType = true
                //});

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Window,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Tab,
                    IsCommonElementType = true
                });


                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TabItem,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TreeView,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.TreeItem,
                    IsCommonElementType = false
                });
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.DatePicker,
                    IsCommonElementType = true
                });
                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Dialog,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.HyperLink,
                    IsCommonElementType = false
                });

                mPlatformElementTypeOperations.Add(new ElementTypeData()
                {
                    ElementType = eElementType.Document,
                    IsCommonElementType = false
                });

            }
            return mPlatformElementTypeOperations;
        }
        private string SetElementExtInfo(eElementType elementType)
        {
            var elementExtInfo = string.Empty;
            switch (elementType)
            {
                case eElementType.Browser:
                case eElementType.Div:
                case eElementType.Span:
                case eElementType.HyperLink:
                    elementExtInfo = "For Embedded Html";
                    break;
                default:
                    elementExtInfo = string.Empty;
                    break;
            }
            return elementExtInfo;
        }
        public override Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList()
        {
            ObservableList<UIElementFilter> uIBasicElementFilters = new ObservableList<UIElementFilter>();
            ObservableList<UIElementFilter> uIAdvancedElementFilters = new ObservableList<UIElementFilter>();
            foreach (ElementTypeData elementTypeOperation in GetPlatformElementTypesData())
            {
                var elementExtInfo = SetElementExtInfo(elementTypeOperation.ElementType);
                if (elementTypeOperation.IsCommonElementType)
                {
                    uIBasicElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, elementExtInfo, true));
                }
                else
                {
                    uIAdvancedElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, elementExtInfo, false));
                }
            }

            Dictionary<string, ObservableList<UIElementFilter>> elementListDic = new Dictionary<string, ObservableList<UIElementFilter>>();
            elementListDic.Add("Basic", new ObservableList<UIElementFilter>(uIBasicElementFilters));
            elementListDic.Add("Advanced", new ObservableList<UIElementFilter>(uIAdvancedElementFilters));

            return elementListDic;
        }
        public override Act GetPlatformActionByElementInfo(ElementInfo elementInfo, ElementActionCongifuration actConfig)
        {
            var pomExcutionUtil = new POMExecutionUtils();
            Act elementAction = null;
            if (elementInfo != null)
            {
                List<ActUIElement.eElementAction> elementTypeOperations;
                if (elementInfo.GetType().Equals(typeof(HTMLElementInfo)))
                {
                    elementTypeOperations = GetPlatformWidgetsUIActionsList(elementInfo.ElementTypeEnum);
                }
                else
                {
                    elementTypeOperations = GetPlatformUIElementActionsList(elementInfo.ElementTypeEnum);
                }
                if (actConfig != null)
                {
                    if (string.IsNullOrWhiteSpace(actConfig.Operation))
                        actConfig.Operation = GetDefaultElementOperation(elementInfo.ElementTypeEnum);
                }
                if ((elementTypeOperations != null) && (elementTypeOperations.Count > 0))
                {
                    elementAction = new ActUIElement()
                    {
                        Description = string.IsNullOrWhiteSpace(actConfig.Description) ? "UI Element Action : " + actConfig.Operation + " - " + elementInfo.ItemName : actConfig.Description,
                        ElementAction = (ActUIElement.eElementAction)System.Enum.Parse(typeof(ActUIElement.eElementAction), actConfig.Operation),
                        ElementLocateValue = actConfig.LocateValue,
                        Value = actConfig.ElementValue
                    };

                    if (elementInfo.ElementTypeEnum.Equals(eElementType.Table))
                    {
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, actConfig.WhereColumnValue);
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.LocateRowType, actConfig.LocateRowType);
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue, actConfig.RowValue);
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue, actConfig.ColSelectorValue);
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle, actConfig.LocateColTitle);
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, actConfig.ControlAction);
                    }
                    if (elementInfo.GetType().Equals(typeof(HTMLElementInfo)))
                    {
                        elementAction.GetOrCreateInputParam(ActUIElement.Fields.IsWidgetsElement, "true");
                    }
                    pomExcutionUtil.SetPOMProperties(elementAction, elementInfo, actConfig);
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
                case eElementType.TabItem:
                case eElementType.RadioButton:
                case eElementType.ListItem:
                case eElementType.TreeItem:
                case eElementType.HyperLink:
                case eElementType.MenuItem:
                    return ActUIElement.eElementAction.Click.ToString();

                case eElementType.Label:
                    return ActUIElement.eElementAction.GetValue.ToString();

                case eElementType.Tab:
                case eElementType.List:
                case eElementType.TreeView:
                    return ActUIElement.eElementAction.Select.ToString();

                case eElementType.TextBox:
                case eElementType.ComboBox:
                case eElementType.Document:
                case eElementType.DatePicker:
                    return ActUIElement.eElementAction.SetValue.ToString();


                case eElementType.Window:                    
                case eElementType.Dialog:
                    return ActUIElement.eElementAction.Switch.ToString();


                case eElementType.CheckBox:
                    return ActUIElement.eElementAction.Toggle.ToString();

                case eElementType.MenuBar:
                    return ActUIElement.eElementAction.Click.ToString();

                //case eElementType.Browser:
                //    return ActBrowserElement.eControlAction.InitializeBrowser.ToString();//Need to test

                //case eElementType.ScrollBar:
                //    return ActUIElement.eElementAction.ScrollDown.ToString();


                default:
                    return ActUIElement.eElementAction.IsExist.ToString();
            }
        }

        public static eElementType GetElementType(string elementType, string elementClass)
        {
            eElementType elementTypeEnum;

            if (elementType.Equals("button", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Button;
            }
            else if (elementType.Equals("edit", StringComparison.OrdinalIgnoreCase) || elementType.Equals("edit box", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.TextBox;
            }
            else if (elementType.Equals("label", StringComparison.OrdinalIgnoreCase) || elementType.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Label;
            }
            else if (elementType.Equals("combo box", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.ComboBox;
            }
            else if (elementType.Equals("tab", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Tab;
            }
            else if (elementType.Equals("tab item", StringComparison.OrdinalIgnoreCase) || elementType.Equals("item", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.TabItem;
            }
            else if (elementType.Equals("check box", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.CheckBox;
            }
            else if (elementType.Equals("radio button", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.RadioButton;
            }
            else if (elementType.Equals("list view", StringComparison.OrdinalIgnoreCase) || elementType.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.List;
            }
            else if (elementType.Equals("list item", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.ListItem;
            }
            else if (elementType.Equals("tree view", StringComparison.OrdinalIgnoreCase) || elementType.Equals("tree", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.TreeView;
            }
            else if (elementType.Equals("tree view item", StringComparison.OrdinalIgnoreCase) || elementType.Equals("tree item", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.TreeItem;
            }
            else if (elementType.Equals("link", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.HyperLink;
            }
            else if (elementType.Equals("document", StringComparison.OrdinalIgnoreCase) || elementType.Equals("", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Document;
            }
            else if (elementType.Equals("dialog", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Dialog;
            }
            else if (elementType.Equals("window", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Window;
            }
            else if (elementType.Equals("menu item", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.MenuItem;
            }
            else if (elementType.Equals("menu bar", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.MenuBar;
            }
            else if (elementType.Equals("image", StringComparison.OrdinalIgnoreCase))
            {
                elementTypeEnum = eElementType.Image;
            }
            else if (elementType.Equals("pane", StringComparison.OrdinalIgnoreCase))
            {
                // TODO: Remove Dependency on class name. Find a generic way
                if (elementClass.Contains("SysDateTimePick32"))
                {
                    elementTypeEnum = eElementType.DatePicker;
                }
                //else if (elementClass == "Internet Explorer_Server")
                //{
                //    elementTypeEnum = eElementType.Browser;
                //}
                else
                {
                    elementTypeEnum = eElementType.Unknown;
                }
            }
            else
            {
                elementTypeEnum = eElementType.Unknown;
            }
            
            return elementTypeEnum;
        }

        public override string GetPageUrlRadioLabelText()
        {
            return "Window Title";
        }

        public override string GetNextBtnToolTip()
        {
            return "Switch Window";
        }

        /// <summary>
        /// This method is used to check if the paltform includes GUI based Application Instance and thus, supports Sikuli based operations
        /// </summary>
        /// <returns></returns>
        public override bool IsSikuliSupported()
        {
            return true;
        }

        public override List<ePosition> GetElementPositionList()
        {
            throw new NotImplementedException();
        }
    }
}
