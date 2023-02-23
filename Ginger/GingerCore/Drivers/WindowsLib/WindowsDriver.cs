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

extern alias UIAComWrapperNetstandard;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.UIAutomation;
using GingerCore.Actions.VisualTesting;
using GingerCore.Actions.Windows;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

using System.Linq;
using System.Reflection;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using OpenQA.Selenium;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;

namespace GingerCore.Drivers.WindowsLib
{
    public class WindowsDriver : UIAutomationDriverBase, IWindowExplorer, IVisualTestingDriver,IVirtualDriver
    {
        int mActionTimeout = 10;

        [UserConfigured]
        [UserConfiguredDefault("150")] 
        [UserConfiguredDescription("Action Timeout - default is 150 seconds")]
        public override int ActionTimeout
        {
            get
            {
                return mActionTimeout;
            }
            set
            { mActionTimeout = value; }
        }

        int mImplicitWait = 30;

        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Amount of time the driver should wait when searching for an element if it is not immediately present")]
        public int ImplicitWait 
        {
            get
            {
                return mImplicitWait;
            }
            set
            {
                mImplicitWait = value;
            }
        }

        public override bool StopProcess
        {
            get
            {
                return base.StopProcess;
            }
            set
            {
                base.StopProcess = value;
                if (mUIAutomationHelper != null)
                {
                    mUIAutomationHelper.StopProcess = value;
                }
            }
        }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Applitool View Key number")]
        public String ApplitoolsViewKey { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Applitool Server Url")]
        public String ApplitoolsServerUrl { get; set; }

        protected IWebDriver Driver;


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
                    ((UIAComWrapperHelper)mUIAutomationHelper).mPlatform = ePlatformType.Windows;

                    mUIElementOperationsHelper = new UIElementOperationsHelper();

                    break;

            }
            mUIAutomationHelper.ImplicitWait = mImplicitWait;
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

                    case "ActUIElement":
                        HandleUIElementAction(act);
                        break;

                    case "ActWindowsControl":
                        HandleWindowsControlAction((ActWindowsControl)act);
                        break;

                    case "ActWindow":
                        ActWindow actWindow = (ActWindow)act;
                        HandleWindowAction(actWindow);
                        break;

                    //TODO: ActSwitchWindow is the correct approach for switch window. And we should guide the users accordingly.
                    case "ActSwitchWindow":
                        mUIAutomationHelper.SmartSwitchWindow((ActSwitchWindow)act);
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

