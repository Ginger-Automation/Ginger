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

using Amdocs.Ginger.Common;
using System;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Talks to the control through window messages. This reaches legacy controls
    /// that expose no usable UIA or MSAA pattern, and unlike physical input it needs
    /// neither focus nor the foreground window.
    /// </summary>
    public sealed class Win32Layer : IDesktopAutomationLayer
    {
        public string Name => "Win32";

        public bool CanHandle(DesktopActionContext context)
        {
            if (context.NativeWindowHandle != IntPtr.Zero)
            {
                return true;
            }

            // A target point on a known window is enough on its own: a grid cell is
            // drawn rather than created as a window, so it has no handle of its own and
            // would otherwise never reach this layer at all. It serves the clicks only
            // - reading and writing a value both need the target's own handle, which a
            // context like this does not have, so claiming them would put a technique
            // in the attempt log that was never going to be tried.
            return context.HasTargetPoint
                && context.PointOwnerWindowHandle != IntPtr.Zero
                && (context.Operation == DesktopOperation.Click || context.Operation == DesktopOperation.DoubleClick);
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            try
            {
                IntPtr hwnd = context.NativeWindowHandle;
                switch (context.Operation)
                {
                    case DesktopOperation.Click:
                        // Asked before BM_CLICK, not after it. A click that reached
                        // both patterns and neither took it went to the mouse before
                        // this chain existed, and a button message arriving there
                        // instead would change what an ordinary unlocked run does to
                        // every button whose Invoke and default action both decline.
                        // Permission alone is enough to stand aside here because the
                        // mapper only grants it on a desktop that can take it.
                        if (context.AllowPhysicalInput)
                        {
                            return LayerResult.Skip("Physical input is permitted and working; leaving the click to it");
                        }

                        if (Win32Native.ClickButton(hwnd, context.TimeoutMs))
                        {
                            return LayerResult.Ok(Name, "Click via BM_CLICK");
                        }

                        // Stands in for the mouse, so it runs only where the mouse is
                        // not on the table. The desktop is asked rather than the
                        // caller's own permission: most call sites switch physical
                        // input off because they never used it, which says nothing
                        // about whether it works, and treating that as licence to post
                        // a click would change what every one of them does on an
                        // ordinary unlocked desktop. Behind a lock screen the mouse
                        // reaches nothing, and this is the only way such a target can
                        // still be clicked. An operator who asked for window messages
                        // has said the same thing about a desktop that is not locked,
                        // which is the one case where a working mouse is not a reason
                        // to stand aside.
                        if (context.DesktopCanTakePhysicalInput && !context.PreferWindowMessages)
                        {
                            return LayerResult.Skip("HWND is not an enabled button control; leaving the click to physical input");
                        }
                        if (!context.HasTargetPoint)
                        {
                            return LayerResult.Skip("HWND is not an enabled button control and the target reports no rectangle to aim at");
                        }
                        IntPtr pointOwner = context.PointOwnerWindowHandle != IntPtr.Zero ? context.PointOwnerWindowHandle : hwnd;
                        PointClickOutcome clickOutcome = Win32Native.ClickAtScreenPoint(
                            pointOwner, context.TargetScreenX, context.TargetScreenY, context.TimeoutMs);
                        if (clickOutcome == PointClickOutcome.Delivered)
                        {
                            return LayerResult.Ok(Name, "Click via WM_LBUTTONDOWN/UP at the target point");
                        }

                        return LayerResult.Skip(DescribeRefusedPoint(context, pointOwner, clickOutcome));

                    case DesktopOperation.DoubleClick:
                        // No BM_CLICK equivalent to try first: that message activates a
                        // button once and carries no notion of a double click, so the
                        // point is the only thing to aim at here.
                        if (context.AllowPhysicalInput
                            || (context.DesktopCanTakePhysicalInput && !context.PreferWindowMessages))
                        {
                            return LayerResult.Skip("Leaving the double click to physical input");
                        }
                        if (!context.HasTargetPoint)
                        {
                            return LayerResult.Skip("The target reports no rectangle to aim a double click at");
                        }

                        IntPtr doubleClickOwner = context.PointOwnerWindowHandle != IntPtr.Zero ? context.PointOwnerWindowHandle : hwnd;
                        PointClickOutcome doubleClickOutcome = Win32Native.DoubleClickAtScreenPoint(
                            doubleClickOwner, context.TargetScreenX, context.TargetScreenY, context.TimeoutMs);
                        if (doubleClickOutcome == PointClickOutcome.Delivered)
                        {
                            return LayerResult.Ok(Name, "DoubleClick via WM_LBUTTONDBLCLK at the target point");
                        }

                        return LayerResult.Skip(DescribeRefusedPoint(context, doubleClickOwner, doubleClickOutcome));

                    // Right click and Ctrl click share this shape because neither has
                    // anything to try before the point: BM_CLICK presses a button with
                    // the left button and carries no notion of which button or which
                    // modifier, so the coordinates are all there is to aim at.
                    case DesktopOperation.RightClick:
                    case DesktopOperation.ControlClick:
                        bool isRightClick = context.Operation == DesktopOperation.RightClick;
                        if (context.AllowPhysicalInput
                            || (context.DesktopCanTakePhysicalInput && !context.PreferWindowMessages))
                        {
                            return LayerResult.Skip("Leaving the " + (isRightClick ? "right click" : "Ctrl click")
                                + " to physical input");
                        }
                        if (!context.HasTargetPoint)
                        {
                            return LayerResult.Skip("The target reports no rectangle to aim a "
                                + (isRightClick ? "right click" : "Ctrl click") + " at");
                        }

                        IntPtr buttonClickOwner = context.PointOwnerWindowHandle != IntPtr.Zero ? context.PointOwnerWindowHandle : hwnd;
                        PointClickOutcome buttonClickOutcome = isRightClick
                            ? Win32Native.RightClickAtScreenPoint(
                                buttonClickOwner, context.TargetScreenX, context.TargetScreenY, context.TimeoutMs)
                            : Win32Native.ControlClickAtScreenPoint(
                                buttonClickOwner, context.TargetScreenX, context.TargetScreenY, context.TimeoutMs);
                        if (buttonClickOutcome == PointClickOutcome.Delivered)
                        {
                            return LayerResult.Ok(Name, isRightClick
                                ? "RightClick via WM_RBUTTONDOWN/UP at the target point"
                                : "ControlClick via WM_LBUTTONDOWN/UP with MK_CONTROL at the target point");
                        }

                        return LayerResult.Skip(DescribeRefusedPoint(context, buttonClickOwner, buttonClickOutcome));

                    case DesktopOperation.SetValue:
                        if (Win32Native.SetControlText(hwnd, context.Value ?? string.Empty, context.TimeoutMs))
                        {
                            return LayerResult.Ok(Name, "SetValue via verified WM_SETTEXT/EM_REPLACESEL");
                        }
                        return LayerResult.Skip("HWND is not an edit control or the text did not read back");

                    case DesktopOperation.GetValue:
                        string text = Win32Native.GetControlText(hwnd, context.TimeoutMs);
                        if (text != null)
                        {
                            return LayerResult.Ok(Name, "GetValue via WM_GETTEXT", text);
                        }
                        return LayerResult.Skip("WM_GETTEXT is not available on this HWND");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Win32 layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by window messages");
        }

        /// <summary>
        /// Why a coordinate click was refused, in enough detail to act on.
        /// </summary>
        /// <remarks>
        /// Both handles are named because they are chosen separately: the target's own
        /// window comes from the element, the one aimed at is the nearest ancestor that
        /// owns pixels, and a refusal caused by picking the wrong ancestor looks exactly
        /// like one caused by a target that has moved.
        /// </remarks>
        private static string DescribeRefusedPoint(DesktopActionContext context, IntPtr pointOwner, PointClickOutcome outcome)
        {
            string cause = outcome == PointClickOutcome.PointNotOwned
                ? "The point " + context.TargetScreenX + "," + context.TargetScreenY + " is not inside the window it was aimed at."
                : "The application did not take the click at " + context.TargetScreenX + "," + context.TargetScreenY
                    + ": its window stopped answering within " + context.TimeoutMs + "ms, so it is hung or not pumping messages.";

            return cause + " Target's own window " + context.NativeWindowHandle.ToInt64() + "; aimed at "
                + Win32Native.DescribePointOwnership(pointOwner, context.TargetScreenX, context.TargetScreenY);
        }
    }
}
