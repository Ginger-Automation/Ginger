using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Run;
using GingerCore;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTest.RunTestslib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GingerCoreNETUnitTest.PluginsLib
{
    // Generic platform plugin tester

    [TestClass]
    public class WebPlatformPluginTest
    {
        static GingerGrid GG;
        static Agent agent;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            // Init workspace
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws);

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
            ActBrowserElementFake actBrowserElementFake = new ActBrowserElementFake() { BrowserAction = "Navigate", value = "http://www.facebook.com" };            

            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElementFake);

            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElementFake.Error));
        }


        [TestMethod]
        public void SetTextBoxText()
        {
            // Arrange
            ActBrowserElementFake actBrowserElementFake = new ActBrowserElementFake() { BrowserAction = "Navigate", value = "http://www.facebook.com" };
            // ActUIElement actUIElement  // Until we will have ActUIElement in GingerCoreNEt we create a fake action
            ActUIElementFake actUIElementFake = new ActUIElementFake() { LocateBy = "ByID", LocateValue = "u_0_c", ElementType = "TextBox", ElementAction = "SetText" , Value = "hello"};

            // Act
            ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actBrowserElementFake);
            for (int i = 0; i < 1000; i++)
            {
                actUIElementFake.Value = "#" + i;
                ExecuteOnPlugin.ExecutePlugInActionOnAgent(agent, actUIElementFake);
            }


            // Assert
            Assert.IsTrue(string.IsNullOrEmpty(actBrowserElementFake.Error));
            
        }


    }
}
