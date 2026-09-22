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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Window message helpers for the Win32 layer. Nothing here can block the run:
    /// the messages that ask a control something go through SendMessageTimeout, and
    /// the mouse buttons are posted, because a control that answers a press by
    /// capturing the mouse will not release a press delivered in line.
    /// </summary>
    public static class Win32Native
    {
        private const int WM_NULL = 0x0000;
        private const int WM_SETTEXT = 0x000C;
        private const int WM_GETTEXT = 0x000D;
        private const int WM_GETTEXTLENGTH = 0x000E;
        private const int EM_SETSEL = 0x00B1;
        private const int EM_REPLACESEL = 0x00C2;
        private const int BM_CLICK = 0x00F5;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_LBUTTONDBLCLK = 0x0203;
        private const int MK_LBUTTON = 0x0001;
        private const uint CWP_SKIPINVISIBLE = 0x0001;
        private const uint CWP_SKIPDISABLED = 0x0002;
        private const uint CWP_SKIPTRANSPARENT = 0x0004;
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

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr ChildWindowFromPointEx(IntPtr parent, POINT point, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
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

        /// <summary>
        /// Clicks a point inside a control by sending the button messages the mouse
        /// would have produced, which reaches a custom drawn target that exposes no
        /// pattern, no accessible action and no button class - a grid cell being the
        /// usual one - without needing the foreground window or an unlocked desktop.
        /// </summary>
        /// <remarks>
        /// The point is given in screen coordinates because that is what an element
        /// rectangle reports, and it is rejected unless it lands inside the client
        /// area of <paramref name="hwnd"/>. A message carrying a point the window does
        /// not own would be answered as handled while doing nothing, or would act on
        /// the wrong place in the control.
        /// </remarks>
        public static PointClickOutcome ClickAtScreenPoint(IntPtr hwnd, int screenX, int screenY, int timeoutMs)
        {
            if (!TryResolveClientPoint(hwnd, screenX, screenY, out IntPtr target, out IntPtr position))
            {
                return PointClickOutcome.PointNotOwned;
            }

            return ConfirmThreadIsPumping(target, PostPress(target, position), timeoutMs);
        }

        /// <summary>
        /// Double clicks a point inside a control by sending the button messages the
        /// mouse would have produced, for the same targets and the same reasons as
        /// <see cref="ClickAtScreenPoint"/>.
        /// </summary>
        /// <remarks>
        /// The second press is WM_LBUTTONDBLCLK rather than another WM_LBUTTONDOWN,
        /// because that message is the only thing that tells a control a double click
        /// happened: two ordinary presses read as two single clicks however close
        /// together they arrive, since nothing here goes through the system double
        /// click timer that the real mouse relies on.
        /// </remarks>
        public static PointClickOutcome DoubleClickAtScreenPoint(IntPtr hwnd, int screenX, int screenY, int timeoutMs)
        {
            if (!TryResolveClientPoint(hwnd, screenX, screenY, out IntPtr target, out IntPtr position))
            {
                return PointClickOutcome.PointNotOwned;
            }

            bool queued = PostPress(target, position)
                && PostMessage(target, WM_LBUTTONDBLCLK, new IntPtr(MK_LBUTTON), position)
                && PostMessage(target, WM_LBUTTONUP, IntPtr.Zero, position);

            return ConfirmThreadIsPumping(target, queued, timeoutMs);
        }

        /// <summary>
        /// Queues a press rather than delivering it in line.
        /// </summary>
        /// <remarks>
        /// Posting is what makes this work on a button. A button captures the mouse on
        /// WM_LBUTTONDOWN and runs its own loop until the matching release arrives, so
        /// a press delivered in line never comes back: the release that would end that
        /// loop is stuck behind the very call waiting for it. The deadlock ran to the
        /// timeout and was then reported as a point the window did not own, which sent
        /// the investigation at the wrong half of the problem.
        /// </remarks>
        private static bool PostPress(IntPtr target, IntPtr position)
        {
            return PostMessage(target, WM_LBUTTONDOWN, new IntPtr(MK_LBUTTON), position)
                && PostMessage(target, WM_LBUTTONUP, IntPtr.Zero, position);
        }

        /// <summary>
        /// Confirms the window that was just clicked belongs to a thread that is alive
        /// and pumping, so a click queued onto a dead or hung window is not reported as
        /// done.
        /// </summary>
        /// <remarks>
        /// WM_NULL is used because it does nothing: it cannot disturb the control, and
        /// answering it at all requires the owning thread to be pumping. It is worth
        /// being exact about what this does and does not establish. Windows dispatches
        /// a sent message ahead of the posted ones already queued, so a reply here does
        /// not prove the clicks have been consumed yet - only that the thread taking
        /// them is running. Proof that a click had its intended effect belongs to the
        /// action's own validation, exactly as it did when the mouse was doing this.
        /// </remarks>
        private static PointClickOutcome ConfirmThreadIsPumping(IntPtr target, bool queued, int timeoutMs)
        {
            if (!queued)
            {
                return PointClickOutcome.NotAcknowledged;
            }

            return TrySend(target, WM_NULL, IntPtr.Zero, IntPtr.Zero, timeoutMs)
                ? PointClickOutcome.Delivered
                : PointClickOutcome.NotAcknowledged;
        }

        /// <summary>
        /// Resolves a screen point to the deepest window that owns it and the lParam
        /// the button messages need, or fails if no descendant owns the point.
        /// </summary>
        private static bool TryResolveClientPoint(IntPtr hwnd, int screenX, int screenY, out IntPtr target, out IntPtr position)
        {
            position = IntPtr.Zero;
            target = ResolveWindowOwningPoint(hwnd, screenX, screenY);
            if (target == IntPtr.Zero)
            {
                return false;
            }

            POINT point = new POINT { x = screenX, y = screenY };
            if (!ScreenToClient(target, ref point))
            {
                return false;
            }

            position = MakePoint(point.x, point.y);
            return true;
        }

        /// <summary>
        /// Finds the deepest descendant of <paramref name="hwnd"/> that owns a screen
        /// point, so a click lands on the control drawing that point rather than on the
        /// frame around it.
        /// </summary>
        /// <remarks>
        /// A grid cell is drawn rather than created as a window, so the handle a caller
        /// has is the containing control or the frame, not the cell. Descending stays
        /// inside that window's own subtree, which is what keeps this from ever
        /// reaching another application - unlike resolving the point globally, where
        /// any window that happened to sit on top would be clicked instead.
        /// </remarks>
        private static IntPtr ResolveWindowOwningPoint(IntPtr hwnd, int screenX, int screenY)
        {
            if (!ClientAreaContains(hwnd, screenX, screenY))
            {
                return IntPtr.Zero;
            }

            IntPtr current = hwnd;
            // Bounded so a provider that answers with its own handle cannot spin here.
            for (int depth = 0; depth < 16; depth++)
            {
                POINT point = new POINT { x = screenX, y = screenY };
                if (!ScreenToClient(current, ref point))
                {
                    break;
                }

                IntPtr child = ChildWindowFromPointEx(current, point, CWP_SKIPINVISIBLE | CWP_SKIPDISABLED | CWP_SKIPTRANSPARENT);
                if (child == IntPtr.Zero || child == current || !ClientAreaContains(child, screenX, screenY))
                {
                    break;
                }

                current = child;
            }

            return current;
        }

        /// <summary>
        /// True when the point lands inside the client area of an enabled window. A
        /// message carrying a point the window does not own would be answered as
        /// handled while acting on the wrong place, or on nothing at all.
        /// </summary>
        private static bool ClientAreaContains(IntPtr hwnd, int screenX, int screenY)
        {
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd) || !IsWindowEnabled(hwnd))
            {
                return false;
            }

            POINT point = new POINT { x = screenX, y = screenY };
            return ScreenToClient(hwnd, ref point)
                && GetClientRect(hwnd, out RECT client)
                && point.x >= client.left && point.x < client.right
                && point.y >= client.top && point.y < client.bottom;
        }

        /// <summary>
        /// Describes where a window actually is, next to the point it was asked to
        /// take, so a refusal can be acted on instead of merely noted.
        /// </summary>
        /// <remarks>
        /// A window somewhere else entirely, the wrong window, a minimised one parked
        /// at the coordinates Windows gives those, and a rectangle measured at a
        /// different scaling than the one the messages use are four different problems
        /// needing four different fixes, and they all surface as the same refusal. The
        /// bounds and the DPI are what tell them apart.
        /// </remarks>
        public static string DescribePointOwnership(IntPtr hwnd, int screenX, int screenY)
        {
            if (hwnd == IntPtr.Zero || !IsWindow(hwnd))
            {
                return "window " + hwnd.ToInt64() + " does not exist";
            }

            StringBuilder description = new StringBuilder();
            description.Append("window ").Append(hwnd.ToInt64())
                .Append(" class '").Append(GetWindowClassName(hwnd)).Append('\'');

            description.Append(GetWindowRect(hwnd, out RECT bounds)
                ? ", on screen at (" + bounds.left + "," + bounds.top + ")-(" + bounds.right + "," + bounds.bottom + ")"
                : ", screen position unavailable");

            if (GetClientRect(hwnd, out RECT client))
            {
                description.Append(", client area ")
                    .Append(client.right - client.left).Append('x').Append(client.bottom - client.top);
            }

            if (!IsWindowVisible(hwnd)) { description.Append(", hidden"); }
            if (!IsWindowEnabled(hwnd)) { description.Append(", disabled"); }
            if (IsIconic(hwnd)) { description.Append(", minimised"); }

            // Reported because the rectangle a target is measured from and the
            // coordinates a window message is resolved in do not have to be the same
            // scale, and a mismatch moves every coordinate click by that factor.
            // Guarded because this is the one reading here that is not available on
            // every supported Windows version, and it is taken while explaining a
            // click that already failed - losing the scale is a worse diagnosis, but
            // losing the whole explanation to an EntryPointNotFoundException would
            // replace it with a missing-export message that says nothing about the
            // click at all.
            string dpi = TryDescribeDpi(hwnd);
            if (dpi != null)
            {
                description.Append(", ").Append(dpi).Append(" dpi");
            }

            description.Append("; asked for ").Append(screenX).Append(',').Append(screenY);
            return description.ToString();
        }

        /// <summary>
        /// The window's dots per inch, or null where this Windows version cannot say.
        /// </summary>
        /// <remarks>
        /// GetDpiForWindow arrived in Windows 10 1607, so on anything earlier the
        /// import resolves to nothing and calling it throws rather than returning a
        /// failure code.
        /// </remarks>
        private static string TryDescribeDpi(IntPtr hwnd)
        {
            try
            {
                uint dpi = GetDpiForWindow(hwnd);
                return dpi > 0 ? dpi.ToString(CultureInfo.InvariantCulture) : null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Packs a client point into the lParam layout the mouse messages expect.
        /// </summary>
        private static IntPtr MakePoint(int x, int y)
        {
            return new IntPtr(((y & 0xFFFF) << 16) | (x & 0xFFFF));
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
