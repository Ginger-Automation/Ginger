#region License
/*
Copyright © 2014-2022 European Support Limited

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
using GingerCore.Actions.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Automation;

namespace GingerCore.Drivers.Common
{
    public class ActionResult
    {
        public string errorMessage { get; set;  }
        public string executionInfo { get; set;  }
        public string outputValue { get; set;  }
    }


    public class UIElementOperationsHelper
    {

        private WinAPIAutomation winAPI = new WinAPIAutomation();
        public bool taskFinished;
        public ActionResult ToggleElement(AutomationElement_Extend automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();           
            object togglePattern;

            try
            {
                automationElement.TryGetCurrentPattern(TogglePatternExtended.Pattern, out togglePattern);
                if (togglePattern != null)
                {
                    ToggleStateExtended originalState = ((TogglePatternExtended)togglePattern).Current.ToggleState;

                    ((TogglePatternExtended)togglePattern).Toggle();

                    ToggleStateExtended newState = ((TogglePatternExtended)togglePattern).Current.ToggleState;

                    if (originalState != newState)
                    {
                        actionResult.outputValue = newState.ToString();
                        actionResult.executionInfo = "Element state toggled to" + newState.ToString();
                    }
                    else
                    {
                        actionResult.errorMessage = "Failed to toggle the state for the element";
                    }
                }
                else
                {
                    actionResult.errorMessage = "Toggle is not supported for this element type "+ elementType;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Toggle state using toggle pattern", ex);
                actionResult.errorMessage = "Failed to toggle the value for the element";
            }


            return actionResult;
        }

        public ActionResult SetValue(AutomationElement_Extend automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(ValuePatternExtended.Pattern, out pattern);

                if (pattern != null)
                {
                    ((ValuePatternExtended)pattern).SetValue(value);
                    actionResult.executionInfo = "Element Value set to " + value;
                }
                else
                {
                    actionResult.errorMessage = "Failed to set the value " + value;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Setting the value using value pattern", ex);
                actionResult.errorMessage = "Failed to set the value " + value;
            }
            return actionResult;
        }

        public ActionResult SetText(AutomationElement_Extend automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(LegacyIAccessiblePatternExtended.Pattern, out pattern);

                if (pattern != null)
                {
                    ((LegacyIAccessiblePatternExtended)pattern).SetValue(value);
                    actionResult.executionInfo = "Element Value set to " + value;
                }
                else
                {
                    actionResult.errorMessage = "Failed to set the value " + value;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Setting the value using Legacy pattern", ex);
                actionResult.errorMessage = "Failed to set the value " + value;
            }
            return actionResult;
        }

        public ActionResult SendKeys(AutomationElement_Extend automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                winAPI.SendKeysByLibrary(automationElement, value);
                //TODO: if value is not set using above then try winAPI.SetElementText(automationElement, value);
                actionResult.executionInfo = "Successfully Sent keys";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Performing SendKeys", ex);
                actionResult.errorMessage = "Failed to perform send keys";
            }
            return actionResult;
        }


        public ActionResult SelectValue(AutomationElement_Extend automationElement, eElementType elementType, string value)
        {
            ActionResult actionResult = new ActionResult();

            AutomationElement_Extend itemToSelect = automationElement.FindFirst(TreeScopeExtended.Descendants, new
                       PropertyConditionExtended(AutomationElement_Extend.NameProperty, value));

            if (itemToSelect != null)
            {
                actionResult = SelementElement(itemToSelect);
            }
            else
            {
                actionResult.errorMessage = "Unable to find the value to select";
            }

            return actionResult;
        }

        internal ActionResult SelementElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPatternExtended.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPatternExtended)selectionItemPattern).Select();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult AddToSelection(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPatternExtended.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPatternExtended)selectionItemPattern).AddToSelection();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult RemoveFromSelection(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPatternExtended.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPatternExtended)selectionItemPattern).RemoveFromSelection();
                actionResult.executionInfo = "Succesfully removed the element from selection";
            }
            else
            {
                actionResult.errorMessage = "Failed to remove the element from selection";
            }
            return actionResult;
        }

        public ActionResult ExpandElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object expandPattern;
            automationElement.TryGetCurrentPattern(ExpandCollapsePatternExtended.Pattern, out expandPattern);

            if (expandPattern != null)
            {
                ((ExpandCollapsePatternExtended)expandPattern).Expand();
                actionResult.executionInfo = "Successfully expanded the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to expand the element";
            }
            return actionResult;
        }
        public ActionResult CollapseElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object collapsePattern;
            automationElement.TryGetCurrentPattern(ExpandCollapsePatternExtended.Pattern, out collapsePattern);

            if (collapsePattern != null)
            {
                ((ExpandCollapsePatternExtended)collapsePattern).Collapse();
                actionResult.executionInfo = "Successfully collapsed the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to collapse the element";
            }
            return actionResult;
        }
        internal ActionResult ScrollToView(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object scrollPattern;
            automationElement.TryGetCurrentPattern(ScrollItemPatternExtended.Pattern, out scrollPattern);
            if (scrollPattern != null)
            {               
                ((ScrollItemPatternExtended)scrollPattern).ScrollIntoView();
                actionResult.executionInfo = "Successfully scrolled the element into view";
            }
            else
            {
                actionResult.errorMessage = "Failed to scroll the element into view";
            }
            return actionResult;
        }

        internal ActionResult GetPropertyValue(AutomationElement_Extend automationElement, AutomationPropertyExtended automationProperty)
        {
  

            ActionResult actionResult = new ActionResult();
            object selectedValue=  automationElement.GetCurrentPropertyValue(automationProperty);

            if (selectedValue != null)
            {
                actionResult.outputValue = Convert.ToString(selectedValue);
                actionResult.executionInfo = "Succesfully retrieved the " + automationProperty.ToString() + " property value";
            }
            else
            {
                actionResult.errorMessage = "Failed to retrieve the " + automationProperty.ToString() + "  property";
            }
            return actionResult;
        }

        public ActionResult ClickElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            Boolean clickTriggeredFlag = false;
            try
            {
               actionResult= ClickUsingInvokePattern(automationElement, ref clickTriggeredFlag);
                if (!string.IsNullOrEmpty(actionResult.errorMessage))
                {
                    clickTriggeredFlag = false;
                    actionResult = ClickUsingLegacyPattern(automationElement, ref clickTriggeredFlag);
                }                       
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }

        public ActionResult MouseClickElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();         
            try
            {
               winAPI.SendClick(automationElement);
               actionResult.executionInfo = "Successfully clicked the element";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }

        public ActionResult ClickElementUsingXY(AutomationElement_Extend automationElement, int xCoordinate, int yCoordinate)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int x = (int)automationElement.Current.BoundingRectangle.X + xCoordinate;
                int y = (int)automationElement.Current.BoundingRectangle.Y + yCoordinate;

                winAPI.SendClickOnXYPoint(automationElement, x, y);
                actionResult.executionInfo = "Successfully clicked the element";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }

        public ActionResult DoubleClickElementUsingXY(AutomationElement_Extend automationElement, int xCoordinate, int yCoordinate)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string xy = string.Empty;
                if(xCoordinate !=0 && yCoordinate!=0)
                {
                    xy= xCoordinate + ","+ yCoordinate; 
                }

                winAPI.SendDoubleClick(automationElement, xy);
                actionResult.executionInfo = "Successfully double clicked the element";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in docule click Element", ex);
                actionResult.errorMessage = "Failed to Double click the element";
            }
            return actionResult;
        }

        public ActionResult RightClickElementUsingXY(AutomationElement_Extend automationElement, int xCoordinate, int yCoordinate)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string xy = string.Empty;
                if (xCoordinate != 0 && yCoordinate != 0)
                {
                    xy = xCoordinate + "," + yCoordinate;
                }

                winAPI.SendRightClick(automationElement, xy);
                actionResult.executionInfo = "Successfully double clicked the element";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in docule click Element", ex);
                actionResult.errorMessage = "Failed to Double click the element";
            }
            return actionResult;
        }


        public ActionResult AsyncClickElement(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            Boolean clickTriggeredFlag = false;
            try
            {
                Thread UIAClickThread = new Thread(new ThreadStart(() =>
                {
                    actionResult = ClickUsingInvokePattern(automationElement, ref clickTriggeredFlag);
                    if (!string.IsNullOrEmpty(actionResult.errorMessage))
                    {
                        clickTriggeredFlag = false;
                        actionResult = ClickUsingLegacyPattern(automationElement, ref clickTriggeredFlag);
                    }

                }));
                UIAClickThread.IsBackground = true;            
                UIAClickThread.Start();

                Stopwatch st = new Stopwatch();
                st.Start();

                while (clickTriggeredFlag==false && st.ElapsedMilliseconds < 30000)
                {
                    Thread.Sleep(10);
                }
                Thread.Sleep(100);

                //if (e != null) throw e;

                UIAClickThread.Abort();

               if(clickTriggeredFlag)
                {
                    actionResult.executionInfo = "Successfully triggered async click";
                }

                return actionResult;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }




        internal ActionResult ClickUsingInvokePattern(AutomationElement_Extend automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object invokePattern;
            try
            {

                automationElement.TryGetCurrentPattern(InvokePatternExtended.Pattern, out invokePattern);

                if (invokePattern != null)
                {
                    clickTriggeredFlag = true;                   
                    ((InvokePatternExtended)invokePattern).Invoke();
                    actionResult.executionInfo = "Successfully clicked the element";
                }
                else
                {
                    actionResult.errorMessage = "Failed to click the element";
                }
            }
            catch (Exception ex)
            {
                clickTriggeredFlag = false;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }

        internal ActionResult ClickUsingLegacyPattern(AutomationElement_Extend automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object legacyPattern;
            try
            {

                automationElement.TryGetCurrentPattern(LegacyIAccessiblePatternExtended.Pattern, out legacyPattern);

                if (legacyPattern != null)
                {
                    actionResult= GetPropertyValue(automationElement, LegacyIAccessiblePatternIdentifiersExtended.DefaultActionProperty);
                    if(string.IsNullOrEmpty(actionResult.errorMessage))
                    {
                        if(!string.IsNullOrEmpty(actionResult.outputValue))
                        {
                            clickTriggeredFlag = true;
                            ((LegacyIAccessiblePatternExtended)legacyPattern).DoDefaultAction();
                            actionResult.executionInfo = "Successfully clicked the element";
                        }
                        else
                        {
                            actionResult.errorMessage = "Failed to click the element";
                        }
                    }                  
                }
                else
                {
                    actionResult.errorMessage = "Failed to click the element";
                }
            }
            catch (Exception ex)
            {
                clickTriggeredFlag = false;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElement", ex);
                actionResult.errorMessage = "Failed to click the element";
            }
            return actionResult;
        }


        //public ActionResult GetControlProperty(AutomationElement_Extend automationElement, string propertyName)
        //{
        //    ActionResult actionResult = new ActionResult();
        //    try
        //    {


        //        actionResult.executionInfo = "Successfully double clicked the element";
        //    }
        //    catch (Exception ex)
        //    {
        //        Reporter.ToLog(eLogLevel.DEBUG, "Exception in docule click Element", ex);
        //        actionResult.errorMessage = "Failed to Double click the element";
        //    }
        //    return actionResult;
        //}


        public ActionResult GetValue(AutomationElement_Extend automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();
            object valuePattern;
            try
            {
                //if(elementType==eElementType.CheckBox)
                //{
                //    object togglePattern;
                //    automationElement.TryGetCurrentPattern(TogglePatternExtended.Pattern, out togglePattern);

                //    ToggleStateExtended toggleState = ((TogglePatternExtended)togglePattern).Current.ToggleStateExtended;
                //    actionResult.outputValue = Convert.ToString(toggleState);
                //}
                //else
                //{
                    actionResult = GetPropertyValue(automationElement, ValuePatternIdentifiersExtended.ValueProperty);
                    if (!string.IsNullOrEmpty(actionResult.errorMessage)|| string.IsNullOrEmpty(actionResult.outputValue))
                    {
                        actionResult = GetPropertyValue(automationElement, LegacyIAccessiblePatternIdentifiersExtended.ValueProperty);
                    }
                //}      
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Get Value", ex);
                actionResult.errorMessage = "Failed to Get the value";
            }
            return actionResult;

        }

        public ActionResult GetText(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object textPattern;
            try
            {
                automationElement.TryGetCurrentPattern(TextPatternExtended.Pattern, out textPattern);
                if (textPattern != null)
                {
                    actionResult.outputValue = ((TextPatternExtended)textPattern).DocumentRange.GetText(-1);
                    return actionResult;
                }

                automationElement.TryGetCurrentPattern(ValuePatternExtended.Pattern, out textPattern);
                if (textPattern != null)
                {
                    var valuePattern = (ValuePatternExtended)textPattern;
                    actionResult.outputValue = valuePattern.Current.Value;
                    return actionResult;
                }

                if (string.IsNullOrEmpty(actionResult.outputValue))
                {
                    actionResult.outputValue = automationElement.Current.Name;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Get text", ex);
                actionResult.errorMessage = "Failed to Get the text";
            }
            return actionResult;
        }
        public ActionResult GetSelectedValue(AutomationElement_Extend automationElement)
        {
            ActionResult actionResult = new ActionResult();
            string ListItems = null;
            taskFinished = false;
            try
            {
                bool isMultiSelect = (bool)automationElement.GetCurrentPropertyValue(SelectionPatternIdentifiersExtended.CanSelectMultipleProperty);
                if (isMultiSelect)
                {
                    ListItems = GetSelectedItems(automationElement);
                }
                else
                {
                    object vp;
                    automationElement.TryGetCurrentPattern(ValuePatternExtended.Pattern, out vp);
                    if (vp != null)
                    {
                        actionResult = GetValue(automationElement, eElementType.Unknown);
                    }
                    else
                    {
                        AutomationElement_Extend elementNode = TreeWalkerExtended.RawViewWalker.GetFirstChild(automationElement);
                        string isItemSelected;
                        while (elementNode != null && !taskFinished)
                        {
                            isItemSelected = (elementNode.GetCurrentPropertyValue(SelectionItemPatternIdentifiersExtended.IsSelectedProperty)).ToString();
                            if (isItemSelected == "True")
                            {
                                if (ListItems == null && isItemSelected == "True")
                                {
                                    ListItems = elementNode.Current.Name;
                                }
                                else if (isItemSelected == "True")
                                {
                                    ListItems += "," + elementNode.Current.Name;
                                }
                            }
                            elementNode = TreeWalkerExtended.RawViewWalker.GetNextSibling(elementNode);
                        }
                        actionResult.executionInfo = "No Item selected";
                    }
                }
                actionResult.outputValue = ListItems;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Get selected value", ex);
                actionResult.errorMessage = "Failed to Get selected value";
            }
            return actionResult;
        }

        private string GetSelectedItems(AutomationElement_Extend element)
        {
            AutomationElement_Extend elementNode = TreeWalkerExtended.RawViewWalker.GetFirstChild(element);
            String ListItems = null;
            do
            {
                string isItemSelected = (elementNode.GetCurrentPropertyValue(SelectionItemPatternIdentifiersExtended.IsSelectedProperty)).ToString();
                if (ListItems == null && isItemSelected == "True")
                {
                    ListItems = elementNode.Current.Name;
                }
                else if (isItemSelected == "True")
                {
                    ListItems += "," + elementNode.Current.Name;
                }

                elementNode = TreeWalkerExtended.RawViewWalker.GetNextSibling(elementNode);
            } while (elementNode != null && !taskFinished);
            return ListItems;
        }

        public ActionResult CloseWindow(AutomationElement_Extend window)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(WindowPatternExtended.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((WindowPatternExtended)windowPattern).Close();
                    actionResult.executionInfo = "Window closed";
                }
                else
                {
                    actionResult.errorMessage = "Unable to close the window";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "CLosing the windpow", ex);
                actionResult.errorMessage = "Unable to close the window";
            }
            return actionResult;
            
        }

        public ActionResult GetTitle(AutomationElement_Extend window)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                actionResult.outputValue = window.Current.Name;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Get Title", ex);
                actionResult.errorMessage = "Failed to Get the Title";
            }
            return actionResult;

        }

        public ActionResult SetWindowState(AutomationElement_Extend window, WindowVisualStateExtended windowVisualState)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(WindowPatternExtended.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((WindowPatternExtended)windowPattern).SetWindowVisualState(windowVisualState);
                    actionResult.executionInfo = "Window is " + windowVisualState;
                }
                else
                {
                    actionResult.errorMessage = "Unable to "+ windowVisualState + " the window";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "SetWindowState", ex);
                actionResult.errorMessage = "Unable to " + windowVisualState + " the window";
            }
            return actionResult;

        }
        
        public bool PerformClick(AutomationElement_Extend automationElement, ActUIElement.eElementAction clickType)
        {
            ActionResult actionResult = new ActionResult();
            bool result = false;
            switch (clickType)
            {
                case ActUIElement.eElementAction.Click:
                    actionResult = ClickElement(automationElement);
                    break;

                case ActUIElement.eElementAction.AsyncClick:
                    actionResult = AsyncClickElement(automationElement);
                    break;

                case ActUIElement.eElementAction.MouseClick:
                    actionResult = MouseClickElement(automationElement);
                    break;
            }
            if (string.IsNullOrEmpty(actionResult.errorMessage))
            {
                result = true;
            }
            return result;
        }

        public bool LocateAndValidateElement(AutomationElement_Extend elementToValidate, string elementType, ActUIElement.eElementAction actionType, string validationValue = "")
        {
            ActionResult actionResult = new ActionResult();
            bool result = false;

            switch (actionType)
            {
                case ActUIElement.eElementAction.IsEnabled:
                    result = elementToValidate.Current.IsEnabled;
                    break;

                case ActUIElement.eElementAction.Exist:
                    if (elementToValidate != null)
                    {
                        result = true;
                    }
                    break;

                case ActUIElement.eElementAction.NotExist:
                    if (elementToValidate == null)
                    {
                        result = true;
                    }
                    break;
                case ActUIElement.eElementAction.GetValue:
                    if (elementToValidate == null)
                    {
                        result = false;
                    }
                    actionResult = GetValue(elementToValidate, eElementType.Unknown);
                    if (!string.IsNullOrEmpty(actionResult.errorMessage) || string.IsNullOrEmpty(actionResult.outputValue))
                    {
                        actionResult = GetText(elementToValidate);
                    }
                    if (actionResult.outputValue == validationValue)
                    {
                        result = true;
                    }
                    break;
            }

            return result;
        }

        public ActionResult ClickElementByOthertypes(ActUIElement.eElementAction executedClick, List<ActUIElement.eElementAction> clicks, AutomationElement_Extend automationElement, AutomationElement_Extend elementToValidate, string validationElementType, ActUIElement.eElementAction validationType)
        {
            ActUIElement.eElementAction currentClick;
            //string result = "";
            ActionResult actionResult = new ActionResult();

            bool isClicked = false;
            bool isValidated = false;

            for (int i = 0; i < clicks.Count; i++)
            {
                currentClick = clicks[i];
                if (currentClick != executedClick)
                {
                    isClicked = PerformClick(automationElement, currentClick);
                    if (isClicked)
                    {
                        isValidated = LocateAndValidateElement(elementToValidate, validationElementType, validationType);
                        if (isValidated)
                        {
                            break;
                        }
                    }
                }
            }
            if (isValidated)
            {
               actionResult.executionInfo = "Successfully clicked and validated";
            }
            return actionResult;
        }
    }
}
