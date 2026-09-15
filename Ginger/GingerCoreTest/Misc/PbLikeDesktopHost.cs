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
using GingerCore.Drivers.Common.LegacyAutomation;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UIAuto = UIAComWrapperNetstandard::System.Windows.Automation;

namespace GingerCoreTest.Misc
{
    /// <summary>
    /// In-process stand-in for a PowerBuilder screen: Ginger attaches to a facade
    /// that has no Value/Invoke pattern, while a native Edit and Button HWND still
    /// accept window messages.
    /// </summary>
    internal sealed class PbLikeDesktopHost : IDisposable
    {
        private const int WsChild = 0x40000000;
        private const int WsVisible = 0x10000000;
        private const int WsTabStop = 0x00010000;
        private const int EsAutoHScroll = 0x0080;

        private readonly Form mForm;
        private bool mDisposed;

        public IntPtr EditHwnd { get; }
        public IntPtr ButtonHwnd { get; }
        public UIAuto.AutomationElement FacadeElement { get; }

        public PbLikeDesktopHost()
        {
            mForm = new Form
            {
                Text = "PB-like host",
                Width = 420,
                Height = 180,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000)
            };

            Panel facade = new Panel
            {
                Width = 90,
                Height = 28,
                Location = new Point(12, 12)
            };
            mForm.Controls.Add(facade);
            mForm.CreateControl();
            facade.CreateControl();
            mForm.Show();

            EditHwnd = CreateChild("EDIT", string.Empty, 12, 52, 220, 24, EsAutoHScroll);
            ButtonHwnd = CreateChild("BUTTON", "OK", 244, 52, 80, 24, 0);
            if (EditHwnd == IntPtr.Zero || ButtonHwnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create native child controls for the PB-like host.");
            }

            FacadeElement = UIAuto.AutomationElement.FromHandle(facade.Handle);
            if (FacadeElement == null)
            {
                throw new InvalidOperationException("Failed to resolve a UIA element for the PB-like facade.");
            }
        }

        public DesktopActionContext CreateContext(DesktopOperation operation, IntPtr nativeHandle, string value = null)
        {
            return new DesktopActionContext
            {
                Operation = operation,
                AutomationElement = FacadeElement,
                NativeWindowHandle = nativeHandle,
                Value = value,
                AllowPhysicalInput = false,
                TimeoutMs = 2000
            };
        }

        public void Dispose()
        {
            if (mDisposed)
            {
                return;
            }

            mDisposed = true;
            if (ButtonHwnd != IntPtr.Zero)
            {
                DestroyWindow(ButtonHwnd);
            }
            if (EditHwnd != IntPtr.Zero)
            {
                DestroyWindow(EditHwnd);
            }
            mForm.Dispose();
        }

        private IntPtr CreateChild(string className, string title, int x, int y, int width, int height, int extraStyle)
        {
            int style = WsChild | WsVisible | WsTabStop | extraStyle;
            return CreateWindowEx(0, className, title, style, x, y, width, height, mForm.Handle, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string moduleName);
    }
}
