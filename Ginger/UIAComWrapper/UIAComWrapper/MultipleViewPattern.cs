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
    public class MultipleViewPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationMultipleViewPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = MultipleViewPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended CurrentViewProperty = MultipleViewPatternIdentifiers.CurrentViewProperty;
        public static readonly AutomationPropertyExtended SupportedViewsProperty = MultipleViewPatternIdentifiers.SupportedViewsProperty;
        
        private MultipleViewPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationMultipleViewPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new MultipleViewPattern(el, (UIAutomationClient.IUIAutomationMultipleViewPattern)pattern, cached);
        }

        public string GetViewName(int viewId)
        {
            try
            {
                return this._pattern.GetViewName(viewId);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void SetCurrentView(int viewId)
        {
            try
            {
                this._pattern.SetCurrentView(viewId);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public MultipleViewPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new MultipleViewPatternInformation(this._el, true);
            }
        }

        public MultipleViewPatternInformation Current
        {
            get
            {
                return new MultipleViewPatternInformation(this._el, false);
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct MultipleViewPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal MultipleViewPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public int CurrentView
            {
                get
                {
                    return (int)this._el.GetPropertyValue(MultipleViewPattern.CurrentViewProperty, _isCached);
                }
            }
            public int[] GetSupportedViews()
            {
                    return (int[])this._el.GetPropertyValue(MultipleViewPattern.SupportedViewsProperty, _isCached);
            }
        }
    }
}
