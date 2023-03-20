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
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class ItemContainerPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationItemContainerPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = ItemContainerPatternIdentifiers.Pattern;

        private ItemContainerPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationItemContainerPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public AutomationElement_Extend FindItemByProperty(AutomationElement_Extend startAfter, AutomationPropertyExtended property, object value)
        {
            try
            {
                return AutomationElement_Extend.Wrap(
                    this._pattern.FindItemByProperty(
                        (startAfter == null) ? null : startAfter.NativeElement,
                        (property == null) ? 0 : property.Id,
                        Utility.UnwrapObject(value)));
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new ItemContainerPattern(el, (UIAutomationClient.IUIAutomationItemContainerPattern)pattern, cached);
        }
    }

    public class VirtualizedItemPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationVirtualizedItemPattern _pattern;
        public static readonly AutomationPatternExtended Pattern = VirtualizedItemPatternIdentifiers.Pattern;

        private VirtualizedItemPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationVirtualizedItemPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public void Realize()
        {
            try
            {
                this._pattern.Realize();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new VirtualizedItemPattern(el, (UIAutomationClient.IUIAutomationVirtualizedItemPattern)pattern, cached);
        }
    }
}
