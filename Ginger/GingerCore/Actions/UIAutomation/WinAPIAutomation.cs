#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Windows.Forms;
using mshtml;
using System.Threading;
using Amdocs.Ginger.Common;

namespace GingerCore.Drivers 
{
    class WinAPIAutomation
    {
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr AttachThreadInput(IntPtr idAttach, IntPtr idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out System.Windows.Point lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public SendInputEventType type;
            public MouseKeybdhardwareInputUnion mkhi;
        }
        [StructLayout(LayoutKind.Explicit)]
        struct MouseKeybdhardwareInputUnion
        {
            [FieldOffset(0)]
            public MouseInputData mi;

            [FieldOffset(0)]
            public KEYBDINPUT ki;

            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }
        struct MouseInputData
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public MouseEventFlags dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }
        [Flags]
        enum MouseEventFlags : uint
        {
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }
        enum SendInputEventType : int
        {
            InputMouse,
            InputKeyboard,
            InputHardware
        }

        enum KEYEVENTF : uint
        {
            KEYDOWN = 0x0000,
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }
        

        internal static void ClickLeftMouseButton(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            mouseInput.mkhi.mi.dx = (x);
            mouseInput.mkhi.mi.dy = (y);
            mouseInput.mkhi.mi.mouseData = 0;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x , y );
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x , y );
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

        }
      
        internal static void ClickRightMouseButton(int x, int y)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            mouseInput.mkhi.mi.dx = (x);
            mouseInput.mkhi.mi.dy = (y);
            mouseInput.mkhi.mi.mouseData = 0;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_MOVE | MouseEventFlags.MOUSEEVENTF_ABSOLUTE;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
         
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
          
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTDOWN;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            System.Threading.Thread.Sleep(100);
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_RIGHTUP;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            System.Threading.Thread.Sleep(100);
    
        }

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int WS_STYLE = (-16);
        private const UInt32 WS_MAXIMIZE = 0x1000000;
        private const UInt32 WS_MINIMIZE = 0x20000000;
        private const UInt32 WS_MINIMIZEBOX = 0x20000;

        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        public static void MinimizeWindow(int proccessID)
        {
            Process pr = Process.GetProcessById(proccessID);
            ShowWindowAsync(pr.MainWindowHandle, SW_SHOWMINIMIZED);
            System.Threading.Thread.Sleep(500);
        }
        public static void MaxmizeWindow(int proccessID)
        {
            Process pr = Process.GetProcessById(proccessID);
            ShowWindowAsync(pr.MainWindowHandle, SW_SHOWMAXIMIZED);
            System.Threading.Thread.Sleep(500);
        }
        public static void NormalizeWindow(int proccessID)
        {
            Process pr = Process.GetProcessById(proccessID);
            ShowWindowAsync(pr.MainWindowHandle, SW_SHOWNORMAL);
            System.Threading.Thread.Sleep(500);
        }
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool SetForegroundWindow(IntPtr windowHandle);

        [System.Runtime.InteropServices.DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        
        public static void ShowWindow(AutomationElement window)
        {
            try
            {


                string clname = window.Current.ClassName;
                string winname = window.Current.Name;
                IntPtr hwnd = FindWindow(clname, winname);
                if (hwnd == IntPtr.Zero)
                {
                    hwnd = (IntPtr)window.Current.NativeWindowHandle;
                }
                uint processId;
                GetWindowThreadProcessId(hwnd, out processId);
                Process proc = Process.GetProcessById((int)processId);

                int style = GetWindowLong(proc.MainWindowHandle, WS_STYLE);
                if ((style & WS_MAXIMIZE) == WS_MAXIMIZE)
                {
                    //It's maximized
                    ShowWindow(hwnd, 9);
                    SetForegroundWindow(hwnd);
                    MaxmizeWindow((int)processId);
                }
                else if ((style & WS_MINIMIZE) == WS_MINIMIZE)
                {
                    ShowWindow(hwnd, 9);
                    SetForegroundWindow(hwnd);
                    //It's minimized
                }
                else if ((style & WS_MINIMIZEBOX) == WS_MINIMIZEBOX)
                {
                    //Its minimize box 
                    ShowWindow(hwnd, 9);
                    SetForegroundWindow(hwnd);
                }
                else
                {
                    ShowWindow(hwnd, 9);
                    SetForegroundWindow(hwnd);
                }
            }
            catch (COMException e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "COM Exception when ShowWindow Error details:", e);
                throw e;
            }
            catch (ElementNotAvailableException e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Element not available Exception when ShowWindow Error details:", e);
                throw e;
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Argument Exception when ShowWindow Error details:", e);
                throw e;
            }
        }

        public static bool SetForeGroundWindow(int proccessID)
        {
            Process pr = Process.GetProcessById(proccessID);
            if (pr.MainWindowHandle != IntPtr.Zero)
            {
            SetForegroundWindow(pr.MainWindowHandle);
            System.Threading.Thread.Sleep(500);
                return true;
            }
            else
            {
                return false;
            }
            
        }

        //Get the process id for Foreground window
        internal static int GetForeGroundWindowProcessId()
        {
            IntPtr Hwnd = GetForegroundWindow();
            uint processID;
            GetWindowThreadProcessId(Hwnd, out processID);

            return (int)processID;
        }

        internal static void SendInputKeys(string value)
        {
            INPUT ip = new INPUT(); ; 
            ip.type = SendInputEventType.InputKeyboard;
            ip.mkhi.ki.time = 0;
            ip.mkhi.ki.dwFlags = (uint)KEYEVENTF.UNICODE;
            ip.mkhi.ki.wVk = 0;
            ip.mkhi.ki.dwExtraInfo = IntPtr.Zero;
            
            foreach (char c in value)
            {
                ip.mkhi.ki.wScan = c;
                SendInput(1, ref ip, Marshal.SizeOf(ip));
                System.Threading.Thread.Sleep(100);
            }
        }

        internal static void SendTabKey()
        {
            INPUT ip = new INPUT();
            ip.type = SendInputEventType.InputKeyboard;
            ip.mkhi.ki.time = 0;
            ip.mkhi.ki.dwFlags = (uint)KEYEVENTF.KEYDOWN;
            ip.mkhi.ki.wVk = 0x09;
            ip.mkhi.ki.dwExtraInfo = IntPtr.Zero;
            ip.mkhi.ki.wScan = 0;
            SendInput(1, ref ip, Marshal.SizeOf(ip));

            ip.mkhi.ki.dwFlags = (uint)KEYEVENTF.KEYUP;
            SendInput(1, ref ip, Marshal.SizeOf(ip));
        }


        public void SendClick(AutomationElement element,bool flag=true)
        {
            if (flag == true)
            {
                int targetProcessID = element.Current.ProcessId;
                SetForeGroundWindow(targetProcessID);
            }
            //TODO: FIXME - do not change cursor position or set foreground window, maybe needed only for PB + Calc clickable point and not +3 + send Button Up
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;            
            int x = (int)element.Current.BoundingRectangle.X + ((int)element.Current.BoundingRectangle.Width / 2);
            int y = (int)element.Current.BoundingRectangle.Y + ((int)element.Current.BoundingRectangle.Height / 2);

            ClickLeftMouseButton(x, y);
            System.Threading.Thread.Sleep(500);
            System.Windows.Forms.Cursor.Position = p;
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy,int dwData, int dwExtraInfo);

        [Flags]
        public enum MouseEventFlags11
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        public static void LeftClick(int x, int y)
        {
            Cursor.Position = new System.Drawing.Point(x, y);
            mouse_event((int)(MouseEventFlags11.LEFTDOWN), 0, 0, 0, 0);
            mouse_event((int)(MouseEventFlags11.LEFTUP), 0, 0, 0, 0);
        }

       /* [DllImportAttribute("user32.dll")]                                          //new code for double click
        public static extern int SendMessage(IntPtr hWnd,int Msg, int wParam, int lParam);
        private const int WM_LBUTTONDBLCLK = 0x0203;

        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDBLCLK = 0x0206; */


        public void SendDoubleClick(AutomationElement element,string XY="")
        {
            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            int x = 0;
            int y = 0;

            if ((!String.IsNullOrEmpty(XY))&& XY.IndexOf(",") > 0)
            {
                string[] coordinates = XY.Split(',');
                //User will specify the X,Y relative to the element instead of related to whole window
                x = (int)element.Current.BoundingRectangle.X + Int32.Parse(coordinates[0]);
                y = (int)element.Current.BoundingRectangle.Y + Int32.Parse(coordinates[1]);
            } 
            else
            {
                x = (int)element.Current.BoundingRectangle.X + ((int)element.Current.BoundingRectangle.Width / 2);
                y = (int)element.Current.BoundingRectangle.Y + ((int)element.Current.BoundingRectangle.Height / 2);
            }
            
            Cursor.Position = new System.Drawing.Point(x, y);       
           LeftClick(x,y);
           LeftClick(x,y);
        }
        public void MoveMousetoXYPoint(AutomationElement element, int x, int y)
        {
            System.Windows.Rect boundingRect = (System.Windows.Rect)
            element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            Cursor.Position = new System.Drawing.Point((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y + y));
        }

        public void SendClickOnXYPoint(AutomationElement element, int x, int y)
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            
            ClickLeftMouseButton(x, y);
            System.Windows.Forms.Cursor.Position = p;
        }
        public void SendClickOnWinXYPoint(AutomationElement element, int x, int y)
        {
            System.Windows.Rect boundingRect = (System.Windows.Rect)
            element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);


            ClickLeftMouseButton((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y+y));
            System.Windows.Forms.Cursor.Position = p;
        }

        public void SendDoubleClickOnWinXYPoint(AutomationElement element, int x, int y)
        {
            System.Windows.Rect boundingRect = (System.Windows.Rect)
    element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);


            ClickLeftMouseButton((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y + y));
            System.Threading.Thread.Sleep(500);
            ClickLeftMouseButton((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y + y));
            System.Windows.Forms.Cursor.Position = p;
        }
        public void SendRightClickOnWinXYPoint(AutomationElement element, int x, int y)
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;
            System.Windows.Rect boundingRect = (System.Windows.Rect)
            element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);


            ClickRightMouseButton((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y + y));
            System.Windows.Forms.Cursor.Position = p;
        }

        public void SetElementTextOnWinXYPoint(AutomationElement element, string value, int x, int y)
        {
            //Save the Current Cursor Position
            System.Windows.Rect boundingRect = (System.Windows.Rect)
            element.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            ClickLeftMouseButton((int)(boundingRect.TopLeft.X + x), (int)(boundingRect.TopLeft.Y + y));
            SendInputKeys(value);
            SendTabKey();

            System.Windows.Forms.Cursor.Position = p;
        }
        public void SendRightClick(AutomationElement element,string XY="")
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            int x = 0;
            int y = 0;
            if (XY != "" && XY.IndexOf(",") > 0)
            {
                string[] coordinates = XY.Split(',');                
                //User will specify the X,Y relative to the element instead of related to whole window
                x = (int)element.Current.BoundingRectangle.X + Int32.Parse(coordinates[0]);
                y = (int)element.Current.BoundingRectangle.Y + Int32.Parse(coordinates[1]);
            }
            else
            {
                x = (int)element.Current.BoundingRectangle.X + 10;
                y = (int)element.Current.BoundingRectangle.Y + 5;
            }
            ClickRightMouseButton(x, y);
            System.Windows.Forms.Cursor.Position = p;
        }

        public void SetElementText(AutomationElement element, string value)
        {
            //Save the Current Cursor Position

            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            int x1 = (int)element.Current.BoundingRectangle.X + 2;
            int y1 = (int)element.Current.BoundingRectangle.Y + 2;
            ClickLeftMouseButton(x1, y1);
            //SendKeys.SendWait(value);
            SendInputKeys(value);               
            SendTabKey();

                    System.Windows.Forms.Cursor.Position = p;
        }

        public void SetElementTextWithFocus(AutomationElement element, string value)
        {
            //Save the Current Cursor Position

            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int targetProcessID = element.Current.ProcessId;
            SetForeGroundWindow(targetProcessID);

            int x1 = (int)element.Current.BoundingRectangle.X + 2;
            int y1 = (int)element.Current.BoundingRectangle.Y + 2;
            ClickLeftMouseButton(x1, y1);
            element.SetFocus();
            // SendInputKeys(value);
            SendKeys.SendWait(value);

            SendTabKey();

            System.Windows.Forms.Cursor.Position = p;
        }

        public void SetDate(AutomationElement element, string value)
        {
            System.Drawing.Point p = System.Windows.Forms.Cursor.Position;

            int x1 = (int)element.Current.BoundingRectangle.X + 2;
            int y1 = (int)element.Current.BoundingRectangle.Y + 2;

            int id = element.Current.ProcessId;
            SetForeGroundWindow(id);

            ClickLeftMouseButton(x1, y1);

            INPUT ip = new INPUT();
            ip.type = SendInputEventType.InputKeyboard;
            ip.mkhi.ki.time = 0;
            ip.mkhi.ki.dwFlags = (uint)KEYEVENTF.UNICODE;
            ip.mkhi.ki.wVk = 0;
            ip.mkhi.ki.dwExtraInfo = IntPtr.Zero;

            string[] date = value.Split('\\');

            foreach (string str in date)
            {

                foreach (char c in str)
                {
                    ip.mkhi.ki.wScan = c;
                    SendInput(1, ref ip, Marshal.SizeOf(ip));
                    System.Threading.Thread.Sleep(100);
                }

            }
            SendTabKey();
            System.Windows.Forms.Cursor.Position = p;
        }
        public void ClickLeftMouseButtonAndHoldAndDrop(AutomationElement AE, int sourceX, int sourceY, int destX, int destY)
        {
            INPUT mouseInput = new INPUT();
            mouseInput.type = SendInputEventType.InputMouse;
            mouseInput.mkhi.mi.dx = (sourceX);
            mouseInput.mkhi.mi.dy = (sourceY);
            mouseInput.mkhi.mi.mouseData = 0;

            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(sourceX, sourceY);
            //Performs full left click action on object to move
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(sourceX, sourceY); ;
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));

            Thread.Sleep(1000);
            //Peformed Mouse Left click and hold
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTDOWN;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(destX, destY);
            mouseInput.mkhi.mi.dx = (destX);
            mouseInput.mkhi.mi.dy = (destY);

            Thread.Sleep(1000);
            //Mouse left click released at Target location
            mouseInput.mkhi.mi.dwFlags = MouseEventFlags.MOUSEEVENTF_LEFTUP;
            SendInput(1, ref mouseInput, Marshal.SizeOf(new INPUT()));
            return;
        }

        //TODO: Currently we are iterating and clicking over a tab width to locate a tab. This adds unnecessary click events. Fix it to avoid additional click 
        

        public void SendKeysByLibrary(AutomationElement element, string value)
        {
            element.SetFocus();
            SendKeys.SendWait(value);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowText", CharSet = CharSet.Auto)]
        static extern IntPtr GetWindowCaption(IntPtr hwnd, StringBuilder lpString, int maxCount);
                        
        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public const uint KEYEVENTF_KEYUP = 0x0002;
        public const uint VK_CONTROL = 0xA2;        
        public const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        

        internal static void HoldControlKeyOfKeyboard()
        {            
            keybd_event((byte)VK_CONTROL, 0, 0, 0);            
        }

        internal static void ReleaseControlKeyOfKeyboard()
        {            
            keybd_event((byte)VK_CONTROL, 0, (byte)KEYEVENTF_KEYUP, 0);            
        }

        public static string GetText(IntPtr handle)
        {
            // Max 50 chars as it will go to the tree
            StringBuilder sb = new StringBuilder(50);
            //if (this.isFlauiAutomationElement)
            //{

            //}
            //  //  GetWindowCaption((IntPtr)this.FlaUIAutomationElement.BasicAutomationElement.Properties.NativeWindowHandle, sb, 50);
            //else
           // AutomationElement element = (AutomationElement)ElementObject;
            GetWindowCaption(handle, sb, 50);
            return sb.ToString();
        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, bool bErase);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UpdateWindow(IntPtr hWnd);

        public bool PaintWindow(IntPtr hWnd)
        {
            InvalidateRect(hWnd, IntPtr.Zero, true);
            return UpdateWindow(hWnd);
        }

        [DllImport("kernel32.dll")]

        private static extern int LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi)]

        private static extern int GetProcAddress(IntPtr ModuleHandle,
string ProcName);

        [DllImport("kernel32.dll")]

        private static extern int FreeLibrary(int ModuleHandle);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageA")]

        public static extern uint RegisterWindowMessage(string
lpString);

        [DllImport("user32.dll", SetLastError = true, CharSet =
CharSet.Auto)]

        public static extern IntPtr SendMessageTimeout(

            IntPtr windowHandle,

            uint Msg,

            IntPtr wParam,

            IntPtr lParam,

            uint flags,

            uint timeout,

            out IntPtr result);

        [DllImport("oleacc.dll")]

        public static extern int ObjectFromLresult(int lResult, ref Guid
riid, int wParam, ref IHTMLDocument2 ppvObject);

        [ComImport]

        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]

        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]

        interface IServiceProvider
        {

            void QueryService(ref Guid guidService, ref Guid riid,

                [MarshalAs(UnmanagedType.Interface)] out object
ppvObject);

        }

        private static Guid SID_SWebBrowserApp = new
