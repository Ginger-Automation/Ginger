#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System.Diagnostics;
using System.Runtime.InteropServices;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class WindowPatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationWindowPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = WindowPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended CanMaximizeProperty = WindowPatternIdentifiers.CanMaximizeProperty;
        public static readonly AutomationPropertyExtended CanMinimizeProperty = WindowPatternIdentifiers.CanMinimizeProperty;
        public static readonly AutomationPropertyExtended IsModalProperty = WindowPatternIdentifiers.IsModalProperty;
        public static readonly AutomationPropertyExtended IsTopmostProperty = WindowPatternIdentifiers.IsTopmostProperty;
        public static readonly AutomationEventExtended WindowClosedEvent = WindowPatternIdentifiers.WindowClosedEvent;
        public static readonly AutomationPropertyExtended WindowInteractionStateProperty = WindowPatternIdentifiers.WindowInteractionStateProperty;
        public static readonly AutomationEventExtended WindowOpenedEvent = WindowPatternIdentifiers.WindowOpenedEvent;
        public static readonly AutomationPropertyExtended WindowVisualStateProperty = WindowPatternIdentifiers.WindowVisualStateProperty;
        
        private WindowPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationWindowPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new WindowPatternExtended(el, (UIAutomationClient.IUIAutomationWindowPattern)pattern, cached);
        }

        public void Close()
        {
            try
            {
                this._pattern.Close();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void SetWindowVisualState(WindowVisualStateExtended state)
        {
            try
            {
                this._pattern.SetWindowVisualState((UIAutomationClient.WindowVisualState)state);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public bool WaitForInputIdle(int milliseconds)
        {
            try
            {
                return (0 != this._pattern.WaitForInputIdle(milliseconds));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public WindowPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new WindowPatternInformation(this._el, true);
            }
        }

        public WindowPatternInformation Current
        {
            get
            {
                return new WindowPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal WindowPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public bool CanMaximize
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(WindowPatternExtended.CanMaximizeProperty, _isCached);
                }
            }

            public bool CanMinimize
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(WindowPatternExtended.CanMinimizeProperty, _isCached);
                }
            }

            public bool IsModal
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(WindowPatternExtended.IsModalProperty, _isCached);
                }
            }

            public WindowVisualStateExtended WindowVisualState
            {
                get
                {
                    return (WindowVisualStateExtended)this._el.GetPropertyValue(WindowPatternExtended.WindowVisualStateProperty, _isCached);
                }
            }

            public WindowInteractionStateExtended WindowInteractionState
            {
                get
                {
                    return (WindowInteractionStateExtended)this._el.GetPropertyValue(WindowPatternExtended.WindowInteractionStateProperty, _isCached);
                }
            }

            public bool IsTopmost
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(WindowPatternExtended.IsTopmostProperty, _isCached);
                }
            }
        }
    }
}