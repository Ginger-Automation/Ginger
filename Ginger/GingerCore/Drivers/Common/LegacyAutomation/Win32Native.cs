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

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Window message helpers for the Win32 layer. Every call goes through
    /// SendMessageTimeout so an unresponsive application cannot block the run.
    /// </summary>
    public static class Win32Native
    {
        private const int WM_SETTEXT = 0x000C;
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int EM_SETSEL = 0x00B1;
        private const int EM_REPLACESEL = 0x00C2;
        private const int BM_CLICK = 0x00F5;
        private const int GWL_STYLE = -16;
        private const long WS_CHILD = 0x40000000L;
        private const uint SMTO_BLOCK = 0x0001;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, string lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, StringBuilder lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowEnabled(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong32(IntPtr hWnd, int index);

        /// <summary>
        /// Writes text to a child edit control and confirms it by reading the value
        /// back, so a message the control silently ignored is reported as a failure.
        /// </summary>
        public static bool SetControlText(IntPtr hwnd, string value, int timeoutMs)
        {
            if (!IsChildWindow(hwnd) || !IsEditClass(hwnd))
            {
                return false;
            }

            string expected = value ?? string.Empty;
            if (TrySend(hwnd, WM_SETTEXT, IntPtr.Zero, expected, timeoutMs)
                && string.Equals(ReadText(hwnd, timeoutMs), expected, StringComparison.Ordinal))
            {
                return true;
            }

            // Some native edit controls ignore WM_SETTEXT but accept a replacement of
            // the current selection, which still needs no foreground keyboard input.
            if (!TrySend(hwnd, EM_SETSEL, IntPtr.Zero, new IntPtr(-1), timeoutMs)
                || !TrySend(hwnd, EM_REPLACESEL, new IntPtr(1), expected, timeoutMs))
            {
                return false;
            }

            return string.Equals(ReadText(hwnd, timeoutMs), expected, StringComparison.Ordinal);
        }

        /// <summary>
        /// Reads text from a child edit control only. A top-level HWND would answer
        /// WM_GETTEXT with the window caption, which is never the control value.
        /// </summary>
        public static string GetControlText(IntPtr hwnd, int timeoutMs)
        {
            return IsChildWindow(hwnd) && IsEditClass(hwnd) ? ReadText(hwnd, timeoutMs) : null;
        }

        public static bool ClickButton(IntPtr hwnd, int timeoutMs)
        {
            if (!IsChildWindow(hwnd) || !IsWindowEnabled(hwnd) || !IsButtonClass(hwnd))
            {
                return false;
            }

            return TrySend(hwnd, BM_CLICK, IntPtr.Zero, IntPtr.Zero, timeoutMs);
        }

        public static bool IsChildWindow(IntPtr hwnd)
        {
            return hwnd != IntPtr.Zero
                && IsWindow(hwnd)
                && (GetWindowStyle(hwnd) & WS_CHILD) != 0;
        }

        public static string GetWindowClassName(IntPtr hwnd)
        {
            StringBuilder className = new StringBuilder(256);
            return GetClassName(hwnd, className, className.Capacity) > 0 ? className.ToString() : string.Empty;
        }

        /// <summary>
        /// Uses GetWindowLongPtr on 64-bit processes and GetWindowLong on 32-bit so
        /// the Win32 layer does not throw EntryPointNotFoundException.
        /// </summary>
        public static long GetWindowStyle(IntPtr hwnd)
        {
            if (IntPtr.Size == 8)
            {
                return GetWindowLongPtr64(hwnd, GWL_STYLE).ToInt64();
            }

            return GetWindowLong32(hwnd, GWL_STYLE);
        }

        public static bool IsButtonClass(IntPtr hwnd)
        {
            string className = GetWindowClassName(hwnd);
            return string.Equals(className, "Button", StringComparison.OrdinalIgnoreCase)
                || className.StartsWith("WindowsForms10.BUTTON", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsEditClass(IntPtr hwnd)
        {
            string className = GetWindowClassName(hwnd);
            return className.Equals("Edit", StringComparison.OrdinalIgnoreCase)
                || className.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ReadText(IntPtr hwnd, int timeoutMs)
        {
            if (!TrySend(hwnd, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero, timeoutMs, out IntPtr lengthResult))
            {
                return null;
            }

            int length = lengthResult.ToInt32();
            if (length <= 0)
            {
                return string.Empty;
            }

            StringBuilder buffer = new StringBuilder(length + 1);
            return TrySend(hwnd, WM_GETTEXT, (IntPtr)buffer.Capacity, buffer, timeoutMs) ? buffer.ToString() : null;
        }

        private static uint NormalizeTimeout(int timeoutMs)
        {
            return (uint)Math.Max(100, Math.Min(timeoutMs, 30000));
        }

        private static bool TrySend(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, int timeoutMs)
        {
            return TrySend(hwnd, message, wParam, lParam, timeoutMs, out _);
        }

        private static bool TrySend(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, int timeoutMs, out IntPtr result)
        {
            return SendMessageTimeout(hwnd, message, wParam, lParam, SMTO_BLOCK | SMTO_ABORTIFHUNG, NormalizeTimeout(timeoutMs), out result) != IntPtr.Zero;
        }

        private static bool TrySend(IntPtr hwnd, int message, IntPtr wParam, string lParam, int timeoutMs)
        {
            return SendMessageTimeout(hwnd, message, wParam, lParam, SMTO_BLOCK | SMTO_ABORTIFHUNG, NormalizeTimeout(timeoutMs), out _) != IntPtr.Zero;
        }

        private static bool TrySend(IntPtr hwnd, int message, IntPtr wParam, StringBuilder lParam, int timeoutMs)
        {
            return SendMessageTimeout(hwnd, message, wParam, lParam, SMTO_BLOCK | SMTO_ABORTIFHUNG, NormalizeTimeout(timeoutMs), out _) != IntPtr.Zero;
        }
    }
}
