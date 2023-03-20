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
    public class DockPattern : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationDockPattern _pattern;
        public static readonly AutomationPropertyExtended DockPositionProperty = DockPatternIdentifiers.DockPositionProperty;
        public static readonly AutomationPatternExtended Pattern = DockPatternIdentifiers.Pattern;

        
        private DockPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationDockPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public void SetDockPosition(DockPosition dockPosition)
        {
            try
            {
                this._pattern.SetDockPosition((UIAutomationClient.DockPosition)dockPosition);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new DockPattern(el, (UIAutomationClient.IUIAutomationDockPattern)pattern, cached);
        }

        
        public DockPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new DockPatternInformation(this._el, true);
            }
        }

        public DockPatternInformation Current
        {
            get
            {
                return new DockPatternInformation(this._el, false);
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct DockPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal DockPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public DockPosition DockPosition
            {
                get
                {
                    return (DockPosition)this._el.GetPropertyValue(DockPattern.DockPositionProperty, _isCached);
                }
            }
        }
    }
}
