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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.UIAutomation;
using GingerCore.Actions.VisualTesting;
using GingerCore.Actions.Windows;
using GingerCore.Drivers.Common;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Automation;

namespace GingerCore.Drivers.WindowsLib
{
    public class WindowsDriver : UIAutomationDriverBase, IWindowExplorer, IVisualTestingDriver
    {
        int mActionTimeout = 10;

        [UserConfigured]
        [UserConfiguredDefault("30")] 
        [UserConfiguredDescription("Action Timeout - default is 30 seconds")]
        public override int ActionTimeout
        {
            get
            {
                return mActionTimeout;
            }
            set
            { mActionTimeout = value; }
        }

        public WindowsDriver(BusinessFlow BF, eUIALibraryType type= eUIALibraryType.ComWrapper)
        {
            LibraryType = type;
            BusinessFlow = BF;
        }

        public override void StartDriver()
        {
            switch (LibraryType)
            {
                    
                case eUIALibraryType.ComWrapper:
                    mUIAutomationHelper= new UIAComWrapperHelper();
                    ((UIAComWrapperHelper)mUIAutomationHelper).WindowExplorer = this;
                    ((UIAComWrapperHelper)mUIAutomationHelper).BusinessFlow = BusinessFlow;
                    ((UIAComWrapperHelper)mUIAutomationHelper).mPlatform = UIAComWrapperHelper.ePlatform.Windows;
                    break;

            }
        }

        public override void UpdateContext(Context context)
        {
            base.UpdateContext(context);
            mUIAutomationHelper.BusinessFlow = context.BusinessFlow;
        }

        public override void CloseDriver()
        {
            mUIAutomationHelper.StopRecording();
            mUIAutomationHelper = null;
        }

        public override Act GetCurrentElement()
        {
            return null;
        }

        public override void RunAction(Act act)
        {
            //TODO: add func to Act + Enum for switch
            string actClass = act.GetType().Name;

            mUIAutomationHelper.mLoadTimeOut = mActionTimeout;
            mUIAutomationHelper.taskFinished = false;

            if (!actClass.Equals(typeof(ActSwitchWindow)) || (actClass.Equals(typeof(ActWindow)) && ((ActWindow)act).WindowActionType != ActWindow.eWindowActionType.Switch))
            {
                var checkWindow = true;
                if(actClass.Equals(typeof(ActUIElement)) && ((ActUIElement)act).ElementAction.Equals(ActUIElement.eElementAction.Switch))
                {
                    checkWindow = false;
                }
                if (checkWindow)
                {
                    CheckRetrySwitchWindowIsNeeded();
                }
                
            }

            if (act.Timeout != null)
            {
                mUIAutomationHelper.mLoadTimeOut = act.Timeout;
            }

            try
            {
                switch (actClass)
                {
                    case "ActWindow":
                        ActWindow actWindow = (ActWindow)act;
                        HandleWindowAction(actWindow);
                        break;

                    //TODO: ActSwitchWindow is the correct approach for switch window. And we should guide the users accordingly.
                    case "ActSwitchWindow":
                        mUIAutomationHelper.SmartSwitchWindow((ActSwitchWindow)act);
                        break;
                    case "ActWindowsControl":
                        HandleWindowsControlAction((ActWindowsControl)act);
                        break;

                    case "ActGenElement":
                        ActGenElement AGE = (ActGenElement)act;
                        HandleWindowsGenericWidgetControlAction(AGE);
                        break;

                    case "ActMenuItem":
                        ActMenuItem actMenuItem = (ActMenuItem)act;
                        HandleMenuControlAction(actMenuItem);
                        break;

                    case "ActSmartSync":
                        mUIAutomationHelper.SmartSyncHandler((ActSmartSync)act);
                        break;

                    case "ActScreenShot":
                        try
                        {
                            //TODO: When capturing all windows, we do showwindow. for few applications show window is causing application to minimize
                            //Disabling the capturing all windows for Windows driver until we fix show window issue
                            
                            Bitmap bmp = mUIAutomationHelper.GetCurrentWindowBitmap();
                            act.AddScreenShot(bmp);
                            //if not running well. need to add return same as PBDrive
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;
                    
                case "ActUIElement":
                    HandleUIElementAction(act);
                    break;
                case "ActBrowserElement":
                    ActBrowserElement actWBE = (ActBrowserElement)act;
                    HandleWindowsBrowserElementAction(actWBE);
                    break;
                case "ActTableElement":
                    ActTableElement actTable = (ActTableElement)act;
                    HandleWindowsWidgetTableControlAction(actTable);
                    break;

                    default:
                        throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
                }                
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                CheckAndRetryRunAction(act, e);
                return;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                CheckAndRetryRunAction(act, e);
                return;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                CheckAndRetryRunAction(act, e);
                return;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.WARN, "Exception at Run action", e);
                act.Error = e.Message;
            }
        }

