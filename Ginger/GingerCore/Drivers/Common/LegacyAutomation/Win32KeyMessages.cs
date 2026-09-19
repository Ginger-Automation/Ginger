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

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Delivers keystrokes through window messages instead of the physical
    /// keyboard, so Ginger can type into edit / button HWNDs without the target
    /// window being in the foreground or the desktop being unlocked.
    /// </summary>
    public static class Win32KeyMessages
    {
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_CHAR = 0x0102;

        private const ushort VK_TAB = 0x09;

        private const uint SMTO_BLOCK = 0x0001;
        private const uint SMTO_ABORTIFHUNG = 0x0002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        /// <summary>
        /// Returns the focused child HWND inside the same thread as
        /// <paramref name="topLevelHwnd"/>, or <see cref="IntPtr.Zero"/> when the
        /// focus cannot be narrowed to a child. A key message posted to a frame
        /// returns success while doing nothing, so the caller must treat
        /// <see cref="IntPtr.Zero"/> as a failure rather than falling back.
        /// </summary>
        public static IntPtr ResolveFocusedChild(IntPtr topLevelHwnd)
        {
            if (topLevelHwnd == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            uint threadId = GetWindowThreadProcessId(topLevelHwnd, out _);
            if (threadId == 0)
            {
                return IntPtr.Zero;
            }

            GUITHREADINFO info = new GUITHREADINFO { cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO)) };
            if (GetGUIThreadInfo(threadId, ref info) && Win32Native.IsChildWindow(info.hwndFocus))
            {
                return info.hwndFocus;
            }

            return Win32Native.IsChildWindow(topLevelHwnd) ? topLevelHwnd : IntPtr.Zero;
        }

        /// <summary>
        /// Sends VK_TAB down + up to <paramref name="hwnd"/> via window messages.
        /// </summary>
        public static bool SendTab(IntPtr hwnd, int timeoutMs)
        {
            return SendKey(hwnd, VK_TAB, timeoutMs);
        }

        /// <summary>
        /// Sends a single virtual-key down + up via window messages.
        /// </summary>
        private static bool SendKey(IntPtr hwnd, ushort virtualKey, int timeoutMs)
        {
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }

            uint normalized = NormalizeTimeout(timeoutMs);
            bool down = SendMessageTimeout(hwnd, WM_KEYDOWN, (IntPtr)virtualKey, IntPtr.Zero, SMTO_BLOCK | SMTO_ABORTIFHUNG, normalized, out _) != IntPtr.Zero;
            bool up = SendMessageTimeout(hwnd, WM_KEYUP, (IntPtr)virtualKey, IntPtr.Zero, SMTO_BLOCK | SMTO_ABORTIFHUNG, normalized, out _) != IntPtr.Zero;
            return down && up;
        }

        /// <summary>
        /// Types a literal Unicode string through <c>WM_CHAR</c>. The text is sent
        /// verbatim, so SendKeys-style tokens and modifiers are not interpreted.
        /// </summary>
        /// <remarks>
        /// Sending stops at the first character the window refuses, so the caller
        /// sees a failure and retypes the whole value. Pushing the rest of the
        /// string after a refusal would leave the control holding text that is
        /// neither the old value nor the new one.
        /// </remarks>
        public static bool SendChars(IntPtr hwnd, string text, int timeoutMs)
        {
            if (hwnd == IntPtr.Zero)
            {
                return false;
            }
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            uint normalized = NormalizeTimeout(timeoutMs);
            foreach (char c in text)
            {
                if (SendMessageTimeout(hwnd, WM_CHAR, (IntPtr)c, IntPtr.Zero, SMTO_BLOCK | SMTO_ABORTIFHUNG, normalized, out _) == IntPtr.Zero)
                {
                    return false;
                }
            }
            return true;
        }

        private static uint NormalizeTimeout(int timeoutMs)
        {
            return (uint)Math.Max(100, Math.Min(timeoutMs <= 0 ? 2000 : timeoutMs, 30000));
        }
    }
}
