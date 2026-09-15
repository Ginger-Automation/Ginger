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
using System;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    public static class DesktopActionMapper
    {
        public static DesktopActionContext FromElement(UIAuto.AutomationElement element, DesktopOperation operation, string value, bool allowPhysicalInput)
        {
            DesktopActionContext context = new DesktopActionContext
            {
                Operation = operation,
                AutomationElement = element,
                Value = value,
                AllowPhysicalInput = allowPhysicalInput
            };

            if (element != null)
            {
                try
                {
                    context.NativeWindowHandle = new IntPtr(Convert.ToInt64(element.Current.NativeWindowHandle));
                }
                catch
                {
                    context.NativeWindowHandle = IntPtr.Zero;
                }
            }

            return context;
        }

        public static ActionResult ToActionResult(DesktopEngineResult engineResult)
        {
            ActionResult actionResult = new ActionResult();
            if (engineResult == null)
            {
                actionResult.errorMessage = "Desktop automation engine returned no result";
                return actionResult;
            }

            if (engineResult.Success)
            {
                actionResult.executionInfo = engineResult.ExecutionInfo;
                actionResult.outputValue = engineResult.OutputValue;
            }
            else
            {
                actionResult.errorMessage = engineResult.ErrorMessage;
                actionResult.executionInfo = engineResult.ExecutionInfo;
            }

            return actionResult;
        }
    }
}
