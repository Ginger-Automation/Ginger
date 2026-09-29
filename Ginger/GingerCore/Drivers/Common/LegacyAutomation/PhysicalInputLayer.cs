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
using GingerCore.Drivers;
using System;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Moves the real cursor, so it only runs for call sites that already did this
    /// before the engine existed and only after every non-intrusive layer declined.
    /// </summary>
    public sealed class PhysicalInputLayer : IDesktopAutomationLayer
    {
        public string Name => "PhysicalInput";

        private readonly WinAPIAutomation mWinApi = new WinAPIAutomation();

        public bool CanHandle(DesktopActionContext context)
        {
            return context.AllowPhysicalInput
                && context.Operation == DesktopOperation.Click
                && context.AutomationElement is UIAuto.AutomationElement;
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            // The desktop is asked again here, and only here, because this is the one
            // route that cannot tell whether it worked. The answer on the context was
            // taken when the action started; a screen locked in the time the quieter
            // layers took to decline would leave the call below moving a cursor
            // nobody can see, throwing nothing and reporting a click the application
            // never received. Skipping instead lets the engine say the action failed,
            // which is the one thing a locked screen must never turn into a pass.
            if (!InteractiveDesktop.IsAvailable())
            {
                return LayerResult.Skip("The screen was locked after the action started, so the mouse reaches nothing");
            }

            try
            {
                mWinApi.SendClick((UIAuto.AutomationElement)context.AutomationElement);
                return LayerResult.Ok(Name, "Click via Mouse event");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Physical input layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }
        }
    }
}
