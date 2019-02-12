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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using GingerCoreNET.Drivers.CommunicationProtocol;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class GingerSocketServer2
    {
        Socket mServerSocketlistener;
        int mPort;
        public List<GingerSocketInfo> Clients = new List<GingerSocketInfo>();

        // Thread signal.  
        public ManualResetEvent allDone = new ManualResetEvent(false);
        public Action<GingerSocketInfo> MessageHandler { get; set; }
        public int ClientsCounter { get { return Clients.Count(); } }
        public string IPInfo;
        Task mTask;
        public bool isReady = false;

        public void StartServer(int port)
        {
            // Run the server on its own thread
            mTask = new Task(() => {
                DoStartServer(port);
            }) ;            
            mTask.Start();
            Stopwatch st = Stopwatch.StartNew();
            while (!isReady && st.ElapsedMilliseconds < 3000)  // shouldn't take more than 3 seconds to bind a port
            {
                Thread.Sleep(100);
            }
        }

        void DoStartServer(int port)
        {
            mPort = port;
                        
            IPAddress ipAddress = IPAddress.Parse(SocketHelper.GetLocalHostIP());  
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, mPort);

            IPInfo = ipAddress.ToString() + ":" + port;

            // Create a TCP/IP socket.  
            mServerSocketlistener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                mServerSocketlistener.Bind(localEndPoint); // TODO: Add Endpoint, config - can bind several IPs on same machine
                mServerSocketlistener.Listen(100);
                isReady = true;
                while (true)
                {
                    // Set the event to nonsignaled state.  
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.                      
                    mServerSocketlistener.BeginAccept(new AsyncCallback(AcceptCallback), mServerSocketlistener);
                    
                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                    Thread.Sleep(250);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }

        public void Shutdown()
        {
            //TODO: send message to all clients of shut down
        }
        
        // New incoming Ginger Client
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket socket = listener.EndAccept(ar);
            
            // Create the state object - one per Ginger client
            GingerSocketInfo gingerSocketInfo = new GingerSocketInfo();
            gingerSocketInfo.Socket = socket;
            gingerSocketInfo.SessionID = Guid.NewGuid();  // Create new session id
            gingerSocketInfo.MessageHandler = MessageHandler;
            gingerSocketInfo.Receive();
            Clients.Add(gingerSocketInfo);                        
        }

        // Sending a Payload from server to client expecting an answer, server initiated the request
        public NewPayLoad SendPayLoad(Guid sessionID, NewPayLoad pL)
        {
            GingerSocketInfo c = (from x in Clients where x.SessionID == sessionID select x).SingleOrDefault();
            if (c == null)
            {
                throw new Exception("SendPayLoad, SessionId not found: " + sessionID);
            }
            pL.PaylodType = NewPayLoad.ePaylodType.RequestPayload;            
            return c.SendRequest(pL);            
        }

        // Send Message to all clients, answer??
        public void Broadcast(NewPayLoad payLoad)
        {            
            // we can do Parallel for each..- speed
            foreach (GingerSocketInfo client in Clients)
            {
                if (client.Socket.Connected)
                {                                         
                    NewPayLoad rc = client.SendRequest(payLoad); // TODO: return message? or? or make it one way send 
                    
                }
                // TODO: else remove client
            }
        }
    }
}
