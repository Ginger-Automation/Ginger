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
using FlaUI.Core;
using FlaUI.UIA3;
using FlaUI.Core.AutomationElements.Infrastructure;
using GingerCore.Actions.UIAutomation;
using System.Windows;
using System.Drawing;
using GingerCore.Drivers.Common;
using GingerCore.Actions;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3.Patterns;
using FlaUI.Core.Identifiers;
using FlaUI.Core.Patterns;
using System.Diagnostics;
using FlaUI.UIA3.Identifiers;
using GingerCore.Drivers.PBDriver;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers.WindowsLib
{
    public class FlaUIHelper : UIAutomationHelperBase,IXPath
    {
        private UIA3Automation automation = null;
        public AutomationElement CurrentWindow = null;
        public AutomationElement CurrentItem = null;
        UIAComWrapperHelper UIA = new UIAComWrapperHelper();
        
        #region Pattern Initialization
        UIA3AutomationElementProperties val = new UIA3AutomationElementProperties();
        SelectionItemPatternProperties selItmProp = new SelectionItemPatternProperties();
        SelectionPatternProperties selectnPattrn = new SelectionPatternProperties();
        #endregion


        private XPathHelper mXPathHelper = null;
        List<string> ImportentProperties = new List<string>();
        
        public FlaUIHelper()
        {
            automation = new UIA3Automation();
            InitXpathHelper();
        }
        
        private void InitXpathHelper()
        {
            mXPathHelper = new XPathHelper(this, ImportentProperties);
            ImportentProperties.Add("Name");
            ImportentProperties.Add("AutomationId");
            ImportentProperties.Add("LocalizedControlType");
            mXPathHelper = new XPathHelper(this, ImportentProperties);
        }

        public override string SetControlValue(object obj, string value)
        {
            AutomationElement element = (AutomationElement) obj;
            //Check if control is enabled 
            if (!element.Properties.IsEnabled)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Properties.AutomationId.ToString()
                    + " is not enabled.\n\n");
            }

            object vp;

            string controlType = element.Properties.LocalizedControlType;

            switch (controlType)
            {
                case "edit":
                case "Edit Box":   // Windows     
                    AutomationElementConversionExtensions.AsTextBox(element).Text=value;
                    break;
                case "list":
                case "list item":
                    bool isMultiSelect;
                    AutomationElement parentElement = element.Automation.TreeWalkerFactory.GetContentViewWalker().GetParent(element);
                    try
                    {
                        isMultiSelect = (bool)parentElement.BasicAutomationElement.GetPropertyValue(selectnPattrn.CanSelectMultiple);
                    }
                    catch(Exception e)
                    {
                        isMultiSelect = false;
                        Console.WriteLine(e.StackTrace);
                    }
                    if (isMultiSelect)
                    {
                        String IsItemSelected = (element.BasicAutomationElement.GetPropertyValue(selItmProp.IsSelected)).ToString();
                        if (IsItemSelected == "False")
                        {
                            element.BasicAutomationElement.TryGetNativePattern(SelectionItemPattern.Pattern, out vp);
                            ((ISelectionItemPattern)vp).AddToSelection();
                        }
                        else
                        {
                            element.BasicAutomationElement.TryGetNativePattern(SelectionItemPattern.Pattern, out vp);
                            ((ISelectionItemPattern)vp).RemoveFromSelection();
                        }
                    }
                    else
                    {
                        element.BasicAutomationElement.TryGetNativePattern(SelectionItemPattern.Pattern, out vp);
                        ((ISelectionItemPattern)vp).Select();
                    }

                    break;

                default:                    
                    Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                    break;

            }
            return "true";
        }

        public override object FindWindowByLocator(eLocateBy eLocatorType, string LocateValue)
        {
            throw new NotImplementedException();
        }

        public override object FindElementByLocator(eLocateBy eLocatorType, string LocateValue)
        {
            AutomationElement CurAE = null;
            switch (eLocatorType)
            {
                case eLocateBy.ByName:
                    ConditionBase CurCond2 = new PropertyCondition(AutomationObjectIds.NameProperty, LocateValue);
                    CurAE = this.CurrentWindow.FindFirst(TreeScope.Subtree, CurCond2);

                    //For old compatibility where Name was the text we fail over to search by Text, PB Only, it is slower as it scan the tree and call win api to get the text
                    if (Object.ReferenceEquals(CurAE, null) && mPlatform == ePlatform.PowerBuilder)
                    {
                    }
                    break;

                case eLocateBy.ByXPath:
                    UIAElementInfo e = (UIAElementInfo)mXPathHelper.GetElementByXpath(LocateValue);
                    CurAE = (AutomationElement)e.ElementObject;
                    break;

                case eLocateBy.ByAutomationID:
                    ConditionBase CurCond3 = new PropertyCondition(val.AutomationId, LocateValue);
                    CurAE = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond3);
                    break;

                case eLocateBy.ByClassName:
                    ConditionBase CurCond4 = new PropertyCondition(val.ClassName, LocateValue);
                    CurAE = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond4);
                    break;

                case eLocateBy.ByXY:
                    double xx = 0, yy = 0;
                    bool statusFlag;
                    string[] str = LocateValue.Split(',');
                    xx = Convert.ToDouble(str[0]);
                    yy = Convert.ToDouble(str[1]);
                    xx = xx + Convert.ToInt32(CurrentWindow.Properties.BoundingRectangle.Value.X);
                    yy = yy + Convert.ToInt32(CurrentWindow.Properties.BoundingRectangle.Value.Y);
                    System.Windows.Point point = new System.Windows.Point(xx, yy);
                    statusFlag = WinAPIAutomation.SetForeGroundWindow(CurrentWindow.Properties.ProcessId);
                    if (statusFlag == false)
                    {
                    }
                    CurAE = CurrentWindow.Automation.FromPoint(point);
                    break;
                default:
                    throw new Exception("Locator not implement - " + eLocatorType.ToString());
            }
            return CurAE;
        }

        public override void StopRecording()
        {
        }
        
        public override object GetCurrentWindow()
        {
            return CurrentWindow;
        }

        public override bool IsWindowValid(object window)
        {
            return true;
        }

        public override bool HandleSwitchWindow(eLocateBy locateBy, string locateValue)
        {
            if (locateBy == eLocateBy.ByXPath)
            {
                object AE = FindElementByLocator(locateBy, locateValue);
                CurrentWindow = (AutomationElement)AE;
                UpdateRootElement();
                return true;
            }
            return SwitchToWindow(locateValue);
        }

        public override bool SwitchToWindow(string title)
        {
            ConditionBase cb=new PropertyCondition(AutomationObjectIds.NameProperty,title);
            //TODO: Handle when there more than 1 window with same name
            AutomationElement window = automation.GetDesktop().FindFirstChild(cb);

            if (window != null)
            {
                CurrentWindow = window;
                UpdateRootElement();
                return true;
            }

            return false;
        }

        public override bool IsWindowExist(Act act)
        {
            throw new NotImplementedException();
        }

        public override bool SetWindowVisualState(ActWindow act)
        {
            throw new NotImplementedException();
        }

        public override string SetElementVisualState(object AE,string state)
        {
            throw new NotImplementedException();
        }

        public override string SetElementSize(object Ae, string size)
        {
            throw new NotImplementedException();
        }

        public override bool CloseWindow(Act act)
        {
            throw new NotImplementedException();
        }

        public override void SendKeysToControl(object element, string value)
        {
            throw new NotImplementedException();
        }

        public override string GetControlText(object element,string XY="")
        {
            throw new NotImplementedException();
        }

        public override string GetControlValue(object element)
        {
            throw new NotImplementedException();
        }

        public override string ClickElement(object element, bool asyncFlag = false)
        {
            throw new NotImplementedException();
        }

        public override void ClickOnXYPoint(object element, string value)
        {
            throw new NotImplementedException();
        }

        public override void DoRightClick(object element, string XY="")
        {
            throw new NotImplementedException();
        }

        public override void DoDoubleClick(object element, string XY = "")
        {
            throw new NotImplementedException();
        }        

        public override string GetSelectedItem(object element)
        {
            throw new NotImplementedException();
        }

        public override string GetDialogTitle(object element)
        {
            throw new NotImplementedException();
        }

        public override string ToggleControlValue(object element)
        {
            throw new NotImplementedException();
        }

        public override string IsEnabledControl(object element)
        {
            throw new NotImplementedException();
        }

        public override bool IsElementExist(eLocateBy eLocatorType, string LocateValue)
        {
            throw new NotImplementedException();
        }

        public override bool IsChildElementExist(eLocateBy eLocatorType, string LocateValue,string ValueForDriver)
        {
            throw new NotImplementedException();
        }
        public override void ScrollDown(object element)
        {
            throw new NotImplementedException();
        }

        public override void ScrollUp(object element)
        {
            throw new NotImplementedException();
        }

        public override string IsControlSelected(object element)
        {
            throw new NotImplementedException();
        }
        
        public override ElementInfo GetElementInfoFor(object element)
        {
            AutomationElement AE = (AutomationElement)element;
            UIAElementInfo EI = new UIAElementInfo();
            EI.ElementObject = AE;
            EI.WindowExplorer = WindowExplorer;
            return EI;
        }

        public override void ExpandControlElement(object element)
        {
            throw new NotImplementedException();
        }

        public override void CollapseControlElement(object element)
        {
            throw new NotImplementedException();
        }

        public override void ClickMenuElement(Act act)
        {
            throw new NotImplementedException();
        }

        public override object GetElementFromCursor()
        {
            throw new NotImplementedException();
        }

        public override List<ElementInfo> GetVisibleControls()
        {
            throw new NotImplementedException();
        }
        
        public override object GetElementData(object element)
        {
            throw new NotImplementedException();
        }

        public override ObservableList<ElementInfo> GetElements(ElementLocator EL)
        {
            throw new NotImplementedException();
        }

        public override string GetWindowInfo(object window)
        {
            return GetControlPropertyValue(window, "Name");
        }

        public  override void TakeScreenShot(ActScreenShot act)
        {
            throw new NotImplementedException();
        }
        
        public override List<object> GetListOfWindows()
        {
            List<object> list = new List<object>();
            AutomationElement[] apps = automation.GetDesktop().FindAllChildren();

            if (mPlatform == ePlatform.Windows)
            {
                foreach (AutomationElement AE in apps)
                {
                    if (AE.Properties.ClassName != null && !AE.Properties.ClassName.ValueOrDefault.StartsWith("FNW"))
                    {
                        list.Add(AE);
                    }
                }
            }
            else
            {
                foreach (AutomationElement AE in apps)
                {
                    if (AE.Properties.ClassName != null && AE.Properties.ClassName.ValueOrDefault.StartsWith("FNW"))
                    {
                        list.Add(AE);
                    }
                }
            }
            return list;
        }

        public override List<AppWindow> GetListOfDriverAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();
            List<object> AppWindows = GetListOfWindows();

            if (mPlatform == ePlatform.Windows)
            {
                foreach (AutomationElement window in AppWindows)
                {
                    string WindowTitle = GetWindowInfo(window);
                    list.Add(GetAppWinodowForElement(window, WindowTitle, AppWindow.eWindowType.Windows));
                }
            }
            else
            {
                foreach (AutomationElement window in AppWindows)
                {
                    string WindowTitle = GetWindowInfo(window);
                    if (String.IsNullOrEmpty(WindowTitle))
                    {
                        WindowTitle = "NoTitleWindow";
                    }
                    if (!String.IsNullOrEmpty(WindowTitle))
                    {

                        list.Add(GetAppWinodowForElement(window, WindowTitle, AppWindow.eWindowType.PowerBuilder));
                    }
                }
            }

            return list;
        }
        
        public override Bitmap GetCurrentWindowBitmap()
        {
            return CurrentWindow.Capture();
        }

        public override void SmartSyncHandler(ActSmartSync act)
        {
            throw new NotImplementedException();
        }

        public override HTMLHelper GetHTMLHelper()
        {
            throw new NotImplementedException();
        }

        public override string GetControlFieldValue(object element, string value)
        {
            throw new NotImplementedException();
        }

        public override string GetControlPropertyValue(object obj, string propertyName)
        {
            AutomationElement element = (AutomationElement) obj;
            if (propertyName == "XOffset")
            {
                try
                {
                    Rectangle r = element.Properties.BoundingRectangle.ValueOrDefault;
                    if (r != null)
                        return element.Properties.BoundingRectangle.ValueOrDefault.X.ToString();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return "1";
            }
            if (propertyName == "YOffset")
            {
                try
                {
                    Rectangle r = element.Properties.BoundingRectangle.ValueOrDefault;
                    if (r != null)
                        return element.Properties.BoundingRectangle.ValueOrDefault.Y.ToString();
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                }
                return "1";
            }

            PropertyId propertyId= GetPropertyIDByName(propertyName);

            if (propertyId==null)
            {
                return null;
            }
            String value = String.Empty;
            element.BasicAutomationElement.TryGetPropertyValue(propertyId, out value);

            if (value == null)
            {
                value = String.Empty;
            }
            return value;
        }

        public override Bitmap GetAppWindowAsBitmap(AppWindow aw)
        {
            AutomationElement tempWindow = (AutomationElement)((UIAElementInfo)aw.RefObject).ElementObject;
            return tempWindow.Capture();
        }

        public override List<Bitmap> GetAppDialogAsBitmap(AppWindow aw)
        {
            throw new NotImplementedException();
        }

        public override void SwitchWindow(ActUIASwitchWindow act)
        {
            throw new NotImplementedException();
        }

        public override List<ElementInfo> GetElementChilderns(object obj)
        {
            List<ElementInfo> list = new List<ElementInfo>();
            AutomationElement element = (AutomationElement)obj;

            AutomationElement [] elementList=element.FindAllChildren();

            foreach (AutomationElement elementNode in elementList)
            {
                UIAElementInfo WEI = (UIAElementInfo)GetElementInfoFor(elementNode);
                WEI.WindowExplorer = WindowExplorer;
                list.Add(WEI);
            }
            return list;
        }

        public override string InitializeBrowser(object element)
        {
            throw new NotImplementedException();
        }

        public override void HandleGridControlAction(ActTableElement act)
        {
            throw new NotImplementedException();
        }

        public override void SmartSwitchWindow(ActSwitchWindow actSW)
        {
            Stopwatch St = new Stopwatch();
            St.Reset();
            St.Start();

            Boolean switchDoneFlag = false;
            String windowTitle = actSW.LocateValueCalculated;
            while (!switchDoneFlag)
            {

                switchDoneFlag = SwitchToWindow(windowTitle);

                if (St.ElapsedMilliseconds > actSW.WaitTime * 1000)
                {
                    break;
                }
            }

            if (!switchDoneFlag)
                actSW.Error += "Window with title-" + windowTitle + " not found within specified time";
        }

        public XPathHelper GetXPathHelper(ElementInfo ei=null)
        {
            return mXPathHelper;
        }

        public ElementInfo GetRootElement()
        {
            return CurrentWindowRootElement;
        }

        public ElementInfo UseRootElement()
        {
            UIAElementInfo root = new UIAElementInfo();
            root.ElementObject = automation.GetDesktop();
            return root;
        }

        public ElementInfo GetElementParent(ElementInfo ElementInfo)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;


            if (EI.ElementObject.Equals(CurrentWindow)) return null;

            ITreeWalker walker = ((AutomationElement) EI.ElementObject).Automation.TreeWalkerFactory.GetControlViewWalker();

            AutomationElement ParentAE = walker.GetParent((AutomationElement)EI.ElementObject);

            if (ParentAE.Equals(CurrentWindow)) return null; // CurrentWindowRootElement;  // Since there are check done on root element we must return the same when found

            if (ParentAE == null) return null;

            UIAElementInfo RC = new UIAElementInfo() { ElementObject = ParentAE };    // return minimial EI
            return RC;
        }

        public string GetElementProperty(ElementInfo ElementInfo, string PropertyName)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with AutomationElement = null");
            }
            //FIXME
            AutomationElement element = (AutomationElement) EI.ElementObject;
            if (PropertyName.ToUpper() == "XPATH") { return GetElementAbsoluteXPath(element); }
            if (PropertyName.ToUpper() == "VALUE") { return GetControlValue(element); }

            return GetControlPropertyValue(element, PropertyName);
        }

        public List<ElementInfo> GetElementChildren(ElementInfo ElementInfo)
        {
            throw new NotImplementedException();
        }

        public ElementInfo FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;

            //FIXME - calc all props
            PropertyId AP = GetPropertyIDByName(conditions[0].PropertyName);
            ConditionBase cond = new PropertyCondition(AP, conditions[0].Value);

            AutomationElement AE = ((AutomationElement)EI.ElementObject).FindFirst(TreeScope.Children, cond);
            if (AE == null) return null;

            UIAElementInfo RC = (UIAElementInfo)GetElementInfoFor(AE);
            return RC;
        }

        public List<ElementInfo> FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;

            //FIXME - calc all conditions, meanwhile do the first
            PropertyId AP = GetPropertyIDByName(conditions[0].PropertyName);
            ConditionBase cond = new PropertyCondition(AP, conditions[0].Value);
            //---

            AutomationElement [] list=  ((AutomationElement)EI.ElementObject).FindAll(TreeScope.Children, cond);

            if (list.Length == 0) return null;

            List<ElementInfo> rc = new List<ElementInfo>();

            for (int i = 0; i < list.Length; i++)
            {
                UIAElementInfo EI1 = new UIAElementInfo() { ElementObject = list[i] };
                rc.Add(EI1);
            }

            return rc;
        }
        public string GetElementID(ElementInfo EI)
        {
            return "";
        }

        public string GetElementTagName(ElementInfo EI)
        {
            return "";
        }

        public List<object> GetAllElementsByLocator(eLocateBy LocatorType, string LocValue)
        {
            return null;
        }
        

        public ElementInfo GetPreviousSibling(ElementInfo EI)
        {
            ITreeWalker walker = ((AutomationElement)EI.ElementObject).Automation.TreeWalkerFactory.GetRawViewWalker();
            AutomationElement elementNode = walker.GetPreviousSibling((AutomationElement)EI.ElementObject);
            if (elementNode == null) return null;
            UIAElementInfo RC = new UIAElementInfo() { ElementObject = elementNode };
            return RC;
        }

        public ElementInfo GetNextSibling(ElementInfo EI)
        {
            ITreeWalker walker = ((AutomationElement)EI.ElementObject).Automation.TreeWalkerFactory.GetRawViewWalker();
            AutomationElement elementNode = walker.GetNextSibling((AutomationElement)EI.ElementObject);
            if (elementNode == null) return null;
            UIAElementInfo RC = new UIAElementInfo() { ElementObject = elementNode };
            return RC;
        }

        public override string GetElementAbsoluteXPath(object obj)
        {
            UIAElementInfo EI = new UIAElementInfo(); //Create small simple EI
            EI.ElementObject = obj;
            EI.WindowExplorer = WindowExplorer;
            string XPath = mXPathHelper.GetElementXpathAbsulote(EI);
            return XPath;

        }

        PropertyId GetPropertyIDByName(string prop)
        {
            if (prop == "Name") return AutomationObjectIds.NameProperty;
            if (prop == "AutomationId") return AutomationObjectIds.AutomationIdProperty;
            if (prop == "LocalizedControlType") return AutomationObjectIds.LocalizedControlTypeProperty;
            if (prop == "Value") return ValuePattern.ValueProperty;
            if (prop == "LegacyValue") return LegacyIAccessiblePattern.ValueProperty;
            if (prop == "ClassName") return AutomationObjectIds.ClassNameProperty;
            if (prop == "ToggleState") return TogglePattern.ToggleStateProperty;
            if (prop == "IsSelected") return SelectionItemPattern.IsSelectedProperty;
            if (prop == "Text") return ValuePattern.ValueProperty;
            //TODO: add all, and if not found lookup all properties list of AE
            throw new Exception("prop not found - " + prop);
        }

        public override string GetElementControlType(object obj)
        {
            AutomationElement element = (AutomationElement) obj;
            return element.Properties.LocalizedControlType.ValueOrDefault;
        }

        public override Rect GetElementBoundingRectangle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            return element.Properties.BoundingRectangle.ValueOrDefault;
        }

        public override int GetElementNativeWindowHandle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            return element.Properties.NativeWindowHandle.ValueOrDefault.ToInt32();
        }

        public override string GetElementTitle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            string Name = WinAPIAutomation.GetText(element.Properties.NativeWindowHandle.ValueOrDefault);
            if (string.IsNullOrEmpty(Name))
            {
                Name = element.Properties.Name.ValueOrDefault;
                if (string.IsNullOrEmpty(Name))
                {
                    Name = element.Properties.LocalizedControlType.ValueOrDefault;
                    if (!string.IsNullOrEmpty(element.Properties.AutomationId.ValueOrDefault))
                    {
                        Name += " (" + element.Properties.AutomationId.ValueOrDefault + ")";
                    }
                }
            }

            return Name;
        }

        public override bool HasAtleastOneChild(object obj)
        {
            AutomationElement element = (AutomationElement) obj;
            ITreeWalker walker = element.Automation.TreeWalkerFactory.GetControlViewWalker();
            if (walker.GetFirstChild(element) != null)
                return true;
            return false;
        }

        public override ObservableList<ControlProperty> GetElementProperties(object obj)
        {
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
            AutomationElement AE = (AutomationElement) obj;
            foreach (PropertyId AP in AE.BasicAutomationElement.GetSupportedProperties())
            {
                ControlProperty CP = new ControlProperty();
                CP.Name = AP.Name;
                CP.Name = CP.Name.Replace("AutomationElementIdentifiers.", "");
                CP.Name = CP.Name.Replace("Property", "");
                object propValue = null;
                AE.BasicAutomationElement.TryGetPropertyValue(AP, out propValue);
                // If Property Value is not null then only add it to the list. 
                if (propValue != null)
                {
                    CP.Value = propValue.ToString();
                    list.Add(CP);
                }
            }
            return list;
        }

        public override void HandlePaintWindow(object element)
        {
            PaintWindow(((AutomationElement)element).Properties.NativeWindowHandle);
        }

        public override object[] GetSupportedPatterns(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            return element.Automation.PatternLibrary.AllForCurrentFramework;
        }

        public override string GetPatternName(object pattern)
        {
            PatternId AP = (PatternId)pattern;
            return AP.Name;
        }


        public override void TestPattern(object objElement, object objPattern)
        {
            //TODO: Implement Test pattern for Fla UI
        }

        public override bool ClickContextMenuItem(object element, string value)
        {
            throw new NotImplementedException();
        }

        public override void DragAndDrop(object element, ActUIElement action)
        {
            throw new NotImplementedException();
        }
        public override void SelectControlByIndex(object element, string value)
        {
            throw new NotImplementedException();
        }

        public override string ClickAndValidteHandler(object element, ActUIElement action)
        {
            throw new NotImplementedException();
        }

        public override string SendKeysAndValidateHandler(object element, ActUIElement action)
        {
            throw new NotImplementedException();
        }

        public override string SelectAndValidateHandler(object element, ActUIElement action)
        {
            throw new NotImplementedException();
        }

        public string GetElementXpath(ElementInfo EI)
        {
            return null;
        }
    }
}
