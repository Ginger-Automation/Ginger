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

using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class GingerSocketClient2
    {
        GingerSocketInfo mGingerSocketInfo = null;
        
        public int IncomingRequetsesCounter { get { return mGingerSocketInfo.IncomingRequetsesCounter; } }
        public int OutgoingRequetsesCounter { get { return mGingerSocketInfo.OutgoingRequetsesCounter; } }
        public int BytesIn { get { return mGingerSocketInfo.BytesIn; } }
        public int bytesOut { get { return mGingerSocketInfo.bytesOut; } }

        public Guid SessionID {get { return mGingerSocketInfo.SessionID; } }

        // ManualResetEvent instances signal completion.  
        private ManualResetEvent mConnectDone = new ManualResetEvent(false);

        bool mConnected = false;
        
        // Class using it attach its handler to Action
        public Action<GingerSocketInfo> MessageHandler { get; set; }
        public bool IsConnected
        {
            get
            {
                return mConnected;
            }
        }
        public void Connect(string IP, int port)
        {            
            try
            {                
                //Local host
                IPAddress ipAddress = IPAddress.Parse(IP); 
                IPEndPoint remoteIP = new IPEndPoint(ipAddress, port);                
                Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                Console.WriteLine("Connecting to: " + remoteIP + ":" + port);
                // Connect to Ginger Server async, retyr max 10 seconds                
                Stopwatch stopwatch = Stopwatch.StartNew();
                int retrycount = 0;
                while (!socket.Connected && stopwatch.ElapsedMilliseconds < 30000)
                {                                        
                    socket.BeginConnect(remoteIP, new AsyncCallback(ConnectCallback), socket);
                    mConnectDone.WaitOne();                    
                    if (!socket.Connected)
                    {
                        retrycount++;
                        Console.WriteLine("Connect retry #" + retrycount);
                        Thread.Sleep(5000);
                    }
                }

                if (socket.Connected)
                {
                    // Now Ginger client is ready for incoming data
                    mConnected = true;
                }
                else
                {
                    Console.WriteLine("Failed to connect, exiting");
                    mConnected = false;
                    return;
                }

                //start waiting for incoming data async - non blocking

                // Create the state object.  
                mGingerSocketInfo = new GingerSocketInfo();
                mGingerSocketInfo.Socket = socket;
                mGingerSocketInfo.MessageHandler = MessageHandler;
                mGingerSocketInfo.Receive();

                
                // if there is code here it will run - no wait

                //TODO: handshake: version, security, encryption
                NewPayLoad HandShake1 = new NewPayLoad("GetSession", "v1");
                HandShake1.PaylodType = NewPayLoad.ePaylodType.SocketRequest;
                NewPayLoad RC = SendSocketPayLoad(HandShake1);
                if (RC.Name == "SessionID")
                {
                    mGingerSocketInfo.SessionID = RC.GetGuid();
                }
                else
                {                    
                    throw new Exception("Error in connecting to GingerSocketServer invalid Session");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                throw e;
            }
        }

        public void CloseConnection()
        {
            NewPayLoad payload = new NewPayLoad("NodeClosing", SessionID);
            payload.PaylodType = NewPayLoad.ePaylodType.SocketRequest;
            NewPayLoad RC = SendSocketPayLoad(payload);
        }

        public NewPayLoad SendRequestPayLoad(NewPayLoad pl)
        {
            pl.PaylodType = NewPayLoad.ePaylodType.RequestPayload;
            return mGingerSocketInfo.SendRequest(pl);            
        }

        public NewPayLoad SendSocketPayLoad(NewPayLoad pl)
        {
            pl.PaylodType = NewPayLoad.ePaylodType.SocketRequest;
            return mGingerSocketInfo.SendRequest(pl);
        }

        // After connect is succesful we get callback
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;
                if (client.Connected)
                {
                    client.EndConnect(ar);
                    mConnected = true;
                }
                else
                {
                    mConnected = false;
                }
                
                // Complete the connection 

                // Signal that the connection has been made.  
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error at Connect Callback: " + ex.Message);
                

                // the connect fail we will retry, need to release the Wait code                
            }
            finally
            {
                mConnectDone.Set();
            }
        }
    }
}
