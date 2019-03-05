using System;
using System.Collections.Generic;

using System.Text;
using PureWebSockets;

namespace Amdocs.Ginger.Common.NodeJs
{
    public class NodeClient
    {
        int port;
        PureWebSocket Ws;
        PureWebSocketOptions socketOptions = new PureWebSocketOptions()
        {
            DebugMode = true,
            SendDelay = 100,
            IgnoreCertErrors = true,
            MyReconnectStrategy = new ReconnectStrategy(2000, 4000, 20)
        };
        public NodeClient()
        {
            Ws = new PureWebSocket("ws://localhost:2999", socketOptions);
            Ws.Connect();
            Ws.OnMessage += Ws_OnMessage;
            Ws.Send(@"C:\Users\mohdkhan\Pictures\abc.jpg");
        }

        private void Ws_OnMessage(string message)
        {

            System.IO.File.WriteAllText(@"C:\Users\mohdkhan\Pictures\abc.txt", message);
            Console.WriteLine(message);   
        }


        public void SendMessage(string message)
        {
            Ws.Send(message);
        }
    }
}
