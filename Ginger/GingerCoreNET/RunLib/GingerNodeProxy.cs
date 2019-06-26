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
        // This is the socket connected or not - doesn't mean if the driver is started or closed
        public bool IsConnected
        {
            get
            {
                return mIsConnected;
            }
        }

        public GingerNodeProxy(GingerNodeInfo GNI)
        {
            mGingerNodeInfo = GNI;
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

        public NewPayLoad SendRequestPayLoad(NewPayLoad payload)
        {
            if (!mRecordingSocketTraffic)
            {
                return GingerGrid.SendRequestPayLoad(mGingerNodeInfo.SessionID, payload);
            }
            else
            {
                GingerSocketLog gingerSocketLog = new GingerSocketLog();
                gingerSocketLog.SetPayLoad(payload);
                gingerSocketLog.LogType = "Send";
                Monitor.Add(gingerSocketLog);

                Stopwatch st = Stopwatch.StartNew();

                // if local grid use !!!!!!!!!!!!!!
                NewPayLoad responsePayload = GingerGrid.SendRequestPayLoad(mGingerNodeInfo.SessionID, payload);
                // else use remote grid

                st.Stop();

                GingerSocketLog rc = new GingerSocketLog();
                rc.SetPayLoad(responsePayload);
                rc.LogType = "Recv";
                rc.Elapsed = st.ElapsedMilliseconds;
                Monitor.Add(rc);

                return responsePayload;
            }

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