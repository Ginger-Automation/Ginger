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

using Amdocs.Ginger.Common;
using System;

namespace GingerCore.Drivers.Common.LegacyAutomation
{
    /// <summary>
    /// Talks to the control through window messages. This reaches legacy controls
    /// that expose no usable UIA or MSAA pattern, and unlike physical input it needs
    /// neither focus nor the foreground window.
    /// </summary>
    public sealed class Win32Layer : IDesktopAutomationLayer
    {
        public string Name => "Win32";

        public bool CanHandle(DesktopActionContext context)
        {
            return context.NativeWindowHandle != IntPtr.Zero;
        }

        public LayerResult TryExecute(DesktopActionContext context)
        {
            try
            {
                IntPtr hwnd = context.NativeWindowHandle;
                switch (context.Operation)
                {
                    case DesktopOperation.Click:
                        if (Win32Native.ClickButton(hwnd, context.TimeoutMs))
                        {
                            return LayerResult.Ok(Name, "Click via BM_CLICK");
                        }
                        return LayerResult.Skip("HWND is not an enabled button control");

                    case DesktopOperation.SetValue:
                        if (Win32Native.SetControlText(hwnd, context.Value ?? string.Empty, context.TimeoutMs))
                        {
                            return LayerResult.Ok(Name, "SetValue via verified WM_SETTEXT/EM_REPLACESEL");
                        }
                        return LayerResult.Skip("HWND is not an edit control or the text did not read back");

                    case DesktopOperation.GetValue:
                        string text = Win32Native.GetControlText(hwnd, context.TimeoutMs);
                        if (text != null)
                        {
                            return LayerResult.Ok(Name, "GetValue via WM_GETTEXT", text);
                        }
                        return LayerResult.Skip("WM_GETTEXT is not available on this HWND");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Win32 layer failed", ex);
                return LayerResult.Fail(Name, ex.Message);
            }

            return LayerResult.Skip("Operation not supported by window messages");
        }
    }
}
