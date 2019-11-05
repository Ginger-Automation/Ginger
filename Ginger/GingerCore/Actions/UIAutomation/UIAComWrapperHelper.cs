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
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using GingerCore.Drivers.Common;
using GingerCore.Actions.UIAutomation;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using GingerCore.Drivers.PBDriver;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

// a lot of samples from Microsoft on UIA at: https://uiautomationverify.svn.codeplex.com/svn/UIAVerify/
// DO NOT add any specific driver here, this is generic windows app driver helper 

namespace GingerCore.Drivers
{
    public class UIAComWrapperHelper : UIAutomationHelperBase, IXPath
    {
        Dictionary<AutomationElement, Dictionary<string, AutomationElement[]>> parentDictionary = new Dictionary<AutomationElement, Dictionary<string, AutomationElement[]>>();

        // the current App window we do all searches on
        public string CurrentWindowName { get; set; }
        public AutomationElement CurrentWindow { get; set; }      

        public ObservableList<Act> ElementLocators = new ObservableList<Act>();
        public ObservableList<Act> ParentLocators = new ObservableList<Act>();
        public System.Windows.Automation.Condition ParentCondition;
        public System.Windows.Automation.Condition ElementCondition;

        private int loadwaitSeconds;
        private WinAPIAutomation winAPI = new WinAPIAutomation();
          public Dictionary<string, AutomationElement[]> MainDict = new Dictionary<string, AutomationElement[]>();
        public HTMLHelper HTMLhelperObj{get;set;}

        List<string> ImportentProperties = new List<string>();

        private XPathHelper mXPathHelper = null;

        public UIAComWrapperHelper()
        {
            InitXpathHelper();
        }

        public UIAComWrapperHelper(BusinessFlow BF)
        {
            BusinessFlow = BF;

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
        
        private AutomationElement lastFocusedElement;
        
        /// <summary>
        /// Determines if element is a content element used for searching elements that content is presented to user (non peripheral elements)
        /// </summary>
        private static readonly PropertyCondition ConditionContent = new PropertyCondition(AutomationElement.IsContentElementProperty, true);

        #region RECORDING DRIVER
        //create Element cache
        private bool foundParent;

        private string sLastEvent = null;
        private Action<AutomationElement, DriverBase.ActionName> CreateActionEvent;

        // CreateActionHandler is event handler for call back, every time we need to create action bu the driver who use UIAutomation
        public void StartRecording(Action<AutomationElement, DriverBase.ActionName> CreateActionHandler)
        {
            CreateActionEvent = CreateActionHandler;

            // We create the first action to switch window
            CreateActionEvent(CurrentWindow, DriverBase.ActionName.SwitchWindow);

            AddEventHandlers();
        }

        //TODO: if AE window close do remove all ev handler

        private void AddEventHandlers()
        {
            Automation.RemoveAllEventHandlers();

            Automation.AddAutomationEventHandler(InvokePattern.InvokedEvent, CurrentWindow, TreeScope.Subtree, recInvokedEvent);
            Automation.AddAutomationEventHandler(SelectionItemPatternIdentifiers.ElementSelectedEvent, CurrentWindow, TreeScope.Subtree, recElementSelectedEvent);
            Automation.AddAutomationEventHandler(SelectionItemPatternIdentifiers.ElementAddedToSelectionEvent, CurrentWindow, TreeScope.Subtree, recElementAddedToSelectionEvent);
            Automation.AddAutomationEventHandler(SelectionItemPatternIdentifiers.ElementRemovedFromSelectionEvent, CurrentWindow, TreeScope.Subtree, recElementRemovedFromSelectionEvent);
            Automation.AddAutomationEventHandler(WindowPattern.WindowClosedEvent, CurrentWindow, TreeScope.Subtree, recWindowClosedEvent);
            Automation.AddAutomationEventHandler(WindowPattern.WindowOpenedEvent, CurrentWindow, TreeScope.Subtree, recWindowOpenedEvent);

            Automation.AddAutomationFocusChangedEventHandler(FocusChangedHandler);

            AutomationProperty[] propertyList = new[]{
              TogglePattern.ToggleStateProperty,
                 ExpandCollapsePattern.ExpandCollapseStateProperty                 
            };

            Automation.AddAutomationPropertyChangedEventHandler(CurrentWindow, TreeScope.Subtree, OnPropertyChange, propertyList);
        }

        public override void StopRecording()
        {
            Automation.RemoveAllEventHandlers();
        }

        #region RECORDING EVENTS
        private void recStructureChangedEvent(object sender, StructureChangedEventArgs e)
        {

            //TODO: Need to update this even handler

            //AddLog("recStructureChangedEvent : Captured Event but Not Implemented Yet");

            AutomationElement element = (AutomationElement)sender;
            // Check for Tab control click
            if (e.StructureChangeType == StructureChangeType.ChildAdded)
            {
                CreateActionEvent(element, DriverBase.ActionName.ClickTab);
                // AddTabClickAction(element);
                //Object windowPattern;
                //if (false == element.TryGetCurrentPattern(WindowPattern.Pattern, out windowPattern))
                //{
                //    return;
                //}
                //int[] rid = e.GetRuntimeId();
                //if (RuntimeIdListed(rid, savedRuntimeIds) < 0)
                //{
                //    AddToWindowHandler(element);
                //    savedRuntimeIds.Add(rid);
                //}
            }
        }
        
        private void recElementSelectedEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            if (sLastEvent == GetAEEventInfo(AE, e.EventId)) return;

            string controlType = AE.Current.LocalizedControlType;

            if (controlType.Equals("radio button"))
            {
                CreateActionEvent(AE, DriverBase.ActionName.SelectRadioButton);
            }

            else if (controlType.Equals("list item"))
            {
                CreateActionEvent(AE, DriverBase.ActionName.SelectListItem);
            }
            SetLastEvent(AE, e.EventId);
        }

        private void recElementAddedToSelectionEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            CreateActionEvent(AE, DriverBase.ActionName.AddToSelection);
            SetLastEvent(AE, e.EventId);
        }

        private void recElementRemovedFromSelectionEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            CreateActionEvent(AE, DriverBase.ActionName.RemoveFromSelection);
            SetLastEvent(AE, e.EventId);
        }

        private void recWindowClosedEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            CreateActionEvent(AE, DriverBase.ActionName.CloseWindow);
        }

        private void recWindowOpenedEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            if (sLastEvent == GetAEEventInfo(AE, e.EventId)) return;

            if (AE.Current.LocalizedControlType == "window")
            {
                CreateActionEvent(AE, DriverBase.ActionName.SwitchWindow);
            }

            SetLastEvent(AE, e.EventId);
        }

        private void recInvokedEvent(object sender, AutomationEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            if (sLastEvent == GetAEEventInfo(AE, e.EventId)) return;
            CreateActionEvent(AE, DriverBase.ActionName.Click);
            SetLastEvent(AE, e.EventId);
        }

        private void SetLastEvent(AutomationElement AE, AutomationEvent AEvent)
        {
            sLastEvent = GetAEEventInfo(AE, AEvent);
        }

        private string GetAEEventInfo(AutomationElement AE, AutomationEvent AEvent)
        {
            // Make sure we do not report duplicate events by combining all info below
            string s = AEvent.Id + "^" + AEvent.ProgrammaticName + "^" + AE.GetHashCode() + "^" + DateTime.Now.ToString();
            return s;
        }

        private void OnPropertyChange(object src, AutomationPropertyChangedEventArgs e)
        {
            AutomationElement AE = (AutomationElement)src;
            if (sLastEvent == GetAEEventInfo(AE, e.EventId)) return;

            string controlType = AE.Current.LocalizedControlType;
            switch (controlType)
            {
                case "check box":
                    if (e.Property.ProgrammaticName == "TogglePatternIdentifiers.ToggleStateProperty")
                    {
                        //TODO: fix me
                        if (e.NewValue.ToString() == "1")
                        {
                            CreateActionEvent(AE, DriverBase.ActionName.UnCheckCheckBox);
                        }

                        if (e.NewValue.ToString() == "0")
                        {
                            CreateActionEvent(AE, DriverBase.ActionName.CheckCheckBox);
                        }
                    }
                    break;
                case "list item":
                    CreateActionEvent(AE, DriverBase.ActionName.SelectListItem);
                    break;

                case "menu item":
                    CreateActionEvent(AE, DriverBase.ActionName.ExpandMenu);
                    break;
            }

            SetLastEvent(AE, e.EventId);
        }

        private void FocusChangedHandler(object sender, AutomationFocusChangedEventArgs e)
        {
            AutomationElement AE = (AutomationElement)sender;
            try
            {
                if (lastFocusedElement != null)
                {
                    // check that the element still exist or it will go to exception
                    string value = getAECurrentValue(lastFocusedElement);
                    if ((lastFocusedElement.Current.LocalizedControlType == "edit") && !(string.IsNullOrEmpty(value)))
                    {
                        // CreateSetTextAction(lastFocusedElement);
                        CreateActionEvent(AE, DriverBase.ActionName.SetText);
                    }
                }
            }
            catch (ElementNotAvailableException ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in FocusChangedHandler", ex);
                //TODO: Need to handle the exception
            }
            //Check that the item in focus is in our window we are recording as focus can go to any desktop window and we get a call            
            System.Windows.Automation.Condition c = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, AE.Current.AutomationId);
            AE = CurrentWindow.FindFirst(TreeScope.Children, c);
            if (AE != null)
            {
                // We are focusing on element in same form we record so save it
                lastFocusedElement = AE;
            }
            else
            {
                // form is not in our scope of recording
                lastFocusedElement = null;
            }
        }
        #endregion RECORDING EVENTS

        internal void DragDropControl(AutomationElement AE, string valueForDriver)
        {
            try
            {
                AE.SetFocus();
                object patn1 = null;
                ((TransformPattern)(patn1)).Move(830, 350);
                TransformPattern patn = AE.GetCurrentPattern(TransformPattern.Pattern) as TransformPattern;
                patn.Move(830, 350);
                InvokePattern ivp = AE.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
                ivp.Invoke();
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in DragDropControl", ex);
            }
        }

        public string getAECurrentValue(AutomationElement AE)
        {
            object objPattern;
            ValuePattern valPattern;
            string curValue = "";
            if (AE != null)
            {
                if (AE.TryGetCurrentPattern(ValuePattern.Pattern, out objPattern))
                {
                    valPattern = objPattern as ValuePattern;
                    curValue = valPattern.Current.Value;
                }
            }
            return curValue;
        }

        private string getAEProperty(AutomationElement AE, AutomationProperty AP)
        {
            object propAE = AE.GetCurrentPropertyValue(AP);
            return (string)propAE;
        }

        private System.Windows.Automation.Condition createCondbyStr(string cond)
        {
            List<System.Windows.Automation.Condition> conditions = new List<System.Windows.Automation.Condition>();
            string[] lstMultiLocVals = cond.Split('|');

            if (lstMultiLocVals.Length >= 1)
            {
                foreach (string s in lstMultiLocVals)
                {
                    string[] ls = s.Split(':');
                    if (ls.Length > 0)
                    {
                        conditions.Add(new PropertyCondition(getAELocatorTypeByString(ls[0].ToString()), ls[1].ToString()));
                    }
                    //WHY?
                    conditions.Add(new PropertyCondition(AutomationElement.IsOffscreenProperty, true));
                }
            }
            if (conditions.Count == 1) ElementCondition = conditions[0];
            else ElementCondition = new AndCondition(conditions.ToArray());
            return ElementCondition;
        }
        public bool isChildofAppWindow(AutomationElement AE)
        {
            //TreeWalker walker = TreeWalker.ControlViewWalker;
            TreeWalker walker = TreeWalker.RawViewWalker;
            AutomationElement PE = walker.GetParent(AE); foundParent = false;
            if (PE != null)
            {
                if (PE == CurrentWindow)
                { foundParent = true; }
                else
                {
                    if (PE != AutomationElement.RootElement) { isChildofAppWindow(PE); }
                    else { foundParent = false; }
                }
            }
            return foundParent;
        }
              
        public override List<object> GetListOfWindows()
        {
            List<object> appWindows = new List<object>();
            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement PE = walker.GetFirstChild(AutomationElement.RootElement);

            while (PE != null)
            {
                appWindows.Add(PE);               
                try
                {
                    PE = walker.GetNextSibling(PE);
                }
                catch(Exception ex)
                {
                    // Do nothing
                    // other option is to turn this exception off...
                    // 1. Navigate to Debug->Exceptions...
                    // 2. Expand "Managed Debugging Assistants"
                    // 3. Uncheck the NonComVisibleBaseClass Thrown option.        
                    // 4. Click [Ok]
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetListOfWindows", ex);
                }
            }
            return appWindows;
        }

        public override List<AppWindow> GetListOfDriverAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();
            try
            {
                List<object> AppWindows = GetListOfWindows();

                if (mPlatform == ePlatform.Windows)
                {
                    int gingerProcessId = Process.GetCurrentProcess().Id;

                    foreach (AutomationElement window in AppWindows)
                    {

                        //Exclude own process
                        if (window.Current.ProcessId == gingerProcessId)
                        {
                            continue;
                        }
                        if(CheckUserSpecificProcess(window) == false)
                        {
                            continue;
                        }

                        //list All Windows except PB windows - FNW

                        if (!window.Current.ClassName.StartsWith("FNW"))
                        {
                            string WindowTitle = GetWindowInfo(window);
                            if (String.IsNullOrEmpty(WindowTitle))
                            {
                                //Currently Element name for PDF is blank. So we get title using Process

                                //TODO: When we have two pdfs then both of them will be linked to same process and will have same title
                                // Need to find a way to handle this scenario

                                Process p = Process.GetProcessById(window.Current.ProcessId);
                                if (p.ProcessName == "AcroRd32")
                                {
                                    WindowTitle = Process.GetProcessById(window.Current.ProcessId).MainWindowTitle;
                                }                                
                            }
                            if (!String.IsNullOrEmpty(WindowTitle))
                            {

                                list.Add(GetAppWinodowForElement(window, WindowTitle, AppWindow.eWindowType.Windows));
                            }

                            if (CurrentWindow != null && CurrentWindow.Equals(window))
                            {
                                if (IsWindowValid(window))
                                {
                                    CurrentWindow = window;                                    
                                    CurrentWindowName = GetWindowInfo(window);                                    
                                }
                            }
                        }
                    }
                }

                else
                {
                    Dictionary<string, int> windowDictionary = new Dictionary<string, int>();

                    foreach (AutomationElement window in AppWindows)
                    {
                        if (!IsWindowValid(window)) 
                        {
                            continue;
                        }
                        if (CheckUserSpecificProcess(window) == false)
                        {
                            continue;
                        }
                        if (window.Current.ClassName.StartsWith("FNW") || (window.Current.LocalizedControlType == "dialog" && window.Current.ClassName.StartsWith("#")))
                        {
                            string WindowTitle = GetWindowInfo(window);
                            if (String.IsNullOrEmpty(WindowTitle))
                            {
                                WindowTitle = "NoTitleWindow";
                            }
                            if (!String.IsNullOrEmpty(WindowTitle))
                            {

                                if (windowDictionary.ContainsKey(WindowTitle))
                                {
                                    int currentCount = windowDictionary[WindowTitle];
                                    currentCount++;
                                    windowDictionary[WindowTitle] = currentCount;
                                    WindowTitle = WindowTitle + " [Index:" + currentCount + "]";
                                }
                                else
                                {
                                    windowDictionary.Add(WindowTitle, 0);
                                }
                                list.Add(GetAppWinodowForElement(window, WindowTitle, AppWindow.eWindowType.PowerBuilder));
                            }
                            if (CurrentWindow != null && CurrentWindow.Equals(window))
                            {
                                if (IsWindowValid(window))
                                {
                                    CurrentWindow = window;
                                    CurrentWindowName = window.Current.Name;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetListOfDriverAppWindows", ex);
                throw ex;
            }

            return list;
        }
        private bool CheckUserSpecificProcess(AutomationElement window)
        {
            Process currentProcess = Process.GetProcessById(window.Current.ProcessId);
            if (currentProcess.StartInfo.Environment["USERNAME"] != Environment.UserName)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
 

        #endregion RECORDING DRIVER

        public void CreateLocatorList(Act a, ObservableList<Act> Locators)
        {
            string[] lstMultiLocVals;
            string[] lstLocator = a.LocateValue.Split('#');
            if (lstLocator.Length > 1) { lstMultiLocVals = lstLocator[0].Split('|'); }
            else { lstMultiLocVals = a.LocateValue.Split('|'); }

            if (lstMultiLocVals.Length > 1)
            {
                foreach (string s in lstMultiLocVals)
                {
                    string[] ls = s.Split(':');
                    if (ls.Length > 0)
                    {
                        Locators.Add(new ActDummy() { LocateBy = getLocatorTypeByString(ls[0].ToString()), LocateValue = ls[1].ToString() });
                    }
                }
            }
            if (Locators.Count == 0) Locators.Add(new ActDummy() { LocateBy = eLocateBy.ByLocalizedControlType, LocateValue = "edit" });
        }


        public string getSelectedLocators(ObservableList<Act> Locators)
        {
            string strLoc = "";
            foreach (ActDummy a in Locators)
            {
                if (strLoc == "") { strLoc = strLoc + a.LocateBy.ToString() + ":" + a.LocateValue.ToString(); }
                else { strLoc = strLoc + "|" + a.LocateBy.ToString() + ":" + a.LocateValue.ToString(); }
            }
            return strLoc;
        }


        public bool ifSameParent(Act a, ObservableList<Act> olParent)
        {
            if (olParent == CreateParentLocators(a)) return true;
            else return false;
        }


        public ObservableList<Act> CreateParentLocators(Act a)
        {
            ObservableList<Act> olParLoc = new ObservableList<Act>();
            //TODO make multi properties object oriented
            if (a.LocateValue.IndexOf('#') >= 0)
            {
                string[] lstMultiLocVals;
                string[] lstParentLocator = a.LocateValue.Split('#');
                if (lstParentLocator.Length > 1)
                {
                    lstMultiLocVals = lstParentLocator[1].Split('|');
                    if (lstMultiLocVals.Length >= 1)
                    {
                        foreach (string s in lstMultiLocVals)
                        {
                            string[] ls = s.Split(':');
                            if (ls.Length > 0)
                            {
                                olParLoc.Add(new ActDummy() { LocateBy = getLocatorTypeByString(ls[0].ToString()), LocateValue = ls[1].ToString() });
                            }
                        }
                    }
                }
                else
                { olParLoc.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = "RootWindow" }); }
            }
            else
            { olParLoc.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = "RootWindow" }); }
            ParentLocators = olParLoc;
            return olParLoc;
        }


        public bool getActParentLevel(Act a)
        {
            if (a.LocateValue == null) return false;
            if (a.LocateValue.ToString().IndexOf("#RootWindow") >= 0) return true;
            else return false;
        }


        public System.Windows.Automation.Condition CreateElementCondition(Act a)
        {
            List<System.Windows.Automation.Condition> conditions = new List<System.Windows.Automation.Condition>();
            foreach (Act aLoc in ElementLocators)
            {
                conditions.Add(new PropertyCondition(getAELocatorTypeByString(aLoc.LocateBy.ToString()), aLoc.LocateValue));
            }
            if (conditions.Count == 1) ElementCondition = conditions[0];
            else ElementCondition = new AndCondition(conditions.ToArray());
            return ElementCondition;
        }
        
        public System.Windows.Automation.Condition CreateParentCondition(Act a)
        {
            List<System.Windows.Automation.Condition> conditions = new List<System.Windows.Automation.Condition>();
            if (ParentLocators.Count == 0) CreateParentLocators(a);
            foreach (Act aLoc in ParentLocators)
            {
                conditions.Add(new PropertyCondition(getAELocatorTypeByString(aLoc.LocateBy.ToString()), aLoc.LocateValue));
            }
            if (conditions.Count == 1) ParentCondition = conditions[0];
            else ParentCondition = new AndCondition(conditions.ToArray());
            return ParentCondition;
        }

        public AutomationProperty getAELocatorTypeByString(string sLocType)
        {
            AutomationProperty AP;
            AP = AutomationElementIdentifiers.ClassNameProperty;
            switch (sLocType)
            {
                case "ByControlType":
                    AP = AutomationElementIdentifiers.ControlTypeProperty;
                    break;
                case "ByClassName":
                    AP = AutomationElementIdentifiers.ClassNameProperty;
                    break;
                case "ByLocalizedControlType":
                    AP = AutomationElementIdentifiers.LocalizedControlTypeProperty;
                    break;
                case "ByName":
                    AP = AutomationElementIdentifiers.NameProperty;
                    break;
                case "ByAutomationID":
                    AP = AutomationElementIdentifiers.AutomationIdProperty;
                    break;
                case "ByBoundingRectangle":
                    AP = AutomationElementIdentifiers.BoundingRectangleProperty;
                    break;
                case "IsEnabled":
                    AP = AutomationElementIdentifiers.IsEnabledProperty;
                    break;
                case "IsOffscreen":
                    AP = AutomationElementIdentifiers.IsOffscreenProperty;
                    break;
            }
            return AP;
        }


        public eLocateBy getLocatorTypeByString(string sLocType)
        {
            eLocateBy eL = eLocateBy.NA;
            switch (sLocType)
            {
                case "NA":
                    eL = eLocateBy.NA;
                    break;
                case "ByID":
                    eL = eLocateBy.ByID;
                    break;
                case "ByName":
                    eL = eLocateBy.ByName;
                    break;
                case "ByCSS":
                    eL = eLocateBy.ByCSS;
                    break;
                case "ByXPath":
                    eL = eLocateBy.ByXPath;
                    break;
                case "ByXY":
                    eL = eLocateBy.ByXY;
                    break;
                case "ByHref":
                    eL = eLocateBy.ByHref;
                    break;
                case "ByLinkText":
                    eL = eLocateBy.ByLinkText;
                    break;
                case "ByValue":
                    eL = eLocateBy.ByValue;
                    break;
                case "ByIndex":
                    eL = eLocateBy.ByIndex;
                    break;
                case "ByClassName":
                    eL = eLocateBy.ByClassName;
                    break;
                case "ByAutomationID":
                    eL = eLocateBy.ByAutomationID;
                    break;
                case "ByLocalizedControlType":
                    eL = eLocateBy.ByLocalizedControlType;
                    break;
                case "ByBoundingRectangle":
                    eL = eLocateBy.ByBoundingRectangle;
                    break;
                case "IsEnabled":
                    eL = eLocateBy.IsEnabled;
                    break;
                case "IsOffscreen":
                    eL = eLocateBy.IsOffscreen;
                    break;
            }
            return eL;
        }

        public System.Windows.Automation.Condition getActCondition(Act a)
        {
            string LocateValue = a.LocateValueCalculated;
            switch (a.LocateBy)
            {
                case eLocateBy.ByAutomationID:
                    ElementCondition = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, LocateValue);
                    return ElementCondition;
                case eLocateBy.ByName:
                    ElementCondition = new PropertyCondition(AutomationElementIdentifiers.NameProperty, a.LocateValue);
                    return ElementCondition;
                default:
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    a.ExInfo = "Unknown Locator - " + a.LocateBy;
                    return null;
                    //break;
            }
        }

        //Find first matching element based on locator
        public override object FindElementByLocator(eLocateBy locateBy, string locateValue)
        {
            object element = null;
            loadwaitSeconds = mLoadTimeOut.Value;

            int count = 0;
            bool isLoaded = false;
            while (!isLoaded && !taskFinished)
            {
                if (!IsWindowValid(CurrentWindow))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "FindElementByLocator Exception while reading the current window name, loading the window again.");
                    AutomationElement currentWin = GetCurrentActiveWindow();
                    if (currentWin != null && IsWindowValid(currentWin))
                    {
                        CurrentWindow = currentWin;
                        CurrentWindowName = CurrentWindow.Current.Name;
                        isLoaded = true;
                    }
                }
                else
                {
                    isLoaded = true;
                }
                count++;
                if (count >= 3)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "FindElementByLocator Trying again count-" + count);
                    isLoaded = true;
                }
                Thread.Sleep(100);
            }

            count = 0;
            int ecount = 0;
            DateTime startingTime = DateTime.Now;
            isLoaded = false;
            while (!isLoaded && !taskFinished)
            {
                try
                {
                    switch (locateBy)
                    {
                        case eLocateBy.ByName:
                            System.Windows.Automation.Condition nameCondition = new PropertyCondition(AutomationElementIdentifiers.NameProperty, locateValue);
                            element = CurrentWindow.FindFirst(TreeScope.Subtree, nameCondition);

                            //If Name has been derived using Controltype & AutomationId
                            if (element==null && locateValue.Contains("(") && locateValue.Contains(")"))
                            {
                                int lastIndex = locateValue.IndexOf(")");
                                int firstindex = locateValue.IndexOf("(");
                                string controlType = locateValue.Substring(0, firstindex).Trim();
                                string automationId = locateValue.Substring(firstindex + 1, lastIndex - firstindex - 1);
                                //Anding condition to find element by LocalizedControlType & Automation Id
                                AndCondition andConditn = new AndCondition(
                                new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, automationId),
                                new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, controlType));
                                element = CurrentWindow.FindFirst(TreeScope.Subtree, andConditn);
                            }

                            //For old compatibility where Name was the text we fail over to search by Text, PB Only, it is slower as it scan the tree and call win api to get the text
                            if (element == null && mPlatform == ePlatform.PowerBuilder)
                            {
                                element = GetElementByText(CurrentWindow, locateValue);
                            }

                            break;
                        case eLocateBy.ByXPath:
                            try
                            {
                                UIAElementInfo e = (UIAElementInfo)mXPathHelper.GetElementByXpath(locateValue);
                                element = (AutomationElement)e.ElementObject;
                            }
                            catch (Exception e)
                            {
                                element = OLD_GetElementByXpath_OLD(locateValue);
                                Reporter.ToLog(eLogLevel.DEBUG, "Exception in Getting Element by XPath", e);
                            }
                            break;
                        case eLocateBy.ByAutomationID:
                            System.Windows.Automation.Condition CurCond3 = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, locateValue);
                            element = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond3);
                            break;

                        case eLocateBy.ByClassName:
                            System.Windows.Automation.Condition CurCond4 = new PropertyCondition(AutomationElementIdentifiers.ClassNameProperty, locateValue);
                            element = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond4);
                            break;

                        case eLocateBy.ByXY:
                            double xx = 0, yy = 0;
                            bool statusFlag;
                            string[] str = locateValue.Split(',');
                            xx = Convert.ToDouble(str[0]);
                            yy = Convert.ToDouble(str[1]);
                            xx = xx + Convert.ToInt32(CurrentWindow.Current.BoundingRectangle.X);
                            yy = yy + Convert.ToInt32(CurrentWindow.Current.BoundingRectangle.Y);
                            System.Windows.Point point = new System.Windows.Point(xx, yy);
                            //SwitchWindow("NoTitleWindow");  //What is this?
                            statusFlag = WinAPIAutomation.SetForeGroundWindow(CurrentWindow.Current.ProcessId);
                            if (statusFlag == false)
                            {
                                WinAPIAutomation.ShowWindow(CurrentWindow);
                            }
                            element = AutomationElement.FromPoint(point);
                            break;
                        default:
                            throw new Exception("Locator not implement - " + locateBy.ToString());
                    }
                }
                catch(Exception e)
                {
                    ecount++;
                    Thread.Sleep(100);
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in FindElementByLocator", e);
                    if (ecount<5)
                        continue;
                }
                Reporter.ToLog(eLogLevel.DEBUG, "** Total time" + (DateTime.Now - startingTime).TotalSeconds + "  Load Wait Time  :  " + loadwaitSeconds);
                if (element == null && (DateTime.Now - startingTime).TotalSeconds <= loadwaitSeconds && !taskFinished)
                {
                    continue;
                }
                if (element == null)
                    break;
                count++;
                if ((element!=null && IsWindowValid(element)) || count >= 3)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "FindElementByLocator Trying again count-" + count);
                    isLoaded = true;
                }

            }

            if(element == null)
                Reporter.ToLog(eLogLevel.DEBUG, "FindElementByLocator Element is NULL.");
            else
                Reporter.ToLog(eLogLevel.DEBUG, "FindElementByLocator Element found Successfully.");

            if (!IsWindowValid(element))
                return null;

            return element;
        }

        private AutomationElement GetCurrentActiveWindow()
        {
            AutomationElement currentWin = null;
            try
            {
                List<object> AppWindows = GetListOfWindows();
                string WindowTitle = "";
                string WindowClassName = "";
                foreach (AutomationElement window in AppWindows)
                {
                    WindowTitle = GetWindowInfo(window);
                    WindowClassName = GetControlPropertyValue(window, "ClassName");
                    if (!String.IsNullOrEmpty(WindowTitle) && !String.IsNullOrEmpty(WindowClassName) && WindowClassName.StartsWith("FNW") && WindowTitle == CurrentWindowName)
                    {
                        if (IsWindowValid(window))
                        {
                            currentWin = window;
                            UpdateRootElement();
                            CurrentWindowName = CurrentWindow.Current.Name;
                            break; 
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetCurrentActiveWindow", ex);
            }
            return currentWin;
        }

        public override bool ClickContextMenuItem(object obj,string value)
        {
            bool result = false;
            AutomationElement element = null,AE=null;
            try
            {
                AE = obj as AutomationElement;
                winAPI.SendClick(AE);
                PropertyCondition controlTypeCondition = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "pane");
                PropertyCondition processIdCondition = new PropertyCondition(AutomationElementIdentifiers.ProcessIdProperty, CurrentWindow.Current.ProcessId);
                var conditionGroup = new AndCondition(controlTypeCondition, processIdCondition);


                AutomationElementCollection AELIst = AutomationElement.RootElement.FindAll(TreeScope.Children, conditionGroup);

                if (AELIst.Count == 0)
                    return false;

                AutomationElement paneAE = AELIst[0];

                if (paneAE == null)
                    return false;

                controlTypeCondition = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "list");

                AutomationElement listAE = paneAE.FindFirst(TreeScope.Subtree, controlTypeCondition);
                var boundingRec = listAE.Current.BoundingRectangle;
                int x = 0, incremFact = 10;
                int y = 0;
                bool found = false;
                x = Convert.ToInt32(boundingRec.X+(boundingRec.Width / 2));
                y = Convert.ToInt32(boundingRec.Y);
                while (!found && y <= (boundingRec.Y + boundingRec.Height))
                {
                    element = AutomationElement.FromPoint(new System.Windows.Point(x, y));
                    if (ReferenceEquals(element, null))
                    {
                        y += incremFact;
                    }
                    else
                    {
                        if (element.Current.Name.Equals(value))
                        {
                            found = true;
                        }
                        else
                            y += incremFact;
                    }
                }
                if (found)
                {
                    winAPI.SendClick(element);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickContexMenuItem", ex);
            }
            return result;
        }

        // start search from root and drill down 
        private AutomationElement GetElementByText(AutomationElement root, string locateValue)
        {
            string txt;
            if (!root.Current.IsContentElement) return null;
            AutomationElement AE = TreeWalker.ContentViewWalker.GetFirstChild(root);
            if (AE == null) return null;
            while (AE != null && !taskFinished)
            {
                txt = GetElementTitle(AE);
                if (txt == locateValue) return AE;

                //Try children, drill down recursively
                AutomationElement AE1= GetElementByText(AE, locateValue);
                if (AE1 != null)
                {
                    txt = GetElementTitle(AE1);
                    if (txt == locateValue) return AE1;
                }
                AE = TreeWalker.ContentViewWalker.GetNextSibling(AE);
            }
            return null;
        }

        //Find ALL matching elements based on locator
        private List<AutomationElement> FindElementsByLocator(ElementLocator EL)
        {
            eLocateBy eLocatorType = EL.LocateBy;
            string LocValueCalculated = EL.LocateValue;

            AutomationElementCollection AECollection = null;
            switch (eLocatorType)
            {
                case eLocateBy.ByName:
                    System.Windows.Automation.Condition NameCond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, LocValueCalculated);
                    AECollection = CurrentWindow.FindAll(TreeScope.Subtree, NameCond);                                        
                    return GetElementsListFromCollection(AECollection);
                case eLocateBy.ByXPath:
                    return GetElementsByXpath(LocValueCalculated);
                case eLocateBy.ByAutomationID:
                    System.Windows.Automation.Condition AutomationIDCond = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, LocValueCalculated);
                    AECollection = CurrentWindow.FindAll(TreeScope.Subtree, AutomationIDCond);
                    return GetElementsListFromCollection(AECollection);
                case eLocateBy.ByClassName:
                    System.Windows.Automation.Condition ClassNameCond = new PropertyCondition(AutomationElement.ClassNameProperty, LocValueCalculated);
                    AECollection = CurrentWindow.FindAll(TreeScope.Subtree, ClassNameCond);
                    return GetElementsListFromCollection(AECollection);
                case eLocateBy.ByXY:
                    double xx = 0, yy = 0;
                    bool statusFlag;
                    string[] str = LocValueCalculated.Split(',');
                    xx = Convert.ToDouble(str[0]);
                    yy = Convert.ToDouble(str[1]);
                    xx = xx + Convert.ToInt32(CurrentWindow.Current.BoundingRectangle.X);
                    yy = yy + Convert.ToInt32(CurrentWindow.Current.BoundingRectangle.Y);
                    System.Windows.Point point = new System.Windows.Point(xx, yy);
                    statusFlag = WinAPIAutomation.SetForeGroundWindow(CurrentWindow.Current.ProcessId);
                    if (statusFlag == false)
                    {
                        WinAPIAutomation.ShowWindow(CurrentWindow);
                    }
                    List<AutomationElement> AEXYList = null;
                    //FIXIME
                    AutomationElement AE2 = AutomationElement.FromPoint(point);
                    if (AE2 != null)
                    {
                        AEXYList = new List<AutomationElement>();
                        AEXYList.Add(AE2);
                        return AEXYList;
                    }
                    break;
                default:
                    throw new Exception("Locator not implement - " + eLocatorType.ToString());
            }

            return null;
        }

        private List<AutomationElement> GetElementsListFromCollection(AutomationElementCollection CurAE)
        {
            if (CurAE.Count == 0) return null;

            List<AutomationElement> list = new List<AutomationElement>();
            for (int i = 0; i < CurAE.Count; i++)
            {
                list.Add(CurAE[i]);
            }
            return list;
        }

        public AutomationElement OLD_GetElementByXpath_OLD(string XPath)
        {
            AutomationElement AE = null;
            string[] PathItems;
            if(XPath.Contains("/"))
            {
                PathItems = XPath.Split('/');
            }
            else
                PathItems = XPath.Split('\\');

            System.Windows.Automation.Condition CurCond2 = null;
            int? index2 = null;

            string PathOK = "";

            foreach (string PathItem in PathItems)
            {
                if (PathItem == "")
                    continue;
                if (PathItem.ToUpper() == "DESKTOP") { continue; }
                int i = -1;
                string attr = null;
                string value = null;
                int index = -1;
                index2 = null;
                /// ???

                string PathNode = PathItem.Replace("~~~", @"\");


                //Check Xpath Syntax for open parenthesis and closing parenthesis
                if (!(IsValidXpath(PathNode)))
                {
                    return null;
                }

                //If it start with [[ then it means More than one conditions are combined        
                if ((PathNode.StartsWith("[[") && PathNode.EndsWith("]]")))
                {
                    var attributeValueList = getPropertyValueList(PathNode, ref index);
                    if (index != -1)
                    {
                        AE = getElementByIndex(attributeValueList, index);
                        continue;
                    }

                    foreach (KeyValuePair<string, string> acct in attributeValueList)
                    {
                        attr = acct.Key;
                        value = acct.Value;

                        CurCond2 = this.getPropertyCondition(attr, value);
                        AutomationElement tempElement = null;
                        AutomationElementCollection AEList;
                        if (AE == null)
                        {
                            AEList = CurrentWindow.FindAll(TreeScope.Subtree, CurCond2);
                        }
                        else
                        {
                            AEList = AE.FindAll(TreeScope.Children, CurCond2);
                        }
                        if (AEList.Count >= 1)
                        {
                            foreach (AutomationElement element in AEList)
                            {
                                tempElement = element;
                                if (!(matchAllProperties(attributeValueList, tempElement))) { continue; }
                                AE = tempElement;
                                break;
                            }
                        }
                        //If no matching element found then try to find the element using other property conditions specified in XPath
                        else if (AEList.Count == 0)
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    //If it start with [ then it means property of the AE
                    if (PathNode.StartsWith("[") && PathNode.EndsWith("]"))
                    {

                        if (PathNode.Contains(":"))
                            i = PathNode.IndexOf(":");
                        else if (PathNode.Contains("~"))
                            i = PathNode.IndexOf("~");

                        attr = PathNode.Substring(1, i - 1);
                        value = PathNode.Substring(i + 1, PathNode.Length - attr.Length - 3);

                        //check for index
                        int i1 = value.IndexOf('[');
                        int i2= value.IndexOf(']');

                        if (i1>0 && i2 > 0) // We have index
                        {
                            string sIDX = value.Substring(i1+1, i2 - i1 -1);
                            value = value.Substring(0, i1);
                            index2 = int.Parse(sIDX);
                        }
                        CurCond2 = this.getPropertyCondition(attr, value);
                    }
                    //If it does not contain parenthesis  that means it is name property of an AE
                    else
                    {
                        //Search By Name
                        CurCond2 = new PropertyCondition(AutomationElementIdentifiers.NameProperty, PathNode);
                    }
                    if (AE == null) // first item in path/tree so start from the current window
                    {
                        if (CurrentWindow == null)
                        {
                            AE = AutomationElement.RootElement.FindFirst(TreeScope.Subtree, CurCond2);
                        }
                        else
                        {
                            AE = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond2);
                        }
                    }
                    else
                    {
                        // sub item so start from the last AE
                        if (index2 == null)
                        {
                            AE = AE.FindFirst(TreeScope.Children , CurCond2);
                        }
                        else
                        {
                            //TODO: fix me for speed use walker instead of FindAll which is heavy.

                            // We have index so move next till the item we want
                            
                            AutomationElementCollection AEC = AE.FindAll(TreeScope.Children, CurCond2);
                            if ((int)index2 < AEC.Count - 1)  // make sure we don't try to get elem out of boundaries
                            {
                                AE = AEC[(int)index2];
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "Cannot find element" + value + "[" + index2 + "] Because array contains only " + AEC.Count + "Elements");
                            }
                        }
                    }
                    //If we didn't find sub elem we throw exception and show the path till were we got OK.

                    if (AE == null )
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Cannot find element, Found path to element at:" + PathOK + ",But couldn't find next element:" + PathNode);
                    }
                    else
                    {
                        PathOK += "\\" + PathNode;
                    }
                }
            }
            return AE;
        }

        //Return FIRST element matching the given XPath
        public AutomationElement GetElementByXpath(string XPath)
        {
            UIAElementInfo EI = (UIAElementInfo)mXPathHelper.GetElementByXpath(XPath);
            return (AutomationElement)EI.ElementObject;
        }

        //Return ALL elements matching the given XPath
        public List<AutomationElement> GetElementsByXpath(string XPath)
        {
            List<ElementInfo> elems = mXPathHelper.GetElementsByXpath(XPath);
            List<AutomationElement> list = new List<AutomationElement>();
            foreach(ElementInfo EI in elems)
            {
                list.Add((AutomationElement)EI.ElementObject);
            }
            return list;
        }

        private List<KeyValuePair<string, string>> getPropertyValueList(string PathNode, ref int index)
        {
            string Node = PathNode;
            string attributeName = null;
            string attributeValue = null;
            int i = -1;

            if (Node.Contains("[[")) Node = Node.Replace("[[", string.Empty);

            if (Node.Contains("[")) Node = Node.Replace("[", string.Empty);

            if (Node.Contains("]]")) Node = Node.Replace("]]", string.Empty);

            string[] subPathItems = Node.Split(']');

            var attributeValueList = new List<KeyValuePair<string, string>>();


            foreach (string subPathNode in subPathItems)
            {

                if (subPathNode.Contains(":")) i = subPathNode.IndexOf(":");

                else if (subPathNode.Contains("~")) i = subPathNode.IndexOf("~");

                attributeName = subPathNode.Substring(0, i);
                attributeValue = subPathNode.Substring(i + 1, subPathNode.Length - attributeName.Length - 1);

                if (attributeName.Equals("Index"))
                {
                    Int32.TryParse(attributeValue, out index);

                }
                else
                    attributeValueList.Add(new KeyValuePair<string, string>(attributeName, attributeValue));

            }

            return attributeValueList;
        }

        private bool IsValidXpath(string PathNode)
        {
            int iterator = 0;
            char tempChar;
            int openParenthesisCount = 0;
            int closingParenthesisCount = 0;
            char[] tempArray = PathNode.ToCharArray();

            while (iterator < PathNode.Length)
            {
                tempChar = tempArray[iterator];
                if (tempChar.Equals('[')) openParenthesisCount++;
                else if (tempChar.Equals(']')) closingParenthesisCount++;
                iterator++;

            }
            if (openParenthesisCount != closingParenthesisCount)
            {
                return false;
            }
            return true;
        }

        private AutomationElement getElementByIndex(List<KeyValuePair<string, string>> propertyList, int index)
        {
            PropertyCondition condition;
            AutomationElementCollection elementList;
            AutomationElement element = null;
            foreach (KeyValuePair<string, string> acct in propertyList)
            {
                condition = this.getPropertyCondition(acct.Key, acct.Value);
                elementList = CurrentWindow.FindAll(TreeScope.Subtree, condition);
                try
                {
                    element = elementList[index];
                }
                catch (IndexOutOfRangeException)
                {                    
                    Reporter.ToUser(eUserMsgKey.InvalidIndexValue, index + "\n\n Valid Index should be between 0 to " + (elementList.Count - 1));
                }
            }
            return element;
        }

        private bool matchAllProperties(List<KeyValuePair<string, string>> propertyList, AutomationElement element)
        {
            bool matchAllFlag = true;
            string actualValue = null;
            string expectedValue = null;

            foreach (KeyValuePair<string, string> acct in propertyList)
            {
                actualValue = element.GetCurrentPropertyValue(this.getProperty(acct.Key)).ToString();
                expectedValue = acct.Value;

                if (!(actualValue.Equals(expectedValue)))
                {
                    matchAllFlag = false;
                    break;
                }
            }
            return matchAllFlag;
        }

        private PropertyCondition getPropertyCondition(string attr, string value)
        {
            PropertyCondition attributeMatchCondition = null;
            switch (attr)
            {
                case "AutomationId":
                    attributeMatchCondition = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, value);
                    break;

                case "Name":
                    attributeMatchCondition = new PropertyCondition(AutomationElementIdentifiers.NameProperty, value);
                    break;

                case "ClassName":
                    attributeMatchCondition = new PropertyCondition(AutomationElementIdentifiers.ClassNameProperty, value);
                    break;

                case "LocalizedControlType":
                    attributeMatchCondition = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, value);
                    break;

                case "Value":
                    attributeMatchCondition = new PropertyCondition(ValuePatternIdentifiers.ValueProperty, value);
                    break;

                case "ToggleState":
                    ToggleState toggleState = ToggleState.Off;
                    if (value == "True") toggleState = ToggleState.On;
                    attributeMatchCondition = new PropertyCondition(TogglePatternIdentifiers.ToggleStateProperty, toggleState);
                    break;

                case "IsSelected":
                    bool IsSelected = false;
                    if (value == "True") IsSelected = true;
                    attributeMatchCondition = new PropertyCondition(SelectionItemPatternIdentifiers.IsSelectedProperty, IsSelected);
                    break;
            }
            return attributeMatchCondition;
        }

        public AutomationProperty getProperty(string attr)
        {
            AutomationProperty elementProperty = null;
            switch (attr)
            {
                case "AutomationId":
                    elementProperty = AutomationElementIdentifiers.AutomationIdProperty;
                    break;

                case "Name":
                    elementProperty = AutomationElementIdentifiers.NameProperty;
                    break;

                case "ClassName":
                    elementProperty = AutomationElementIdentifiers.ClassNameProperty;
                    break;

                case "LocalizedControlType":
                    elementProperty = AutomationElementIdentifiers.LocalizedControlTypeProperty;
                    break;

                case "Value":
                    elementProperty = ValuePatternIdentifiers.ValueProperty;
                    break;

                case "ToggleState":

                    elementProperty = TogglePatternIdentifiers.ToggleStateProperty;
                    break;

                case "IsSelected":

                    elementProperty = SelectionItemPatternIdentifiers.IsSelectedProperty;
                    break;
            }
            return elementProperty;
        }
        
        public System.Windows.Automation.Condition getGridActColumnCond(Act act)
        {
            AndCondition GridCond;
            AndCondition ColumnPropertiesConds;

            //get actionColumn
            string[] strAct = act.LocateValue.Split('~'); //"ByName:ButtonClick|ByClass:button~ByName:columnName|ByClass:Edit~ByName:columnName2|ByClass:button"
            string strColumn = strAct[1];

            List<System.Windows.Automation.Condition> conditions = new List<System.Windows.Automation.Condition>();
            ObservableList<Act> actLocators = CreateElementLocators(strColumn);
            foreach (Act aLoc in actLocators)
            {
                conditions.Add(new PropertyCondition(getAELocatorTypeByString(aLoc.LocateBy.ToString()), aLoc.LocateValue));
            }
            if (conditions.Count == 1) ColumnPropertiesConds = new AndCondition(conditions[0]);
            else ColumnPropertiesConds = new AndCondition(conditions.ToArray());

            GridCond = new AndCondition(ConditionContent, ColumnPropertiesConds);
            return GridCond;
        }

        private ObservableList<Act> CreateElementLocators(string a)
        {
            ObservableList<Act> olElementLoc = new ObservableList<Act>();
            //TODO make multi properties object oriented
            string[] lstMultiLocVals;

            lstMultiLocVals = a.Split('|');
            if (lstMultiLocVals.Length >= 1)
            {
                foreach (string s in lstMultiLocVals)
                {
                    string[] ls = s.Split(':');
                    if (ls.Length > 0)
                    {
                        olElementLoc.Add(new ActDummy() { LocateBy = getLocatorTypeByString(ls[0].ToString()), LocateValue = ls[1].ToString() });
                    }
                }
            }
            if (olElementLoc.Count == 0)
            {
                olElementLoc.Add(new ActDummy() { LocateBy = eLocateBy.NA, LocateValue = "" });
            }
            return olElementLoc;
        }
        
        public override Boolean IsWindowExist(Act act)
        {
            bool isExist = false;
            AutomationElement AEWindow = null;
            try
            {
                AEWindow = (AutomationElement)FindWindowByLocator(act.LocateBy, act.LocateValueCalculated);

                if (AEWindow != null)
                {
                    isExist = true;
                }
            }
            catch (Exception ex)
            {
                act.Error += "ERROR::";
                act.ExInfo += ex.Message;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in getting the window", ex);
            }               
            
            return isExist;
        }

        public override Boolean CloseWindow(Act act)
        {
            bool isClosed = false;
            AutomationElement AEWindow = null;         
            try
            {
                AEWindow = (AutomationElement)FindWindowByLocator(act.LocateBy, act.LocateValueCalculated);
                
                if (AEWindow != null)
                {
                    (AEWindow.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern).Close();
                    isClosed = true;
                }
            }
            catch (Exception ex)
            {
                act.Error += "ERROR::";
                act.ExInfo += ex.Message;
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in closing the window.", ex);
            }

            return isClosed;
        }
        public override object FindWindowByLocator(eLocateBy locateBy,string locateValue)
        {
            AutomationElement AEWindow=null;
            try
            {
                if (locateBy == eLocateBy.ByTitle)
                {
                    List<object> AppWindows = GetListOfWindows();
                    foreach (AutomationElement window in AppWindows)
                    {
                        string WindowTitle = GetWindowInfo(window);
                        if (WindowTitle != null && WindowTitle.Equals(locateValue))
                        {
                            AEWindow = window;
                            break;
                        }
                    }

                    if (AEWindow == null)
                    {
                        AEWindow = (AutomationElement)FindElementByLocator(eLocateBy.ByName, locateValue);
                    }
                }
                else if (locateBy == eLocateBy.ByName)
                {
                    // Window action--> IsExist was earlier used for checking element exist or not
                    // But later it is changed to check if window exist or not
                    // And for element user need to use Window Control action--> Is Exist
                    // But still we want to support old flows.
                    AEWindow = (AutomationElement)FindElementByLocator(locateBy, locateValue);
                }
                else
                {
                    throw new Exception(locateBy.ToString() + " is not valid for is Exist.Only locate by title is supported");                    
                }                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetWindowElementByLocator", ex);
            }
            return AEWindow;
        }
       
        public override Boolean SetWindowVisualState(ActWindow act)
        {
            AutomationElement AEWindow = null;        

            AEWindow = (AutomationElement)FindWindowByLocator(act.LocateBy, act.LocateValueCalculated);

            string status = SetElementVisualState(AEWindow, act.WindowActionType.ToString());

            if(status != "State set successfully")                
            {
                act.Error += "ERROR::";
                act.ExInfo += status;
                return false;
            }
            return true;            
        }
        public override string SetElementVisualState(object objAE,string State)
        {
            try
            {
                AutomationElement AE = (AutomationElement)objAE;
                WindowPattern windowPattern = null;
                object vp;

                if (AE == null)
                    return "Element not found";

                AE.TryGetCurrentPattern(WindowPattern.Pattern, out vp);

                if (vp == null)
                    return "Unable to perform Action";

                windowPattern = (WindowPattern)vp;
                // Make sure the element is usable.
                while (windowPattern.Current.WindowInteractionState != WindowInteractionState.ReadyForUserInteraction && !taskFinished)
                {
                    Thread.Sleep(500);
                }
                if (windowPattern.Current.WindowInteractionState != WindowInteractionState.ReadyForUserInteraction)
                {
                    return "Unable to perform Action";
                }

                switch (State)
                {
                    case "Maximize":
                        // Confirm that the element can be maximized
                        if ((windowPattern.Current.CanMaximize) &&
                            !(windowPattern.Current.IsModal))
                        {
                            windowPattern.SetWindowVisualState(
                                WindowVisualState.Maximized);
                            // TODO: additional processing
                        }
                        break;
                    case "Minimize":
                        // Confirm that the element can be minimized
                        if ((windowPattern.Current.CanMinimize) &&
                            !(windowPattern.Current.IsModal))
                        {
                            windowPattern.SetWindowVisualState(
                                WindowVisualState.Minimized);
                            // TODO: additional processing
                        }
                        break;
                    case "Restore":
                        windowPattern.SetWindowVisualState(
                            WindowVisualState.Normal);
                        break;
                    default:
                        windowPattern.SetWindowVisualState(
                            WindowVisualState.Normal);
                        // TODO: additional processing
                        break;
                }

                return "State set successfully";
            }
            catch (Exception ex)
            {
                // object is not able to perform the requested action                
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in SetWindowVisualState", ex);
                return ex.Message;
            }
        }
        public override string SetElementSize(object objAE, string size)
        {
            double width = -1;
            double height = -1;
            try
            {
                string[] ls = size.Split(',');
                if (ls.Length == 1)
                    return "Invalid value for size::" + size;

                if (!Double.TryParse(ls[0], out width) || !Double.TryParse(ls[1], out height))               
                    return "Invalid value for size::" + size;

                AutomationElement AE = (AutomationElement)objAE;
                TransformPattern transformPattern = null;
                object vp;

                if (AE == null)
                    return "Element not found";

                AE.TryGetCurrentPattern(TransformPattern.Pattern, out vp);

                if (vp == null)
                    return "Unable to perform Action";

                transformPattern = (TransformPattern)vp;

                if (!transformPattern.Current.CanResize)                
                    return "Unable to perform Action";

                transformPattern.Resize(width, height);
                
                return "Element resize Successfully";
            }
            catch (Exception ex)
            {
                // object is not able to perform the requested action                
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in SetWindowVisualState", ex);
                return ex.Message;
            }
        }
        public override void DragAndDrop(object obj, ActUIElement act)
        {
            //ActPBControl actPB = (ActPBControl)act;
            ActUIElement actUIDragAndDrop = (ActUIElement)act;
            AutomationElement SourceElement = (AutomationElement)obj;

            AutomationElement TargetElement = (AutomationElement)FindElementByLocator((eLocateBy)actUIDragAndDrop.TargetLocateBy, actUIDragAndDrop.TargetLocateValueForDriver);
            if(TargetElement == null)
            {
                act.Error = "Target Element not FOund";
                return;
            }
            int xsource = 0;
            int ysource = 0;
            int xdest = 0;
            int ydest = 0;
            string SourceXY = actUIDragAndDrop.GetInputParamValue(ActUIElement.Fields.SourceDragXY);
            System.Windows.Rect ae1Rect = SourceElement.Current.BoundingRectangle;

            if (!String.IsNullOrEmpty(SourceXY) && SourceXY.IndexOf(",") > 0)
            {
                string[] coordinates = SourceXY.Split(',');
                //User will specify the X,Y relative to the element instead of related to whole window
                xsource = (Convert.ToInt16(SourceElement.Current.BoundingRectangle.X)) + Int16.Parse(coordinates[0]);
                ysource = (Convert.ToInt16(SourceElement.Current.BoundingRectangle.Y)) + Int16.Parse(coordinates[1]);
            }
            else
            {
                
                xsource = (Convert.ToInt16(SourceElement.Current.BoundingRectangle.TopLeft.X) + Convert.ToInt16(SourceElement.Current.BoundingRectangle.TopRight.X)) / 2;
                ysource = (Convert.ToInt16(SourceElement.Current.BoundingRectangle.BottomLeft.Y) + Convert.ToInt16(SourceElement.Current.BoundingRectangle.TopRight.Y)) / 2;
            }

            string TargetXY = actUIDragAndDrop.GetInputParamValue(ActUIElement.Fields.TargetDropXY);
            if (!String.IsNullOrEmpty(TargetXY) && TargetXY.IndexOf(",") > 0)
            {
                string[] coordinates = TargetXY.Split(',');
                //User will specify the X,Y relative to the element instead of related to whole window
                xdest = (Convert.ToInt16(TargetElement.Current.BoundingRectangle.X)) + Int16.Parse(coordinates[0]);
                ydest = (Convert.ToInt16(TargetElement.Current.BoundingRectangle.Y)) + Int16.Parse(coordinates[1]);
            }
            else
            {
                xdest = (Convert.ToInt16(TargetElement.Current.BoundingRectangle.TopLeft.X) + Convert.ToInt16(TargetElement.Current.BoundingRectangle.TopRight.X)) / 2;
                ydest = (Convert.ToInt16(TargetElement.Current.BoundingRectangle.BottomLeft.Y) + Convert.ToInt16(TargetElement.Current.BoundingRectangle.TopRight.Y)) / 2;
            }

            CurrentWindow.SetFocus();
            winAPI.ClickLeftMouseButtonAndHoldAndDrop(SourceElement, xsource, ysource, xdest, ydest);
            if (!(AutomationElement.FromPoint(new System.Windows.Point(ae1Rect.X, ae1Rect.Y)).Equals(SourceElement)))
            {
                act.Error = "Drag and Drop action failed ";
                return;
            }
            else
                act.ExInfo = act.LocateValueCalculated + "Dragged and Dropped on " + act.ValueForDriver;
        }

        public override string ClickAndValidteHandler(object obj, ActUIElement act)
        {
            AutomationElement AE = (AutomationElement)obj;                        
            ActUIElement.eElementAction clickType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ClickType).ToString(), out clickType) == false)
            {
                act.Error = "Unknown Click Type";
                return "false";
            }
            
            ActUIElement.eElementAction validationType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString(), out validationType) == false)
            {
                act.Error = "Unknown Validation Type";
                return "false";
            }
            string validationElementType = act.GetInputParamValue(ActUIElement.Fields.ValidationElement);
            
            eLocateBy validationElementLocateby;
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString(), out validationElementLocateby) == false)
            {
                act.Error = "Unknown Validation Element Locate By";
                return "false";
            }

            string validattionElementLocateValue = act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocatorValue);
            bool LoopNextCheck = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.LoopThroughClicks).ToString()) == "True")
                LoopNextCheck = true;

            string result;
            bool flag;

            result=DoUIElementClick(clickType, AE);

            if (result.Contains("Clicked Successfully"))
            {
                flag = LocateAndValidateElement(validationElementLocateby, validattionElementLocateValue, validationElementType, validationType);
                if(flag)
                {
                    return result;
                }
                if ((!flag) && (LoopNextCheck))
                {
                    return ClickElementByOthertypes(clickType, AE, validationElementLocateby, validattionElementLocateValue, validationElementType, validationType);
                }
                else
                {
                    return "Validation Failed";
                }
            }
            else
            {
                if (LoopNextCheck)
                {
                    return ClickElementByOthertypes(clickType, AE, validationElementLocateby, validattionElementLocateValue, validationElementType, validationType);
                }
            }
            
            return result;     
        }

        public string ClickElementByOthertypes(ActUIElement.eElementAction executedClick, AutomationElement AE, eLocateBy validationElementLocateby,string validattionElementLocateValue,string validationElementType,ActUIElement.eElementAction validationType)
        {
            Platforms.PlatformsInfo.PowerBuilderPlatform powerBuilderPlatform = new Platforms.PlatformsInfo.PowerBuilderPlatform();
            List<ActUIElement.eElementAction> clicks = powerBuilderPlatform.GetPlatformUIClickTypeList();            
            ActUIElement.eElementAction currentClick;
            string result = "";
            bool flag;

            for (int i = 0; i < clicks.Count; i++)
            {
                currentClick = clicks[i];
                if (currentClick != executedClick)
                {
                    result=DoUIElementClick(currentClick, AE);
                    if (result.Contains("Clicked Successfully"))
                    {
                        flag = LocateAndValidateElement(validationElementLocateby, validattionElementLocateValue, validationElementType, validationType);
                        if (flag)
                        {
                            return result;
                        }
                    }
                }
            }
            return "Validation Failed";
        }

        public string DoUIElementClick(ActUIElement.eElementAction clickType, AutomationElement element)
        {            
            Boolean clickTriggeredFlag = false;
            string result = "";
            
            while (!IsWindowValid(element) && !taskFinished)
            {
                Thread.Sleep(100);
            }
            if (taskFinished)
                return "Unable to load the click element";

            switch (clickType)
            {
                case ActUIElement.eElementAction.InvokeClick:
                    result=ClickElementUsingInvokePattern(element, ref clickTriggeredFlag);
                    break;

                case ActUIElement.eElementAction.LegacyClick:
                    result=ClickElementUsingLegacyPattern(element, ref clickTriggeredFlag);
                    break;

                case ActUIElement.eElementAction.MouseClick:
                    winAPI.SendClick(element);
                    result = "Clicked Successfully by Mouse Click";
                    break;
            }
            return result;
        }

        public override string SendKeysAndValidateHandler(object obj, ActUIElement act)
        {
            AutomationElement AE = (AutomationElement)obj;

            bool DefineHandleAction = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.DefineHandleAction).ToString()) == "True")
                DefineHandleAction = true;
            eLocateBy handleElementLocateby = eLocateBy.ByAutomationID;
            string handleElementLocateValue = "";
            ActUIElement.eElementAction handleActionType = ActUIElement.eElementAction.Click;

            if (DefineHandleAction==true)
            {   
                if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.HandleElementLocateBy).ToString(), out handleElementLocateby) == false)
                {
                    act.Error = "Unknown Handle Element Locate By";
                    return "false";
                }
                handleElementLocateValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.HandleElementLocatorValue);
                handleActionType = act.HandleActionType;                
            }

            ActUIElement.eElementAction validationType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString(), out validationType) == false)
            {
                act.Error = "Unknown Validation Type";
                return "false";
            }
            string validationElementType = act.GetInputParamValue(ActUIElement.Fields.ValidationElement);

            eLocateBy validationElementLocateby;
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString(), out validationElementLocateby) == false)
            {
                act.Error = "Unknown Validation Element Locate By";
                return "false";
            }
            string validattionElementLocateValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.ValidationElementLocatorValue);
            string validationValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.ValidationElementValue);

            string Value = act.GetInputParamCalculatedValue(ActUIElement.Fields.Value).ToString();
             
            
            bool flag=false;
            int iLoop = 0;
           

            while (flag == false && iLoop<20)
            {
                SendKeysToControl(AE, Value);
                if (DefineHandleAction == true)
                {
                    LocateAndHandleActionElement(handleElementLocateby, handleElementLocateValue, validationElementType, handleActionType);
                }
                flag = LocateAndValidateElement(validationElementLocateby, validattionElementLocateValue, validationElementType, validationType,validationValue);
                   
                if (flag)
                {
                    return "true";
                }                
                iLoop++;
                if ((!flag) && (iLoop>=20))                
                {
                    return "Validation Failed";
                }                
            }
            return "true";
        }
        public bool LocateAndValidateElement(eLocateBy LocateBy, string LocateValue, string elementType, ActUIElement.eElementAction actionType,string validationValue="")
        {
            int? tempLoadTimeout = mLoadTimeOut;
            if(actionType== ActUIElement.eElementAction.NotExist)            
                mLoadTimeOut = -1;            
            object obj = FindElementByLocator(LocateBy, LocateValue);
            mLoadTimeOut = tempLoadTimeout;

            AutomationElement AE = (AutomationElement)obj;

            switch (actionType)
            {
                case ActUIElement.eElementAction.IsEnabled:
                    string result = IsEnabledControl(AE);
                    if (result.Equals("true"))
                    {
                        return true;
                    }
                    break;

                case ActUIElement.eElementAction.Exist:
                    if (AE != null)
                    {
                        return true;
                    }

                    break;

                case ActUIElement.eElementAction.NotExist:
                    if (AE == null)
                    {
                        return true;
                    }
                    break;
                case ActUIElement.eElementAction.GetValue:
                    if (AE == null)
                    {
                        return false;
                    }                    
                    if(GetControlValue(AE) ==  validationValue)
                        return true;
                    break;
            }
            return false;

        }

        public override string SelectAndValidateHandler(object obj, ActUIElement act)
        {
            AutomationElement AE = (AutomationElement)obj;

            bool DefineHandleAction = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.DefineHandleAction).ToString()) == "True")
                DefineHandleAction = true;
            eLocateBy handleElementLocateby = eLocateBy.ByAutomationID;
            string handleElementLocateValue = "";
            ActUIElement.eElementAction handleActionType = ActUIElement.eElementAction.Click;

            if (DefineHandleAction == true)
            {
                if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.HandleElementLocateBy).ToString(), out handleElementLocateby) == false)
                {
                    act.Error = "Unknown Handle Element Locate By";
                    return "false";
                }
                handleElementLocateValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.HandleElementLocatorValue);
                handleActionType = act.HandleActionType;
            }           
            string subElementType = act.GetInputParamValue(ActUIElement.Fields.SubElementType);

            eLocateBy subElementLocateby;
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.SubElementLocateBy).ToString(), out subElementLocateby) == false)
            {
                act.Error = "Unknown Validation Element Locate By";
                return "false";
            }
            string subElementLocateValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.SubElementLocatorValue);            
            string Value = act.GetInputParamCalculatedValue(ActUIElement.Fields.Value).ToString();


            bool flag = false;
            bool endPane = false;
            int iLoop = 0;            
            int iPaneY = 0;
            if(subElementType == ActUIElement.eSubElementType.Pane.ToString())
            {
                if (GetControlValue(AE) == Value)
                { 
                    return "true";
                }

                int iClick = 0;
                string oldValue = "&*%^%$#";
                while (flag == false && iLoop < 30)
                {   
                    ClickOnXYPoint(AE, "10,10");
                    AutomationElement subElement = (AutomationElement)FindElementByLocator(subElementLocateby, subElementLocateValue);
                    if (subElement == null || subElement.Current.LocalizedControlType != "pane")
                    {
                        return "Invalid Sub Element";
                    }
                    //Thread.Sleep(100);
                    AutomationElement pageUp = null, pageDown = null, lineDown = null, lineUp = null;
                    if (TreeWalker.ContentViewWalker.GetFirstChild(subElement) != null)
                    {
                        pageDown = subElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
                        pageUp = subElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
                        lineDown = subElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Line down"));
                    }
                    else
                    {
                        List<AutomationElement> gridControls = GetGridControlsFromPoint(subElement);
                        foreach (AutomationElement subChild in gridControls)
                        {
                            if (subChild.Current.Name == "Page down")
                            { 
                                pageDown = subChild;
                            }
                            else if (subChild.Current.Name == "Page up")
                            { 
                                pageUp = subChild;
                            }
                            else if (subChild.Current.Name == "Line up")
                            { 
                                lineUp = subChild;
                            }
                            else if (subChild.Current.Name == "Line down")
                            { 
                                lineDown = subChild;
                            }
                        }
                    }                    
                    int iCount = 0;
                    while (lineUp != null && iCount < 30 && iLoop==0)
                    {
                        ClickOnXYPoint(lineUp, "5,5");
                        iCount++;
                        Reporter.ToLog(eLogLevel.DEBUG, "lineDown:iCount:" + iCount);
                    }                    
                    for (int i = 0; i < iClick; i++)
                    {
                        ClickOnXYPoint(lineDown, "5,5");
                        Reporter.ToLog(eLogLevel.DEBUG, "lineDown:" + iClick + ":" + i);
                    }
                    ClickOnXYPoint(subElement, "10," + (10 + iPaneY));
                    Reporter.ToLog(eLogLevel.DEBUG, "iPaneY:" + iPaneY);                   

                    string newValue = GetControlValue(AE);
                    bool ishandled = false;
                    if (DefineHandleAction == true)
                    {
                        int? loadTime = mLoadTimeOut.Value;
                        mLoadTimeOut = -1;
                        ishandled = LocateAndHandleActionElement(handleElementLocateby, handleElementLocateValue, subElementType, handleActionType);
                        Reporter.ToLog(eLogLevel.DEBUG, "ishandled:" + ishandled);
                        if (ishandled)
                        {
                            iClick++;                            
                        }                            
                        else
                        {
                            iClick = 1;
                        }                            
                        mLoadTimeOut = loadTime;                        
                        Reporter.ToLog(eLogLevel.DEBUG, "DefineHandleAction:" + iClick);
                    }
                    else
                    { 
                        iClick = 1;
                    }

                    if (newValue.Replace("&", "") == Value.Replace("&", ""))
                    {
                        if(GetControlValue(AE).Replace("&", "") == Value.Replace("&", ""))
                        {
                            return "true";                           
                        }
                        else
                        {
                            return "Error Occurred while selecting :" + Value;
                        }
                    }
                    Reporter.ToLog(eLogLevel.DEBUG, "oldValue:" + oldValue + " newValue:" + newValue + " endPane:" + endPane);
                    if (oldValue != newValue && endPane==false)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "in not end pane:");
                        if (iLoop == 0 && iClick == 2)
                        { 
                            iLoop = 0;
                        }
                        else
                        { 
                            iLoop++;
                        }
                        oldValue = newValue;
                    }                        
                    else
                    {
                        iClick = 0;
                        Reporter.ToLog(eLogLevel.DEBUG, "in end pane:");
                        endPane = true;
                        if (iPaneY < subElement.Current.BoundingRectangle.Height)
                        { 
                            iPaneY += 10;
                        }
                        else
                        { 
                            break;
                        }
                    }
                    Reporter.ToLog(eLogLevel.DEBUG, "iLoop now:" + iLoop);                    
                }                          
            }
            return "Could not find the Value in the list-" + Value;
        }

        public bool SelectFromPane(eLocateBy LocateBy, string LocateValue, string elementType, ActUIElement.eElementAction actionType, string validationValue = "")
        {
            int? tempLoadTimeout = mLoadTimeOut;
            if (actionType == ActUIElement.eElementAction.NotExist)
                mLoadTimeOut = -1;
            object obj = FindElementByLocator(LocateBy, LocateValue);
            mLoadTimeOut = tempLoadTimeout;

            AutomationElement AE = (AutomationElement)obj;

            switch (actionType)
            {
                case ActUIElement.eElementAction.IsEnabled:
                    string result = IsEnabledControl(AE);
                    if (result.Equals("true"))
                    {
                        return true;
                    }
                    break;

                case ActUIElement.eElementAction.Exist:
                    if (AE != null)
                    {
                        return true;
                    }

                    break;

                case ActUIElement.eElementAction.NotExist:
                    if (AE == null)
                    {
                        return true;
                    }
                    break;
                case ActUIElement.eElementAction.GetValue:
                    if (AE == null)
                    {
                        return false;
                    }
                    if (GetControlValue(AE) == validationValue)
                        return true;
                    break;
            }
            return false;

        }
        public override Boolean IsElementExist(eLocateBy LocateBy, string LocateValue)
        {
            AutomationElement AE = null;
            try
            {
                AE = (AutomationElement) FindElementByLocator(LocateBy, LocateValue);
                if (AE != null) return true;
            }
            catch (Exception)
            {
            }
            return false;
        }

        public override Boolean IsChildElementExist(eLocateBy LocateBy, string LocateValue,string ValueForDriver)
        {
            AutomationElement AE = null;
            AutomationElement childAE = null;
            try
            {
                AE = (AutomationElement)FindElementByLocator(LocateBy, LocateValue);
                if (AE != null)
                {
                    if(AE.Current.ClassName.Equals("PBTabControl32_100"))
                    {
                        
                        PropertyCondition tabSelectCondition = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "pane");
                        childAE = AE.FindFirst(TreeScope.Descendants, tabSelectCondition);
                        bool result= SelectTab(AE, ValueForDriver, false);                        
                        SelectTab(AE, childAE.Current.Name, false);
                        return result;
                    }
                    else
                    {
                        PropertyCondition childNameCondition = new PropertyCondition(AutomationElementIdentifiers.NameProperty, ValueForDriver);
                        childAE = AE.FindFirst(TreeScope.Descendants, childNameCondition);
                        if (childAE == null)
                            return false;
                        else
                            return true;
                    }
                }
            }
            catch (Exception)
            {
            }

            return false;
        }

        public bool LocateAndHandleActionElement(eLocateBy LocateBy, string LocateValue, string elementType, ActUIElement.eElementAction actionType)
        {
            object obj = FindElementByLocator(LocateBy, LocateValue);
            AutomationElement AE = (AutomationElement)obj;
            Reporter.ToLog(eLogLevel.DEBUG, "Check if AE NUll:");
            if (AE == null)
                return false;
            Reporter.ToLog(eLogLevel.DEBUG, "After AE not NUll:");
            switch (actionType)
            {
                case ActUIElement.eElementAction.Click:                    
                    string result = ClickElement(AE);
                    if (result.Equals("true"))
                    {
                        return true;
                    }
                    break;

                case ActUIElement.eElementAction.DismissDialog:
                    if (AE != null)
                    {
                        ElementInfo EI = new ElementInfo();
                        EI.ElementObject = AE;
                        List<ElementInfo> lstElem=GetElementChildren(EI);
                        foreach(ElementInfo elemInfo in lstElem)
                        {
                            if (((AutomationElement)elemInfo.ElementObject).Current.Name.ToString().ToLower() == "cancel")
                            {
                                result = ClickElement(elemInfo.ElementObject);
                                if (result.Equals("true"))
                                {
                                    return true;
                                }
                                break;
                            }
                        }
                    }

                    break;

                case ActUIElement.eElementAction.AcceptDialog:
                    if (AE != null)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "In Accept Dialog:");
                        UIAElementInfo EI = new UIAElementInfo
                        {
                            ElementObject = AE
                        };
                        List<ElementInfo> lstElem = GetElementChildren(EI);
                        foreach (ElementInfo elemInfo in lstElem)
                        {
                            if (((AutomationElement)elemInfo.ElementObject).Current.Name.ToString().ToLower() == "ok" )
                            {
                                result = ClickElement(elemInfo.ElementObject);
                                Reporter.ToLog(eLogLevel.DEBUG, "after click ok:" + result);
                                if (result.ToLower().Contains("clicked successfully"))
                                {
                                    return true;
                                }
                                Reporter.ToLog(eLogLevel.DEBUG, "for break:");
                                break;
                            }
                        }
                    }
                    break;
            }
            return false;

        }

        public override void SmartSyncHandler(ActSmartSync act)
        {
            AutomationElement AE = null;
            Stopwatch st = new Stopwatch();
            int? MaxTimeout = 0;
            try
            {
                if (act.WaitTime.HasValue == true)
                {
                    MaxTimeout = act.WaitTime.GetValueOrDefault();
                }
                else if (string.IsNullOrEmpty(act.GetInputParamValue("Value")))
                {
                    MaxTimeout = mLoadTimeOut;
                }
                else
                {
                    MaxTimeout = Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
                }
            }
            catch (Exception)
            {
                MaxTimeout = mLoadTimeOut;
            }
            mLoadTimeOut = -1;           
            switch (act.SmartSyncAction)
            {
                case ActSmartSync.eSmartSyncAction.WaitUntilDisplay:
                    st.Reset();
                    st.Start();

                    try { AE = (AutomationElement)GetActElement(act); }
                    catch (Exception) { }
                    Thread.Sleep(100);
                    while (!(AE != null && (AE.Current.IsKeyboardFocusable || !AE.Current.IsOffscreen)))
                    {
                        try { AE = (AutomationElement)GetActElement(act); }
                        catch (Exception) { }
                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                        {
                            act.Error = "Smart Sync of WaitUntilDisplay is timeout";
                            break;
                        }
                        Thread.Sleep(100);
                    }
                    break;
                case ActSmartSync.eSmartSyncAction.WaitUntilDisapear:
                    st.Reset();
                    try { AE = (AutomationElement)GetActElement(act); }
                    catch (Exception) { }
                    if (AE == null)
                    {
                        return;
                    }
                    else
                    {
                        st.Start();

                        while ((AE != null && (AE.Current.IsKeyboardFocusable || !AE.Current.IsOffscreen)))
                        {
                            Thread.Sleep(100);
                            try { AE = (AutomationElement)GetActElement(act); }
                            catch (Exception) { }

                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisapear is timeout";
                                break;
                            }
                        }

                    }
                    break;
            }
            return;
        }

        public void SetTextBoxValue(Act act)
        {
            string txt = act.GetInputParamCalculatedValue("Value");
            AutomationElement AE = (AutomationElement)GetActElement(act);
            if (AE != null)
            {
                SetControlValue(AE, txt);
            }
            else
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.ExInfo = "Element not found";
            }
        }

        public void GetTextBoxValue(Act act)
        {
            GetTextBoxValue(act, (AutomationElement)GetActElement(act));
        }
        
        public string ClickElementUsingInvokePattern(AutomationElement element, ref Boolean clickTriggeredFlag)
        {
            try
            {
                InvokePattern invokePattern = (InvokePattern)element.GetCurrentPattern(InvokePattern.Pattern);
                if (invokePattern == null) return "Failed to Perform Click , Invoke pattern is not available";

                clickTriggeredFlag = true;

                invokePattern.Invoke();
            }
            catch (COMException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingInvokePattern", e);
                return e.Message;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingInvokePattern", e);
                return e.Message;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingInvokePattern", e);
                return e.Message;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingInvokePattern", e);
                return e.Message;
            }

            return "Clicked Successfully using Invoke Pattern";

        }

        public string ClickElementUsingLegacyPattern(AutomationElement element, ref Boolean clickTriggeredFlag)
        {
            try
            {
                LegacyIAccessiblePattern legacyPattern = (LegacyIAccessiblePattern)element.GetCurrentPattern(LegacyIAccessiblePattern.Pattern);
                if (legacyPattern == null) return "Failed to Perform click , Legacy Pattern is not available";

                String sLegacyDefaultAction = (String)element.GetCurrentPropertyValue(LegacyIAccessiblePatternIdentifiers.DefaultActionProperty);

                if (sLegacyDefaultAction == null) return "Failed to Perform click, Do Default of Legacy action is not available";

                clickTriggeredFlag = true;
                legacyPattern.DoDefaultAction();
            }
            catch (COMException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingLegacyPattern", e);
                return e.Message;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingLegacyPattern", e);
                return e.Message;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingLegacyPattern", e);
                return e.Message;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in ClickElementUsingLegacyPattern", e);
                return e.Message;
            }
            return "Clicked Successfully using Legacy Pattern";
        }
        
        public override string ClickElement(object obj, bool asyncFlag = false)
        {
            AutomationElement element = (AutomationElement)obj;

            if (element.Current.IsEnabled)
            {

               //TODO: Instead of using flags, use Autoresetevent
                Boolean clickTriggeredFlag = false;
                Exception e = null;
                
                if(!asyncFlag)// Regular Click
                {
                    string status = ClickElementUsingInvokePattern(element, ref clickTriggeredFlag);
                    if (!status.Contains("Clicked Successfully"))
                    {
                        status = ClickElementUsingLegacyPattern(element, ref clickTriggeredFlag);
                        if (!status.Contains("Clicked Successfully"))
                        {
                            winAPI.SendClick(element);
                            status = "Clicked Successfully using Mouse event";
                        }
                    }
                    return status;
                }
                else// Async Click
                {
                    string status = "";
                    Thread UIAClickThread = new Thread(new ThreadStart(() =>
                    {
                        try
                        {
                             status = ClickElementUsingLegacyPattern(element, ref clickTriggeredFlag);
                            if (!status.Contains("Clicked Successfully"))
                            {
                                status = ClickElementUsingInvokePattern(element, ref clickTriggeredFlag);
                                if (!status.Contains("Clicked Successfully"))
                                {
                                    winAPI.SendClick(element);
                                    status = "Clicked Successfully using Mouse event";
                                    clickTriggeredFlag = true;
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            e = ex;                            
                        }

                    }));
                    UIAClickThread.IsBackground = true;
                    clickTriggeredFlag = false;
                    UIAClickThread.Start();

                    Stopwatch st = new Stopwatch();
                    st.Start();

                    while (clickTriggeredFlag == false && e == null && st.ElapsedMilliseconds < 30000)
                    {
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(100);

                    if (e != null) throw e;

                    UIAClickThread.Abort();
                    return status;
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Element you are trying to click is not Enabled.");
                return "Element you are trying to click is not Enabled.";
            }
        }

        public override void ClickOnXYPoint(object obj, string clickPoint)
        {
            AutomationElement AE = (AutomationElement) obj;
            string[] coordinates = clickPoint.Split(',');
            int x = (int)AE.Current.BoundingRectangle.X + Int32.Parse(coordinates[0]);
            int y = (int)AE.Current.BoundingRectangle.Y + Int32.Parse(coordinates[1]);

            winAPI.SendClickOnXYPoint(AE, x, y);
        }

        public override void DoRightClick(object obj,string XY="")
        {
            AutomationElement AE = (AutomationElement) obj;
            winAPI.SendRightClick(AE, XY);
        }

        public override void DoDoubleClick(object obj, string XY = "")
        {
            AutomationElement element = (AutomationElement) obj;
            winAPI.SendDoubleClick(element, XY);
        }

        public override void ClickMenuElement(Act act)
        {
            System.Drawing.Point currentPosition = System.Windows.Forms.Cursor.Position;            
            System.Drawing.Point newPosition = new System.Drawing.Point(1, 1);
            if (CurrentWindow.Current.BoundingRectangle != null)
            {
                newPosition.X = (int)CurrentWindow.Current.BoundingRectangle.X+7;
                newPosition.Y = (int)CurrentWindow.Current.BoundingRectangle.Y+7;
            }
            System.Windows.Forms.Cursor.Position = newPosition;
            string str = act.LocateValueCalculated;
            string[] locateValues = str.Split('|');
            int j = 0;
            
            foreach (string nodeItem in locateValues)
            {
                str = nodeItem;
                if (nodeItem.Contains(@"\t"))
                {
                    str = nodeItem.Replace(@"\t", "TABSign");

                    string[] parts = str.Split(new string[] { "TABSign" }, StringSplitOptions.None);

                    for (int k = 0; k < parts.Length - 1; k++)
                    {
                        str = parts[0] + Convert.ToChar("\t");
                    }

                    str += parts[parts.Length - 1];
                }
                locateValues[j] = str;
                j++;
            }
            int i = 0;
            AutomationElement menuElement = null, rootElement = null;
            rootElement = (AutomationElement)FindElementByLocator(eLocateBy.ByName, locateValues[0]);
            menuElement = rootElement;
                                    
            CollapseControlElement(rootElement);
                
            if (menuElement == null)
            {
                act.Error = "Cannot find Menu element: " + locateValues[i];
                return;
            }

            winAPI.SendClick(menuElement);  // clicking on main menu .. expanding it .. taking this action outof loop
            i++;
            try
            {
                for (; i < locateValues.Count(); i++)
                {

                    AutomationElement tempAE = null;
                    //Thread.Sleep(1500);
                    int counter = 0;
                    do
                    {
                        tempAE = GetNextMenuElement(menuElement, locateValues[i]);
                        if (tempAE != null)                            
                            break;
                        Thread.Sleep(100);
                        counter++;
                    } while (tempAE == null && counter < 10 && !taskFinished);                    
                    if (tempAE == null)
                    {
                        //act.Error = "Cannot find '" + locateValues[i] + "' Sub menu elements should be separated with '|'";
                        CollapseControlElement(rootElement);
                        string status = ClickMenuElementByXY(rootElement, locateValues);
                        if (status.Contains("False"))
                        {
                            CollapseControlElement(rootElement);
                            act.Error = status;
                        }
                        return;
                    }
                    menuElement = tempAE;
                    winAPI.SendClick(menuElement, false);
                }
                System.Windows.Forms.Cursor.Position = currentPosition;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception while finding sub menu... ",e);                

                CollapseControlElement(rootElement);
                string status = ClickMenuElementByXY(rootElement, locateValues);
                if (status.Contains("False"))
                {
                    CollapseControlElement(rootElement);
                    act.Error = status;
                }
                return;
            }
        }

        public string ClickMenuElementByXY(AutomationElement rootElement,string[] locateValues)
        {            
            string str="";
            int i = 1,x=0,y=0;
            AutomationElement menuElement = rootElement;
            System.Windows.Point point;


            CollapseControlElement(rootElement);
            x = (int)menuElement.Current.BoundingRectangle.X + ((int)menuElement.Current.BoundingRectangle.Width / 2);
            y = (int)menuElement.Current.BoundingRectangle.Y + ((int)menuElement.Current.BoundingRectangle.Height / 2);
            winAPI.SendClick(rootElement);
            str = "True";
            Thread.Sleep(100);
            for (; i < locateValues.Count() && (Convert.ToBoolean(str)); i++)
            {
                str = "False";
                Reporter.ToLog(eLogLevel.DEBUG, "***** Started Menu click by location for  :  " + locateValues[i]);                           
                while (menuElement != null && !taskFinished && ((menuElement.Current.ProcessId==rootElement.Current.ProcessId)||(menuElement.Current.ProcessId==0)))
                {
                    if (menuElement.Current.LocalizedControlType.Equals("menu item"))
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "***** Menu Element from XY  :   " + x + "," + y + "  :  " + menuElement.Current.Name);
                        if (menuElement.Current.Name.Equals(locateValues[i]))
                        {
                            if (menuElement.Current.IsEnabled)
                            {
                                str = "True";
                                winAPI.SendClick(menuElement);
                                Reporter.ToLog(eLogLevel.DEBUG, "***** Got Menu element  :  " + menuElement.Current.Name + "  Now changing the X");
                                if (menuElement != null && i == (locateValues.Count() - 1))
                                {
                                    Reporter.ToLog(eLogLevel.DEBUG, "***** Clicked Element Successfully : " + menuElement.Current.Name);
                                    return true + "Clicked element Successfully";
                                }
                                while (((menuElement.Current.ProcessId == rootElement.Current.ProcessId) || (menuElement.Current.ProcessId == 0)) && menuElement.Current.Name.Equals(locateValues[i]) && !taskFinished)
                                {
                                    
                                    x = x + 10;
                                    Reporter.ToLog(eLogLevel.DEBUG, "*****  changing the X to " +x);
                                    point = new System.Windows.Point(x, y);
                                    menuElement = AutomationElement.FromPoint(point);
                                }
                                
                                //x = (int)menuElement.Current.BoundingRectangle.Width + 25;                                
                                break;
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "***** Menu Element :  "+locateValues[i]+" is not enabled");
                                return false + " " + locateValues[i - 1] + " Element is not enabled";
                            }
                        }
                        else
                        {
                            y = y + 12;
                        }
                        point = new System.Windows.Point(x, y);
                        menuElement = AutomationElement.FromPoint(point);
                    }
                    else
                    {                        
                        while(menuElement!=null && !taskFinished && (!(menuElement.Current.LocalizedControlType.Equals("menu item"))) && ((menuElement.Current.ProcessId == rootElement.Current.ProcessId) || (menuElement.Current.ProcessId == 0)))
                        {
                            y = y + 4;
                            point = new System.Windows.Point(x, y);                            
                            menuElement = AutomationElement.FromPoint(point);
                        }
                    }                                        
                }
                Reporter.ToLog(eLogLevel.DEBUG, "***** Exit from While loop, Element :  "+locateValues[i] + " Not Found!");
            }
            Reporter.ToLog(eLogLevel.DEBUG, "***** Exit from locate value loop  ");
            return false+" "+locateValues[i-1]+" Element not found";
        }
        
        public AutomationElement GetNextMenuElement(AutomationElement menuElement, string NextItem)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start GetNextMenuElement  :  " + menuElement.Current.Name);
            List<System.Windows.Automation.Condition> conditions = new List<System.Windows.Automation.Condition>();
            conditions.Add(new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, menuElement.Current.LocalizedControlType));
            conditions.Add(new PropertyCondition(AutomationElementIdentifiers.NameProperty, NextItem));            
            AndCondition CurCond2 = new AndCondition(conditions.ToArray());
            menuElement = CurrentWindow.FindFirst(TreeScope.Subtree, CurCond2);            
            return menuElement;
        }
        
        private void GetTextBoxValue(Act act, AutomationElement element)
        {
            string val = "Not Valid Value";
            try
            {
                object valuePattern = null;
                if (!element.TryGetCurrentPattern(
                        ValuePattern.Pattern, out valuePattern))
                {
                    if (valuePattern != null)
                    {
                        val = ((ValuePattern)valuePattern).Current.Value;
                    }
                    else { val = element.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString(); }

                }
                else
                {
                    val = element.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString();
                }
            }
            catch { val = element.GetCurrentPropertyValue(AutomationElement.NameProperty).ToString(); }
            act.AddOrUpdateReturnParamActual("Actual", val);
        }

        public override void SendKeysToControl(object obj, string value)
        {
            Boolean IsSpecialCharExist = false;
            AutomationElement element = (AutomationElement)obj;

            if (value.Equals("~") || (value.StartsWith("{") && value.EndsWith("}")) || (value.StartsWith("+") || value.StartsWith("^") || value.StartsWith("%")))
            {
                IsSpecialCharExist = true;
            }
            if (IsSpecialCharExist)
            {
                winAPI.SetElementTextWithFocus(element, value);
            }
            else
            {
                winAPI.SetElementText(element, value);
            }
        }

        public override void SelectControlByIndex(object obj, string value)
        {
            Boolean status;
            AutomationElement element = (AutomationElement)obj;
            if (element.Current.ClassName == "PBTabControl32_100")
            {
                //TOOD: Find a way to handle with UI Automation instead of Win API action
                status = SelectTab(element, value, true);
                if (!status)
                {
                    throw new InvalidOperationException(
              "Tab with name " + value + " Not Found");
                }
            }
        }
        public Boolean SelectTab(AutomationElement element, string value, bool searchByIndx = false)
        {
            Boolean flag = false;
            int indexNum = -1;
            if (searchByIndx)
            {
                if (int.TryParse(value, out indexNum) == false || (indexNum <= 0))
                {
                    //report on error- wrong input
                    return false;
                }
            }

            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
            AutomationElement childAE = null, initialTab = null;
            int id = element.Current.ProcessId;
            int y1 = (int)(element.Current.BoundingRectangle.BottomLeft.Y - 10);
            int startPoint = (int)element.Current.BoundingRectangle.X + 5;
            int endPoint = (int)(element.Current.BoundingRectangle.X + element.Current.BoundingRectangle.Width);
            PropertyCondition tabSelectCondition = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "pane");
            int foundTabsCount = 0;
            initialTab = element.FindFirst(TreeScope.Descendants, tabSelectCondition);
            if (((element.Current.BoundingRectangle.Bottom) - (initialTab.Current.BoundingRectangle.Bottom)) < ((initialTab.Current.BoundingRectangle.Top) - (element.Current.BoundingRectangle.Top)))
            {
                startPoint = (int)element.Current.BoundingRectangle.X + 5;
                y1 = (int)((element.Current.BoundingRectangle.Y + initialTab.Current.BoundingRectangle.Y) / 2);
            }

            while (startPoint < endPoint && !taskFinished)
            {                
                winAPI.SendClickOnXYPoint(element,startPoint, y1);
                AutomationElement currentAE = element.FindFirst(TreeScope.Descendants, tabSelectCondition);
                if (currentAE != childAE)
                {
                    childAE = currentAE;
                    foundTabsCount++;
                    if (!searchByIndx)
                    {
                        //search by title
                        if (childAE.Current.Name.Equals(value) || childAE.Current.Name.Contains(value))
                        {
                            flag = true;
                            break;//tab found                        
                        }
                    }
                    else
                    {
                        //search by index                        
                        if (foundTabsCount == indexNum)
                        {
                            flag = true;
                            break;//tab found 
                        }
                    }
                }
                startPoint += 15;
            }
            System.Windows.Forms.Cursor.Position = p;
            if (!flag)
            {
                return false;
            }
            return true;
        }

        public override string SetControlValue(object obj, string value)
        {

            try
            {
                AutomationElement element = (AutomationElement)obj;
                if (!element.Current.IsEnabled)
                {
                    throw new InvalidOperationException(
                        "The control with an AutomationID of "
                        + element.Current.AutomationId.ToString()
                        + " is not enabled.\n\n");
                }

                object vp;

                string controlType = element.Current.LocalizedControlType;

                switch (controlType)
                {
                    case "Edit Box":   // Windows         sfd            
                        element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                        ((ValuePattern)vp).SetValue(value);
                        break;
                    case "text":
                    case "edit":
                    case "list view":
                        if (mPlatform == ePlatform.PowerBuilder)
                        {
                            try
                            {
                                string isReadOnly = element.GetCurrentPropertyValue(ValuePattern.IsReadOnlyProperty).ToString();
                                bool isKeyBoardFocusable = element.Current.IsKeyboardFocusable;

                                if (isKeyBoardFocusable && isReadOnly != "true")
                                {
                                    element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                                    ((ValuePattern)vp).SetValue(value);

                                    WinAPIAutomation.SendTabKey();
                                    }
                                else
                                {

                                    element.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out vp);
                                    if (vp != null)
                                    {
                                        ((LegacyIAccessiblePattern)vp).SetValue(value);

                                    }
                                    else
                                    {
                                        //TODO: one day try to work without win apu and moving the cursor
                                        //something like below, didn't work
                                        // It did fine the edit text box but value disappear...
                                        //element.SetFocus();
                                        //Thread.Sleep(1);
                                        //AutomationElement parentElement2 = TreeWalker.ContentViewWalker.GetParent(element);
                                        //// Find The PB Edit box which is created after click cell
                                        //PropertyCondition cond = new PropertyCondition(AutomationElementIdentifiers.AutomationIdProperty, "10"); 
                                        //AutomationElement AEEditBox = parentElement2.FindFirst(TreeScope.Children, cond);
                                        
                                        winAPI.SetElementText(element, value);
                                        element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                winAPI.SetElementText(element, value);
                                element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                                Reporter.ToLog(eLogLevel.DEBUG, "Error in SetControlValue", e);
                            }
                        }
                        else
                        {
                            element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                            if (vp != null)
                            {
                                ((ValuePattern)vp).SetValue(value);
                            }
                            else
                            {
                                throw new Exception("Element doesn't support ValuePattern.Pattern, make sure locator is finding the correct element");
                            }

                        }
                        break;
                    case "combo box":
                        //Catching the exception here will pass the action without error. For exception action should be failed and it is handled inside driver.
                        Reporter.ToLog(eLogLevel.DEBUG, "In Combo Box ::");
                        if (mPlatform == ePlatform.PowerBuilder)
                        {
                            string isReadOnly = element.GetCurrentPropertyValue(ValuePattern.IsReadOnlyProperty).ToString();
                            bool isKeyBoardFocusable = element.Current.IsKeyboardFocusable;

                            if (isKeyBoardFocusable && isReadOnly != "true")
                            {
                                element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                                ((ValuePattern)vp).SetValue(value);

                                WinAPIAutomation.SendTabKey();
                                    }
                            else
                            {
                                try
                                {
                                    AutomationElement CurrentSelectedElemet = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "list item"));
                                    string CurrentElementTitle = null;
                                    if (CurrentSelectedElemet == null || String.IsNullOrEmpty(CurrentSelectedElemet.Current.Name))
                                    {
                                        CurrentElementTitle = "";
                                    }
                                    else
                                    {
                                        CurrentElementTitle = CurrentSelectedElemet.Current.Name;
                                    }
                                    winAPI.SendClickOnXYPoint(element, ((int)element.Current.BoundingRectangle.X + 2), ((int)element.Current.BoundingRectangle.Y + 2));
                                    AutomationElementCollection AECollection = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ProcessIdProperty, CurrentWindow.Current.ProcessId));
                                    AutomationElement childElement = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, value));
                                    if (childElement == null)
                                    {
                                        throw new System.NullReferenceException();
                                    }
                                    AutomationElement FirstElement = element.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.ProcessIdProperty, element.Current.ProcessId));
                                    
                                    childElement.TryGetCurrentPattern(InvokePatternIdentifiers.Pattern, out vp);
                                    if (vp != null)
                                    {
                                        ((InvokePattern)vp).Invoke();
                                    }
                                    else
                                    {
                                        childElement.TryGetCurrentPattern(LegacyIAccessiblePattern.Pattern, out vp);
                                        if (vp != null)
                                        {
                                            childElement.SetFocus();
                                            ((LegacyIAccessiblePattern)vp).DoDefaultAction();

                                        }
                                        else
                                        {
                                            childElement.TryGetCurrentPattern(SelectionItemPattern.Pattern, out vp);
                                            if (vp != null)
                                            {
                                                ((SelectionItemPattern)vp).Select();
                                            }
                                            else
                                            {
                                                winAPI.SetElementText(element, value);

                                            }
                                        }
                                    }
                                    if (!CurrentElementTitle.Equals(value))
                                    {
                                        if (FirstElement != null && value.Equals(FirstElement.Current.Name))
                                        {
                                            winAPI.SendKeysByLibrary(childElement, "{DOWN}");
                                            winAPI.SendKeysByLibrary(childElement, "{UP}");
                                        }
                                        else
                                        {
                                            winAPI.SendKeysByLibrary(childElement, "{UP}");
                                            winAPI.SendKeysByLibrary(childElement, "{DOWN}");
                                        }
                                        WinAPIAutomation.SendTabKey();
                                    }
                                }
                                catch (System.NullReferenceException)
                                {
                                    throw new Exception("Unable to set value. Value - " + value);
                                }
                                catch (Exception e)
                                {
                                    winAPI.SetElementText(element, value);
                                    Reporter.ToLog(eLogLevel.DEBUG, "Error in SetControlValue", e);
                                }
                            }
                        }
                        else
                        {
                            element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                            if (vp != null)
                            {
                                ((ValuePattern)vp).SetValue(value);
                            }
                            else
                            {
                                SetCombobValueByUIA(element, value);
                            }
                        }
                        break;

                    // command button handler
                    case "button":
                        element.TryGetCurrentPattern(InvokePatternIdentifiers.Pattern, out vp);
                        ((InvokePattern)vp).Invoke();
                        break;

                    // check box handler
                    case "check box":
                    case "tree item":                    
                        if (value != "Checked" && value != "Unchecked")
                        {
                            throw new Exception(
                                "Unknown Value: [" + value + "] for CheckBox please use: Checked or Unchecked");
                        }

                        element.TryGetCurrentPattern(TogglePattern.Pattern, out vp);

                        ToggleState x = ((TogglePattern)vp).Current.ToggleState;

                        if (value == "Checked" && x == ToggleState.Off)
                        {
                            ((TogglePattern)vp).Toggle();
                        }

                        if (value == "Unchecked" && x == ToggleState.On)
                        {
                            ((TogglePattern)vp).Toggle();
                        }
                        break;
                
                    // radio button handler
                    case "radio button":
                        value = "True";
                        element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out vp);
                        ((SelectionItemPattern)vp).Select();
                        break;
                    case "list":
                    //Combo Box and List Box Handler
                    case "list item":
                    case "tree view item":
                        Reporter.ToLog(eLogLevel.DEBUG, "In List Item ::");
                        AutomationElement parentElement = TreeWalker.ContentViewWalker.GetParent(element);
                        
                        bool isMultiSelect = (bool)parentElement.GetCurrentPropertyValue(SelectionPatternIdentifiers.CanSelectMultipleProperty);
                        Reporter.ToLog(eLogLevel.DEBUG, "In List Item isMultiSelect::" + isMultiSelect);
                        if (isMultiSelect)
                        {
                            String IsItemSelected = (element.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                            if (IsItemSelected == "False")
                            {
                                element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out vp);
                                ((SelectionItemPattern)vp).AddToSelection();
                            }
                            else
                            {
                                element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out vp);
                                ((SelectionItemPattern)vp).RemoveFromSelection();
                            }
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Multi Select ::" + isMultiSelect.ToString());
                            element.TryGetCurrentPattern(ScrollItemPattern.Pattern, out vp);
                            if (vp != null)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "In Scrolltoview ::");
                                ((ScrollItemPattern)vp).ScrollIntoView();
                            }
                                

                            element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out vp);
                            if (vp != null)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "In Select Pattern::");
                                ((SelectionItemPattern)vp).Select();
                                Reporter.ToLog(eLogLevel.DEBUG, "Is Selected ::" + ((SelectionItemPattern)vp).Current.IsSelected);
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "In Send keys ::");
                                AutomationElement ParentElement = TreeWalker.ContentViewWalker.GetParent(element);
                                winAPI.SendKeysByLibrary(ParentElement,element.Current.Name);
                                string selItem = GetSelectedItem(ParentElement);
                                if (selItem != element.Current.Name)
                                {
                                    Reporter.ToLog(eLogLevel.DEBUG, "In Send keys ENTER. Current Selected Item is" + selItem);
                                    winAPI.SendKeysByLibrary(ParentElement, "{ENTER}");
                                }
                                    
                            }
                                
                        }
                        break;

                    case "pane":
                        if (element.Current.ClassName.Contains("SysDateTimePick32"))
                        {
                            WinAPIAutomation.SendInputKeys(value);
                        }
                        //Tab Control handling for PB
                        else if (element.Current.ClassName == "PBTabControl32_100")
                        {
                            //TOOD: Find a way to handle with UI Automation instead of Win API action
                            Boolean status = SelectTab(element, value);
                            if (!status)
                            {
                                throw new InvalidOperationException(
                          "Tab with name " + value + " Not Found");
                            }
                            return status.ToString();
                        }
                        break;
                    //Tab Item Handling for Windows
                    case "tab item":
                    case "item":
                        element.TryGetCurrentPattern(SelectionItemPatternIdentifiers.Pattern, out vp);
                            ((SelectionItemPattern)vp).Select();

                        break;

                    case "scroll bar":
                        element.TryGetCurrentPattern(RangeValuePattern.Pattern, out vp);
                        ((RangeValuePattern)vp).SetValue(((RangeValuePattern)vp).Current.Value + ((RangeValuePattern)vp).Current.SmallChange);
                        break;

                    case "document":
                        winAPI.SetElementText(element, value);

                        break;

                    // TODO:Find a better way to handle this.
                    //To handle document objects in CSM having blank control type
                    case "":
                            winAPI.SetElementText(element, value);

                        break;

                    case "tree view":
                        if(string.IsNullOrEmpty(value))
                            return "Empty Value: [" + value + "] for Tree View ";

                        List<string> nodeNames = value.Split(';').ToList<string>();
                        string IsTreeViewExpanded = (element.GetCurrentPropertyValue(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty).ToString());
                        if (General.CompareStringsIgnoreCase(IsTreeViewExpanded, "Collapsed"))
                        {
                            element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out vp);
                            ((ExpandCollapsePattern)vp).Expand();
                        }
                        element = TreeWalker.RawViewWalker.GetFirstChild(element);

                        for (int i = 0; i < nodeNames.Count - 1; i++)
                        {
                            element = ExpandTreeNode(element, nodeNames[i]);
                            if (element == null)
                                return nodeNames[i] + " not found in " + value;
                            element = TreeWalker.RawViewWalker.GetFirstChild(element);
                        }
                        if (AddChildToSelection(element, nodeNames[nodeNames.Count - 1]) == false)
                            return nodeNames[nodeNames.Count - 1] + "not found" + value;
                        break;
                    default:                        
                        Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                        break;

                }
                if (mPlatform == ePlatform.PowerBuilder)
                {
                    string str = "";
                    if (element.Current.LocalizedControlType.Equals("radio button") || element.Current.LocalizedControlType.Equals("list item"))
                    {
                        str = IsControlSelected(element);
                        return str;
                    }
                    if (value == null)
                    {
                        return "";
                    }
                    
                    str = GetControlValue(element);
                    if(String.IsNullOrEmpty(str))
                    {
                        str = GetControlText(element);
                    }
                    
                    value = value.Replace(" ","").Replace("-","");
                    str=str.Replace(" ", "").Replace("-", "");
                    if(value.Equals(str))
                    {
                        return "True";
                    }
                    if(str.StartsWith(value))
                    {
                        return "True";
                    }                    

                    DateTime outGetDate = new DateTime();
                    string[] dateformats = { "MMddyyyy", "MM/dd/yyyy", "MM-dd-yyyy", "M/d/yyyy", "M-d-yyyy", "Mdyyyy", "ddMMyyyy", "dd/MM/yyyy", "dd-MM-yyyy", "d/M/yyyy", "d-M-yyyy", "dMyyyy" };
                    if (DateTime.TryParseExact(str, dateformats, System.Globalization.CultureInfo.InvariantCulture, 0, out outGetDate))
                    {
                        DateTime outSetDate = new DateTime();
                        if (DateTime.TryParseExact(value, dateformats, System.Globalization.CultureInfo.InvariantCulture, 0, out outSetDate))
                        {
                            if (outGetDate.ToLongDateString() == outSetDate.ToLongDateString())
                                return "True";
                        }
                    }

                    value = value.Replace("/", "");
                    str = str.Replace("/", "");

                    if ((str.StartsWith("*")) && (str.EndsWith("*")) && (str.Length == value.Length))
                    {
                        return "True";
                    }
                    if ((str.StartsWith("#")) && (str.EndsWith("#")) && (str.Length == value.Length))
                    {
                        return "True";
                    }
                    return "False" + str;
                }
                return "True";
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Unable to set value", e);
                throw new Exception("Unable to set value. Value - " + value);
            }
        }

        /// <summary>
        /// This method is used to select the
        /// </summary>
        /// <param name="element"></param>
        /// <param name="val"></param>
        private void SetCombobValueByUIA(AutomationElement element, string val)
        {
            try
            {
                ExpandCollapsePattern exPat = element.GetCurrentPattern(ExpandCollapsePattern.Pattern)
                                                                              as ExpandCollapsePattern;

                if (exPat == null)
                {
                    throw new ApplicationException("Unable to set value");
                }

                exPat.Expand();

                AutomationElement itemToSelect = element.FindFirst(TreeScope.Descendants, new
                                      PropertyCondition(AutomationElement.NameProperty, val));

                SelectionItemPattern sPat = itemToSelect.GetCurrentPattern(
                                                          SelectionItemPattern.Pattern) as SelectionItemPattern;
                sPat.Select();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "In Combo Box Exception vp is null::" + ex.Message);
                throw new Exception("Element doesn't support ValuePattern.Pattern, make sure locator is finding the correct element");
            }
        }

        /// <summary>
        /// This method is used to expand the combobox
        /// </summary>
        /// <param name="element"></param>
        public override bool ExpandComboboxByUIA(object element)
        {
            bool isExpanded = false;
            try
            {
                AutomationElement autoElement = (AutomationElement)element;
                ExpandCollapsePattern exPat = autoElement.GetCurrentPattern(ExpandCollapsePattern.Pattern)
                                                                              as ExpandCollapsePattern;

                if (exPat == null)
                {
                    throw new ApplicationException("Unable to expand the combobox");
                }

                exPat.Expand();
                isExpanded = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "In Combo Box Exception vp is null::" + ex.Message);
                throw new Exception("Element doesn't support Expand method, make sure locator is finding the correct element");
            }
            return isExpanded;
        }

        private bool ComparePBActualExpected(string actual,string exp)
        {
            return false;

        }
        private AutomationElement ExpandTreeNode(AutomationElement node, string nodeName)
        {
            object ecp;
            string IsTreeViewExpanded = string.Empty;
            while (node != null && !taskFinished)
            {
                if (node.Current.IsOffscreen)
                {
                    node.TryGetCurrentPattern(ScrollItemPattern.Pattern, out ecp);
                    if (ecp != null)
                        ((ScrollItemPattern)ecp).ScrollIntoView();
                }
                if (node.Current.Name.Contains(nodeName))
                {
                    Thread.Sleep(1000);
                    IsTreeViewExpanded = (node.GetCurrentPropertyValue(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty).ToString());
                    Reporter.ToLog(eLogLevel.DEBUG , "ExpandTreeNode::" + node.Current.Name + " IsTreeViewExpanded::" + IsTreeViewExpanded);
                    if (General.CompareStringsIgnoreCase(IsTreeViewExpanded, "Collapsed"))
                    {
                        node.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out ecp);
                        ((ExpandCollapsePattern)ecp).Expand();                        
                    }                    
                    return node;                    
                }
                node = TreeWalker.RawViewWalker.GetNextSibling(node);
            }
            Reporter.ToLog(eLogLevel.DEBUG, nodeName + " not found in ExpandTreeNode");
            return null;
        }

        private  bool AddChildToSelection(AutomationElement node, string childToSelect)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "AddChildToSelection::" + childToSelect);
            object sip = null;
            AutomationElement orgElement;
            bool nodeFound = false;

            string sMultiSelect = "Single";
            bool bExact = true;
            if (childToSelect == "*")
                sMultiSelect = "All";
            else if (childToSelect.StartsWith("*") && childToSelect.EndsWith("*"))
            {
                sMultiSelect = "Multi";
                childToSelect = childToSelect.Substring(1, childToSelect.Length - 2);
                bExact = false;
            }
            else if (childToSelect.EndsWith("*"))
            {
                bExact = false;
                childToSelect = childToSelect.Substring(0, childToSelect.Length - 1);
            }
            while (node != null && !taskFinished && !String.IsNullOrEmpty(childToSelect))
            {   
                Reporter.ToLog(eLogLevel.DEBUG, childToSelect + " sMultiSelect::" + sMultiSelect);
                Reporter.ToLog(eLogLevel.DEBUG, "Node Found::" + node.Current.Name);                                
                bool isMultiSelect = (bool)node.GetCurrentPropertyValue(SelectionPatternIdentifiers.CanSelectMultipleProperty);
                Reporter.ToLog(eLogLevel.DEBUG, "isMultiSelect ::" + isMultiSelect);                
                if (node.Current.IsOffscreen)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Node IsOffscreen::");
                    node.TryGetCurrentPattern(ScrollItemPattern.Pattern, out sip);
                    if (sip != null)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "ScrollItemPattern value :");
                        ((ScrollItemPattern)sip).ScrollIntoView();
                    }
                }
                if(sMultiSelect == "All")
                {
                    nodeFound = true;
                    node.TryGetCurrentPattern(SelectionItemPattern.Pattern, out sip);
                    string isChildSelected = (node.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                    Reporter.ToLog(eLogLevel.DEBUG, "isChildSelected value :" + isChildSelected);                 
                    if (isChildSelected == "False")
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Inside Add to selection All");
                        
                        if (sip != null && isMultiSelect)                         
                            ((SelectionItemPattern)sip).AddToSelection();                                                                              
                        else
                        {
                            WinAPIAutomation.HoldControlKeyOfKeyboard();
                            AutomationElement nodeText= node.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "text"));
                            if(nodeText != null)
                                winAPI.SendClick(nodeText);
                            else
                                winAPI.SendClick(node);
                            WinAPIAutomation.ReleaseControlKeyOfKeyboard();
                        }
                            
                    }
                }
                else if (node.Current.Name.Contains(childToSelect))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Inside Contains :" + node.Current.Name);
                    if (bExact == false)
                        nodeFound = true;
                    else if (node.Current.Name != childToSelect && bExact == true)
                    {
                        AutomationElement nodeText = node.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "text"));
                        Reporter.ToLog(eLogLevel.DEBUG, "nodeText value :" + nodeText.Current.Name);
                        if (nodeText != null && nodeText.Current.Name == childToSelect)
                        {
                            nodeFound = true;
                        }                        
                    }
                    Reporter.ToLog(eLogLevel.DEBUG, "nodeFound :" + nodeFound);
                    if (nodeFound == true)
                    {
                        Thread.Sleep(1000);
                        node.TryGetCurrentPattern(SelectionItemPattern.Pattern, out sip);
                        string isChildSelected = (node.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                        Reporter.ToLog(eLogLevel.DEBUG, "isChildSelected value :" + isChildSelected);
                        if (isChildSelected == "True")
                            return true;
                        
                        if(sMultiSelect == "Single")
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Inside Add to selection Single");
                            if (sip != null)
                                ((SelectionItemPattern)sip).Select();
                            else
                            {
                                AutomationElement nodeText = node.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "text"));
                                if (nodeText != null)
                                    winAPI.SendClick(nodeText);
                                else
                                    winAPI.SendClick(node);
                            }
                            return true;
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Inside Add to Selection Multi");
                            if (sip != null && isMultiSelect)
                                ((SelectionItemPattern)sip).AddToSelection();
                            else
                            {
                                WinAPIAutomation.HoldControlKeyOfKeyboard();
                                AutomationElement nodeText = node.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "text"));
                                if (nodeText != null)
                                    winAPI.SendClick(nodeText);
                                else
                                    winAPI.SendClick(node);
                                WinAPIAutomation.ReleaseControlKeyOfKeyboard();
                            }

                        }                                               
                    }
                }
                orgElement = node;
                node = TreeWalker.RawViewWalker.GetNextSibling(node);
            }
            Reporter.ToLog(eLogLevel.DEBUG, "Inside Add to selection End");
            if (nodeFound == false)
            {
                Reporter.ToLog(eLogLevel.DEBUG, childToSelect + " not found in AddChildToSelection");
                return false;
            }
            else
                return true;
        }

        public override void ScrollDown(object obj)
       {
           AutomationElement element = (AutomationElement) obj;
            //Check if control is enabled 
            if (!element.Current.IsEnabled)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId.ToString()
                    + " is not enabled.\n\n");
            }

            object vp, scrollPattern;

            string _controlType = element.Current.LocalizedControlType;

            string controlType = Regex.Replace(_controlType, @"\s", "").ToLower();
            switch (controlType)
            {
                // Edit/TextBox handler
                case "scrollbar":
                    element.TryGetCurrentPattern(RangeValuePattern.Pattern, out vp);
                    ((RangeValuePattern)vp).SetValue(((RangeValuePattern)vp).Current.Value + ((RangeValuePattern)vp).Current.LargeChange);
                    break;
                case "custom":
                    string AutomationId = element.Current.AutomationId;                    
                    if (AutomationId == "Data Area")
                    {
                        int x1 = 0;
                        int y1 = 0;
                        x1 = Convert.ToInt32(element.Current.BoundingRectangle.BottomRight.X - 5);
                        y1 = Convert.ToInt32(element.Current.BoundingRectangle.BottomRight.Y - 5);

                        winAPI.SendClickOnXYPoint(element, x1, y1);
                    }
                    break;
                case "treeview":
                    element.TryGetCurrentPattern(ScrollPatternIdentifiers.Pattern, out scrollPattern);
                    if (scrollPattern != null && ((ScrollPattern)scrollPattern).Current.VerticallyScrollable == true)
                    {
                        if (((ScrollPattern)scrollPattern).Current.VerticalScrollPercent < 100)
                        {
                            ((ScrollPattern)scrollPattern).SetScrollPercent(((ScrollPattern)scrollPattern).Current.HorizontalScrollPercent, ((ScrollPattern)scrollPattern).Current.VerticalScrollPercent + 3);
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Scrollable properly is not supported for " + controlType);
                    }
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                    break;
            }
        }

       public override void ScrollUp(object obj)
       {
           AutomationElement element = (AutomationElement)obj;
                //Check if control is enabled 
                if (!element.Current.IsEnabled)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId.ToString()
                    + " is not enabled.\n\n");
            }

            object vp, scrollPattern;

            string _controlType = element.Current.LocalizedControlType;

            string controlType = Regex.Replace(_controlType, @"\s", "").ToLower();
            switch (controlType)
            {
                // Edit/TextBox handler
                case "scrollbar":
                    element.TryGetCurrentPattern(RangeValuePattern.Pattern, out vp);
                    ((RangeValuePattern)vp).SetValue(((RangeValuePattern)vp).Current.Value - ((RangeValuePattern)vp).Current.SmallChange);
                    break;
                case "custom":
                    string AutomationId = element.Current.AutomationId;
                    if (AutomationId == "Data Area")
                    {
                        int x1 = 0;
                        int y1 = 0;
                        x1 = Convert.ToInt32(element.Current.BoundingRectangle.TopRight.X - 5);
                        y1 = Convert.ToInt32(element.Current.BoundingRectangle.TopRight.Y + 5);

                        winAPI.SendClickOnXYPoint(element, x1, y1);
                    }
                    break;
                case "treeview":
                    element.TryGetCurrentPattern(ScrollPatternIdentifiers.Pattern, out scrollPattern);
                    if (scrollPattern != null && ((ScrollPattern)scrollPattern).Current.VerticallyScrollable == true)
                    {
                        if (((ScrollPattern)scrollPattern).Current.VerticalScrollPercent > 1)
                        {
                            ((ScrollPattern)scrollPattern).SetScrollPercent(((ScrollPattern)scrollPattern).Current.HorizontalScrollPercent, ((ScrollPattern)scrollPattern).Current.VerticalScrollPercent - 3);
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Scrollable properly is not supported for " + controlType);
                    }
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                    break;
            }
        }

        public override string GetControlPropertyValue(object obj, string propertyName)
        {
            AutomationElement element = (AutomationElement) obj;
            //TODO: Use Reflection for this instead of switch case to handle it in generic way
            string propValue = String.Empty;
            try
            {
                switch (propertyName)
                {
                    case "Name":
                        propValue = element.Current.Name;
                        break;
                    case "Value":
                        object tempVal = GetControlValue(element);
                        if (tempVal != null)
                            propValue = tempVal.ToString();
                        break;
                    case "AutomationId":
                        propValue = element.Current.AutomationId;
                        break;

                    case "ClassName":
                        propValue = element.Current.ClassName;
                        break;

                    case "LocalizedControlType":
                        propValue = element.Current.LocalizedControlType;
                        break;

                    case "IsPassword":
                        propValue = element.Current.IsPassword.ToString();
                        break;

                    case "ProcessId":
                        propValue = element.Current.ProcessId.ToString();
                        break;

                    case "IsEnabled":
                        propValue = element.Current.IsEnabled.ToString();
                        break;

                    case "IsKeyboardFocusable":
                        propValue = element.Current.IsKeyboardFocusable.ToString();
                        break;

                    case "IsOffScreen":
                        propValue = element.Current.IsOffscreen.ToString();
                        break;

                    case "XOffset":
                        propValue = element.Current.BoundingRectangle.X.ToString();
                        break;

                    case "YOffset":
                        propValue = element.Current.BoundingRectangle.Y.ToString();
                        break;

                    case "NativeWindowHandle":
                        propValue = element.Current.NativeWindowHandle.ToString();
                        break;
                    case "ToggleState":
                        TogglePattern togPattern;
                        Object objPattern;
                        if (true == element.TryGetCurrentPattern(TogglePattern.Pattern, out objPattern))
                        {
                            togPattern = (TogglePattern)objPattern;
                            propValue = togPattern.Current.ToggleState.ToString();
                        }
                        break;
                    case "IsSelected":
                        object selectionItemPattern;
                        if (true == element.TryGetCurrentPattern(SelectionItemPattern.Pattern, out selectionItemPattern))
                        {
                            if (!ReferenceEquals(selectionItemPattern, null))
                            {
                                propValue = ((SelectionItemPattern)selectionItemPattern).Current.IsSelected.ToString();
                            }
                        }
                        break;
                    case "Text":
                        object valPattern;
                        if (true == element.TryGetCurrentPattern(ValuePattern.Pattern, out valPattern))
                        {
                            if (!ReferenceEquals(valPattern, null))
                            {
                                propValue = ((ValuePattern)valPattern).Current.Value;
                            }
                        }
                        break;
                    case "XPATH":
                        propValue = GetElementAbsoluteXPath(element);
                        break;
                    default:
                        propValue = "Retrieve value for this Property is not Supported";
                        break;
                }
            }
            catch (COMException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetControlPropertyValue", e);
                throw e;

            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetControlPropertyValue", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetControlPropertyValue", e);
                throw e;
            } 
           
            return propValue;
        }

        public override String GetControlText(object obj, string XY="")
        {
            AutomationElement element = (AutomationElement)obj;

            string val = string.Empty;

            try
            {
                if (element.Current.IsOffscreen == false)
                {
                    ClearClipboardText();
                    DoRightClick(obj,XY);
                    Thread.Sleep(200);
                    CurrentWindow.SetFocus();
                    System.Windows.Forms.SendKeys.SendWait("c");
                    val = GetClipboardText();
                    ClearClipboardText();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetControlText", ex);
            }

            return val;
        }

        private static string GetClipboardText()
        {
            string clipboardText = "";
            bool bDone = false;

            var t = new Thread(() =>
            {
                try
                {
                    clipboardText = Clipboard.GetText(TextDataFormat.Text);
                    bDone = true;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetClipboardText", ex);
                    bDone = true;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (bDone == false)
            {
                Thread.Sleep(100);
            }
            return clipboardText;
        }
        
        private static void ClearClipboardText()
        {
            bool bDone = false;
            var t = new Thread(() =>
            {
                try
                {                    
                    Clipboard.Clear();
                    bDone = true;
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetClipboardText", ex);
                    bDone = true;
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            while (bDone == false)
            {
                Thread.Sleep(100);
            }
        }
        
        /// <summary>
        /// This method will try to get the value from child control from the Automation element, basically it is used to get the value from Combobox control
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private string GetControlValueFromChildControl(AutomationElement element)
        {
            string val = string.Empty;
            try
            {
                if (element.Current.BoundingRectangle == null || mPlatform.Equals(ePlatform.Windows))
                {
                    return val;
                }
                double xCoordinate = ((element.Current.BoundingRectangle.X + element.Current.BoundingRectangle.Width / 2));
                double yCoordinate = ((element.Current.BoundingRectangle.Y + element.Current.BoundingRectangle.Height / 2));
                element.SetFocus();
                Thread.Sleep(1000);
                winAPI.SendClickOnXYPoint(element, Convert.ToInt32(xCoordinate), Convert.ToInt32(yCoordinate));
                System.Windows.Point point = new System.Windows.Point(Convert.ToInt32(xCoordinate), Convert.ToInt32(yCoordinate));
                var newElement = AutomationElement.FromPoint(point);
                WinAPIAutomation.SendTabKey();
                if (newElement == null)
                {
                    return "";
                }
                element = newElement;

                if (element.Current.LocalizedControlType.Equals("pane"))
                {
                    if (!String.IsNullOrEmpty(element.Current.Name))
                    {
                        return element.Current.Name;
                    }

                }
                val = GetElementValueByValuePattern(element);
                if (String.IsNullOrEmpty(val))
                {
                    val = GetElementValueByLegacyIAccessiblePattern(element);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetControlValueFromXYCoordinates", ex);
            }

            return val;
        }

        /// <summary>
        /// This method will try to get the value from child control from the Automation element, basically it is used to get the value by creating the image and fetching text from the image
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override String GetControlValue(object obj)
        {
            AutomationElement element = (AutomationElement) obj;
            object vp;
            string ControlType = element.Current.LocalizedControlType.ToString();
            if (General.CompareStringsIgnoreCase(ControlType ,"Edit Box" )||General.CompareStringsIgnoreCase(ControlType, "edit")||
                General.CompareStringsIgnoreCase(ControlType, "item")|| General.CompareStringsIgnoreCase(ControlType, ""))
            {
                string elementValue = GetElementValueByValuePattern(element);

                if (String.IsNullOrEmpty(elementValue))
                {
                    elementValue = GetElementValueByLegacyIAccessiblePattern(element);
                }
                if (String.IsNullOrEmpty(elementValue))
                {
                    elementValue = GetElementValueByTextpattern(element);
                }
                if (string.IsNullOrEmpty(elementValue) && General.CompareStringsIgnoreCase(ControlType, ""))
                {
                    elementValue = GetControlValueFromChildControl(element);
                }
                return elementValue;
            }

            if (General.CompareStringsIgnoreCase(ControlType, "text"))
            {
                string value = GetElementValueByValuePattern(element);
                if (string.IsNullOrEmpty(value) && mPlatform.Equals(ePlatform.PowerBuilder))
                {
                    value = GetControlValueFromChildControl(element);
                }
                if (string.IsNullOrEmpty(value))
                {
                    // In some cases the name is the actual control value, weird but exist...maybe there is no developer name
                    value = element.Current.Name;
                }
                return value;
            }

            if (General.CompareStringsIgnoreCase(ControlType, "combo box"))
            {
                string value = GetControlValueFromChildControl(element);
                return value;
            }
            //-----------------------------------

            else if (General.CompareStringsIgnoreCase(ControlType, "document"))
            {
                string value = GetElementValueByTextpattern(element);
                return value;
            }
            // check box handler
            if (General.CompareStringsIgnoreCase(ControlType, "check box"))
            {
                element.TryGetCurrentPattern(TogglePattern.Pattern, out vp);

                ToggleState x = ((TogglePattern)vp).Current.ToggleState;

                if (x == ToggleState.On)
                {
                    return "Checked";
                }

                if (x == ToggleState.Off)
                {
                    return "Unchecked";
                }

                return "Unknown Toggle Status " + x.ToString();
            }

            else if (General.CompareStringsIgnoreCase(ControlType, "dialog"))
            {
                element = TreeWalker.RawViewWalker.GetFirstChild(element);
                do
                {
                    if (General.CompareStringsIgnoreCase(element.Current.LocalizedControlType.ToString(), "text"))
                    {
                        return element.Current.Name;
                    }
                    element = TreeWalker.RawViewWalker.GetNextSibling(element);
                } while (element != null && !taskFinished);

            }
            if (General.CompareStringsIgnoreCase(ControlType, "title bar"))
            {
                string value = element.Current.Name.ToString();
                return value;
            }

            //DatePicker
            if (General.CompareStringsIgnoreCase(ControlType, "pane"))
            {
                //We have query on below DatePicker Control as the Pattern is not supporting.
                if (General.CompareStringsIgnoreCase(ControlType, "SysDateTimePick32"))
                {
                    string str = GetElementValueByValuePattern(element);
                    return str;
                }
                else if (General.CompareStringsIgnoreCase(ControlType, "pbdw126"))
                {
                    AutomationElement gridElem = AutomationElement.RootElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.AutomationIdProperty, "BackFeedDataGridControl"));
                    GridPattern gridPattern = (GridPattern)gridElem.GetCurrentPattern(GridPattern.Pattern);
                    var item = gridPattern.GetItem(2, 0);
                    ValuePattern valPat = (ValuePattern)item.GetCurrentPattern(ValuePattern.Pattern);
                }
            }
            else if (General.CompareStringsIgnoreCase(ControlType, "list"))
            {
                return this.GetAllChildElements(element);
            }
            else if (General.CompareStringsIgnoreCase(ControlType, "menu bar"))
            {
                return this.GetAllChildElements(element);
            }
            if (General.CompareStringsIgnoreCase(ControlType, "button"))
            {
                string value = element.Current.Name;
                return value;
            }
            else if (General.CompareStringsIgnoreCase(ControlType, "radio button"))
            {
                return element.Current.Name;
            }

            //TODO: throw error
            return "Unknown Control type - cannot get value";
        }

        public override string GetControlFieldValue(object obj, string value)
        {
            AutomationElement element = (AutomationElement) obj;
            AutomationElement fieldValueElement;
            PropertyCondition conditionMatchMenu = new PropertyCondition(AutomationElement.NameProperty, value);
            AutomationElement fieldNameElement = element.FindFirst(TreeScope.Descendants, conditionMatchMenu);
            fieldValueElement = TreeWalker.RawViewWalker.GetNextSibling(fieldNameElement);
            return fieldValueElement.Current.Name;
        }
        
       public override string ToggleControlValue(object obj)
       {
           AutomationElement element = (AutomationElement) obj;
            //Check if control is enabled 
            if (element == null)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId.ToString()
                    + " is not enabled.\n\n");
            }

            object vp;

            string controlType = element.Current.LocalizedControlType;

            switch (controlType)
            {
                // check box handler
                case "check box":
                case "tree item":
                    element.TryGetCurrentPattern(TogglePattern.Pattern, out vp);
                    ToggleState x = ((TogglePattern)vp).Current.ToggleState;
                    if (x == ToggleState.Off)
                    {
                        SetControlValue(element, "Checked");
                        return "Checked";
                    }

                    if (x == ToggleState.On)
                    {
                        SetControlValue(element, "Unchecked");
                        return "Unchecked";
                    }
                    break;

                default:
                    Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                    break;
            }
            return "not found";
        }

        public override string IsEnabledControl(object obj)
        {
            AutomationElement element = (AutomationElement) obj;
            //Check if control is enabled 
            if (element == null)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId.ToString()
                    + " Cannot find its value.\n\n");
            }
            
            string controlType = element.Current.LocalizedControlType;

            switch (controlType)
            {
                // check box handler
                case "check box":
                    if (element.Current.IsEnabled)
                    {
                        return "true";
                    }

                    else
                    {
                        return "false";
                    }
                case "button":
                    if (element.Current.IsEnabled)
                    {
                        return "true";
                    }

                    else
                    {
                        return "false";
                    }

                default:
                    Reporter.ToUser(eUserMsgKey.ActionNotImplemented, controlType);
                    break;

            }
            return "not found";
        }

        public override string GetSelectedItem(object obj)
        {

            AutomationElement element = (AutomationElement)obj;

            bool isMultiSelect = (bool)element.GetCurrentPropertyValue(SelectionPatternIdentifiers.CanSelectMultipleProperty);
            if (isMultiSelect)
            {
                return this.GetSelectedItems(element);
            }
            else
            {
                object vp;
                element.TryGetCurrentPattern(ValuePattern.Pattern, out vp);
                if (vp != null)
                {
                    return GetElementValueByValuePattern(element);

                }
                else
                {
                    AutomationElement elementNode = TreeWalker.RawViewWalker.GetFirstChild(element);
                    string isItemSelected;
                    while (elementNode != null && !taskFinished)
                    {
                        isItemSelected = (elementNode.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                        if (isItemSelected == "True")
                        {
                            return elementNode.Current.Name;
                        }
                        elementNode = TreeWalker.RawViewWalker.GetNextSibling(elementNode);
                    }
                    return "No Item selected";
                }
            }

        }

        private string GetSelectedItems(AutomationElement element)
        {
            AutomationElement elementNode = TreeWalker.RawViewWalker.GetFirstChild(element);
            String ListItems = null;
            do
            {
                string isItemSelected = (elementNode.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty)).ToString();
                if (ListItems == null && isItemSelected == "True")
                {
                    ListItems = elementNode.Current.Name;
                }
                else if (isItemSelected == "True")
                {
                    ListItems += "," + elementNode.Current.Name;
                }
                
                elementNode = TreeWalker.RawViewWalker.GetNextSibling(elementNode);
            } while (elementNode != null && !taskFinished);
            return ListItems;
        }

        public override string GetDialogTitle(object obj)
        {
            AutomationElement element = (AutomationElement) obj;

            return element.Current.Name;
        }

        private string GetAllChildElements(AutomationElement element)
        {
            AutomationElement elementNode = TreeWalker.RawViewWalker.GetFirstChild(element);
            String ListItems = "";
            do
            {
                if (ListItems.Length > 0) ListItems += ", ";

                ListItems += elementNode.Current.Name;
                elementNode = TreeWalker.RawViewWalker.GetNextSibling(elementNode);
            } while (elementNode != null && !taskFinished);
            return ListItems;
        }


        public override void ExpandControlElement(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            object vp;
            element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out vp);
            if (vp != null)
            {
                ((ExpandCollapsePattern)vp).Expand();
            }
            else
            {
                winAPI.SendClick(element);
            }
        }

        public override void CollapseControlElement(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            Reporter.ToLog(eLogLevel.DEBUG, "Collapsing element  :  "+element.Current.Name);
            object vp;
            string status = "";
            element.TryGetCurrentPattern(ExpandCollapsePattern.Pattern, out vp);            
            if (vp != null)
            {
                status = ((ExpandCollapsePattern)vp).Current.ExpandCollapseState.ToString();
                if (status.Equals("Expanded"))
                {
                    ((ExpandCollapsePattern)vp).Collapse();
                }
                else
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "element  :  "+element.Current.Name+" is already collapsed");

                }
            }
        }
        

        public override string IsControlSelected(object obj)
        {
            AutomationElement element=(AutomationElement)obj;
            string isControlSelected = element.GetCurrentPropertyValue(SelectionItemPatternIdentifiers.IsSelectedProperty).ToString();
            return isControlSelected;
        }
        
        private const int WmPaint = 0x000F;

        [DllImport("User32.dll")]
        public static extern Int64 SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        
        public static void ForcePaint(IntPtr hWnd)
        {
            SendMessage(hWnd, WmPaint, IntPtr.Zero, IntPtr.Zero);
        }

        public override void HandlePaintWindow(object obj)
        {
            PaintWindow((IntPtr)((AutomationElement)obj).Current.NativeWindowHandle);
        }

        private void waitForElementState(AutomationElement ae)
        {
            int itry = 0; if (loadwaitSeconds <= 0) loadwaitSeconds = 10;
            while (loadwaitSeconds >= itry && ae.Current.ItemStatus == "Busy")
            {
                System.Threading.Thread.Sleep(1000);
            }
        }
        public void setFocus(AutomationElement ae)
        {
            try { ae.SetFocus(); } //TODO set window focus using runtime services. http://blog.coderdowhat.com/2010/02/automationelementsetfocus-is-unreliable.html
            catch { }
        }
        
        public override void SmartSwitchWindow(ActSwitchWindow actSW)
        {
            Stopwatch St = new Stopwatch();
            St.Reset();
            St.Start();

            Boolean switchDoneFlag = false;
            String windowTitle = actSW.LocateValueCalculated;
            while (!switchDoneFlag && !taskFinished)
            {
                if (actSW.WaitTime == -1)
                    actSW.WaitTime = mLoadTimeOut.Value;
                switchDoneFlag = SwitchToWindow(windowTitle);

                if (St.ElapsedMilliseconds > actSW.WaitTime * 1000)
                {
                    break;
                }
            }

            if (!switchDoneFlag)
                actSW.Error += "Window with title-" + windowTitle + " not found within specified time";
        }

        public override void ActUISwitchWindow(Act act)
        {
            ActUIElement actUIElement = (ActUIElement)act;
            
            Stopwatch St = new Stopwatch();
            St.Reset();
            St.Start();

            bool switchDoneFlag = false;
            string windowTitle = actUIElement.ElementLocateValue;

            var syncTime = Convert.ToInt32(actUIElement.GetInputParamCalculatedValue(ActUIElement.Fields.SyncTime));
            while (!switchDoneFlag && !taskFinished)
            {
                if (syncTime <= 0)
                {
                    syncTime = mLoadTimeOut.Value;
                }
                switchDoneFlag = SwitchToWindow(windowTitle);

                if (St.ElapsedMilliseconds > syncTime * 1000)
                {
                    break;
                }
            }

            if (!switchDoneFlag)
                actUIElement.Error += "Window with title-" + windowTitle + " not found within specified time";
        }

        public override void SwitchWindow(ActUIASwitchWindow act)
        {
            bool isLoaded = false;
            int count = 0;
            while (!isLoaded && !taskFinished)
            {
                // TODO: add param to getAppwindows to return the selected window when found or create func to get one window faster
                List<object> AppWindows = GetListOfWindows();
                string WindowTitle = "";
                string WindowClassName = "";
                try
                {
                    bool isFound = false;
                    foreach (AutomationElement window in AppWindows)
                    {
                        // TODO: handle ByTitle, ByName etc...
                        WindowTitle = GetWindowInfo(window);
                        WindowClassName = GetControlPropertyValue(window, "ClassName");
                        if ((String.IsNullOrEmpty(WindowTitle)) && (!String.IsNullOrEmpty(WindowClassName)))
                        {
                            if (WindowClassName.StartsWith("FNW") && act.LocateValueCalculated == "NoTitleWindow")
                            {
                                if (IsWindowValid(window))
                                {
                                    CurrentWindow = window;
                                    UpdateRootElement();
                                    WinAPIAutomation.ShowWindow(CurrentWindow);
                                    act.ExInfo = "Switched to Window -with no title";
                                    isFound = true;
                                    CurrentWindowName = CurrentWindow.Current.Name;
                                    break; 
                                }
                            }
                        }
                    }

                    if (!isFound)
                    {
                        foreach (AutomationElement window in AppWindows)
                        {
                            WindowTitle = GetWindowInfo(window);
                            if (!String.IsNullOrEmpty(WindowTitle))
                            {
                                if (WindowTitle.Equals(act.LocateValueCalculated))
                                {
                                    if (IsWindowValid(window))
                                    {
                                        CurrentWindow = window;
                                        UpdateRootElement();
                                        WinAPIAutomation.ShowWindow(CurrentWindow);
                                        act.ExInfo = "Switched to Window - " + act.LocateValueCalculated;
                                        isFound = true;
                                        CurrentWindowName = CurrentWindow.Current.Name;
                                        break; 
                                    }
                                }
                            }
                        } 
                    }

                    if (!isFound)
                    {
                        foreach (AutomationElement window in AppWindows)
                        {
                            WindowTitle = GetWindowInfo(window);
                            if (!String.IsNullOrEmpty(WindowTitle))
                            {
                                if (WindowTitle.Contains(act.LocateValueCalculated))
                                {
                                    if (IsWindowValid(window))
                                    {
                                        CurrentWindow = window;
                                        UpdateRootElement();
                                        WinAPIAutomation.ShowWindow(CurrentWindow);
                                        act.ExInfo = "Switched to Window - " + act.LocateValueCalculated;
                                        isFound = true;
                                        CurrentWindowName = CurrentWindow.Current.Name;
                                        break; 
                                    }
                                }
                            }
                        } 
                    }
                    if (!isFound && !taskFinished)
                        continue;
                    if (!isFound)
                    {
                        //TODO: below is not correct condition.WindowTitle is title of window from last iteration of for loop
                        if (String.IsNullOrEmpty(WindowTitle))
                        {
                            act.Error = "Missing window title to search for, in case not title window is required then please Provide Window Title 'NoTitleWindow'";
                        }
                        else
                        {
                            act.Error = "Please Provide Window Title in Locator Value and Please select  locate By Title";
                        } 
                    }
                   
                    isLoaded = true;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error in SwitchWindow", ex);
                    isLoaded = false;
                    if (count == 6)
                    {                        
                        throw ex;
                    }
                }
                count++;
            }
        }
        
        public override bool HandleSwitchWindow(eLocateBy locateBy, string locateValue)
        {
            if (locateBy == eLocateBy.ByXPath)
            {
                object AE = FindElementByLocator(locateBy, locateValue);

                if (IsWindowValid(AE))
                {
                    CurrentWindow = (AutomationElement)AE;
                    CurrentWindowName = CurrentWindow.Current.Name;
                    UpdateRootElement();
                    return true; 
                }
            }
            
            return SwitchToWindow(locateValue);
            
        }

        public override bool SwitchToWindow(String Title)
        {
            int count = 0;
            bool isLoaded = false;
            taskFinished = false;
            while (!isLoaded && !taskFinished)
            {
                try
                {
                    // TODO: add param to getAppwindows to return the selected window when found or create func to get one window faster          
                    List<AppWindow> AppWindows = GetListOfDriverAppWindows();

                    foreach (AppWindow aw in AppWindows)
                    {
                        AutomationElement window = (AutomationElement)((UIAElementInfo)aw.RefObject).ElementObject;
                        // TODO: handle ByTitle, ByName etc...
                        string WindowTitle = aw.Title;
                        string WindowClassName = GetControlPropertyValue(window, "ClassName");

                        if (WindowTitle == "" && !String.IsNullOrEmpty(WindowClassName) && WindowClassName.StartsWith("FNW") && Title == "NoTitleWindow")
                        {
                            if (IsWindowValid(window))
                            {
                                CurrentWindow = window;
                                UpdateRootElement();
                                isLoaded = true;
                                CurrentWindowName = CurrentWindow.Current.Name;
                                break; 
                            }
                        }
                        else if (WindowTitle == "")
                        {
                            //TODO: Currently we handle only for PDF Windows with blank Name. 
                            //Also if more than one pdf windows are open then process for both of them will same
                            //Need to handle this scenario
                            Process[] listProcesses = Process.GetProcessesByName("AcroRd32");
                            
                            foreach (Process p in listProcesses)
                            {
                                if (p.MainWindowTitle.Contains(Title))
                                {
                                    AutomationElement AE = AutomationElement.RootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ProcessIdProperty, p.Id));
                                    if (IsWindowValid(AE))
                                    {
                                        CurrentWindow = AE;
                                        UpdateRootElement();
                                        isLoaded = true;
                                        CurrentWindowName = AE.Current.Name;
                                        break; 
                                    }
                                }
                            }
                        }
                        else if (WindowTitle != null && WindowTitle.Contains(Title))
                        {
                            if (IsWindowValid(window))
                            {
                                CurrentWindow = window;
                                UpdateRootElement();
                                isLoaded = true;                                
                                CurrentWindowName = GetWindowInfo(window);                                
                                break; 
                            }
                        }
                    }
                    break;
                }                
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error in SwitchToWindow", ex);
                    isLoaded = false;
                    if (count == 6)
                    {                       
                        throw ex;
                    }
                }
                count++;
            }
            return isLoaded;
        }
        
        public override void TakeScreenShot(ActScreenShot act)
        {
            try
            {
                //TODO: Implement Multi window capture
                //not in use
                if (act.WindowsToCapture == Act.eWindowsToCapture.AllAvailableWindows)
                {
                    //FIXME                    
                }
                else
                {

                    Bitmap tempBmp = GetCurrentWindowBitmap();
                    act.AddScreenShot(tempBmp);
                }
                return;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to create UIAutomation application window screenshot", ex);
            }
        }

        //TODO: Move to WinApiAutomation
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);

        public override Bitmap GetCurrentWindowBitmap()
        {            
            Bitmap bmp = WindowToBitmap(CurrentWindow);
            return bmp;
        }

        public List<AppWindow> GetCurrentAppWindows()
        {
            AutomationElementCollection AECollection = AutomationElement.RootElement.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ProcessIdProperty, CurrentWindow.Current.ProcessId));
            List<AppWindow> list = new List<AppWindow>();
            for (int i = 0; i < AECollection.Count; i++)
            {
                AppWindow AW = new AppWindow();

                UIAElementInfo WEI = new UIAElementInfo();
                WEI.ElementObject = AECollection[i];
                AW.RefObject = WEI;

                list.Add(AW);
            }
            return list;

        }
        
        public override Bitmap GetAppWindowAsBitmap(AppWindow aw)  //******************        
        {
            AutomationElement tempWindow = (AutomationElement)((UIAElementInfo)aw.RefObject).ElementObject;
            Bitmap bmp = WindowToBitmap(tempWindow);
            return bmp;
        }

        public override List<Bitmap> GetAppDialogAsBitmap(AppWindow aw)  ///********
        {
            List<Bitmap> bList = new List<Bitmap>();
            AutomationElement tempAE = (AutomationElement)(((UIAElementInfo)aw.RefObject).ElementObject);
            //TODO: Add function for reduntnt code
            AutomationElementCollection AEList;
            AutomationElementCollection AEList1;
            AutomationElementCollection AEListWindows;

            PropertyCondition condDialog = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "Dialog");
            PropertyCondition condDialog1 = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "dialog");
            PropertyCondition condWindow = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "window");
            AEList = tempAE.FindAll(TreeScope.Descendants, condDialog);
            AEList1 = tempAE.FindAll(TreeScope.Descendants, condDialog1);
            AEListWindows = tempAE.FindAll(TreeScope.Descendants, condWindow);

            // adding screenshot of popup which present in main window
            for (int i = 0; i < AEList.Count; i++)
            {
                AutomationElement ele = AEList[i];
                Bitmap bmp = WindowToBitmap(ele);
                bList.Add(bmp);
            }
            for (int i = 0; i < AEList1.Count; i++)
            {
                AutomationElement ele = AEList1[i];
                Bitmap bmp = WindowToBitmap(ele);
                bList.Add(bmp);                
            }

            for (int i = 0; i < AEListWindows.Count; i++)
            {
                List<Bitmap> winPopup = new List<Bitmap>();
                if (AEListWindows[i].Current.BoundingRectangle.X != 0 && AEListWindows[i].Current.BoundingRectangle.Y != 0)
                {
                    Bitmap bmp = WindowToBitmap(AEListWindows[i]);
                    winPopup.Add(bmp); //adding screenshot of element type "window"
                }
                bList.AddRange(winPopup);
            }

            return bList;
        }

        public override bool IsWindowValid(object obj)
        {
            if (obj!=null && obj.GetType().Equals(typeof(AutomationElement)))
            {
                AutomationElement element = (AutomationElement)obj;
                try
                {
                    string name = element.Current.Name;
                    if (element.Current.ProcessId == 0)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Exception when checking IsWindowValid with process Id as zero");
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception when checking IsWindowValid", e);
                    return false;
                }
            }            
            return true;

        }

        public override string GetWindowInfo(object obj)
        {
            AutomationElement window = (AutomationElement) obj;
            string locVal = String.Empty;
            try
            {
                locVal = getAEProperty(window, AutomationElement.NameProperty);            
                if (!IsWindowValid(obj))
                    return "";
                
                if (string.IsNullOrEmpty(locVal) && !string.IsNullOrEmpty(window.Current.AutomationId))
                {                                        
                    locVal = window.Current.LocalizedControlType;
                    locVal += " (" + window.Current.AutomationId + ")";                    
                }                
            }
            catch (COMException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetWindowInfo", e);
                throw e;

            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetWindowInfo", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetWindowInfo", e);
                throw e;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetWindowInfo", e);
                throw e;
            }
            return locVal;
        }
        
        public AutomationElement GetLastFocusedElement()
        {
            return lastFocusedElement;
        }
        
        //Will get all visible control including recursive drill down, for AE which have invoke method
        public override List<ElementInfo> GetVisibleControls()
        {
            List<ElementInfo> list = new List<ElementInfo>();
            List<ElementInfo> HTMLlist = new List<ElementInfo>();

            //TODO: find a better property - since if the window is off screen controls will not show            
            System.Windows.Automation.Condition cond = new PropertyCondition(AutomationElement.IsOffscreenProperty, false);                        
            AutomationElementCollection AEC = CurrentWindow.FindAll(TreeScope.Descendants, cond);
            string IEElementXpath="";

            foreach (AutomationElement AE in AEC) 
            {
                UIAElementInfo ei = (UIAElementInfo)GetElementInfoFor(AE);
                if (AE.Current.ClassName.Equals("Internet Explorer_Server"))
                {
                        ei = (UIAElementInfo)GetElementInfoFor(AE);
                    IEElementXpath = ei.XPath;
                    InitializeBrowser(AE);
                    HTMLlist = HTMLhelperObj.GetVisibleElement();
                    list.Add(ei);
                    foreach(ElementInfo e1 in HTMLlist)
                    {
                        list.Add(e1);
                    }
                }
                

                if (String.IsNullOrEmpty(IEElementXpath))
                {
                    list.Add(ei);
                }
                else if (!ei.XPath.Contains(IEElementXpath))
                {
                    //TODO: Here we check if automation element is child of IE browser element 
                    // If yes then we skip it because we already have HTML element for this
                    // Checking it by XPath makes it slow , because xpath is calculated for this element at runtime
                    // Need to find a better way to speed up
                    list.Add(ei);
                }
                
            }

            return list;
        }
              
        public override string InitializeBrowser(object obj)
        {
            AutomationElement AE = (AutomationElement)obj;
            string result = "";
                    SHDocVw.InternetExplorer internetExplorer = winAPI.GetIEFromAutomationelement(AE);
                    HTMLhelperObj = new HTMLHelper(internetExplorer,AE);
            result = internetExplorer == null ? "false" : "true";
            return result;
        }
        
        public override string GetElementAbsoluteXPath(object obj)
        {
            UIAElementInfo EI = new UIAElementInfo
            {
                ElementObject = obj,
                WindowExplorer = WindowExplorer
            }; //Create small simple EI
            string XPath = mXPathHelper.GetElementXpathAbsulote(EI);
            return XPath;

        }

        public static string OLD_GetElementXPath_OLD(AutomationElement AE)
        {

            TreeWalker walker = TreeWalker.ControlViewWalker;
            AutomationElement elementParent;
            AutomationElement node = AE;

            string xPath = "";
            do
            {
                elementParent = walker.GetParent(node);
                if (elementParent != AutomationElement.RootElement)
                {
                    if (xPath.Length > 0) xPath = @"\" + xPath;

                    if (!String.IsNullOrEmpty(node.Current.Name))
                    {
                        //Check if we have only one node with this name

                        System.Windows.Automation.Condition c2 = new PropertyCondition(AutomationElement.NameProperty, node.Current.Name);

                        AutomationElementCollection AEC = elementParent.FindAll(TreeScope.Children, c2);
                        string sOKName = node.Current.Name;

                        if (sOKName.Contains(@"\")) // Part of XPath will mess in parsing
                        {
                            sOKName = sOKName.Replace(@"\", "~~~");
                            // sOKName = "ZZZZZZZZZZZZZZZZZZZ - " + sOKName;
                        }
                        if (AEC.Count == 1)
                        {
                            xPath = sOKName + xPath;
                        }
                        else
                        {
                            int count = 0;
                            foreach (AutomationElement AE2 in AEC)
                            {
                                if (AE2 == AE)
                                {
                                    break;
                                }
                                count++;
                            }

                            xPath = sOKName + "[" + count + "]" + xPath;
                        }

                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(node.Current.AutomationId))
                        {
                            xPath = "[AutomationId:" + node.Current.AutomationId + "]" + xPath;
                        }
                        else
                        {
                            xPath = OLD_GetElemClassIndex_OLD(node) + xPath;
                        }
                    }
                    node = elementParent;
                }
                else
                {
                    break;
                }
            }
            while (true);

            return xPath;
        }

        private static string OLD_GetElemClassIndex_OLD(AutomationElement node)
        {
            // when we don't have Name or Automation ID, we do class[index] , for example: class:pane[1]
            string type = node.Current.LocalizedControlType;
            string path = "[LocalizedControlType:" + type;


            //Get parent
            AutomationElement parent = TreeWalker.ContentViewWalker.GetParent(node);

            //find all child with same LocalizedControlType, count where is our node located = index
            System.Windows.Automation.Condition cond = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, type);
            AutomationElementCollection elems = parent.FindAll(TreeScope.Children, cond);

            if (elems.Count == 1)  // only one item no need for index
            {
                path += "]";
            }
            else
            {
                int i = 0;
                foreach (AutomationElement AE in elems)
                {
                    if (AE == node) break;
                    i++;
                }
                path += "[" + i + "]]";
            }
            return path; 
        }
        
        public override object GetElementData(object obj)
        {
            AutomationElement AE = (AutomationElement) obj;
            string type = AE.Current.LocalizedControlType;


            if (type == "combo box")
            {
                return GetComboValues(AE);
            }
            if(type=="pane" && AE.Current.ClassName.StartsWith("pbd"))
            {
                TableElementInfo TBI = new TableElementInfo();
                TBI = GetTableElementInfoForGrid(AE);


                if (parentDictionary.Count >= 0)
                {
                    if (!parentDictionary.ContainsKey(AE))
                    {
                        parentDictionary.Add(AE, TBI.MainDict);
                    }
                }

                return TBI;
            }

            //TODO: handle table and other 

            return null;
        }

        public AutomationElement[] GetColumnCollectionByColNum(int colNumber, AutomationElement AE)
        {
            int maxcount = GetRowCountforGrid(AE);
            int tempcount, CurrentColNum = 0;
            AutomationElement[] AECollection;
            List<string> keys = MainDict.Keys.ToList();
            foreach (string str in keys)
            {
                AECollection = MainDict[str];
                tempcount = AECollection.Count();

                if (tempcount == maxcount)
                {
                    if (colNumber == CurrentColNum)
                    {
                        return AECollection;

                    }
                    CurrentColNum++;
                }
            }
            return null;
        }

        public int GetRowNumFromCollection(AutomationElement[] AECollection, ActTableElement.eRunColPropertyValue propVal, ActTableElement.eRunColOperator WhereOperator, string value)
        {
            AutomationElement targetElement;
            string controlValue="";
            if (ActTableElement.eRunColOperator.Equals == WhereOperator)
            {
                for (int i = 0; i < AECollection.Count(); i++)
                {
                    targetElement = AECollection[i];
                    if (propVal == ActTableElement.eRunColPropertyValue.Text)
                        controlValue = GetControlText(targetElement);
                    else if (propVal == ActTableElement.eRunColPropertyValue.Value)
                        controlValue = GetControlValue(targetElement);

                    if (value.Equals(controlValue))
                    {
                        return i;
                    }
                }
            }

            if (ActTableElement.eRunColOperator.Contains == WhereOperator)
            {
                for (int i = 0; i < AECollection.Count(); i++)
                {
                    targetElement = AECollection[i];
                    if (propVal == ActTableElement.eRunColPropertyValue.Text)
                        controlValue = GetControlText(targetElement);
                    else if (propVal == ActTableElement.eRunColPropertyValue.Value)
                        controlValue = GetControlValue(targetElement);

                    if (controlValue.Contains(value))
                    {
                        return i;
                    }
                }
            }

            throw new Exception("Given Row Value " + value + " is not present in Grid");
        }

        public Dictionary<string, AutomationElement[]> GetUpdateDictionary(AutomationElement AE, ref bool isElementsFromPoints)
        {
            Dictionary<string, AutomationElement[]> MainDict = new Dictionary<string, AutomationElement[]>();
            MainDict = GetDictionaryForGrid(AE, ref isElementsFromPoints);
            parentDictionary.Remove(AE);
            parentDictionary.Add(AE, MainDict);
            return MainDict;
        }

        public AutomationElement[] GetColumnCollection(ActTableElement.eRunColSelectorValue ColumnSelectorValue, string ColValue, AutomationElement AE)
        {
            AutomationElement[] AEColl = null;
            int ColNumber, CurrentRowCount;

            switch (ColumnSelectorValue)
            {
                case ActTableElement.eRunColSelectorValue.ColNum:

                    ColNumber = Convert.ToInt32(ColValue);
                    AEColl = GetColumnCollectionByColNum(ColNumber, AE);

                    if (AEColl == null)
                    {
                        throw new Exception("Input Column Number " + ColValue + " not present in Grid");

                    }
                    CurrentRowCount = GetCurrentCountForGrid(AE, AEColl[0]);
                    if (CurrentRowCount == 0)
                    {
                        throw new Exception("No any row available in Column " + ColValue);
                    }
                    //if (CurrentRowCount != AEColl.Count())
                    //{
                    //    MainDict = GetUpdateDictionary(AE);
                    //    AEColl = GetColumnCollectionByColNum(ColNumber, AE);
                    //}

                    return AEColl;

                case ActTableElement.eRunColSelectorValue.ColTitle:
                    List<string> keys = MainDict.Keys.ToList();
                    if (!keys.Contains(ColValue))
                    {
                        throw new Exception("Input Column Name " + ColValue + " not present in Grid");
                    }
                    AEColl = MainDict[ColValue];
                    if (AEColl == null)
                    {
                        throw new Exception("Input Column Name " + ColValue + " not present in Grid");
                    }

                    CurrentRowCount = GetCurrentCountForGrid(AE, AEColl[0]);
                    if (CurrentRowCount == 0)
                    {
                        throw new Exception("No any row available in Column " + ColValue);
                    }
                    //if (CurrentRowCount != AEColl.Count())
                    //{
                    //    MainDict = GetUpdateDictionary(AE);
                    //    AEColl = MainDict[ColValue];
                    //}
                    return AEColl;
            }
            return AEColl;
        }

        public override void HandleGridControlAction(ActTableElement actGrid)
        {
            bool isElementsFromPoints = false;
            object obj = GetActElement(actGrid);
            if (obj == null)
            {
                actGrid.Error = "Element not Found - " + actGrid.LocateBy + " " + actGrid.LocateValueCalculated;
                return;
            }
            AutomationElement AE = (AutomationElement)obj;
            AutomationElement[] AEColl = null, AEWhereColl = null;
            AutomationElement targetAE = null;
            int RowNumber = -1, RowCount;

            //Dictionary<string, AutomationElementCollection> MainDict = new Dictionary<string, AutomationElementCollection>();            

            MainDict = GetUpdateDictionary(AE, ref isElementsFromPoints);
            //Not to Remove Yet
            //if (!parentDictionary.ContainsKey(AE) || parentDictionary[AE].Keys.Count == 0)
            //{
            //    if(parentDictionary.ContainsKey(AE))
            //        parentDictionary.Remove(AE);

            //    MainDict = GetDictionaryForGrid(AE);
            //    parentDictionary.Add(AE, MainDict);
            //}
            //else
            //{
            //    MainDict = parentDictionary[AE];
            //}
            try
            {
                RowCount = GetRowCountforGrid(AE,true);
                if (actGrid.ControlAction.Equals(ActTableElement.eTableAction.GetRowCount))
                {

                    actGrid.AddOrUpdateReturnParamActual("Actual", RowCount.ToString());
                    actGrid.ExInfo = RowCount.ToString();
                    return;
                }                                              
                
                switch (actGrid.LocateRowType)
                {
                    case "Row Number":
                        // validation for Row number with max count
                        RowNumber = Convert.ToInt32(actGrid.GetInputParamCalculatedValue(ActTableElement.Fields.LocateRowValue));
                        if (RowNumber >= RowCount)
                        {
                            actGrid.Error = "Given Row Number " + RowNumber + " is not present in Column";
                            return;
                        }

                        break;

                    case "Any Row":
                        Random rnd = new Random();
                        RowNumber = rnd.Next(0, RowCount);
                        break;

                    case "By Selected Row":
                        break;

                    case "Where":
                        AutomationElement whereColumnElement;
                        if (!actGrid.ControlAction.Equals(ActTableElement.eTableAction.SetText))
                        {
                            if (!isElementsFromPoints)
                            {
                                AEWhereColl = GetColumnCollection(actGrid.WhereColSelector, actGrid.WhereColumnTitle, AE);
                                RowNumber = GetRowNumFromCollection(AEWhereColl, actGrid.WhereProperty, actGrid.WhereOperator, actGrid.GetInputParamCalculatedValue(ActTableElement.Fields.WhereColumnValue));
                            }
                            else
                            {
                                whereColumnElement = GetColumnElementByPoint(AE, actGrid.WhereColumnTitle, actGrid.GetInputParamCalculatedValue(ActTableElement.Fields.WhereColumnValue));
                                targetAE = GetColOnScreenByPoint(AE, whereColumnElement, actGrid.LocateColTitle);
                            }
                        }
                        break;
                }

                if (targetAE == null && (!actGrid.ControlAction.Equals(ActTableElement.eTableAction.SetText)))
                {
                    AEColl = GetColumnCollection(actGrid.ColSelectorValue, actGrid.LocateColTitle, AE);
                    if (AEColl[RowNumber].Current.IsOffscreen)
                        getTableElementOnScreen(AE, AEColl[RowNumber], RowNumber);
                    if (AEColl[RowNumber].Current.IsOffscreen)
                    {
                        actGrid.Error = "Unable to bring cell on screen";
                        return;
                    }
                    targetAE = AEColl[RowNumber];
                }

                HandleTableAction(AE, targetAE, actGrid);

            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                parentDictionary.Remove(AE);
                Reporter.ToLog(eLogLevel.DEBUG,"Error in HandleGridcontrolAction", e);
                throw new System.Runtime.InteropServices.COMException();
            }
        }

        /// <summary>
        /// finding grid element by GetText and performing Click and Sendkeys on it
        /// </summary>
        public void SetTextByElementGetText(AutomationElement tableElement, string targetColumnTitle, string whereColumnTitle, string whereColumnValue, string value)
        {
            int targetXPoint;
            List<string> keys = MainDict.Keys.ToList();
            if (!keys.Contains(whereColumnTitle))
            {
                throw new Exception("Input Column Name " + whereColumnTitle + " not present in Grid");
            }
            if (!keys.Contains(targetColumnTitle))
            {
                throw new Exception("Input Column Name " + targetColumnTitle + " not present in Grid");
            }

            AutomationElement element = (MainDict[whereColumnTitle])[0];
            getColumnOnScreen(tableElement, whereColumnTitle);

            int sourceYPoint = (int)element.Current.BoundingRectangle.Height + 7;
            int tableHeight = (int)tableElement.Current.BoundingRectangle.Height;
            AutomationElement Scroll = tableElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.OrientationProperty, OrientationType.Vertical));
            AutomationElement pageDown = null, pageUp = null;

            if (Scroll != null)
            {
                pageDown = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
                pageUp = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
            }
            while (pageUp != null && (!pageUp.Current.IsOffscreen))
            {
                ClickElement(pageUp);
                pageDown = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
            }

            do
            {
                while (sourceYPoint < tableHeight)
                {
                    string XY = "" + 10 + "," + sourceYPoint;
                    string val = GetControlText(element, XY);
                    if (val.Equals(whereColumnValue))
                    {
                        sourceYPoint = sourceYPoint + (int)element.Current.BoundingRectangle.Y;
                        getColumnOnScreen(tableElement, targetColumnTitle);
                        targetXPoint = (int)MainDict[targetColumnTitle][0].Current.BoundingRectangle.X + 5;

                        winAPI.SendClickOnXYPoint(tableElement, targetXPoint, sourceYPoint);
                        WinAPIAutomation.SendInputKeys(value);
                        WinAPIAutomation.SendTabKey();
                        return;
                    }
                    sourceYPoint = sourceYPoint + 5;
                }
                if (pageDown != null && (!pageDown.Current.IsOffscreen))
                    ClickElement(pageDown);
            } while (pageDown != null);
            throw new Exception("Given Value " + whereColumnValue + " is not present in column " + whereColumnTitle);
        }

        /// <summary>
        /// finding Expected Column then Expected Cell from that column by point
        /// </summary>
        public AutomationElement GetColumnElementByPoint(AutomationElement AE, string ColTitle, string ColValue)
        {
            AutomationElement[] AEColl = MainDict[MainDict.Keys.Last()];
            AutomationElement targetColFirstAE;
            int count = AEColl.Length, j = MainDict.Count - 1;

            while (count != 0 && (AEColl[0].Current.LocalizedControlType.Equals("thumb")) || AEColl[0].Current.LocalizedControlType.Equals("scroll bar") || AEColl[0].Current.LocalizedControlType.Equals("image") || (AEColl[0].Current.LocalizedControlType.Equals("button")))
            {                
                j--;
                AEColl = MainDict.Values.ElementAt(j);
                count = AEColl.Count();
            }
            if (!AEColl[0].Current.IsOffscreen)
            {
                targetColFirstAE = GetColOnScreenByPoint(AE, AEColl[0], ColTitle);
                return GetColTargetCellByPoint(AE, targetColFirstAE, ColValue);
            }
            return null;
        }

        public void getTableElementOnScreen(AutomationElement AE, AutomationElement targetAE, int RowNumber)
        {            
                int minRowOnScreen = -1;
                string ColName = targetAE.Current.Name;
                minRowOnScreen = getColumnOnScreen(AE,ColName);
                if (minRowOnScreen == -1)
                    return;

                getRowOnScreen(AE, targetAE, RowNumber, minRowOnScreen);
                return;            
        }

        /// <summary>
        /// finding Expected Row from Column and moving it OnScreen
        /// </summary>
        public AutomationElement GetColTargetCellByPoint(AutomationElement tableAE, AutomationElement colFirstAE, string targetCellvalue)
        {
            AutomationElement Scroll = tableAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.OrientationProperty, OrientationType.Vertical));
            AutomationElement pageUp = null, pageDown = null, TargetAE = null;            

            int x = ((int)colFirstAE.Current.BoundingRectangle.X + 2);
            int y = ((int)tableAE.Current.BoundingRectangle.Y) + (((int)(colFirstAE.Current.BoundingRectangle.Height)) / 2);
            int Limit = (int)tableAE.Current.BoundingRectangle.Y + (int)tableAE.Current.BoundingRectangle.Height;

            TargetAE = FindElementFromRowByPoint(x, y, Limit, targetCellvalue);
            if (TargetAE != null)
            {
                return TargetAE;
            }

            if (Scroll != null)
            {
                pageDown = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
                pageUp = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
                while (pageUp != null && (!pageUp.Current.IsOffscreen))
                {
                    ClickElement(pageUp);
                }
                do
                {
                    pageDown = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
                    TargetAE = FindElementFromRowByPoint(x, y, Limit, targetCellvalue);
                    if (TargetAE != null)
                    {
                        return TargetAE;
                    }
                    if (pageDown != null && (!pageDown.Current.IsOffscreen))
                        ClickElement(pageDown);                    
                } while (pageDown!=null);                
            }
            throw new Exception("Could not find Row with value  :  " + targetCellvalue);            
        }

        /// <summary>
        /// Moving Expected Column OnScreen 
        /// </summary>
        public AutomationElement GetColOnScreenByPoint(AutomationElement TableAE, AutomationElement refCol, string targetColValue)
        {
            int x = (int)TableAE.Current.BoundingRectangle.X;
            int y = (int)refCol.Current.BoundingRectangle.Y + 2;
            AutomationElement Scroll = TableAE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.OrientationProperty, OrientationType.Horizontal));
            AutomationElement pageLeft = null, pageRight = null;

            //if element is visible on screen
            refCol = FindColumnByPoint(x, y, ((int)TableAE.Current.BoundingRectangle.X + (int)TableAE.Current.BoundingRectangle.Width), targetColValue);
            if (refCol!=null)            
                return refCol;

            if (Scroll != null)
            {
                pageLeft = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page left"));
                while (pageLeft != null && (!pageLeft.Current.IsOffscreen))
                {
                    ClickElement(pageLeft);
                }
                do
                {
                    pageRight = Scroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page right"));
                    refCol = FindColumnByPoint(x, y, ((int)TableAE.Current.BoundingRectangle.X + (int)TableAE.Current.BoundingRectangle.Width), targetColValue);
                    if (refCol != null)
                        return refCol;

                    if (pageRight != null && (!pageRight.Current.IsOffscreen))
                        ClickElement(pageRight);

                } while(pageRight!=null);
            }                       
            throw new Exception("Unable to bring " + targetColValue + " Column on screen ");
        }

        public AutomationElement FindColumnByPoint(int x, int y, int xLimit, string value)
        {
            while (x < xLimit)
            {
                System.Windows.Point point = new System.Windows.Point(x, y);
                AutomationElement targetAE = AutomationElement.FromPoint(point);
                if (!String.IsNullOrEmpty(targetAE.Current.Name) && value.Equals(targetAE.Current.Name) && (!targetAE.Current.IsOffscreen))
                {
                    return targetAE;
                }
                x = x + 3;
            }
            return null;
        }

        public AutomationElement FindElementFromRowByPoint(int x, int y, int yLimit, string value)
        {
            AutomationElement TargetElement = null;
            string elementValue = "";
            while (y < yLimit)
            {
                System.Windows.Point point = new System.Windows.Point(x, y);
                TargetElement = AutomationElement.FromPoint(point);
                elementValue = GetControlValue(TargetElement);
                if (!String.IsNullOrEmpty(elementValue) && value.Equals(GetControlValue(TargetElement)) && (!TargetElement.Current.IsOffscreen))
                {
                    return TargetElement;
                }
                y = y + 3;
            }
            return null;
        }

        public void getRowOnScreen(AutomationElement AE, AutomationElement targetAE, int RowNumber, int MinRowOnScreen)
        {
            AutomationElement ScrollButton = AE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.OrientationProperty, OrientationType.Vertical));
            if (targetAE.Current.IsOffscreen && ScrollButton != null)
            {                
                if (MinRowOnScreen > RowNumber)
                {
                    ScrollButton = ScrollButton.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
                }
                else
                {
                    ScrollButton = ScrollButton.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
                }
                do
                {
                    if (!targetAE.Current.IsOffscreen)
                    {
                        break;
                    }
                    ClickElement(ScrollButton);
                } while (!ScrollButton.Current.IsOffscreen);
            }
        }



        public int getColumnOnScreen(AutomationElement AE, string ColName)
        {
            AutomationElement[] AEColl = MainDict[ColName];
            int minRowOnScreen = -1;
            AutomationElement horScroll = AE.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.OrientationProperty, OrientationType.Horizontal));
            AutomationElement pageRight = null;
            if (horScroll != null)
            {
                AutomationElement pageLeft = horScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page left"));
                while (pageLeft != null && !pageLeft.Current.IsOffscreen)
                {
                    ClickElement(pageLeft);
                }
                pageRight = horScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page right"));
            }

            minRowOnScreen = getMinRowOnscreen(AEColl);
            if (minRowOnScreen != -1)
            {
                return minRowOnScreen;
            }
            while ((pageRight != null) && ((!pageRight.Current.IsOffscreen) && minRowOnScreen < 0))
            {
                ClickElement(pageRight);
                minRowOnScreen = getMinRowOnscreen(AEColl);
            }
            return minRowOnScreen;
        }

        public int getMinRowOnscreen(AutomationElement[] AEColl)
        {
            for (int i = 0; i < AEColl.Length; i++)
            {
                if (!AEColl[i].Current.IsOffscreen)
                {
                    return i+1;
                }
            }
            return -1;
        }

        public int GetRowCountforGrid(AutomationElement AE, bool avail = false)
        {
            List<string> keys = MainDict.Keys.ToList();
            int maxcount = 0, tempcount = 0;
            AutomationElement element = null;
            foreach (string str in keys)
            {
                AutomationElement[] AECollection = MainDict[str];
                tempcount = AECollection.Count();
                if (tempcount > maxcount)
                {
                    maxcount = tempcount;
                    element = AECollection[0];
                }
            }
            if (element != null)
            {
                tempcount = GetCurrentCountForGrid(AE, element, avail);
                if(tempcount<2 && maxcount<2)
                {
                    tempcount = getCountwithZeroOrOne();
                }
                if (tempcount != maxcount)
                {
                    return tempcount;
                }
            }
            return tempcount;
        }

        private void HandleTableAction(AutomationElement TableAE, AutomationElement element, ActTableElement actGrid)
        {
            switch (actGrid.ControlAction)
            {
                case ActTableElement.eTableAction.SetValue:
                    SetControlValue(element, actGrid.ValueForDriver);
                    actGrid.ExInfo = actGrid.ValueForDriver + " set";
                    break;

                case ActTableElement.eTableAction.GetValue:
                    string val = GetControlValue(element);
                    actGrid.AddOrUpdateReturnParamActual("Actual", val);
                    actGrid.ExInfo = val;
                    break;
                case ActTableElement.eTableAction.GetText:
                    string valText = GetControlText(element);
                    actGrid.AddOrUpdateReturnParamActual("Actual", valText);
                    actGrid.ExInfo = valText;
                    break;
                case ActTableElement.eTableAction.Toggle:
                    string value = ToggleControlValue(element);
                    actGrid.AddOrUpdateReturnParamActual("Actual", value);
                    actGrid.ExInfo = value;
                    break;

                case ActTableElement.eTableAction.Click:
                    string status = ClickElement(element);
                    if (!status.Contains("Clicked Successfully"))
                    {
                        actGrid.Error += status;
                    }
                    else
                        actGrid.ExInfo += status;
                    break;
                case ActTableElement.eTableAction.ClickXY:
                    ClickOnXYPoint(element, actGrid.ValueForDriver);
                    break;

                case ActTableElement.eTableAction.DoubleClick:
                    DoDoubleClick(element, actGrid.ValueForDriver);
                    break;

                case ActTableElement.eTableAction.SetText:
                    //TODO: make support SetText Table action by Row Number and Column Number.
                    if (!actGrid.LocateRowType.Equals("Where") || (actGrid.WhereColSelector.Equals(ActTableElement.eRunColSelectorValue.ColNum)) || (actGrid.ColSelectorValue.Equals(ActTableElement.eRunColSelectorValue.ColNum)))
                    {
                        actGrid.Error = "SetText Table Action is currently supported only by ColumnTitle with Where Option. Other available options are not supported yet";
                        break;
                    }

                    SetTextByElementGetText(TableAE, actGrid.LocateColTitle, actGrid.WhereColumnTitle, actGrid.GetInputParamCalculatedValue(ActTableElement.Fields.WhereColumnValue), actGrid.ValueForDriver);
                    break;

                default:
                    actGrid.Error = "Action  - " + actGrid.ControlAction + " not supported for Grids";
                    break;

            }
        }

        public int GetCurrentCountForGrid(AutomationElement AE, AutomationElement childAE,bool avail=false)
        {
            int count = 0;
            AutomationElementCollection AECollection;
            System.Windows.Automation.Condition NameCond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, childAE.Current.Name);
            AECollection = AE.FindAll(TreeScope.Children, NameCond);
            if(avail == true)
            {
                foreach(AutomationElement aeChild in AECollection)
                {
                    object vp = null;
                    if (aeChild.TryGetCurrentPattern(ValuePatternIdentifiers.Pattern, out vp))
                    {
                        if (vp != null)
                        {
                            if (((ValuePattern)vp).Current.Value != null && (((ValuePattern)vp).Current.Value != "" || (((ValuePattern)vp).Current.Value == "" && aeChild.Current.IsOffscreen == false)))
                                count++;
                        }
                    }
                    else
                        count++;
                }
            }
            else
                count = AECollection.Count;

            if (AECollection.Count == 0)
            {
                // need to take decision. do we need to make support this feature or not? it will fail if we are taking childs from location and if there scroll bar exist hen this will return incorrect row count
                List<AutomationElement> gridControls = GetGridControlsFromPoint(AE);
                Dictionary<string, AutomationElement[]> dictionary = new Dictionary<string, AutomationElement[]>();
                string sElementName = "";

                foreach (AutomationElement grdElement in gridControls)
                {
                    sElementName = grdElement.Current.Name;
                    if (!String.IsNullOrEmpty(sElementName))
                    {
                        if (!dictionary.ContainsKey(sElementName))
                        {
                            dictionary.Add(sElementName, gridControls.FindAll(AE1 => AE1.Current.Name == sElementName).ToArray());
                        }
                        else { break; }
                    }
                }
                if (parentDictionary.ContainsKey(AE))
                {
                    parentDictionary[AE] = dictionary; // updating parent dictionary with latest elements
                }
                else
                {
                    parentDictionary.Add(AE, dictionary);
                }

                // TODO : support grid with Scrolled elements. if elements are taking by location

                List<string> keys = dictionary.Keys.ToList();
                int maxcount = 0, tempcount = 0;
                foreach (string str in keys)
                {
                    AutomationElement[] collection = dictionary[str];
                    tempcount = collection.Count();

                    if (tempcount > maxcount)
                    {
                        maxcount = tempcount;
                    }
                }

                return maxcount;

            }

            //AutomationElementCollection AEScrolls = AE.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ScrollBar));
            //AutomationElement verScroll = null;
            //Rect emptyRect = new Rect(0, 0, 0, 0);
            //foreach (AutomationElement aeScroll in AEScrolls)
            //{
            //    if (aeScroll.Current.Orientation == OrientationType.Vertical)
            //        verScroll = aeScroll;
            //}
            //int i = 0;
            //if (verScroll != null)
            //{
            //    AutomationElement pageDown = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
            //    while (pageDown != null)
            //    {
            //        while (i < AECollection.Count && (!AECollection[i].Current.BoundingRectangle.Equals(emptyRect)))
            //        {
            //            count++;
            //            i++;
            //        }
            //        ClickElement(pageDown);
            //        pageDown = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
            //    }
            //}
            //while (i < AECollection.Count && (!AECollection[i].Current.BoundingRectangle.Equals(emptyRect)) && !taskFinished)
            //{
            //    count++;
            //    i++;
            //}
            //if (verScroll != null)
            //{
            //    AutomationElement pageUp = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
            //    while (pageUp != null)
            //    {
            //        ClickElement(pageUp);
            //        pageUp = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
            //    }
            //}

            return count;
        }
        public int getCountwithZeroOrOne()
        {
            int yPoint = 0;
            int count = 0;
            List<AutomationElement> gridControls = new List<AutomationElement>();
            foreach(string str in MainDict.Keys)
            {
                gridControls.Add(MainDict[str].ToList<AutomationElement>()[0]);
            }
            if (gridControls.Count != 0)
            {
                yPoint = (int)gridControls[0].Current.BoundingRectangle.Y;
                for (int i = 0; i < gridControls.Count() && gridControls[i] != null; i++)
                {
                    if (gridControls[i] != null && yPoint != (int)gridControls[i].Current.BoundingRectangle.Y && (gridControls[i].Current.BoundingRectangle.Y != 0) && (!gridControls[i].Current.LocalizedControlType.Equals("scroll bar")) && (!gridControls[i].Current.LocalizedControlType.Equals("thumb")) && (!gridControls[i].Current.LocalizedControlType.Equals("image")))
                    {
                        count = 1;
                        break;
                    }
                }
            }

            return count;
        }


        public Dictionary<string, AutomationElement[]> GetDictionaryForGrid(AutomationElement AE, ref bool isElementsFromPoints)
        {
            Dictionary<string, AutomationElement[]> MainDict = new Dictionary<string, AutomationElement[]>();
            AutomationElement tempElement;
            ElementLocator eleLoc = new ElementLocator();
            tempElement = TreeWalker.ContentViewWalker.GetFirstChild(AE);
            eleLoc.LocateBy = eLocateBy.ByName;
            AutomationElementCollection AECollection;

            //Calculate total cells of Grid
            string sElementName = "";
            do
            {
                if (tempElement == null) break;
                sElementName = tempElement.Current.Name;
                if (!String.IsNullOrEmpty(sElementName))
                {
                    if (!MainDict.ContainsKey(sElementName))
                    {
                        System.Windows.Automation.Condition NameCond = new PropertyCondition(AutomationElementIdentifiers.NameProperty, sElementName);
                        AECollection = AE.FindAll(TreeScope.Children, NameCond);
                        AutomationElement[] elementArray = new AutomationElement[AECollection.Count];
                        AECollection.CopyTo(elementArray, 0);
                        MainDict.Add(sElementName, elementArray);
                    }
                    else { break; }
                }
                try
                {
                    tempElement = TreeWalker.ContentViewWalker.GetNextSibling(tempElement);
                }catch(Exception e)                
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetDictionaryForGrid", e);
                    tempElement = null;
                }

            } while (tempElement != null && !taskFinished);
            if(MainDict.Count < 4)
            {
                List<AutomationElement> gridControls = GetGridControlsFromPoint(AE);
                isElementsFromPoints = true;
                foreach (AutomationElement grdElement in gridControls)
                {
                    sElementName = grdElement.Current.Name;
                    if (!String.IsNullOrEmpty(sElementName))
                    {
                        if (!MainDict.ContainsKey(sElementName))
                        {

                            MainDict.Add(sElementName, gridControls.FindAll(AE1 => AE1.Current.Name == sElementName).ToArray());
                        }
                        else { break; }
                    }
                }
            }
            return MainDict;
        }

        public TableElementInfo GetTableElementInfoForGrid(AutomationElement AE)
        {
            bool isElementsFromPoints = false;
            Dictionary<string, AutomationElement[]> tempDictionary = GetDictionaryForGrid(AE, ref isElementsFromPoints);
            List<String> mColNames = new List<string>();
            List<string> keys = tempDictionary.Keys.ToList();
            int maxcount = 0, tempcount = 0;
            foreach (string str in keys)
            {
                AutomationElement[] AECollection = tempDictionary[str];
                tempcount = AECollection.Count();
                if (tempcount > maxcount)
                {
                    maxcount = tempcount;
                }
            }

            foreach (string str in keys)
            {
                AutomationElement[] AECollection = tempDictionary[str];
                tempcount = AECollection.Count();

                if (tempcount == maxcount)
                {
                    mColNames.Add(str);
                }
            }
            TableElementInfo TBI = new TableElementInfo();
            TBI.ColumnNames = mColNames;
            TBI.RowCount = maxcount;
            TBI.MainDict = tempDictionary;
            return TBI;
        }

        private object GetComboValues(AutomationElement AE)
        {
            List<ComboBoxElementItem> ComboValues = new List<ComboBoxElementItem>();

            //AutomationElement elementNode = TreeWalker.ControlViewWalker.GetFirstChild(AE);
            AutomationElement elementNode = TreeWalker.RawViewWalker.GetFirstChild(AE);
            while (elementNode != null)
            {
                ComboValues.Add(new ComboBoxElementItem() { Value = "???" , Text = elementNode.Current.Name });
                //elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
                elementNode = TreeWalker.RawViewWalker.GetNextSibling(elementNode);
            }

            return ComboValues;
        }
        
        public override ElementInfo GetElementInfoFor(object obj)
        {
            AutomationElement AE = (AutomationElement) obj;
            UIAElementInfo EI = new UIAElementInfo();
            EI.ElementObject = AE;
            EI.X = (int)AE.Current.BoundingRectangle.X;
            EI.Y = (int)AE.Current.BoundingRectangle.Y;
            EI.Width = (int)AE.Current.BoundingRectangle.Width;
            EI.Height = (int)AE.Current.BoundingRectangle.Height;
            EI.WindowExplorer = WindowExplorer;
            //EI.IsExpandable = AE.Current.IsContentElement;
            return EI;
        }

        public override object GetElementFromCursor()
        {
            taskFinished = false;
            // Convert mouse position from System.Drawing.Point to System.Windows.Point.
            System.Windows.Point point = new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y);            
            object element = AutomationElement.FromPoint(point);
            
            // check it is in current window - CurrentWindow if not return null
            // go to parent until CurrentWindow found then inside our window else return null
            AutomationElement ParentElement = (AutomationElement)element;            
            while (ParentElement != null && !taskFinished)
            {
                // Currently we support widgets only for PB. below condition to be removed once we support it for windows
                
                
                    if (ParentElement.Current.ClassName == "Internet Explorer_Server")
                    {
                        point.X = point.X - ParentElement.Current.BoundingRectangle.X;
                        point.Y = point.Y - ParentElement.Current.BoundingRectangle.Y;
                        if (HTMLhelperObj == null)
                            InitializeBrowser(ParentElement);


                        element = HTMLhelperObj.GetHTMLElementFromPoint(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
                    }
                               
                if (ParentElement == CurrentWindow)
                {       
                    return element;
                }
                ParentElement = TreeWalker.RawViewWalker.GetParent(ParentElement);                
            }
            
            //not found in our current window
            return null;


        }

        public Bitmap WindowToBitmap(AutomationElement tempWindow)
        {
            //WinAPIAutomation.ShowWindow(CurrentWindow);            
            HandlePaintWindow(CurrentWindow);
            Thread.Sleep(200);
            int width = (int)tempWindow.Current.BoundingRectangle.Width;
            int height= (int)tempWindow.Current.BoundingRectangle.Height;

            Bitmap bmp = new Bitmap(width, height);
            
            Graphics memoryGraphics = Graphics.FromImage(bmp);
            IntPtr dc = memoryGraphics.GetHdc();
            PrintWindow((IntPtr)tempWindow.Current.NativeWindowHandle, dc, 0);
            memoryGraphics.ReleaseHdc(dc);
            return bmp;
        }
        
        public override ObservableList<ElementInfo> GetElements(ElementLocator EL)
        {
            ObservableList<ElementInfo> list = new ObservableList<ElementInfo>();
            //temp for test
            List<AutomationElement> AEList = FindElementsByLocator(EL);
            if (AEList != null)
            {
                foreach (AutomationElement AE in AEList)
                {
                    UIAElementInfo a =(UIAElementInfo) GetElementInfoFor(AE);
                    list.Add(a);
                }
            }

            return list;
        }

        public override object GetCurrentWindow()
        {
            return CurrentWindow;
        }

        public override HTMLHelper GetHTMLHelper()
        {
            return HTMLhelperObj;
        }

        public override List<ElementInfo> GetElementChilderns(object obj)
        {
            taskFinished = false;
            List<ElementInfo> list = new List<ElementInfo>();
            if (IsWindowValid(obj))
            {
                AutomationElement AE = (AutomationElement)obj;
                if (FindHasChild(AE))
                {
                    //AutomationElement elementNode = TreeWalker.ControlViewWalker.GetFirstChild(AE);
                    AutomationElement elementNode = TreeWalker.RawViewWalker.GetFirstChild(AE);
                    List<AutomationElement> AEChildList = new List<AutomationElement>();
                    while (elementNode != null && !taskFinished)
                    {
                        AEChildList.Add(elementNode);
                        UIAElementInfo WEI = (UIAElementInfo)GetElementInfoFor(elementNode);
                        WEI.WindowExplorer = WindowExplorer;
                        list.Add(WEI);
                        //elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
                        elementNode = TreeWalker.RawViewWalker.GetNextSibling(elementNode);
                        //Added this condition to show less elements in explorer for table
                        if (AE.Current.LocalizedControlType == "pane" && AE.Current.ClassName.StartsWith("pbdw"))
                        {
                            if (list.Count > 500)
                                break;
                        }
                    }
                    if (AE.Current.LocalizedControlType == "pane" && AE.Current.ClassName.StartsWith("pbdw"))
                    {
                        if (AEChildList.Count < 4)
                        {
                            List<AutomationElement> gridControls = GetGridControlsFromPoint(AE);
                            foreach (AutomationElement grdChild in gridControls)
                            {
                                if (!AEChildList.Contains(grdChild))
                                {
                                    UIAElementInfo WEI = (UIAElementInfo)GetElementInfoFor(grdChild);
                                    WEI.WindowExplorer = WindowExplorer;
                                    list.Add(WEI);
                                }
                            }
                        }
                    }  
                }                
            }
            else
            {                
                Reporter.ToUser(eUserMsgKey.ObjectUnavailable, "Selected Object is not available");
            }
            return list;
        }

        /// <summary>
        /// Retrieves an element in a list, using FindAll.
        /// </summary>
        /// <param name="parent">The list element.</param>
        /// <returns>The list item.</returns>
        bool FindHasChild(AutomationElement parent)
        {
            bool isPresent = false;
            try
            {                
                System.Windows.Automation.Condition findCondition = new PropertyCondition(AutomationElement.IsControlElementProperty, true);
                AutomationElementCollection found = parent.FindAll(TreeScope.Children, findCondition);
                AutomationElement childExist=TreeWalker.RawViewWalker.GetFirstChild(parent);
                if (found.Count >= 1 || !(ReferenceEquals( childExist,null)))
                {
                    isPresent = true;
                }                
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "COM Exception when HandlePBControlAction Error", ex);
            }            
            return isPresent;
        }

        private List<AutomationElement> GetGridControlsFromPoint(AutomationElement AE)
        {            
            List<AutomationElement> childList = new List<AutomationElement>();
            //AutomationElementCollection AEScrolls = AE.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.ScrollBar));
            //AutomationElement horScroll=null;
            //AutomationElement verScroll=null;
            updateGridControls(AE,ref childList);
            //TODO::Scroll Handling
            //foreach (AutomationElement aeScroll in AEScrolls)
            //{
            //    if (aeScroll.Current.Orientation == OrientationType.Horizontal)
            //        horScroll = aeScroll;
            //    else if (aeScroll.Current.Orientation == OrientationType.Vertical)
            //        verScroll = aeScroll;                
            //}
            //if(horScroll!=null)
            //{
            //    AutomationElement pageLeft = horScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page right"));
            //    while(pageLeft != null)
            //    {
            //        ClickElement(pageLeft);
            //        updateGridControls(AE, ref childList);
            //        pageLeft = horScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page right"));                    
            //    }
            //}
            //if (verScroll != null)
            //{
            //    AutomationElement pageDown = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page down"));
            //    while (pageDown != null)
            //    {
            //        ClickElement(pageDown);
            //        updateGridControls(AE, ref childList);
            //        pageDown = horScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page right"));
            //    }
            //    AutomationElement pageUp = verScroll.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Page up"));
            //    while (pageUp != null)
            //    {
            //        ClickElement(pageUp);
            //    }
            //}
            return childList;
        }

        private void updateGridControls(AutomationElement AE,ref List<AutomationElement> childList)
        {
            AE.SetFocus();
            Rect recGrid = AE.Current.BoundingRectangle;
            int iX = (int)recGrid.X + 5;
            int iY = (int)recGrid.Y + 7;
            //Get All Rows
            while (iY < (int)recGrid.Y + recGrid.Height && !taskFinished)
            {
                int iRowHeight = 0;
                while (iX < recGrid.X + recGrid.Width && !taskFinished)
                {
                    System.Windows.Point point = new System.Windows.Point(iX, iY);
                    object element = AutomationElement.FromPoint(point);
                    // check it is in current window - CurrentWindow if not return null
                    // go to parent until CurrentWindow found then inside our window else return null                    
                    AutomationElement AEElement = (AutomationElement)element;
                    if (AEElement.Current.AutomationId == AE.Current.AutomationId || AEElement.Current.BoundingRectangle.Width > AE.Current.BoundingRectangle.Width || AEElement.Current.BoundingRectangle.Height > AE.Current.BoundingRectangle.Height)
                    {
                        iX = iX + 10;
                        continue;
                    }
                    //ParentElement.SetFocus();
                    if (AEElement.Current.ProcessId == CurrentWindow.Current.ProcessId)
                    {                           
                        if (!childList.Contains((AutomationElement)element))
                        {
                            childList.Add((AutomationElement)element);
                            Rect recChild = ((AutomationElement)element).Current.BoundingRectangle;
                            iX = (int)recChild.X + (int)recChild.Width + 5;
                            iY = (int)recChild.Y + 5;
                            if (iRowHeight == 0)
                                iRowHeight = iY + (int)recChild.Height;
                            
                        }
                        else
                            iX = iX + 5;
                    }
                    else
                        iX = iX + 5;
                }
                if (iRowHeight == 0)
                    iRowHeight = iY;
                iX = (int)recGrid.X + 5;
                iY = iRowHeight + 10;
            }
        }

        #region IXPath
        // ----------------------------------------------------------------------------------------------------------------------------
        // IXPath Implementation
        // ----------------------------------------------------------------------------------------------------------------------------

        ElementInfo IXPath.GetRootElement()
        {
            return CurrentWindowRootElement;
        }

        ElementInfo IXPath.UseRootElement()
        {
            UIAElementInfo root = new UIAElementInfo();
            root.ElementObject = AutomationElement.RootElement;
            return root;
        }
        
        XPathHelper IXPath.GetXPathHelper(ElementInfo info)
        {
            return mXPathHelper;
        }

        ElementInfo IXPath.GetElementParent(ElementInfo ElementInfo)
        {
            try
            {
                UIAElementInfo EI = (UIAElementInfo)ElementInfo;
                // Check if we are at root do not go up and return null
                if (EI.ElementObject.Equals(CurrentWindow)) return null;

                //TreeWalker walker = TreeWalker.ControlViewWalker;
                TreeWalker walker = TreeWalker.RawViewWalker;
                AutomationElement ParentAE = walker.GetParent((AutomationElement)EI.ElementObject);

                //if (object.ReferenceEquals(ParentAE, null)) return null;
                if (ParentAE == null) return null;

                if (ParentAE.Equals(CurrentWindow)) return null; // CurrentWindowRootElement;  // Since there are check done on root element we must return the same when found

                UIAElementInfo RC = new UIAElementInfo() { ElementObject = ParentAE };    // return minimial EI
                return RC;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetElementParent", e);
                return null;
            }
        }

        string IXPath.GetElementProperty(ElementInfo ElementInfo, string PropertyName)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;
            if (EI.ElementObject == null)
            {
                throw new Exception("Error: GetElementProperty received ElementInfo with AutomationElement = null");
            }
            //FIXME
            // AutomationProperty AP = new AutomationProperty();
            AutomationProperty AP = null;

            if (PropertyName.ToUpper() == "XPATH") { return GetElementAbsoluteXPath((AutomationElement)EI.ElementObject); }
            else if (PropertyName.ToUpper() == "VALUE") { return GetControlValue((AutomationElement)EI.ElementObject); }
            else
            {
                AP = GetAutomationPropertyForXpathProp(PropertyName);
            }
            //TODO: the rest

            if (AP == null) return null;
            object val = ((AutomationElement)EI.ElementObject).GetCurrentPropertyValue(AP);
            if (val == null) return null;
            return val.ToString();
        }

        List<ElementInfo> IXPath.GetElementChildren(ElementInfo ElementInfo)
        {
            return GetElementChildren(ElementInfo);
        }


        //Find first in childrent only
        ElementInfo IXPath.FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;

            //FIXME - calc all props
            AutomationProperty AP = GetAutomationPropertyForXpathProp(conditions[0].PropertyName);
            System.Windows.Automation.Condition cond = new PropertyCondition(AP, conditions[0].Value);
            //---
         
            AutomationElement AE = ((AutomationElement)EI.ElementObject).FindFirst(TreeScope.Children, cond);
            if (AE == null)
            {
                GetChildrenUsingRawWalker((AutomationElement)EI.ElementObject, conditions, 
                    out List<AutomationElement> listChildren,true);
                if(listChildren.Count==0)
                {
                    return null;
                }
                AE = listChildren[0];
            }
            UIAElementInfo RC =(UIAElementInfo) GetElementInfoFor(AE);
            return RC;
        }

        string IXPath.GetElementID(ElementInfo EI)
        {
            return "";
        }

        string IXPath.GetElementTagName(ElementInfo EI)
        {
            return "";
        }

        List<object> IXPath.GetAllElementsByLocator(eLocateBy LocatorType, string LocValue)
        {
            return null;
        }

        private void GetChildrenUsingRawWalker(
            AutomationElement EAE, List<XpathPropertyCondition> conditions, out List<AutomationElement> listRAE,
            bool flagFindFirst=false)
        {
            listRAE = new List<AutomationElement>();
            AutomationElement TAE;
            AutomationElement AE = TreeWalker.RawViewWalker.GetFirstChild(EAE);
            while (AE != null && !taskFinished)
            {
                
                TAE = AE;
                if (General.CompareStringsIgnoreCase(conditions[0].PropertyName, "LocalizedControlType"))
                {
                    if (General.CompareStringsIgnoreCase(AE.Current.LocalizedControlType, conditions[0].Value))
                    {
                        listRAE.Add(AE);
                    }
                }
                else if (General.CompareStringsIgnoreCase(conditions[0].PropertyName, "Name"))
                {
                    if (General.CompareStringsIgnoreCase(AE.Current.LocalizedControlType, conditions[0].Value))
                    {
                        listRAE.Add(AE);
                    }
                }
                else if (General.CompareStringsIgnoreCase(conditions[0].PropertyName, "AutomationId"))
                {
                    if (General.CompareStringsIgnoreCase(AE.Current.LocalizedControlType, conditions[0].Value))
                    {
                        listRAE.Add(AE);
                    }
                }
                else if (General.CompareStringsIgnoreCase(conditions[0].PropertyName, "ClassName"))
                {
                    if (General.CompareStringsIgnoreCase(AE.Current.LocalizedControlType, conditions[0].Value))
                    {
                        listRAE.Add(AE);
                    }
                }
                if (flagFindFirst && listRAE.Count==1) break;
                AE = TreeWalker.RawViewWalker.GetNextSibling(AE);
                if (TAE.Equals(AE))
                    break;
            }
        }

        List<ElementInfo> IXPath.FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;

            //FIXME - calc all conditions, meanwhile do the first
            AutomationProperty AP = GetAutomationPropertyForXpathProp(conditions[0].PropertyName);
            System.Windows.Automation.Condition cond = new PropertyCondition(AP, conditions[0].Value);
            //---
            List<ElementInfo> rc = new List<ElementInfo>();
            UIAElementInfo EI1;
            AutomationElementCollection list = ((AutomationElement)EI.ElementObject).FindAll(TreeScope.Children, cond);
            if (list.Count == 0)
            {
                List<AutomationElement> listByRawWalker;
                GetChildrenUsingRawWalker((AutomationElement)EI.ElementObject, conditions, out listByRawWalker);

                if (listByRawWalker.Count == 0)
                {
                    return null;
                }
                for (int i = 0; i < listByRawWalker.Count; i++)
                {
                    EI1 = new UIAElementInfo() { ElementObject = listByRawWalker[i] };
                    rc.Add(EI1);
                    EI1 = null;
                }
                return rc;
            }
            
            for (int i = 0; i < list.Count; i++)
            {
                EI1 = new UIAElementInfo() { ElementObject = list[i] };
                rc.Add(EI1);
            }
            return rc;
        }

        ElementInfo IXPath.GetPreviousSibling(ElementInfo EI)
        {
            try
            {
                AutomationElement elementNode = TreeWalker.RawViewWalker.GetPreviousSibling((AutomationElement)EI.ElementObject);
                if (elementNode == null) return null;
                UIAElementInfo RC = new UIAElementInfo() { ElementObject = elementNode };
                return RC;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetPreviousSibling", e);
                return null;
            }
        }

        ElementInfo IXPath.GetNextSibling(ElementInfo EI)
        {
            try
            {
                AutomationElement elementNode = TreeWalker.RawViewWalker.GetNextSibling((AutomationElement)EI.ElementObject);
                if (elementNode == null) return null;
                UIAElementInfo RC = new UIAElementInfo() { ElementObject = elementNode };
                return RC;
            }
            catch(Exception e)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetNextSibling", e);
                return null;
            }
}
        #endregion IXPath


        AutomationProperty GetAutomationPropertyForXpathProp(string prop)
        {
            if (prop == "Name") return AutomationElement.NameProperty;
            if (prop == "AutomationId") return AutomationElement.AutomationIdProperty;
            if (prop == "LocalizedControlType") return AutomationElement.LocalizedControlTypeProperty;
            if (prop == "Value") return ValuePatternIdentifiers.ValueProperty;
            if (prop == "LegacyValue") return LegacyIAccessiblePatternIdentifiers.ValueProperty;
            if (prop == "ClassName") return AutomationElement.ClassNameProperty;
            if (prop == "ToggleState") return TogglePatternIdentifiers.ToggleStateProperty;
            if (prop == "IsSelected") return SelectionItemPatternIdentifiers.IsSelectedProperty;
            if (prop == "Text") return ValuePatternIdentifiers.ValueProperty;

            //TODO: add all, and if not found lookup all properties list of AE

            //meanwhile raise exception
            throw new Exception("prop not found - " + prop);
        }



        public override string GetElementControlType(object obj)
        {

            AutomationElement element = (AutomationElement)obj;
            if (IsWindowValid(element))
                return element.Current.LocalizedControlType;
            else
                return "";             
        }

        public override Rect GetElementBoundingRectangle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            if (IsWindowValid(element))
                return element.Current.BoundingRectangle;
            else
                return new Rect(0,0,0,0);
        }

        public override int GetElementNativeWindowHandle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            if (IsWindowValid(element))
                return element.Current.NativeWindowHandle;
            else
                return 0;
        }

        public override string GetElementTitle(object obj)
        {
            AutomationElement element = (AutomationElement)obj;

            if (!IsWindowValid(element))
                return "";

            string Name = WinAPIAutomation.GetText((IntPtr)element.Current.NativeWindowHandle);
            if (string.IsNullOrEmpty(Name))
            {
                Name = element.Current.Name;
                if (string.IsNullOrEmpty(Name))
                {
                    Name = element.Current.LocalizedControlType;
                    if (!string.IsNullOrEmpty(element.Current.AutomationId))
                    {
                        Name += " (" + element.Current.AutomationId + ")";
                    }
                }
            }

            return Name;
        }

        public override bool HasAtleastOneChild(object obj)
        {
            AutomationElement element = (AutomationElement)obj;
            
            //if (TreeWalker.ControlViewWalker.GetFirstChild(element)!= null)
              if (TreeWalker.RawViewWalker.GetFirstChild(element)!= null)
                return true;
            return false;
        }

        public List<ElementInfo> GetElementChildren(ElementInfo ElementInfo)
        {
            UIAElementInfo EI = (UIAElementInfo)ElementInfo;            
            //TreeWalker walker = TreeWalker.ControlViewWalker;
            TreeWalker walker = TreeWalker.RawViewWalker;

            List<ElementInfo> list = new List<ElementInfo>();

            try
            {
                AutomationElement child = walker.GetFirstChild((AutomationElement)EI.ElementObject);

                while (child != null && !taskFinished)
                {
                    UIAElementInfo EIChild = new UIAElementInfo() { ElementObject = child };
                    list.Add(EIChild);
                    child = walker.GetNextSibling(child);
                }
                return list;
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG,"",ex);
                return new List<ElementInfo>();
            }

            
        }

        public override ObservableList<ControlProperty> GetElementProperties(object obj)
        {
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

            AutomationElement AE = (AutomationElement)obj;

            AutomationProperty[] props = AE.GetSupportedProperties();            
            foreach (AutomationProperty AP in props)
            {
                ControlProperty CP = new ControlProperty();
                CP.Name = AP.ProgrammaticName;
                CP.Name = CP.Name.Replace("AutomationElementIdentifiers.", "");
                CP.Name = CP.Name.Replace("Property", "");
                object propValue;
                try
                {
                    propValue = AE.GetCurrentPropertyValue(AP);
                }
                catch(Exception e)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetElementProperties while GetCurrentPropertyValue::", e);
                    propValue = null;
                }
                if (propValue != null)
                {
                    //TODO: based on type get the value instead of ToString, use ToString only in Switch default
                    CP.Value = propValue.ToString();
                }
                list.Add(CP);
            }
            return list;
        }

        public override object[] GetSupportedPatterns(object obj)
        {
            AutomationElement element = (AutomationElement) obj;
            return element.GetSupportedPatterns();
        }

        public override string GetPatternName(object pattern)
        {
            AutomationPattern AP = (AutomationPattern) pattern;
            return AP.ProgrammaticName;
        }

        public override void TestPattern(object objElement, object objPattern)
        {

            object p = ((AutomationElement)objElement).GetCurrentPattern((AutomationPattern)objPattern);

            string PatternType = p.GetType().ToString();
            switch (PatternType)
            {
                case "System.Windows.Automation.ValuePattern":
                    ValuePattern VV = (ValuePattern)p;
                    // HighLightCurrentElement();
                    try
                    {
                        VV.SetValue("ABC");
                    }
                    catch
                    {
                        //TODO: use workaround
                    }
                    //TODO: check readonly
                    // VV.Current.IsReadOnly;
                    break;
                case "System.Windows.Automation.TextPattern":
                    TextPattern TP = (TextPattern)p;
                    // TP..DocumentRange.GetType .SetValue(ValueTextBox.Text);
                    break;

                case "System.Windows.Automation.InvokePattern":
                    InvokePattern IV = (InvokePattern)p;
                    IV.Invoke();
                    break;

                case "System.Windows.Automation.SelectionItemPattern":
                    SelectionItemPattern SP = (SelectionItemPattern)p;
                    SP.Select();
                    break;

                case "System.Windows.Automation.TogglePattern":
                    TogglePattern TGP = (TogglePattern)p;
                    TGP.Toggle();
                    break;

                case "System.Windows.Automation.ExpandCollapsePattern":
                    ExpandCollapsePattern ECP = (ExpandCollapsePattern)p;
                    ECP.Expand();
                    break;


                default:                    
                    Reporter.ToUser(eUserMsgKey.PatternNotHandled, "Pattern not handled yet - " + PatternType);
                    break;
            }
        }

        public String GetElementValueByValuePattern(AutomationElement element)
        {
            string value = string.Empty;
            try
            {
                value = (String)element.GetCurrentPropertyValue(ValuePatternIdentifiers.ValueProperty);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetValueProperty while GetCurrentPropertyValue::", ex);
            }
            return value;
        }

        public String GetElementValueByLegacyIAccessiblePattern(AutomationElement element)
        {
            string value = string.Empty;
            try
            {
                value = (String)element.GetCurrentPropertyValue(LegacyIAccessiblePatternIdentifiers.ValueProperty);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetValueProperty while GetCurrentPropertyValue::", ex);
            }
            return value;
        }

        public String GetElementValueByTextpattern(AutomationElement element)
        {
            string value = string.Empty;
            object vp;
            try
            {
                element.TryGetCurrentPattern(TextPattern.Pattern, out vp);
                if (vp != null)
                {
                    value = ((TextPattern)vp).DocumentRange.GetText(-1);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception in GetPattern while TryGetCurrentPattern::", ex);
            }
            return value;
        }

        public string GetElementXpath(ElementInfo EI)
        {
            return null;
        }
    }

}
