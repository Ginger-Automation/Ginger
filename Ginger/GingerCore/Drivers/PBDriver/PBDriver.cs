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
using System;
using System.Collections.Generic;
using GingerCore.Actions;
using System.Windows.Automation;
using System.Drawing;
using System.Windows.Forms;
using GingerCore.Drivers.Common;
using GingerCore.Actions.UIAutomation;
using mshtml;
using GingerCore.Drivers.WindowsLib;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Threading;
using Amdocs.Ginger.Repository;

namespace GingerCore.Drivers.PBDriver
{
    //This class is for Power Builder UIAutomation
    public class PBDriver : UIAutomationDriverBase, IWindowExplorer
    {
        Dictionary<AutomationElement, AutomationElement[,]> gridDictionary;

        int mActionTimeout = 60;

        [UserConfigured]
        [UserConfiguredDefault("60")]  // Local host 
        [UserConfiguredDescription("Action Timeout - default is 60 seconds")]
        public override int ActionTimeout
        {
            get
            {
                return mActionTimeout;
            }
            set
            { mActionTimeout = value; }
        }
        
        //Check why it is needed?
        public PBDriver()
        {
        }

        public PBDriver(BusinessFlow BF, eUIALibraryType type = eUIALibraryType.ComWrapper)
        {
            BusinessFlow = BF;
            LibraryType = type;
            gridDictionary = new Dictionary<AutomationElement, AutomationElement[,]>();
        }

        public override void StartDriver()
        {
            switch (LibraryType)
            {

                case eUIALibraryType.ComWrapper:
                    mUIAutomationHelper = new UIAComWrapperHelper();
                    ((UIAComWrapperHelper)mUIAutomationHelper).WindowExplorer = this;
                    ((UIAComWrapperHelper)mUIAutomationHelper).BusinessFlow = BusinessFlow;
                    ((UIAComWrapperHelper)mUIAutomationHelper).mPlatform = UIAComWrapperHelper.ePlatform.PowerBuilder;
                    break;
                                   
            }
        }

        public override void UpdateContext(Context context)
        {
            base.UpdateContext(context);
            mUIAutomationHelper.BusinessFlow = context.BusinessFlow;
        }

