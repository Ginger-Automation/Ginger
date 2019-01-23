#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Diagnostics;
using System.Threading;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCore.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTests.NonUITests
{
    [TestClass]

  //  [Level1]
    public class GingerSocketTest 
    {

        static GingerSocketClient mGingerSocketClient;
        static GingerSocketServer mGingerSocketServer;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            mGingerSocketClient = new GingerSocketClient();

            mGingerSocketServer = new GingerSocketServer();
            mGingerSocketServer.Message += mGingerSocketServer_Message;

            int ServerPort = SocketHelper.GetOpenPort();
            mGingerSocketServer.StratServer(ServerPort);
            mGingerSocketClient.Connect(SocketHelper.GetLocalHostIP(), ServerPort);
        }

        [ClassCleanup()]
        public static void ClassCleanup() { 
            mGingerSocketClient.CloseConnection();
            mGingerSocketServer.CloseConnection();
        }


        // Simulated server impl
        // we get a message here every time the client send soemthing
        private static void mGingerSocketServer_Message(object sender, MessageEventArgs e)
        {
            //TODO:
            switch (e.MessageType)
            {
                case GingerSocket.eProtocolMessageType.PayLoad:
                    PayLoad PL = (PayLoad)e.obj;
                    PayLoad PLRC = HandlePayLoad(PL);
                    e.Response = PLRC;
                    break;
            }
            

            
        }

        private static PayLoad HandlePayLoad(PayLoad PL)
        {
            switch (PL.Name)
            {
                case "Echo":
                    string txt = PL.GetValueString();
                    PayLoad PLEcho = new PayLoad("EchoBack", txt);
                    return PLEcho;                    
                case "DummyAction":
                    PayLoad PLDummy = new PayLoad("OK", "Done");
                    return PLDummy;
                case "SpeedTest":
                    PayLoad PLSpeedTest = new PayLoad("OK", "Speedy");
                    return PLSpeedTest;                    
                case "SlowResponse1000":
                    Thread.Sleep(1000);
                    PayLoad PLOK1000 = new PayLoad("OK1000", "Done");
                    return PLOK1000;     
                case "LongAction":
                    Thread.Sleep(10000);
                    PayLoad PLOKLongAction = new PayLoad("LongActionDone", "Done");
                    return PLOKLongAction;     
                default :
                    throw new Exception("Unknown PayLoad Action - " + PL.Name);
            }
            

        }

        

        [TestMethod]  [Timeout(60000)]
        public void ClientConnect()
        {
            // Arrange

            //Act
          //  mGingerSocketClient.Connect("127.0.0.1", 7002);   

            //Assert
        }

        [TestMethod]  [Timeout(60000)]
        public void Echo()
        {
            // Arrange
            string txt = "abcABC123";
            PayLoad PL = new PayLoad("Echo", txt);

            //Act
            PayLoad PLRC = mGingerSocketClient.SendPayLoad(PL);
            string txt2 = PLRC.GetValueString();

            //Assert
           Assert.AreEqual(PLRC.Name, "EchoBack", "PLRC.Name = EchoBack");
           Assert.AreEqual(txt, txt2, "txt = txt2");
        }

        [TestMethod]  [Timeout(60000)]
        public void Echo1000Speed()
        {

            // We measure speed so we will not introduce code with communcation speed impact 

            // Arrange            
            PayLoad PL = new PayLoad("SpeedTest", "Hello Server");

            //Act
            Stopwatch st = new Stopwatch();
            st.Reset();
            st.Start();
            
                        
            for (int i=0;i<1000;i++)
            {
                PayLoad PLRC = mGingerSocketClient.SendPayLoad(PL);

                Assert.IsTrue(PLRC.IsOK()," PLRC.IsOK()");
            }
            st.Stop();
            
            //Assert
            // on fast PC it take less than 1500, on the build server it take 2,000 so keeping some buffer so UT will not fail            
            Assert.IsTrue(st.ElapsedMilliseconds < 3000, "st.ElapsedMilliseconds < 3000");
        }

        [TestMethod]  [Timeout(60000)]
        public void SlowResponse1000()
        {
            // Arrange            
            PayLoad PL = new PayLoad("SlowResponse1000", "Please respond after 1000 ms");

            //Act
            PayLoad PLRC = mGingerSocketClient.SendPayLoad(PL);

            //Assert
           Assert.AreEqual(PLRC.Name, "OK1000", "PLRC.Name = OK1000");
        }


        [TestMethod]  [Timeout(60000)]
        public void LongAction()
        {
            // Arrange            
            PayLoad PL = new PayLoad("LongAction", "Please respond after 10 seconds");

            //Act
            PayLoad PLRC = mGingerSocketClient.SendPayLoad(PL);            

            //Assert
           Assert.AreEqual(PLRC.Name, "LongActionDone", "PLRC.Name = LongActionDone");            
        }


    }
}
