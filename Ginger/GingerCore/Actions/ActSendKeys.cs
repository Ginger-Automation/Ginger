#region License
/*
Copyright © 2014-2026 European Support Limited

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
using GingerCore.Drivers.Common.LegacyAutomation;
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

        /// <summary>
        /// Opens every message about a screen that was locked when the step ran, so the
        /// run report names the cause before it names the workaround.
        /// </summary>
        private const string LockedScreen = "The screen is locked, so keystrokes cannot be sent through the keyboard. ";

        /// <summary>
        /// How long the target window gets to accept each character. A window too busy
        /// to take text is worth reporting rather than waiting behind.
        /// </summary>
        private const int TypingTimeoutMs = 2000;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr A_0, int A_1, int A_2, int A_3);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public override void Execute()
        {
            try
            {
                //locate by
                if (string.IsNullOrEmpty(LocateValueCalculated))
                {
                    IntPtr foregroundWindow = GetForegroundWindow();
                    if (foregroundWindow == IntPtr.Zero)
                    {
                        // A locked screen is the usual reason there is no foreground
                        // window, and without a window title there is nothing to type
                        // into directly either, so this one can only be reported.
                        Error = InteractiveDesktop.IsAvailable()
                            ? "No window is currently focused. Please focus the target window before sending keys."
                            : LockedScreen + "Set the window title on this action so the text can be typed into it directly, "
                                + "unlock the screen for this step, or use an action on the element itself.";
                        return;
                    }
                    Reporter.ToLog(eLogLevel.DEBUG, $"Method - {MethodBase.GetCurrentMethod().Name}, Sending keys");
                    // If no LocateBy or LocateValue, send keys to current focus
                    if (IsSendKeysSlowly)
                    {
                        SendKeysSlowly(ValueForDriver);
                    }
                    else
                    {
                        SendKeys(ValueForDriver);
                    }
                    return;
                }
                else if (LocateBy is not eLocateBy.ByTitle and not eLocateBy.ByClassName)
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

            // Bringing a window to the front and typing at it both need a desktop that
            // can receive input. Behind a lock screen neither reports that it did
            // nothing, so the keys go nowhere and the action still passes. Typing into
            // the window directly reaches it either way.
            if (!InteractiveDesktop.IsAvailable())
            {
                TypeIntoWindow(winhandle);
                return;
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

        /// <summary>
        /// Sends the value to <paramref name="windowHandle"/> without going through
        /// the keyboard, so the step still works while the screen is locked.
        /// </summary>
        /// <remarks>
        /// Text and named keys both travel this way. A modifier does not, because a
        /// window message leaves the target thread's key state alone and Ctrl+A would
        /// arrive as a plain A, so a value carrying one is reported as a failure
        /// rather than sent without it.
        /// </remarks>
        private void TypeIntoWindow(IntPtr windowHandle)
        {
            IntPtr focused = Win32KeyMessages.ResolveFocusedChild(windowHandle);
            if (focused == IntPtr.Zero)
            {
                Error = LockedScreen + "The text was going to be typed into '" + LocateValueCalculated
                    + "' directly instead, but nothing in that window is ready to receive text. "
                    + "Add a step that puts the cursor in the field first, unlock the screen for this step, "
                    + "or use an action on the element itself.";
                return;
            }

            if (!Win32KeyMessages.TrySendNotation(focused, ValueForDriver, TypingTimeoutMs, out string failure))
            {
                Error = LockedScreen + "The keys were going to be sent to '" + LocateValueCalculated
                    + "' directly instead, and " + failure + ". "
                    + "Unlock the screen for this step, or use an action on the element itself.";
                return;
            }

            Reporter.ToLog(eLogLevel.DEBUG, $"Method - {MethodBase.GetCurrentMethod().Name}, screen locked, typed into the window instead of using the keyboard");
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