        public override void RunAction(Act act)
        {
            //TODO: add func to Act + Enum for switch
            string actClass = act.GetType().ToString();
            mUIAutomationHelper.mLoadTimeOut = mActionTimeout;
            mUIAutomationHelper.taskFinished = false;
            //TODO: avoid hard coded string... 
            actClass = actClass.Replace("GingerCore.Actions.", "");
            if (!actClass.Equals(typeof(ActUIASwitchWindow)) && !actClass.Equals(typeof(ActSwitchWindow)) && (actClass == "Common.ActUIElement" && (((ActUIElement)act).ElementAction != ActUIElement.eElementAction.Switch)))
            {
                CheckRetrySwitchWindowIsNeeded();
            }
            if(act.Timeout!=null)
            {
                mUIAutomationHelper.mLoadTimeOut = act.Timeout;
            }
            
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Start Executing action of type '" + actClass + "' Description is" + act.Description);
                 
                switch (actClass)
                {
                    case "ActPBControl":
                        ActPBControl actPBC = (ActPBControl)act;
                        HandlePBControlAction(actPBC);
                        break;

                    case "ActGenElement":
                        ActGenElement AGE = (ActGenElement)act;
                        HandlePBGenericWidgetControlAction(AGE);
                        break;
                    case "ActBrowserElement":
                        ActBrowserElement actBE=(ActBrowserElement)act;
                        HandlePBBrowserElementAction(actBE);
                        break;
                    //TODO: ActSwitchWindow is the correct approach for switch window. And we should guide the users accordingly.
                    //Down the line we need to clean up ActUIASwitchWindow. This is old way, but we keep it to support old actions
                    case "ActUIASwitchWindow":
                        mUIAutomationHelper.SwitchWindow((ActUIASwitchWindow)act);
                        break;

                    case "ActSwitchWindow":
                        mUIAutomationHelper.SmartSwitchWindow((ActSwitchWindow)act);
                        break;

                    case "ActMenuItem":
                        ActMenuItem actMenuItem = (ActMenuItem)act;
                        HandleMenuControlAction(actMenuItem);
                        break;

                    case "ActWindow":
                        ActWindow actWindow = (ActWindow)act;
                        HandleWindowControlAction(actWindow);
                        break;
                    //case "ActUIAButton":
                    //    ActUIAButton b = (ActUIAButton)act;
                    //    UIA.ClickButton(b);
                    //    break;
                    //case "ActUIATextBox":
                    //    ActUIATextBox ATB = (ActUIATextBox)act;
                    //    if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.SetValue)
                    //    {
                    //        UIA.SetTextBoxValue(ATB);
                    //        return;
                    //    }
                    //    if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.GetValue) UIA.GetTextBoxValue(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.GetValue) UIA.GetTextBoxValue(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.IsDisabled) UIA.IsTextBoxDisabled(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.GetFont) UIA.GetTextBoxFont(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.IsPrepopulated) UIA.IsTextBoxPrepopulated(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.IsRequired) UIA.IsTextBoxRequired(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.IsDisplayed) UIA.IsTextBoxDisplayed(ATB);
                    //    //if (ATB.UIATextBoxAction == ActUIATextBox.eUIATextBoxAction.GetInputLength) UIA.GetTextBoxInputLength(ATB);
                    //    break;
                    //case "ActUIALabel":
                    //    if (((ActUIALabel)act).LabelAction == ActUIALabel.eLabelAction.GetForeColor) UIA.LabelGetColor((ActUIALabel)act);
                    //    break;

                    //TODO: What Jave is doing here? is it needed?
                    case "Java.ActTableElement":
                        ActTableElement actTable = (ActTableElement)act;
                        mUIAutomationHelper.HandleGridControlAction(actTable);
                        break;

                    case "ActTableElement":
                        ActTableElement actTable1 = (ActTableElement)act;
                        mUIAutomationHelper.HandleGridControlAction(actTable1);
                        break;

                    case "ActSmartSync":
                        mUIAutomationHelper.SmartSyncHandler((ActSmartSync)act);
                        break;

                    //case "ActUIAClickOnPoint":
                    //    ActUIAClickOnPoint ACP = (ActUIAClickOnPoint)act;
                    //    if (ACP.ActUIAClickOnPointAction == ActUIAClickOnPoint.eUIAClickOnPointAction.ClickXY) UIA.ClickOnPoint(ACP);
                    //    break;
                    case "ActScreenShot":                        
                        try
                        {
                            //TODO: Implement Multi window capture
                            if (act.WindowsToCapture == Act.eWindowsToCapture.AllAvailableWindows)
                            {
                                List<AppWindow> list = mUIAutomationHelper.GetListOfDriverAppWindows();
                                List<Bitmap> bList;
                                foreach (AppWindow aw in list)
                                {
                                    Bitmap bmp = mUIAutomationHelper.GetAppWindowAsBitmap(aw);
                                    act.AddScreenShot(bmp);
                                    bList = mUIAutomationHelper.GetAppDialogAsBitmap(aw);
                                    foreach (Bitmap tempbmp in bList)
                                    {
                                        act.AddScreenShot(tempbmp);
                                    }
                                    bList.Clear();
                                }
                            }
                            else
                            {
                                Bitmap bmp = mUIAutomationHelper.GetCurrentWindowBitmap();
                                act.AddScreenShot(bmp);
                            }
                            return;
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;
                    //case "ActUIAImage":
                    //    if (((ActUIAImage)act).ImageAction == ActUIAImage.eImageAction.IsVisible) UIA.ImageIsVisible((ActUIAImage)act);
                    //    break;
                    //case "ActPBControl":
                    //    ActPBControl APBC = (ActPBControl)act;
                    //    RunControlAction(APBC);
                    //    break;
                    //case "ActMenuItem":
                    //    ActMenuItem ami = (ActMenuItem)act;
                    //    MenuItem(ami);
                    //    break;
                    case "Common.ActUIElement":
                    //ActUIElement actUIPBC = (ActUIElement)act;
                    HandleUIElementAction(act);
                    break;

                    default:
                        throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());                        
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType()+ " Description:"+act.Description+" Error details:", e);
                CheckAndRetryRunAction(act,e);
                return;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                CheckAndRetryRunAction(act,e);
                return;
            }
            catch (ArgumentException e)
            {                
                Reporter.ToLog(eLogLevel.ERROR, "Exception at Run action:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                CheckAndRetryRunAction(act, e);
                return;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.WARN, "Exception at Run action",e);
                act.Error = e.Message;
            }
        }

