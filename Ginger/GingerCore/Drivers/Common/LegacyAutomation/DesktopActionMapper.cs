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
using System;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    public static class DesktopActionMapper
    {
        public static DesktopActionContext FromElement(UIAuto.AutomationElement element, DesktopOperation operation, string value, bool allowPhysicalInput)
        {
            // Physical input stays available as the engine's last resort, because
            // some controls - a grid cell with no Invoke pattern, no accessible
            // action and no window handle - can only be reached that way. It is
            // dropped only when the desktop cannot receive it at all, where a
            // click would be silently swallowed and reported as a success.
            // Probed once and carried on the context: asking again further down would
            // let one action decide twice, and a screen unlocked in between would have
            // the layers disagree about what happened.
            bool desktopCanTakePhysicalInput = InteractiveDesktop.IsAvailable();

            DesktopActionContext context = new DesktopActionContext
            {
                Operation = operation,
                AutomationElement = element,
                Value = value,
                AllowPhysicalInput = allowPhysicalInput && desktopCanTakePhysicalInput,
                DesktopCanTakePhysicalInput = desktopCanTakePhysicalInput
            };

            if (element != null)
            {
                try
                {
                    context.NativeWindowHandle = new IntPtr(Convert.ToInt64(element.Current.NativeWindowHandle));
                }
                catch
                {
                    context.NativeWindowHandle = IntPtr.Zero;
                }

                try
                {
                    var bounds = element.Current.BoundingRectangle;
                    if (bounds.Width > 0 && bounds.Height > 0)
                    {
                        context.TargetScreenX = (int)(bounds.X + bounds.Width / 2);
                        context.TargetScreenY = (int)(bounds.Y + bounds.Height / 2);
                        context.HasTargetPoint = true;
                    }
                }
                catch
                {
                    context.HasTargetPoint = false;
                }

                context.PointOwnerWindowHandle = context.NativeWindowHandle != IntPtr.Zero
                    ? context.NativeWindowHandle
                    : FindAncestorWindowHandle(element);
            }

            return context;
        }

        /// <summary>
        /// Context for a click aimed at an explicit screen point rather than at the
        /// element as a whole, for the callers that named coordinates.
        /// </summary>
        /// <remarks>
        /// Physical input is withheld rather than offered as a last resort: this is
        /// built only where the mouse has already been ruled out, and leaving it on is
        /// what tells the layers to hand the click back to it.
        /// </remarks>
        public static DesktopActionContext ForPoint(UIAuto.AutomationElement element, DesktopOperation operation, int screenX, int screenY)
        {
            DesktopActionContext context = FromElement(element, operation, null, allowPhysicalInput: false);

            context.HasTargetPoint = true;
            context.TargetScreenX = screenX;
            context.TargetScreenY = screenY;
            return context;
        }

        /// <summary>
        /// Why a coordinate click got nowhere by window message, for the run report.
        /// </summary>
        /// <remarks>
        /// Names the point and the state of the desktop together, because neither on
        /// its own explains the failure. Reporting it at all is the change: the
        /// coordinate paths returned nothing, so a click that reached no window was
        /// still recorded as a step that passed.
        ///
        /// The desktop is described rather than characterised. This route also runs on
        /// a desktop that can take physical input, whenever the operator asked for the
        /// quiet one, so asserting the opposite produced a line reading that input
        /// could not be taken beside a session reporting it as available - which reads
        /// as a contradiction and sends whoever finds it after the wrong cause.
        /// </remarks>
        public static string DescribeUnreachablePoint(DesktopOperation operation, int screenX, int screenY, DesktopEngineResult engineResult)
        {
            return "Ginger could not " + (operation == DesktopOperation.DoubleClick ? "double click" : "click")
                + " the point " + screenX + "," + screenY + " by window message ("
                + InteractiveDesktop.DescribeSession() + "). " + engineResult?.ExecutionInfo;
        }

        /// <summary>
        /// Walks up to the first ancestor that has a window handle, which is the window
        /// whose pixels the target is drawn on.
        /// </summary>
        /// <remarks>
        /// A grid cell is drawn rather than created as a window, so it reports no handle
        /// of its own and a message based click would have nothing to aim at. The walk
        /// is bounded because a provider that answers with itself as its own parent
        /// would otherwise loop.
        /// </remarks>
        private static IntPtr FindAncestorWindowHandle(UIAuto.AutomationElement element)
        {
            try
            {
                UIAuto.AutomationElement current = element;
                for (int depth = 0; depth < 32; depth++)
                {
                    current = UIAuto.TreeWalker.RawViewWalker.GetParent(current);
                    if (current == null)
                    {
                        return IntPtr.Zero;
                    }

                    IntPtr handle = new IntPtr(Convert.ToInt64(current.Current.NativeWindowHandle));
                    if (handle != IntPtr.Zero)
                    {
                        return handle;
                    }
                }
            }
            catch
            {
                // A provider that cannot be walked simply leaves no point to aim at.
            }

            return IntPtr.Zero;
        }

        public static ActionResult ToActionResult(DesktopEngineResult engineResult)
        {
            ActionResult actionResult = new ActionResult();
            if (engineResult == null)
            {
                actionResult.errorMessage = "Ginger could not complete this operation on the element.";
                return actionResult;
            }

            if (engineResult.Success)
            {
                actionResult.executionInfo = engineResult.ExecutionInfo;
                actionResult.outputValue = engineResult.OutputValue;
            }
            else
            {
                actionResult.errorMessage = engineResult.ErrorMessage;
                actionResult.executionInfo = engineResult.ExecutionInfo;
            }

            return actionResult;
        }
    }
}
