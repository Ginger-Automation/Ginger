#region License
/*
Copyright © 2014-2019 European Support Limited

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
    public class ScrollPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationScrollPattern _pattern;
        public static readonly AutomationPattern Pattern = ScrollPatternIdentifiers.Pattern;
        public static readonly AutomationProperty HorizontallyScrollableProperty = ScrollPatternIdentifiers.HorizontallyScrollableProperty;
        public static readonly AutomationProperty HorizontalScrollPercentProperty = ScrollPatternIdentifiers.HorizontalScrollPercentProperty;
        public static readonly AutomationProperty HorizontalViewSizeProperty = ScrollPatternIdentifiers.HorizontalViewSizeProperty;
        public const double NoScroll = -1.0;
        public static readonly AutomationProperty VerticallyScrollableProperty = ScrollPatternIdentifiers.VerticallyScrollableProperty;
        public static readonly AutomationProperty VerticalScrollPercentProperty = ScrollPatternIdentifiers.VerticalScrollPercentProperty;
        public static readonly AutomationProperty VerticalViewSizeProperty = ScrollPatternIdentifiers.VerticalViewSizeProperty;
        
        private ScrollPattern(AutomationElement el, UIAutomationClient.IUIAutomationScrollPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ScrollPattern(el, (UIAutomationClient.IUIAutomationScrollPattern)pattern, cached);
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
            private AutomationElement _el;
            private bool _isCached;
            internal ScrollPatternInformation(AutomationElement element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public double HorizontalScrollPercent
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPattern.HorizontalScrollPercentProperty, _isCached);
                }
            }
            public double VerticalScrollPercent
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPattern.VerticalScrollPercentProperty, _isCached);
                }
            }
            public double HorizontalViewSize
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPattern.HorizontalViewSizeProperty, _isCached);
                }
            }
            public double VerticalViewSize
            {
                get
                {
                    return (double)this._el.GetPropertyValue(ScrollPattern.VerticalViewSizeProperty, _isCached);
                }
            }
            public bool HorizontallyScrollable
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(ScrollPattern.HorizontallyScrollableProperty, _isCached);
                }
            }
            public bool VerticallyScrollable
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(ScrollPattern.VerticallyScrollableProperty, _isCached);
                }
            }
        }
    }

    public class ScrollItemPattern : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationScrollItemPattern _pattern;
        public static readonly AutomationPattern Pattern = ScrollItemPatternIdentifiers.Pattern;

        
        private ScrollItemPattern(AutomationElement el, UIAutomationClient.IUIAutomationScrollItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ScrollItemPattern(el, (UIAutomationClient.IUIAutomationScrollItemPattern)pattern, cached);
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