        private void HandleUIElementAction(Act actPBC)
        {
            ActUIElement actUIPBC = (ActUIElement)actPBC;

            object AE = null;
            if (!actUIPBC.ElementType.Equals(eElementType.Window) && !actUIPBC.ElementAction.Equals(ActUIElement.eElementAction.Switch))
            {
                string locateValue = actUIPBC.ElementLocateValueForDriver;
                AE = mUIAutomationHelper.FindElementByLocator((eLocateBy)actUIPBC.ElementLocateBy, locateValue);

                if (AE == null && actUIPBC.ElementAction != ActUIElement.eElementAction.IsEnabled)
                {
                    actUIPBC.Error = "Element not Found - " + actUIPBC.ElementLocateBy + " " + actUIPBC.ElementLocateValueForDriver;
                    return;
                }
            }

            
            switch (actUIPBC.ElementAction)
            {
                case ActUIElement.eElementAction.DragDrop:
                    mUIAutomationHelper.DragAndDrop(AE, actUIPBC);
                    break;

                case ActUIElement.eElementAction.ClickAndValidate:
                    string status = mUIAutomationHelper.ClickAndValidteHandler(AE, actUIPBC);
                    if (!status.Contains("Clicked Successfully"))
                    {
                        actUIPBC.Error += status;
                    }
                    else
                    {
                        actUIPBC.ExInfo += status;
                        actUIPBC.AddOrUpdateReturnParamActual("Actual", status);
                    }
                    break;
                case ActUIElement.eElementAction.SendKeysAndValidate:
                    string status1 = mUIAutomationHelper.SendKeysAndValidateHandler(AE, actUIPBC);
                    if (!status1.Contains("true"))
                    {
                        actUIPBC.Error += status1;
                    }
                    else
                    {
                        actUIPBC.ExInfo += status1;
                        actUIPBC.AddOrUpdateReturnParamActual("Actual", status1);
                    }
                    break;
                case ActUIElement.eElementAction.SelectandValidate:
                    string statusSel = mUIAutomationHelper.SelectAndValidateHandler(AE, actUIPBC);
                    if (!statusSel.Contains("true"))
                    {
                        actUIPBC.Error += statusSel;
                    }
                    else
                    {
                        actUIPBC.ExInfo += statusSel;
                        actUIPBC.AddOrUpdateReturnParamActual("Actual", statusSel);
                    }
                    break;
                case ActUIElement.eElementAction.Switch:
                    mUIAutomationHelper.ActUISwitchWindow(actUIPBC);
                    break;
                default:
                    actUIPBC.Error = "Unable to perform operation";
                    break;
            }
        }
        
