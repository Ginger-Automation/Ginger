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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Linq;

namespace GingerCoreNET.RunLib
{
    public class GingerGrid
    {
        // Maybe better ASP.NET - with web pages and rest API to communicate
        GingerSocketServer2 mGingerSocketServer;

        ObservableList<GingerNodeInfo> mGingerNodeInfo = new ObservableList<GingerNodeInfo>();

        int mPort;

        public GingerGrid(int Port)
        {
            mPort = Port;
        }

        public void Start()
        {
            mGingerSocketServer = new GingerSocketServer2();
            mGingerSocketServer.StartServer(mPort);
            mGingerSocketServer.MessageHandler = GingerSocketServerMessageHandler;
        }

        private void GingerSocketServerMessageHandler(GingerSocketInfo gingerSocketInfo)
        {
            NewPayLoad p = gingerSocketInfo.DataAsPayload;
            switch (p.Name)
            {
                case "List":  //TODO: use const
                    {
                        NewPayLoad RC = new NewPayLoad("RC");
                        RC.AddValue("OK");

                        //TODO: return the list of GingerNodeInfo

                        RC.ClosePackage();
                        gingerSocketInfo.Response = RC;
                        break;
                    }

                case SocketMessages.Register:
                    {
                        string NodeName = p.GetValueString();
                        string NodeServiceID = p.GetValueString();
                        string NodeOS = p.GetValueString();
                        string NodeHost = p.GetValueString();
                        string NodeIP = p.GetValueString();

                        NewPayLoad RC = new NewPayLoad("SessionID", gingerSocketInfo.SessionID);
                        gingerSocketInfo.Response = RC;

                        // add the info of the new node to the grid list
                        mGingerNodeInfo.Add(new GingerNodeInfo() { Name = NodeName, ServiceId = NodeServiceID, OS = NodeOS, Host = NodeHost, IP = NodeIP, SessionID = gingerSocketInfo.SessionID , Status = GingerNodeInfo.eStatus.Ready });
                        break;
                    }

                case SocketMessages.Unregister:
                    {
                        Guid SessionID = p.GetGuid();
                        GingerNodeInfo GNI = (from x in mGingerNodeInfo where x.SessionID == SessionID select x).FirstOrDefault();
                        if (GNI == null)
                        {
                            gingerSocketInfo.Response =  new NewPayLoad("Error", "Ginger node info not found for session id " + SessionID.ToString());
                        }

                        mGingerNodeInfo.Remove(GNI);

                        NewPayLoad RC = new NewPayLoad("OK");
                        RC.ClosePackage();
                        gingerSocketInfo.Response = RC;
                        break;
                    }
                default:
                    throw new Exception("GingerSocketServerMessageHandler: Unknown Message type: " + p.Name);
            }
        }

        public bool Reserve(Guid GingerNodeSessionID) // send the GNI
        {
            //TODO: handle SessionID null

            NewPayLoad ReservePL = new NewPayLoad(SocketMessages.Reserve, GingerNodeSessionID);
            NewPayLoad rc = mGingerSocketServer.SendPayLoad(GingerNodeSessionID, ReservePL);
            string s = rc.GetValueString();
            if (s == "OK")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stop()
        {
            //TODO: notify all clinets that server is closing,
            mGingerSocketServer.Shutdown();
        }

        public string Status
        {
            get { return SocketHelper.GetLocalHostIP() + " Port: " + mPort; }  // TODO: add status enum 
        }

        public ObservableList<GingerNodeInfo> NodeList
        {
            get
            {
                return mGingerNodeInfo;
            }
        }

        public int Port { get { return mPort; } }

        public void LoadLocalDrivers()
        {

        }
        

        internal NewPayLoad SendRequestPayLoad(Guid sessionID, NewPayLoad payload)
        {
            NewPayLoad rc = mGingerSocketServer.SendPayLoad(sessionID, payload);
            return rc;
        }

        public void Reset()
        {
            // TODO: send ShutDown to each node
            //foreach (GingerNodeInfo GNI in NodeList)
            //{
            //    GNI.
            //}

            NodeList.Clear();
        }
    }
}