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

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    public enum DesktopOperation
    {
        Click,
        DoubleClick,

        /// <remarks>
        /// Served by window messages or by the mouse and nothing in between. Neither
        /// UIA nor MSAA exposes a right click - there is no pattern for it and no
        /// accessible action that means it - so unlike a left click this one has no
        /// quiet route above the Win32 layer to fall through from.
        /// </remarks>
        RightClick,

        /// <remarks>
        /// Ctrl held for the duration of one click, which is how a list or tree is
        /// told to add to its selection rather than replace it.
        /// </remarks>
        ControlClick,
        SetValue,
        GetValue
    }

    /// <summary>
    /// What became of a click aimed at a screen point.
    /// </summary>
    /// <remarks>
    /// The two failures have to be told apart. A point no window owns is a locator or
    /// a layout problem; an application that stopped answering is a timing or a load
    /// one. Reporting both as "the point is not inside the window" produced a refusal
    /// that flatly contradicted the bounds printed beside it.
    /// </remarks>
    public enum PointClickOutcome
    {
        /// <summary>
        /// The clicks were queued on a window whose thread is alive and pumping.
        /// Whether they had the intended effect is for the action to validate.
        /// </summary>
        Delivered,

        /// <summary>No window under the given point, so nothing was sent.</summary>
        PointNotOwned,

        /// <summary>
        /// The clicks could not be queued, or the owning window stopped answering.
        /// </summary>
        NotAcknowledged
    }

    public sealed class DesktopActionContext
    {
        public DesktopOperation Operation { get; set; }
        public object AutomationElement { get; set; }
        public IntPtr NativeWindowHandle { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Only the call sites that already moved the real mouse before this engine
        /// existed may enable this. Physical input is the last resort because it
        /// clicks screen coordinates and hits whatever window is on top.
        /// </summary>
        public bool AllowPhysicalInput { get; set; }

        /// <summary>
        /// Whether the real mouse can currently land on the application, which is a
        /// different question from whether this caller is allowed to use it.
        /// </summary>
        /// <remarks>
        /// Kept apart from <see cref="AllowPhysicalInput"/> because most call sites
        /// never used the mouse and so switch it off permanently, which says nothing
        /// about the desktop. Only this one licenses a message click to stand in for
        /// the mouse: a posted click is acknowledged once the window takes it, which
        /// is not the same as the control acting on it, so substituting one where the
        /// mouse still works would turn a click that honestly failed into one that
        /// reports success and never happened.
        /// </remarks>
        public bool DesktopCanTakePhysicalInput { get; set; }

        /// <summary>
        /// Whether the operator asked this agent to work through window messages, so
        /// a layer that would otherwise stand aside for a working mouse should go
        /// ahead instead.
        /// </summary>
        /// <remarks>
        /// <see cref="DesktopCanTakePhysicalInput"/> answers whether the mouse
        /// reaches the application; this answers whether it should be used. The two
        /// were the same question while the only reason to send messages was a locked
        /// screen, and collapsing them left the non-intrusive setting with no effect
        /// at all on an unlocked desktop: every layer saw a working mouse, stood
        /// aside for it, and the run took over the screen exactly as before.
        /// </remarks>
        public bool PreferWindowMessages { get; set; }

        /// <summary>
        /// Centre of the target in screen coordinates, when the target reports a
        /// rectangle. This is the same point the physical mouse would have been sent
        /// to, which lets a layer aim at a control that exposes no pattern, no
        /// accessible action and no HWND of its own - a grid cell being the usual one.
        /// </summary>
        public bool HasTargetPoint { get; set; }
        public int TargetScreenX { get; set; }
        public int TargetScreenY { get; set; }

        /// <summary>
        /// Nearest ancestor window that actually owns the target's pixels, used only to
        /// aim a coordinate click. It is kept apart from
        /// <see cref="NativeWindowHandle"/> on purpose: that one stays the target's own
        /// handle, so the layers that decide what they can serve from the window class
        /// keep seeing exactly what they see today.
        /// </summary>
        public IntPtr PointOwnerWindowHandle { get; set; }

        public int TimeoutMs { get; set; } = 5000;
    }

    public enum LayerExecutionStatus
    {
        Skipped,
        Failed,
        Succeeded
    }

    public sealed class LayerResult
    {
        public LayerExecutionStatus Status { get; private set; }
        public string LayerName { get; set; }
        public string Message { get; set; }
        public string OutputValue { get; set; }

        public static LayerResult Skip(string message)
        {
            return new LayerResult { Status = LayerExecutionStatus.Skipped, Message = message };
        }

        public static LayerResult Ok(string layerName, string message, string outputValue = null)
        {
            return new LayerResult { Status = LayerExecutionStatus.Succeeded, LayerName = layerName, Message = message, OutputValue = outputValue };
        }

        public static LayerResult Fail(string layerName, string message)
        {
            return new LayerResult { Status = LayerExecutionStatus.Failed, LayerName = layerName, Message = message };
        }
    }

    public interface IDesktopAutomationLayer
    {
        string Name { get; }
        bool CanHandle(DesktopActionContext context);
        LayerResult TryExecute(DesktopActionContext context);
    }

    public sealed class DesktopEngineResult
    {
        public bool Success { get; set; }
        public string UsedLayer { get; set; }
        public string ExecutionInfo { get; set; }
        public string ErrorMessage { get; set; }
        public string OutputValue { get; set; }
    }
}