        private void HandleWindowsWidgetTableControlAction(ActTableElement actWWC)
        {
            HtmlAgilityPack.HtmlNode WinWidEle = mUIAutomationHelper.GetHTMLHelper().GetActNode(actWWC);
            if (WinWidEle == null)
            {
                actWWC.Error = "Element not Found - " + actWWC.LocateBy + " " + actWWC.LocateValueCalculated;
                return;
            }
            mUIAutomationHelper.GetHTMLHelper().HandleWidgetTableControlAction(actWWC, WinWidEle);
        }

        private void HandleWindowsBrowserElementAction(ActBrowserElement actWBE)
        {
            string result = "";
            switch (actWBE.ControlAction)
            {
                case ActBrowserElement.eControlAction.InitializeBrowser:
                    object AE = mUIAutomationHelper.GetActElement(actWBE);
                    if (AE == null)
                    {
                        actWBE.Error = "Element not Found - " + actWBE.LocateBy + " " + actWBE.LocateValueCalculated;
                        return;
                    }
                    result = mUIAutomationHelper.InitializeBrowser(AE);
                    if (result.Equals("true"))
                    {
                        actWBE.AddOrUpdateReturnParamActual("Actual", result);
                        actWBE.ExInfo = "Browser Initialized";
                    }
                    else
                    {
                        actWBE.Error = "Unable to initialize browser";
                    }
                    break;
                case ActBrowserElement.eControlAction.GetPageSource:
                    result = mUIAutomationHelper.GetHTMLHelper().GetPageInfo("PageSource");
                    if (!result.Equals(""))
                    {
                        actWBE.AddOrUpdateReturnParamActual("Actual", result);
                        actWBE.ExInfo = result;
                    }
                    else
                    {
                        actWBE.Error = "Unable to fetch Page Source";
                    }
                    break;
                case ActBrowserElement.eControlAction.GetPageURL:
                    result = mUIAutomationHelper.GetHTMLHelper().GetPageInfo("PageURL");
                    if (!result.Equals(""))
                    {
                        actWBE.AddOrUpdateReturnParamActual("Actual", result);
                        actWBE.ExInfo = result;
                    }
                    else
                    {
                        actWBE.Error = "Unable to get Page URL";
                    }

                    break;
                case ActBrowserElement.eControlAction.SwitchFrame:
                    result=mUIAutomationHelper.GetHTMLHelper().SwitchFrame(actWBE.LocateBy, actWBE.LocateValueCalculated);
                    if (result.Equals("true"))
                    {
                        actWBE.AddOrUpdateReturnParamActual("Actual", result);
                        actWBE.ExInfo = "Switched to frame";
                    }
                    else
                    {
                        actWBE.Error = "Unable Switch frame";
                    }                    
                    break;
                case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                    mUIAutomationHelper.GetHTMLHelper().SwitchToDefaultFrame();
                    actWBE.AddOrUpdateReturnParamActual("Actual", result);
                    actWBE.ExInfo = "Switched to Default Frame";
                    break;
                default:
                    actWBE.Error = "Unable to perform operation";
                    break;
            }
        }

