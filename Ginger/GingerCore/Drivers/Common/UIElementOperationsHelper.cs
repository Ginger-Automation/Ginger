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
extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.GingerOCR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;

using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common
{
    public class ActionResult
    {
        public string errorMessage { get; set; }
        public string executionInfo { get; set; }
        public string outputValue { get; set; }
    }


    public class UIElementOperationsHelper
    {

        private WinAPIAutomation winAPI = new WinAPIAutomation();
        public bool taskFinished;
        public ActionResult ToggleElement(UIAuto.AutomationElement automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();
            object togglePattern;

            try
            {
                automationElement.TryGetCurrentPattern(UIAuto.TogglePattern.Pattern, out togglePattern);
                if (togglePattern != null)
                {
                    Interop.UIAutomationClient.ToggleState originalState = ((UIAuto.TogglePattern)togglePattern).Current.ToggleState;

                    ((UIAuto.TogglePattern)togglePattern).Toggle();

                    Interop.UIAutomationClient.ToggleState newState = ((UIAuto.TogglePattern)togglePattern).Current.ToggleState;

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
                    actionResult.errorMessage = "Toggle is not supported for this element type " + elementType;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Toggle state using toggle pattern", ex);
                actionResult.errorMessage = "Failed to toggle the value for the element";
            }


            return actionResult;
        }

        public ActionResult SetValue(UIAuto.AutomationElement automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(UIAuto.ValuePattern.Pattern, out pattern);

                if (pattern != null)
                {
                    ((UIAuto.ValuePattern)pattern).SetValue(value);
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

        public ActionResult SetText(UIAuto.AutomationElement automationElement, string value)
        {
            ActionResult actionResult = new ActionResult();
            object pattern;

            try
            {
                automationElement.TryGetCurrentPattern(UIAuto.LegacyIAccessiblePattern.Pattern, out pattern);

                if (pattern != null)
                {
                    ((UIAuto.LegacyIAccessiblePattern)pattern).SetValue(value);
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

        public ActionResult SendKeys(UIAuto.AutomationElement automationElement, string value)
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


        public ActionResult SelectValue(UIAuto.AutomationElement automationElement, eElementType elementType, string value)
        {
            ActionResult actionResult = new ActionResult();

            UIAuto.AutomationElement itemToSelect = automationElement.FindFirst(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants, new
                       UIAuto.PropertyCondition(UIAuto.AutomationElement.NameProperty, value));

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

        internal ActionResult SelementElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(UIAuto.SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((UIAuto.SelectionItemPattern)selectionItemPattern).Select();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult AddToSelection(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(UIAuto.SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((UIAuto.SelectionItemPattern)selectionItemPattern).AddToSelection();
                actionResult.executionInfo = "Succesfully selected the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to select the element";
            }
            return actionResult;
        }

        internal ActionResult RemoveFromSelection(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object selectionItemPattern;
            automationElement.TryGetCurrentPattern(UIAuto.SelectionItemPattern.Pattern, out selectionItemPattern);
            if (selectionItemPattern != null)
            {
                ((UIAuto.SelectionItemPattern)selectionItemPattern).RemoveFromSelection();
                actionResult.executionInfo = "Succesfully removed the element from selection";
            }
            else
            {
                actionResult.errorMessage = "Failed to remove the element from selection";
            }
            return actionResult;
        }

        public ActionResult ExpandElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object expandPattern;
            automationElement.TryGetCurrentPattern(UIAuto.ExpandCollapsePattern.Pattern, out expandPattern);

            if (expandPattern != null)
            {
                ((UIAuto.ExpandCollapsePattern)expandPattern).Expand();
                actionResult.executionInfo = "Successfully expanded the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to expand the element";
            }
            return actionResult;
        }
        public ActionResult CollapseElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object collapsePattern;
            automationElement.TryGetCurrentPattern(UIAuto.ExpandCollapsePattern.Pattern, out collapsePattern);

            if (collapsePattern != null)
            {
                ((UIAuto.ExpandCollapsePattern)collapsePattern).Collapse();
                actionResult.executionInfo = "Successfully collapsed the element";
            }
            else
            {
                actionResult.errorMessage = "Failed to collapse the element";
            }
            return actionResult;
        }
        internal ActionResult ScrollToView(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object scrollPattern;
            automationElement.TryGetCurrentPattern(UIAuto.ScrollItemPattern.Pattern, out scrollPattern);
            if (scrollPattern != null)
            {
                ((UIAuto.ScrollItemPattern)scrollPattern).ScrollIntoView();
                actionResult.executionInfo = "Successfully scrolled the element into view";
            }
            else
            {
                actionResult.errorMessage = "Failed to scroll the element into view";
            }
            return actionResult;
        }

        internal ActionResult GetPropertyValue(UIAuto.AutomationElement automationElement, UIAuto.AutomationProperty automationProperty)
        {


            ActionResult actionResult = new ActionResult();
            object selectedValue = automationElement.GetCurrentPropertyValue(automationProperty);

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

        public ActionResult ClickElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            Boolean clickTriggeredFlag = false;
            try
            {
                actionResult = ClickUsingInvokePattern(automationElement, ref clickTriggeredFlag);
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

        public ActionResult DoubleClickElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            bool doubleClickTriggeredFlag = false;

            try
            {
                // 1) Try InvokePattern twice
                actionResult = DobuleClickUsingInvokePattern(automationElement, ref doubleClickTriggeredFlag);
                if (!string.IsNullOrEmpty(actionResult.errorMessage))
                {
                    // 2) Try Legacy default action twice
                    doubleClickTriggeredFlag = false;
                    actionResult = DobuleClickUsingLegacyPattern(automationElement, ref doubleClickTriggeredFlag);
                }
                if(!string.IsNullOrEmpty(actionResult.errorMessage))
                {
                    // 3) Fallback to mouse double-click
                    winAPI.DoubleSendClick(automationElement);
                    actionResult.executionInfo = "Successfully double-clicked the element";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in DoubleClickElement", ex);
                actionResult.errorMessage = "Failed to double-click the element";
            }

            return actionResult;
        }



        public ActionResult MouseClickElement(UIAuto.AutomationElement automationElement)
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

        public ActionResult MouseDoubleClickElement(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                winAPI.DoubleSendClick(automationElement);
                actionResult.executionInfo = "Successfully double-clicked the element";
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in MouseDoubleClickElement", ex);
                actionResult.errorMessage = "Failed to double-click the element";
            }
            return actionResult;
        }

        public ActionResult ClickElementUsingXY(UIAuto.AutomationElement automationElement, int xCoordinate, int yCoordinate)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                int x = automationElement.Current.BoundingRectangle.X + xCoordinate;
                int y = automationElement.Current.BoundingRectangle.Y + yCoordinate;

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

        public ActionResult DoubleClickElementUsingXY(UIAuto.AutomationElement automationElement, int xCoordinate, int yCoordinate)
        {
            ActionResult actionResult = new ActionResult();
            try
            {
                string xy = string.Empty;
                if (xCoordinate != 0 && yCoordinate != 0)
                {
                    xy = xCoordinate + "," + yCoordinate;
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

        public ActionResult RightClickElementUsingXY(UIAuto.AutomationElement automationElement, int xCoordinate, int yCoordinate)
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


        public ActionResult AsyncClickElement(UIAuto.AutomationElement automationElement)
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

                }))
                {
                    IsBackground = true
                };
                UIAClickThread.Start();

                Stopwatch st = new Stopwatch();
                st.Start();

                while (clickTriggeredFlag == false && st.ElapsedMilliseconds < 30000)
                {
                    Thread.Sleep(10);
                }
                Thread.Sleep(100);

                //if (e != null) throw e;

                UIAClickThread.Abort();

                if (clickTriggeredFlag)
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




        internal ActionResult ClickUsingInvokePattern(UIAuto.AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object invokePattern;
            try
            {

                automationElement.TryGetCurrentPattern(UIAuto.InvokePattern.Pattern, out invokePattern);

                if (invokePattern != null)
                {
                    clickTriggeredFlag = true;
                    ((UIAuto.InvokePattern)invokePattern).Invoke();
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

        internal ActionResult ClickUsingLegacyPattern(UIAuto.AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object legacyPattern;
            try
            {

                automationElement.TryGetCurrentPattern(UIAuto.LegacyIAccessiblePattern.Pattern, out legacyPattern);

                if (legacyPattern != null)
                {
                    actionResult = GetPropertyValue(automationElement, UIAuto.LegacyIAccessiblePatternIdentifiers.DefaultActionProperty);
                    if (string.IsNullOrEmpty(actionResult.errorMessage))
                    {
                        if (!string.IsNullOrEmpty(actionResult.outputValue))
                        {
                            clickTriggeredFlag = true;
                            ((UIAuto.LegacyIAccessiblePattern)legacyPattern).DoDefaultAction();
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

        internal ActionResult DobuleClickUsingInvokePattern(UIAuto.AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object invokePattern;
            try
            {
                if (automationElement.TryGetCurrentPattern(UIAuto.InvokePattern.Pattern, out invokePattern) && invokePattern != null)
                {
                    clickTriggeredFlag = true;
                    ((UIAuto.InvokePattern)invokePattern).Invoke();
                    Thread.Sleep(100);
                    ((UIAuto.InvokePattern)invokePattern).Invoke();
                    actionResult.executionInfo = "Successfully double-clicked the element";
                }
                else
                {
                    actionResult.errorMessage = "Failed to double-clicked the element";
                }
            }
            catch (Exception ex)
            {
                clickTriggeredFlag = false;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in DoubleClickElement", ex);
                actionResult.errorMessage = "Failed to double-clicked the element";
            }
            return actionResult;
        }

        internal ActionResult DobuleClickUsingLegacyPattern(UIAuto.AutomationElement automationElement, ref Boolean clickTriggeredFlag)
        {
            ActionResult actionResult = new ActionResult();
            object legacyPattern;
            try
            {
                if (automationElement.TryGetCurrentPattern(UIAuto.LegacyIAccessiblePattern.Pattern, out legacyPattern) && legacyPattern != null)
                {
                    actionResult = GetPropertyValue(automationElement, UIAuto.LegacyIAccessiblePatternIdentifiers.DefaultActionProperty);
                    if (string.IsNullOrEmpty(actionResult.errorMessage))
                    {
                        if (!string.IsNullOrEmpty(actionResult.outputValue))
                        {
                            clickTriggeredFlag = true;
                            ((UIAuto.LegacyIAccessiblePattern)legacyPattern).DoDefaultAction();
                            Thread.Sleep(100);
                            ((UIAuto.LegacyIAccessiblePattern)legacyPattern).DoDefaultAction();
                            actionResult.executionInfo = "Successfully double-clicked  the element";
                        }
                        else
                        {
                            actionResult.errorMessage = "Failed to double-clicked the element";
                        }
                    }
                }
                else
                {
                    actionResult.errorMessage = "Failed to double-clicked the element";
                }
            }
            catch (Exception ex)
            {
                clickTriggeredFlag = false;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in DobuleClickElement", ex);
                actionResult.errorMessage = "Failed to double-clicked the element";
            }
            return actionResult;
        }


        //public ActionResult GetControlProperty(UIAuto.AutomationElement automationElement, string propertyName)
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


        public ActionResult GetValue(UIAuto.AutomationElement automationElement, eElementType elementType)
        {
            ActionResult actionResult = new ActionResult();
            object valuePattern;
            try
            {
                //if(elementType==eElementType.CheckBox)
                //{
                //    object togglePattern;
                //    automationElement.TryGetCurrentPattern(UIAuto.TogglePattern.Pattern, out togglePattern);

                //    ToggleState toggleState = ((UIAuto.TogglePattern)togglePattern).Current.ToggleState;
                //    actionResult.outputValue = Convert.ToString(toggleState);
                //}
                //else
                //{
                actionResult = GetPropertyValue(automationElement, UIAuto.ValuePatternIdentifiers.ValueProperty);
                if (!string.IsNullOrEmpty(actionResult.errorMessage) || string.IsNullOrEmpty(actionResult.outputValue))
                {
                    actionResult = GetPropertyValue(automationElement, UIAuto.LegacyIAccessiblePatternIdentifiers.ValueProperty);
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

        public static ActionResult GetValueByOCR(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            byte[] imageBytes = null;

            try
            {
                BringElementWindowToForeground(automationElement);

                // 1. Get bounding rectangle
                var rect = automationElement.Current.BoundingRectangle;
                int left = rect.X;
                int top = rect.Y;
                int width = rect.Width;
                int height = rect.Height;

                using (var bmp = new Bitmap(width, height))
                {
                    using (var g = Graphics.FromImage(bmp))
                    {
                        g.CopyFromScreen(left, top, 0, 0, new Size(width, height));
                    }

                    // 2. Convert bitmap to PNG bytes
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        imageBytes = ms.ToArray();
                    }
                }

                // 3. OCR
                string ocrText = GingerOcrOperations.ReadTextFromByteArray(imageBytes);
                actionResult.outputValue = string.IsNullOrWhiteSpace(ocrText) ? ocrText : ocrText.Trim();
                actionResult.executionInfo = "OCR value extracted successfully";
            }
            catch (Exception ex)
            {
                actionResult.errorMessage = "Failed to get value by OCR: " + ex.Message;
            }
            finally
            {
                if (imageBytes != null)
                {
                    // Overwrite with zeros for cleanup
                    Array.Clear(imageBytes, 0, imageBytes.Length);
                }
            }
            return actionResult;
        }

        public ActionResult GetText(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            object textPattern;
            try
            {
                automationElement.TryGetCurrentPattern(UIAuto.TextPattern.Pattern, out textPattern);
                if (textPattern != null)
                {
                    actionResult.outputValue = ((UIAuto.TextPattern)textPattern).DocumentRange.GetText(-1);
                    return actionResult;
                }

                automationElement.TryGetCurrentPattern(UIAuto.ValuePattern.Pattern, out textPattern);
                if (textPattern != null)
                {
                    var valuePattern = (UIAuto.ValuePattern)textPattern;
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
        public ActionResult GetSelectedValue(UIAuto.AutomationElement automationElement)
        {
            ActionResult actionResult = new ActionResult();
            string ListItems = null;
            taskFinished = false;
            try
            {
                bool isMultiSelect = (bool)automationElement.GetCurrentPropertyValue(UIAuto.SelectionPatternIdentifiers.CanSelectMultipleProperty);
                if (isMultiSelect)
                {
                    ListItems = GetSelectedItems(automationElement);
                }
                else
                {
                    object vp;
                    automationElement.TryGetCurrentPattern(UIAuto.ValuePattern.Pattern, out vp);
                    if (vp != null)
                    {
                        actionResult = GetValue(automationElement, eElementType.Unknown);
                    }
                    else
                    {
                        UIAuto.AutomationElement elementNode = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(automationElement);
                        string isItemSelected;
                        while (elementNode != null && !taskFinished)
                        {
                            isItemSelected = (elementNode.GetCurrentPropertyValue(UIAuto.SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
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
                            elementNode = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(elementNode);
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

        private string GetSelectedItems(UIAuto.AutomationElement element)
        {
            UIAuto.AutomationElement elementNode = UIAuto.TreeWalker.RawViewWalker.GetFirstChild(element);
            String ListItems = null;
            do
            {
                string isItemSelected = (elementNode.GetCurrentPropertyValue(UIAuto.SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                if (ListItems == null && isItemSelected == "True")
                {
                    ListItems = elementNode.Current.Name;
                }
                else if (isItemSelected == "True")
                {
                    ListItems += "," + elementNode.Current.Name;
                }

                elementNode = UIAuto.TreeWalker.RawViewWalker.GetNextSibling(elementNode);
            } while (elementNode != null && !taskFinished);
            return ListItems;
        }

        public ActionResult CloseWindow(UIAuto.AutomationElement window)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(UIAuto.WindowPattern.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((UIAuto.WindowPattern)windowPattern).Close();
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

        public ActionResult GetTitle(UIAuto.AutomationElement window)
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

        public ActionResult SetWindowState(UIAuto.AutomationElement window, Interop.UIAutomationClient.WindowVisualState windowVisualState)
        {
            ActionResult actionResult = new ActionResult();
            Object windowPattern;

            try
            {
                window.TryGetCurrentPattern(UIAuto.WindowPattern.Pattern, out windowPattern);
                if (windowPattern != null)
                {
                    ((UIAuto.WindowPattern)windowPattern).SetWindowVisualState(windowVisualState);
                    actionResult.executionInfo = "Window is " + windowVisualState;
                }
                else
                {
                    actionResult.errorMessage = "Unable to " + windowVisualState + " the window";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "SetWindowState", ex);
                actionResult.errorMessage = "Unable to " + windowVisualState + " the window";
            }
            return actionResult;

        }

        public bool PerformClick(UIAuto.AutomationElement automationElement, ActUIElement.eElementAction clickType)
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

        public bool LocateAndValidateElement(UIAuto.AutomationElement elementToValidate, string elementType, ActUIElement.eElementAction actionType, string validationValue = "")
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
                case ActUIElement.eElementAction.GetValueByOCR:
                    if (elementToValidate == null)
                    {
                        result = false;
                        break;
                    }
                    actionResult = GetValueByOCR(elementToValidate);
                    var actual = actionResult.outputValue?.Trim();
                    var expected = validationValue?.Trim();
                    if (!string.IsNullOrEmpty(actionResult.errorMessage) || string.IsNullOrEmpty(actual))
                    {
                        actionResult = GetText(elementToValidate);
                        actual = actionResult.outputValue?.Trim();
                    }
                    if (string.Equals(actual, expected, StringComparison.Ordinal))
                    {
                        result = true;
                    }
                    break;
            }

            return result;
        }

        public ActionResult ClickElementByOthertypes(ActUIElement.eElementAction executedClick, List<ActUIElement.eElementAction> clicks, UIAuto.AutomationElement automationElement, UIAuto.AutomationElement elementToValidate, string validationElementType, ActUIElement.eElementAction validationType)
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

        /// <summary>
        /// Brings the window containing the given AutomationElement to the foreground and optionally resizes it.
        /// </summary>
        /// <param name="automationElement">The UI Automation element whose window should be focused.</param>
        /// <param name="resizeWidth">Optional width to resize the window to.</param>
        /// <param name="resizeHeight">Optional height to resize the window to.</param>
        public static void BringElementWindowToForeground(UIAuto.AutomationElement automationElement, int? resizeWidth = null, int? resizeHeight = null)
        {
            if (automationElement == null)
            {
                return;
            }

            try
            {
                // Bring window to foreground
                WinAPIAutomation.ShowWindow(automationElement);

                // Optionally resize
                if (resizeWidth.HasValue && resizeHeight.HasValue)
                {
                    WinAPIAutomation.ResizeExternalWindow(automationElement, resizeWidth.Value, resizeHeight.Value);
                }

                // Give time to the window to come to the front
                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to bring window to foreground", ex);
            }
        }
    }
}
