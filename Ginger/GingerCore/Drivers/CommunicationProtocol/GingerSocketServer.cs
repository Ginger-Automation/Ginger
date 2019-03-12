#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GingerCore.Drivers.CommunicationProtocol
{
    // Ginger server can handle many clients in parallel 
    // each client is running on separate thread
    public class GingerSocketServer
    {
        private TcpListener tcpListener;
        private Thread listenThread;
        
        private int mPort;

        bool IsGingerSocketLogging = false;
        public ObservableList<GingerSocketLog> GingerSocketLogs;

        public event MessageEventHandler Message;

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);

        public PayLoad OnMessage(GingerSocket.eProtocolMessageType MessageType, object obj = null)
        {
            MessageEventHandler handler = Message;
            if (handler != null)
            {
                MessageEventArgs EVArgs = new MessageEventArgs(MessageType, obj);
                handler(this, EVArgs);

                if (IsGingerSocketLogging)
                {
                    GingerSocketLog GSL = new GingerSocketLog();
                    GSL.TimeStamp = DateTime.Now;
                    GSL.Name = MessageType.ToString();
                    GSL.Info = obj.ToString();
                    GSL.LogType = "Message";
                    GingerSocketLogs.Add(GSL);
                }
                return EVArgs.Response;                
            }
            else
            {
                return null;
            }
        }
        
        public void StratServer(int Port)
        {
            mPort = Port;
            string IP = Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();                        
            tcpListener = new TcpListener(IPAddress.Any, mPort);
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
        }

        public void StopServer()
        {
            tcpListener.Stop();
            listenThread.Abort();
        }

        private void ListenForClients()
        {
            //TODO: handle err if socket is not available

            this.tcpListener.Start();
            while (true)
            {
                //blocks until client has connected or we get ThreadAbort
                try
                {
                    TcpClient client = this.tcpListener.AcceptTcpClient();
                    //create a thread to handle communication with client
                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch
                {
                    // Thread abort can close it
                    //TODO: ??
                }
            }
        }

        public void HandleClientComm(object client)
        {
            TcpClient tcpClient = (TcpClient)client;

            // TODO: need to decide when to go out of the loop !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            while (true)
            {                
                try
                {
                    PayLoad PL = GingerSocket.ReadPayLoad(tcpClient);
                    if (PL.Name == "GingerSocket")
                    {
                        // This is Client/Server Protocol talks
                        string txt = PL.GetValueString();
                        if (txt == "Disconnect")
                        {
                            tcpClient.Close();
                            break;
                        }
                    }
                    else
                    {
                        PayLoad PLRC = ProcessMessage(PL);
                        if (PLRC != null)  // if async then it will be null? or we must return something, for async we should return start processing - or key id of req to pair later
                        {
                            SendMessage(tcpClient, PLRC);
                        }
                    }                    
                }
                catch(Exception ex)
                {
                    //TODO: handle
                    // Client cut the connection or some err
                    //a socket error has occurred
                    // or any other server err
                    PayLoad PLErr = PayLoad.Error("Error - " + ex.Message);
                    SendMessage(tcpClient, PLErr);
                }
            }
            tcpClient.Close();
        }

        private PayLoad ProcessMessage(PayLoad PL)
        {
            PayLoad PLRC = OnMessage(GingerSocket.eProtocolMessageType.PayLoad, PL);
            return PLRC;
        }

        private void SendMessage(TcpClient tcpClient, PayLoad PL)
        {                        
            NetworkStream clientStream = tcpClient.GetStream();
            GingerSocket.Send(clientStream, PL);
        }

        public void CloseConnection()
        {
            tcpListener.Stop();            
            listenThread.Abort();  
        }
    }
}