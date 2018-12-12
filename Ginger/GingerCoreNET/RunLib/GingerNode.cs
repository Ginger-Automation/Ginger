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
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core;
using GingerCoreNET.Drivers.CommunicationProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace GingerCoreNET.DriversLib
{
    //This class is for end driver side communication - the GingerNodeProxy will connect to it and send commands
    public class GingerNode
    {                
        private object mService;   // one service per GingerNode, can be any class object marked with [GingerService] Attr like:  [GingerService("MyDriver", "My driver who can speak")]
        private string mServiceID;

        // We use Hub client to send Register/UnRegister message - to GingerGrid manager
        GingerSocketClient2 mHubClient;
        Guid mSessionID;


        public GingerNode(object gingerServiceObject)
        {
            mService = gingerServiceObject;

            GingerServiceAttribute attr = (GingerServiceAttribute)Attribute.GetCustomAttribute(mService.GetType(), typeof(GingerServiceAttribute), false);
            mServiceID = attr.Id;
        }

        public enum eGingerNodeEventType
        {
            Started,
            Shutdown
        }

        public delegate void GingerNodeMessageEvent(GingerNode gingerNode, eGingerNodeEventType GingerNodeEventType);
        public event GingerNodeMessageEvent GingerNodeMessage;

        public void StartGingerNode(string configFileName)
        {
            NodeConfigFile nodeConfigFile = new NodeConfigFile(configFileName);            
            StartGingerNode(nodeConfigFile.Name, nodeConfigFile.GingerGridHost, nodeConfigFile.GingerGridPort);
        }
        
        public void StartGingerNode(string Name, string HubIP, int HubPort)
        {
            Console.WriteLine("Starting Ginger Node");
            Console.WriteLine("ServiceID: " + mServiceID); 

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
            
            //Register the service in GG
            NewPayLoad PLRegister = new NewPayLoad(SocketMessages.Register);
            PLRegister.AddValue(Name);
            PLRegister.AddValue(mServiceID);
            PLRegister.AddValue(OSVersion);  
            PLRegister.AddValue(MachineName); 
            PLRegister.AddValue(IP);            
            PLRegister.ClosePackage();
            NewPayLoad RC = mHubClient.SendRequestPayLoad(PLRegister);
                        
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

        // Being used in plugin - DO NOT Remove will show 0 ref
        public void NotifyNodeClosing()
        {
            Console.WriteLine("GingerNode is closing");
            //Unregister the service in GG
            NewPayLoad PLUnregister = new NewPayLoad(SocketMessages.Unregister);
            PLUnregister.AddValue(mSessionID);
            PLUnregister.ClosePackage();
            NewPayLoad RC = mHubClient.SendRequestPayLoad(PLUnregister);

            if (RC.Name == "OK")
            {
                Console.WriteLine("GingerNode removed successsfully from Grid");

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
            // DO NOT remove will be fixed back later

            //string host = pl.GetValueString();
            //int port = pl.GetValueInt();
            //RemoteObjectsClient c = new RemoteObjectsClient();            
            //c.Connect(host, port);

            ////TODO: fix hard coded !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //Assembly driverAssembly = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerWebServicesPlugin\bin\Debug\netstandard2.0\WebServices.GingerPlugin.dll");
            //Type t = driverAssembly.GetType("Amdocs.Ginger.WebServices.IWebServicesDriverDisplay");

            //// We do all using reflection, since we don't have ref to the driver dll, it will load at run time

            //MethodInfo mi = typeof(RemoteObjectsClient).GetMethod("GetObject").MakeGenericMethod(new Type[] { t });
            //object driverDisplayRemoteObject = mi.Invoke(c, new object[] { "ID aas as !!!" });

            //mDriver.GetType().GetMethod("AttachDisplay").Invoke(mDriver, new object[] { driverDisplayRemoteObject });
            //return new NewPayLoad("OK", "Done");
            return null;
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
            ScanService();

            Console.WriteLine(">>> Payload - Run Ginger Action");
            string ActionID = pl.GetValueString();
            Console.WriteLine("Received RunAction, ActionID - " + ActionID);
            
            ActionHandler AH = (from x in mServiceActions where x.ServiceActionId == ActionID select x).FirstOrDefault();
            if (AH == null)
            {
                Console.WriteLine("Unknown ActionID to handle - " + ActionID);
                throw new Exception("Unknown ActionID to handle - " + ActionID);
            }

            //Conver the Payload to GingerAction
            NodeGingerAction nodeGingerAction = new NodeGingerAction();
            AH.NodeGingerAction = nodeGingerAction;            
            Console.WriteLine("Found Action Handler, setting parameters");

            // setting parameters
            List<NewPayLoad> Params = pl.GetListPayLoad();            
            Console.WriteLine("Found " + Params.Count + " parameters");
            ActionInputParams actionInputParams = new ActionInputParams();

            foreach (NewPayLoad PLP in Params)
            {
                // we get Param name and value
                string Name = PLP.GetValueString();
                object Value = PLP.GetValueByObjectType();
                Console.WriteLine("Param " + Name + " = " + Value);
                ActionParam AP = actionInputParams[Name];
                if (AP != null)
                {
                    actionInputParams[Name].Value = Value;
                }
                else
                {
                    Console.WriteLine("Cannot find input param - " + Name);
                }
            }

            // TODO: add hookd for before and after using interface
            //if (IBeforeAfterAction != null)            
            //    mService.BeforeRunAction(AH.GingerAction);
            // mService.RunAction(AH.GingerAction);
            
            ExecuteMethod(AH, actionInputParams, nodeGingerAction);
            
            // We send back only item which can change - ExInfo and Output values
            NewPayLoad PLRC = new NewPayLoad("ActionResult");   //TODO: use const
            PLRC.AddValue(nodeGingerAction.ExInfo);
            PLRC.AddValue(nodeGingerAction.Errors);            
            PLRC.AddListPayLoad(GetOutpuValuesPayLoad(nodeGingerAction.Output.OutputValues));
            PLRC.ClosePackage();
            return PLRC;            
        }

        private List<NewPayLoad> GetOutpuValuesPayLoad(object values)
        {
            throw new NotImplementedException();
        }

        public static void ExecuteMethod(ActionHandler AH, ActionInputParams p, NodeGingerAction GA)  
        {            
            try
            {                    
                ParameterInfo[] PIs = AH.MethodInfo.GetParameters();

                object[] parameters = new object[PIs.Count()];

                int paramnum = 0;
                foreach (ParameterInfo PI in PIs)
                {
                    if (paramnum == 0)
                    {
                        // verify param 0 is GA
                        parameters[0] = GA;
                    }
                    else
                    {
                        object ActionParam = p[PI.Name];
                        if (ActionParam != null)
                        {
                            object val = null;
                            // For each type we need to get the val correctly so the function will get it right

                            if (PI.ParameterType == typeof(string))
                            {
                                val = p[PI.Name].Value;
                            }
                            else if (PI.ParameterType.IsEnum)
                            {
                                if (p[PI.Name].Value != null)
                                {
                                    val = Enum.Parse(PI.ParameterType, p[PI.Name].Value.ToString());
                                }
                                else
                                {
                                    // TODO: err or check if it is nullable enum
                                }
                            }
                            else if (PI.ParameterType == typeof(Int32))
                            {
                                val = p[PI.Name].GetValueAsInt();
                            }
                            else if (PI.ParameterType.IsGenericType && PI.ParameterType.GetGenericTypeDefinition() == typeof(List<>))
                            {                                
                                // This is List of objects
                                Type itemType = PI.ParameterType.GetGenericArguments()[0];  // List item type                               
                                Type listType = typeof(List<>).MakeGenericType(itemType); // List with the item type
                                // val = Activator.CreateInstance(listType);
                                val = JSONHelper.DeserializeObject(p[PI.Name].Value.ToString(), listType);
                            }                            
                            else
                            {
                                val = p[PI.Name].Value;
                            }

                            parameters[paramnum] = val;
                        }
                        else
                        {
                            //check if param is optional then ignore
                            if (!PI.HasDefaultValue)
                            {
                                throw new Exception("GingerAction is Missing Param/Value for ActionParam - " + PI.Name);
                            }
                            else
                            {
                                // from here on all params are optional...
                            }
                        }
                    }
                    paramnum++;
                }
                AH.MethodInfo.Invoke(AH.Instance, parameters);   // here is where we call the action directly with the relevant parameters                                
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += Environment.NewLine + ex.InnerException.Message;
                }
                GA.AddError("Error when trying to invoke: " + AH.ServiceActionId + " - " + message);
            }
        }

        
        List<ActionHandler> mServiceActions = null;
        private void ScanService()
        {
            // Scan once and cache
            if (mServiceActions != null)
            {
                return;
            }
            Console.WriteLine("Scanning Service: " + mService.GetType().FullName) ;
            mServiceActions = new List<ActionHandler>();

            // Register all actions which have 'GingerAction' attribute
            Type t = mService.GetType();
            var v = t.GetMethods(); //BindingFlags.Public  BindingFlags.DeclaredOnly);
            foreach (MethodInfo MI in v)
            {
                GingerActionAttribute GAA = (GingerActionAttribute)MI.GetCustomAttribute(typeof(GingerActionAttribute));
                if (GAA != null)
                {
                    ActionHandler AH = new ActionHandler();
                    AH.ServiceActionId = GAA.Id;                    
                    AH.MethodInfo = MI;
                    AH.Instance = mService;
                    mServiceActions.Add(AH);

                    Console.WriteLine("Found Action: " + AH.ServiceActionId);
                }
            }
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
            Console.WriteLine("Payload - Stop Session");
            ((IServiceSession)mService).StopSession();
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.ClosePackage();
            return PLRC;            
        }

        private NewPayLoad StartDriver()
        {
            Console.WriteLine("Payload - Start Session");
            ((IServiceSession)mService).StartSession();
            NewPayLoad PLRC = new NewPayLoad("OK");
            PLRC.ClosePackage();
            return PLRC;
            
        }

        internal List<NewPayLoad> GetOutpuValuesPayLoad(List<NodeActionOutputValue> AOVs)
        {
            List<NewPayLoad> OutputValuesPayLoad = new List<NewPayLoad>();
            foreach (NodeActionOutputValue AOV in AOVs)
            {
                NewPayLoad PLO = new NewPayLoad(SocketMessages.ActionOutputValue);  
                PLO.AddValue(AOV.Param);
                PLO.AddValue(AOV.Path);
                PLO.AddEnumValue(AOV.GetParamType());
                switch (AOV.GetParamType())
                {
                    case NodeActionOutputValue.OutputValueType.String:
                        PLO.AddValue(AOV.ValueString);
                        break;
                    case NodeActionOutputValue.OutputValueType.ByteArray:
                        PLO.AddBytes(AOV.ValueByteArray);
                        break;
                        // TODO: add other types
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