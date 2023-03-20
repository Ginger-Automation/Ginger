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
    public class SelectionPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationSelectionPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = SelectionPatternIdentifiersExtended.Pattern;
        public static readonly AutomationPropertyExtended CanSelectMultipleProperty = SelectionPatternIdentifiersExtended.CanSelectMultipleProperty;
        public static readonly AutomationEventExtended InvalidatedEvent = SelectionPatternIdentifiersExtended.InvalidatedEvent;
        public static readonly AutomationPropertyExtended IsSelectionRequiredProperty = SelectionPatternIdentifiersExtended.IsSelectionRequiredProperty;
        public static readonly AutomationPropertyExtended SelectionProperty = SelectionPatternIdentifiersExtended.SelectionProperty;
        
        private SelectionPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationSelectionPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new SelectionPattern(el, (UIAutomationClient.IUIAutomationSelectionPattern)pattern, cached);
        }

        
        public SelectionPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new SelectionPatternInformation(this._el, true);
            }
        }

        public SelectionPatternInformation Current
        {
            get
            {
                return new SelectionPatternInformation(this._el, false);
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct SelectionPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal SelectionPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public AutomationElement_Extend[] GetSelection()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(SelectionPattern.SelectionProperty, _isCached);
            }

            public bool CanSelectMultiple
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(SelectionPattern.CanSelectMultipleProperty, _isCached);
                }
            }
            public bool IsSelectionRequired
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(SelectionPattern.IsSelectionRequiredProperty, _isCached);
                }
            }
        }
    }

    public class SelectionItemPatternExtended : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationSelectionItemPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = SelectionItemPatternIdentifiersExtended.Pattern;
        public static readonly AutomationEventExtended ElementAddedToSelectionEvent = SelectionItemPatternIdentifiersExtended.ElementAddedToSelectionEvent;
        public static readonly AutomationEventExtended ElementRemovedFromSelectionEvent = SelectionItemPatternIdentifiersExtended.ElementRemovedFromSelectionEvent;
        public static readonly AutomationEventExtended ElementSelectedEvent = SelectionItemPatternIdentifiersExtended.ElementSelectedEvent;
        public static readonly AutomationPropertyExtended IsSelectedProperty = SelectionItemPatternIdentifiersExtended.IsSelectedProperty;
        public static readonly AutomationPropertyExtended SelectionContainerProperty = SelectionItemPatternIdentifiersExtended.SelectionContainerProperty;


        
        private SelectionItemPatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationSelectionItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new SelectionItemPatternExtended(el, (UIAutomationClient.IUIAutomationSelectionItemPattern)pattern, cached);
        }

        public void AddToSelection()
        {
            try
            {
                this._pattern.AddToSelection();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void RemoveFromSelection()
        {
            try
            {
                this._pattern.RemoveFromSelection();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Select()
        {
            try
            {
                this._pattern.Select();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public SelectionItemPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new SelectionItemPatternInformation(this._el, true);
            }
        }

        public SelectionItemPatternInformation Current
        {
            get
            {
                return new SelectionItemPatternInformation(this._el, false);
            }
        }

        
        [StructLayout(LayoutKind.Sequential)]
        public struct SelectionItemPatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal SelectionItemPatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public bool IsSelected
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(SelectionItemPatternExtended.IsSelectedProperty, _isCached);
                }
            }

            public AutomationElement_Extend SelectionContainer
            {
                get
                {
                    return (AutomationElement_Extend)this._el.GetPropertyValue(SelectionItemPatternExtended.SelectionContainerProperty, _isCached);
                }
            }
        }
    }
}