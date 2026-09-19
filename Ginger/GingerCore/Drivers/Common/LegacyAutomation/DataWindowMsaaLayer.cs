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
using System;
using System.Runtime.InteropServices;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common.LegacyAutomation
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
    /// Writes are verified by reading the value back, and a DataWindow that does
    /// not honour the write is reported as skipped so the engine moves on to the
    /// layer that runs today.
    /// </remarks>
    public sealed class DataWindowMsaaLayer : IDesktopAutomationLayer
    {
        private const uint OBJID_CLIENT = 0xFFFFFFFC;
        private const int CHILDID_SELF = 0;
        private const int SELFLAG_TAKEFOCUS = 0x1;
        private const int SELFLAG_TAKESELECTION = 0x2;

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

        public bool CanHandle(DesktopActionContext context)
        {
            return context.NativeWindowHandle != IntPtr.Zero
                && IsDataWindowClass(Win32Native.GetWindowClassName(context.NativeWindowHandle));
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            try
            {
                if (!TryGetAccessible(context.NativeWindowHandle, out IAccessible dataWindow))
                {
                    return LayerResult.Skip("DataWindow did not answer WM_GETOBJECT");
                }

                if (!TryGetTargetCell(dataWindow, context, out IAccessible cell, out object cellId))
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
                        cell.accSelect(SELFLAG_TAKEFOCUS | SELFLAG_TAKESELECTION, cellId);
                        return LayerResult.Ok(Name, "Click via IAccessible.accSelect on the targeted DataWindow cell");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "DataWindow MSAA layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by the DataWindow accessible object");
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

            string readBack = cell.get_accValue(cellId);
            if (!string.Equals(readBack ?? string.Empty, value, StringComparison.Ordinal))
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
        private static bool TryGetTargetCell(IAccessible dataWindow, DesktopActionContext context, out IAccessible cell, out object cellId)
        {
            cell = null;
            cellId = null;

            return TryGetTargetCentre(context, out int x, out int y)
                && TryResolveChild(dataWindow, dataWindow.accHitTest(x, y), out cell, out cellId);
        }

        /// <summary>
        /// Centre of the target element in screen coordinates, but only when that
        /// element is smaller than the DataWindow itself. An element whose
        /// rectangle fills the control is the DataWindow rather than one of its
        /// cells, and its centre points at whatever cell happens to sit in the
        /// middle of the grid.
        /// </summary>
        private static bool TryGetTargetCentre(DesktopActionContext context, out int x, out int y)
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

            if (!GetWindowRect(context.NativeWindowHandle, out RECT control)
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