        private void HandlePBBrowserElementAction(ActBrowserElement actBE)
        {
            string result = "";

            switch (actBE.ControlAction)
            {
                case ActBrowserElement.eControlAction.InitializeBrowser:
                    object AE = mUIAutomationHelper.GetActElement(actBE);
                    if (AE == null)
                    {
                        actBE.Error = "Element not Found - " + actBE.LocateBy + " " + actBE.LocateValueCalculated;
                        return;
                    }
                    result = mUIAutomationHelper.InitializeBrowser(AE);
                    if (result.Equals("true"))
                    {
                        actBE.AddOrUpdateReturnParamActual("Actual", result);
                        actBE.ExInfo = "Browser Initialized";
                    }
                    else
                    {
                        actBE.Error = "Unable to initialize browser";
                    }
                    break;
                case ActBrowserElement.eControlAction.GetPageSource:
                    result = mUIAutomationHelper.GetHTMLHelper().GetPageInfo("PageSource");
                    if (!result.Equals(""))
                    {
                        actBE.AddOrUpdateReturnParamActual("Actual", result);
                        actBE.ExInfo = result;
                    }
                    else
                    {
                        actBE.Error = "Unable to fetch Page Source";
                    }
                    break;
                case ActBrowserElement.eControlAction.GetPageURL:
                    result = mUIAutomationHelper.GetHTMLHelper().GetPageInfo("PageURL");
                    if (!result.Equals(""))
                    {
                        actBE.AddOrUpdateReturnParamActual("Actual", result);
                        actBE.ExInfo = result;
                    }
                    else
                    {
                        actBE.Error = "Unable to get Page URL";
                    }
                    break;
                case ActBrowserElement.eControlAction.SwitchFrame:
                    result=mUIAutomationHelper.GetHTMLHelper().SwitchFrame(actBE.LocateBy, actBE.LocateValueCalculated);
                    if (result.Equals("true"))
                    {
                        actBE.AddOrUpdateReturnParamActual("Actual", result);
                        actBE.ExInfo = "Switched to frame";
                    }
                    else
                    {
                        actBE.Error = "Unable Switch frame";
                    }
                    break;
                case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                    mUIAutomationHelper.GetHTMLHelper().SwitchToDefaultFrame();
                    actBE.AddOrUpdateReturnParamActual("Actual", result);
                    actBE.ExInfo = "Switched to Default Frame";
                    break;
                default:
                    actBE.Error = "Unable to perform operation";
                    break;
            }
        }

