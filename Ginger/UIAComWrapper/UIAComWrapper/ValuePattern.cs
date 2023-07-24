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
    public class RangeValuePatternExtended : BasePattern
    {
        private UIAutomationClient.IUIAutomationRangeValuePattern _pattern;
        public static readonly AutomationPatternExtended Pattern = RangeValuePatternIdentifiers.Pattern;
        public static readonly AutomationPropertyExtended IsReadOnlyProperty = RangeValuePatternIdentifiers.IsReadOnlyProperty;
        public static readonly AutomationPropertyExtended LargeChangeProperty = RangeValuePatternIdentifiers.LargeChangeProperty;
        public static readonly AutomationPropertyExtended MaximumProperty = RangeValuePatternIdentifiers.MaximumProperty;
        public static readonly AutomationPropertyExtended MinimumProperty = RangeValuePatternIdentifiers.MinimumProperty;
        public static readonly AutomationPropertyExtended SmallChangeProperty = RangeValuePatternIdentifiers.SmallChangeProperty;
        public static readonly AutomationPropertyExtended ValueProperty = RangeValuePatternIdentifiers.ValueProperty;
        
        private RangeValuePatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationRangeValuePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new RangeValuePatternExtended(el, (UIAutomationClient.IUIAutomationRangeValuePattern)pattern, cached);
        }

        public void SetValue(double value)
        {
            try
            {
                this._pattern.SetValue(value);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public RangeValuePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new RangeValuePatternInformation(this._el, true);
            }
        }

        public RangeValuePatternInformation Current
        {
            get
            {
                return new RangeValuePatternInformation(this._el, false);
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct RangeValuePatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal RangeValuePatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public double Value
            {
                get
                {
                    return (double)this._el.GetPropertyValue(RangeValuePatternExtended.ValueProperty, _isCached);
                }
            }
            public bool IsReadOnly
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(RangeValuePatternExtended.IsReadOnlyProperty, _isCached);
                }
            }
            public double Maximum
            {
                get
                {
                    return (double)this._el.GetPropertyValue(RangeValuePatternExtended.MaximumProperty, _isCached);
                }
            }
            public double Minimum
            {
                get
                {
                    return (double)this._el.GetPropertyValue(RangeValuePatternExtended.MinimumProperty, _isCached);
                }
            }
            public double LargeChange
            {
                get
                {
                    return (double)this._el.GetPropertyValue(RangeValuePatternExtended.LargeChangeProperty, _isCached);
                }
            }
            public double SmallChange
            {
                get
                {
                    return (double)this._el.GetPropertyValue(RangeValuePatternExtended.SmallChangeProperty, _isCached);
                }
            }
        }
    }

    public class ValuePatternExtended : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationValuePattern _pattern;
        public static readonly AutomationPatternExtended Pattern = ValuePatternIdentifiersExtended.Pattern;
        public static readonly AutomationPropertyExtended IsReadOnlyProperty = ValuePatternIdentifiersExtended.IsReadOnlyProperty;
        public static readonly AutomationPropertyExtended ValueProperty = ValuePatternIdentifiersExtended.ValueProperty;


        
        private ValuePatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationValuePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ValuePatternExtended(el, (UIAutomationClient.IUIAutomationValuePattern)pattern, cached);
        }

        public void SetValue(string value)
        {
            try
            {
                this._pattern.SetValue(value);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public ValuePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new ValuePatternInformation(this._el, true);
            }
        }

        public ValuePatternInformation Current
        {
            get
            {
                return new ValuePatternInformation(this._el, false);
            }
        }


        
        [StructLayout(LayoutKind.Sequential)]
        public struct ValuePatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal ValuePatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public string Value
            {
                get
                {
                    return (string)this._el.GetPropertyValue(ValuePatternExtended.ValueProperty, _isCached);
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(ValuePatternExtended.IsReadOnlyProperty, _isCached);
                }
            }
        }
    }
}
