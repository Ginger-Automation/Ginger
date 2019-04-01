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
    public class GridPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationGridPattern _pattern;
        public static readonly AutomationPattern Pattern = GridPatternIdentifiers.Pattern;
        public static readonly AutomationProperty ColumnCountProperty = GridPatternIdentifiers.ColumnCountProperty;
        public static readonly AutomationProperty RowCountProperty = GridPatternIdentifiers.RowCountProperty;
        
        protected GridPattern(AutomationElement el, UIAutomationClient.IUIAutomationGridPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new GridPattern(el, (UIAutomationClient.IUIAutomationGridPattern)pattern, cached);
        }

        public AutomationElement GetItem(int row, int column)
        {
            try
            {
                // Looks like we have to cache explicitly here, since GetItem doesn't
                // take a cache request.
                return AutomationElement.Wrap(this._pattern.GetItem(row, column)).GetUpdatedCache(CacheRequest.Current);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }
        
        public GridPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new GridPatternInformation(this._el, true);
            }
        }

        public GridPatternInformation Current
        {
            get
            {
                return new GridPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct GridPatternInformation
        {
            private AutomationElement _el;
            private bool _isCached;
            internal GridPatternInformation(AutomationElement element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public int RowCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridPattern.RowCountProperty, _isCached);
                }
            }
            public int ColumnCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridPattern.ColumnCountProperty, _isCached);
                }
            }
        }
    }

    public class GridItemPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationGridItemPattern _pattern;
        public static readonly AutomationPattern Pattern = GridItemPatternIdentifiers.Pattern;
        public static readonly AutomationProperty ColumnProperty = GridItemPatternIdentifiers.ColumnProperty;
        public static readonly AutomationProperty ColumnSpanProperty = GridItemPatternIdentifiers.ColumnSpanProperty;
        public static readonly AutomationProperty ContainingGridProperty = GridItemPatternIdentifiers.ContainingGridProperty;
        public static readonly AutomationProperty RowProperty = GridItemPatternIdentifiers.RowProperty;
        public static readonly AutomationProperty RowSpanProperty = GridItemPatternIdentifiers.RowSpanProperty;
        
        protected GridItemPattern(AutomationElement el, UIAutomationClient.IUIAutomationGridItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new GridItemPattern(el, (UIAutomationClient.IUIAutomationGridItemPattern)pattern, cached);
        }

        
        public GridItemPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new GridItemPatternInformation(this._el, true);
            }
        }

        public GridItemPatternInformation Current
        {
            get
            {
                return new GridItemPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct GridItemPatternInformation
        {
            private AutomationElement _el;
            private bool _isCached;
            internal GridItemPatternInformation(AutomationElement element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public int Row
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridItemPattern.RowProperty, _isCached);
                }
            }
            public int Column
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridItemPattern.ColumnProperty, _isCached);
                }
            }
            public int RowSpan
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridItemPattern.RowSpanProperty, _isCached);
                }
            }
            public int ColumnSpan
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridItemPattern.ColumnSpanProperty, _isCached);
                }
            }
            public AutomationElement ContainingGrid
            {
                get
                {
                    return (AutomationElement)this._el.GetPropertyValue(GridItemPattern.ContainingGridProperty, _isCached);
                }
            }
        }
    }
}
