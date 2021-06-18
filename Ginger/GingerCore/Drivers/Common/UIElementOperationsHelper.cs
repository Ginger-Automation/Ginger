#region License
/*
Copyright © 2014-2021 European Support Limited

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
using GingerCore.Actions.UIAutomation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        public ActionResult ToggleElement(AutomationElement automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();           
            object togglePattern;

            try
            {
                automationElement.TryGetCurrentPattern(TogglePattern.Pattern, out togglePattern);
                if (togglePattern != null)
                {
                    ToggleState originalState = ((TogglePattern)togglePattern).Current.ToggleState;

                    ((TogglePattern)togglePattern).Toggle();

                    ToggleState newState = ((TogglePattern)togglePattern).Current.ToggleState;

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

        public ActionResult SetValue(AutomationElement automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(ValuePattern.Pattern, out pattern);

                if (pattern != null)
                {
                    ((ValuePattern)pattern).SetValue(value);
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

        public ActionResult SetText(AutomationElement automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out pattern);

                if (pattern != null)
                {
                    ((LegacyIAccessiblePattern)pattern).SetValue(value);
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

        public ActionResult SendKeys(AutomationElement automationElement, string value)
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


        public ActionResult SelectValue(AutomationElement automationElement, eElementType elementType, string value)
        {
            ActionResult actionResult = new ActionResult();

            AutomationElement itemToSelect = automationElement.FindFirst(TreeScope.Descendants, new
                       PropertyCondition(AutomationElement.NameProperty, value));

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

        internal ActionResult SelementElement(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPattern)selectionItemPattern).Select();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult AddToSelection(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPattern)selectionItemPattern).AddToSelection();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult RemoveFromSelection(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((SelectionItemPattern)selectionItemPattern).RemoveFromSelection();
                actionResult.executionInfo = "Succesfully removed the element from selection";
            }
            else
            {
                actionResult.errorMessage = "Failed to remove the element from selection";
            }
            return actionResult;
        }

        public ActionResult ExpandElement(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object expandPattern;
            automationElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out expandPattern);

            if (expandPattern != null)
            {
                ((ExpandCollapsePattern)expandPattern).Expand();
                actionResult.executionInfo = "Successfully expanded the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to expand the element";
            }
            return actionResult;
        }
        public ActionResult CollapseElement(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object collapsePattern;
            automationElement.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out collapsePattern);

            if (collapsePattern != null)
            {
                ((ExpandCollapsePattern)collapsePattern).Collapse();
                actionResult.executionInfo = "Successfully collapsed the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to collapse the element";
            }
            return actionResult;
        }
        internal ActionResult ScrollToView(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object scrollPattern;
            automationElement.TryGetCurrentPattern(ScrollItemPattern.Pattern, out scrollPattern);
            if (scrollPattern != null)
            {               
                ((ScrollItemPattern)scrollPattern).ScrollIntoView();
                actionResult.executionInfo = "Successfully scrolled the element into view";
            }
            else
            {
                actionResult.errorMessage = "Failed to scroll the element into view";
            }
            return actionResult;
        }

        internal ActionResult GetPropertyValue(AutomationElement automationElement, AutomationProperty automationProperty)
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

        public ActionResult ClickElement(AutomationElement automationElement)
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

        public ActionResult MouseClickElement(AutomationElement automationElement)
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

        public ActionResult ClickElementUsingXY(AutomationElement automationElement, int xCoordinate, int yCoordinate)
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

        public ActionResult DoubleClickElementUsingXY(AutomationElement automationElement, int xCoordinate, int yCoordinate)
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

        public ActionResult RightClickElementUsingXY(AutomationElement automationElement, int xCoordinate, int yCoordinate)
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


        public ActionResult AsyncClickElement(AutomationElement automationElement)
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




        internal ActionResult ClickUsingInvokePattern(AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object invokePattern;
            try
            {

                automationElement.TryGetCurrentPattern(InvokePattern.Pattern, out invokePattern);

                if (invokePattern != null)
                {
                    clickTriggeredFlag = true;                   
                    ((InvokePattern)invokePattern).Invoke();
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

        internal ActionResult ClickUsingLegacyPattern(AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object legacyPattern;
            try
            {

                automationElement.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out legacyPattern);

                if (legacyPattern != null)
                {
                    actionResult= GetPropertyValue(automationElement, LegacyIAccessiblePatternIdentifiers.DefaultActionProperty);
                    if(string.IsNullOrEmpty(actionResult.errorMessage))
                    {
                        if(!string.IsNullOrEmpty(actionResult.outputValue))
                        {
                            clickTriggeredFlag = true;
                            ((LegacyIAccessiblePattern)legacyPattern).DoDefaultAction();
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


        //public ActionResult GetControlProperty(AutomationElement automationElement, string propertyName)
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


        public ActionResult GetValue(AutomationElement automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();
            object valuePattern;
            try
            {
                //if(elementType==eElementType.CheckBox)
                //{
                //    object togglePattern;
                //    automationElement.TryGetCurrentPattern(TogglePattern.Pattern, out togglePattern);

                //    ToggleState toggleState = ((TogglePattern)togglePattern).Current.ToggleState;
                //    actionResult.outputValue = Convert.ToString(toggleState);
                //}
                //else
                //{
                    actionResult = GetPropertyValue(automationElement, ValuePatternIdentifiers.ValueProperty);
                    if (!string.IsNullOrEmpty(actionResult.errorMessage)|| string.IsNullOrEmpty(actionResult.outputValue))
                    {
                        actionResult = GetPropertyValue(automationElement, LegacyIAccessiblePatternIdentifiers.ValueProperty);
                    }
                //}      

                //if (string.IsNullOrEmpty(actionResult.outputValue))
                //{
                //    actionResult.outputValue = automationElement.Current.Name;
                //}
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Get Value", ex);
                actionResult.errorMessage = "Failed to Get the value";
            }
            return actionResult;

        }

        public ActionResult GetText(AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object textPattern;
            try
            {
                automationElement.TryGetCurrentPattern(TextPattern.Pattern, out textPattern);
                if (textPattern != null)
                {
                    actionResult.outputValue = ((TextPattern)textPattern).DocumentRange.GetText(-1);
                    return actionResult;
                }

                automationElement.TryGetCurrentPattern(ValuePattern.Pattern, out textPattern);
                if (textPattern != null)
                {
                    var valuePattern = (ValuePattern)textPattern;
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

        public ActionResult CloseWindow(AutomationElement window)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(WindowPattern.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((WindowPattern)windowPattern).Close();
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

        public ActionResult GetTitle(AutomationElement window)
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

        public ActionResult SetWindowState(AutomationElement window, WindowVisualState windowVisualState)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(WindowPattern.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((WindowPattern)windowPattern).SetWindowVisualState(windowVisualState);
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
        
        public ActionResult ClickAndValidte(AutomationElement automationElement, ActUIElement act)
        {
            ActionResult actionResult = new ActionResult();


            ActUIElement.eElementAction clickType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ClickType).ToString(), out clickType) == false)
            {
                actionResult.errorMessage = "Unknown Click Type";
                return actionResult;
            }

            ActUIElement.eElementAction validationType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString(), out validationType) == false)
            {
                actionResult.errorMessage = "Unknown Validation Type";
                return actionResult;
            }
            string validationElementType = act.GetInputParamValue(ActUIElement.Fields.ValidationElement);

            eLocateBy validationElementLocateby;
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString(), out validationElementLocateby) == false)
            {
                actionResult.errorMessage = "Unknown Validation Element Locate By";
                return actionResult;
            }

            string validattionElementLocateValue = act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocatorValue);
            bool LoopNextCheck = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.LoopThroughClicks).ToString()) == "True")
            {
                LoopNextCheck = true;
            }

            //perform click
            bool isClicked = performClick(automationElement, clickType);
            if (isClicked)
            {
                //validate
            }
            else 
            {
                if (LoopNextCheck)
                {
                    //click element by other types
                }
            }




            return actionResult;
        }

        public bool performClick(AutomationElement automationElement, ActUIElement.eElementAction clickType)
        {
            ActionResult actionResult = new ActionResult();
            Boolean clickTriggeredFlag = false;
            bool result = false;
            switch (clickType)
            {
                case ActUIElement.eElementAction.InvokeClick:
                    actionResult = ClickUsingInvokePattern(automationElement, ref clickTriggeredFlag);
                    break;

                case ActUIElement.eElementAction.LegacyClick:
                    actionResult = ClickUsingLegacyPattern(automationElement, ref clickTriggeredFlag);
                    break;

                case ActUIElement.eElementAction.MouseClick:
                    actionResult = MouseClickElement(automationElement);
                    break;
            }
            if (!string.IsNullOrEmpty(actionResult.errorMessage))
            {
                result = true;
            }
            return result;
        }

        public bool LocateAndValidateElement(eLocateBy LocateBy, string LocateValue, string elementType, ActUIElement.eElementAction actionType, string validationValue = "")
        {
            bool result = false;
            //object obj = FindElementByLocator(LocateBy, LocateValue);

            
            return result;
        }

    }
}
