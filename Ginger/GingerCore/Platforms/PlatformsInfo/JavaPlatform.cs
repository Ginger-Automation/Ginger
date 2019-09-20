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
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Platforms.PlatformsInfo
{
    public class JavaPlatform : PlatformInfoBase
    {


        private List<eElementType> mJavaPlatformElementActionslist { get; set; } = null;

        public override List<ActUIElement.eElementAction> GetPlatformUIValidationTypesList()
        {
            throw new NotImplementedException();
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIClickTypeList()
        {
            throw new NotImplementedException();
        }
        public override List<ActBrowserElement.eControlAction> GetPlatformBrowserControlOperations()
        {
            List<ActBrowserElement.eControlAction> browserActElementList = new List<ActBrowserElement.eControlAction>();

            browserActElementList.Add(ActBrowserElement.eControlAction.InitializeBrowser);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageSource);
            browserActElementList.Add(ActBrowserElement.eControlAction.GetPageURL);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchToDefaultFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.SwitchFrame);
            browserActElementList.Add(ActBrowserElement.eControlAction.RunJavaScript);
            return browserActElementList;
        }
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                //Arrange locator on priority basis
                mElementLocatorsTypeList = new List<eLocateBy>();
                mElementLocatorsTypeList.Add(eLocateBy.POMElement);
                mElementLocatorsTypeList.Add(eLocateBy.ByXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByName);
                mElementLocatorsTypeList.Add(eLocateBy.ByMulitpleProperties);
                mElementLocatorsTypeList.Add(eLocateBy.ByID);
                mElementLocatorsTypeList.Add(eLocateBy.ByRelXPath);
                mElementLocatorsTypeList.Add(eLocateBy.ByContainerName);
                mElementLocatorsTypeList.Add(eLocateBy.ByTitle);
                mElementLocatorsTypeList.Add(eLocateBy.ByClassName);
                mElementLocatorsTypeList.Add(eLocateBy.ByText);
                mElementLocatorsTypeList.Add(eLocateBy.ByCSSSelector);
                mElementLocatorsTypeList.Add(eLocateBy.ByValue);
            }
            return mElementLocatorsTypeList;
        }

        public override ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            if (elementInfo.GetType() == typeof(HTMLElementInfo))
            {
                return GetWidgetUIElementList(elementInfo);
            }

            ObservableList<Act> UIElementsActionsList = new ObservableList<Act>();
            eElementType elementType = GetElementType(elementInfo.ElementType);

            if (elementType.Equals(eElementType.Table))
            {
                //get all action list supported to table
                var tableActionList = new[] { ActUIElement.eElementAction.TableCellAction, ActUIElement.eElementAction.TableAction, ActUIElement.eElementAction.TableRowAction }
                                            .SelectMany(action => GetTableControlActions(action))
                                            .ToList();
                foreach (var action in tableActionList)
                {
                    var elementAction = ActUIElement.eElementAction.TableCellAction;

                    if (action.Equals(ActUIElement.eTableAction.GetRowCount) || action.Equals(ActUIElement.eTableAction.SelectAllRows))
                    {
                        elementAction = ActUIElement.eElementAction.TableAction;
                    }
                    else if (action.Equals(ActUIElement.eTableAction.GetSelectedRow) || action.Equals(ActUIElement.eTableAction.ActivateRow))
                    {
                        elementAction = ActUIElement.eElementAction.TableRowAction;
                    }

                    var actUITableAction = new ActUIElement()
                    {
                        Description = action + " : " + elementInfo.ElementTitle,
                        ElementType = eElementType.Table,
                        ElementAction = elementAction,
                    };
                    actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, action.ToString());
                    actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.Medium.ToString());
                    if (!action.Equals(ActUIElement.eElementAction.TableAction))
                    {
                        actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.RowSelectorRadioParam, "RowNum");
                        actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.LocateRowType, "Row Number");
                        actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.LocateRowValue, "0");

                        actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColNum.ToString());
                        actUITableAction.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle, "0");
                    }
                    UIElementsActionsList.Add(actUITableAction);
                }
            }
            else
            {
                var actionList = GetPlatformUIElementActionsList(elementType);

                if (actionList.Count > 0)
                {
                    foreach (var action in actionList)
                    {
                        UIElementsActionsList.Add(CreateUIElementAction(elementInfo, elementType, action));
                    }
                }
            }
            
            return UIElementsActionsList;
        }

        private static ActUIElement CreateUIElementAction(ElementInfo elementInfo, eElementType elementType, ActUIElement.eElementAction action)
        {
            return new ActUIElement()
            {
                Description = action + " : " + elementInfo.ElementTitle,
                ElementAction = action,
                ElementType = elementType,
            };
        }

        private ObservableList<Act> GetWidgetUIElementList(ElementInfo elementInfo)
        {
            var widgetsActionList = GetPlatformWidgetsUIActionsList(elementInfo.ElementTypeEnum);

            ObservableList<Act> UIElementsActionsList = new ObservableList<Act>();

            if (widgetsActionList.Count > 0)
            {
                foreach (var action in widgetsActionList)
                {
                    var widgetsAction = CreateUIElementAction(elementInfo, elementInfo.ElementTypeEnum, action);

                    widgetsAction.GetOrCreateInputParam(ActUIElement.Fields.IsWidgetsElement, "true");

                    UIElementsActionsList.Add(widgetsAction);
                }
            }

            return UIElementsActionsList;
        }

        private eElementType GetElementType(string elementType)
        {
            eElementType element;

            switch (elementType)
            {
                case "javax.swing.JTextField":
                case "javax.swing.JTextPane":
                case "javax.swing.JTextArea":
                    element = eElementType.TextBox;
                    break;
                case "javax.swing.JButton":
                    element = eElementType.Button;
                    break;
                case "javax.swing.JLabel":
                    element = eElementType.Label;
                    break;
                case "javax.swing.JCheckBox":
                    element = eElementType.CheckBox;
                    break;
                case "javax.swing.JRadioButton":
                    element = eElementType.RadioButton;
                    break;
                case "javax.swing.JComboBox":
                    element = eElementType.ComboBox;
                    break;
                case "javax.swing.JList":
                    element = eElementType.List;
                    break;
                case "javax.swing.JTable":
                    element = eElementType.Table;
                    break;
                case "javax.swing.JScrollPane":
                case "javax.swing.JScrollPane$ScrollBar":
                    element = eElementType.ScrollBar;
                    break;
                case "javax.swing.JTree":
                    element = eElementType.TreeView;
                    break;
                case "javax.swing.JMenu":
                    element = eElementType.MenuItem;
                    break;
                case "javax.swing.JTabbedPane":
                    element = eElementType.Tab;
                    break;
                case "javax.swing.JEditorPane":
                    element = eElementType.EditorPane;
                    break;
                default:
                    element = eElementType.Unknown;
                    break;
            }


            return element;
        }

        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> javaPlatformElementActionslist = base.GetPlatformUIElementActionsList(ElementType);
          
            switch (ElementType)
            {
                case eElementType.Button:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.winDoubleClick);
                    break;
                case eElementType.TextBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeyPressRelease);
                    break;
                case eElementType.ComboBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    break;
                case eElementType.CheckBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Toggle);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsChecked);
                    break;
                case eElementType.RadioButton:
                   
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);                  
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    break;
                case eElementType.List:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    break;
                case eElementType.Label:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    break;
                case eElementType.MenuItem:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    break;
                case eElementType.Window:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.CloseWindow);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsExist);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Switch);
                    break;
                case eElementType.Tab:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    break;
                case eElementType.EditorPane:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.InitializeJEditorPane);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.JEditorPaneElementAction);
                    break;
                case eElementType.TreeView:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetSelectedNodeChildItems);
                    break;
            }

            return javaPlatformElementActionslist;
        }

        public override List<ActUIElement.eElementAction> GetPlatformWidgetsUIActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> widgetsActionslist = new List<ActUIElement.eElementAction>();
            
            //common action type for all elementType
            widgetsActionslist.Add(ActUIElement.eElementAction.IsVisible);
            widgetsActionslist.Add(ActUIElement.eElementAction.RunJavaScript);
            switch (ElementType)
            {
                case eElementType.Button:
                    widgetsActionslist.Add(ActUIElement.eElementAction.Click);
                    widgetsActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    widgetsActionslist.Add(ActUIElement.eElementAction.TriggerJavaScriptEvent);
                    break;

                case eElementType.TextBox:
                    widgetsActionslist.Add(ActUIElement.eElementAction.SetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    widgetsActionslist.Add(ActUIElement.eElementAction.TriggerJavaScriptEvent);
                    break;

                case eElementType.ComboBox:
                    widgetsActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    widgetsActionslist.Add(ActUIElement.eElementAction.Select);
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    break;


                case eElementType.ScrollBar:
                    widgetsActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                    widgetsActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                    break;

                case eElementType.RadioButton:
                case eElementType.CheckBox:
                case eElementType.Span:
                    widgetsActionslist.Add(ActUIElement.eElementAction.Click);
                    widgetsActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    break;

                case eElementType.Label:
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    break;

                default:
                    widgetsActionslist.Add(ActUIElement.eElementAction.Click);
                    widgetsActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    widgetsActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    widgetsActionslist.Add(ActUIElement.eElementAction.Select);
                    widgetsActionslist.Add(ActUIElement.eElementAction.SetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetValue);
                    widgetsActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    widgetsActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                    widgetsActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                    widgetsActionslist.Add(ActUIElement.eElementAction.TriggerJavaScriptEvent);
                    break;
            }

            return widgetsActionslist;
        }

        public override List<eElementType> GetPlatformWidgetsUIElementsType()
        {
            var mWidgetsElementsTypeList = new List<eElementType>();
            mWidgetsElementsTypeList.Add(eElementType.Unknown);
            mWidgetsElementsTypeList.Add(eElementType.Button);
            mWidgetsElementsTypeList.Add(eElementType.ScrollBar);
            mWidgetsElementsTypeList.Add(eElementType.ComboBox);
            mWidgetsElementsTypeList.Add(eElementType.RadioButton);
            mWidgetsElementsTypeList.Add(eElementType.TextBox);
            mWidgetsElementsTypeList.Add(eElementType.CheckBox);
            mWidgetsElementsTypeList.Add(eElementType.Label);
            mWidgetsElementsTypeList.Add(eElementType.Span);

            return mWidgetsElementsTypeList;
        }
        public override string GetPlatformGenericElementEditControls()
        {
            return "UIElementJavaPlatformPage";
        }

        public override List<ActUIElement.eElementProperty> GetPlatformElementProperties()
        {
            List<ActUIElement.eElementProperty> elementPropertyList = base.GetPlatformElementProperties();

            elementPropertyList.Add(ActUIElement.eElementProperty.DateTimeValue);
            elementPropertyList.Add(ActUIElement.eElementProperty.HTML);
            elementPropertyList.Add(ActUIElement.eElementProperty.List);//????
            elementPropertyList.Add(ActUIElement.eElementProperty.ToggleState);
            return elementPropertyList;
        }

        public override List<eElementType> GetPlatformUIElementsType()
        {
            if(mJavaPlatformElementActionslist==null)
            {
                mJavaPlatformElementActionslist = base.GetPlatformUIElementsType();
                mJavaPlatformElementActionslist.Add(eElementType.EditorPane);
                mJavaPlatformElementActionslist.Add(eElementType.TreeView);
            }
            return mJavaPlatformElementActionslist;
        }

        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations = new List<ElementTypeData>();
                foreach(eElementType eElementType in GetPlatformUIElementsType())
                {                   
                    mPlatformElementTypeOperations.Add(new ElementTypeData()
                    {
                        ElementType = eElementType,
                        IsCommonElementType = true,
                        ActionType = typeof(ActUIElement),
                        ElementOperationsList = GetPlatformUIElementActionsList(eElementType).ConvertAll(delegate (ActUIElement.eElementAction x) {
                            return (Enum)Enum.Parse(typeof(Enum), x.ToString());
                        }) 
                    });
                }
            }
            return mPlatformElementTypeOperations;
        }

        public override List<ActUIElement.eSubElementType> GetSubElementType(eElementType elementType)
        {
            List<ActUIElement.eSubElementType> list = new List<ActUIElement.eSubElementType>();
            switch (elementType)
            {
                case eElementType.EditorPane:
                    list.Add(ActUIElement.eSubElementType.HTMLTable);
                    break;
            }
            return list;
        }

        public override List<ActUIElement.eElementAction> GetSubElementAction(ActUIElement.eSubElementType subElementType)
        {
            List<ActUIElement.eElementAction> list = new List<ActUIElement.eElementAction>();
            switch (subElementType)
            {
                case ActUIElement.eSubElementType.HTMLTable:
                    list.Add(ActUIElement.eElementAction.TableAction);
                    list.Add(ActUIElement.eElementAction.TableCellAction);
                    list.Add(ActUIElement.eElementAction.TableRowAction);
                    break;
                default:
                    list.Add(ActUIElement.eElementAction.TableAction);
                    list.Add(ActUIElement.eElementAction.TableCellAction);
                    list.Add(ActUIElement.eElementAction.TableRowAction);
                    break;
            }
            return list;
        }

        public override List<string> GetPlatformUIElementPropertiesList(eElementType ElementType)
        {
            throw new NotImplementedException();
        }

        public override ePlatformType PlatformType()
        {
            return ePlatformType.Java;
        }

        public override List<ActUIElement.eTableAction> GetTableControlActions(ActUIElement.eElementAction tableAction)
        {
            List<ActUIElement.eTableAction> list = new List<ActUIElement.eTableAction>();
            switch (tableAction)
            {
                case ActUIElement.eElementAction.TableCellAction:
                    list.Add(ActUIElement.eTableAction.IsCellEnabled);
                    list.Add(ActUIElement.eTableAction.IsVisible);
                    list.Add(ActUIElement.eTableAction.GetValue);
                    list.Add(ActUIElement.eTableAction.SetValue);
                    list.Add(ActUIElement.eTableAction.SetFocus);
                    list.Add(ActUIElement.eTableAction.Click);
                    list.Add(ActUIElement.eTableAction.AsyncClick);
                    list.Add(ActUIElement.eTableAction.WinClick);
                    list.Add(ActUIElement.eTableAction.Toggle);
                    list.Add(ActUIElement.eTableAction.DoubleClick);
                    list.Add(ActUIElement.eTableAction.Type);
                    list.Add(ActUIElement.eTableAction.MousePressAndRelease);
                    list.Add(ActUIElement.eTableAction.IsChecked);
                    list.Add(ActUIElement.eTableAction.RightClick);
                    break;
                case ActUIElement.eElementAction.TableRowAction:
                    list.Add(ActUIElement.eTableAction.GetSelectedRow);
                    list.Add(ActUIElement.eTableAction.ActivateRow);
                    break;
                case ActUIElement.eElementAction.TableAction:
                    list.Add(ActUIElement.eTableAction.GetRowCount);
                    list.Add(ActUIElement.eTableAction.SelectAllRows);
                    //list.Add(ActUIElement.eTableAction.RightClick);
                    break;
                case ActUIElement.eElementAction.Unknown:
                    list.Add(ActUIElement.eTableAction.IsCellEnabled);
                    list.Add(ActUIElement.eTableAction.IsVisible);
                    list.Add(ActUIElement.eTableAction.GetValue);
                    list.Add(ActUIElement.eTableAction.SetValue);
                    list.Add(ActUIElement.eTableAction.SetFocus);
                    list.Add(ActUIElement.eTableAction.Click);
                    list.Add(ActUIElement.eTableAction.AsyncClick);
                    list.Add(ActUIElement.eTableAction.WinClick);
                    list.Add(ActUIElement.eTableAction.Toggle);
                    list.Add(ActUIElement.eTableAction.GetRowCount);
                    list.Add(ActUIElement.eTableAction.GetSelectedRow);
                    break;
            }
            return list;
        }

        public override List<ActUIElement.eElementDragDropType> GetPlatformDragDropTypeList()
        {
            throw new NotImplementedException();
        }

        public override ObservableList<ElementLocator> GetLearningLocators()
        {
            ObservableList<ElementLocator> learningLocatorsList = new ObservableList<ElementLocator>();
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByContainerName, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Very Recommended (usually unique)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByTitle, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByClassName, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByText, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID, Help = "Recommended (sensitive to page design changes)" });
            learningLocatorsList.Add(new ElementLocator() { Active = true, LocateBy = eLocateBy.ByCSSSelector, Help = "Recommended (sensitive to page design changes)" });           

            return learningLocatorsList;
        }
    }
}
