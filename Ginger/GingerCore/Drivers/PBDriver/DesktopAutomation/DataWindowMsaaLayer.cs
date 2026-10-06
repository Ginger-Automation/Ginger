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
using Accessibility;
using Amdocs.Ginger.Common;
using GingerCore.Drivers.Common.LegacyAutomation;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.PBDriver.DesktopAutomation
{
    /// <summary>
    /// Reaches inside a PowerBuilder DataWindow, which is custom drawn and so has
    /// no child HWNDs and no native UIA provider. The control still answers
    /// WM_GETOBJECT, so its cells can be read and written through MSAA, and the
    /// accessibility bridge surfaces those same cells to UIA - which is what gives
    /// each cell the screen rectangle this layer targets. None of that needs focus,
    /// the foreground window, or an unlocked desktop, which is what makes DataWindow
    /// steps runnable unattended.
    /// </summary>
    /// <remarks>
    /// The layer is inert for every window class other than a DataWindow, so it
    /// cannot alter the behaviour of any control that automates correctly today.
    ///
    /// Writing has two routes, because not every DataWindow takes one back through
    /// MSAA. The accessible write is tried first; where the control refuses it the
    /// value is typed in behind a click, but only behind a lock screen, so a machine
    /// the mouse can still reach keeps the route it uses today. Both are verified by
    /// reading the value back, and a write that cannot be confirmed is reported as a
    /// failure rather than left to look like one that worked.
    /// </remarks>
    public sealed class DataWindowMsaaLayer : IDesktopAutomationLayer
    {
        private const uint OBJID_CLIENT = 0xFFFFFFFC;
        private const int CHILDID_SELF = 0;
        private const int SELFLAG_TAKEFOCUS = 0x1;
        private const int SELFLAG_TAKESELECTION = 0x2;
        private const int ReadBackAttempts = 10;
        private const int ReadBackIntervalMs = 100;
        private const int EditorWaitMs = 500;

        private static Guid IID_IAccessible = new Guid("618736e0-3c3d-11cf-810c-00aa00389b71");

        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint objectId, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object accessible);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public string Name => "DataWindowMsaa";

        /// <summary>
        /// PowerBuilder names the DataWindow window class after the runtime build,
        /// for example pbdw100, pbdw110 or pbdw170.
        /// </summary>
        public static bool IsDataWindowClass(string className)
        {
            return !string.IsNullOrEmpty(className)
                && className.StartsWith("pbdw", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The DataWindow's own window, which is the only one there is to talk to.
        /// </summary>
        /// <remarks>
        /// A cell is drawn rather than created as a window, so it reports no handle
        /// of its own and the owning control's has to stand in for it. Reading the
        /// target's handle alone left this layer unable to see the very thing it was
        /// written for: every cell declined here and the step fell through to a
        /// technique that needs the mouse.
        /// </remarks>
        private static IntPtr ResolveDataWindow(DesktopActionContext context)
        {
            return context.NativeWindowHandle != IntPtr.Zero
                ? context.NativeWindowHandle
                : context.PointOwnerWindowHandle;
        }

        public bool CanHandle(DesktopActionContext context)
        {
            IntPtr dataWindow = ResolveDataWindow(context);
            return dataWindow != IntPtr.Zero
                && IsDataWindowClass(Win32Native.GetWindowClassName(dataWindow));
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            IntPtr dataWindowHandle = ResolveDataWindow(context);
            LayerResult result = TryThroughAccessibleObject(context, dataWindowHandle);

            if (result.Status == LayerExecutionStatus.Succeeded || context.Operation != DesktopOperation.SetValue)
            {
                return result;
            }

            return TrySetValueByTyping(context, dataWindowHandle, result.Message);
        }

        private LayerResult TryThroughAccessibleObject(DesktopActionContext context, IntPtr dataWindowHandle)
        {
            try
            {
                if (!TryGetAccessible(dataWindowHandle, out IAccessible dataWindow))
                {
                    return LayerResult.Skip("DataWindow did not answer WM_GETOBJECT");
                }

                if (!TryGetTargetCell(dataWindow, dataWindowHandle, context, out IAccessible cell, out object cellId))
                {
                    return LayerResult.Skip("Target does not resolve to a single DataWindow cell");
                }

                switch (context.Operation)
                {
                    case DesktopOperation.GetValue:
                        return ReadCell(cell, cellId);

                    case DesktopOperation.SetValue:
                        return WriteCell(cell, cellId, context.Value ?? string.Empty);

                    case DesktopOperation.Click:
                        if (!string.IsNullOrEmpty(cell.get_accDefaultAction(cellId)))
                        {
                            cell.accDoDefaultAction(cellId);
                            return LayerResult.Ok(Name, "Click via IAccessible.accDoDefaultAction on the targeted DataWindow cell");
                        }
                        return LayerResult.Skip("DataWindow cell exposes no default action; leaving the click to the later layers");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "DataWindow MSAA layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by the DataWindow accessible object");
        }

        /// <summary>
        /// Writes the cell the way a person would - click into it, type, Tab - for a
        /// DataWindow that hands out its cell values but will not take one back.
        /// </summary>
        /// <remarks>
        /// PowerBuilder answers put_accValue with DISP_E_MEMBERNOTFOUND while
        /// answering get_accValue perfectly well, so a grid whose cells read back
        /// fine can still refuse every pattern and accessible write there is. The
        /// only thing that has ever set those cells is the physical fallback, which
        /// is this same gesture driven by the real mouse and keyboard.
        ///
        /// So this runs only where that cannot: behind a lock screen, or where the
        /// operator asked for window messages. While the mouse can still reach the
        /// machine and nobody has asked otherwise the value is left to it, exactly as
        /// before, because a message click is acknowledged once the window takes it
        /// and standing that in for input that works today would trade a route that
        /// is known to write the cell for one that only reports it did.
        ///
        /// The typing is confirmed by reading the cell back. The messages are queued
        /// rather than delivered in line, so nothing about sending them says the
        /// value arrived, or arrived in the cell that was aimed at.
        /// </remarks>
        private LayerResult TrySetValueByTyping(DesktopActionContext context, IntPtr dataWindow, string accessibleFailure)
        {
            if (context.DesktopCanTakePhysicalInput && !context.PreferWindowMessages)
            {
                return LayerResult.Skip(accessibleFailure + " Leaving the value to the mouse and keyboard.");
            }

            if (!context.HasTargetPoint)
            {
                return LayerResult.Skip(accessibleFailure + " The cell reports no rectangle to click into.");
            }

            string expected = context.Value ?? string.Empty;
            string cell = context.TargetScreenX + "," + context.TargetScreenY;

            Win32KeyMessages.FocusSnapshot beforeClick = Win32KeyMessages.CaptureFocus(dataWindow);
            PointClickOutcome click = Win32Native.ClickAtScreenPoint(
                dataWindow, context.TargetScreenX, context.TargetScreenY, context.TimeoutMs);
            if (click != PointClickOutcome.Delivered)
            {
                return LayerResult.Skip(accessibleFailure + " The DataWindow did not take a click at " + cell + ".");
            }

            // Resolved after the click rather than before it, because the click is
            // what opens the cell, and a DataWindow that edits through a window of
            // its own has not created it until then.
            IntPtr editor = Win32KeyMessages.WaitForEditor(dataWindow, beforeClick, EditorWaitMs);
            if (editor == IntPtr.Zero)
            {
                return LayerResult.Fail(Name, "The cell at " + cell
                    + " was clicked and nothing in the DataWindow took the keyboard, so there was nowhere to type.");
            }

            if (!Win32KeyMessages.SendChars(editor, expected, context.TimeoutMs))
            {
                return LayerResult.Fail(Name, "The cell at " + cell
                    + " was clicked and window " + editor.ToInt64() + " refused the text, so it may hold part of the value.");
            }

            // A Tab that moves nothing says the cell was never in edit mode, which is
            // the difference between a value that was rejected and one that was never
            // offered. It is reported rather than acted on, because the read back
            // below is what decides the outcome.
            bool committed = Win32KeyMessages.SendTab(editor, context.TimeoutMs);

            if (!WaitForCellToRead(context, dataWindow, expected, out string actual))
            {
                return LayerResult.Fail(Name, "The cell at " + cell + " was clicked, window " + editor.ToInt64()
                    + " took the text" + (committed ? string.Empty : " but did not act on the Tab that follows it")
                    + " and the cell now reads " + Describe(actual) + " instead of " + Describe(expected) + ".");
            }

            return LayerResult.Ok(Name, "SetValue via a verified click and typed keys into the targeted DataWindow cell");
        }

        /// <summary>
        /// Quotes a value for a message, so trailing padding and an empty or
        /// unreadable cell can be told apart by whoever reads the run report.
        /// </summary>
        private static string Describe(string value)
        {
            return value == null ? "nothing at all" : "'" + value + "'";
        }

        /// <summary>
        /// Waits for the cell to show what it was typed.
        /// </summary>
        /// <remarks>
        /// The first read can legitimately come back with the old text: the messages
        /// are queued rather than delivered in line, and PowerBuilder commits the
        /// edit in its own time once the Tab reaches it. A short wait is what
        /// separates that from a value the cell never took.
        /// </remarks>
        private static bool WaitForCellToRead(DesktopActionContext context, IntPtr dataWindow, string expected, out string actual)
        {
            actual = null;
            for (int attempt = 0; attempt < ReadBackAttempts; attempt++)
            {
                if (attempt > 0)
                {
                    Thread.Sleep(ReadBackIntervalMs);
                }

                actual = ReadTargetValue(context, dataWindow);
                if (CellHoldsValue(actual, expected))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether the cell now holds the value, allowing for the padding a
        /// DataWindow puts around it.
        /// </summary>
        /// <remarks>
        /// Cells come back padded out to the column width: a character column reads
        /// "2109 Fox Dr, Champaign IL " with the trailing space still on it, and a
        /// right aligned number carries leading ones. Comparing that exactly rejects
        /// a write that worked perfectly well.
        ///
        /// Trimming does not weaken the check. What it has to tell apart is the new
        /// value from the old one, and old contents do not turn into new ones by
        /// losing their padding.
        /// </remarks>
        public static bool CellHoldsValue(string cellValue, string expected)
        {
            return cellValue != null
                && string.Equals(cellValue.Trim(), (expected ?? string.Empty).Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// What the cell now holds, read the way the run's own GetValue steps read
        /// it and, failing that, the way this layer reads it.
        /// </summary>
        /// <remarks>
        /// UIA is asked first because that is the reading a subsequent GetValue step
        /// will take, so it is the one a set has to satisfy. Where the cell carries
        /// no value pattern the accessible object is asked instead, since a write
        /// that landed must not be reported as a failure merely because the
        /// confirming read went down a route this control does not offer.
        /// </remarks>
        private static string ReadTargetValue(DesktopActionContext context, IntPtr dataWindow)
        {
            return ReadThroughValuePattern(context) ?? ReadThroughAccessibleObject(context, dataWindow);
        }

        private static string ReadThroughValuePattern(DesktopActionContext context)
        {
            if (context.AutomationElement is not UIAuto.AutomationElement element)
            {
                return null;
            }

            try
            {
                return element.TryGetCurrentPattern(UIAuto.ValuePattern.Pattern, out object pattern)
                    && pattern is UIAuto.ValuePattern value
                        ? value.Current.Value
                        : null;
            }
            catch (Exception)
            {
                // An unreadable cell is an unverifiable write, which is a failure.
                return null;
            }
        }

        private static string ReadThroughAccessibleObject(DesktopActionContext context, IntPtr dataWindow)
        {
            try
            {
                return TryGetAccessible(dataWindow, out IAccessible accessible)
                    && TryGetTargetCell(accessible, dataWindow, context, out IAccessible cell, out object cellId)
                        ? cell.get_accValue(cellId)
                        : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private LayerResult ReadCell(IAccessible cell, object cellId)
        {
            string value = cell.get_accValue(cellId);
            if (value != null)
            {
                return LayerResult.Ok(Name, "GetValue via IAccessible on the targeted DataWindow cell", value);
            }

            // Static cells such as computed fields carry their text as the name.
            string name = cell.get_accName(cellId);
            if (name != null)
            {
                return LayerResult.Ok(Name, "GetValue via the DataWindow cell accessible name", name);
            }

            return LayerResult.Skip("DataWindow cell exposes neither a value nor a name");
        }

        private LayerResult WriteCell(IAccessible cell, object cellId, string value)
        {
            cell.set_accValue(cellId, value);

            if (!CellHoldsValue(cell.get_accValue(cellId), value))
            {
                return LayerResult.Skip("DataWindow cell did not accept the value through MSAA");
            }

            return LayerResult.Ok(Name, "SetValue via verified IAccessible write to the targeted DataWindow cell");
        }

        private static bool TryGetAccessible(IntPtr hwnd, out IAccessible accessible)
        {
            accessible = null;
            if (AccessibleObjectFromWindow(hwnd, OBJID_CLIENT, ref IID_IAccessible, out object raw) != 0)
            {
                return false;
            }

            accessible = raw as IAccessible;
            return accessible != null;
        }

        /// <summary>
        /// Resolves the cell the step is aimed at by hit testing the centre of the
        /// target's own screen rectangle.
        /// </summary>
        /// <remarks>
        /// The obvious shortcut is to take whichever cell holds focus, but the
        /// target is not necessarily the focused one, and a read or a write that
        /// silently lands on a neighbouring cell reports success while doing the
        /// wrong thing. Hit testing binds the operation to the element that was
        /// actually asked for, and anything that does not resolve to one cell is
        /// declined so the engine moves on to the next layer.
        /// </remarks>
        private static bool TryGetTargetCell(IAccessible dataWindow, IntPtr dataWindowHandle, DesktopActionContext context, out IAccessible cell, out object cellId)
        {
            cell = null;
            cellId = null;

            return TryGetTargetCentre(context, dataWindowHandle, out int x, out int y)
                && TryResolveChild(dataWindow, dataWindow.accHitTest(x, y), out cell, out cellId);
        }

        /// <summary>
        /// Centre of the target element in screen coordinates, but only when that
        /// element is smaller than the DataWindow itself. An element whose
        /// rectangle fills the control is the DataWindow rather than one of its
        /// cells, and its centre points at whatever cell happens to sit in the
        /// middle of the grid.
        /// </summary>
        private static bool TryGetTargetCentre(DesktopActionContext context, IntPtr dataWindowHandle, out int x, out int y)
        {
            x = 0;
            y = 0;

            if (context.AutomationElement is not UIAuto.AutomationElement element)
            {
                return false;
            }

            var target = element.Current.BoundingRectangle;
            if (target.Width <= 0 || target.Height <= 0)
            {
                return false;
            }

            if (!GetWindowRect(dataWindowHandle, out RECT control)
                || (target.Width >= control.right - control.left && target.Height >= control.bottom - control.top))
            {
                return false;
            }

            x = (int)(target.X + target.Width / 2);
            y = (int)(target.Y + target.Height / 2);
            return true;
        }

        /// <summary>
        /// accHitTest answers with either a nested accessible object or a child id
        /// that has to be used against the parent, so both shapes are normalised
        /// here. A child id of CHILDID_SELF means the point landed on the
        /// DataWindow itself rather than on a cell.
        /// </summary>
        private static bool TryResolveChild(IAccessible dataWindow, object hit, out IAccessible cell, out object cellId)
        {
            if (hit is IAccessible nested)
            {
                cell = nested;
                cellId = CHILDID_SELF;
                return true;
            }

            if (hit is int childId && childId != CHILDID_SELF)
            {
                cell = dataWindow;
                cellId = childId;
                return true;
            }

            cell = null;
            cellId = null;
            return false;
        }
    }
}
