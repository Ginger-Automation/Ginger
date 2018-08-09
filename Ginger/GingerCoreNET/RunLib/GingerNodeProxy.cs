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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
using GingerPlugInsNET.ActionsLib;
using GingerPlugInsNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static GingerPlugInsNET.ActionsLib.ActionOutputValue;

namespace GingerCoreNET.RunLib
{
    public class GingerNodeProxy
    {
        public GingerGrid GingerGrid { get; set; }

        GingerNodeInfo mGingerNodeInfo;

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

        // C#

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

        

        public void Reserve()
        {
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

        public void RunAction(GingerAction GA)
        {
            // Here we decompose the GA and create Payload to transfer it to the agent
            NewPayLoad PL = new NewPayLoad("RunAction");
            PL.AddValue(GA.ID);
            List<NewPayLoad> Params = new List<NewPayLoad>();
            foreach (ActionParam AP in GA.InputParams.Values)
            {
                // TODO: use const
                NewPayLoad p = new NewPayLoad("P");   // To save network trafic we send just one letter
                p.AddValue(AP.Name);
                p.AddValue(AP.Value.ToString());
                p.ClosePackage();
                Params.Add(p);
            }

            PL.AddListPayLoad(Params);
            PL.ClosePackage();

            // TODO: use function which goes to local grid or remote grid
            NewPayLoad RC = SendRequestPayLoad(PL);

            // After we send it we parse the driver response

            if (RC.Name == "ActionResult")
            {
                // We read the ExInfo, Err and output params
                GA.ExInfo = RC.GetValueString();
                string Error = RC.GetValueString();
                if (!string.IsNullOrEmpty(Error))
                {
                    GA.AddError("Driver", RC.GetValueString());   // We need to get Error even if Payload is OK - since it might be in
                }

                List<NewPayLoad> OutpuValues = RC.GetListPayLoad();
                foreach (NewPayLoad OPL in OutpuValues)
                {
                    //TODO: change to use PL AddValueByObjectType

                    // it is param name, type and value
                    string PName = OPL.GetValueString();
                    string mOutputValueType = OPL.GetValueEnum();

                    switch (mOutputValueType)
                    {
                        case nameof(OutputValueType.String):
                            string v = OPL.GetValueString();
                            GA.Output.Values.Add(new ActionOutputValue() { Param = PName, ValueString = v });
                            break;
                        case nameof(OutputValueType.ByteArray):
                            byte[] b = OPL.GetBytes();
                            GA.Output.Values.Add(new ActionOutputValue() { Param = PName, ValueByteArray = b });
                            break;
                        default:
                            throw new Exception("Unknown param type: " + mOutputValueType);
                    }
                }
            }
            else
            {
                // The RC is not OK when we faced some unexpected exception 
                //TODO: 
                string Err = RC.GetValueString();
                GA.AddError("RunAction", Err);
            }
        }

        private NewPayLoad SendRequestPayLoad(NewPayLoad pL)
        {
            
            // if local grid use
            return GingerGrid.SendRequestPayLoad(mGingerNodeInfo.SessionID, pL);
            // else use remote grid
            
        }

        public void StartDriver()
        {
            //TODO: get return code - based on it set status if running OK
            NewPayLoad PL = new NewPayLoad("StartDriver");
            PL.ClosePackage();
            SendRequestPayLoad(PL);
        }

        public void CloseDriver()
        {
            NewPayLoad PL = new NewPayLoad("CloseDriver");
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

        //internal void RunAction(Act act)
        //{
        //    // We can create the Payload here and avoid creating GingerAction 

        //    try
        //    {
        //        GingerAction GA = new GingerAction(act.ID);
        //        if (string.IsNullOrEmpty(GA.ID))
        //        {
        //            throw new Exception("Ginger Action ID cannot be empty - " + act.Description);
        //        }

        //        // Copy the parameters
        //        foreach (ActInputValue AP in act.InputValues)
        //        {
        //            GA.InputParams[AP.Param].Value = AP.ValueForDriver;
        //        }

        //        RunAction(GA);

        //        act.Error = GA.Errors;
        //        act.ExInfo = GA.ExInfo;

        //        act.AddNewReturnParams = true;
        //        //Copy output values
        //        foreach (ActionOutputValue AOV in GA.Output.Values)
        //        {
        //            act.AddOrUpdateReturnParamActual(AOV.Param, AOV.ValueString);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        act.Status = eRunStatus.Failed;
        //        act.Error += ex.Message;
        //    }
        //}
    }
}