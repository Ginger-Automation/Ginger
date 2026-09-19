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
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace GingerCoreTest.Misc
{
    [TestClass]
    public class PbLikeDesktopAutomationEvidenceTest
    {
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

        [TestMethod]
        public void Mapper_KeepsPhysicalInputAsLastResort_OnAUsableDesktop()
        {
            using PbLikeDesktopHost host = new PbLikeDesktopHost();

            // Regression: a grid cell with no Invoke pattern, no accessible default
            // action and no window handle can only be clicked with the mouse.
            // Clamping it away made every such Table action fail.
            DesktopActionContext context = DesktopActionMapper.FromElement(
                host.FacadeElement, DesktopOperation.Click, null, allowPhysicalInput: true);

            Assert.IsTrue(context.AllowPhysicalInput,
                "A usable desktop must keep the only way to reach such a control");
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

        [TestMethod]
        public void InteractiveDesktop_ReportsAvailable_OnAnUnlockedTestSession()
        {
            // The suite runs on an unlocked interactive desktop, so the probe must
            // say so. This also pins the fail-open contract: a probe that cannot
            // read the desktop state must never report "unavailable".
            Assert.IsTrue(InteractiveDesktop.IsAvailable());
        }

        /// <summary>
        /// The session line is what turns "most of the overnight run failed" into a
        /// diagnosis, so it has to carry the facts that distinguish a locked machine
        /// from a working one, and it has to produce them on a normal desktop too.
        /// </summary>
        [TestMethod]
        public void DescribeSession_ReportsTheFactsThatExplainAnOvernightFailure()
        {
            string description = InteractiveDesktop.DescribeSession();

            // Input desktop is the single clearest signal: Default while someone is
            // working, Winlogon once the workstation locks.
            StringAssert.Contains(description, "input desktop Default");

            // Resolution is reported because a session detached with tscon drops to
            // 1024x768 or 640x480, which silently breaks coordinate and image steps.
            StringAssert.Matches(description, new Regex(@"screen \d+x\d+"));

            StringAssert.Contains(description, "Session ");
            StringAssert.Contains(description, "physical input available");
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
