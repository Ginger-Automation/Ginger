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
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

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
            List<ActBrowserElement.eControlAction> browserActElementList =
            [
                ActBrowserElement.eControlAction.InitializeBrowser,
                ActBrowserElement.eControlAction.GetPageSource,
                ActBrowserElement.eControlAction.GetPageURL,
                ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                ActBrowserElement.eControlAction.SwitchFrame,
                ActBrowserElement.eControlAction.RunJavaScript,
            ];
            return browserActElementList;
        }
        public override List<eLocateBy> GetPlatformUIElementLocatorsList()
        {
            if (mElementLocatorsTypeList == null)
            {
                //Arrange locator on priority basis
                mElementLocatorsTypeList =
                [
                    eLocateBy.POMElement,
                    eLocateBy.ByXPath,
                    eLocateBy.ByName,
                    eLocateBy.ByMulitpleProperties,
                    eLocateBy.ByID,
                    eLocateBy.ByRelXPath,
                    eLocateBy.ByContainerName,
                    eLocateBy.ByTitle,
                    eLocateBy.ByClassName,
                    eLocateBy.ByText,
                    eLocateBy.ByCSSSelector,
                    eLocateBy.ByValue,
                ];
            }
            return mElementLocatorsTypeList;
        }

        public override ObservableList<Act> GetPlatformElementActions(ElementInfo elementInfo)
        {
            if (elementInfo.GetType() == typeof(HTMLElementInfo))
            {
                return GetWidgetUIElementList(elementInfo);
            }

            ObservableList<Act> UIElementsActionsList = [];

            if (elementInfo.ElementTypeEnum == eElementType.Table)
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
                var actionList = GetPlatformUIElementActionsList(elementInfo.ElementTypeEnum);

                if (actionList.Count > 0)
                {
                    foreach (var action in actionList)
                    {
                        UIElementsActionsList.Add(CreateUIElementAction(elementInfo, action));
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
                    {
                        actConfig.Operation = GetDefaultElementOperation(elementInfo.ElementTypeEnum);
                    }
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
            return ElementTypeEnum switch
            {
                eElementType.TreeView or eElementType.MenuItem or eElementType.List or eElementType.RadioButton or eElementType.Button or eElementType.TableItem => ActUIElement.eElementAction.Click.ToString(),
                eElementType.CheckBox => ActUIElement.eElementAction.Toggle.ToString(),
                eElementType.TextBox => ActUIElement.eElementAction.SetValue.ToString(),
                eElementType.Tab or eElementType.ComboBox => ActUIElement.eElementAction.Select.ToString(),
                eElementType.Span or eElementType.Label => ActUIElement.eElementAction.GetValue.ToString(),
                eElementType.Window => ActUIElement.eElementAction.Switch.ToString(),
                eElementType.EditorPane => ActUIElement.eElementAction.InitializeJEditorPane.ToString(),
                eElementType.Table => ActUIElement.eElementAction.TableCellAction.ToString(),
                eElementType.DatePicker => ActUIElement.eElementAction.SetDate.ToString(),
                eElementType.Dialog => ActUIElement.eElementAction.AcceptDialog.ToString(),
                eElementType.ScrollBar => ActUIElement.eElementAction.ScrollDown.ToString(),
                _ => ActUIElement.eElementAction.Unknown.ToString(),
            };
        }

        private static ActUIElement CreateUIElementAction(ElementInfo elementInfo, ActUIElement.eElementAction action)
        {
            return new ActUIElement()
            {
                Description = action + " : " + elementInfo.ElementTitle,
                ElementAction = action,
                ElementType = elementInfo.ElementTypeEnum
            };
        }

        private ObservableList<Act> GetWidgetUIElementList(ElementInfo elementInfo)
        {
            var widgetsActionList = GetPlatformWidgetsUIActionsList(elementInfo.ElementTypeEnum);

            ObservableList<Act> UIElementsActionsList = [];

            if (widgetsActionList.Count > 0)
            {
                foreach (var action in widgetsActionList)
                {
                    var widgetsAction = CreateUIElementAction(elementInfo, action);

                    widgetsAction.GetOrCreateInputParam(ActUIElement.Fields.IsWidgetsElement, "true");

                    UIElementsActionsList.Add(widgetsAction);
                }
            }

            return UIElementsActionsList;
        }

        public static eElementType GetElementType(string elementType)
        {
            //TODO: J.G all this logic should be moved to java side 
            //and payload should return simply element type as Buttton or TextBox etc.
            var elementTypeEnum = elementType switch
            {
                "javax.swing.JTextField" or "javax.swing.JTextPane" or "javax.swing.JTextArea" => eElementType.TextBox,
                "javax.swing.JButton" => eElementType.Button,
                "javax.swing.JLabel" => eElementType.Label,
                //  added to support live spy in JxBrowserBrowserComponent
                "com.amdocs.uif.widgets.browser.JxBrowserBrowserComponent" or "com.amdocs.uif.widgets.browser.JExplorerBrowserComponent" => eElementType.Browser,
                "javax.swing.JCheckBox" => eElementType.CheckBox,
                "javax.swing.JRadioButton" => eElementType.RadioButton,
                "com.amdocs.uif.widgets.CalendarComponent" or "com.amdocs.uif.widgets.DateTimeNative$2" or "lt.monarch.swing.JDateField$Editor" => eElementType.DatePicker,
                "javax.swing.JComboBox" or "com.amdocs.uif.widgets.ComboBoxNative$1" => eElementType.ComboBox,
                "javax.swing.JList" => eElementType.List,
                "javax.swing.JTable" or "com.amdocs.uif.widgets.search.SearchJTable" => eElementType.Table,
                "javax.swing.JScrollPane" or "javax.swing.JScrollPane$ScrollBar" => eElementType.ScrollBar,
                "javax.swing.JTree" or "com.amdocs.uif.widgets.TreeNative$SmartJTree" => eElementType.TreeView,
                "javax.swing.JMenu" => eElementType.MenuItem,
                "javax.swing.JTabbedPane" or "com.amdocs.uif.widgets.JXTabbedPane" => eElementType.Tab,
                "javax.swing.JEditorPane" => eElementType.EditorPane,
                "javax.swing.JInternalFrame" or "com.amdocs.uif.workspace.MDIWorkspace$27" => eElementType.Iframe,
                _ => eElementType.Unknown,
            };
            return elementTypeEnum;
        }

        public static eElementType GetHTMLElementType(string elementTypeString)
        {
            elementTypeString = elementTypeString.ToUpper();

            return elementTypeString switch
            {
                "INPUT.TEXT" or "TEXTAREA" or "INPUT.UNDEFINED" or "INPUT.PASSWORD" or "INPUT.EMAIL" or "HTMLTEXT" or "HTMLTEXTAREA" or "HTMLPASSWORD" => eElementType.TextBox,
                "INPUT.BUTTON" or "BUTTON" or "INPUT.IMAGE" or "INPUT.SUBMIT" or "HTMLBUTTON" or "HTMLSUBMIT" => eElementType.Button,
                "INPUT.CHECKBOX" or "HTMLCHECKBOX" => eElementType.CheckBox,
                "INPUT.RADIO" or "HTMLRADIO" => eElementType.RadioButton,
                "DIV" or "htmlDIV" or "HTMLP" => eElementType.Div,
                "SPAN" or "HTMLSPAN" => eElementType.Span,
                "IFRAME" => eElementType.Iframe,
                "TD" or "TH" => eElementType.TableItem,
                "LINK" or "A" or "HTMLA" => eElementType.HyperLink,
                "LABEL" => eElementType.Label,
                "SELECT" or "HTMLSELECT-ONE" => eElementType.ComboBox,
                "TABLE" => eElementType.Table,
                "JEDITOR.TABLE" => eElementType.EditorTable,
                "IMG" => eElementType.Image,
                "CANVAS" => eElementType.Canvas,
                "HTMLLI" => eElementType.ListItem,
                _ => eElementType.Unknown,
            };
        }
        public override List<ActUIElement.eElementAction> GetPlatformUIElementActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> javaPlatformElementActionslist = [];

            switch (ElementType)
            {
                case eElementType.Button:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.winDoubleClick);
                    break;
                case eElementType.TextBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetText);

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeyPressRelease);
                    //
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    break;
                case eElementType.ComboBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);


                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetAllValues);
                    //
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);


                    break;
                case eElementType.CheckBox:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Toggle);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    //

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsChecked);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClick);

                    break;
                case eElementType.RadioButton:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);

                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);

                    break;
                case eElementType.List:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);

                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);

                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    //javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
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
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
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
                case eElementType.Table:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.TableAction);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.TableCellAction);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.TableRowAction);
                    break;
                case eElementType.DatePicker:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetDate);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    break;

                case eElementType.ScrollBar:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollLeft);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollRight);
                    break;

                case eElementType.Dialog:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AcceptDialog);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DismissDialog);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetDialogText);
                    break;

                case eElementType.Unknown:
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Click);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ClickXY);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MouseClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.MousePressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetValue);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeys);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SendKeyPressRelease);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetControlProperty);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DoubleClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetSelectedNodeChildItems);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollDown);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollUp);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollLeft);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.ScrollRight);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Select);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncSelect);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SelectByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetItemCount);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetAllValues);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetValueByIndex);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Toggle);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AsyncClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetName);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.Switch);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsExist);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.CloseWindow);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.InitializeJEditorPane);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.JEditorPaneElementAction);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetDate);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.AcceptDialog);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.DismissDialog);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.GetDialogText);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsEnabled);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsVisible);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsMandatory);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.IsChecked);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.WinClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.winDoubleClick);
                    javaPlatformElementActionslist.Add(ActUIElement.eElementAction.SetFocus);
                    break;
            }

            return javaPlatformElementActionslist;
        }

        public override List<ActUIElement.eElementAction> GetPlatformWidgetsUIActionsList(eElementType ElementType)
        {
            List<ActUIElement.eElementAction> widgetsActionslist =
            [
                //common action type for all elementType
                ActUIElement.eElementAction.IsVisible,
                ActUIElement.eElementAction.RunJavaScript,
            ];
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
                case eElementType.TableItem:
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
                    widgetsActionslist.Add(ActUIElement.eElementAction.GetAttrValue);
                    break;
            }

            return widgetsActionslist;
        }

        public override List<eElementType> GetPlatformWidgetsUIElementsType()
        {
            var mWidgetsElementsTypeList = new List<eElementType>
            {
                eElementType.Unknown,
                eElementType.Button,
                eElementType.ScrollBar,
                eElementType.ComboBox,
                eElementType.RadioButton,
                eElementType.TextBox,
                eElementType.CheckBox,
                eElementType.Label,
                eElementType.Span,
                eElementType.Div,
                eElementType.TableItem
            };

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
            if (mJavaPlatformElementActionslist == null)
            {
                mJavaPlatformElementActionslist = base.GetPlatformUIElementsType();
                mJavaPlatformElementActionslist.Add(eElementType.EditorPane);
                mJavaPlatformElementActionslist.Add(eElementType.TreeView);
                mJavaPlatformElementActionslist.Add(eElementType.DatePicker);
                mJavaPlatformElementActionslist.Add(eElementType.Dialog);
            }
            return mJavaPlatformElementActionslist;
        }

        public List<ElementTypeData> GetPlatformElementTypesData()
        {
            if (mPlatformElementTypeOperations == null)
            {
                mPlatformElementTypeOperations =
                [
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Unknown,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Button,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.ScrollBar,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.ComboBox,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.RadioButton,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.TextBox,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.CheckBox,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Image,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Label,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.List,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Table,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.MenuItem,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Window,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Tab,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.EditorPane,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.TreeView,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.DatePicker,
                        IsCommonElementType = true
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Dialog,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Browser,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Div,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.Span,
                        IsCommonElementType = false
                    },
                    new ElementTypeData()
                    {
                        ElementType = eElementType.HyperLink,
                        IsCommonElementType = false
                    },
                ];
            }
            return mPlatformElementTypeOperations;
        }

        public override Dictionary<string, ObservableList<UIElementFilter>> GetUIElementFilterList()
        {
            ObservableList<UIElementFilter> uIBasicElementFilters = [];
            ObservableList<UIElementFilter> uIAdvancedElementFilters = [];
            foreach (ElementTypeData elementTypeOperation in GetPlatformElementTypesData())
            {
                var elementExtInfo = SetElementExtInfo(elementTypeOperation.ElementType);
                if (elementTypeOperation.IsCommonElementType)
                {
                    uIBasicElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, elementExtInfo, true));
                }
                else
                {
                    var isSelected = elementTypeOperation.IsCommonElementType;
                    if (elementTypeOperation.ElementType.Equals(eElementType.Browser))
                    {
                        isSelected = true;
                    }
                    uIAdvancedElementFilters.Add(new UIElementFilter(elementTypeOperation.ElementType, elementExtInfo, isSelected));
                }
            }

            Dictionary<string, ObservableList<UIElementFilter>> elementListDic = new Dictionary<string, ObservableList<UIElementFilter>>
            {
                { "Basic", new ObservableList<UIElementFilter>(uIBasicElementFilters) },
                { "Advanced", new ObservableList<UIElementFilter>(uIAdvancedElementFilters) }
            };

            return elementListDic;
        }

        private string SetElementExtInfo(eElementType elementType)
        {
            var elementExtInfo = string.Empty;
            elementExtInfo = elementType switch
            {
                eElementType.Browser or eElementType.Div or eElementType.Span or eElementType.HyperLink => "For Embedded Html",
                _ => string.Empty,
            };
            return elementExtInfo;
        }
        public override List<ActUIElement.eSubElementType> GetSubElementType(eElementType elementType)
        {
            List<ActUIElement.eSubElementType> list = [];
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
            List<ActUIElement.eElementAction> list = [];
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
            List<ActUIElement.eTableAction> list = [];
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
                    list.Add(ActUIElement.eTableAction.ContextMenuClick);
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
            ObservableList<ElementLocator> learningLocatorsList =
            [
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByName, Help = "Very Recommended (usually unique)" },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID, Help = "Very Recommended (usually unique), Supported for widgets elements only" },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath, Help = "Recommended (sensitive to page design changes),Supported for widgets elements only" },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByXPath, Help = "Recommended (sensitive to page design changes)" },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByClassName, Help = "Recommended (sensitive to page design changes),Supported for widgets elements only" },
            ];

            return learningLocatorsList;
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
        /// This method is used to check if the platform supports POM
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
            return [ePomElementCategory.Java];
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
