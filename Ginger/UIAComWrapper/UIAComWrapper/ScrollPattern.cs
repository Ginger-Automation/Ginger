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
    public class ScrollPatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationScrollPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = ScrollPatternIdentifiersExtended.Pattern;
        public static readonly AutomationPropertyExtended HorizontallyScrollableProperty = ScrollPatternIdentifiersExtended.HorizontallyScrollableProperty;
        public static readonly AutomationPropertyExtended HorizontalScrollPercentProperty = ScrollPatternIdentifiersExtended.HorizontalScrollPercentProperty;
        public static readonly AutomationPropertyExtended HorizontalViewSizeProperty = ScrollPatternIdentifiersExtended.HorizontalViewSizeProperty;
        public const double NoScroll = -1.0;
        public static readonly AutomationPropertyExtended VerticallyScrollableProperty = ScrollPatternIdentifiersExtended.VerticallyScrollableProperty;
        public static readonly AutomationPropertyExtended VerticalScrollPercentProperty = ScrollPatternIdentifiersExtended.VerticalScrollPercentProperty;
        public static readonly AutomationPropertyExtended VerticalViewSizeProperty = ScrollPatternIdentifiersExtended.VerticalViewSizeProperty;
        
        private ScrollPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationScrollPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ScrollPatternExtended(el, (UIAutomationClient.IUIAutomationScrollPattern)pattern, cached);
        }

        public void Scroll(ScrollAmount horizontalAmount, ScrollAmount verticalAmount)
        {
            try
            {
                this._pattern.Scroll((UIAutomationClient.ScrollAmount)horizontalAmount, (UIAutomationClient.ScrollAmount)verticalAmount);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void ScrollHorizontal(ScrollAmount amount)
        {
            try
            {
                this._pattern.Scroll((UIAutomationClient.ScrollAmount)amount, UIAutomationClient.ScrollAmount.ScrollAmount_NoAmount);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void ScrollVertical(ScrollAmount amount)
        {
            try
            {
                this._pattern.Scroll(UIAutomationClient.ScrollAmount.ScrollAmount_NoAmount, (UIAutomationClient.ScrollAmount)amount);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void SetScrollPercent(double horizontalPercent, double verticalPercent)
        {
            try
            {
                this._pattern.SetScrollPercent(horizontalPercent, verticalPercent);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public ScrollPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new ScrollPatternInformation(this._el, true);
            }
        }

        public ScrollPatternInformation Current
        {
            get
            {
                return new ScrollPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct ScrollPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal ScrollPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public double HorizontalScrollPercent
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPatternExtended.HorizontalScrollPercentProperty, _isCached);
                }
            }
            public double VerticalScrollPercent
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPatternExtended.VerticalScrollPercentProperty, _isCached);
                }
            }
            public double HorizontalViewSize
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPatternExtended.HorizontalViewSizeProperty, _isCached);
                }
            }
            public double VerticalViewSize
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPatternExtended.VerticalViewSizeProperty, _isCached);
                }
            }
            public bool HorizontallyScrollable
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(ScrollPatternExtended.HorizontallyScrollableProperty, _isCached);
                }
            }
            public bool VerticallyScrollable
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(ScrollPatternExtended.VerticallyScrollableProperty, _isCached);
                }
            }
        }
    }

    public class ScrollItemPatternExtended : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationScrollItemPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = ScrollItemPatternIdentifiers.Pattern;

        
        private ScrollItemPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationScrollItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ScrollItemPatternExtended(el, (UIAutomationClient.IUIAutomationScrollItemPattern)pattern, cached);
        }

        public void ScrollIntoView()
        {
            try
            {
                this._pattern.ScrollIntoView();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
    }
}