Guid("0002DF05-0000-0000-C000-000000000046");

        private static Guid IID_IHTMLDocument = new
Guid("626FC520-A41E-11CF-A731-00A0C9082637");

        private static Guid IID_IWebBrowser =
typeof(SHDocVw.WebBrowser).GUID;

        [Flags]

        enum SendMessageTimeoutFlags : uint
        {

            SMTO_NORMAL = 0x0000,

            SMTO_BLOCK = 0x0001,

            SMTO_ABORTIFHUNG = 0x0002,

            SMTO_NOTIMEOUTIFNOTHUNG = 0x0008

        }

        public SHDocVw.InternetExplorer GetIEFromAutomationelement(AutomationElement element) //***************
        {

            IntPtr HWND = (IntPtr)element.Current.NativeWindowHandle;

            int hInst = 0;
            IntPtr lres = IntPtr.Zero;
            SHDocVw.InternetExplorer browser = null;
            //System.Windows.Forms.WebBrowser br;
            IHTMLDocument2 htmlDoc = null;
            hInst = LoadLibrary("Oleacc.dll");
            int? addr = GetProcAddress(HWND, "ObjectFromLresult");
            if (addr != null)
            {
                try
                {
                    uint msg = RegisterWindowMessage("WM_HTML_GETOBJECT");
                    SendMessageTimeout(HWND, msg, IntPtr.Zero, IntPtr.Zero, (uint)SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out lres);
                    ObjectFromLresult((int)lres, ref IID_IHTMLDocument, 0, ref htmlDoc);
                    Object o;

                    ((IServiceProvider)htmlDoc).QueryService(ref
SID_SWebBrowserApp, ref IID_IWebBrowser, out o);

                    browser = (SHDocVw.InternetExplorer)o;
                    // br = (System.Windows.Forms.WebBrowser)o;
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
                }

                finally
                {
                    FreeLibrary(hInst);
                }
                return browser;
            }
            else
            {
                return null;
            }
        }
    }
}