        private void HandlePBControlAction(ActPBControl actPBC)
        {
            object AE = null;
            if (actPBC.ControlAction != ActPBControl.eControlAction.IsExist)
            {
                AE = mUIAutomationHelper.GetActElement(actPBC);
                if (AE == null)
                {
                    actPBC.Error = "Element not Found - " + actPBC.LocateBy + " " + actPBC.LocateValueCalculated;
                    return;
                }
            }
            try
            {
                switch (actPBC.ControlAction)
                {
                    case ActPBControl.eControlAction.SetValue:
                        try
                        {
                            string result = mUIAutomationHelper.SetControlValue(AE, actPBC.ValueForDriver);
                            if (result.StartsWith("True"))
                            {                                
                                actPBC.ExInfo = actPBC.ValueForDriver + " Value Set Successfully.";                                
                                actPBC.AddOrUpdateReturnParamActual("Actual", "Passed");
                            }
                            else
                            {
                                result = result.Replace("False","");
                                actPBC.Error = "Validation failed";
                                actPBC.ExInfo = "Expected Value : "+actPBC.ValueForDriver+" , Actual Value  :  "+result;
                            }
                        }
                        catch (Exception e)
                        {
                            actPBC.ExInfo = e.Message;
                        }                        
                        break;

                    case ActPBControl.eControlAction.GetValue:
                        string val = mUIAutomationHelper.GetControlValue(AE);
                        actPBC.AddOrUpdateReturnParamActual("Actual", val);
                        actPBC.ExInfo = val;                        
                        break;
                    case ActPBControl.eControlAction.GetText:
                        string valText = mUIAutomationHelper.GetControlText(AE, actPBC.ValueForDriver);
                        actPBC.AddOrUpdateReturnParamActual("Actual", valText);
                        actPBC.ExInfo = valText;
                        break;

                    case ActPBControl.eControlAction.GetFieldValue:
                        string fieldValue = mUIAutomationHelper.GetControlFieldValue(AE,actPBC.ValueForDriver);
                        actPBC.AddOrUpdateReturnParamActual("Actual", fieldValue);
                        actPBC.ExInfo = fieldValue;
                        break;

                    case ActPBControl.eControlAction.GetControlProperty:
                        string propValue = mUIAutomationHelper.GetControlPropertyValue(AE, actPBC.ValueForDriver);
                        actPBC.AddOrUpdateReturnParamActual("Actual", propValue);
                        actPBC.ExInfo = propValue;
                        break;

                    case ActPBControl.eControlAction.Select:
                        string validation= mUIAutomationHelper.SetControlValue(AE, actPBC.ValueForDriver);
                        if (validation.StartsWith("True"))
                        {
                            actPBC.ExInfo = actPBC.ValueForDriver + " Value Set Successfully.";
                            actPBC.AddOrUpdateReturnParamActual("Actual", "Passed");
                        }
                        else
                        {
                            validation = validation.Replace("False", "");
                            actPBC.Error = "Validation failed";
                            actPBC.ExInfo = "Expected Value : " + actPBC.ValueForDriver + " , Actual Value  :  " + validation;
                        }
                        break;

                    case ActPBControl.eControlAction.SelectByIndex:
                        mUIAutomationHelper.SelectControlByIndex(AE, actPBC.ValueForDriver);
                        break;

                    case ActPBControl.eControlAction.Click:
                        string status = mUIAutomationHelper.ClickElement(AE);
                        if (!status.Contains("Clicked Successfully"))
                        {
                            actPBC.Error = status;
                        }
                        else
                            actPBC.ExInfo += status;
                        break;

                    case ActPBControl.eControlAction.AsyncClick:
                        status = mUIAutomationHelper.ClickElement(AE,true);
                        if (!status.Contains("Clicked Successfully"))
                        {
                            actPBC.Error = status;
                        }
                        else
                            actPBC.ExInfo += status;
                        break;

                    case ActPBControl.eControlAction.ClickXY:
                        mUIAutomationHelper.ClickOnXYPoint(AE,actPBC.ValueForDriver);
                        break;

                    case ActPBControl.eControlAction.RightClick:
                        mUIAutomationHelper.DoRightClick(AE);
                        break;

                    case ActPBControl.eControlAction.DoubleClick:
                        mUIAutomationHelper.DoDoubleClick(AE,actPBC.Value);
                        break;

                    case ActPBControl.eControlAction.Maximize:
                    case ActPBControl.eControlAction.Minimize:
                    case ActPBControl.eControlAction.Restore:
                        status  = mUIAutomationHelper.SetElementVisualState(AE, actPBC.ControlAction.ToString());
                        if (!status.Contains("State set successfully"))
                        {
                            actPBC.Error = status;
                        }
                        else
                            actPBC.ExInfo += status;                        
                        break;
                    case ActPBControl.eControlAction.Resize:
                        status = mUIAutomationHelper.SetElementSize(AE, actPBC.ValueForDriver);
                        if (!status.Contains("Element resize Successfully"))
                        {
                            actPBC.Error = status;
                        }
                        else
                            actPBC.ExInfo += status;
                        break;
                    case ActPBControl.eControlAction.GetSelected:
                        string selectedItem= mUIAutomationHelper.GetSelectedItem(AE);
                        actPBC.AddOrUpdateReturnParamActual("Selected Item", selectedItem);
                        actPBC.ExInfo = selectedItem;
                        break;

                    case ActPBControl.eControlAction.GetTitle:
                        string title = mUIAutomationHelper.GetDialogTitle(AE);
                        actPBC.AddOrUpdateReturnParamActual("Dialog Title", title);
                        actPBC.ExInfo = title;
                        break;

                    case ActPBControl.eControlAction.Toggle:                        
                        string value = mUIAutomationHelper.ToggleControlValue(AE);
                        actPBC.AddOrUpdateReturnParamActual("Actual", value);
                        actPBC.ExInfo = value;
                        break;

                    case ActPBControl.eControlAction.IsEnabled:
                        string valueIsEnabled = mUIAutomationHelper.IsEnabledControl(AE);
                        actPBC.AddOrUpdateReturnParamActual("Actual", valueIsEnabled);
                        actPBC.ExInfo = valueIsEnabled;
                        break;

                    case ActPBControl.eControlAction.IsExist:
                        string valueIsExist;
                        if (!(String.IsNullOrEmpty(actPBC.ValueForDriver)))
                        {
                             valueIsExist = mUIAutomationHelper.IsChildElementExist(actPBC.LocateBy, actPBC.LocateValueCalculated,actPBC.ValueForDriver).ToString();
                        }
                        else
                        {
                             valueIsExist = mUIAutomationHelper.IsElementExist(actPBC.LocateBy, actPBC.LocateValueCalculated).ToString();
                        }
                        actPBC.Error = "";
                        actPBC.AddOrUpdateReturnParamActual("Actual", valueIsExist);
                        actPBC.ExInfo = valueIsExist;
                        break;

                    case ActPBControl.eControlAction.Scrolldown:
                        mUIAutomationHelper.ScrollDown(AE);
                        break;

                    case ActPBControl.eControlAction.ScrollUp:
                        mUIAutomationHelper.ScrollUp(AE);
                        break;

                    case ActPBControl.eControlAction.IsSelected:
                        string isSelected = mUIAutomationHelper.IsControlSelected(AE);
                        actPBC.AddOrUpdateReturnParamActual("Actual", isSelected);
                        actPBC.ExInfo = isSelected;
                        break;

                    case ActPBControl.eControlAction.Highlight:
                        mUIAutomationHelper.HiglightElement(mUIAutomationHelper.GetElementInfoFor(AE));
                        break;

                    case ActPBControl.eControlAction.Repaint:
                        mUIAutomationHelper.HandlePaintWindow(AE);
                        break;

                    case ActPBControl.eControlAction.SendKeys:
                        mUIAutomationHelper.SendKeysToControl(AE, actPBC.ValueForDriver);
                        actPBC.ExInfo = actPBC.ValueForDriver + " set";
                        break;

                    default:
                        actPBC.Error = "Unknown Action  - " + actPBC.ControlAction;
                        break;
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "COM Exception when HandlePBControlAction Error details:", e);
                throw e;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Element not available Exception when HandlePBControlAction Error details:", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Argument Exception when HandlePBControlAction Error details:", e);
                throw e;
            }
            catch (Exception e)
            {
               Reporter.ToLog(eLogLevel.ERROR, "Exception in HandlePBControlAction", e);
                throw e;
            }
        }
        
        private void HandlePBGenericWidgetControlAction(ActGenElement actPBC)
        {
            IHTMLElement PBEle = mUIAutomationHelper.GetHTMLHelper().GetActElement(actPBC);
            if (PBEle == null )
            {
                actPBC.Error = "Element not Found - " + actPBC.LocateBy + " " + actPBC.LocateValueCalculated;
                return;
            }
            string value = string.Empty;
            bool result = false;
            string ValDrv = actPBC.ValueForDriver;
            try
            {
                switch (actPBC.GenElementAction)
                {
                    case ActGenElement.eGenElementAction.SetValue:

                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(PBEle, ValDrv);
                        if (result)
                            actPBC.ExInfo = ValDrv + " set";
                        else
                            actPBC.Error = "Unable to set value to " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.SetAttributeValue:
                        string attrName = "value";
                        string attValue = ValDrv;
                        if (ValDrv.IndexOf("=") > 0)
                        {
                            attrName = ValDrv.Split('=')[0];
                            attValue = ValDrv.Split('=')[1];
                        }
                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(PBEle, attValue, attrName);
                        if (result)
                            actPBC.ExInfo = ValDrv + " set";
                        else
                            actPBC.Error = "Unable to set value to " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.SendKeys:

                        result = mUIAutomationHelper.GetHTMLHelper().SendKeys(PBEle, ValDrv);
                        if (result)
                            actPBC.ExInfo = ValDrv + " Keys Sent";
                        else
                            actPBC.Error = "Unable to Send Keys " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.GetValue:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(PBEle,"value");
                        if (string.IsNullOrEmpty(value))
                        {
                            actPBC.Error = "Unable to Get value of " + ValDrv;
                        }
                        else
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value.ToString());
                            actPBC.ExInfo = value.ToString();
                        }
                        break;                    
                    case ActGenElement.eGenElementAction.Click:
                    case ActGenElement.eGenElementAction.AsyncClick:
                        result = mUIAutomationHelper.GetHTMLHelper().Click(PBEle);
                        if (result) actPBC.ExInfo = "Element Clicked";
                        else actPBC.Error = "Element Unable to Clicked";
                        break;
                    case ActGenElement.eGenElementAction.ClickAt:
                        result = mUIAutomationHelper.GetHTMLHelper().ClickAt(PBEle, ValDrv);
                        if (result) actPBC.ExInfo = "Element Clicked";
                        else actPBC.Error = "Element Unable to Clicked";
                        break;
                    case ActGenElement.eGenElementAction.RightClick:
                        result = mUIAutomationHelper.GetHTMLHelper().RightClick(PBEle, ValDrv);
                        if (result) actPBC.ExInfo = "Element Right Click Done";
                        else actPBC.Error = "Element Unable to do Right Click";
                        break;
                    case ActGenElement.eGenElementAction.Enabled:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(PBEle,"disabled");
                        value = value.ToString().ToLower().Equals("false") ? "true" : "false";
                        actPBC.AddOrUpdateReturnParamActual("Actual", value);
                        actPBC.ExInfo = value.ToString();
                        break;
                    case ActGenElement.eGenElementAction.Hover:
                        result = mUIAutomationHelper.GetHTMLHelper().MouseHover(PBEle, ValDrv);
                        if (result) actPBC.ExInfo = "Element Hover Done";
                        else actPBC.Error = "Unable to Hover the Element";
                        break;
                    case ActGenElement.eGenElementAction.ScrollToElement:
                        result = mUIAutomationHelper.GetHTMLHelper().scrolltoElement(PBEle);
                        if (result) actPBC.ExInfo = "Scroll to Element Done";
                        else actPBC.Error = "Unable to Scroll to Element";
                        break;
                    case ActGenElement.eGenElementAction.FireSpecialEvent:

                        value = mUIAutomationHelper.GetHTMLHelper().FireSpecialEvent(PBEle, ValDrv);
                        if (value.StartsWith("Error"))
                            actPBC.Error = "Unable to fire special event. " + value;
                        else
                            actPBC.ExInfo = "Fire special event " + value;
                        break;
                    case ActGenElement.eGenElementAction.Visible:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(PBEle, "type");
                        value = value.Equals("hidden") ? "false" : "true";
                        actPBC.AddOrUpdateReturnParamActual("Actual", value);
                        actPBC.ExInfo = value.ToString();
                        break;
                    case ActGenElement.eGenElementAction.SelectFromDropDown:

                        value = mUIAutomationHelper.GetHTMLHelper().SelectFromDropDown(PBEle, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actPBC.Error = "Unable to select value-" + ValDrv + " from DropDown";
                        }
                        break;
                    case ActGenElement.eGenElementAction.SelectFromDropDownByIndex:

                        result = mUIAutomationHelper.GetHTMLHelper().SetValue(PBEle, ValDrv);
                        if (result) actPBC.ExInfo = ValDrv + " set";
                        else actPBC.Error = "Unable to Set Value " + ValDrv;
                        break;
                    case ActGenElement.eGenElementAction.GetInnerText:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(PBEle,"innerText");
                        if (!string.IsNullOrEmpty(value))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actPBC.Error = "Unable to get Inner Text";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetElementAttributeValue:
                        value = mUIAutomationHelper.GetHTMLHelper().GetValue(PBEle, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actPBC.Error = "Unable to get custom attribute";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetCustomAttribute:
                        value = mUIAutomationHelper.GetHTMLHelper().GetNodeAttributeValue(PBEle, ValDrv);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actPBC.Error = "Unable to get custom attribute";
                        }
                        break;
                    case ActGenElement.eGenElementAction.GetStyle:
                        value = mUIAutomationHelper.GetHTMLHelper().GetStyle(PBEle);
                        if (!string.IsNullOrEmpty(value))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = value.ToString();
                        }
                        else
                        {
                            actPBC.Error = "Unable to get Element Style";
                        }
                        break;
                    case ActGenElement.eGenElementAction.SwitchFrame:
                        value = mUIAutomationHelper.GetHTMLHelper().SwitchFrame(actPBC.LocateBy, actPBC.LocateValueCalculated);
                        if (value.Equals("true"))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = "Switched to frame";
                        }
                        else
                        {
                            actPBC.Error = "Unable Switch frame";
                        }
                        break;
                    case ActGenElement.eGenElementAction.HighLightElement:
                        value = mUIAutomationHelper.GetHTMLHelper().HighLightElement(PBEle);
                        if (value.Equals("true"))
                        {
                            actPBC.AddOrUpdateReturnParamActual("Actual", value);
                            actPBC.ExInfo = "Element Highlighted";
                        }
                        else
                        {
                            actPBC.Error = "Unable To Highlight Element";
                        }
                        break;

                    default:
                        actPBC.Error = "Unknown Action  - " + actPBC.GenElementAction;
                        break;
                }
            }
            catch (Exception e)
            {
                actPBC.Error = e.Message;
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
                        mUIAutomationHelper.ExpandControlElement(AE);
                        break;

                    case ActMenuItem.eMenuAction.Collapse:
                        AE = mUIAutomationHelper.GetActElement(actMenuItem);
                        mUIAutomationHelper.CollapseControlElement(AE);
                        break;

                    case ActMenuItem.eMenuAction.Click:
                        mUIAutomationHelper.ClickMenuElement(actMenuItem);
                        break;
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "COM Exception when HandleMenuControlAction Error details:", e);
                throw e;

            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Element not available Exception when HandleMenuControlAction Error details:", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Argument Exception when HandleMenuControlAction Error details:", e);
                throw e;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in HandleMenuControlAction", e);
                throw e;
            }
        }      

        private void HandleWindowControlAction(ActWindow actWindow)
        {
            try
            {
                switch (actWindow.WindowActionType)
                {
                    case ActWindow.eWindowActionType.IsExist:
                        string val=mUIAutomationHelper.IsWindowExist(actWindow).ToString();
                        actWindow.Error = "";
                        if (String.IsNullOrEmpty(actWindow.Error))
                        {
                            actWindow.AddOrUpdateReturnParamActual("Actual", val);
                            actWindow.ExInfo += val;
                        }                        
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
            catch (System.Runtime.InteropServices.COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "COM Exception when HandleWindowControlAction Error details:", e);
                throw e;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Element not available Exception when HandleWindowControlAction Error details:", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Argument Exception when HandleWindowControlAction Error details:", e);
                throw e;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in HandleWindowControlAction", e);
                throw e;
            }
        }

        public override void CloseDriver()
        {
        }

        public override Act GetCurrentElement()
        { return null; }

        
        public override string GetURL()
        { return null; }

        
        private void SwitchWindow(ActUIASwitchWindow act)
        {
            mUIAutomationHelper.SwitchWindow(act);
        }

        public override void HighlightActElement(Act act)
        {
        }

        public override ePlatformType Platform { get { return ePlatformType.PowerBuilder; } }

        public override bool IsRunning()
        {
            return true;
        }

        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            return mUIAutomationHelper.GetListOfDriverAppWindows();
        }

        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false)
        {
            return mUIAutomationHelper.GetVisibleControls();
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            mUIAutomationHelper.SwitchToWindow(Title);
        }


        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            HighLightElement(ElementInfo);
            
        }
        string IWindowExplorer.GetFocusedControl()
        {
            AutomationElement AE= AutomationElement.FocusedElement;
            string s = null;

            s = AE.Current.Name + " - " + AE.Current.ClassName + "-" + AE.Current.LocalizedControlType;
            return s;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            return GetControlFromMousePosition();           
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            return EI;
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
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;
            return mUIAutomationHelper.GetElementData(EI.ElementObject);
        }

        private AutomationElement ElementFromCursor()
        {
            // Convert mouse position from System.Drawing.Point to System.Windows.Point.
            System.Windows.Point point = new System.Windows.Point(Cursor.Position.X, Cursor.Position.Y);
            AutomationElement element = AutomationElement.FromPoint(point);
            return element;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            if (mUIAutomationHelper.GetCurrentWindow() == null) return null;

            if (!mUIAutomationHelper.IsWindowValid(mUIAutomationHelper.GetCurrentWindow())) return null;

            AppWindow aw = new AppWindow();
            aw.Title = mUIAutomationHelper.GetWindowInfo(mUIAutomationHelper.GetCurrentWindow());
            return aw;
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            return GetElementChildren(ElementInfo);
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            return mUIAutomationHelper.GetElements(EL);
            //TOOD: Handle for HTML Elements
            //return UIA.HTMLhelperObj.GetElements(EL);
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {
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
