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
    public class TablePattern : GridPatternExtended
    {
        private UIAutomationClient.IUIAutomationTablePattern _pattern;
        public new static readonly AutomationPatternExtended Pattern = TablePatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended ColumnHeadersProperty = TablePatternIdentifiers.ColumnHeadersProperty;
        public static readonly AutomationPropertyExtended RowHeadersProperty = TablePatternIdentifiers.RowHeadersProperty;
        public static readonly AutomationPropertyExtended RowOrColumnMajorProperty = TablePatternIdentifiers.RowOrColumnMajorProperty;
        
        private TablePattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationTablePattern tablePattern, UIAutomationClient.IUIAutomationGridPattern gridPattern, bool cached)
            : base(el, gridPattern, cached)
        {
            Debug.Assert(tablePattern != null);
            this._pattern = tablePattern;
        }

        internal new static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            TablePattern result = null;
            if (pattern != null)
            {
                UIAutomationClient.IUIAutomationGridPattern gridPattern =
                    (UIAutomationClient.IUIAutomationGridPattern)el.GetRawPattern(GridPatternExtended.Pattern, cached);
                if (gridPattern != null)
                {
                    result = new TablePattern(el, (UIAutomationClient.IUIAutomationTablePattern)pattern,
                        gridPattern, cached);
                }
            }
            return result;
        }
        
        public new TablePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new TablePatternInformation(this._el, true);
            }
        }

        public new TablePatternInformation Current
        {
            get
            {
                return new TablePatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct TablePatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal TablePatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public AutomationElement_Extend[] GetRowHeaders()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(TablePattern.RowHeadersProperty, _isCached);
            }

            public AutomationElement_Extend[] GetColumnHeaders()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(TablePattern.ColumnHeadersProperty, _isCached);
            }

            public int RowCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TablePattern.RowCountProperty, _isCached);
                }
            }
            public int ColumnCount
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TablePattern.ColumnCountProperty, _isCached);
                }
            }
            public RowOrColumnMajor RowOrColumnMajor
            {
                get
                {
                    return (RowOrColumnMajor)this._el.GetPropertyValue(TablePattern.RowOrColumnMajorProperty, _isCached);
                }
            }
        }
    }

    public class TableItemPattern : GridItemPattern
    {
        private UIAutomationClient.IUIAutomationTableItemPattern _pattern;
        public new static readonly AutomationPatternExtended Pattern = TableItemPatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended ColumnHeaderItemsProperty = TableItemPatternIdentifiers.ColumnHeaderItemsProperty;
        public static readonly AutomationPropertyExtended RowHeaderItemsProperty = TableItemPatternIdentifiers.RowHeaderItemsProperty;
        
        private TableItemPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationTableItemPattern tablePattern, UIAutomationClient.IUIAutomationGridItemPattern gridPattern, bool cached)
            : base(el, gridPattern, cached)
        {
            Debug.Assert(tablePattern != null);
            this._pattern = tablePattern;
        }

        internal new static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            TableItemPattern result = null;
            if (pattern != null)
            {
                UIAutomationClient.IUIAutomationGridItemPattern gridPattern =
                    (UIAutomationClient.IUIAutomationGridItemPattern)el.GetRawPattern(GridItemPattern.Pattern, cached);
                if (gridPattern != null)
                {
                    result = new TableItemPattern(el, (UIAutomationClient.IUIAutomationTableItemPattern)pattern,
                        gridPattern, cached);
                }
            }
            return result;
        }
        
        public new TableItemPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new TableItemPatternInformation(this._el, true);
            }
        }

        public new TableItemPatternInformation Current
        {
            get
            {
                return new TableItemPatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct TableItemPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal TableItemPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }
            
            public int Row
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TableItemPattern.RowProperty, _isCached);
                }
            }
            public int Column
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TableItemPattern.ColumnProperty, _isCached);
                }
            }
            public int RowSpan
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TableItemPattern.RowSpanProperty, _isCached);
                }
            }
            public int ColumnSpan
            {
                get
                {
                    return (int)this._el.GetPropertyValue(TableItemPattern.ColumnSpanProperty, _isCached);
                }
            }
            public AutomationElement_Extend ContainingGrid
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(TableItemPattern.ContainingGridProperty, _isCached);
                }
            }

            public AutomationElement_Extend[] GetRowHeaderItems()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(TableItemPattern.RowHeaderItemsProperty, _isCached);
            }

            public AutomationElement_Extend[] GetColumnHeaderItems()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(TableItemPattern.ColumnHeaderItemsProperty, _isCached);
            }
        }
    }
}