                    case "ActBrowserElement":
                        ActBrowserElement actWBE = (ActBrowserElement)act;
                        HandleWindowsBrowserElementAction(actWBE);
                        break;
                    case "ActTableElement":
                        ActTableElement actTable = (ActTableElement)act;
                        HandleWindowsWidgetTableControlAction(actTable);
                        break;
                    case "ActVisualTesting":
                        ActVisualTesting actVisual = (ActVisualTesting)act;
                        actVisual.Execute(this);
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
            catch (UIAuto.ElementNotAvailableException e)
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
                    actWBE.Error = string.Format("Selected '{0}' Operation not supported for 'WindowsDriver'", actWBE.ControlAction.ToString());
                    break;
            }
        }

        private void HandleUIElementAction(Act act)
        {
            ActUIElement actUIElement = (ActUIElement)act;
            UIAuto.AutomationElement automationElement = null;
            eElementType elementType=eElementType.Unknown;

            if (actUIElement.ElementLocateBy.Equals(eLocateBy.POMElement))
            {
                automationElement = HandlePOMElememnt(actUIElement);
                if (automationElement == null)
                {
                    actUIElement.Error = "Element not Found - " + actUIElement.ElementLocateBy + " " + actUIElement.ElementLocateValueForDriver;
                    return;
                }
            }
            else if (actUIElement.ElementType!= eElementType.Window && actUIElement.ElementAction !=ActUIElement.eElementAction.IsExist)
            {
                automationElement = (UIAuto.AutomationElement)mUIAutomationHelper.FindElementByLocator(actUIElement.ElementLocateBy, actUIElement.ElementLocateValueForDriver);

                if (automationElement == null && actUIElement.ElementAction != ActUIElement.eElementAction.IsEnabled)
                {
                    actUIElement.Error = "Element not Found - " + actUIElement.ElementLocateBy + " " + actUIElement.ElementLocateValueForDriver;
                    return;
                }
                elementType = WindowsPlatform.GetElementType(mUIAutomationHelper.GetElementControlType(automationElement), mUIAutomationHelper.GetControlPropertyValue(automationElement, "ClassName"));
            }

            int x, y;
            Boolean isoutputvalue = false;
            ActionResult actionResult = new ActionResult();

            switch (actUIElement.ElementAction)
            {

                case ActUIElement.eElementAction.Click:
                    actionResult = mUIElementOperationsHelper.ClickElement(automationElement);
                    break;

                case ActUIElement.eElementAction.MouseClick:
                    actionResult = mUIElementOperationsHelper.MouseClickElement(automationElement);
                    break;

                case ActUIElement.eElementAction.AsyncClick:
                    actionResult = mUIElementOperationsHelper.AsyncClickElement(automationElement);
                    break;

                case ActUIElement.eElementAction.ClickXY:
                    x = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate));
                    y = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate));
                    actionResult = mUIElementOperationsHelper.ClickElementUsingXY(automationElement, x, y);
                    break;
                case ActUIElement.eElementAction.Collapse:
                    actionResult = mUIElementOperationsHelper.CollapseElement(automationElement);
                    break;
                case ActUIElement.eElementAction.DoubleClickXY:
                    x = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate));
                    y = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate));
                    actionResult = mUIElementOperationsHelper.DoubleClickElementUsingXY(automationElement, x, y);
                    break;

                case ActUIElement.eElementAction.RightClickXY:
                    x = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate));
                    y = Int32.Parse(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate));
                    actionResult = mUIElementOperationsHelper.RightClickElementUsingXY(automationElement, x, y);
                    break;

                case ActUIElement.eElementAction.GetValue:
                    actionResult = mUIElementOperationsHelper.GetValue(automationElement, elementType);
                    isoutputvalue = true;
                    break;

                case ActUIElement.eElementAction.GetText:
                    actionResult = mUIElementOperationsHelper.GetText(automationElement);
                    isoutputvalue = true;
                    break;

                case ActUIElement.eElementAction.GetSelectedValue:
                    actionResult = mUIElementOperationsHelper.GetSelectedValue(automationElement);
                    isoutputvalue = true;
                    break;

                case ActUIElement.eElementAction.GetWindowTitle:
                    object windowElement = mUIAutomationHelper.FindWindowByLocator(actUIElement.ElementLocateBy, actUIElement.ElementLocateValueForDriver);
                    if (windowElement != null)
                    {
                        actionResult = mUIElementOperationsHelper.GetTitle((UIAuto.AutomationElement)windowElement);
                    }
                    else 
                    {
                        actionResult = mUIElementOperationsHelper.GetTitle(automationElement);
                    }
                    isoutputvalue = true;
                    break;

                case ActUIElement.eElementAction.SetValue:
                    actionResult = mUIElementOperationsHelper.SetValue(automationElement, actUIElement.ValueForDriver);
                    break;

                case ActUIElement.eElementAction.SetText:
                    actionResult = mUIElementOperationsHelper.SetText(automationElement, actUIElement.ValueForDriver);
                    break;

                case ActUIElement.eElementAction.SendKeys:
                    actionResult = mUIElementOperationsHelper.SendKeys(automationElement, actUIElement.ValueForDriver);
                    break;

                case ActUIElement.eElementAction.Select:
                    actionResult = mUIElementOperationsHelper.SelectValue(automationElement, elementType, actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect));
                    break;

                case ActUIElement.eElementAction.Toggle:
                    actionResult = mUIElementOperationsHelper.ToggleElement(automationElement, elementType);
                    break;

                case ActUIElement.eElementAction.Expand:
                    actionResult = mUIElementOperationsHelper.ExpandElement(automationElement);
                    break;

                case ActUIElement.eElementAction.Switch:
                    mUIAutomationHelper.ActUISwitchWindow(actUIElement);
                    actionResult.executionInfo = "Switch window performed successfully";
                    break;

                case ActUIElement.eElementAction.CloseWindow:
                    bool isClosed = mUIAutomationHelper.CloseWindow(actUIElement);

                    Object windowToClose = mUIAutomationHelper.FindWindowByLocator(actUIElement.ElementLocateBy, actUIElement.ElementLocateValueForDriver);
                    if (windowToClose != null)
                    {
                        actionResult = mUIElementOperationsHelper.CloseWindow((UIAuto.AutomationElement)windowToClose);
                    }
                    else
                    {
                        actionResult.errorMessage = "Failed to find the window";
                    }
                    break;

                case ActUIElement.eElementAction.IsExist:
                    if (eElementType.Window == actUIElement.ElementType)
                    {
                        try
                        {
                            object window = mUIAutomationHelper.FindWindowByLocator(actUIElement.ElementLocateBy, actUIElement.ElementLocateValueForDriver).ToString();
                            if (window != null)
                            {
                                actionResult.outputValue = Boolean.TrueString;
                            }
                            else
                            {
                                actionResult.outputValue = Boolean.FalseString;
                            }
                        }
                        catch (Exception ex)
                        {
                            actionResult.errorMessage = ex.Message;
                        }
                    }
                    else
                    {
                        actionResult.outputValue = mUIAutomationHelper.IsElementExist(actUIElement.ElementLocateBy, actUIElement.ElementLocateValueForDriver).ToString();
                    }
                    isoutputvalue = true;
                    break;

                case ActUIElement.eElementAction.GetControlProperty:
                    try
                    {
                        actionResult.outputValue = mUIAutomationHelper.GetControlPropertyValue(automationElement, actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect));
                        isoutputvalue = true;
                    }
                    catch (Exception e)
                    {
                        actionResult.errorMessage = "Exception in GetControlPropertyValue";
                    }

                    break;

                //case ActUIElement.eElementAction.DragDrop:
                //    mUIAutomationHelper.DragAndDrop(AE, actUIElement);
                //    break;

                case ActUIElement.eElementAction.ClickAndValidate:
                    actionResult = ClickAndValidte(automationElement, actUIElement);
                    break;
                case ActUIElement.eElementAction.Maximize:
                    actionResult = mUIElementOperationsHelper.SetWindowState(automationElement, Interop.UIAutomationClient.WindowVisualState.WindowVisualState_Maximized);
                    break;
                case ActUIElement.eElementAction.Minimize:
                    actionResult = mUIElementOperationsHelper.SetWindowState(automationElement, Interop.UIAutomationClient.WindowVisualState.WindowVisualState_Minimized);
                    break;

                //case ActUIElement.eElementAction.ScrollDown:
                //    mUIAutomationHelper.ScrollDown(AE);
                //    break;

                //case ActUIElement.eElementAction.ScrollUp:
                //    mUIAutomationHelper.ScrollUp(AE);
                //    break;


                default:
                    actUIElement.Error = string.Format("Selected '{0}' Operation not supported for 'WindowsDriver'", actUIElement.ElementAction.ToString());
                    break;
            }

            if(string.IsNullOrEmpty(actionResult.errorMessage))
            {
                if(isoutputvalue)
                    actUIElement.AddOrUpdateReturnParamActual("Actual", actionResult.outputValue);

                actUIElement.ExInfo = actionResult.executionInfo;
            }
            else
            {
                actUIElement.Error = actionResult.errorMessage;
            }
        }
        public ActionResult ClickAndValidte(UIAuto.AutomationElement automationElement, ActUIElement act)
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

            List<ActUIElement.eElementAction> clicks = PlatformInfoBase.GetPlatformImpl(mUIAutomationHelper.mPlatform).GetPlatformUIClickTypeList();
            UIAuto.AutomationElement elementToValidate = (UIAuto.AutomationElement)mUIAutomationHelper.FindElementByLocator(validationElementLocateby, validattionElementLocateValue);

            //perform click
            bool isClicked = mUIElementOperationsHelper.PerformClick(automationElement, clickType);
            if (isClicked)
            {
                //validate
                bool isValidated = mUIElementOperationsHelper.LocateAndValidateElement(elementToValidate, validationElementType, validationType);
                if (isValidated)
                {
                    actionResult.executionInfo = "Clicked Successfully And Validated Element.";
                    return actionResult;
                }
                if ((!isValidated) && (LoopNextCheck))
                {
                    actionResult = mUIElementOperationsHelper.ClickElementByOthertypes(clickType, clicks, automationElement, elementToValidate, validationElementType, validationType);
                }
                else
                {
                    actionResult.executionInfo = "Validation Failed.";
                }
            }
            else
            {
                if (LoopNextCheck)
                {
                    //click element by other types
                    actionResult = mUIElementOperationsHelper.ClickElementByOthertypes(clickType, clicks, automationElement, elementToValidate, validationElementType, validationType);
                }
            }

            return actionResult;
        }
        private UIAuto.AutomationElement HandlePOMElememnt(ActUIElement act)
        {
            ObservableList<ElementLocator> locators = new ObservableList<ElementLocator>();
            var pomExcutionUtil = new POMExecutionUtils(act,act.ElementLocateValue);
            var currentPOM = pomExcutionUtil.GetCurrentPOM();

            ElementInfo currentPOMElementInfo = null;
            if (currentPOM != null)
            {
                currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo();
                locators = currentPOMElementInfo.Locators;
            }
            UIAuto.AutomationElement windowElement = LocateElementByLocators(locators, true);

            if (windowElement == null)
            {
                if (pomExcutionUtil.AutoUpdateCurrentPOM(this.BusinessFlow.CurrentActivity.CurrentAgent) != null)
                {
                    windowElement = LocateElementByLocators(currentPOMElementInfo.Locators,true);
                    if (windowElement != null)
                    {
                        act.ExInfo += "Broken element was auto updated by Self healing operation";
                    }
                }
            }
            if (windowElement != null && currentPOMElementInfo.SelfHealingInfo == SelfHealingInfoEnum.ElementDeleted)
            {
                currentPOMElementInfo.SelfHealingInfo = SelfHealingInfoEnum.None;
            }
            if (windowElement != null)
            {
                pomExcutionUtil.PriotizeLocatorPosition();
            }

            return windowElement;
        }
        ////ActUIElement
        //private void HandleWindowControlUIElementAction(ActUIElement actUIElement, object AE)
        //{
        //    try
        //    {
        //        switch (actUIElement.ElementAction)
        //        {
        //            case ActUIElement.eElementAction.GetWindowTitle:
        //                string title = mUIAutomationHelper.GetDialogTitle(AE);
        //                actUIElement.AddOrUpdateReturnParamActual("Dialog Title", title);
        //                actUIElement.ExInfo = title;
        //                break;
        //            case ActUIElement.eElementAction.Switch:
        //                mUIAutomationHelper.ActUISwitchWindow(actUIElement);
        //                break;
        //            case ActUIElement.eElementAction.IsExist:
        //                string val = mUIAutomationHelper.IsWindowExist(actUIElement).ToString();
        //                actUIElement.Error = "";
        //                actUIElement.AddOrUpdateReturnParamActual("Actual", val);
        //                actUIElement.ExInfo = val;
        //                break;
        //            case ActUIElement.eElementAction.CloseWindow:
        //                bool isClosed = mUIAutomationHelper.CloseWindow(actUIElement);
        //                if (!isClosed)
        //                {
        //                    actUIElement.Error = "Window cannot be closed, please use the close window button.";
        //                }
        //                break;
        //            case ActUIElement.eElementAction.Maximize:                       
        //            case ActUIElement.eElementAction.Minimize:                        
        //                string status = mUIAutomationHelper.SetElementVisualState(AE, actUIElement.ElementAction.ToString());
        //                if (!status.Contains("State set successfully"))
        //                {
        //                    actUIElement.Error = status;
        //                }
        //                else
        //                {
        //                    actUIElement.ExInfo += status;
        //                }
        //                break;

        //            default:
        //                actUIElement.Error = "Unknown Action  - " + actUIElement.ActionType;
        //                break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

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

        ////ActUIElement
        //private void HandleMenuControlUIElementAction(ActUIElement actUIElement)
        //{
        //    object AE;
        //    try
        //    {
        //        switch (actUIElement.ElementAction)
        //        {
        //            case ActUIElement.eElementAction.Expand:
        //                AE = mUIAutomationHelper.GetActElement(actUIElement);
        //                if (AE == null)
        //                {
        //                    actUIElement.Error = "Unable to locate Menu Item";
        //                    return;
        //                }
        //                mUIAutomationHelper.ExpandControlElement(AE);
        //                break;

        //            case ActUIElement.eElementAction.Collapse:
        //                AE = mUIAutomationHelper.GetActElement(actUIElement);
        //                if (AE == null)
        //                {
        //                    actUIElement.Error = "Unable to locate Menu Item";
        //                    return;
        //                }
        //                mUIAutomationHelper.CollapseControlElement(AE);
        //                break;

        //            case ActUIElement.eElementAction.Click:
        //                mUIAutomationHelper.ClickMenuElement(actUIElement);
        //                break;

        //            default:
        //                actUIElement.Error = string.Format("Selected '{0}' Operation not supported for 'WindowsDriver'", actUIElement.ElementAction.ToString());
        //                break;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }
        //}

        public override string GetURL()
        {
            return ((IWindowExplorer)this).GetActiveWindow().Title;
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

        public ElementInfo LearnElementInfoDetails(ElementInfo EI, PomSetting pomSetting = null)
        {
            if (ElementInfo.IsElementTypeSupportingOptionalValues(EI.ElementTypeEnum))
            {
                EI.OptionalValuesObjectsList = ((IWindowExplorer)this).GetOptionalValuesList(EI, eLocateBy.ByXPath, EI.XPath);
            }
            if (EI.OptionalValuesObjectsList.Count > 0)
            {
                EI.OptionalValuesObjectsList[0].IsDefault = true;
            }

            EI.Properties = ((IWindowExplorer)this).GetElementProperties(EI);
            EI.Locators = ((IWindowExplorer)this).GetElementLocators(EI);
            foreach (var elementLocator in EI.Locators)
            {
                elementLocator.Active = true;
                elementLocator.IsAutoLearned = true;
            }

            return EI;
        }

        async Task<List<ElementInfo>> IWindowExplorer.GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null)
        {
            return await Task.Run(async () =>
            {
                if (foundElementsList == null)
                {
                    foundElementsList = new ObservableList<ElementInfo>();
                }
                List<ElementInfo> elementInfoList = await mUIAutomationHelper.GetVisibleControls();

                foreach (UIAElementInfo foundElemntInfo in elementInfoList)
                {
                    if(StopProcess)
                    {
                        break;
                    }

                    ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo,pomSetting);

                    bool learnElement = true;
                    if (pomSetting.filteredElementType != null)
                    {
                        if (!pomSetting.filteredElementType.Contains(foundElemntInfo.ElementTypeEnum))
                        {
                            learnElement = false;
                        }
                    }
                    if (learnElement)
                    {
                        foundElemntInfo.IsAutoLearned = true;
                        foundElementsList.Add(foundElemntInfo);
                    }
                }

                elementInfoList = General.ConvertObservableListToList<ElementInfo>(foundElementsList);
                return elementInfoList;
            });
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            return GetElementChildren(ElementInfo);           
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            //only return necessery properties
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
            UIAElementInfo uIAElement  = (UIAElementInfo)ElementInfo;
            if (!string.IsNullOrWhiteSpace(ElementInfo.ElementType))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.PlatformElementType, Value = ElementInfo.ElementType });
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(ElementInfo.ElementTypeEnum)))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.ElementType, Value = ElementInfo.ElementTypeEnum.ToString() });
            }
            if (!string.IsNullOrWhiteSpace(Convert.ToString(uIAElement.BoundingRectangle)))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.BoundingRectangle, Value = uIAElement.BoundingRectangle.ToString() });
            }
            if (!string.IsNullOrWhiteSpace(uIAElement.LocalizedControlType))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.LocalizedControlType, Value = uIAElement.LocalizedControlType });
            }
            if (!string.IsNullOrWhiteSpace(ElementInfo.ElementTitle))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.Name, Value = ElementInfo.ElementTitle });
            }
            if (!string.IsNullOrWhiteSpace(uIAElement.AutomationId))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.AutomationId, Value = uIAElement.AutomationId });
            }
            if (!string.IsNullOrWhiteSpace(uIAElement.Text))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.Text, Value = uIAElement.Text });
            }
            if (!string.IsNullOrWhiteSpace(uIAElement.ClassName))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.ClassName, Value = uIAElement.ClassName });
            }
            if (!string.IsNullOrWhiteSpace(uIAElement.ToggleState))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.ToggleState, Value = uIAElement.ToggleState });
            }
            if (!string.IsNullOrWhiteSpace(ElementInfo.XPath))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.XPath, Value = ElementInfo.XPath });
            }
            if (!string.IsNullOrWhiteSpace(ElementInfo.Value))
            {
                list.Add(new ControlProperty() { Name = ElementProperty.Value, Value = ElementInfo.Value });
            }
            
            list.Add(new ControlProperty() { Name = ElementProperty.Height, Value = ElementInfo.Height.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.Width, Value = ElementInfo.Width.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.X, Value = ElementInfo.X.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.Y, Value = ElementInfo.Y.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsKeyboardFocusable, Value = uIAElement.IsKeyboardFocusable.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsEnabled, Value = uIAElement.IsEnabled.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsKeyboardFocusable, Value = uIAElement.IsKeyboardFocusable.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsPassword, Value = uIAElement.IsPassword.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsOffscreen, Value = uIAElement.IsOffscreen.ToString() });
            list.Add(new ControlProperty() { Name = ElementProperty.IsSelected, Value = uIAElement.IsSelected.ToString() });

            return list;       
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            ObservableList<ElementLocator> Locators = GetElementLocators(ElementInfo);
            foreach (var elementLocator in Locators)
            {
                elementLocator.Active = true;
                elementLocator.IsAutoLearned = true;
            }
            return Locators;
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
            if (ElementInfo.ElementObject == null || locateElementByItLocators)
            {
                UIAuto.AutomationElement windowElement = LocateElementByLocators(ElementInfo.Locators, true);
                if (windowElement != null)
                {
                    ElementInfo.ElementObject = (object)windowElement;
                }
            }
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

        public Bitmap GetScreenShot(Tuple<int, int> setScreenSize = null, bool IsFullPageScreenshot = false)
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
            List<ElementInfo> list = mUIAutomationHelper.GetVisibleControls().Result;

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
        }

        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null)
        {
            try
            {
                mIsDriverBusy = true;
                List<ElementLocator> activesElementLocators = EI.Locators.Where(x => x.Active).ToList();

                LocateElementByLocators(EI.Locators, GetOutAfterFoundElement);

                if (activesElementLocators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                foreach (ElementLocator el in EI.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Pending).ToList())
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                }
                mIsDriverBusy = false;
            }
        }

        public UIAuto.AutomationElement LocateElementByLocators(ObservableList<ElementLocator> Locators, bool GetOutAfterFoundElement = false)
        {
            UIAuto.AutomationElement elem = null;
            foreach (ElementLocator locator in Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            foreach (ElementLocator locator in Locators.Where(x => x.Active).ToList())
            {
                if (!locator.IsAutoLearned)
                {
                    elem = LocateElementIfNotAutoLearned(locator);
                }
                else
                {
                    elem = LocateElementByLocator(locator);
                }

                if (elem != null)
                {
                    locator.StatusError = string.Empty;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    if (GetOutAfterFoundElement)
                    {
                        return elem;
                    }
                }
                else
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                }
            }

            return elem;
        }

        private UIAuto.AutomationElement LocateElementByLocator(ElementLocator locator, bool AlwaysReturn = true)
        {
            locator.StatusError = "";
            locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            UIAuto.AutomationElement AE = null;
            try
            {
                object obj = mUIAutomationHelper.FindElementByLocator(locator.LocateBy, locator.LocateValue);
                AE = (UIAuto.AutomationElement)obj;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when LocateElementByLocator", ex);
                if (AlwaysReturn)
                {
                    AE = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return AE;
                }
                else
                    throw;
            }
            return AE;
        }
        private UIAuto.AutomationElement LocateElementIfNotAutoLearned(ElementLocator locator)
        {
            ElementLocator evaluatedLocator = locator.CreateInstance() as ElementLocator;
            ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);
            evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);

            object obj = mUIAutomationHelper.FindElementByLocator(evaluatedLocator.LocateBy, evaluatedLocator.LocateValue);
            UIAuto.AutomationElement AE = (UIAuto.AutomationElement)obj;
            return AE;
        }
        public override void ActionCompleted(Act act)
        {
            mUIAutomationHelper.taskFinished = true;
            mUIElementOperationsHelper.taskFinished = true;
            if (!String.IsNullOrEmpty(act.Error) && act.Error.StartsWith("Time out !"))
            {
                Thread.Sleep(1000);
            }
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            try
            {
                mIsDriverBusy = true;
                foreach (ElementInfo EI in originalList)
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Pending;
                }
                foreach (ElementInfo EI in originalList)
                {
                    try
                    {
                        UIAuto.AutomationElement elem = LocateElementByLocators(EI.Locators);
                        if (elem != null)
                        {
                            EI.ElementObject = elem;
                            EI.ElementStatus = ElementInfo.eElementStatus.Passed;
                        }
                        else
                        {
                            EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        }
                    }
                    catch (Exception ex)
                    {
                        EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                }
            }
            finally
            {
                mIsDriverBusy = false;
            }
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            //try by type and Xpath comparison
            ElementInfo OriginalElementInfo = originalElements.Where(x => (x.ElementTypeEnum == latestElement.ElementTypeEnum)
                                                                && (x.XPath == latestElement.XPath)
                                                                && (x.Path == latestElement.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(latestElement.Path)))
                                                                && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                    || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                        && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                        )
                                                                    )
                                                                ).FirstOrDefault();
            return OriginalElementInfo;
        }

        public void StartSpying()
        {
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            ObservableList<OptionalValue> optionalValues = new ObservableList<OptionalValue>();
            UIAuto.AutomationElement automationElement = (UIAuto.AutomationElement)ElementInfo.ElementObject;

            //get child elements expand if combobox
            object expandPattern;
            automationElement.TryGetCurrentPattern(UIAuto.ExpandCollapsePattern.Pattern, out expandPattern);
            if (expandPattern != null && automationElement.Current.IsEnabled)
            {
                //try catch to handle if the screen is not focused
                try
                {
                    ((UIAuto.ExpandCollapsePattern)expandPattern).Expand();
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed To Expand Element while POM Learning...", ex);
                }
            }

            UIAuto.AutomationElementCollection itemList = automationElement.FindAll(Interop.UIAutomationClient.TreeScope.TreeScope_Descendants,
                new UIAuto.PropertyCondition( UIAuto.AutomationElement.LocalizedControlTypeProperty, "list item"));
            foreach (UIAuto.AutomationElement ae in itemList)
            {
                optionalValues.Add(new OptionalValue { Value = ae.Current.Name, IsDefault = false });
            }
            
            return optionalValues;
        }
        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        public List<AppWindow> GetWindowAllFrames()
        {
            throw new NotImplementedException();
        }

        public async Task<ElementInfo> GetElementAtPoint(long ptX, long ptY)
        {
            object elem = mUIAutomationHelper.GetElementAtPoint(new System.Drawing.Point((int)ptX, (int)ptY));

            if (elem == null) return null;
            ElementInfo EI = null;

            if (elem.GetType().Equals(typeof(UIAuto.AutomationElement)))
            {
                EI = mUIAutomationHelper.GetElementInfoFor((UIAuto.AutomationElement)elem);
            }
            else
            {
                EI = mUIAutomationHelper.GetHTMLHelper().GetHtmlElementInfo((IHTMLElement)elem);
            }

            EI.WindowExplorer = this;

            return EI;
        }

        public bool IsRecordingSupported()
        {
            return false;
        }

        public bool IsPOMSupported()
        {
            return true;
        }

        public bool IsLiveSpySupported()
        {
            return true;
        }

        public bool IsWinowSelectionRequired()
        {
            return true;
        }

        public List<eTabView> SupportedViews()
        {
            return new List<eTabView>() { eTabView.Screenshot, eTabView.GridView, eTabView.TreeView };
        }

        public eTabView DefaultView()
        {
            return eTabView.TreeView;
        }

        public string SelectionWindowText()
        {
            return "Window:";
        }

        public Task<object> GetPageSourceDocument(bool ReloadHtmlDoc)
        {
            return null;
        }

        public string GetCurrentPageSourceString()
        {
            return null;
        }


        public string GetApplitoolServerURL()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl;
        }

        public string GetApplitoolKey()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
        }

        public ePlatformType GetPlatform()
        {
            return this.Platform;
        }

        public string GetEnvironment()
        {
            return this.BusinessFlow.Environment;
        }

        public IWebDriver GetWebDriver()
        {
            return Driver;
        }

        public Bitmap GetElementScreenshot(Act act)
        {
            throw new NotImplementedException();
        }

        public string GetAgentAppName()
        {
            return this.Platform.ToString();
        }

        public string GetViewport()
        {
            Size size = new Size();
            size.Height = (int)((UIAuto.AutomationElement)mUIAutomationHelper.GetCurrentWindow()).Current.BoundingRectangle.Height;
            size.Width = (int)((UIAuto.AutomationElement)mUIAutomationHelper.GetCurrentWindow()).Current.BoundingRectangle.Width;
            return size.ToString();
        }

        public ObservableList<ElementLocator> GetElement
            (ElementInfo ElementInfo)
        {
            throw new NotImplementedException();
        }

        public ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            throw new NotImplementedException();
        }
    }
}
