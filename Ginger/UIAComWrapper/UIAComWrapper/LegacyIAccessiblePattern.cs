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
    public class LegacyIAccessiblePatternExtended : BasePattern
    {
        public static readonly AutomationPropertyExtended ChildIdProperty = LegacyIAccessiblePatternIdentifiersExtended.ChildIdProperty;
        public static readonly AutomationPropertyExtended NameProperty = LegacyIAccessiblePatternIdentifiersExtended.NameProperty;
        public static readonly AutomationPropertyExtended ValueProperty = LegacyIAccessiblePatternIdentifiersExtended.ValueProperty;
        public static readonly AutomationPropertyExtended DescriptionProperty = LegacyIAccessiblePatternIdentifiersExtended.DescriptionProperty;
        public static readonly AutomationPropertyExtended RoleProperty = LegacyIAccessiblePatternIdentifiersExtended.RoleProperty;
        public static readonly AutomationPropertyExtended StateProperty = LegacyIAccessiblePatternIdentifiersExtended.StateProperty;
        public static readonly AutomationPropertyExtended HelpProperty = LegacyIAccessiblePatternIdentifiersExtended.HelpProperty;
        public static readonly AutomationPropertyExtended KeyboardShortcutProperty = LegacyIAccessiblePatternIdentifiersExtended.KeyboardShortcutProperty;
        public static readonly AutomationPropertyExtended SelectionProperty = LegacyIAccessiblePatternIdentifiersExtended.SelectionProperty;
        public static readonly AutomationPropertyExtended DefaultActionProperty = LegacyIAccessiblePatternIdentifiersExtended.DefaultActionProperty;
        public static readonly AutomationPatternExtended Pattern = LegacyIAccessiblePatternIdentifiersExtended.Pattern;

        private UIAutomationClient.IUIAutomationLegacyIAccessiblePattern _pattern;

        private LegacyIAccessiblePatternExtended(AutomationElement_Extend el, UIAutomationClient.IUIAutomationLegacyIAccessiblePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public void Select(int flagsSelect)
        {
            try
            {
                this._pattern.Select(flagsSelect);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void DoDefaultAction()
        {
            try
            {
                this._pattern.DoDefaultAction();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
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

        public Accessibility.IAccessible GetIAccessible()
        {
            try
            {
                return (Accessibility.IAccessible)this._pattern.GetIAccessible();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new LegacyIAccessiblePatternExtended(el, (UIAutomationClient.IUIAutomationLegacyIAccessiblePattern)pattern, cached);
        }


        public LegacyIAccessiblePatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new LegacyIAccessiblePatternInformation(this._el, true);
            }
        }

        public LegacyIAccessiblePatternInformation Current
        {
            get
            {
                return new LegacyIAccessiblePatternInformation(this._el, false);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LegacyIAccessiblePatternInformation
        {
            private AutomationElement_Extend _el;
            private bool _isCached;
            internal LegacyIAccessiblePatternInformation(AutomationElement_Extend element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }
            
            public AutomationElement_Extend[] GetSelection()
            {
                return (AutomationElement_Extend[])this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.SelectionProperty, _isCached);
            }

            public int ChildId
            {
                get
                {
                    return (int)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.ChildIdProperty, _isCached);
                }
            }

            public string DefaultAction
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.DefaultActionProperty, _isCached);
                }
            }

            public string Description
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.DescriptionProperty, _isCached);
                }
            }

            public string Help
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.HelpProperty, _isCached);
                }
            }

            public string KeyboardShortcut
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.KeyboardShortcutProperty, _isCached);
                }
            }

            public string Name
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.NameProperty, _isCached);
                }
            }

            public uint Role
            {
                get
                {
                    return Convert.ToUInt32(this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.RoleProperty, _isCached));
                }
            }

            public uint State
            {
                get
                {
                    return Convert.ToUInt32(this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.StateProperty, _isCached));
                }
            }

            public string Value
            {
                get
                {
                    return (string)this._el.GetPropertyValue(LegacyIAccessiblePatternExtended.ValueProperty, _isCached);
                }
            }
        }
    }
}