        private void HandleUIElementAction(Act actWC)
        {
            ActUIElement actUIElement = (ActUIElement)actWC;
            object AE = null;
            if (!actUIElement.ElementType.Equals(eElementType.Window) && !actUIElement.ElementAction.Equals(ActUIElement.eElementAction.Switch))
            {
                string locateValue = actUIElement.ElementLocateValueForDriver;
                AE = mUIAutomationHelper.FindElementByLocator((eLocateBy)actUIElement.ElementLocateBy, locateValue);

                if (AE == null && actUIElement.ElementAction != ActUIElement.eElementAction.IsEnabled)
                {
                    actUIElement.Error = "Element not Found - " + actUIElement.ElementLocateBy + " " + actUIElement.ElementLocateValueForDriver;
                    return;
                }
            }
            switch (actUIElement.ElementAction)
            {
                case ActUIElement.eElementAction.DragDrop:
                    mUIAutomationHelper.DragAndDrop(AE, actUIElement);
                    break;

                case ActUIElement.eElementAction.ClickAndValidate:
                    string status = mUIAutomationHelper.ClickAndValidteHandler(AE, actUIElement);
                    if (!status.Contains("Clicked Successfully"))
                    {
                        actUIElement.Error += status;
                    }
                    else
                    {
                        actUIElement.ExInfo += status;
                    }
                    break;
                case ActUIElement.eElementAction.Switch:
                    mUIAutomationHelper.ActUISwitchWindow(actUIElement);
                    break;

                default:
                    actUIElement.Error = "Unable to perform operation";
                    break;
            }
        }

