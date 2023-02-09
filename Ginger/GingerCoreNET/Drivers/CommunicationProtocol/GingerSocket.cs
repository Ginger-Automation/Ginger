#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Net.Sockets;
using System.Threading;

namespace GingerCoreNET.Drivers.CommunicationProtocol
{
    //Common items for GingerSocket Client/Server
    public static class GingerSocket
    {
        public enum eProtocolMessageType
        {
            GetVersion,
            PayLoad,
            CloseConnection,
            LostConnection,
            CommunicationError
        }

        internal static void Send(System.Net.Sockets.NetworkStream ns, NewPayLoad pl)
        {
            byte[] b = pl.GetPackage();
            ns.Write(b, 0, b.Length);
            ns.Flush();
        }

        internal static NewPayLoad ReadPayLoad(TcpClient clientSocket)
        {
            System.Net.Sockets.NetworkStream ns = clientSocket.GetStream();
            byte[] rcvLenBytesBB = new byte[4];

            //TODO: wait till max comm timeout                
            
            while (!ns.DataAvailable)
            {
                //TODO: adaptive sleep
                // General.DoEvents();

            // TODO: try async call instead of loop and thread sleep - = much faster
                // meanwhile we can sleep 1
                Thread.Sleep(1);   //TODO 10 is too big how can we avoid? - change to 1 for super speed but check CPU spin on long requests
                if (!SocketConnected(clientSocket))
                {
                    throw new Exception("GingerSocket.GetResponse ERROR: Lost connection");                                        
                }
            }
            ns.Read(rcvLenBytesBB, 0, 4);

            int rcvLen = ((rcvLenBytesBB[0]) << 24) + (rcvLenBytesBB[1] << 16) + (rcvLenBytesBB[2] << 8) + rcvLenBytesBB[3];

            int received = 0;

            byte[] rcvBytes = new byte[rcvLen + 4];

            //Copy len
            rcvBytes[0] = rcvLenBytesBB[0];
            rcvBytes[1] = rcvLenBytesBB[1];
            rcvBytes[2] = rcvLenBytesBB[2];
            rcvBytes[3] = rcvLenBytesBB[3];

            while (received < rcvLen)
            {
                received += ns.Read(rcvBytes, received + 4, rcvLen - received);
            }

            ns.Flush();

            NewPayLoad plRC = new NewPayLoad(rcvBytes);

            return plRC;
        }

        public static bool SocketConnected(TcpClient s)
        {
            //Keep this part as sometime bad discoonect happend and the below s.connected will still report true!!
            bool part1 = s.Client.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Client.Available == 0);
            if (part1 & part2)
            {
                return false;
            }

            // Check using the socket, not working all the time
            try
            {
                if (!s.Connected)
                {
                    //connection is closed
                    return false;
                }                
                return true;
            }
            catch
            {                
                return false;

            }
        }
    }
}