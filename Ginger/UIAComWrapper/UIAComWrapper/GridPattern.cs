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
    public class GridPatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationGridPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = GridPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended ColumnCountProperty = GridPatternIdentifiers.ColumnCountProperty;
        public static readonly AutomationPropertyExtended RowCountProperty = GridPatternIdentifiers.RowCountProperty;
        
        protected GridPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationGridPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new GridPatternExtended(el, (UIAutomationClient.IUIAutomationGridPattern)pattern, cached);
        }

        public AutomationElement_Extend GetItem(int row, int column)
        {
            try
            {
                // Looks like we have to cache explicitly here, since GetItem doesn't
                // take a cache request.
                return AutomationElement_Extend.Wrap(this._pattern.GetItem(row, column)).GetUpdatedCache(CacheRequest.Current);
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
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal GridPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public int RowCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridPatternExtended.RowCountProperty, _isCached);
                }
            }
            public int ColumnCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(GridPatternExtended.ColumnCountProperty, _isCached);
                }
            }
        }
    }

    public class GridItemPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationGridItemPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = GridItemPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended ColumnProperty = GridItemPatternIdentifiers.ColumnProperty;
        public static readonly AutomationPropertyExtended ColumnSpanProperty = GridItemPatternIdentifiers.ColumnSpanProperty;
        public static readonly AutomationPropertyExtended ContainingGridProperty = GridItemPatternIdentifiers.ContainingGridProperty;
        public static readonly AutomationPropertyExtended RowProperty = GridItemPatternIdentifiers.RowProperty;
        public static readonly AutomationPropertyExtended RowSpanProperty = GridItemPatternIdentifiers.RowSpanProperty;
        
        protected GridItemPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationGridItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
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
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal GridItemPatternInformation(AutomationElement_Extend element, bool isCached)
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
            public AutomationElement_Extend ContainingGrid
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(GridItemPattern.ContainingGridProperty, _isCached);
                }
            }
        }
    }
}
