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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;
namespace Ginger.Actions
{

    public class ActSendKeys : ActWithoutDriver
    {

        public override string ActionDescription { get { return "Send Keys Action"; } }

        public override eImageType Image { get { return eImageType.KeyboardLayout; } }
        public override string ActionUserDescription { get { return "Send Keys to specific window"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to send keys to specific window");
            TBH.AddLineBreak();
            TBH.AddText("Locate Value is the window title - can be partial match");
            TBH.AddLineBreak();
            TBH.AddText("There is built in automatic retry until the window is found for 30 secs");
            TBH.AddLineBreak();
            TBH.AddText("This action usually used to handle CRM Launch when Java window or security window appear before CRM starts");
            TBH.AddLineBreak();
            TBH.AddText("To send special keystrokes please see below");
            TBH.AddLineBreak();
            TBH.AddText("BACKSPACE - {BACKSPACE}, {BS}, or {BKSP}");
            TBH.AddLineBreak();
            TBH.AddText("BREAK - {BREAK}");

            //TODO: add the full list in nice formatted table from:
            TBH.AddText("https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys(v=vs.110).aspx");
        }

        public override string ActionEditPage { get { return "ActSendKeysEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return false; } }



        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }

        public override string ActionType
        {
            get { return "ActSendKeys"; }
        }


        public new static partial class Fields
        {
            public static string IsSendKeysSlowly = "IsSendKeysSlowly";
            public static string ISWindowFocusRequired = "ISWindowFocusRequired";
            public static string Value = "Value";
        }


        public bool IsSendKeysSlowly
        {
            get
            {
                bool value = false;
                bool.TryParse(GetOrCreateInputParam(nameof(IsSendKeysSlowly)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(IsSendKeysSlowly), value.ToString());
                OnPropertyChanged(nameof(IsSendKeysSlowly));
            }
        }


        public bool ISWindowFocusRequired
        {
            get
            {
                bool value = true;
                bool.TryParse(GetOrCreateInputParam(nameof(ISWindowFocusRequired), value.ToString()).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ISWindowFocusRequired), value.ToString());
                OnPropertyChanged(nameof(ISWindowFocusRequired));
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr A_0, int A_1, int A_2, int A_3);

        public override void Execute()
        {
            try
            {
                //locate by
                if (LocateBy is not eLocateBy.ByTitle and not eLocateBy.ByClassName)
                {
                    Error = "Invalid Locate By- only ByTitle and ByClassName is supported.";
                    return;
                }

                //locate value
                String titleFromUser = LocateValueCalculated;
                if (string.IsNullOrEmpty(titleFromUser))
                {
                    Error = "Missing window Locate Value.";
                    return;

                }
            }
            catch (Exception e)
            {
                Error = "Failed to get the window Locate By/Value";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.StackTrace}", e);
                return;
            }


            IntPtr winhandle = IntPtr.Zero;
            UIAuto.AutomationElement window;



            //Wait max up to 30 secs for the window to appear
            for (int i = 0; i < 30; i++)
            {
                window = GetWindow(LocateValueCalculated);

                if (window != null)
                {
                    winhandle = window.Current.NativeWindowHandle;
                    if (winhandle != IntPtr.Zero)
                    {
                        break;
                    }
                }
                Thread.Sleep(200);
            }

            if (winhandle == IntPtr.Zero)
            {
                switch (LocateBy)
                {
                    case eLocateBy.ByTitle:
                        winhandle = ActivateApp(LocateValueCalculated);
                        if (winhandle == IntPtr.Zero)
                        {
                            //Status = eStatus.Fail;
                            Error = "Window with Title - '" + LocateValueCalculated + "' not found";
                            return;
                        }
                        break;

                    case eLocateBy.ByClassName:
                        Error = "Window with Class - '" + LocateValueCalculated + "' not found";
                        return;
                        //break;
                }
            }

            if (ISWindowFocusRequired)
            {
                SetForegroundWindow(winhandle);
            }

            if (IsSendKeysSlowly)
            {
                SendKeysSlowly(ValueForDriver);
            }
            else
            {
                SendKeys(ValueForDriver);
            }
        }

        internal void SendKeys(string text)
        {
            System.Windows.Forms.SendKeys.SendWait(text);
        }

        private void SendKeysSlowly(string text)
        {
            System.Threading.Thread.Sleep(40);
            string tmp = "";
            bool inSpecalKey = false;
            foreach (char s in text)
            {
                if (s == '{')
                {
                    inSpecalKey = true;
                }

                if (inSpecalKey)
                {
                    tmp = tmp + s;
                }
                else
                {
                    tmp = "" + s;
                }

                if (s == '}')
                {
                    inSpecalKey = false;
                }

                if (!inSpecalKey)
                {
                    System.Windows.Forms.SendKeys.SendWait(tmp); // Choose the appropriate send routine
                    if (tmp.Length > 1)
                    {
                        System.Threading.Thread.Sleep(200); // Milliseconds, adjust as needed
                    }

                    tmp = "";
                }
                System.Threading.Thread.Sleep(40); // Milliseconds, adjust as needed
            }
        }
        IntPtr ActivateApp(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);

            // Activate the first application we find with this name
            if (p.Any())
            {
                return p[0].MainWindowHandle;
            }

            return IntPtr.Zero;
        }
        UIAuto.AutomationElement GetWindow(string LocValCal) //*******
        {
            UIAComWrapperHelper UIA = new UIAComWrapperHelper();

            List<object> AppWindows = UIA.GetListOfWindows();


            foreach (UIAuto.AutomationElement window in AppWindows)
            {
                string WindowTitle = UIA.GetWindowInfo(window);

                if (WindowTitle == null)
                {
                    WindowTitle = "";
                }

                Reporter.ToLog(eLogLevel.DEBUG, $"Method - {MethodBase.GetCurrentMethod().Name}, WindowTitle - {WindowTitle}");
                switch (LocateBy)
                {

                    case eLocateBy.ByTitle:
                        if (WindowTitle.Contains(LocValCal))
                        {
                            return window;
                        }
                        break;
                    case eLocateBy.ByClassName:
                        if (window.Current.ClassName.Equals(LocValCal))
                        {
                            return window;
                        }
                        break;
                }
            }
            return null;
        }
    }

}
