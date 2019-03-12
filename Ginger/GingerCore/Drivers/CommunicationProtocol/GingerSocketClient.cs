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
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace GingerCore.Drivers.CommunicationProtocol
{
    // Generic class for ginger sockets to talk with each other
    // Can be C# to C# - like Ginger and GingerRemoteAgent
    // Can be C# to Java - like C# to Java driver.
    // Can be C# to Java - like C# to Android Java driver etc.
    // protocol is send/receive Payload will make it easier for consumers

    // Target is to avoid the implementation in each driver
    // It also includes built in packets logger to see the communication

    // TODO: remove it from Andorid driver
    // TODO: remove it from java driver
    // TODO: remove it from ASCF driver    
    // TODO: remove it from QCConnector

    public class GingerSocketClient
    {
        NetworkStream mNetworkStream;        
        TcpClient mClientSocket;
        IPEndPoint mServerAddress;

        bool IsGingerSocketLogging = false;
        GingerSocketMonitorWindow mGingerSocketMonitorWindow;

        public ObservableList<GingerSocketLog> GingerSocketLogs;

        public event MessageEventHandler Message;

        public delegate void MessageEventHandler(object sender, MessageEventArgs e);

        public void SetGingerSocketLogging(bool b)
        {
            IsGingerSocketLogging = b;

            if (IsGingerSocketLogging)
            {                
                    GingerSocketLogs = new ObservableList<GingerSocketLog>();

                    mGingerSocketMonitorWindow = new GingerSocketMonitorWindow(this);
                    mGingerSocketMonitorWindow.Show();
                    Thread.Sleep(100);                
            }
            else
            {            
                if (mGingerSocketMonitorWindow != null)
                {
                    GingerSocketLogs = null;
                    mGingerSocketMonitorWindow.Close();
                }
            }
        }

        public void OnMessage(GingerSocket.eProtocolMessageType MessageType, object obj = null)
        {
            MessageEventHandler handler = Message;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(MessageType, obj));
            }

            if (IsGingerSocketLogging)
            {
                GingerSocketLog GSL = new GingerSocketLog();
                GSL.Name = MessageType.ToString();
                GSL.Info = obj.ToString();
                GSL.LogType = "Message";
                GingerSocketLogs.Add(GSL);
            }
        }

        public void Connect(string IP, int Port)
        {
            //TODO: fix hard code local ip + port - so we can do remote when device is connected to another machine                        

            mServerAddress = new IPEndPoint(IPAddress.Parse(IP), Port);
            mClientSocket = new TcpClient();

            mClientSocket.ReceiveTimeout = 60 * 1000; ;
            mClientSocket.SendTimeout = 60 * 1000; 
            mClientSocket.ReceiveBufferSize = 1000000;
            mClientSocket.SendBufferSize = 1000000;
            mClientSocket.NoDelay = true;
            mClientSocket.Connect(mServerAddress);            
            mNetworkStream = mClientSocket.GetStream();     
        }
        
        // Make sure this function Synchronized so no new requests will be processed while the last one is not completed yet, requests will be queued and process one by one
        [MethodImpl(MethodImplOptions.Synchronized)]
        public PayLoad SendPayLoad(PayLoad pl)
        {
            if (IsGingerSocketLogging)
            {
                GingerSocketLog GSL = new GingerSocketLog();                
                GSL.LogType = "Send";
                GSL.SetPayLoad(pl);
                GingerSocketLogs.Add(GSL);
            }

            try
            {                
                Stopwatch st = new Stopwatch();
                st.Start();
                GingerSocket.Send(mNetworkStream, pl);
                PayLoad plrc = GingerSocket.ReadPayLoad(mClientSocket);
                st.Stop();

                if (IsGingerSocketLogging)
                {
                    GingerSocketLog GSL = new GingerSocketLog();                    
                    GSL.SetPayLoad(plrc);
                    GSL.LogType = "Recv";                    
                    GSL.Elapsed = st.ElapsedMilliseconds;

                    GingerSocketLogs.Add(GSL);
                }

                return plrc;
            }
            catch (Exception ex)
            {
                // Raise event so consumer can show a message or close nicely or retry connect
                OnMessage(GingerSocket.eProtocolMessageType.CommunicationError , ex.Message);
                return PayLoad.Error("Error in SendPayLoad:" + ex.Message);
            }
        }

        public void CloseConnection()
        {
            if (GingerSocket.SocketConnected(mClientSocket))            
            {
                // send graceful disconnect
                PayLoad PL = new PayLoad("GingerSocket", "Disconnect");
                SendPayLoad(PL);
            }
            if (mGingerSocketMonitorWindow != null)
            {
               
                mGingerSocketMonitorWindow.Dispatcher.Invoke(() =>
                {
                    mGingerSocketMonitorWindow.DelayedClose();
                });
            }
            
            mClientSocket.Close();
        }
    }
}