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

using GingerCore.Drivers.Common.LegacyAutomation;
using GingerCore.Drivers.PBDriver.DesktopAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace GingerCoreTest.Misc
{
    [TestClass]
    public class PbLikeDesktopAutomationEvidenceTest
    {
        public TestContext TestContext { get; set; }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateWindowExW(int exStyle, string className, string windowName,
            int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetModuleHandleW(string moduleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [TestMethod]
        public void UiaOnly_CannotSetValueOnFacadeWithoutValuePattern()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine uiaOnly = new DesktopAutomationEngine([new UiaPatternLayer()]);

            DesktopEngineResult result = uiaOnly.Execute(host.CreateContext(DesktopOperation.SetValue, host.EditHwnd, "account-99"));

            Assert.IsFalse(result.Success);
            Assert.AreNotEqual("account-99", Win32Native.GetControlText(host.EditHwnd, 2000));
        }

        [TestMethod]
        public void UiaPlusWin32_SetsAndReadsEditWhenUiaPatternsAreMissing()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine engine = new DesktopAutomationEngine([new UiaPatternLayer(), new Win32Layer()]);

            DesktopEngineResult setResult = engine.Execute(host.CreateContext(DesktopOperation.SetValue, host.EditHwnd, "account-99"));

            Assert.IsTrue(setResult.Success);
            Assert.AreEqual("Win32", setResult.UsedLayer);
            Assert.AreEqual("account-99", Win32Native.GetControlText(host.EditHwnd, 2000));

            DesktopEngineResult getResult = engine.Execute(host.CreateContext(DesktopOperation.GetValue, host.EditHwnd));

            Assert.IsTrue(getResult.Success);
            Assert.AreEqual("Win32", getResult.UsedLayer);
            Assert.AreEqual("account-99", getResult.OutputValue);
        }

        [TestMethod]
        public void UiaOnly_CannotClickFacadeWithoutInvokePattern()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine uiaOnly = new DesktopAutomationEngine([new UiaPatternLayer()]);

            DesktopEngineResult result = uiaOnly.Execute(host.CreateContext(DesktopOperation.Click, host.ButtonHwnd));

            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void UiaPlusWin32_ClicksNativeButtonWithoutPhysicalInput()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine engine = new DesktopAutomationEngine([new UiaPatternLayer(), new Win32Layer()]);
            DesktopActionContext context = host.CreateContext(DesktopOperation.Click, host.ButtonHwnd);

            DesktopEngineResult result = engine.Execute(context);

            Assert.IsFalse(context.AllowPhysicalInput);
            Assert.IsTrue(result.Success);
            Assert.AreEqual("Win32", result.UsedLayer);
            StringAssert.Contains(result.ExecutionInfo, "BM_CLICK");

            // The message answering says only that the button's window procedure ran.
            // What says the button was activated is the notification it sends its
            // parent afterwards, which is what an application's click handler runs
            // off, so that is what the click is proved by here.
            Assert.IsTrue(host.PumpUntil(() => host.ButtonActivationCount > 0),
                "The button has to tell the frame it was clicked, not merely take the message");
            Assert.AreEqual(host.ButtonHwnd, host.LastActivatedControl,
                "The notification has to come from the button that was aimed at");
        }

        [TestMethod]
        public void LayeredEngine_SetsAndClicksNativeChildren_WithoutForegroundOrCursor()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine engine = DesktopAutomationEngine.Default;

            IntPtr foregroundBefore = GetForegroundWindow();

            DesktopActionContext setContext = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.SetValue, "quiet-42", allowPhysicalInput: true);
            setContext.NativeWindowHandle = host.EditHwnd;
            setContext.TimeoutMs = 2000;

            DesktopEngineResult setResult = engine.Execute(setContext);
            Assert.IsTrue(setResult.Success, "SetValue should succeed via the layered engine");
            Assert.AreNotEqual("PhysicalInput", setResult.UsedLayer,
                "A control the quiet layers can serve must never reach the physical mouse or keyboard");
            Assert.AreEqual("quiet-42", Win32Native.GetControlText(host.EditHwnd, 2000));

            // Physical input withheld, which is what both the non-intrusive flag and a
            // locked screen produce, and the only state in which the quiet route is
            // meant to carry a click. Permitted and working, the mouse keeps it.
            DesktopActionContext clickContext = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: false);
            clickContext.NativeWindowHandle = host.ButtonHwnd;
            clickContext.TimeoutMs = 2000;

            DesktopEngineResult clickResult = engine.Execute(clickContext);
            Assert.IsTrue(clickResult.Success, "Click should succeed via the layered engine");
            Assert.AreNotEqual("PhysicalInput", clickResult.UsedLayer);
            StringAssert.Contains(clickResult.ExecutionInfo, "BM_CLICK");

            Assert.AreEqual(foregroundBefore, GetForegroundWindow(),
                "The layered engine must not steal foreground focus");
        }

        /// <summary>
        /// The mouse keeps every click it already had on an ordinary unlocked run.
        /// </summary>
        /// <remarks>
        /// A button whose Invoke and default action both decline went to the mouse
        /// before this chain existed. BM_CLICK is not interchangeable with a real
        /// click for every control, so letting it answer here would quietly change
        /// what a default run does to such a button - which is the one thing this
        /// chain must not do, and what it did until the guard moved ahead of the
        /// button message rather than behind it.
        /// </remarks>
        [TestMethod]
        public void Win32_StandsAsideForTheMouse_WhenPhysicalInputIsPermittedAndWorking()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: true);
            context.NativeWindowHandle = host.ButtonHwnd;
            context.TimeoutMs = 2000;

            if (!context.AllowPhysicalInput)
            {
                Assert.Inconclusive("This pins behaviour on a desktop that can take physical input.");
            }

            DesktopEngineResult result = new DesktopAutomationEngine([new Win32Layer()]).Execute(context);

            Assert.IsFalse(result.Success,
                "Win32 must decline so the click falls through to the mouse, as it did before the layered"
                + " chain existed: " + result.ExecutionInfo);
        }

        [TestMethod]
        public void Engine_ReportsPlainFailureRatherThanThrowing_WhenNoLayerCanHandleTheTarget()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopAutomationEngine engine = DesktopAutomationEngine.Default;

            // The facade exposes no ValuePattern and a top-level HWND is not an
            // edit, so every layer declines. The engine must report that as a
            // plain failure so the caller can fall back to its legacy path; a
            // throw here would fail an action that works today.
            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.SetValue, "no-layer", allowPhysicalInput: true);
            context.NativeWindowHandle = GetForegroundWindow();
            context.TimeoutMs = 2000;

            DesktopEngineResult result = engine.Execute(context);

            Assert.IsFalse(result.Success);
            StringAssert.Contains(result.ErrorMessage, "Ginger could not set a value on this element");
        }

        /// <summary>
        /// The mapper passes the caller's permission straight through, narrowed by
        /// nothing except whether the desktop can actually take input.
        /// </summary>
        /// <remarks>
        /// Regression: a grid cell with no Invoke pattern, no accessible default
        /// action and no window handle can only be clicked with the mouse, and
        /// clamping that away on a usable desktop made every such Table action fail.
        /// Stated against the live desktop rather than assuming an unlocked agent, so
        /// the same contract is pinned on a locked machine - where the answer has to
        /// be the opposite one, and keeping the mouse would reach nothing.
        /// </remarks>
        [TestMethod]
        public void Mapper_KeepsPhysicalInputAsLastResortOnlyWhileTheDesktopCanTakeIt()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: true);

            Assert.AreEqual(InteractiveDesktop.IsAvailable(), context.AllowPhysicalInput,
                "The mouse is the only way to reach such a control on a usable desktop, and reaches nothing on a locked one");
        }

        [TestMethod]
        public void Mapper_HonoursCallersThatNeverAllowedPhysicalInput()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: false);

            Assert.IsFalse(context.AllowPhysicalInput,
                "The desktop probe must never add physical input to a call site that opted out");
        }

        [TestMethod]
        public void Win32KeyMessages_TypesIntoChildEditWithoutFocusSteal()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            IntPtr foregroundBefore = GetForegroundWindow();

            bool ok = Win32KeyMessages.SendChars(host.EditHwnd, "abc123", 2000);

            Assert.IsTrue(ok, "Expected Win32KeyMessages.SendChars to deliver WM_CHAR to the child edit");
            Assert.AreEqual("abc123", Win32Native.GetControlText(host.EditHwnd, 2000));

            Assert.AreEqual(foregroundBefore, GetForegroundWindow(),
                "Win32KeyMessages must not activate a different foreground window");
        }

        /// <summary>
        /// The shape behind the locked run's Send Keys failure: the field was expected
        /// to hold "Gingernotes" and held "notes".
        /// </summary>
        /// <remarks>
        /// The keyboard path focuses the control by clicking it and then types, and
        /// behind a lock screen that click lands nowhere, so nothing was typed while
        /// the step still reported a pass. Typing into the control's own window has to
        /// add to what is there rather than replace it, or a Send Keys step would start
        /// overwriting the fields it has only ever appended to.
        /// </remarks>
        [TestMethod]
        public void MessageTyping_AddsToTheExistingValueRatherThanReplacingIt()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            Assert.IsTrue(Win32Native.SetControlText(host.EditHwnd, "notes", 2000), "the field could not be primed");

            Assert.IsTrue(Win32KeyMessages.SendChars(host.EditHwnd, "Ginger", 2000), "the control refused the characters");

            string typed = Win32Native.GetControlText(host.EditHwnd, 2000);
            Assert.AreEqual("notes".Length + "Ginger".Length, typed.Length,
                "the value was replaced rather than typed into: " + typed);
            StringAssert.Contains(typed, "notes", typed);
            StringAssert.Contains(typed, "Ginger", typed);
        }

        [TestMethod]
        public void Win32KeyMessages_RejectsTopLevelOnlyTarget()
        {
            IntPtr resolved = IntPtr.Zero;

            // A window with no focused child, on a thread that has never held
            // focus, so the resolver has nothing to narrow down to.
            Thread worker = new Thread(() =>
            {
                using Form frame = new Form
                {
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual,
                    Location = new System.Drawing.Point(-32000, -32000)
                };
                frame.CreateControl();

                resolved = Win32KeyMessages.ResolveFocusedChild(frame.Handle);
            });
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
            worker.Join();

            Assert.AreEqual(IntPtr.Zero, resolved,
                "A key message posted to a frame returns non-zero while doing nothing, so a top-level-only target must be rejected");
        }

        /// <summary>
        /// The shape behind a PowerBuilder value that was typed but never committed.
        /// </summary>
        /// <remarks>
        /// Tab belongs to the dialog manager, which only ever sees keys that came off
        /// the message queue, so a WM_KEYDOWN sent straight to a standard edit reaches
        /// a window procedure with no reason to act on it. The control still takes the
        /// message and still answers zero, which is the same answer it gives for a key
        /// it acted on - so a delivery check reads that as a commit, and the keyboard
        /// that would have done the job never runs.
        /// </remarks>
        [TestMethod]
        public void SendTab_ReportsFailureWhenTheKeystrokeMovesNoFocus()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            SetFocus(host.EditHwnd);
            Assert.AreEqual(host.EditHwnd, GetFocus(), "the field could not be focused, so there is nothing to tab out of");

            bool committed = Win32KeyMessages.SendTab(host.EditHwnd, 2000);

            Assert.AreEqual(host.EditHwnd, GetFocus(),
                "the premise of this test is a Tab that does nothing, and this one moved the focus");
            Assert.IsFalse(committed,
                "SendTab reported a commit the focus shows never happened, so the caller skipped the keyboard fallback");
        }

        /// <summary>
        /// The other half of the same judgement: a control that reads Tab itself, the
        /// way a DataWindow steps between columns, must still be reported as a
        /// success. Calling that a failure would send the keyboard after it and tab
        /// twice, past the field the run meant to land on.
        /// </summary>
        [TestMethod]
        public void SendTab_ReportsSuccessWhenTheControlMovesTheFocusItself()
        {
            using Form frame = new Form
            {
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.Manual,
                Location = new System.Drawing.Point(-32000, -32000)
            };

            using TextBox second = new TextBox { Top = 40 };
            using SelfTabbingBox first = new SelfTabbingBox(second);
            frame.Controls.Add(first);
            frame.Controls.Add(second);
            frame.Show();

            Assert.IsTrue(first.Focus(), "the first field could not be focused");

            bool committed = Win32KeyMessages.SendTab(first.Handle, 2000);

            Assert.IsTrue(second.Focused, "the premise of this test is a control that moves the focus on Tab");
            Assert.IsTrue(committed,
                "SendTab called a Tab that did move the focus a failure, which sends the keyboard after it and tabs twice");
        }

        /// <summary>
        /// A plain field in a dialog must commit without the keyboard.
        /// </summary>
        /// <remarks>
        /// This is the ordinary case and it was the one that never worked. Tab
        /// belongs to the dialog manager, so a key sent straight to the field
        /// reaches a window procedure with no reason to act on it and every
        /// set-value in a standard dialog fell through to the keyboard - taking the
        /// foreground to press a key the dialog could be asked for directly, which
        /// is the one thing the non-intrusive route exists to avoid.
        ///
        /// Built from the dialog class rather than from a Form because only a real
        /// dialog runs the dialog manager, and a container that does its tab
        /// handling in the message loop instead cannot be served this way at all.
        /// </remarks>
        [TestMethod]
        public void SendTab_CommitsAPlainDialogFieldByAskingTheDialogRatherThanTheKeyboard()
        {
            const int WsOverlapped = 0x00CF0000;
            const int WsChild = 0x40000000;
            const int WsVisible = 0x10000000;
            const int WsTabStop = 0x00010000;
            const string DialogClass = "#32770";

            IntPtr instance = GetModuleHandleW(null);
            IntPtr dialog = CreateWindowExW(0, DialogClass, "commit host", WsOverlapped | WsVisible,
                -32000, -32000, 300, 200, IntPtr.Zero, IntPtr.Zero, instance, IntPtr.Zero);
            Assert.AreNotEqual(IntPtr.Zero, dialog, "could not create a dialog to test against");

            try
            {
                IntPtr first = CreateWindowExW(0, "EDIT", string.Empty, WsChild | WsVisible | WsTabStop,
                    0, 0, 200, 24, dialog, IntPtr.Zero, instance, IntPtr.Zero);
                IntPtr second = CreateWindowExW(0, "EDIT", string.Empty, WsChild | WsVisible | WsTabStop,
                    0, 40, 200, 24, dialog, IntPtr.Zero, instance, IntPtr.Zero);
                Assert.AreNotEqual(IntPtr.Zero, first, "the dialog has no field to tab out of");
                Assert.AreNotEqual(IntPtr.Zero, second, "the dialog has nowhere to tab to");

                SetFocus(first);
                Assert.AreEqual(first, GetFocus(), "the field could not be focused, so there is nothing to commit");

                bool committed = Win32KeyMessages.SendTab(first, 2000);

                Assert.IsTrue(committed,
                    "a dialog field could not be tabbed out of without the keyboard, so every set-value still takes the foreground");
                Assert.AreEqual(second, GetFocus(),
                    "SendTab reported a commit while the keyboard was still in the field it was meant to leave");
            }
            finally
            {
                DestroyWindow(dialog);
            }
        }

        /// <summary>
        /// Stands in for a control that reads keys itself instead of leaving them to
        /// the dialog manager, which is how a DataWindow moves between its columns.
        /// </summary>
        private sealed class SelfTabbingBox : TextBox
        {
            private const int WmKeyDown = 0x0100;
            private const int VkTab = 0x09;

            private readonly Control mMoveTo;

            internal SelfTabbingBox(Control moveTo)
            {
                mMoveTo = moveTo;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WmKeyDown && (int)m.WParam == VkTab)
                {
                    mMoveTo.Focus();
                    return;
                }
                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// Reproduces the regression seen in the locked run: a click on a target that
        /// exposes no Invoke pattern, no accessible default action and no button class
        /// could only be served by the mouse, so it failed outright once the desktop
        /// stopped accepting physical input.
        /// </summary>
        [TestMethod]
        public void Win32_ClicksAPatternlessTargetByMessageWhenTheMouseIsUnavailable()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);

            DesktopEngineResult result = DesktopAutomationEngine.Default.Execute(context);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.AreEqual("Win32", result.UsedLayer);
            StringAssert.Contains(result.ExecutionInfo, "WM_LBUTTONDOWN/UP");
            Assert.IsTrue(host.PumpUntil(() => host.FacadeClickCount == 1),
                "The click has to land, not merely be reported; got " + host.FacadeClickCount);
        }

        /// <summary>
        /// The non-breaking contract. A message click and a real one are not
        /// interchangeable for every control, so while the mouse is available the
        /// click is left to it and this path stays out of the way entirely.
        /// </summary>
        [TestMethod]
        public void Win32_LeavesThePatternlessClickToTheMouseWhileItIsAvailable()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: true);
            DesktopAutomationEngine win32Only = new DesktopAutomationEngine([new Win32Layer()]);

            DesktopEngineResult result = win32Only.Execute(context);

            Assert.IsFalse(result.Success, "Win32 must decline so the mouse keeps serving this click");
            Assert.AreEqual(0, host.FacadeClickCount);
        }

        /// <summary>
        /// A point the window does not own would be answered as handled while acting
        /// on the wrong place in the control, or on nothing at all.
        /// </summary>
        [TestMethod]
        public void Win32_RefusesATargetPointOutsideTheControl()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);
            context.TargetScreenX += 5000;
            context.TargetScreenY += 5000;

            DesktopAutomationEngine win32Only = new DesktopAutomationEngine([new Win32Layer()]);
            DesktopEngineResult result = win32Only.Execute(context);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(0, host.FacadeClickCount);
        }

        /// <summary>
        /// The exact shape that failed in the locked run: UIA reports no window handle
        /// for a drawn cell, so the layer had nothing to aim at and declined even though
        /// the pixels belong to a window that accepts messages perfectly well.
        /// </summary>
        [TestMethod]
        public void Win32_ReachesATargetWithNoWindowOfItsOwnByDescendingFromTheOwningWindow()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);
            context.NativeWindowHandle = IntPtr.Zero;
            context.PointOwnerWindowHandle = host.FormHwnd;

            DesktopAutomationEngine win32Only = new DesktopAutomationEngine([new Win32Layer()]);
            DesktopEngineResult result = win32Only.Execute(context);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeClickCount == 1),
                "The descent has to land on the control drawing the point, not on the frame around it");
        }

        /// <summary>
        /// The regression behind the 19 Sep failure: ClickXY reported success having
        /// opened nothing, because the application never held the foreground and the
        /// injected click went to whatever window was in front. A Win32-only engine is
        /// what ClickXY falls back to, and it has to land on the offset it was given.
        /// </summary>
        [TestMethod]
        public void PointClick_LandsOnTheGivenOffsetRatherThanTheElementCentre()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);
            // Aim a quarter in from the top-left, the way a ClickXY offset would.
            context.TargetScreenX = host.FacadeTopLeft.X + (host.FacadeWidth / 4);
            context.TargetScreenY = host.FacadeTopLeft.Y + (host.FacadeHeight / 4);

            DesktopAutomationEngine win32Only = new DesktopAutomationEngine([new Win32Layer()]);
            DesktopEngineResult result = win32Only.Execute(context);

            Assert.IsTrue(result.Success, result.ErrorMessage);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeClickCount == 1),
                "The offset has to be delivered, not the element centre");
            Assert.AreEqual(host.FacadeWidth / 4, host.LastClickClientX,
                "A ClickXY that lands anywhere other than the offset it was given is the bug, not the fix");
            Assert.AreEqual(host.FacadeHeight / 4, host.LastClickClientY);
        }

        /// <summary>
        /// The gate that keeps existing automation untouched: this layer is the
        /// fallback, so while physical input is on the table it must decline and let
        /// the mouse do exactly what it does today.
        /// </summary>
        [TestMethod]
        public void PointClick_DeclinesWhileThePhysicalMouseIsStillUsable()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: true);

            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status);
            Assert.AreEqual(0, host.FacadeClickCount, "Nothing may be delivered on the declined path");
        }

        /// <summary>
        /// A caller that never used the mouse must not have a message click posted on
        /// its behalf while the mouse still works.
        /// </summary>
        /// <remarks>
        /// Most call sites switch physical input off permanently because the operation
        /// never moved the real mouse, so that flag says nothing about the desktop.
        /// Reading it as "the mouse is unavailable" would post a click for every one of
        /// them on an ordinary unlocked desktop, and because a posted click is
        /// acknowledged once the window takes it rather than once the control acts on
        /// it, clicks that honestly fail today would start reporting success. The
        /// desktop state is set on the context here rather than taken from the machine
        /// so this holds whether or not the screen running the test is locked.
        /// </remarks>
        [TestMethod]
        public void PointClick_IsWithheldFromCallersThatSimplyNeverUsedTheMouse()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            foreach (DesktopOperation operation in new[] { DesktopOperation.Click, DesktopOperation.DoubleClick })
            {
                DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);
                context.Operation = operation;
                context.DesktopCanTakePhysicalInput = true;

                LayerResult result = new Win32Layer().TryExecute(context);

                Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, operation + " was not left to the mouse");
                Assert.AreEqual(0, host.FacadeClickCount, "A " + operation + " was posted while the mouse still worked");
            }
        }

        /// <summary>
        /// The same caller on a desktop that genuinely cannot take physical input is
        /// the case the message click exists for, so it has to go through.
        /// </summary>
        [TestMethod]
        public void PointClick_ServesTheSameCallerOnceTheMouseCanReachNothing()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateFacadePointContext(allowPhysicalInput: false);
            context.DesktopCanTakePhysicalInput = false;

            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            host.PumpUntil(() => host.FacadeClickCount > 0, 2000);
            Assert.AreEqual(1, host.FacadeClickCount, "The one route left to this click did not reach the control");
        }

        /// <summary>
        /// The mapper decides the desktop question once and the layers read that
        /// answer, so a screen locked or unlocked mid-action cannot have two parts of
        /// the same action disagree about which route was available.
        /// </summary>
        [TestMethod]
        public void Mapper_SettlesTheDesktopQuestionOncePerAction()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext fromElement = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: true);

            Assert.AreEqual(InteractiveDesktop.IsAvailable(), fromElement.DesktopCanTakePhysicalInput,
                "The context has to carry the desktop state the mapper probed");
            Assert.IsFalse(fromElement.AllowPhysicalInput && !fromElement.DesktopCanTakePhysicalInput,
                "Physical input may never stay allowed once the desktop cannot take it");

            // A coordinate click carries the same answer as everything else rather than
            // declaring the mouse unavailable on its own account. Saying otherwise let
            // a blind click be posted to a desktop whose mouse worked, where nothing
            // reports back whether the control acted on it.
            DesktopActionContext forPoint = DesktopActionMapper.ForPoint(
                host.FacadeElement, DesktopOperation.Click, 10, 10);

            Assert.AreEqual(InteractiveDesktop.IsAvailable(), forPoint.DesktopCanTakePhysicalInput,
                "A coordinate click has to read the same desktop answer as every other action");
            Assert.IsFalse(forPoint.AllowPhysicalInput,
                "The point chain never runs physical input itself; the caller keeps that as its own last resort");
        }

        /// <summary>
        /// The layer can only aim at a target point if the mapper carries one, so the
        /// element rectangle has to survive the hop into the context.
        /// </summary>
        [TestMethod]
        public void Mapper_CarriesTheTargetPointFromTheElementRectangle()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: false);

            Assert.IsTrue(context.HasTargetPoint, "Without a point there is no message click to make");
            Assert.AreNotEqual(IntPtr.Zero, context.PointOwnerWindowHandle,
                "Without an owning window there is nothing to send the click to");

            var bounds = host.FacadeElement.Current.BoundingRectangle;
            Assert.IsTrue(context.TargetScreenX > bounds.X && context.TargetScreenX < bounds.X + bounds.Width);
            Assert.IsTrue(context.TargetScreenY > bounds.Y && context.TargetScreenY < bounds.Y + bounds.Height);
        }

        [TestMethod]
        public void DataWindowMsaa_RecognisesPowerBuilderDataWindowClassesOnly()
        {
            // PowerBuilder suffixes the DataWindow class with the runtime build.
            Assert.IsTrue(DataWindowMsaaLayer.IsDataWindowClass("pbdw100"));
            Assert.IsTrue(DataWindowMsaaLayer.IsDataWindowClass("pbdw170"));
            Assert.IsTrue(DataWindowMsaaLayer.IsDataWindowClass("PBDW125"));

            // Everything else must keep the behaviour it has today.
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass("PBTabControl32_100"));
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass("Edit"));
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass("Button"));
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass("FNWND390"));
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass(""));
            Assert.IsFalse(DataWindowMsaaLayer.IsDataWindowClass(null));
        }

        [TestMethod]
        public void DataWindowMsaa_DeclinesOrdinaryControls_SoTodaysLayersStillDecide()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            Assert.IsFalse(layer.CanHandle(host.CreateContext(DesktopOperation.SetValue, host.EditHwnd, "x")),
                "A plain EDIT must stay on the Win32 layer");
            Assert.IsFalse(layer.CanHandle(host.CreateContext(DesktopOperation.Click, host.ButtonHwnd)),
                "A plain BUTTON must stay on the Win32 layer");
            Assert.IsFalse(layer.CanHandle(host.CreateContext(DesktopOperation.GetValue, IntPtr.Zero)),
                "No HWND means there is no DataWindow to talk to");
        }

        /// <summary>
        /// The shape behind a locked run that reported a grid value as set while the
        /// cell kept its old text.
        /// </summary>
        /// <remarks>
        /// A DataWindow cell is drawn rather than created as a window, so it reports
        /// no handle of its own. Reading only the target's handle meant this layer
        /// declined every cell it exists to serve, the step fell through to the
        /// keyboard, and behind a lock screen the keyboard wrote nothing.
        /// </remarks>
        [TestMethod]
        public void DataWindowMsaa_ClaimsACellThatHasNoWindowOfItsOwn()
        {
            using DataWindowClassWindow dataWindow = new DataWindowClassWindow();
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            DesktopActionContext cell = new DesktopActionContext
            {
                Operation = DesktopOperation.SetValue,
                NativeWindowHandle = IntPtr.Zero,
                PointOwnerWindowHandle = dataWindow.Handle,
                Value = "Pune",
                TimeoutMs = 2000
            };

            Assert.IsTrue(layer.CanHandle(cell),
                "A cell owned by a DataWindow must reach the only layer that can write it");
        }

        [TestMethod]
        public void DataWindowMsaa_StillDeclinesACellOwnedByAnOrdinaryWindow()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            DesktopActionContext cell = new DesktopActionContext
            {
                Operation = DesktopOperation.SetValue,
                NativeWindowHandle = IntPtr.Zero,
                PointOwnerWindowHandle = host.FormHwnd,
                Value = "x",
                TimeoutMs = 2000
            };

            Assert.IsFalse(layer.CanHandle(cell),
                "Falling back to the owning window must not hand this layer ordinary controls");
        }

        /// <summary>
        /// The route left for a DataWindow that hands out its cell values but will
        /// not take one back: click into the cell and type, the way the physical
        /// fallback does it.
        /// </summary>
        /// <remarks>
        /// A coordinate click is queued rather than delivered in line, so it is
        /// still in the application's queue when the call that queued it returns.
        /// Typing at that moment addresses whatever held the keyboard beforehand,
        /// which is the wrong window as soon as the click is what opens an editor.
        ///
        /// The control starts with text and is clicked past the end of it, so
        /// waiting for the click and not waiting leave different contents behind.
        /// An empty control would read the same either way and prove nothing.
        /// </remarks>
        [TestMethod]
        public void Win32_WaitsForTheClickToLandBeforeTypingIntoWhatItOpened()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            Assert.IsTrue(Win32Native.SetControlText(host.EditHwnd, "AB", 2000));

            GetWindowRect(host.EditHwnd, out RECT edit);
            int centreX = edit.left + ((edit.right - edit.left) / 2);
            int centreY = edit.top + ((edit.bottom - edit.top) / 2);

            Assert.AreEqual(PointClickOutcome.Delivered,
                Win32Native.ClickAtScreenPoint(host.EditHwnd, centreX, centreY, 2000));

            // Waiting for the keyboard to arrive on the control is what proves the
            // queued click has been handled. The host shares this thread, so it has
            // to be told to pump; a real application is doing so continuously,
            // which is what the settle in WaitForEditor stands in for.
            Assert.IsTrue(host.PumpUntil(() => GetFocus() == host.EditHwnd),
                "The click has to be taken up before there is anywhere to type");

            Assert.IsTrue(Win32KeyMessages.SendChars(host.EditHwnd, "Pune", 2000));
            Assert.AreEqual("ABPune", Win32Native.GetControlText(host.EditHwnd, 2000),
                "The characters have to arrive behind the click, not ahead of it");
        }

        /// <summary>
        /// The wait for a click to be taken up has to wait even when it sees
        /// nothing change.
        /// </summary>
        /// <remarks>
        /// This is the case that made a locked run type ahead of its own click: the
        /// cell already held the keyboard from the step before, so nothing moved
        /// when it was clicked, and a wait that returned the moment it saw no
        /// change handed back the state from before the click.
        /// </remarks>
        [TestMethod]
        public void Win32_WaitsForAClickEvenWhereTheKeyboardNeverMoves()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            // Captured after everything has settled, so nothing moves during the
            // wait and only the floor can end it.
            Win32KeyMessages.FocusSnapshot settled = Win32KeyMessages.CaptureFocus(host.EditHwnd);

            const int ceilingMs = 400;

            Stopwatch clock = Stopwatch.StartNew();
            IntPtr editor = Win32KeyMessages.WaitForEditor(host.EditHwnd, settled, ceilingMs);
            clock.Stop();

            Assert.IsTrue(clock.ElapsedMilliseconds >= Win32KeyMessages.EditorSettleMs,
                "Returning straight away types ahead of the click: waited only " + clock.ElapsedMilliseconds + "ms");
            Assert.IsTrue(clock.ElapsedMilliseconds < ceilingMs * 3,
                "Nothing moving is not a reason to keep waiting past the ceiling: waited " + clock.ElapsedMilliseconds + "ms");
            Assert.AreEqual(host.EditHwnd, editor,
                "Having waited, it still has to answer with the window holding the keyboard");
        }

        /// <summary>
        /// A write is confirmed by reading the cell back, and a DataWindow hands
        /// that reading back padded out to the column width.
        /// </summary>
        /// <remarks>
        /// Taken from a real run: a Get Value on this grid returned
        /// "2109 Fox Dr, Champaign IL " with the trailing space still attached.
        /// Comparing exactly meant a value that was written correctly was reported
        /// as one the cell never took.
        /// </remarks>
        [TestMethod]
        public void DataWindowMsaa_AcceptsTheValueThroughTheDataWindowsOwnPadding()
        {
            Assert.IsTrue(DataWindowMsaaLayer.CellHoldsValue("Pune      ", "Pune"),
                "A character column pads on the right");
            Assert.IsTrue(DataWindowMsaaLayer.CellHoldsValue("      1250", "1250"),
                "A right aligned number pads on the left");
            Assert.IsTrue(DataWindowMsaaLayer.CellHoldsValue("2109 Fox Dr, Champaign IL ", "2109 Fox Dr, Champaign IL"));

            // Padding is all it is allowed to forgive. Anything else still has to
            // read as the cell refusing the value.
            Assert.IsFalse(DataWindowMsaaLayer.CellHoldsValue("Mumbai    ", "Pune"),
                "A cell still holding its old value has not taken the new one");
            Assert.IsFalse(DataWindowMsaaLayer.CellHoldsValue("PuneMumbai", "Pune"),
                "Text typed alongside what was already there is not the value either");
            Assert.IsFalse(DataWindowMsaaLayer.CellHoldsValue("Pu ne", "Pune"),
                "Spaces inside the value are part of it");
            Assert.IsFalse(DataWindowMsaaLayer.CellHoldsValue(null, "Pune"),
                "A cell that cannot be read is an unverifiable write");
        }

        /// <summary>
        /// The typing route is for a desktop the mouse cannot reach. While it can,
        /// the value stays with the physical fallback that writes these cells today.
        /// </summary>
        /// <remarks>
        /// This is the line that keeps an unlocked run behaving exactly as it does
        /// now. A message click is acknowledged once the window takes it, which is
        /// not the same as the cell accepting the value, so standing it in for input
        /// that is known to work would swap a route that writes the cell for one
        /// that only reports it did.
        /// </remarks>
        [TestMethod]
        public void DataWindowMsaa_LeavesTheValueToTheMouseWhileTheMouseCanReachIt()
        {
            using DataWindowClassWindow dataWindow = new DataWindowClassWindow();
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            DesktopActionContext cell = new DesktopActionContext
            {
                Operation = DesktopOperation.SetValue,
                NativeWindowHandle = IntPtr.Zero,
                PointOwnerWindowHandle = dataWindow.Handle,
                Value = "Pune",
                HasTargetPoint = true,
                TargetScreenX = -31900,
                TargetScreenY = -31940,
                DesktopCanTakePhysicalInput = true,
                TimeoutMs = 2000
            };

            Assert.AreEqual(LayerExecutionStatus.Skipped, layer.TryExecute(cell).Status,
                "With the mouse available the engine has to fall through to it, as it does today");
        }

        /// <summary>
        /// Without a rectangle there is no cell to aim at, and a click sent anyway
        /// would type the value into whatever the DataWindow has current.
        /// </summary>
        [TestMethod]
        public void DataWindowMsaa_DeclinesRatherThanTypingIntoACellItCannotAimAt()
        {
            using DataWindowClassWindow dataWindow = new DataWindowClassWindow();
            DataWindowMsaaLayer layer = new DataWindowMsaaLayer();

            DesktopActionContext cell = new DesktopActionContext
            {
                Operation = DesktopOperation.SetValue,
                NativeWindowHandle = IntPtr.Zero,
                PointOwnerWindowHandle = dataWindow.Handle,
                Value = "Pune",
                HasTargetPoint = false,
                DesktopCanTakePhysicalInput = false,
                TimeoutMs = 2000
            };

            Assert.AreEqual(LayerExecutionStatus.Skipped, layer.TryExecute(cell).Status);
        }

        /// <summary>
        /// A window of PowerBuilder's DataWindow class, which is all the layer's
        /// class test looks at. It draws nothing and serves no cells.
        /// </summary>
        private sealed class DataWindowClassWindow : IDisposable
        {
            private const string DataWindowClass = "pbdw170";
            private const int WsPopup = unchecked((int)0x80000000);

            private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            private static readonly object mGate = new object();
            private static WndProc mKeepAlive;
            private static bool mRegistered;

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            private struct WNDCLASS
            {
                public uint style;
                public IntPtr lpfnWndProc;
                public int cbClsExtra;
                public int cbWndExtra;
                public IntPtr hInstance;
                public IntPtr hIcon;
                public IntPtr hCursor;
                public IntPtr hbrBackground;
                [MarshalAs(UnmanagedType.LPWStr)] public string lpszMenuName;
                [MarshalAs(UnmanagedType.LPWStr)] public string lpszClassName;
            }

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern ushort RegisterClassW(ref WNDCLASS wndClass);

            [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr CreateWindowExW(int exStyle, string className, string windowName,
                int style, int x, int y, int width, int height, IntPtr parent, IntPtr menu, IntPtr instance, IntPtr param);

            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool DestroyWindow(IntPtr hWnd);

            [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
            private static extern IntPtr GetModuleHandleW(string moduleName);

            internal IntPtr Handle { get; }

            internal DataWindowClassWindow()
            {
                IntPtr instance = GetModuleHandleW(null);

                lock (mGate)
                {
                    if (!mRegistered)
                    {
                        // Held in a field because the window class outlives this call
                        // and the delegate behind the pointer must not be collected.
                        mKeepAlive = DefWindowProcW;
                        WNDCLASS wndClass = new WNDCLASS
                        {
                            lpfnWndProc = Marshal.GetFunctionPointerForDelegate(mKeepAlive),
                            hInstance = instance,
                            lpszClassName = DataWindowClass
                        };

                        if (RegisterClassW(ref wndClass) == 0)
                        {
                            throw new InvalidOperationException(
                                "Could not register a DataWindow window class: " + Marshal.GetLastWin32Error());
                        }
                        mRegistered = true;
                    }
                }

                Handle = CreateWindowExW(0, DataWindowClass, "dw", WsPopup, -32000, -32000, 240, 120,
                    IntPtr.Zero, IntPtr.Zero, instance, IntPtr.Zero);
                if (Handle == IntPtr.Zero)
                {
                    throw new InvalidOperationException(
                        "Could not create a DataWindow-classed window: " + Marshal.GetLastWin32Error());
                }
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                {
                    DestroyWindow(Handle);
                }
            }
        }

        [TestMethod]
        public void DataWindowMsaa_DoesNotChangeTheOutcomeForNonDataWindowControls()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            // The default engine now contains the DataWindow layer. A normal edit
            // must still be served by Win32 exactly as before it was added.
            DesktopActionContext context = host.CreateContext(DesktopOperation.SetValue, host.EditHwnd, "unchanged-7");
            DesktopEngineResult result = DesktopAutomationEngine.Default.Execute(context);

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Win32", result.UsedLayer);
            Assert.AreEqual("unchanged-7", Win32Native.GetControlText(host.EditHwnd, 2000));
        }

        /// <summary>
        /// A double click has to arrive as a double click. Behind a lock screen the
        /// mouse reaches nothing, and this is the only way such a target still gets
        /// one.
        /// </summary>
        /// <remarks>
        /// Asserts the double-click message specifically, not just that presses
        /// arrived: two ordinary presses read as two single clicks to a control, so a
        /// test that only counted presses would pass on an implementation that never
        /// delivered a double click at all.
        /// </remarks>
        [TestMethod]
        public void MessageDoubleClick_DeliversARealDoubleClickToTheTargetPoint()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            Assert.AreEqual(0, host.FacadeDoubleClickCount, "nothing has been clicked yet");

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.DoubleClick, allowPhysicalInput: false);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeDoubleClickCount == 1),
                "the facade had to receive one double click; got " + host.FacadeDoubleClickCount);
            Assert.AreEqual(host.FacadeWidth / 2, host.LastClickClientX, "the press landed off the requested point");
            Assert.AreEqual(host.FacadeHeight / 2, host.LastClickClientY, "the press landed off the requested point");
        }

        /// <summary>
        /// While the mouse can still reach the target it keeps the double click, so
        /// every unlocked run behaves exactly as it does today.
        /// </summary>
        [TestMethod]
        public void MessageDoubleClick_LeavesItToTheMouseWhileThatIsStillUsable()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.DoubleClick, allowPhysicalInput: true);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, result.Message);
            Assert.AreEqual(0, host.FacadeDoubleClickCount, "the message path must not double click behind the mouse's back");
        }

        /// <summary>
        /// A point that no descendant owns must be refused rather than delivered
        /// somewhere else.
        /// </summary>
        /// <remarks>
        /// A window answers a message carrying a point it does not own as handled
        /// while acting on the wrong place, or on nothing - so a double click aimed
        /// outside the control would be reported as delivered.
        /// </remarks>
        [TestMethod]
        public void MessageDoubleClick_RefusesATargetPointOutsideTheControl()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.DoubleClick, allowPhysicalInput: false);
            context.TargetScreenX = host.FacadeTopLeft.X + host.FacadeWidth + 50;
            context.TargetScreenY = host.FacadeTopLeft.Y + host.FacadeHeight + 50;

            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, result.Message);
            Assert.AreEqual(0, host.FacadeDoubleClickCount, "a point outside the control must not be double clicked");

            // A refusal that only says no cannot be acted on. A locked overnight run
            // produced exactly this and left no way to tell a target that had moved
            // from one measured at a different scaling, so the window's real position
            // has to travel with the refusal.
            StringAssert.Contains(result.Message, "on screen at", result.Message);
            StringAssert.Contains(result.Message, "dpi", result.Message);
            StringAssert.Contains(result.Message, context.TargetScreenX + "," + context.TargetScreenY, result.Message);
            StringAssert.Contains(result.Message, "is not inside the window", result.Message);
        }

        /// <summary>
        /// A right click has to arrive as a right click, which is the only way a
        /// context menu opens behind a lock screen.
        /// </summary>
        /// <remarks>
        /// Asserts the right-button message rather than that some press arrived: a
        /// left click reaches the same control at the same point and opens nothing, so
        /// a test counting presses alone would pass on an implementation that sent the
        /// wrong button.
        /// </remarks>
        [TestMethod]
        public void MessageRightClick_DeliversARightClickRatherThanSomeOtherButton()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.RightClick, allowPhysicalInput: false);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeRightClickCount == 1),
                "the facade had to receive one right click; got " + host.FacadeRightClickCount);
            Assert.AreEqual(0, host.FacadeClickCount, "a right click must not arrive as a left one");
        }

        /// <summary>
        /// While the mouse can still reach the target it keeps the right click, so
        /// every unlocked default run behaves exactly as it does today.
        /// </summary>
        [TestMethod]
        public void MessageRightClick_LeavesItToTheMouseWhileThatIsStillUsable()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.RightClick, allowPhysicalInput: true);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, result.Message);
            Assert.AreEqual(0, host.FacadeRightClickCount, "the message path must not right click behind the mouse's back");
        }

        /// <summary>
        /// A right click aimed outside the control has to be refused rather than
        /// reported, for the same reason the other buttons are.
        /// </summary>
        [TestMethod]
        public void MessageRightClick_RefusesATargetPointOutsideTheControl()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.RightClick, allowPhysicalInput: false);
            context.TargetScreenX = host.FacadeTopLeft.X + host.FacadeWidth + 50;
            context.TargetScreenY = host.FacadeTopLeft.Y + host.FacadeHeight + 50;

            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, result.Message);
            Assert.AreEqual(0, host.FacadeRightClickCount, "a point outside the control must not be right clicked");
        }

        /// <summary>
        /// Extending a selection means a click that says Ctrl was held, and the
        /// modifier has to reach the control for it to add rather than replace.
        /// </summary>
        /// <remarks>
        /// The alternative was holding the real Ctrl key around the click, which sets
        /// it for the whole machine and stays down if the click in between throws.
        /// Carrying it in the message is what makes a multi-select step safe to run on
        /// a shared desktop - and the modifier arriving is the whole of that, so it is
        /// what gets asserted rather than merely that a click landed.
        /// </remarks>
        [TestMethod]
        public void MessageControlClick_CarriesTheModifierInTheMessageRatherThanOnTheKeyboard()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.ControlClick, allowPhysicalInput: false);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeClickCount == 1),
                "the click has to land; got " + host.FacadeClickCount);
            Assert.IsTrue(host.FacadeLastClickHadControl,
                "without the modifier the control replaces its selection instead of extending it");
        }

        /// <summary>
        /// An ordinary click must not carry Ctrl, or every single-select step would
        /// start extending the selection instead of setting it.
        /// </summary>
        [TestMethod]
        public void MessageClick_DoesNotCarryTheModifierAnOrdinaryClickNeverHad()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            DesktopActionContext context = host.CreateFacadePointContext(DesktopOperation.Click, allowPhysicalInput: false);
            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
            Assert.IsTrue(host.PumpUntil(() => host.FacadeClickCount == 1));
            Assert.IsFalse(host.FacadeLastClickHadControl, "a plain click has to arrive plain");
        }

        /// <summary>
        /// The case the agent setting exists for: an unlocked desktop where the run was
        /// asked to keep off the mouse anyway.
        /// </summary>
        /// <remarks>
        /// Every other gate test here pairs "the mouse is unusable" with "use
        /// messages", so an implementation that only ever read the desktop state would
        /// pass all of them and ignore the setting completely - which is what the
        /// ordinary click did until this was added. Here the desktop says the mouse
        /// works and the setting says not to use it, and the messages still have to go
        /// out: that combination is the whole feature.
        /// </remarks>
        [TestMethod]
        public void Win32_HonoursTheQuietSettingOnADesktopThatCouldStillTakeTheMouse()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            foreach (DesktopOperation operation in new[]
            {
                DesktopOperation.Click,
                DesktopOperation.DoubleClick,
                DesktopOperation.RightClick,
                DesktopOperation.ControlClick
            })
            {
                DesktopActionContext context = host.CreateFacadePointContext(operation, allowPhysicalInput: false);
                context.DesktopCanTakePhysicalInput = true;
                context.PreferWindowMessages = true;

                int leftBefore = host.FacadeClickCount;
                int doubleBefore = host.FacadeDoubleClickCount;
                int rightBefore = host.FacadeRightClickCount;

                LayerResult result = new Win32Layer().TryExecute(context);

                Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status,
                    operation + " was left to the mouse despite the agent asking for the quiet route: " + result.Message);

                // Counted as a change rather than a total, since one operation is
                // several messages and the exact number is the transport's business.
                Assert.IsTrue(
                    host.PumpUntil(() => host.FacadeClickCount > leftBefore
                        || host.FacadeDoubleClickCount > doubleBefore
                        || host.FacadeRightClickCount > rightBefore),
                    operation + " was reported as sent but never reached the control");
            }
        }

        /// <summary>
        /// The other half of that setting: left off, an unlocked run keeps the mouse
        /// exactly as it always did.
        /// </summary>
        [TestMethod]
        public void Win32_StandsAsideOnAnUnlockedDesktopWhenTheQuietSettingIsOff()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            foreach (DesktopOperation operation in new[]
            {
                DesktopOperation.Click,
                DesktopOperation.DoubleClick,
                DesktopOperation.RightClick,
                DesktopOperation.ControlClick
            })
            {
                DesktopActionContext context = host.CreateFacadePointContext(operation, allowPhysicalInput: false);
                context.DesktopCanTakePhysicalInput = true;
                context.PreferWindowMessages = false;

                LayerResult result = new Win32Layer().TryExecute(context);

                Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, operation + " was not left to the mouse");
            }

            Assert.AreEqual(0, host.FacadeClickCount, "nothing may be delivered while the mouse is still the route");
            Assert.AreEqual(0, host.FacadeDoubleClickCount);
            Assert.AreEqual(0, host.FacadeRightClickCount);
        }

        /// <summary>
        /// The locked-run failure itself: a coordinate click on a button has to come
        /// back promptly instead of running to its timeout.
        /// </summary>
        /// <remarks>
        /// A button answers a press by capturing the mouse and running its own loop
        /// until the release arrives. Delivered in line, the press therefore blocked on
        /// a release that could not be sent until the blocking call returned, and the
        /// three failed double clicks each burned 9 to 18 seconds against a 5 second
        /// per-message timeout before being reported - as a point outside a window
        /// whose own bounds contained it. The timing is the assertion: a refusal here
        /// is acceptable, a stall is the bug.
        /// </remarks>
        [TestMethod]
        public void PointClick_OnAButtonReturnsPromptlyInsteadOfBlockingOnItsOwnCaptureLoop()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            DesktopActionContext context = host.CreateButtonPointContext(DesktopOperation.DoubleClick);

            Stopwatch clock = Stopwatch.StartNew();
            LayerResult result = new Win32Layer().TryExecute(context);
            clock.Stop();

            Assert.IsTrue(clock.ElapsedMilliseconds < context.TimeoutMs,
                "a coordinate click on a button took " + clock.ElapsedMilliseconds
                + "ms against a " + context.TimeoutMs + "ms timeout, so it is still blocking on the capture loop");
            Assert.AreEqual(LayerExecutionStatus.Succeeded, result.Status, result.Message);
        }

        /// <summary>
        /// A coordinate click that got nowhere must never be worded as one that
        /// worked.
        /// </summary>
        /// <remarks>
        /// The drivers decide pass or fail by looking for "Clicked Successfully" in
        /// the status string, so a refusal that happened to contain that phrase would
        /// be read as a pass. That is the silent success this whole path exists to
        /// remove, and it would return the moment the refusal was reworded to
        /// something like "not clicked successfully".
        /// </remarks>
        [TestMethod]
        public void AnUnreachablePointIsNeverWordedAsASuccessfulClick()
        {
            DesktopEngineResult refused = new DesktopEngineResult
            {
                Success = false,
                ErrorMessage = "no layer could reach it",
                ExecutionInfo = "Win32 [Skipped]: the point is not inside the window it was aimed at"
            };

            foreach (DesktopOperation operation in new[] { DesktopOperation.Click, DesktopOperation.DoubleClick })
            {
                string described = DesktopActionMapper.DescribeUnreachablePoint(operation, 188, 325, refused);

                Assert.IsFalse(described.Contains("Clicked Successfully"),
                    "the drivers would read this refusal as a pass: " + described);
                StringAssert.Contains(described, "188,325", described);
            }
        }

        /// <summary>
        /// A click the application never took must not be reported as a point the
        /// window does not own.
        /// </summary>
        /// <remarks>
        /// These are opposite problems - a bad locator against a busy application - and
        /// reporting both the same way produced a refusal contradicting the bounds
        /// printed next to it, which cost a full investigation.
        /// </remarks>
        [TestMethod]
        public void PointClick_TellsAnUnacknowledgedClickApartFromAPointTheWindowDoesNotOwn()
        {
            using UnresponsiveWindow deaf = new UnresponsiveWindow();
            Assert.AreNotEqual(IntPtr.Zero, deaf.Handle, "the stand-in window was not created");

            DesktopActionContext context = new DesktopActionContext
            {
                Operation = DesktopOperation.Click,
                NativeWindowHandle = deaf.Handle,
                PointOwnerWindowHandle = deaf.Handle,
                HasTargetPoint = true,
                TargetScreenX = deaf.CentreScreenX,
                TargetScreenY = deaf.CentreScreenY,
                AllowPhysicalInput = false,
                TimeoutMs = 300
            };

            LayerResult result = new Win32Layer().TryExecute(context);

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status, result.Message);
            StringAssert.Contains(result.Message, "did not take the click", result.Message);
            Assert.IsFalse(result.Message.Contains("is not inside the window"),
                "this window owns the point, so blaming the geometry sends the reader the wrong way: " + result.Message);
        }

        /// <summary>
        /// A target point only helps the clicks, so the layer must not claim the value
        /// operations on the strength of one.
        /// </summary>
        /// <remarks>
        /// Reading and writing a value both go through the target's own handle, which
        /// a point-only context does not have. Claiming them anyway put a technique in
        /// the attempt log that was never going to be tried, and that log is now part
        /// of the message an operator reads on a failure.
        /// </remarks>
        [TestMethod]
        public void Win32Layer_ClaimsAPointOnlyTargetForClicksAndNothingElse()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            Win32Layer layer = new Win32Layer();

            foreach (DesktopOperation clickable in new[] { DesktopOperation.Click, DesktopOperation.DoubleClick })
            {
                DesktopActionContext context = host.CreateFacadePointContext(clickable, allowPhysicalInput: false);
                context.NativeWindowHandle = IntPtr.Zero;

                Assert.IsTrue(layer.CanHandle(context), clickable + " is reachable by aiming at the point");
            }

            foreach (DesktopOperation valued in new[] { DesktopOperation.SetValue, DesktopOperation.GetValue })
            {
                DesktopActionContext context = host.CreateFacadePointContext(valued, allowPhysicalInput: false);
                context.NativeWindowHandle = IntPtr.Zero;

                Assert.IsFalse(layer.CanHandle(context), valued + " needs the target's own handle, which this context has none of");
            }
        }

        /// <summary>
        /// The session lock is what decides whether physical input is worth
        /// attempting, so the two must never disagree - in either direction.
        /// </summary>
        /// <remarks>
        /// Asserted both ways round on purpose. Reporting "available" on a locked
        /// session leaves every quiet window-message path switched off for exactly
        /// the runs that need it, and reporting "unavailable" on an ordinary desktop
        /// would divert clicks and keystrokes away from the mouse and keyboard that
        /// were working perfectly well. This deliberately does not assume the agent
        /// is unlocked, so locking the machine cannot fail the suite on its own.
        /// </remarks>
        /// <summary>
        /// Ends a test as inconclusive where Windows itself will not report the
        /// session lock, which is a fact about the machine rather than about this
        /// code.
        /// </summary>
        /// <remarks>
        /// The driver treats an unreadable lock as unlocked and carries on, so nothing
        /// is blocked by it - the probe fails open by design. These tests used to fail
        /// closed on the same condition, so a suite run where terminal services cannot
        /// answer - a trimmed server installation, a container, a process in session
        /// zero - failed indistinguishably from a build whose struct offsets had
        /// broken. Only the second is a defect, and callers of this keep asserting it.
        /// </remarks>
        private static void SkipWhereWindowsWillNotReportTheSessionLock(string description)
        {
            if (description.Contains("lock state unavailable"))
            {
                Assert.Inconclusive("This machine will not report its session lock, so there is nothing to check here: " + description);
            }
        }

        [TestMethod]
        public void InteractiveDesktop_AgreesWithTheSessionLockInEitherDirection()
        {
            string description = InteractiveDesktop.DescribeSession();
            SkipWhereWindowsWillNotReportTheSessionLock(description);

            Match state = Regex.Match(description, @"session (?<state>locked|unlocked)");
            Assert.IsTrue(state.Success, description);

            bool locked = state.Groups["state"].Value == "locked";

            Assert.AreEqual(!locked, InteractiveDesktop.IsAvailable(), description);
            Assert.AreEqual(!locked, description.EndsWith("physical input available"), description);
        }

        /// <summary>
        /// The mouse is asked for again at the moment it is used, not once when the
        /// action started, so a screen locked part way through an action cannot be
        /// clicked into.
        /// </summary>
        /// <remarks>
        /// This is the one layer that cannot tell whether it worked: moving the cursor
        /// on a locked desktop throws nothing and changes nothing, so the old code
        /// returned a click the application never received. The gap is real rather
        /// than theoretical - the desktop is read when the action begins, and the
        /// quieter layers take their own time to decline before this one runs, which
        /// is ample room for someone to lock the machine.
        ///
        /// A null element stands in for the target because it is never reached on a
        /// locked machine, and on an unlocked one it fails on the first line of
        /// SendClick before the cursor is touched. So the check runs either way
        /// without moving the mouse of whoever is running the suite.
        /// </remarks>
        [TestMethod]
        public void PhysicalInput_AsksAgainForTheMouseRatherThanTrustingTheAnswerFromWhenTheActionStarted()
        {
            DesktopActionContext context = new DesktopActionContext
            {
                Operation = DesktopOperation.Click,
                AllowPhysicalInput = true,
                DesktopCanTakePhysicalInput = true,
                AutomationElement = null
            };

            LayerResult result = new PhysicalInputLayer().TryExecute(context);

            if (InteractiveDesktop.IsAvailable())
            {
                Assert.AreNotEqual(LayerExecutionStatus.Skipped, result.Status,
                    "The mouse still works, so the layer must not refuse the click: " + result.Message);
                return;
            }

            Assert.AreEqual(LayerExecutionStatus.Skipped, result.Status,
                "A locked screen was reported as a click rather than refused");
            StringAssert.Contains(result.Message, "locked",
                "A refusal has to say the screen was locked, or the run gives no clue why the click never happened");
        }

        /// <summary>
        /// A secure desktop still proves input cannot land, so that direction has to
        /// keep holding.
        /// </summary>
        /// <remarks>
        /// Only that direction. The converse was asserted here until a locked
        /// workstation was caught naming its input desktop Default, so a name of
        /// Default is no longer evidence of anything - whether the handle opens and
        /// what it is called both depend on the calling process's token, which is why
        /// the same locked machine answers differently from different processes. The
        /// session lock is the reading that decides, and it is asserted separately.
        /// </remarks>
        [TestMethod]
        public void InteractiveDesktop_NeverReportsInputAsUsableOnASecureDesktop()
        {
            string description = InteractiveDesktop.DescribeSession();

            Match name = Regex.Match(description, @"input desktop (?<name>[^,]+)");
            Assert.IsTrue(name.Success, description);

            string owner = name.Groups["name"].Value.Trim();
            bool ownedBySecureDesktop = owner.Equals("Winlogon", StringComparison.OrdinalIgnoreCase)
                || owner.Equals("Screen-saver", StringComparison.OrdinalIgnoreCase)
                || owner.StartsWith("not accessible", StringComparison.OrdinalIgnoreCase);

            if (ownedBySecureDesktop)
            {
                Assert.IsFalse(InteractiveDesktop.IsAvailable(), description);
            }
        }

        /// <summary>
        /// The session line is what turns "most of the overnight run failed" into a
        /// diagnosis, so every fact it exists to carry has to be present.
        /// </summary>
        /// <remarks>
        /// Each reading is asserted by shape rather than by value, because the line
        /// has to be just as complete on a locked machine as on a working one - and
        /// pinning the values meant locking the screen failed the suite by itself.
        /// </remarks>
        [TestMethod]
        public void DescribeSession_ReportsTheFactsThatExplainAnOvernightFailure()
        {
            string description = InteractiveDesktop.DescribeSession();

            // Recorded in the run output as well as asserted. Everything in this class
            // behaves differently depending on whether the machine was locked, so a
            // result read months later is only worth anything next to the state it ran
            // under.
            TestContext.WriteLine(description);

            StringAssert.Matches(description, new Regex(@"^Session \d+ \((console|remote)\)"), description);

            // The desktop that owns input, and the session lock next to it. Both are
            // reported because they disagree: a locked workstation has been seen
            // naming its input desktop Default.
            StringAssert.Matches(description, new Regex(@"input desktop [^,]+"), description);

            // Every wording is accepted here on purpose. This test is about the line
            // carrying the reading at all, on any machine; whether the reading itself
            // is a real answer is asserted where the offsets are.
            StringAssert.Matches(description, new Regex(@"session (locked|unlocked|lock state (unreadable|unavailable))"), description);

            // Resolution is reported because a session detached with tscon drops to
            // 1024x768 or 640x480, which silently breaks coordinate and image steps.
            StringAssert.Matches(description, new Regex(@"screen \d+x\d+"), description);

            // Whether anything held the foreground explains a step that could not be
            // given focus even on a desktop that was never locked.
            StringAssert.Matches(description, new Regex(@"foreground window (present|absent)"), description);

            StringAssert.Matches(description, new Regex(@"physical input (available|unavailable)$"), description);
        }

        /// <summary>
        /// The session flags are read out of a native buffer at fixed offsets, so this
        /// is what proves those offsets are right.
        /// </summary>
        /// <remarks>
        /// Asserted before the machine is let off, and deliberately so. A buffer read
        /// at the wrong offsets is reported as unreadable rather than unavailable, and
        /// that has to fail wherever it happens: it would otherwise be reported on
        /// every run while the lock went undetected and the machine looked like one
        /// nobody had locked. A machine that will not answer at all is a separate
        /// matter and is excused below.
        /// </remarks>
        [TestMethod]
        public void SessionLockState_IsActuallyReadRatherThanQuietlyMisread()
        {
            string description = InteractiveDesktop.DescribeSession();

            Assert.IsFalse(description.Contains("lock state unreadable"),
                "Windows answered but the buffer was not understood, which means the offsets are wrong: " + description);

            // Retired wording. It covered both causes at once, which is what made a
            // machine that could not answer look the same as offsets that had broken.
            Assert.IsFalse(description.Contains("lock state unknown"), description);

            SkipWhereWindowsWillNotReportTheSessionLock(description);
            StringAssert.Matches(description, new Regex(@"session (locked|unlocked)"), description);
        }

        /// <summary>
        /// The lock state is reported next to the desktop name because the two can
        /// disagree: a locked workstation has been seen naming its input desktop
        /// Default. Asserting both appear keeps them from being folded back together,
        /// which is what previously let a locked run be recorded as a user at the
        /// keyboard.
        /// </summary>
        [TestMethod]
        public void DescribeSession_ReportsTheSessionLockApartFromTheDesktopName()
        {
            string description = InteractiveDesktop.DescribeSession();
            SkipWhereWindowsWillNotReportTheSessionLock(description);

            StringAssert.Matches(description, new Regex(@"input desktop [^,]+, session (locked|unlocked)"), description);
        }

        /// <summary>
        /// A locked session cannot have usable physical input, so these two readings
        /// contradict each other by construction. The line used to be built from a
        /// second, independent probe, which meant it could report a state its caller
        /// was never given - and that reading is what the lock diagnosis rests on.
        /// </summary>
        [TestMethod]
        public void DescribeSession_NeverReportsALockedSessionAsUsable()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                string description = InteractiveDesktop.DescribeSession();
                bool locked = description.Contains("session locked");
                bool usable = description.EndsWith("physical input available");

                Assert.IsFalse(locked && usable, description);
            }
        }

        /// <summary>
        /// A diagnostic that throws would take down driver startup, which is a far
        /// worse outcome than having no diagnostic at all.
        /// </summary>
        [TestMethod]
        public void DescribeSession_AlwaysReturnsTextRatherThanThrowing()
        {
            for (int attempt = 0; attempt < 3; attempt++)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(InteractiveDesktop.DescribeSession()));
            }
        }

        /// <summary>
        /// A key that produces no character still has to move the caret, which is
        /// what says the key press itself arrived.
        /// </summary>
        /// <remarks>
        /// Without End the caret sits where the text was set, at the start, so the
        /// typed character would land in front of the existing text. Where it ends up
        /// is the evidence.
        /// </remarks>
        [TestMethod]
        public void SendNotation_DeliversANavigationKeyAsAKeyPressRatherThanAsText()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            Assert.IsTrue(Win32Native.SetControlText(host.EditHwnd, "ABC", 2000));

            Assert.IsTrue(Win32KeyMessages.TrySendNotation(host.EditHwnd, "{END}X", 2000, out string failure), failure);

            Assert.AreEqual("ABCX", Win32Native.GetControlText(host.EditHwnd, 2000),
                "End has to move the caret, otherwise the character lands at the front");
        }

        /// <summary>
        /// A named key has to carry the character it would have produced, or the
        /// control it reaches does nothing with it.
        /// </summary>
        /// <remarks>
        /// A real key press becomes a character in the application's own message
        /// loop, which is the step a sent message skips. Backspace shows it: the key
        /// alone leaves the text as it was, and only the character deletes.
        /// </remarks>
        [TestMethod]
        public void SendNotation_GivesANamedKeyTheCharacterTheMessageLoopWouldHaveMade()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();
            Assert.IsTrue(Win32Native.SetControlText(host.EditHwnd, "ABC", 2000));

            Assert.IsTrue(Win32KeyMessages.TrySendNotation(host.EditHwnd, "{END}{BACKSPACE}", 2000, out string failure), failure);

            Assert.AreEqual("AB", Win32Native.GetControlText(host.EditHwnd, 2000),
                "Backspace has to remove a character rather than arrive and do nothing");
        }

        /// <summary>
        /// The shorthands a Send Keys value is allowed to use.
        /// </summary>
        [TestMethod]
        public void SendNotation_ReadsTheShorthandsAValueMayUse()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            // A repeat count means that many presses, and braces around a single
            // character escape it, so this is three plus signs rather than the Shift
            // modifier three times.
            Assert.IsTrue(Win32KeyMessages.TrySendNotation(host.EditHwnd, "{+ 3}", 2000, out string failure), failure);
            Assert.AreEqual("+++", Win32Native.GetControlText(host.EditHwnd, 2000));
        }

        /// <summary>
        /// Punctuation that looks special but means nothing has to go through as
        /// itself.
        /// </summary>
        /// <remarks>
        /// The other half of refusing what cannot be delivered. Reading ordinary
        /// text as notation would fail everyday form filling on a locked screen,
        /// which is most of what these steps do.
        /// </remarks>
        [TestMethod]
        public void SendNotation_DoesNotMistakeOrdinaryPunctuationForNotation()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            const string typed = "user@example.com Ref #A&B/12.5";
            Assert.IsTrue(Win32KeyMessages.TrySendNotation(host.EditHwnd, typed, 2000, out string failure), failure);

            Assert.AreEqual(typed, Win32Native.GetControlText(host.EditHwnd, 2000));
        }

        /// <summary>
        /// Case arrives as it was written, whatever the keyboard's lock keys are
        /// doing.
        /// </summary>
        /// <remarks>
        /// The keyboard route builds an upper case letter by holding Shift, so with
        /// Caps Lock engaged the two cancel and "CoreTeam" is typed as "cOREtEAM". A
        /// character sent as a message carries its own codepoint and never consults
        /// the keyboard, which is why this route cannot reproduce that fault - and
        /// why a locked run and an unlocked one can disagree about case.
        /// </remarks>
        [TestMethod]
        public void SendNotation_PreservesCaseWhateverTheKeyboardLockKeysAreDoing()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            const string mixedCase = "CoreTeam";
            Assert.IsTrue(Win32KeyMessages.TrySendNotation(host.EditHwnd, mixedCase, 2000, out string failure), failure);

            string actual = Win32Native.GetControlText(host.EditHwnd, 2000);
            Assert.AreEqual(mixedCase, actual, "Case has to survive the trip, not be rebuilt from Shift");
            Assert.AreNotEqual("cOREtEAM", actual, "That is the keyboard route's Caps Lock fault, not this one's");
        }

        /// <summary>
        /// A value this cannot carry has to be refused rather than dropped or typed.
        /// </summary>
        /// <remarks>
        /// A window message leaves the target thread's key state alone, so a control
        /// asking whether Ctrl is held is told it is not and Ctrl+A arrives as a
        /// plain A. Delivering that would run a different step from the one that was
        /// written, and typing the value verbatim would put "^a" in the field and
        /// call it a pass. Malformed and unknown are refused for the same reason.
        /// </remarks>
        [TestMethod]
        public void SendNotation_RefusesWhatItCannotDeliverRatherThanApproximatingIt()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            foreach (string value in new[] { "^a", "+{TAB}", "%{F4}", "^(ab)", "{ENTER", "{NOTAKEY}", "abc}" })
            {
                Assert.IsFalse(Win32KeyMessages.TrySendNotation(host.EditHwnd, value, 2000, out string refusal, out bool partiallyDelivered),
                    "'" + value + "' cannot be delivered as window messages");
                Assert.IsFalse(string.IsNullOrWhiteSpace(refusal), "A refusal has to say what got in the way");
                Assert.IsFalse(partiallyDelivered,
                    "Nothing was sent, so the keyboard is still free to carry the whole value");
            }

            Assert.AreEqual(string.Empty, Win32Native.GetControlText(host.EditHwnd, 2000),
                "A refused value must not leave part of itself behind");
        }

        /// <summary>
        /// Keys aimed at no window have to be reported, not swallowed.
        /// </summary>
        /// <remarks>
        /// This is the case that made a locked run pass while typing nothing: the
        /// keyboard route focused an element that could not be focused and typed at a
        /// desktop that was not listening, and neither half said so.
        /// </remarks>
        [TestMethod]
        public void SendNotation_SaysWhyRatherThanReportingAnEmptySuccess()
        {
            Assert.IsFalse(Win32KeyMessages.TrySendNotation(IntPtr.Zero, "Pune", 2000, out string failure));
            StringAssert.Contains(failure, "no window");
        }

        /// <summary>
        /// A window that took part of a value before it stopped answering has to say
        /// so, not merely that it failed.
        /// </summary>
        /// <remarks>
        /// Every caller keeps the keyboard behind this route, and the keyboard types
        /// the value it was given rather than the part still missing. Reported as a
        /// plain failure, an application that went busy after "ab" of "abc" left the
        /// field holding "ababc", and a value ending in {ENTER} or {TAB} pressed that
        /// key a second time. The flag is what lets the caller fail the step instead.
        /// </remarks>
        [TestMethod]
        public void SendNotation_ReportsThatPartOfTheValueArrivedBeforeTheWindowStopped()
        {
            using StallsPartwayWindow window = new StallsPartwayWindow(acceptBeforeStalling: 2);

            Assert.IsFalse(Win32KeyMessages.TrySendNotation(window.Handle, "abc", 200, out string failure, out bool partiallyDelivered));
            Assert.IsTrue(partiallyDelivered, "Two characters were taken, so the window is holding 'ab'");
            Assert.IsFalse(string.IsNullOrWhiteSpace(failure), "A failure has to say what got in the way");
        }

        /// <summary>
        /// One key press is several messages, and the first of them arriving is
        /// already enough to leave the control changed.
        /// </summary>
        /// <remarks>
        /// Counting whole keystrokes instead misses exactly this. A value opening
        /// with {ENTER} whose key-down landed and whose character did not has still
        /// been acted on by a control that reads keys itself - a DataWindow commits
        /// on the key - and the keyboard would then press Enter a second time.
        /// </remarks>
        [TestMethod]
        public void SendNotation_CountsAKeyPressThatArrivedWithoutItsCharacter()
        {
            using StallsPartwayWindow window = new StallsPartwayWindow(acceptBeforeStalling: 0);

            Assert.IsFalse(Win32KeyMessages.TrySendNotation(window.Handle, "{ENTER}", 200, out _, out bool partiallyDelivered));
            Assert.IsTrue(partiallyDelivered,
                "The key went down before the character was refused, so the press itself arrived");
        }

        /// <summary>
        /// A window that took nothing must not be reported as holding part of the
        /// value.
        /// </summary>
        /// <remarks>
        /// The other half of the same decision, and the more costly one to get wrong:
        /// this is the ordinary failure the keyboard exists to pick up, so calling it
        /// a partial delivery would fail steps that the fallback would have carried.
        /// </remarks>
        [TestMethod]
        public void SendNotation_DoesNotCallAnUndeliveredValueAPartialOne()
        {
            using UnresponsiveWindow window = new UnresponsiveWindow();

            Assert.IsFalse(Win32KeyMessages.TrySendNotation(window.Handle, "abc", 200, out _, out bool partiallyDelivered));
            Assert.IsFalse(partiallyDelivered, "Nothing arrived, so the whole value can still be sent another way");
        }
    }
}
