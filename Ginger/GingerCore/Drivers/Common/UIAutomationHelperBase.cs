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
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using GingerCore.Actions;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.PBDriver;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace GingerCore.Drivers.Common
{
    public abstract class UIAutomationHelperBase
    {
        public enum ePlatform
        {
            PowerBuilder,
            Windows
        }

        public ePlatform mPlatform;
        int LastHighLightHWND = 0;
        public int? mLoadTimeOut;

        public BusinessFlow BusinessFlow { get; set; }
        public IWindowExplorer WindowExplorer { get; set; }
        public bool taskFinished;
        public abstract void StopRecording();
    
        public UIAElementInfo CurrentWindowRootElement = null;
        public void UpdateRootElement()
        {
            CurrentWindowRootElement = new UIAElementInfo() { ElementName=GetControlPropertyValue(GetCurrentWindow(),"Name"), ElementObject = GetCurrentWindow() };
        }

        public abstract object GetCurrentWindow();
        public abstract HTMLHelper GetHTMLHelper();
        public abstract bool IsWindowValid(object window);
        public abstract bool HandleSwitchWindow(eLocateBy locateBy, string locateValue);
        public abstract Boolean SwitchToWindow(string title);
        public abstract Boolean IsWindowExist(Act act);
        public abstract Boolean CloseWindow(Act act);
        public abstract Boolean SetWindowVisualState(ActWindow act);
        public abstract bool ExpandComboboxByUIA(object element);

        public abstract string SetElementVisualState(object AE,string state);

        public abstract string SetElementSize(object AE, string size);

        public object GetActElement(Act act)
        {
            object CurAE = null;
            try
            {
                string LocValueCalculated = act.LocateValueCalculated;
                CurAE = FindElementByLocator(act.LocateBy, LocValueCalculated);                
            }
            catch (COMException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "COM Exception when GetActElement Type:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                throw e;

            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Element not available Exception when GetActElement of Type:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Argument Exception when GetActElement of Type:" + act.GetType() + " Description:" + act.Description + " Error details:", e);
                throw e;
            }
            return CurAE;
        }

        public abstract object FindElementByLocator(eLocateBy eLocatorType, string LocateValue);
        public abstract object FindWindowByLocator(eLocateBy eLocatorType, string LocateValue);
        public abstract string SetControlValue(object element, string value);
        public abstract void DragAndDrop(object element, ActUIElement action);
        public abstract string ClickAndValidteHandler(object element, ActUIElement action);
        public abstract string SendKeysAndValidateHandler(object element, ActUIElement action);
        public abstract string SelectAndValidateHandler(object element, ActUIElement action);
        public abstract void SelectControlByIndex(object element, string value);
        public abstract void SendKeysToControl(object element, string value);
        public abstract  String GetControlValue(object element);
        public abstract String GetControlText(object element, string XY= "");
        public abstract  String GetControlFieldValue(object element,String value);
        public abstract String GetControlPropertyValue(object element, String value);
        public abstract bool ClickContextMenuItem(object element, string value);
        public abstract string ClickElement(object element, Boolean asyncFlag=false);
        public abstract void ClickOnXYPoint(object element, string value);

        public abstract void DoRightClick(object element, string XY="");

        public abstract void DoDoubleClick(object element, string XY = "");

        public abstract string GetSelectedItem(object element);

        public abstract string GetDialogTitle(object element);

        public abstract string ToggleControlValue(object element);


        public abstract string IsEnabledControl(object element);

        public abstract Boolean IsElementExist(eLocateBy eLocatorType, string LocateValue); //IsChildElementExist

        public abstract Boolean IsChildElementExist(eLocateBy eLocatorType, string LocateValue,string ValueForDriver);

        public abstract void ScrollDown(object element);

        public abstract void ScrollUp(object element);

        public abstract string IsControlSelected(object element);
        
        public abstract ElementInfo GetElementInfoFor(object element);

        public abstract  void ExpandControlElement(object element);

        public abstract void CollapseControlElement(object element);

        public abstract void ClickMenuElement(Act act);

        public abstract object GetElementFromCursor();

        public abstract List<ElementInfo> GetVisibleControls();
                
        public  ObservableList<ElementLocator> GetElementLocators(ElementInfo ei)
        {
            string elementName = GetElementTitle(ei.ElementObject);

            ObservableList<ElementLocator> list = new ObservableList<ElementLocator>();
            if (!string.IsNullOrEmpty(elementName))
            {
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByName, LocateValue = elementName, Help = "Highly Recommended when Name is unique in page" });
            }

            string AbsXPath = GetElementAbsoluteXPath(ei.ElementObject);
            list.Add(new ElementLocator() { LocateBy = eLocateBy.ByXPath, LocateValue = AbsXPath, Help = "using Absolute Xpath Recommended but sensitive to screen changes " });


            //TODO: fix me to get Smart XPath
            string elementAutomamtionID = GetControlPropertyValue(ei.ElementObject, "AutomationId");
            if (!string.IsNullOrEmpty(elementAutomamtionID))
            {
                list.Add(new ElementLocator() { LocateBy = eLocateBy.ByAutomationID, LocateValue = elementAutomamtionID, Help = "Recommended and usually stable" });
            }
            
            //TODO: Fix the issue with X,Y Calculations for Windows Driver. meanwhile showing x,y locator only for PB
            if (mPlatform == ePlatform.PowerBuilder)
            {
                double x;
                double y;

                x = ((AutomationElement)GetCurrentWindow()).Current.BoundingRectangle.X;
                y = ((AutomationElement)GetCurrentWindow()).Current.BoundingRectangle.Y;

                double xCordinate = double.Parse(GetControlPropertyValue(ei.ElementObject, "XOffset")) - x;
                double yCordinate = double.Parse(GetControlPropertyValue(ei.ElementObject, "YOffset")) - y;
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXY,
                    LocateValue = xCordinate + "," + yCordinate,
                    Help = "Not Recommended"
                });
            }
            return list;
        }

        public abstract object GetElementData(object element);

        public abstract ObservableList<ElementInfo> GetElements(ElementLocator EL);

        public abstract string GetWindowInfo(Object window);

        public abstract void TakeScreenShot(ActScreenShot act);

        public abstract List<object> GetListOfWindows();

        public abstract List<AppWindow> GetListOfDriverAppWindows();

        public abstract Bitmap GetCurrentWindowBitmap();

        public abstract Bitmap GetAppWindowAsBitmap(AppWindow aw);

        public abstract List<Bitmap> GetAppDialogAsBitmap(AppWindow aw);

        public abstract void SmartSyncHandler(ActSmartSync act);

        public abstract void SmartSwitchWindow(ActSwitchWindow act);

        public abstract void ActUISwitchWindow(Act act);

        public abstract void SwitchWindow(ActUIASwitchWindow act);

        public abstract List<ElementInfo> GetElementChilderns(object element);

        public abstract string InitializeBrowser(object element);

        public abstract void HandleGridControlAction(ActTableElement act);

        public abstract string GetElementAbsoluteXPath(object element);

        public abstract string GetElementControlType(object element);

        public abstract Rect GetElementBoundingRectangle(object element);

        public abstract int GetElementNativeWindowHandle(object element);

        public abstract string GetElementTitle(object element);

        public abstract bool HasAtleastOneChild(object element);

        public abstract ObservableList<ControlProperty> GetElementProperties(object element);

        public abstract void HandlePaintWindow(object element);

        public AppWindow GetAppWinodowForElement(object window, string title, AppWindow.eWindowType windowType)
        {
            AppWindow AW = new AppWindow();
            AW.Title = title;

            if (IsWindowValid(window))
            {
                UIAElementInfo WEI = new UIAElementInfo();
                WEI.ElementTitle = title;
                WEI.ElementObject = window;
                WEI.WindowExplorer = WindowExplorer;
                WEI.XCordinate = double.Parse(GetControlPropertyValue(window, "XOffset"));
                WEI.YCordinate = double.Parse(GetControlPropertyValue(window, "XOffset"));
                AW.RefObject = WEI;
                AW.WindowType = windowType; 
            }
            return AW;
        }

       public abstract object[] GetSupportedPatterns(object element);

        public abstract string GetPatternName(object pattern);

        public abstract void TestPattern(object element, object pattern);

        public void HiglightElement(ElementInfo ei)
        {
            if (IsWindowValid(GetCurrentWindow()))
            {
                //TODO: FIXME there is still glitch in refresh when highlighting form system menu
                UIAElementInfo WEI = (UIAElementInfo)ei;
                //First remove the last highlight if exist
                if (LastHighLightHWND != 0)
                {
                    // Remove the highlighter by asking the window to repaint
                    // Better repaint the whole window, caused some glitched when trying to repaint just the control window
                    HandlePaintWindow(GetCurrentWindow());
                    RedrawWindow((IntPtr)LastHighLightHWND, IntPtr.Zero, IntPtr.Zero, 0x0400/*RDW_FRAME*/ | 0x0100/*RDW_UPDATENOW*/ | 0x0001/*RDW_INVALIDATE*/);
                }

                if (WEI.ElementObject == null)
                {
                    return;
                }

                Rect r = new Rect();
                r = GetElementBoundingRectangle(WEI.ElementObject);

                int hwnd = GetElementNativeWindowHandle(GetCurrentWindow());  // AE.Current.NativeWindowHandle;            
                                                                              // hwnd = (TreeWalker.ContentViewWalker.GetParent(AE)).Current.NativeWindowHandle;
                LastHighLightHWND = hwnd;

                // If user have multiple screens, get the one where the current window is displayed, resolve issue of highlighter not working when app window on secondary monitor
                System.Windows.Forms.Screen scr = System.Windows.Forms.Screen.FromHandle((IntPtr)hwnd);
                HighlightRect(r, scr, WEI); 
            }
            else
            {                
                Reporter.ToUser(eUserMsgKey.ObjectUnavailable, "Selected Object is not available, cannot highlight the element");
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, bool bErase);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);


        [DllImport("gdi32.dll")]
        static extern int CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        public static void HighlightRect(System.Windows.Rect r, System.Windows.Forms.Screen scr, UIAElementInfo WEI)
        {
            FontFamily CurrentFontFamily = null;
            for (int i = 0; i < System.Drawing.FontFamily.Families.Count(); i++)
            {
                CurrentFontFamily = System.Drawing.FontFamily.Families[i];
                if (CurrentFontFamily.Name == "Courier New")
                {
                    break;
                }
            }

            int hdc = CreateDC(scr.DeviceName, null, null, IntPtr.Zero);
            Graphics graphics = Graphics.FromHdc((IntPtr)hdc);

            System.Drawing.Rectangle CalculatedRect = new System.Drawing.Rectangle();
            CalculatedRect.X = (int)r.Left - scr.Bounds.X - 1;
            CalculatedRect.Y = (int)r.Top - scr.Bounds.Y - 1;
            CalculatedRect.Width = (int)r.Width;
            CalculatedRect.Height = (int)r.Height;

            graphics.DrawRectangle(new System.Drawing.Pen(System.Drawing.Color.Red, 2), CalculatedRect);

            // Find the font to draw only once
            if (CurrentFontFamily == null)
            {
                for (int i = 0; i < System.Drawing.FontFamily.Families.Count(); i++)
                {
                    CurrentFontFamily = System.Drawing.FontFamily.Families[i];
                    if (CurrentFontFamily.Name == "Courier New")
                    {
                        break;
                    }
                }
            }

            System.Drawing.Font font = new System.Drawing.Font(CurrentFontFamily, 8);
            System.Drawing.Brush b = System.Drawing.Brushes.DarkRed;
            System.Drawing.Point p = new System.Drawing.Point(CalculatedRect.X, CalculatedRect.Y - 18);
            graphics.DrawString(WEI.ElementTitle, font, b, p);

            graphics.Dispose();
        }

        public bool PaintWindow(IntPtr hWnd)
        {
            InvalidateRect(IntPtr.Zero, IntPtr.Zero, true);
            return UpdateWindow(hWnd);
        }
        public  void StartRecording()
        {
            //TODO : make available recording function for PBDriver and Windows Driver            
            Reporter.ToUser(eUserMsgKey.MissingImplementation, "Recording");
        }
    }
}
