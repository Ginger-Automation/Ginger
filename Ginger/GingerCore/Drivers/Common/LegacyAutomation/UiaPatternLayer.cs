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
    public sealed class UiaPatternLayer : IDesktopAutomationLayer
    {
        public string Name => "UIA-Pattern";

        public bool CanHandle(DesktopActionContext context)
        {
            return context.AutomationElement is UIAuto.AutomationElement;
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            UIAuto.AutomationElement element = (UIAuto.AutomationElement)context.AutomationElement;
            try
            {
                switch (context.Operation)
                {
                    case DesktopOperation.Click:
                        if (element.TryGetCurrentPattern(UIAuto.InvokePattern.Pattern, out object invokePattern) && invokePattern is UIAuto.InvokePattern invoke)
                        {
                            invoke.Invoke();
                            return LayerResult.Ok(Name, "Click via InvokePattern");
                        }
                        return LayerResult.Skip("InvokePattern not available");

                    case DesktopOperation.SetValue:
                        if (element.TryGetCurrentPattern(UIAuto.ValuePattern.Pattern, out object valuePattern) && valuePattern is UIAuto.ValuePattern setValuePattern)
                        {
                            setValuePattern.SetValue(context.Value ?? string.Empty);
                            return LayerResult.Ok(Name, "SetValue via ValuePattern");
                        }
                        return LayerResult.Skip("ValuePattern not available");

                    case DesktopOperation.GetValue:
                        object value = element.GetCurrentPropertyValue(UIAuto.ValuePatternIdentifiers.ValueProperty);
                        if (value != null)
                        {
                            return LayerResult.Ok(Name, "GetValue via ValuePattern", Convert.ToString(value));
                        }
                        return LayerResult.Skip("ValuePattern value property not available");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "UIA pattern layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by UIA patterns");
        }
    }
}
