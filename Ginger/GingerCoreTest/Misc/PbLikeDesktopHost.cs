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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
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
        private const int WmLButtonDown = 0x0201;
        private const int WmLButtonDblClk = 0x0203;
        private const int WmRButtonUp = 0x0205;
        private const int MkControl = 0x0008;

        private readonly NotificationCountingForm mForm;
        private readonly ClickCountingPanel mFacade;
        private bool mDisposed;

        public IntPtr EditHwnd { get; }
        public IntPtr ButtonHwnd { get; }
        public UIAuto.AutomationElement FacadeElement { get; }

        /// <summary>
        /// HWND of the facade itself. Like a grid cell it is neither an edit nor a
        /// button, so the message helpers that key off window class decline it.
        /// </summary>
        public IntPtr FacadeHwnd => mFacade.Handle;

        /// <summary>
        /// The frame that owns the facade, which is what an ancestor walk lands on when
        /// the target itself has no window of its own.
        /// </summary>
        public IntPtr FormHwnd => mForm.Handle;

        /// <summary>
        /// How many left-button presses the facade has received, which is the only
        /// way to tell a click that actually landed from one that was reported.
        /// </summary>
        public int FacadeClickCount => mFacade.ClickCount;

        /// <summary>
        /// How many double-click messages the facade has received. Counted apart from
        /// the presses because two presses and a double click are different events to
        /// the control, and only the latter makes it act on a double click.
        /// </summary>
        public int FacadeDoubleClickCount => mFacade.DoubleClickCount;

        /// <summary>
        /// How many right clicks the facade has received, counted apart from the left
        /// ones because a control answers the two buttons differently and a test that
        /// pooled them could not tell which one arrived.
        /// </summary>
        public int FacadeRightClickCount => mFacade.RightClickCount;

        /// <summary>
        /// Whether the last click the facade received carried the Ctrl modifier, which
        /// is what separates extending a selection from replacing it.
        /// </summary>
        public bool FacadeLastClickHadControl => mFacade.LastClickHadControl;

        /// <summary>
        /// How many times a control has told the frame it was clicked, and which one
        /// sent the last of those notifications.
        /// </summary>
        /// <remarks>
        /// The only evidence that a button was activated rather than merely handed a
        /// message. BM_CLICK answering non-zero says the window procedure ran and
        /// returned; the WM_COMMAND carrying BN_CLICKED that follows says it decided
        /// the button was pressed, and that notification is what an application's own
        /// click handler runs off.
        /// </remarks>
        public int ButtonActivationCount => mForm.ButtonActivationCount;

        public IntPtr LastActivatedControl => mForm.LastActivatedControl;

        /// <summary>
        /// Client coordinates of the last click the facade received, so a test can prove
        /// a click landed on the offset it asked for rather than merely landing.
        /// </summary>
        public int LastClickClientX => mFacade.LastClickX;

        public int LastClickClientY => mFacade.LastClickY;

        public int FacadeWidth => mFacade.Width;

        public int FacadeHeight => mFacade.Height;

        /// <summary>
        /// Top-left of the facade in screen coordinates, the origin a ClickXY offset is
        /// measured from.
        /// </summary>
        public Point FacadeTopLeft => mFacade.PointToScreen(Point.Empty);

        /// <summary>
        /// Pumps this host's messages until <paramref name="reached"/> holds, or gives
        /// up after <paramref name="timeoutMs"/>.
        /// </summary>
        /// <remarks>
        /// A queued click waits for the owning thread to pump. A real application pumps
        /// constantly; a test holds its thread inside the assertion instead, so without
        /// this a click that was delivered correctly would read as one that never
        /// arrived.
        /// </remarks>
        public bool PumpUntil(Func<bool> reached, int timeoutMs = 2000)
        {
            Stopwatch clock = Stopwatch.StartNew();
            while (!reached() && clock.ElapsedMilliseconds < timeoutMs)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }
            return reached();
        }

        /// <summary>
        /// The frame the native children are parented to, which is where a button
        /// sends word that it was clicked.
        /// </summary>
        private sealed class NotificationCountingForm : Form
        {
            private const int WmCommand = 0x0111;
            private const int BnClicked = 0;

            public int ButtonActivationCount { get; private set; }

            public IntPtr LastActivatedControl { get; private set; }

            protected override void WndProc(ref Message m)
            {
                // The sending control is in lParam and the notification code in the
                // high half of wParam, which is what separates a click from the other
                // things a control reports through the same message.
                if (m.Msg == WmCommand
                    && ((m.WParam.ToInt64() >> 16) & 0xFFFF) == BnClicked
                    && m.LParam != IntPtr.Zero)
                {
                    ButtonActivationCount++;
                    LastActivatedControl = m.LParam;
                }

                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// Stands in for a custom drawn control: it reports no Invoke pattern and no
        /// accessible default action, but it does receive mouse messages.
        /// </summary>
        private sealed class ClickCountingPanel : Panel
        {
            public int ClickCount { get; private set; }

            /// <summary>
            /// Where the last click actually landed, in client coordinates. A click that
            /// arrives at the wrong spot is indistinguishable from a correct one if only
            /// the count is checked.
            /// </summary>
            public int LastClickX { get; private set; } = -1;

            public int LastClickY { get; private set; } = -1;

            public int DoubleClickCount { get; private set; }

            public int RightClickCount { get; private set; }

            /// <summary>
            /// Whether the last left press said Ctrl was held.
            /// </summary>
            /// <remarks>
            /// Read off the message's own wParam rather than from <c>ModifierKeys</c>,
            /// which reports the real keyboard and would answer the same whether or not
            /// the modifier was ever sent - so a test built on it would pass on an
            /// implementation that dropped the modifier entirely.
            /// </remarks>
            public bool LastClickHadControl { get; private set; }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ClickCount++;
                    LastClickX = e.X;
                    LastClickY = e.Y;
                }
                base.OnMouseDown(e);
            }

            // Counted off the raw message rather than the MouseDoubleClick event,
            // because whether that event is raised depends on the control's own style
            // bits, and what is being proved here is only that the message arrived.
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmLButtonDblClk)
                {
                    DoubleClickCount++;
                }
                else if (m.Msg == WmLButtonDown)
                {
                    LastClickHadControl = (m.WParam.ToInt64() & MkControl) == MkControl;
                }
                else if (m.Msg == WmRButtonUp)
                {
                    // The release rather than the press, because that is the message a
                    // real right click finishes on and the one DefWindowProc turns into
                    // the context menu.
                    RightClickCount++;
                }
                base.WndProc(ref m);
            }
        }

        public PbLikeDesktopHost()
        {
            mForm = new NotificationCountingForm
            {
                Text = "PB-like host",
                Width = 420,
                Height = 180,
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new Point(-32000, -32000)
            };

            mFacade = new ClickCountingPanel
            {
                Width = 90,
                Height = 28,
                Location = new Point(12, 12)
            };
            mForm.Controls.Add(mFacade);
            mForm.CreateControl();
            mFacade.CreateControl();
            mForm.Show();

            EditHwnd = CreateChild("EDIT", string.Empty, 12, 52, 220, 24, EsAutoHScroll);
            ButtonHwnd = CreateChild("BUTTON", "OK", 244, 52, 80, 24, 0);
            if (EditHwnd == IntPtr.Zero || ButtonHwnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create native child controls for the PB-like host.");
            }

            FacadeElement = UIAuto.AutomationElement.FromHandle(mFacade.Handle);
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
                // Stated because it is what lets the message paths run at all: these
                // tests stand in for a desktop the mouse cannot reach, and they have to
                // behave the same whether or not the machine running them is locked.
                DesktopCanTakePhysicalInput = false,
                TimeoutMs = 2000
            };
        }

        /// <summary>
        /// Click context aimed at the centre of the facade, which is the same point
        /// the physical mouse would have been sent to.
        /// </summary>
        public DesktopActionContext CreateFacadePointContext(bool allowPhysicalInput)
        {
            return CreateFacadePointContext(DesktopOperation.Click, allowPhysicalInput);
        }

        public DesktopActionContext CreateFacadePointContext(DesktopOperation operation, bool allowPhysicalInput)
        {
            Point centre = mFacade.PointToScreen(new Point(mFacade.Width / 2, mFacade.Height / 2));

            DesktopActionContext context = CreateContext(operation, FacadeHwnd);
            context.AllowPhysicalInput = allowPhysicalInput;
            context.HasTargetPoint = true;
            context.TargetScreenX = centre.X;
            context.TargetScreenY = centre.Y;
            context.PointOwnerWindowHandle = FacadeHwnd;
            return context;
        }

        /// <summary>
        /// Context aimed at the centre of the native button, the shape that stalled in
        /// the locked run.
        /// </summary>
        /// <remarks>
        /// A button is the one control that answers a press by taking the mouse and
        /// running its own loop, so it is the case a coordinate click has to be proven
        /// against rather than the drawn facade.
        /// </remarks>
        public DesktopActionContext CreateButtonPointContext(DesktopOperation operation)
        {
            GetWindowRect(ButtonHwnd, out RECT bounds);

            DesktopActionContext context = CreateContext(operation, ButtonHwnd);
            context.AllowPhysicalInput = false;
            context.HasTargetPoint = true;
            context.TargetScreenX = bounds.left + ((bounds.right - bounds.left) / 2);
            context.TargetScreenY = bounds.top + ((bounds.bottom - bounds.top) / 2);
            context.PointOwnerWindowHandle = ButtonHwnd;
            return context;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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

    /// <summary>
    /// A window on a thread that never pumps, so anything sent to it can only time
    /// out.
    /// </summary>
    /// <remarks>
    /// Stands for the application that is busy rather than the locator that is wrong:
    /// this window genuinely owns the point it is asked about and still cannot answer.
    /// The two were reported identically, and the resulting refusal contradicted the
    /// bounds printed beside it.
    /// </remarks>
    internal sealed class UnresponsiveWindow : IDisposable
    {
        private const int WsPopup = unchecked((int)0x80000000);
        private const int WsVisible = 0x10000000;
        private const int Width = 120;
        private const int Height = 60;

        private readonly ManualResetEventSlim mCreated = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim mRelease = new ManualResetEventSlim(false);
        private readonly Thread mOwner;

        public IntPtr Handle { get; private set; }

        public int CentreScreenX => Width / 2;

        public int CentreScreenY => Height / 2;

        public UnresponsiveWindow()
        {
            mOwner = new Thread(Own) { IsBackground = true };
            mOwner.Start();
            mCreated.Wait(5000);
        }

        private void Own()
        {
            Handle = CreateWindowEx(0, "Static", "ginger-unresponsive", WsPopup | WsVisible,
                0, 0, Width, Height, IntPtr.Zero, IntPtr.Zero, GetModuleHandle(null), IntPtr.Zero);
            mCreated.Set();

            // No message loop, deliberately: an unpumped queue is the whole point.
            mRelease.Wait();

            if (Handle != IntPtr.Zero)
            {
                DestroyWindow(Handle);
            }
        }

        public void Dispose()
        {
            mRelease.Set();
            // Joined before the events go, because the owning thread is still waiting on
            // one of them and a window destroyed on any other thread leaks.
            mOwner.Join(5000);
            mCreated.Dispose();
            mRelease.Dispose();
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowEx(int exStyle, string className, string windowName, int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandle(string moduleName);
    }

    /// <summary>
    /// A window that takes the first few characters and then stops answering, which
    /// is the application that goes busy partway through a value.
    /// </summary>
    /// <remarks>
    /// The case <see cref="UnresponsiveWindow"/> cannot stand for: there the first
    /// message already fails and the control is left exactly as it was, so the value
    /// can safely be sent again by another route. Here the control genuinely holds a
    /// prefix, and sending the value again is what puts a second copy of it in.
    /// </remarks>
    internal sealed class StallsPartwayWindow : IDisposable
    {
        private const int WmChar = 0x0102;
        private const int WsPopup = unchecked((int)0x80000000);

        private readonly ManualResetEventSlim mCreated = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim mRelease = new ManualResetEventSlim(false);
        private readonly Thread mOwner;
        private readonly int mAcceptBeforeStalling;
        private int mAccepted;

        public IntPtr Handle { get; private set; }

        public StallsPartwayWindow(int acceptBeforeStalling)
        {
            mAcceptBeforeStalling = acceptBeforeStalling;
            mOwner = new Thread(Own) { IsBackground = true };
            mOwner.Start();
            mCreated.Wait(5000);
        }

        private void Own()
        {
            Stalling window = new Stalling(this);
            window.CreateHandle(new CreateParams
            {
                Caption = "ginger-stalls-partway",
                Style = WsPopup,
                Width = 120,
                Height = 60
            });
            Handle = window.Handle;
            mCreated.Set();

            // Pumped rather than left alone, because the messages before the stall
            // have to be taken for there to be a partial delivery at all.
            while (!mRelease.IsSet)
            {
                Application.DoEvents();
                Thread.Sleep(1);
            }

            window.DestroyHandle();
        }

        private sealed class Stalling : NativeWindow
        {
            private readonly StallsPartwayWindow mHost;

            internal Stalling(StallsPartwayWindow host)
            {
                mHost = host;
            }

            protected override void WndProc(ref Message m)
            {
                // Held for good rather than merely slowed: the sender has to see this
                // message time out while the ones before it went through, and a delay
                // long enough to guarantee that on a loaded build agent is a delay the
                // test would then wait out on every run.
                if (m.Msg == WmChar && mHost.mAccepted++ >= mHost.mAcceptBeforeStalling)
                {
                    mHost.mRelease.Wait();
                }

                base.WndProc(ref m);
            }
        }

        public void Dispose()
        {
            mRelease.Set();
            mOwner.Join(5000);
            mCreated.Dispose();
            mRelease.Dispose();
        }
    }
}
