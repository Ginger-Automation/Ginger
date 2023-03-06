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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Run;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core.Drivers;
using GingerCore;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GingerCoreNET.RunLib
{
    public class GingerNodeProxy
    {
        public GingerGrid GingerGrid { get; set; }

        GingerNodeInfo mGingerNodeInfo;
        
        bool mRecordingSocketTraffic = false;

        private bool mIsConnected = false;

        bool isLocalGrid = true;


        // This is the socket connected or not - doesn't mean if the driver is started or closed
        public bool IsConnected
        {
            get
            {
                return mIsConnected;
            }
        }

        public GingerNodeProxy(GingerNodeInfo GNI, bool RemoteGrid = false)
        {
            mGingerNodeInfo = GNI;
            if (RemoteGrid)
            {
                isLocalGrid = false;
            }
        }

        /// <summary>
        /// Create GingerNodeProxy for Remote Grid Node
        /// </summary>
        /// <param name="hubClient"></param>
        /// <param name="sessionID"></param>
        public GingerNodeProxy(GingerSocketClient2 hubClient, Guid sessionID)
        {            
            this.hubClient = hubClient;
            this.sessionID = sessionID;
            isLocalGrid = false;
        }

        Guid sessionID;

        /// <summary>
        /// Find Node in Remote Grid and return GingerNodeProxy
        /// </summary>
        /// <param name="ServiceID"></param>
        /// <param name="remoteServiceGrids"></param>
        /// <returns></returns>
        public static GingerNodeProxy FindRemoteNode(string ServiceID, ObservableList<RemoteServiceGrid> remoteServiceGrids)
        {            
            foreach (RemoteServiceGrid remoteServiceGrid in remoteServiceGrids)
            {
                GingerSocketClient2 hubClient = new GingerSocketClient2();
                hubClient.Connect(remoteServiceGrid.Host, remoteServiceGrid.HostPort);                

                NewPayLoad findNodePayload = new NewPayLoad(SocketMessages.FindNode, ServiceID, "ccc");    // !!!!!!!!!!!!!!!!   ccc
                NewPayLoad RC = hubClient.SendRequestPayLoad(findNodePayload);
                if (RC.Name == "NodeInfo")
                {
                    Guid sessionID = RC.GetGuid();
                    GingerNodeProxy gingerNodeProxy = new GingerNodeProxy(hubClient, sessionID);
                    return gingerNodeProxy;
                }                   
            }
            return null;
        }



        public void StartRecordingSocketTraffic()
        {
            mRecordingSocketTraffic = true;
            Monitor.ShowMonitor(this);
        }


        public string Description
        {
            get
            {
                return mGingerNodeInfo.Name + "@" + mGingerNodeInfo.Host;
            }
        }

        public string Name
        {
            get
            {
                return mGingerNodeInfo.Name;
            }
        }

        public IGingerNodeMonitor Monitor { get; set; }

        public void Reserve()
        {
            /// !!!! FIXME for remoe grid
            //// if GingerGrid is local - we can do direct communication
            //if (GingerGrid != null)
            //{
            //    mIsConnected = GingerGrid.Reserve(mGingerNodeInfo.SessionID);
            //}
            //// TODO: else send via socket request to remote GG              
        }

        public void Disconnect()
        {
            //TODO: Release lock
        }

        public NewPayLoad RunAction(NewPayLoad actionPayload)
        {
            NewPayLoad resultPayload = SendRequestPayLoad(actionPayload);
            return resultPayload;
        }


        // TEMP needs a list and not here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public static string RemoteGridIP = SocketHelper.GetLocalHostIP();
        public static int RemoteGridPort = 15555;

        


        public NewPayLoad SendRequestPayLoad(NewPayLoad payload)
        {
            if (!mRecordingSocketTraffic)
            {
                return SendToNode(payload);                
            }
            else
            {
                GingerSocketLog gingerSocketLog = new GingerSocketLog();
                gingerSocketLog.SetPayLoad(payload);
                gingerSocketLog.LogType = "Send";
                Monitor.Add(gingerSocketLog);

                Stopwatch st = Stopwatch.StartNew();
                
                NewPayLoad responsePayload = SendToNode(payload);                

                st.Stop();

                GingerSocketLog rc = new GingerSocketLog();
                rc.SetPayLoad(responsePayload);
                rc.LogType = "Recv";
                rc.Elapsed = st.ElapsedMilliseconds;
                Monitor.Add(rc);

                return responsePayload;
            }

        }

        private NewPayLoad SendToNode(NewPayLoad payload)
        {
            if (isLocalGrid)
            {
                return GingerGrid.SendRequestPayLoad(mGingerNodeInfo.SessionID, payload);
            }
            else
            {
                return ExecuteActionOnRemoteGridPlugin(payload);
            }
        }

        
        private GingerSocketClient2 hubClient;

        public NewPayLoad ExecuteActionOnRemoteGridPlugin(NewPayLoad payload)
        {
            bool closeConn = false;            
            if (hubClient == null)
            {
                hubClient = new GingerSocketClient2();
                hubClient.Connect(RemoteGridIP, RemoteGridPort);
                // For action without session
                NewPayLoad fpl = new NewPayLoad(SocketMessages.FindNode, "MathService", "ccc");    // !!!!!!!!!!!!!!!!       
                                                                                                   // NewPayLoad fpl = new NewPayLoad(SocketMessages.FindNode, "SeleniumChromeService", "ccc");    // !!!!!!!!!!!!!!!!         DUP REMOVE !!!!
                NewPayLoad rc = hubClient.SendRequestPayLoad(fpl);

                sessionID = rc.GetGuid();
                closeConn = true;
            }
            else
            {
                
            }

            
            //  TODO: reserve if session


            NewPayLoad fpl3 = new NewPayLoad(SocketMessages.SendToNode, sessionID, payload);
            // fpl3.ClosePackage();
            NewPayLoad rc4 = hubClient.SendRequestPayLoad(fpl3); // Send to Ginger Grid which will send to Ginger Node to run the action

            if (closeConn)
            {
                hubClient.CloseConnection();   // Not for session
            }

            return rc4;                        
        }


        public void StartDriver(Amdocs.Ginger.Common.ObservableList<DriverConfigParam> driverConfiguration=null)
        {
            //TODO: get return code - based on it set status if running OK
            NewPayLoad PL = new NewPayLoad("StartDriver");    //!!!! Rename to StartService + use const
            List<NewPayLoad> DriverConfigs = new List<NewPayLoad>();

            if (driverConfiguration != null)
            {
                foreach (DriverConfigParam DC in driverConfiguration)
                {
                    NewPayLoad FieldPL = new NewPayLoad("Config", DC.Parameter, DC.Value == null ? " " : DC.Value);

                    DriverConfigs.Add(FieldPL);
                }
            }
            PL.AddListPayLoad(DriverConfigs);
            PL.ClosePackage();
            NewPayLoad plss = SendRequestPayLoad(PL);

            if (plss.IsErrorPayLoad())
            {
                throw new Exception("Error in GingerNodeProxy.StartDriver - " + plss.GetValueString());
            }
        }

        public void CloseDriver()
        {
            NewPayLoad PL = new NewPayLoad("CloseDriver");  //!!!! Rename to StopService + use const
            PL.ClosePackage();
            NewPayLoad RC = SendRequestPayLoad(PL);
        }

        public void Shutdown()
        {
            NewPayLoad PL = new NewPayLoad("Shutdown");
            PL.AddValue(mGingerNodeInfo.SessionID);
            PL.ClosePackage();
            NewPayLoad RC = SendRequestPayLoad(PL);
            mIsConnected = false;
        }

        public string Ping()
        {
            //TODO: create test when node is down etc.            
            NewPayLoad PL = new NewPayLoad("Ping");
            PL.ClosePackage();
            Stopwatch stopwatch = Stopwatch.StartNew();
            NewPayLoad RC = SendRequestPayLoad(PL);
            stopwatch.Stop();
            string rc = RC.GetValueString() + ", ElapsedMS=" + stopwatch.ElapsedMilliseconds;
            return rc;
        }

        public void AttachDisplay()
        {
            //TODO: get return code - based on it set status if running OK
            NewPayLoad PL = new NewPayLoad(nameof(IDriverDisplay.AttachDisplay));
            PL.PaylodType = NewPayLoad.ePaylodType.DriverRequest;

            //tODO
            string host = SocketHelper.GetDisplayHost();
            int port = SocketHelper.GetDisplayPort();
            PL.AddValue(host);
            PL.AddValue(port);
            PL.ClosePackage();
            SendRequestPayLoad(PL);
        }


    }
}