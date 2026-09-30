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
    /// <summary>
    /// Sends a Send Keys value to an element as window messages, for the routes that
    /// would otherwise reach for the keyboard.
    /// </summary>
    /// <remarks>
    /// Every Send Keys path in Ginger ends at the same question - which window should
    /// receive this - so it is answered once here rather than three times.
    ///
    /// Nothing here decides whether the keyboard should have been used instead. That
    /// stays with the caller, because only it knows whether a keyboard fallback
    /// exists for the step it is running.
    /// </remarks>
    public static class WindowMessageKeystrokes
    {
        /// <summary>
        /// How long the target window gets to accept each message. A window too busy
        /// to take a keystroke is worth reporting rather than waiting behind.
        /// </summary>
        private const int TimeoutMs = 2000;

        /// <summary>
        /// Delivers <paramref name="value"/> to <paramref name="element"/> without
        /// touching the mouse or the keyboard.
        /// </summary>
        /// <param name="failure">
        /// Why it could not be delivered, phrased to read inside a longer sentence,
        /// when this returns false.
        /// </param>
        /// <param name="partiallyDelivered">
        /// Whether part of the value reached the element before the failure. A caller
        /// holding the keyboard in reserve must fail the step rather than use it
        /// there, because the element keeps what arrived and typing the value again
        /// appends a second copy of that prefix.
        /// </param>
        /// <remarks>
        /// A control with a window of its own is addressed directly, which is what
        /// makes this work with the screen locked: no focus is needed and no click
        /// has to land first. Anything else is resolved to whichever child of its
        /// window currently holds the keyboard, since a key message sent to a frame
        /// is accepted and then ignored.
        /// </remarks>
        public static bool TrySend(UIAuto.AutomationElement element, string value, out string failure, out bool partiallyDelivered)
        {
            IntPtr handle = TryGetNativeHandle(element);
            IntPtr target = Win32Native.IsChildWindow(handle) ? handle : Win32KeyMessages.ResolveFocusedChild(handle);

            return Win32KeyMessages.TrySendNotation(target, value, TimeoutMs, out failure, out partiallyDelivered);
        }

        private static IntPtr TryGetNativeHandle(UIAuto.AutomationElement element)
        {
            if (element == null)
            {
                return IntPtr.Zero;
            }

            try
            {
                return new IntPtr(Convert.ToInt64(element.Current.NativeWindowHandle));
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }
}
