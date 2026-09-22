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

            DesktopActionContext clickContext = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: true);
            clickContext.NativeWindowHandle = host.ButtonHwnd;
            clickContext.TimeoutMs = 2000;

            DesktopEngineResult clickResult = engine.Execute(clickContext);
            Assert.IsTrue(clickResult.Success, "Click should succeed via the layered engine");
            Assert.AreNotEqual("PhysicalInput", clickResult.UsedLayer);
            StringAssert.Contains(clickResult.ExecutionInfo, "BM_CLICK");

            Assert.AreEqual(foregroundBefore, GetForegroundWindow(),
                "The layered engine must not steal foreground focus");
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
        /// On a locked screen Send Keys types straight into the window instead of using
        /// the keyboard. What arrives there is a character rather than a keystroke, so
        /// a value carrying a modifier or a named key has to be refused: typed verbatim
        /// it would put "{ENTER}" in the field and report a pass, which is the silent
        /// failure this whole path exists to remove.
        /// </summary>
        [TestMethod]
        public void SendKeys_RefusesNotationItCannotTypeOnALockedScreen()
        {
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("{ENTER}"));
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("account-99{TAB}"));
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("^a"));
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("%{F4}"));
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("+abc"));
            Assert.IsTrue(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("~"));
        }

        /// <summary>
        /// The other half of the same decision: plain text must still go through, or a
        /// locked screen would fail every Send Keys step rather than only the ones it
        /// genuinely cannot serve.
        /// </summary>
        [TestMethod]
        public void SendKeys_TypesPlainTextOnALockedScreenRatherThanFailingIt()
        {
            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("account-99"));
            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("2026-09-18"));

            // Punctuation that looks special but means nothing to Send Keys. Reading
            // it as notation would fail ordinary form filling on a locked screen.
            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("user@example.com"));
            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation("Ref #A&B/12.5"));

            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation(""));
            Assert.IsFalse(Ginger.Actions.ActSendKeys.UsesSendKeysNotation(null));
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

            // Built only where the mouse has already been ruled out, so it states the
            // answer instead of probing again and having a late unlock withdraw the
            // only route the click has left.
            DesktopActionContext forPoint = DesktopActionMapper.ForPoint(
                host.FacadeElement, DesktopOperation.Click, 10, 10);

            Assert.IsFalse(forPoint.DesktopCanTakePhysicalInput);
            Assert.IsFalse(forPoint.AllowPhysicalInput);
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
    }
}
