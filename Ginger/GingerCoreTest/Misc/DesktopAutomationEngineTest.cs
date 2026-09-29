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

using GingerCore.Drivers.Common;
using GingerCore.Drivers.Common.LegacyAutomation;
using GingerCore.Drivers.PBDriver.DesktopAutomation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace GingerCoreTest.Misc
{
    [TestClass]
    public class DesktopAutomationEngineTest
    {
        private sealed class StubLayer : IDesktopAutomationLayer
        {
            private readonly bool mSucceed;
            private readonly string mOutput;

            public string Name { get; }
            public int Calls { get; private set; }

            public StubLayer(string name, bool succeed, string output = null)
            {
                Name = name;
                mSucceed = succeed;
                mOutput = output;
            }

            public bool CanHandle(DesktopActionContext context)
            {
                return true;
            }

            public LayerResult TryExecute(DesktopActionContext context)
            {
                Calls++;
                return mSucceed ? LayerResult.Ok(Name, Name + " ok", mOutput) : LayerResult.Fail(Name, Name + " failed");
            }
        }

        [TestMethod]
        public void Engine_StopsAtFirstLayerThatWorks()
        {
            StubLayer uia = new StubLayer("UIA", succeed: true);
            StubLayer win32 = new StubLayer("Win32", succeed: true);
            DesktopAutomationEngine engine = new DesktopAutomationEngine([uia, win32]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.Click });

            Assert.IsTrue(result.Success);
            Assert.AreEqual("UIA", result.UsedLayer);
            Assert.AreEqual(1, uia.Calls);
            Assert.AreEqual(0, win32.Calls);

            // The layer's own message is kept and the layer named once as its source.
            // Leading with the operation as well produced "Click via UIA-Pattern.
            // Click via InvokePattern", which reads as two clicks for one attempt.
            Assert.AreEqual("UIA ok (UIA layer)", result.ExecutionInfo);
        }

        [TestMethod]
        public void Engine_FallsBackToTheNextLayerWhenOneFails()
        {
            StubLayer uia = new StubLayer("UIA", succeed: false);
            StubLayer win32 = new StubLayer("Win32", succeed: true);
            DesktopAutomationEngine engine = new DesktopAutomationEngine([uia, win32]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.SetValue, Value = "abc" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Win32", result.UsedLayer);
            Assert.AreEqual(1, uia.Calls);
            Assert.AreEqual(1, win32.Calls);
        }

        [TestMethod]
        public void Engine_ReportsEveryAttemptWhenNothingWorks()
        {
            DesktopAutomationEngine engine = new DesktopAutomationEngine(
            [
                new StubLayer("UIA", succeed: false),
                new StubLayer("Win32", succeed: false)
            ]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.Click });

            Assert.IsFalse(result.Success);

            // The operator-facing message stays free of automation jargon...
            StringAssert.Contains(result.ErrorMessage, "Ginger could not click this element");
            Assert.IsFalse(result.ErrorMessage.Contains("[Failed]"),
                "The reported error must not expose the per-technique attempt log");

            // ...while the detail needed to diagnose it survives in ExInfo.
            StringAssert.Contains(result.ExecutionInfo, "UIA [Failed]");
            StringAssert.Contains(result.ExecutionInfo, "Win32 [Failed]");
        }

        /// <summary>
        /// The drivers used to read the value property and then fall through to the
        /// legacy property when it came back blank. The engine has to keep doing that.
        /// </summary>
        [TestMethod]
        public void Engine_PrefersARealValueOverAnEarlierBlankRead()
        {
            DesktopAutomationEngine engine = new DesktopAutomationEngine(
            [
                new StubLayer("UIA", succeed: true, output: string.Empty),
                new StubLayer("MSAA", succeed: true, output: "account number")
            ]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.GetValue });

            Assert.IsTrue(result.Success);
            Assert.AreEqual("MSAA", result.UsedLayer);
            Assert.AreEqual("account number", result.OutputValue);
        }

        [TestMethod]
        public void Engine_KeepsTheBlankReadWhenNoLayerHasText()
        {
            DesktopAutomationEngine engine = new DesktopAutomationEngine(
            [
                new StubLayer("UIA", succeed: true, output: string.Empty),
                new StubLayer("MSAA", succeed: false)
            ]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.GetValue });

            Assert.IsTrue(result.Success);
            Assert.AreEqual(string.Empty, result.OutputValue);
        }

        [TestMethod]
        public void DefaultEngine_OrdersLayersLeastIntrusiveFirst()
        {
            List<string> names = DesktopAutomationEngine.CreateDefault().Layers.Select(layer => layer.Name).ToList();

            CollectionAssert.AreEqual(new List<string> { "UIA-Pattern", "DataWindowMsaa", "MSAA", "Win32", "PhysicalInput" }, names);
        }

        [TestMethod]
        public void PhysicalInputLayer_OnlyRunsWhenTheCallerOptsIn()
        {
            PhysicalInputLayer layer = new PhysicalInputLayer();

            Assert.IsFalse(layer.CanHandle(new DesktopActionContext { Operation = DesktopOperation.Click, AllowPhysicalInput = false }));
            Assert.IsFalse(layer.CanHandle(new DesktopActionContext { Operation = DesktopOperation.SetValue, AllowPhysicalInput = true }));
        }

        [TestMethod]
        public void ToActionResult_CarriesTheFailureMessageAndTheAttemptLog()
        {
            ActionResult actionResult = DesktopActionMapper.ToActionResult(new DesktopEngineResult
            {
                Success = false,
                ErrorMessage = "Ginger could not click this element.",
                ExecutionInfo = "UIA [Skipped]: no pattern"
            });

            StringAssert.Contains(actionResult.errorMessage, "Ginger could not click this element");
            StringAssert.Contains(actionResult.executionInfo, "no pattern");
        }

        [TestMethod]
        public void Win32_WritesAndReadsBackChildControlText()
        {
            using Form form = new Form();
            form.CreateControl();
            TextBox textBox = new TextBox();
            form.Controls.Add(textBox);
            textBox.CreateControl();

            Assert.IsTrue(Win32Native.SetControlText(textBox.Handle, "message text", 1000));
            Assert.AreEqual("message text", Win32Native.GetControlText(textBox.Handle, 1000));
        }

        [TestMethod]
        public void Win32_IgnoresTopLevelWindowsSoCaptionIsNeverReadAsAValue()
        {
            using Form form = new Form { Text = "Window caption" };
            form.CreateControl();

            Assert.IsNull(Win32Native.GetControlText(form.Handle, 1000));
            Assert.IsFalse(Win32Native.SetControlText(form.Handle, "should not apply", 1000));
            Assert.AreEqual("Window caption", form.Text);
        }

        [TestMethod]
        public void Win32_RejectsControlsThatAreNotButtonsOrEdits()
        {
            using Form form = new Form();
            form.CreateControl();
            Label label = new Label();
            form.Controls.Add(label);
            label.CreateControl();

            Assert.IsFalse(Win32Native.ClickButton(label.Handle, 1000));
            Assert.IsFalse(Win32Native.SetControlText(label.Handle, "text", 1000));
            Assert.IsNull(Win32Native.GetControlText(label.Handle, 1000));
        }

        [TestMethod]
        public void Win32_ClicksOnlyExactButtonClass()
        {
            using Form form = new Form();
            form.CreateControl();
            Button button = new Button { Text = "OK" };
            ComboBox combo = new ComboBox();
            form.Controls.Add(button);
            form.Controls.Add(combo);
            button.CreateControl();
            combo.CreateControl();

            Assert.IsTrue(Win32Native.IsButtonClass(button.Handle));
            Assert.IsTrue(Win32Native.ClickButton(button.Handle, 1000));
            Assert.IsFalse(Win32Native.IsButtonClass(combo.Handle));
            Assert.IsFalse(Win32Native.ClickButton(combo.Handle, 1000));
        }

        [TestMethod]
        public void Win32_GetWindowStyleDoesNotThrowOnChildControls()
        {
            using Form form = new Form();
            form.CreateControl();
            TextBox textBox = new TextBox();
            form.Controls.Add(textBox);
            textBox.CreateControl();

            long style = Win32Native.GetWindowStyle(textBox.Handle);
            Assert.AreNotEqual(0, style);
            Assert.IsTrue(Win32Native.IsChildWindow(textBox.Handle));
            Assert.IsFalse(Win32Native.IsChildWindow(form.Handle));
        }

        [TestMethod]
        public void Win32_TreatsRichEditAsEditButNotAsButton()
        {
            using Form form = new Form();
            form.CreateControl();
            RichTextBox richText = new RichTextBox();
            form.Controls.Add(richText);
            richText.CreateControl();

            Assert.IsTrue(Win32Native.IsEditClass(richText.Handle));
            Assert.IsFalse(Win32Native.IsButtonClass(richText.Handle));
            Assert.IsTrue(Win32Native.SetControlText(richText.Handle, "notes", 1000));
            Assert.AreEqual("notes", Win32Native.GetControlText(richText.Handle, 1000));
        }

        [TestMethod]
        public void Engine_MsaaSetValueSkipFallsThroughToWin32()
        {
            SkipLayer msaa = new SkipLayer("MSAA", "LegacyIAccessible SetValue did not read back");
            StubLayer win32 = new StubLayer("Win32", succeed: true);
            DesktopAutomationEngine engine = new DesktopAutomationEngine([msaa, win32]);

            DesktopEngineResult result = engine.Execute(new DesktopActionContext { Operation = DesktopOperation.SetValue, Value = "abc" });

            Assert.IsTrue(result.Success);
            Assert.AreEqual("Win32", result.UsedLayer);
            Assert.AreEqual(1, msaa.Calls);
            Assert.AreEqual(1, win32.Calls);
        }

        private sealed class SkipLayer : IDesktopAutomationLayer
        {
            public string Name { get; }
            public int Calls { get; private set; }
            private readonly string mMessage;

            public SkipLayer(string name, string message)
            {
                Name = name;
                mMessage = message;
            }

            public bool CanHandle(DesktopActionContext context)
            {
                return true;
            }

            public LayerResult TryExecute(DesktopActionContext context)
            {
                Calls++;
                return LayerResult.Skip(mMessage);
            }
        }
    }
}
