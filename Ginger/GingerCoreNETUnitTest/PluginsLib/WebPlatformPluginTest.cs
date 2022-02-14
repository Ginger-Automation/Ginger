using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GingerCoreNETUnitTest.PluginsLib
{
    // Generic platform plugin tester
    
    [Ignore]  // Fail on Linux with permission denied, on Mac get stuck
    [TestClass]
    [Level3]
    public class WebPlatformPluginTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }
        
        static Agent agent;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);

            mTestHelper.Log("Creating temp solution");
            // Init workspace
            string solutionFolder = TestResources.GetTempFolder("WebPlatformPluginTest");
            WorkspaceHelper.CreateWorkspaceWithTempSolution(solutionFolder);


            // To debug SeleniumPlugin start the plugin from VS to register to LocalGrid             
            string pluginFolder = TestResources.GetTestResourcesFolder("Plugins" + Path.DirectorySeparatorChar + "SeleniumPlugin");
            WorkSpace.Instance.PlugInsManager.Init(WorkSpace.Instance.SolutionRepository);
            mTestHelper.Log("Adding SeleniumPlugin from: " + pluginFolder);
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(pluginFolder);

            // Start Agent
            
            agent = new Agent();
            agent.AgentType = Agent.eAgentType.Service;
            agent.PluginId = "SeleniumPlugin";
            agent.ServiceId = "SeleniumChromeService";
            mTestHelper.Log("StartDriver SeleniumPlugin SeleniumChromeService");
            agent.AgentOperations.StartDriver();

            GingerGrid GG = WorkSpace.Instance.LocalGingerGrid;
            Stopwatch stopwatch = Stopwatch.StartNew();
            mTestHelper.Log("Waiting for node to connect");
            while (GG.NodeList.Count == 0 && stopwatch.ElapsedMilliseconds < 10000)   // wait max 10 seconds
            {
                mTestHelper.Log("GG.NodeList.Count == 0");
                Thread.Sleep(100);
            }

            if (GG.NodeList.Count == 0)
            {
                throw new Exception ("GG.NodeList.Count == 0");
            }

            mTestHelper.Log("Done Waiting");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            agent.AgentOperations.Close();
            mTestHelper.ClassCleanup();
        }


        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }

        [TestMethod]        
        public void GotoURL()
        {
            // Arrange
            ActBrowserElement actBrowserElementFake = new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL,  Value = "http://www.facebook.com" };            

            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElementFake);

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElementFake.Error));
        }


        [TestMethod]
        public void SetTextBoxTextx10()
        {
            // Arrange
            // ActUIElement actUIElement  // Until we will have ActUIElement in GingerCoreNEt we create a fake actions
            ActBrowserElement actBrowserElement = new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, Value = "http://www.facebook.com" };            
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);

            // Act
            ActUIElement actUIElementFake = new ActUIElement() { ElementLocateBy = eLocateBy.ByID, ElementLocateValue = "u_0_e", ElementType = eElementType.TextBox, ElementAction = ActUIElement.eElementAction.SetText};
            for (int i = 0; i < 10; i++)
            {
                actUIElementFake.Value = "#" + i;
                ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actUIElementFake);
                Assert.IsTrue(string.IsNullOrEmpty(actUIElementFake.Error), "no Error #" +  i);
            }

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.Error));
            

        }

        [TestMethod]
        public void SetGetTextBoxText()
        {
            // Arrange            
            ActBrowserElement actBrowserElement = new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, Value = "http://www.facebook.com" };
            ActUIElement setTextBoxAction = new ActUIElement() { ElementLocateBy = eLocateBy.ByID, ElementLocateValue = "u_0_e", ElementType = eElementType.TextBox, ElementAction = ActUIElement.eElementAction.SetText, Value = "hello" };
            ActUIElement getTextBoxAction = new ActUIElement() { ElementLocateBy = eLocateBy.ByID, ElementLocateValue = "u_0_e", ElementType = eElementType.TextBox, ElementAction = ActUIElement.eElementAction.GetText };
            
            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);
            
            setTextBoxAction.Value = "12345";
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, setTextBoxAction);
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, getTextBoxAction);


            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.Error));
            Assert.IsTrue(string.IsNullOrEmpty(setTextBoxAction.Error));
            Assert.IsTrue(string.IsNullOrEmpty(getTextBoxAction.Error));
            Assert.AreEqual("Actual", getTextBoxAction.ReturnValues[0].Param);
            Assert.AreEqual("12345", getTextBoxAction.ReturnValues[0].Actual);

        }

      

    }
}