        private void HandleWindowAction(ActWindow actWindow)
        {
            try
            {
                switch (actWindow.WindowActionType)
                {
                    case ActWindow.eWindowActionType.Switch:
                        bool b =
                            mUIAutomationHelper.HandleSwitchWindow(actWindow.LocateBy, actWindow.LocateValueCalculated);

                        if (!b)
                        {
                            actWindow.Error = "Window not found: " + actWindow.LocateValue;
                        }
                        break;
                    case ActWindow.eWindowActionType.IsExist:
                        string val = mUIAutomationHelper.IsWindowExist(actWindow).ToString();
                        actWindow.Error = "";
                        actWindow.AddOrUpdateReturnParamActual("Actual", val);
                        actWindow.ExInfo = val;
                        break;
                    case ActWindow.eWindowActionType.Close:
                        bool isClosed = mUIAutomationHelper.CloseWindow(actWindow);
                        if (!isClosed)
                        {
                            actWindow.Error = "Window cannot be closed, please use the close window button.";
                        }
                        break;
                    case ActWindow.eWindowActionType.Maximize:                       
                    case ActWindow.eWindowActionType.Minimize:                        
                    case ActWindow.eWindowActionType.Restore:
                        bool isDone = mUIAutomationHelper.SetWindowVisualState(actWindow);
                        if (!isDone)
                        {
                            actWindow.Error = "Window Action " + actWindow.WindowActionType.ToString() + " could not be performed.";
                        }
                        break;

                    default:
                        actWindow.Error = "Unknown Action  - " + actWindow.WindowActionType;
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void HandleWindowsControlAction(ActWindowsControl actWC)
        {
            object AE = null;
            if (actWC.ControlAction != ActWindowsControl.eControlAction.IsExist)
            {
                AE = mUIAutomationHelper.GetActElement(actWC);
                if (AE == null)
                {
                    actWC.Error = "Element not Found - " + actWC.LocateBy + " " + actWC.LocateValueCalculated;
                    return;
                }
            }
            try
            {
                switch (actWC.ControlAction)
                {
                    case ActWindowsControl.eControlAction.SetValue:
                        mUIAutomationHelper.SetControlValue(AE, actWC.ValueForDriver);
                        actWC.ExInfo = actWC.ValueForDriver + " set";
                        break;

                    case ActWindowsControl.eControlAction.SendKeys:
                        mUIAutomationHelper.SendKeysToControl(AE, actWC.ValueForDriver);
                        actWC.ExInfo = actWC.ValueForDriver + " set";
                        break;

                    case ActWindowsControl.eControlAction.GetValue:
                        string val = mUIAutomationHelper.GetControlValue(AE);
                        actWC.AddOrUpdateReturnParamActual("Actual", val);
                        actWC.ExInfo = val;
                        break;

                    case ActWindowsControl.eControlAction.Select:
                        mUIAutomationHelper.SetControlValue(AE, actWC.ValueForDriver);
                        break;

                    case ActWindowsControl.eControlAction.Click:
                        string status = mUIAutomationHelper.ClickElement(AE);
                        if (!status.Contains("Clicked Successfully"))
                        {
                            actWC.Error += status;
                        }
                        else
                            actWC.ExInfo += status;
                        break;

                    case ActWindowsControl.eControlAction.AsyncClick:
                        status = mUIAutomationHelper.ClickElement(AE, true);
                        if (!status.Contains("Clicked Successfully"))
                        {
                            actWC.Error += status;
                        }
                        else
                            actWC.ExInfo += status;
                        break;

                    case ActWindowsControl.eControlAction.ClickXY:
                        mUIAutomationHelper.ClickOnXYPoint(AE, actWC.ValueForDriver);
                        break;

                    case ActWindowsControl.eControlAction.RightClick:
                        mUIAutomationHelper.DoRightClick(AE,actWC.ValueForDriver);
                        break;

                    case ActWindowsControl.eControlAction.DoubleClick:
                        mUIAutomationHelper.DoDoubleClick(AE,actWC.ValueForDriver);
                        break;

                    case ActWindowsControl.eControlAction.Maximize:
                    case ActWindowsControl.eControlAction.Minimize:
                    case ActWindowsControl.eControlAction.Restore:
                        status = mUIAutomationHelper.SetElementVisualState(AE, actWC.ControlAction.ToString());
                        if (!status.Contains("State set successfully"))
                        {
                            actWC.Error = status;
                        }
                        else
                            actWC.ExInfo += status;
                        break;
                    case ActWindowsControl.eControlAction.Resize:
                        status = mUIAutomationHelper.SetElementSize(AE, actWC.ValueForDriver);
                        if (!status.Contains("Element resize Successfully"))
                        {
                            actWC.Error = status;
                        }
                        else
                            actWC.ExInfo += status;
                        break;
                    case ActWindowsControl.eControlAction.GetText:
                        string valText=mUIAutomationHelper.GetControlText(AE,actWC.ValueForDriver);
                        actWC.AddOrUpdateReturnParamActual("Actual", valText);
                        actWC.ExInfo = valText;
                        break;

                    case ActWindowsControl.eControlAction.GetSelected:
                        string selectedItem = mUIAutomationHelper.GetSelectedItem(AE);
                        actWC.AddOrUpdateReturnParamActual("Selected Item", selectedItem);
                        actWC.ExInfo = selectedItem;
                        break;

                    case ActWindowsControl.eControlAction.GetTitle:
                        string title = mUIAutomationHelper.GetDialogTitle(AE);
                        actWC.AddOrUpdateReturnParamActual("Dialog Title", title);
                        actWC.ExInfo = title;
                        break;

                    case ActWindowsControl.eControlAction.Toggle:
                        string value = mUIAutomationHelper.ToggleControlValue(AE);
                        actWC.AddOrUpdateReturnParamActual("Actual", value);
                        actWC.ExInfo = value;
                        break;

                    case ActWindowsControl.eControlAction.IsEnabled:
                        string valueIsEnabled = mUIAutomationHelper.IsEnabledControl(AE);
                        actWC.AddOrUpdateReturnParamActual("Actual", valueIsEnabled);
                        actWC.ExInfo = valueIsEnabled;
                        break;

                    case ActWindowsControl.eControlAction.IsExist:
                        string valueIsExist = mUIAutomationHelper.IsElementExist(actWC.LocateBy, actWC.LocateValue).ToString();
                        actWC.Error = "";
                        actWC.AddOrUpdateReturnParamActual("Actual", valueIsExist);
                        actWC.ExInfo = valueIsExist;
                        break;

                    case ActWindowsControl.eControlAction.Scrolldown:
                        mUIAutomationHelper.ScrollDown(AE);
                        break;

                    case ActWindowsControl.eControlAction.ScrollUp:
                        mUIAutomationHelper.ScrollUp(AE);
                        break;

                    case ActWindowsControl.eControlAction.IsSelected:
                        string isSelected = mUIAutomationHelper.IsControlSelected(AE);
                        actWC.AddOrUpdateReturnParamActual("Actual", isSelected);
                        actWC.ExInfo = isSelected;
                        break;

                    case ActWindowsControl.eControlAction.Highlight:
                        mUIAutomationHelper.HiglightElement(mUIAutomationHelper.GetElementInfoFor(AE));
                        break;

                    case ActWindowsControl.eControlAction.GetControlProperty:
                        string propValue = mUIAutomationHelper.GetControlPropertyValue(AE, actWC.ValueForDriver);
                        actWC.AddOrUpdateReturnParamActual("Actual", propValue);
                        actWC.ExInfo = propValue;
                        break;
                    
                    case ActWindowsControl.eControlAction.Repaint:
                        mUIAutomationHelper.HandlePaintWindow(AE);
                        break;
                    case ActWindowsControl.eControlAction.SelectContextMenuItem:
                        bool result = false;
                        result=mUIAutomationHelper.ClickContextMenuItem(AE, actWC.Value);
                        
                        if (result)
                            actWC.ExInfo += "Element Clicked";
                        else
                            actWC.Error="Unable to Click Element";
                        break;
                    case ActWindowsControl.eControlAction.Expand:
                        mUIAutomationHelper.ExpandComboboxByUIA(AE);
                        break;
                    default:
                        actWC.Error = "Unknown Action  - " + actWC.ControlAction;
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        private void HandleMenuControlAction(ActMenuItem actMenuItem)
        {
            object AE;
            try
            {
                switch (actMenuItem.MenuAction)
                {
                    case ActMenuItem.eMenuAction.Expand:
                        AE = mUIAutomationHelper.GetActElement(actMenuItem);
                        if (AE == null)
                        {
                            actMenuItem.Error = "Unable to locate Menu Item";
                            return;
                        }
                        mUIAutomationHelper.ExpandControlElement(AE);
                        break;

                    case ActMenuItem.eMenuAction.Collapse:
                        AE = mUIAutomationHelper.GetActElement(actMenuItem);
                        if(AE==null)
                        {
                            actMenuItem.Error = "Unable to locate Menu Item";
                            return;
                        }
                        mUIAutomationHelper.CollapseControlElement(AE);
                        break;

                    case ActMenuItem.eMenuAction.Click:

                        mUIAutomationHelper.ClickMenuElement(actMenuItem);
                        break;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        
        public override string GetURL()
        {
            return "TBD";
        }
        

        public override void HighlightActElement(Act act)
        {
            object AE = mUIAutomationHelper.GetActElement(act);
            mUIAutomationHelper.HiglightElement(mUIAutomationHelper.GetElementInfoFor(AE));
        }

        public override ePlatformType Platform
        {
            get
            {
                return ePlatformType.Windows;
            }
        }

        public override bool IsRunning()
        {
                return true;
        }

        //TODO: dup with PB Driver - move to UIAuto and pass param window type: All, Windows, PB etc...
        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            return mUIAutomationHelper.GetListOfDriverAppWindows();
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            mUIAutomationHelper.SwitchToWindow(Title);
        }


        
        string IWindowExplorer.GetFocusedControl()
        {
            return null;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            return GetControlFromMousePosition();
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            return EI;
        }

        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false)
        {
            List<ElementInfo> list = mUIAutomationHelper.GetVisibleControls();
            return list;
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            return GetElementChildren(ElementInfo);           
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            return GetElementProperties(ElementInfo);       
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            return GetElementLocators(ElementInfo);
        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            object obj = null;
            if (ElementInfo.GetType().Equals(typeof(HTMLElementInfo)))
            {
                HTMLElementInfo EI = (HTMLElementInfo)ElementInfo;
                obj = mUIAutomationHelper.GetHTMLHelper();
                obj = EI.ElementObject;

            }
            else if (ElementInfo.GetType().Equals(typeof(UIAElementInfo)))
            {
                UIAElementInfo EI = (UIAElementInfo)ElementInfo;
                obj = mUIAutomationHelper.GetElementData(EI.ElementObject);
            }
            return obj;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            AppWindow aw = new AppWindow();
            if (mUIAutomationHelper.GetCurrentWindow() == null) return null;
            if (!mUIAutomationHelper.IsWindowValid(mUIAutomationHelper.GetCurrentWindow())) return null;
            aw.Title = mUIAutomationHelper.GetWindowInfo(mUIAutomationHelper.GetCurrentWindow());
            return aw;
        }
        
        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            HighLightElement(ElementInfo);           
        }

        private void HandleWindowsGenericWidgetControlAction(ActGenElement actWinC)
        {
            IHTMLElement element = mUIAutomationHelper.GetHTMLHelper().GetActElement(actWinC);
            if (element == null)
            {
                actWinC.Error = "Element not Found - " + actWinC.LocateBy + " " + actWinC.LocateValueCalculated;
                return;
            }

            string value = string.Empty;
            bool result = false;
            string ValDrv = actWinC.ValueForDriver;
            try
            {
                switch (actWinC.GenElementAction)
                {
                    case ActGenElement.eGenElementAction.SetValue:

                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(element, ValDrv);
                        if (result)
                            actWinC.ExInfo = ValDrv + " set";
                        else
                            actWinC.Error = "Unable to set value to " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.SetAttributeValue:
                        string attrName = "value";
                        string attValue = ValDrv;
                        if(ValDrv.IndexOf("=") > 0)
                        {
                            attrName = ValDrv.Split('=')[0];
                            attValue = ValDrv.Split('=')[1];
                        }
                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(element, attValue, attrName);
                        if (result)
                            actWinC.ExInfo = ValDrv + " set";
                        else
                            actWinC.Error = "Unable to set attribute " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.FireSpecialEvent:

                        value = mUIAutomationHelper.GetHTMLHelper().FireSpecialEvent(element, ValDrv);
                        if (value.StartsWith("Error"))
                            actWinC.Error = "Unable to fire special event. " + value;
                        else
                            actWinC.ExInfo = "Fire special event " + value;
                        break;
                    case ActGenElement.eGenElementAction.SendKeys:

                        result = mUIAutomationHelper.GetHTMLHelper().SendKeys(element, ValDrv);
                        if (result)
                            actWinC.ExInfo = ValDrv + " Keys Sent";
                        else
                            actWinC.Error = "Unable to Send Keys " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.GetValue:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(element, "value");
                        if (string.IsNullOrEmpty(value))
                        {
                            actWinC.Error = "Unable to Get value of " + ValDrv;
                        }
                        else
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value.ToString());
                            actWinC.ExInfo = value.ToString();
                        }
                        break;
                    case ActGenElement.eGenElementAction.ClickAt:
                        result = mUIAutomationHelper.GetHTMLHelper().ClickAt(element,ValDrv);
                        if (result) actWinC.ExInfo = "Element Clicked";
                        else actWinC.Error = "Element Unable to Clicked";
                        break;                    
                    case ActGenElement.eGenElementAction.Click:
                    case ActGenElement.eGenElementAction.AsyncClick:
                        result = mUIAutomationHelper.GetHTMLHelper().Click(element);
                        if (result) actWinC.ExInfo = "Element Clicked";
                        else actWinC.Error = "Element Unable to Clicked";
                        break;
                    case ActGenElement.eGenElementAction.RightClick:
                        result = mUIAutomationHelper.GetHTMLHelper().RightClick(element, ValDrv);
                        if (result) actWinC.ExInfo = "Element Right Click Done";
                        else actWinC.Error = "Element Unable to Right Click";
                        break;
                    case ActGenElement.eGenElementAction.Enabled:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(element, "disabled");
                        value = value.ToString().ToLower().Equals("false") ? "true" : "false";
                        actWinC.AddOrUpdateReturnParamActual("Actual", value);
                        actWinC.ExInfo = value.ToString();
                        break;
                    case ActGenElement.eGenElementAction.Hover:
                        result = mUIAutomationHelper.GetHTMLHelper().MouseHover(element, ValDrv);
                        if (result) actWinC.ExInfo = "Element Hover Done";
                        else actWinC.Error = "Unable to Hover the Element";
                        break;
                    case ActGenElement.eGenElementAction.ScrollToElement:
                        result = mUIAutomationHelper.GetHTMLHelper().scrolltoElement(element);
                        if (result) actWinC.ExInfo = "Scroll to Element Done";
                        else actWinC.Error = "Unable to Scroll to Element";
                        break;
                    case ActGenElement.eGenElementAction.Visible:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(element, "type");
                        value = value.Equals("hidden") ? "false" : "true";
                        actWinC.AddOrUpdateReturnParamActual("Actual", value);
                        actWinC.ExInfo = value.ToString();
                        break;
                    case ActGenElement.eGenElementAction.SelectFromDropDown:

                        value = mUIAutomationHelper.GetHTMLHelper().SelectFromDropDown(element, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actWinC.Error = "Unable to select value-" + ValDrv + " from DropDown";
                        }
                        break;
                    case ActGenElement.eGenElementAction.SelectFromDropDownByIndex:

                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(element, ValDrv);
                        if (result) actWinC.ExInfo = ValDrv + " set";
                        else actWinC.Error = "Unable to Set Value " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.GetInnerText:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(element, "innerText");
                        if (!string.IsNullOrEmpty(value))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actWinC.Error = "Unable to get Inner Text";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetStyle:
                        value = mUIAutomationHelper.GetHTMLHelper().GetStyle(element);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actWinC.Error = "Unable to get Element Style";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetElementAttributeValue:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(element, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actWinC.Error = "Unable to get custom attribute";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetCustomAttribute:
                        value = mUIAutomationHelper.GetHTMLHelper().GetNodeAttributeValue(element, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actWinC.Error = "Unable to get custom attribute";
                        }
                        break;
                    case ActGenElement.eGenElementAction.SwitchFrame:
                        value = mUIAutomationHelper.GetHTMLHelper().SwitchFrame(actWinC.LocateBy, actWinC.LocateValueCalculated);
                        if (value.Equals("true"))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = "Switched to frame";
                        }
                        else
                        {
                            actWinC.Error = "Unable Switch frame";
                        }
                        break;
                    case ActGenElement.eGenElementAction.HighLightElement:
                        value = mUIAutomationHelper.GetHTMLHelper().HighLightElement(element);
                        if (value.Equals("true"))
                        {
                            actWinC.AddOrUpdateReturnParamActual("Actual", value);
                            actWinC.ExInfo = "Element Highlighted";
                        }
                        else
                        {
                            actWinC.Error = "Unable To Highlight Element";
                        }
                        break;

                    default:
                        actWinC.Error = "Unknown Action  - " + actWinC.GenElementAction;
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
         }

        public Bitmap GetScreenShot(Tuple<int, int> setScreenSize = null)
        {
            Bitmap bmp = mUIAutomationHelper.GetCurrentWindowBitmap();
            return bmp;
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {            
            return mUIAutomationHelper.GetElements(EL);
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {
        }

        public VisualElementsInfo GetVisualElementsInfo()
        {
            List<ElementInfo> list = mUIAutomationHelper.GetVisibleControls();

            VisualElementsInfo VEI = new VisualElementsInfo();
            foreach(ElementInfo EI in list)
            {
                VisualElement VE = new VisualElement() { ElementType =EI.ElementType, Text = EI.ElementName , X = EI.X, Y = EI.Y, Width = EI.Width, Height = EI.Height };
                VEI.Elements.Add(VE);                
            }
            return VEI;
        }

        public void ChangeAppWindowSize(int Width, int Height)
        {
            throw new NotImplementedException();
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return mUIAutomationHelper.IsWindowValid(obj);
        }

        public override void StartRecording()
        {
            mUIAutomationHelper.StartRecording();
        }

        public override void StopRecording()
        {

        }

        void IWindowExplorer.UnHighLightElements()
        {
            throw new NotImplementedException();
        }

        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
        {
            throw new NotImplementedException();
        }

        public override void ActionCompleted(Act act)
        {
            mUIAutomationHelper.taskFinished = true;
            if (!String.IsNullOrEmpty(act.Error) && act.Error.StartsWith("Time out !"))
            {
                Thread.Sleep(1000);
            }
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            throw new NotImplementedException();
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            throw new NotImplementedException();
        }

        public void StartSpying()
        {
            throw new NotImplementedException();
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }
    }
}
