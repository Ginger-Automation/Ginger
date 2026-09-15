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

namespace GingerCoreTest.Misc
{
    [TestClass]
    public class PbLikeDesktopAutomationEvidenceTest
    {
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
    }
}
