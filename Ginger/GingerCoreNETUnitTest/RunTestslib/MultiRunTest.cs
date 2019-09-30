using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.PlugIns;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.DriversLib;
using GingerCoreNET.RunLib;
using GingerCoreNETUnitTests.RunTestslib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [TestClass]
    public class MultiRunTest
    {
        static GingerGrid GG;        


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            // We start a Ginger grid on open port
            int GingerHubPort = SocketHelper.GetOpenPort();
            GG = new GingerGrid(GingerHubPort);
            GG.Start();

            // Add 2 Ginger Nodes with Dummy Driver
            DummyDriver DummyDriver1 = new DummyDriver();            
            GingerNode GN = new GingerNode(DummyDriver1);
            GN.StartGingerNode("N1", HubIP: SocketHelper.GetLocalHostIP(), HubPort: GingerHubPort);

            DummyDriver DummyDriver2 = new DummyDriver();            
            GingerNode GingerNode2 = new GingerNode(DummyDriver2);
            GingerNode2.StartGingerNode("N2", HubIP: SocketHelper.GetLocalHostIP(), HubPort: GingerHubPort);
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            GG.Stop();
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
        [Timeout(60000)]
        public void ParallelRunTest()
        {
            //Arrange            
            ActPlugIn a1 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn a2 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn a3 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn a4 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn a5 = new ActPlugIn() { ActionId = "A1" };

            ActPlugIn b1 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn b2 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn b3 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn b4 = new ActPlugIn() { ActionId = "A1" };
            ActPlugIn b5 = new ActPlugIn() { ActionId = "A1" };


            // Act

            // We run on 2 drivers in parralel
            Task t1 = Task.Factory.StartNew(() =>
            {
                GingerNodeInfo GNI1 = GG.NodeList[0];
                GingerNodeProxy GNP1 = new GingerNodeProxy(GNI1);
                GNP1.GingerGrid = GG;

                GNP1.Reserve();
                GNP1.StartDriver();

                RunAction(a1, GNP1);
                RunAction(a2, GNP1);
                RunAction(a3, GNP1);
                RunAction(a4, GNP1);
                RunAction(a5, GNP1);


                GNP1.CloseDriver();
            });

            Task t2 = Task.Factory.StartNew(() =>
            {
                GingerNodeInfo GNI2 = GG.NodeList[1];
                GingerNodeProxy GNP2 = new GingerNodeProxy(GNI2);
                GNP2.GingerGrid = GG;

                GNP2.Reserve();
                GNP2.StartDriver();

                RunAction(b1, GNP2);
                RunAction(b2, GNP2);
                RunAction(b3, GNP2);
                RunAction(b4, GNP2);
                RunAction(b5, GNP2);

                GNP2.CloseDriver();
            });

            t1.Wait();
            t2.Wait();


            // Assert
            Assert.AreEqual(a1.Error, null);
            Assert.AreEqual(a1.ExInfo, "A1 Result");

            Assert.AreEqual(a2.Error, null);
            Assert.AreEqual(a2.ExInfo, "A1 Result");

            Assert.AreEqual(a3.Error, null);
            Assert.AreEqual(a3.ExInfo, "A1 Result");

            Assert.AreEqual(a4.Error, null);
            Assert.AreEqual(a4.ExInfo, "A1 Result");

            Assert.AreEqual(a5.Error, null);
            Assert.AreEqual(a5.ExInfo, "A1 Result");

            Assert.AreEqual(b1.Error, null);
            Assert.AreEqual(b1.ExInfo, "A1 Result");

            Assert.AreEqual(b2.Error, null);
            Assert.AreEqual(b2.ExInfo, "A1 Result");

            Assert.AreEqual(b3.Error, null);
            Assert.AreEqual(b3.ExInfo, "A1 Result");

            Assert.AreEqual(b4.Error, null);
            Assert.AreEqual(b4.ExInfo, "A1 Result");

            Assert.AreEqual(b5.Error, null);
            Assert.AreEqual(b5.ExInfo, "A1 Result");


        }

        private void RunAction(ActPlugIn a1, GingerNodeProxy gNP1)
        {
            NewPayLoad a1Payload = ExecuteOnPlugin.CreateActionPayload(a1);
            NewPayLoad rc = gNP1.RunAction(a1Payload);
            ExecuteOnPlugin.ParseActionResult(rc, a1);
        }

       

    }
}
