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

using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Drivers.CommunicationProtocol
{

    [TestClass]
    [Level1]
    public class GingerSocket2Test
    {
        

        // --------------------------------------------------------------------------------------------
        // Sample Ginger Server class
        // --------------------------------------------------------------------------------------------
        class MyGingerServer
        {
            GingerSocketServer2 mGingerSocketServer;
            public int ServerPort { get; set; }

            public void Start()
            {
                mGingerSocketServer = new GingerSocketServer2();
                mGingerSocketServer.MessageHandler = MessageHandler;
                ServerPort = SocketHelper.GetOpenPort();
                mGingerSocketServer.StartServer(ServerPort);   
            }

            public void ShutDown()
            {
                mGingerSocketServer.Shutdown();
            }

            private void MessageHandler(GingerSocketInfo gingerSocketInfo)
            {
                NewPayLoad PLRC = HandlePayLoad(gingerSocketInfo.DataAsPayload);
                gingerSocketInfo.Response = PLRC;
            }


            // Handle Client request
            private NewPayLoad HandlePayLoad(NewPayLoad PL)
            {
                switch (PL.Name)
                {
                    case "Echo":
                        string txt = PL.GetValueString();
                        NewPayLoad PLEcho = new NewPayLoad("EchoBack", txt);
                        return PLEcho;
                    case "DummyAction":
                        NewPayLoad PLDummy = new NewPayLoad("OK", "Done");
                        return PLDummy;
                    case "SpeedTest":
                        NewPayLoad PLSpeedTest = new NewPayLoad("OK", "Speedy");
                        return PLSpeedTest;
                    case "SlowResponse1000":
                        Thread.Sleep(1000);
                        NewPayLoad PLOK1000 = new NewPayLoad("OK1000", "Done");
                        return PLOK1000;
                    case "LongAction":
                        Thread.Sleep(10000);
                        NewPayLoad PLOKLongAction = new NewPayLoad("LongActionDone", "Done");
                        return PLOKLongAction;
                    case "CalcSum":
                        int num = PL.GetValueInt();
                        int total = 0;
                        for (int j = 0; j < num; j++)
                        {
                            total += j;
                        }
                        NewPayLoad PLOKTotalAction = new NewPayLoad("Total", total);
                        return PLOKTotalAction;
                    default:
                        throw new Exception("Unknown PayLoad Action - " + PL.Name);
                }


            }


            internal NewPayLoad SendPayLoad(Guid sessionID, NewPayLoad pL)
            {
                return mGingerSocketServer.SendPayLoad(sessionID, pL);
            }
        }

        // --------------------------------------------------------------------------------------------
        // Sample Ginger Client app
        // --------------------------------------------------------------------------------------------

        class MyGingerClient
        {
            GingerSocketClient2 mGingerSocketClient2;

            public Guid SessionID { get { return mGingerSocketClient2.SessionID; } }

            public void Connect()
            {
                mGingerSocketClient2 = new GingerSocketClient2();
                mGingerSocketClient2.MessageHandler = MessageHandler;

                IPAddress ipAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
                mGingerSocketClient2.Connect(SocketHelper.GetLocalHostIP(), mMyGingerServer.ServerPort);                 
            }

            public void Disconnect()
            {
                mGingerSocketClient2.CloseConnection();
            }

            // Handle server messages
            private void MessageHandler(GingerSocketInfo obj)
            {
                NewPayLoad PLRC = HandlePayLoadFromServer(obj.DataAsPayload);
                obj.Response = PLRC;
            }

            NewPayLoad HandlePayLoadFromServer(NewPayLoad pL)
            {
                return new NewPayLoad("Client Response to server", "OK");
            }

            public NewPayLoad Send(NewPayLoad data)
            {
                NewPayLoad rc = mGingerSocketClient2.SendRequestPayLoad(data);
                return rc;
            }


        }

        // --------------------------------------------------------------------------------------------
        // Static vars
        // --------------------------------------------------------------------------------------------

        static MyGingerServer mMyGingerServer;
        static MyGingerClient mMyGingerClient;

        // --------------------------------------------------------------------------------------------
        // Test Methods
        // --------------------------------------------------------------------------------------------


        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            // run server and client on 2 threads to simulate real world and enable bi-directinal comuncation

            Task task1 = new Task(() =>
            {
                mMyGingerServer = new MyGingerServer();
                mMyGingerServer.Start();
            });
            task1.Start();

            Thread.Sleep(100);

            Task task2 = new Task(() =>
            {
                mMyGingerClient = new MyGingerClient();
                mMyGingerClient.Connect();
            });
            task2.Start();

            Thread.Sleep(500);

        }



        [ClassCleanup()]
        public static void ClassCleanup()
        {

            //mMyGingerClient.Close(); //TODO: !!! ??
            //mMyGingerServer.ShutDown();

        }




        [TestMethod]
        public void Echo()
        {
            // Arrange
            string txt = "abcABC123";
            NewPayLoad PL = new NewPayLoad("Echo", txt);

            //Act
            NewPayLoad PLRC = mMyGingerClient.Send(PL);
            string txt2 = PLRC.GetValueString();

            //Assert
            Assert.AreEqual(PLRC.Name, "EchoBack", "PLRC.Name = EchoBack");
            Assert.AreEqual(txt, txt2, "txt = txt2");
        }

        //[TestMethod]
        //public void Echo1000Speed()
        //{
        //    // We measure speed so we will not introduce code with communcation speed impact 

        //    // Arrange            
        //    Thread.Sleep(100);  // let the system or other process relax...

        //    //Act
        //    Stopwatch st = new Stopwatch();
        //    st.Reset();
        //    st.Start();


        //    for (int i = 0; i < 1000; i++)
        //    {                
        //        NewPayLoad PL = new NewPayLoad("SpeedTest", "Hello Server - " + i);
        //        NewPayLoad PLRC = mMyGingerClient.Send(PL);
        //        Assert.IsTrue(PLRC.IsOK(), " PLRC.IsOK()");
        //    }
        //    st.Stop();

        //    //Assert
        //    // on fast PC it take less than 500, on the build server it take ??? so keeping some buffer so UT will not fail            
        //    Assert.IsTrue(st.ElapsedMilliseconds < 500, "st.ElapsedMilliseconds < 500");
        //}

        [TestMethod]
        public void EchoBig10KMessage()
        {
            // Arrange
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                sb.Append("0123456789");
            }

            NewPayLoad PL = new NewPayLoad("Echo", sb.ToString());

            //Act
            NewPayLoad PLRC = mMyGingerClient.Send(PL);
            string txt2 = PLRC.GetValueString();

            //Assert
            Assert.AreEqual(PLRC.Name, "EchoBack", "PLRC.Name = EchoBack");
            Assert.AreEqual(sb.ToString(), txt2, "sb.ToString() = txt2");
        }


        [TestMethod]
        public void EchoRandomMessageSize()
        {
            // Arrange
            // Create SB size of 1M Bytes - 1,000,000
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 100000; i++)
            {
                sb.Append("0123456789");
            }

            List<string> list = new List<string>();
            list.Add(sb.ToString().Substring(0, 106660));

            list.Add(sb.ToString().Substring(0, 10));
            list.Add(sb.ToString().Substring(0, 500));
            list.Add(sb.ToString().Substring(0, 30000));
            list.Add(sb.ToString().Substring(0, 10));
            list.Add(sb.ToString().Substring(0, 1));
            list.Add(sb.ToString().Substring(0, 20000));
            list.Add(sb.ToString().Substring(0, 100));
            // list.Add(sb.ToString().Substring(0, 1000000));           !!!!!!!!!!!!!!!FIXME
            list.Add(sb.ToString().Substring(0, 400));
            list.Add(sb.ToString().Substring(0, 10000));
            list.Add(sb.ToString().Substring(0, 1024));
            list.Add(sb.ToString().Substring(0, 50000));

            list.Add(sb.ToString().Substring(0, 1350));
            list.Add(sb.ToString().Substring(0, 8921));


            int count = 0;

            //Act
            foreach (string s in list)
            {
                NewPayLoad PL = new NewPayLoad("Echo", s);

                NewPayLoad PLRC = mMyGingerClient.Send(PL);
                string txt2 = PLRC.GetValueString();
                if (s == txt2)
                {
                    count++;
                }
            }

            //Assert
            Assert.AreEqual(list.Count, count);


        }

        
        [TestMethod]
        public void SlowResponse1000()
        {
            // Arrange            
            NewPayLoad PL = new NewPayLoad("SlowResponse1000", "Please respond after 1000 ms");

            //Act
            NewPayLoad PLRC = mMyGingerClient.Send(PL);

            //Assert
            Assert.AreEqual(PLRC.Name, "OK1000", "PLRC.Name = OK1000");
        }


        [TestMethod]
        public void LongAction()
        {
            // Arrange            
            NewPayLoad PL = new NewPayLoad("LongAction", "Please respond after 10 seconds");

            //Act
            NewPayLoad PLRC = mMyGingerClient.Send(PL);

            //Assert
            Assert.AreEqual(PLRC.Name, "LongActionDone", "PLRC.Name = LongActionDone");
        }

        [TestMethod]
        public void ServerSendMessagetoClient()
        {
            //FIXME  get stuck!!!

            // Arrange
            string txt = "This is your server";
            NewPayLoad PL = new NewPayLoad("MyServerMessage", txt);

            //Act
            NewPayLoad PLRC = mMyGingerServer.SendPayLoad(mMyGingerClient.SessionID, PL);
            string txt2 = PLRC.GetValueString();

            //Assert
            Assert.AreEqual("Client Response to server", PLRC.Name, "PLRC.Name = Client Response to server");
            Assert.AreEqual("OK", txt2);
        }



        [TestMethod]
        public void ClientSendCalcSum()
        {
            // Arrange            
            NewPayLoad PL = new NewPayLoad("CalcSum", 100000000);


            //Act
            NewPayLoad PLRC = mMyGingerClient.Send(PL);


            int total = PLRC.GetValueInt();

            //Assert
            // Assert.AreEqual("ClientMessage", PLRC.Name, "PLRC.Name = ClientMessage");
            // Assert.AreEqual("Processing Started", txt2);
        }


        [TestMethod]
        public void ClientConnectSendClose()
        {
            // Arrange
            MyGingerClient client1 = new MyGingerClient();
            string txt = "Hi from Client 1";
            NewPayLoad PL = new NewPayLoad("Echo", txt);

            //Act
            client1.Connect();
            NewPayLoad PLRC = client1.Send(PL);
            string txt2 = PLRC.GetValueString();
            client1.Disconnect();

            //Assert
            Assert.AreEqual(PLRC.Name, "EchoBack", "PLRC.Name = EchoBack");
            Assert.AreEqual(txt, txt2, "txt = txt2");
        }

        
        [TestMethod]
        public void Run10ClientsParallel()
        {
            // Arrange
            List<Task> list = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                Task t = new Task(() =>
                {
                    Random r = new Random();
                    MyGingerClient client1 = new MyGingerClient();
                    string txt = "Hi from Client 1";
                    NewPayLoad PL = new NewPayLoad("Echo", txt);
                    client1.Connect();
                    for (int j = 0; j < 1000; j++)
                    {
                        NewPayLoad PLRC = client1.Send(PL);
                        string txt2 = PLRC.GetValueString();
                        if (txt != txt2)
                        {
                            throw new Exception("Error in Echo!!");
                        }

                        int sleep = r.Next(0, 20);
                        Thread.Sleep(sleep);
                    }
                    client1.Disconnect();
                });
                t.Start();
                list.Add(t);
            }


            //Act
            // Wait for all clients to complete their work
            foreach (Task t in list)
            {
                t.Wait();
            }


            //Assert
            //Assert.AreEqual(PLRC.Name, "EchoBack", "PLRC.Name = EchoBack");
            //Assert.AreEqual(txt, txt2, "txt = txt2");
        }


        [TestMethod]
        public void VerifyClientGetUniqueGUID()
        {
            // Arrange
            Guid EmptyGuid = new Guid();
            MyGingerClient client1 = new MyGingerClient();
            Guid guid;
            
            //Act
            client1.Connect();
            guid = client1.SessionID;
            client1.Disconnect();

            //Assert
            Assert.AreNotEqual(EmptyGuid.ToString(), guid.ToString(), "GUID of client session is not empty GUID");            
        }



    }
}
