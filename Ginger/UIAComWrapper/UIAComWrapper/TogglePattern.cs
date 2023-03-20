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
    public class TogglePatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationTogglePattern _pattern;
        public static readonly AutomationPatternExtended Pattern = TogglePatternIdentifiersExtended.Pattern;
        public static readonly AutomationPropertyExtended ToggleStateProperty = TogglePatternIdentifiersExtended.ToggleStateProperty;
        
        private TogglePatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationTogglePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new TogglePatternExtended(el, (UIAutomationClient.IUIAutomationTogglePattern)pattern, cached);
        }

        public void Toggle()
        {
            try
            {
                this._pattern.Toggle();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public TogglePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new TogglePatternInformation(this._el, true);
            }
        }

        public TogglePatternInformation Current
        {
            get
            {
                return new TogglePatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct TogglePatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal TogglePatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public ToggleStateExtended ToggleState
            {
                get
                {
                    return (ToggleStateExtended)this._el.GetPropertyValue(TogglePatternExtended.ToggleStateProperty, _isCached);
                }
            }
        }
    }
}