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
using GingerCoreNET.Drivers;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerPlugInsNET.ActionsLib;
using GingerPlugInsNET.DriversLib;
using GingerPlugInsNET.ServicesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GingerCoreNET.DriversLib
{
    //This class is for end driver side communication - the GingerNodeAgent will connect to it and send commands
    public class GingerNode
    {
        public string ConnectionString { get; set; }

        private PluginDriverBase mDriver;
        private PluginServiceBase mService;

        // We use Hub client to send Register/UnRegister message - to GingerGrid manager
        GingerSocketClient2 mHubClient;
        Guid mSessionID;

        //TODO: check what we can hide from here and move to GingerCore  -- all the communcation Payload stuff move from here

        public GingerNode(DriverCapabilities DriverCapabilities, PluginDriverBase driver)
        {
            this.mDriver = driver;
        }

        public GingerNode(PluginServiceBase service)
        {
            this.mService = service;            
        }

        public enum eGingerNodeEventType
        {
            Started,
            Shutdown
        }

        public delegate void GingerNodeMessageEvent(GingerNode gingerNode, eGingerNodeEventType GingerNodeEventType);
        public event GingerNodeMessageEvent GingerNodeMessage;

        public void StartGingerNode(string Name, string HubIP, int HubPort)
        {
            Console.WriteLine("Starting Ginger Node");
            
            string Domain = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string IP = SocketHelper.GetLocalHostIP(); 
            string MachineName = System.Environment.MachineName;
            string OSVersion = System.Environment.OSVersion.ToString();      

            //TODO: first register at the hub
            mHubClient = new GingerSocketClient2();
            mHubClient.MessageHandler = HubClientMessageHandler;

            // We have retry mechanism
            bool IsConnected = false;
            int count = 0;
            while (!IsConnected)
            {
                try
                {
                    Console.WriteLine("Connecting to Ginger Grid Hub: " + HubIP + ":" + HubPort);
                    mHubClient.Connect(HubIP, HubPort);
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection failed: " + ex.Message);
                    Console.WriteLine("Will retry in 5 seconds");
                    Thread.Sleep(5000);  
                    count++;
                    if (count>50) //TODO: Change it to stop watch wait for 60 seconds, or based on config
                    {
                        Console.WriteLine("All connection attempts failed, exiting");
                        return;
                    }
                }
            }
            
            NewPayLoad PLRegister = new NewPayLoad(SocketMessages.Register);
            PLRegister.AddValue(Name);
            PLRegister.AddValue(OSVersion);  // TODO: translate to normal name?
            PLRegister.AddValue(MachineName);  // TODO: if local host write local host
            PLRegister.AddValue(IP);
            
            PLRegister.ClosePackage();
            NewPayLoad RC = mHubClient.SendRequestPayLoad(PLRegister);
            
            //TODO: we can disconnect from hub, make Register/Unregister a function

            if (RC.Name == "SessionID")
            {                
                mSessionID = RC.GetGuid();                
                Console.WriteLine("Ginger Node started SessionID - " + mSessionID);                
                if (GingerNodeMessage != null)
                {
                    GingerNodeMessage.Invoke(this, eGingerNodeEventType.Started);
                }
            }
            else
            {
                throw new Exception("Unable to find Ginger Grid at: " + HubIP + ":" + HubPort);
            }
        }

        private void HubClientMessageHandler(GingerSocketInfo gingerSocketInfo)
        {
            Console.WriteLine("Processing Message");

            NewPayLoad pl = gingerSocketInfo.DataAsPayload;
            switch (pl.Name)
            {
                case SocketMessages.Reserve:
                    gingerSocketInfo.Response = Reserve(pl);
                    break;
                case "RunAction":
                    gingerSocketInfo.Response = RunAction(pl);
                    break;
                case "StartDriver":
                    gingerSocketInfo.Response = StartDriver();
                    break;
                case "CloseDriver":
                    gingerSocketInfo.Response = CloseDriver();
                    break;
                case "Shutdown":
                    gingerSocketInfo.Response = ShutdownNode();
                    break;
                case "Ping":
                    gingerSocketInfo.Response = Ping();
                    break;
                case "AttachDisplay":
                    gingerSocketInfo.Response = AttachDisplay(pl);
                    break;
                default:
                    throw new Exception("Unknown Messgae: " + pl.Name);
            }
        }

        private NewPayLoad Reserve(NewPayLoad pl)
        {
            Guid sessionID = pl.GetGuid();
            if (sessionID.Equals(mSessionID))
            {
                return new NewPayLoad("Connected", "OK");
            }
            return NewPayLoad.Error("Bad Session ID: " + sessionID);
        }
       
        private NewPayLoad AttachDisplay(NewPayLoad pl)
        {
            string host = pl.GetValueString();
            int port = pl.GetValueInt();
            RemoteObjectsClient c = new RemoteObjectsClient();            
            c.Connect(host, port);
            
            //TODO: fix hard coded !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Assembly driverAssembly = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerWebServicesPlugin\bin\Debug\netstandard2.0\WebServices.GingerPlugin.dll");
            Type t = driverAssembly.GetType("Amdocs.Ginger.WebServices.IWebServicesDriverDisplay");

            // We do all using reflection, since we don't have ref to the driver dll, it will load at run time
            
            MethodInfo mi = typeof(RemoteObjectsClient).GetMethod("GetObject").MakeGenericMethod(new Type[] { t });
            object driverDisplayRemoteObject = mi.Invoke(c, new object[] { "ID aas as !!!" });
            
            mDriver.GetType().GetMethod("AttachDisplay").Invoke(mDriver, new object[] { driverDisplayRemoteObject });
            return new NewPayLoad("OK", "Done");
        }

        private NewPayLoad Ping()
        {
            Console.WriteLine("Payload - Ping");            
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.AddValue(DateTime.Now.ToString());
            PLRC.ClosePackage();
            return PLRC;
        }

        private NewPayLoad RunAction(NewPayLoad pl)
        {
            Console.WriteLine("Payload - Run Ginger Action");
            string ActionID = pl.GetValueString();
            Console.WriteLine("ActionID - " + ActionID);

            ActionHandler AH = null;
            if (mDriver != null)
            {
                AH = (from x in this.mDriver.ActionHandlers where x.ID == ActionID select x).FirstOrDefault();
            }
            else if (mService != null)
            {
                AH = (from x in this.mService.ActionHandlers where x.ID == ActionID select x).FirstOrDefault();
            }

            if (AH == null)
            {
                Console.WriteLine("Unknown ActionID to handle - " + ActionID);
                throw new Exception("Unknown ActionID to handle - " + ActionID);
            }

            AH.GingerAction = new GingerAction(ActionID);
            AH.GingerAction.ID = ActionID;   // !!!!!!!!!!!!!!!!!!!!! why do we need to keep the ID twice !!

            Console.WriteLine("Found Action Handler, setting parameters");
            List<NewPayLoad> Params = pl.GetListPayLoad();

            Console.WriteLine("Found " + Params.Count + " parameters");
            foreach (NewPayLoad PLP in Params)
            {
                // we get Param name and value
                string Name = PLP.GetValueString();
                string Value = PLP.GetValueString();
                Console.WriteLine("Param " + Name + " = " + Value);
                ActionParam AP = AH.GingerAction.InputParams[Name];
                if (AP != null)
                {
                    AH.GingerAction.InputParams[Name].Value = Value;
                }
                else
                {
                    Console.WriteLine("Cannot find input param - " + Name);
                }
            }

            Console.WriteLine("Setting params done");

            //TODO: print to console:  Running Action: GotoURL(URL="aaa");  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // TODO: cache  !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            if (mDriver != null)
            {
                mDriver.BeforeRunAction(AH.GingerAction);
                mDriver.RunAction(AH.GingerAction);
                mDriver.AfterRunAction(AH.GingerAction);
            }
            else if (mService != null)
            {
                mService.BeforeRunAction(AH.GingerAction);
                mService.RunAction(AH.GingerAction);
                mService.AfterRunAction(AH.GingerAction);
            }

            // We send back only item which can change - ExInfo and Output values
            NewPayLoad PLRC = new NewPayLoad("ActionResult");   //TODO: use const
            PLRC.AddValue(AH.GingerAction.ExInfo);
            PLRC.AddValue(AH.GingerAction.Errors);
            PLRC.AddListPayLoad(GetOutpuValuesPayLoad(AH.GingerAction.Output.Values));
            PLRC.ClosePackage();
            return PLRC;
        }

        private NewPayLoad ShutdownNode()
        {
            Console.WriteLine("Payload - Shutdown, start closing");            
            GingerNodeMessage.Invoke(this, eGingerNodeEventType.Shutdown);
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.ClosePackage();
            return PLRC;
        }

        private NewPayLoad CloseDriver()
        {
            Console.WriteLine("Payload - Close Driver");
            mDriver.CloseDriver();
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.ClosePackage();
            return PLRC;
        }

        private NewPayLoad StartDriver()
        {
            Console.WriteLine("Payload - Start Driver");
            mDriver.StartDriver();
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.ClosePackage();
            return PLRC;
        }

        internal List<NewPayLoad> GetOutpuValuesPayLoad(List<ActionOutputValue> AOVs)
        {
            List<NewPayLoad> OutputValuesPayLoad = new List<NewPayLoad>();
            foreach (ActionOutputValue AOV in AOVs)
            {

                NewPayLoad PLO = new NewPayLoad(SocketMessages.ActionOutputValue);  // Just keep it small size, TODO: use const
                PLO.AddValue(AOV.Param);
                PLO.AddEnumValue(AOV.GetParamType());
                switch (AOV.GetParamType())
                {
                    case ActionOutputValue.OutputValueType.String:
                        PLO.AddValue(AOV.ValueString);
                        break;
                    case ActionOutputValue.OutputValueType.ByteArray:
                        PLO.AddBytes(AOV.ValueByteArray);
                        break;
                    default:
                        throw new Exception("Unknown output Value Type - " + AOV.GetParamType());
                }
                PLO.ClosePackage();

                OutputValuesPayLoad.Add(PLO);
            }
            return OutputValuesPayLoad;
        }
    }
}