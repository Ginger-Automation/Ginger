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
    public class SynchronizedInputPattern : BasePattern
    {
        private UIAutomationClient.IUIAutomationSynchronizedInputPattern _pattern;
        public static readonly AutomationEventExtended InputReachedTargetEvent = SynchronizedInputPatternIdentifiers.InputReachedTargetEvent;
        public static readonly AutomationEventExtended InputReachedOtherElementEvent = SynchronizedInputPatternIdentifiers.InputReachedOtherElementEvent;
        public static readonly AutomationEventExtended InputDiscardedEvent = SynchronizedInputPatternIdentifiers.InputDiscardedEvent;
        public static readonly AutomationPatternExtended Pattern = SynchronizedInputPatternIdentifiers.Pattern;

        private SynchronizedInputPattern(AutomationElement_Extend el, UIAutomationClient.IUIAutomationSynchronizedInputPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        public void Cancel()
        {
            try
            {
                this._pattern.Cancel();
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void StartListening(SynchronizedInputType type)
        {
            try
            {
                this._pattern.StartListening((UIAutomationClient.SynchronizedInputType)type);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        internal static object Wrap(AutomationElement_Extend el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new SynchronizedInputPattern(el, (UIAutomationClient.IUIAutomationSynchronizedInputPattern)pattern, cached);
        }
    }
}
