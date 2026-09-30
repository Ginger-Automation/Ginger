#region License
/*
Copyright © 2014-2026 European Support Limited

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

extern alias UIAComWrapperNetstandard;
using Amdocs.Ginger.Common;
using System;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    public sealed class MsaaLayer : IDesktopAutomationLayer
    {
        public string Name => "MSAA";

        public bool CanHandle(DesktopActionContext context)
        {
            return context.AutomationElement is UIAuto.AutomationElement;
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            UIAuto.AutomationElement element = (UIAuto.AutomationElement)context.AutomationElement;
            try
            {
                if (!element.TryGetCurrentPattern(UIAuto.LegacyIAccessiblePattern.Pattern, out object pattern) || pattern is not UIAuto.LegacyIAccessiblePattern legacy)
                {
                    return LayerResult.Skip("LegacyIAccessible not available");
                }

                switch (context.Operation)
                {
                    case DesktopOperation.Click:
                        object defaultAction = element.GetCurrentPropertyValue(UIAuto.LegacyIAccessiblePatternIdentifiers.DefaultActionProperty);
                        if (defaultAction == null || string.IsNullOrEmpty(Convert.ToString(defaultAction)))
                        {
                            return LayerResult.Skip("LegacyIAccessible has no default action");
                        }
                        legacy.DoDefaultAction();
                        return LayerResult.Ok(Name, "Click via LegacyIAccessible.DoDefaultAction");

                    case DesktopOperation.SetValue:
                        string expected = context.Value ?? string.Empty;
                        legacy.SetValue(expected);
                        object actual = element.GetCurrentPropertyValue(UIAuto.LegacyIAccessiblePatternIdentifiers.ValueProperty);
                        if (!string.Equals(Convert.ToString(actual) ?? string.Empty, expected, StringComparison.Ordinal))
                        {
                            return LayerResult.Skip("LegacyIAccessible SetValue did not read back");
                        }
                        return LayerResult.Ok(Name, "SetValue via verified LegacyIAccessible");

                    case DesktopOperation.GetValue:
                        object value = element.GetCurrentPropertyValue(UIAuto.LegacyIAccessiblePatternIdentifiers.ValueProperty);
                        if (value != null)
                        {
                            return LayerResult.Ok(Name, "GetValue via LegacyIAccessible", Convert.ToString(value));
                        }
                        return LayerResult.Skip("LegacyIAccessible value property not available");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "MSAA layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by LegacyIAccessible");
        }
    }
}
