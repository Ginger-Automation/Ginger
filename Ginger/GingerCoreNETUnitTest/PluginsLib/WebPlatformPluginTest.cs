using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace GingerCoreNETUnitTest.PluginsLib
{
    // Generic platform plugin tester
    [Ignore]  // getting stuck
    [TestClass]
    [Level3]
    public class WebPlatformPluginTest
    {
        
        static GingerGrid GG;
        static Agent agent;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {            
            // Init workspace
            WorkspaceHelper.CreateDummyWorkSpace(nameof(WebPlatformPluginTest));            

            // Strat GG
            // GG = new GingerGrid(15001);  // Get free port !!!!!!!!!

            // start the plugin to register in LocalGrid !!!!!! I ran it on the 2nd visual studio while running the test and the next loop wait for it

            // Start Agent
            GingerGrid GG = WorkSpace.Instance.LocalGingerGrid;
            while (GG.NodeList.Count == 0)
            {
                Thread.Sleep(100);
            }
            agent = new Agent();
            agent.AgentType = Agent.eAgentType.Service;
            agent.PluginId = "SeleniumPlugin";
            agent.ServiceId = "SeleniumChromeService";
            agent.StartDriver();

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            WorkSpace.Instance.ReleaseWorkspace();
        }


        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

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
        public void SetTextBoxText()
        {
            // Arrange
            // ActUIElement actUIElement  // Until we will have ActUIElement in GingerCoreNEt we create a fake actions
            ActBrowserElement actBrowserElement = new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, Value = "http://www.facebook.com" };
            ActUIElement actUIElementFake = new ActUIElement() { LocateBy =  Amdocs.Ginger.Common.UIElement.eLocateBy.ByID, LocateValue = "u_0_c", ElementType =  Amdocs.Ginger.Common.UIElement.eElementType.TextBox, ElementAction =  ActUIElement.eElementAction.SetText , Value = "hello"};            

            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);

            for (int i = 0; i < 10; i++)
            {
                actUIElementFake.Value = "#" + i;
                ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actUIElementFake);
            }

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.Error));
            
        }

        [TestMethod]
        public void SetGetTextBoxText()
        {
            // Arrange
            // ActUIElement actUIElement  // Until we will have ActUIElement in GingerCoreNEt we create a fake actions
            ActBrowserElement actBrowserElement = new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, Value = "http://www.facebook.com" };
            ActUIElement actUIElement = new ActUIElement() { LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID, LocateValue = "u_0_c", ElementType = Amdocs.Ginger.Common.UIElement.eElementType.TextBox, ElementAction = ActUIElement.eElementAction.SetText, Value = "hello" };
            ActUIElement actUIElement2 = new ActUIElement() { LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID, LocateValue = "u_0_c", ElementType = Amdocs.Ginger.Common.UIElement.eElementType.TextBox, ElementAction = ActUIElement.eElementAction.GetText };
            

            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElement);

            
            actUIElement.Value = "12345";
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actUIElement);
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actUIElement2);


            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElement.Error));
            Assert.AreEqual("Value", actUIElement2.ReturnValues[0].Param);
            Assert.AreEqual("hello", actUIElement2.ReturnValues[0].Actual);

        }


    }
}
