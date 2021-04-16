#region License
/*
Copyright © 2014-2021 European Support Limited

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

// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.

using System.Diagnostics;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class InvokePattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationInvokePattern _pattern;
        public static readonly AutomationEvent InvokedEvent = InvokePatternIdentifiers.InvokedEvent;
        public static readonly AutomationPattern Pattern = InvokePatternIdentifiers.Pattern;
        
        private InvokePattern(AutomationElement el, UIAutomationClient.IUIAutomationInvokePattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public void Invoke()
        {
            try
            {
                this._pattern.Invoke();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; } 
            }
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new InvokePattern(el, (UIAutomationClient.IUIAutomationInvokePattern)pattern, cached);
        }
    }
}
