using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib.ActionInputValueUserControlLib;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.PlugIns;
using GingerCore.Environments;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.Run
{
    public class ExecuteOnPlugin
    {

        // keep list of GNI for Plugin which are session
        static Dictionary<string, GingerNodeInfo> dic = new Dictionary<string, GingerNodeInfo>();

        internal static GingerNodeInfo GetGingerNodeInfoForPluginAction(ActPlugIn actPlugin)
        {
            GingerNodeInfo GNI = GetGingerNodeInfo(actPlugin.PluginId, actPlugin.ServiceId);
            if (GNI == null)
            {
                actPlugin.Error = "GNI not found, Timeout waiting for service to be available in GingerGrid";

            }

            return GNI;
        }



        internal static GingerNodeInfo GetGingerNodeInfo(string PluginId, string ServiceID, ObservableList<DriverConfigParam> DriverConfiguration = null)
        {
            Console.WriteLine("In GetGingerNodeInfoForPluginAction..");

            bool DoStartSession = false;
            bool IsSessionService = WorkSpace.Instance.PlugInsManager.IsSessionService(PluginId, ServiceID);
            GingerNodeInfo gingerNodeInfo;
            string key = PluginId + "." + ServiceID;

            Console.WriteLine("Plugin Key:" + key);

            if (IsSessionService)
            {
                bool found = dic.TryGetValue(key, out gingerNodeInfo);
                if (found)
                {
                    if (gingerNodeInfo.IsAlive())
                    {
                        return gingerNodeInfo;
                    }
                    else
                    {
                        dic.Remove(key);
                    }
                }
            }

            // !!!!!!!!!!!!!!!!!
            // Need to lock the grid until we get GNI
            // temo for now we lock all
            // TODO: improve to do it but without StartService which might be long
            // for now it is working and safe
            lock (WorkSpace.Instance.LocalGingerGrid)
            {
                gingerNodeInfo = GetGingerNode(ServiceID);

                if (gingerNodeInfo == null)
                {
                    // call plugin to start service and wait for ready
                    WorkSpace.Instance.PlugInsManager.StartService(PluginId, ServiceID);

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    while (gingerNodeInfo == null && stopwatch.ElapsedMilliseconds < 30000)  // max 30 seconds for service to start
                    {
                        //Todo: remove thread.sleep

                        Thread.Sleep(500);
                        gingerNodeInfo = GetGingerNode(ServiceID);
                    }
                    if (gingerNodeInfo == null)
                    {

                        return null;
                    }
                }


                if (IsSessionService)
                {
                    DoStartSession = true;
                }
                else
                {
                    gingerNodeInfo.Status = GingerNodeInfo.eStatus.Reserved;
                }
            }

            // keep the proxy on agent
            GingerNodeProxy GNP = WorkSpace.Instance.LocalGingerGrid.GetNodeProxy(gingerNodeInfo); // FIXME for remote grid

            Console.WriteLine("Checking for DoStartSession..");

            //TODO: check if service is session start session only once
            if (DoStartSession)
            {
                gingerNodeInfo.Status = GingerNodeInfo.eStatus.Reserved;
                GNP.StartDriver(DriverConfiguration);
                dic.Add(key, gingerNodeInfo);
            }

            return gingerNodeInfo;
        }
        private static GingerNodeInfo GetGingerNode(string ServiceId)
        {
            // TODO: create round robin algorithm or something smarter

            // Menahwile we can the first ready node with least amount of actions so balance across same service
            GingerGrid gingerGrid = WorkSpace.Instance.LocalGingerGrid;

            Console.WriteLine("Number of Nodes found in GingerGrid:" + gingerGrid.NodeList.Count);

            foreach (GingerNodeInfo gingerNodeInfo in gingerGrid.NodeList)
            {
                Console.WriteLine("Name:" + gingerNodeInfo.Name);
                Console.WriteLine("ServiceId:" + gingerNodeInfo.ServiceId);
                Console.WriteLine("Status:" + gingerNodeInfo.Status);
                Console.WriteLine("Host:" + gingerNodeInfo.Host);
                Console.WriteLine("IP:" + gingerNodeInfo.IP);
            }

            Console.WriteLine("Searching for ServiceID=" + ServiceId);

            GingerNodeInfo GNI = (from x in gingerGrid.NodeList
                                  where x.ServiceId == ServiceId
                                       && x.Status == GingerNodeInfo.eStatus.Ready
                                  orderby x.ActionCount
                                  select x).FirstOrDefault();

            if (GNI is null)
            {
                Console.WriteLine("GNI is null");
            }

            return GNI;
        }



        // Use for action which run on Agent - session
        public static void ExecutePlugInActionOnAgent(Agent agent, IActPluginExecution actPlugin)
        {
            // Get the action payload
            // NewPayLoad p = actPlugin.GetActionPayload();
            NewPayLoad p = GeneratePlatformActionPayload(actPlugin, agent);

            // Send the payload to the service
            NewPayLoad RC = agent.GingerNodeProxy.RunAction(p);

            // Pasrse the result
            ParseActionResult(RC, (Act)actPlugin);
        }



        // Use for Actions which run without agent and are of the generic type ActPlugin - 
        internal static void ExecuteActionOnPlugin(ActPlugIn actPlugin, GingerNodeInfo gingerNodeInfo)
        {
            try
            {
                // first verify we have service ready or start service

                Stopwatch st = Stopwatch.StartNew();
                GingerNodeProxy gingerNodeProxy = WorkSpace.Instance.LocalGingerGrid.GetNodeProxy(gingerNodeInfo);  // FIXME for remote grid

                //!!!!!!!!!!!!! TODO: check if null set err

                NewPayLoad p = CreateActionPayload(actPlugin);
                NewPayLoad RC = gingerNodeProxy.RunAction(p);

                // release the node as soon as the result came in
                bool IsSessionService = WorkSpace.Instance.PlugInsManager.IsSessionService(actPlugin.PluginId, gingerNodeInfo.ServiceId);
                if (!IsSessionService)
                {
                    // standalone plugin action release the node
                    gingerNodeInfo.Status = GingerNodeInfo.eStatus.Ready;
                }
                ParseActionResult(RC, actPlugin);

                gingerNodeInfo.IncreaseActionCount();

                st.Stop();
                long millis = st.ElapsedMilliseconds;
                actPlugin.ExInfo += Environment.NewLine + "Elapsed: " + millis + "ms";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                string errorMessage = "";
                if (gingerNodeInfo == null)
                {
                    errorMessage += "Cannot find GingerNodeInfo in service grid for: " + ((ActPlugIn)actPlugin).PluginId + ", Service " + ((ActPlugIn)actPlugin).ServiceId + Environment.NewLine;
                }
                errorMessage += "Error while executing Plugin Service action " + Environment.NewLine;
                errorMessage += ex.Message;
                actPlugin.Error = errorMessage;
            }

        }

        public static void ParseActionResult(NewPayLoad RC, Act actPlugin)
        {
            // After we send it we parse the driver response
            if (RC.Name == "ActionResult")
            {
                // We read the ExInfo, Err and output params
                actPlugin.ExInfo = RC.GetValueString();
                string error = RC.GetValueString();
                if (!string.IsNullOrEmpty(error))
                {
                    actPlugin.Error += error;
                }

                List<NewPayLoad> OutpuValues = RC.GetListPayLoad();
                foreach (NewPayLoad OPL in OutpuValues)
                {
                    //    //TODO: change to use PL AddValueByObjectType

                    //    // it is param name, type and value
                    string PName = OPL.GetValueString();
                    string path = OPL.GetValueString();
                    string mOutputValueType = OPL.GetValueEnum();

                    switch (mOutputValueType)
                    {
                        case nameof(Amdocs.Ginger.CoreNET.RunLib.NodeActionOutputValue.OutputValueType.String):
                            string stringValue = OPL.GetValueString();
                            actPlugin.AddOrUpdateReturnParamActualWithPath(PName, stringValue, path);
                            break;
                        case nameof(Amdocs.Ginger.CoreNET.RunLib.NodeActionOutputValue.OutputValueType.ByteArray):
                            byte[] b = OPL.GetBytes();
                            //actPlugin.ReturnValues.Add(new ActReturnValue() { Param = PName, Path= path, Actual = "aaaaaaa" });   //FIXME!!! when act can have values types
                            actPlugin.AddOrUpdateReturnParamActualWithPath(PName, "aaa", path);   //FIXME!!! when act can have values types
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
                actPlugin.Error += Err;
            }
        }

        // Move code to the ActPlugIn and make it impl IACtPlug...
        public static NewPayLoad CreateActionPayload(ActPlugIn ActPlugIn)
        {
            // Here we decompose the GA and create Payload to transfer it to the agent
            NewPayLoad PL = new NewPayLoad("RunAction");
            PL.AddValue(ActPlugIn.ActionId);
            //Add Params
            List<NewPayLoad> Params = new List<NewPayLoad>();

            // if this is the first time the action run it will not have param type
            // so read it from plugin action info
            if (ActPlugIn.InputValues.Count > 0)
            {
                if (ActPlugIn.InputValues[0].ParamType == null)
                {
                    UpdateParamsType(ActPlugIn);
                }
            }

            foreach (ActInputValue AP in ActPlugIn.InputValues)
            {
                // Why we need GA?
                if (AP.Param == "GA") continue;
                // TODO: use const
                NewPayLoad p = new NewPayLoad("P");   // To save network traffic we send just one letter
                p.AddValue(AP.Param);
                if (AP.ParamType == typeof(string))
                {
                    p.AddValue(AP.ValueForDriver.ToString());
                }
                else if (AP.ParamType == typeof(int))
                {
                    p.AddValue(AP.IntValue);
                }
                else if (AP.ParamType == typeof(bool))
                {
                    p.AddValue(AP.BoolValue);
                }
                else if (AP.ParamType == typeof(DynamicListWrapper))
                {
                    p.AddValue(AP.ValueForDriver.ToString());
                }
                else if (AP.ParamType == typeof(EnumParamWrapper))
                {
                    p.AddValue(AP.ValueForDriver.ToString());
                }
                else
                {
                    throw new Exception("Unknown param type to pack: " + AP.ParamType.FullName);
                }
                p.ClosePackage();
                Params.Add(p);
            }
            PL.AddListPayLoad(Params);

            PL.ClosePackage();
            return PL;
        }

        private static void UpdateParamsType(ActPlugIn actPlugIn)
        {
            List<ActionInputValueInfo> paramsList = WorkSpace.Instance.PlugInsManager.GetActionEditInfo(actPlugIn.PluginId, actPlugIn.ServiceId, actPlugIn.ActionId);
            foreach (ActInputValue AP in actPlugIn.InputValues)
            {
                ActionInputValueInfo actionInputValueInfo = (from x in paramsList where x.Param == AP.Param select x).SingleOrDefault();
                AP.ParamType = actionInputValueInfo.ParamType;
            }
        }



        public static void ExecutesScreenShotActionOnAgent(Agent agent, Act act)
        {
            NewPayLoad PL = new NewPayLoad("ScreenshotAction");
            List<NewPayLoad> PLParams = new List<NewPayLoad>();

            NewPayLoad AIVPL = new NewPayLoad("AIV", "WindowsToCapture", act.WindowsToCapture.ToString());
            PLParams.Add(AIVPL);
            PL.AddListPayLoad(PLParams);
            // Get the action payload

            PL.ClosePackage();
            // Send the payload to the service
            NewPayLoad RC = agent.GingerNodeProxy.RunAction(PL);

            if (RC.Name == "ScreenShots")
            {
                List<NewPayLoad> FieldsandParams = RC.GetListPayLoad();

                foreach (NewPayLoad Np in FieldsandParams)
                {
                    string Name = Np.GetValueString();

                    //string base64string = Np.GetValueString();
                    act.AddScreenShot(Name);
                }
            }
            else
            {
                // The RC is not OK when we faced some unexpected exception 
                //TODO: 
                string Err = RC.GetValueString();
                act.Error += Err;
            }

        }


        static NewPayLoad GeneratePlatformActionPayload(IActPluginExecution ACT, Agent agent)
        {

            NewPayLoad PL = new NewPayLoad("RunPlatformAction");
            PL.AddValue(ACT.GetName());
            if (ACT is Act actPlugin)
            {
                List<NewPayLoad> PLParams = new List<NewPayLoad>();

                if (ACT is ActUIElement ActUi)
                {
                    if (ActUi.ElementLocateBy == Common.UIElement.eLocateBy.POMElement)
                    {
                        NewPayLoad PomPayload = GetPOMPayload(ref ActUi, agent.ProjEnvironment, agent.BusinessFlow);
                        PLParams.Add(PomPayload);
                    }
                }

                string acttype = actPlugin.GetType().FullName + "+Fields";

               
                foreach (FieldInfo FI in Type.GetType(acttype).GetFields())
                {
                    string Name = FI.Name;
                    string Value = actPlugin.GetOrCreateInputParam(Name).ValueForDriver;

                    if (string.IsNullOrEmpty(Value))
                    {
                        object Output = ACT.GetType().GetProperty(Name) != null ? ACT.GetType().GetProperty(Name).GetValue(ACT, null) : string.Empty;

                        if (Output != null)
                        {
                            Value = Output.ToString();
                        }
                    }

                    if (!string.IsNullOrEmpty(Value))
                    {
                        NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
                        PLParams.Add(FieldPL);
                    }
                }

                foreach (FieldInfo FI in typeof(Act.Fields).GetFields())
                {
                    string Name = FI.Name;
                    string Value = actPlugin.GetOrCreateInputParam(Name).ValueForDriver;

                    if (string.IsNullOrEmpty(Value))
                    {
                        object Output = ACT.GetType().GetProperty(Name) != null ? ACT.GetType().GetProperty(Name).GetValue(ACT, null) : string.Empty;

                        if (Output != null)
                        {
                            Value = Output.ToString();
                        }
                    }

                    if (!string.IsNullOrEmpty(Value))
                    {
                        NewPayLoad FieldPL = new NewPayLoad("Field", Name, Value);
                        PLParams.Add(FieldPL);
                    }
                }


                foreach (ActInputValue AIV in actPlugin.InputValues)
                {
                    if (!string.IsNullOrEmpty(AIV.ValueForDriver))
                    {
                        NewPayLoad AIVPL = new NewPayLoad("AIV", AIV.Param, AIV.ValueForDriver);
                        PLParams.Add(AIVPL);
                    }
                }




                PL.AddListPayLoad(PLParams);
            }
            PL.ClosePackage();

            return PL;
        }

        private static NewPayLoad GetPOMPayload(ref ActUIElement actUi, ProjEnvironment projEnvironment, BusinessFlow businessFlow)
        {
            NewPayLoad PL = new NewPayLoad("POMPayload");
           

            string[] pOMandElementGUIDs = actUi.ElementLocateValue.ToString().Split('_');
            Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
            ApplicationPOMModel currentPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);
            if (currentPOM == null)
            {
                actUi.ExInfo = string.Format("Failed to find the mapped element Page Objects Model with GUID '{0}'", selectedPOMGUID.ToString());
                return null;
            }

            {
                Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                ElementInfo selectedPOMElement = (ElementInfo)currentPOM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();

                PL.AddValue(selectedPOMElement.ElementTypeEnum.ToString());
                if (selectedPOMElement == null)
                {
                    actUi.ExInfo = string.Format("Failed to find the mapped element with GUID '{0}' inside the Page Objects Model", selectedPOMElement.ToString());
                    return null;
                }
                else
                {
                    List<NewPayLoad> switchframpayload = new List<NewPayLoad>();

                    if (selectedPOMElement.Path != null)
                    {
                        string[] spliter = new string[] { "," };
                        string[] iframesPathes = selectedPOMElement.Path.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string iframePath in iframesPathes)
                        {
                            NewPayLoad FieldPL = new NewPayLoad("Frame-Xpath", iframePath);
                            switchframpayload.Add(FieldPL);

                        }
                    }
                    PL.AddListPayLoad(switchframpayload);


                    //adding all locators from POM
                    List<NewPayLoad> LocatorsPayload = new List<NewPayLoad>();
                    foreach (ElementLocator locator in selectedPOMElement.Locators.Where(x => x.Active == true).ToList())
                    {
                        NewPayLoad LocatorPayload;
                        string locateValue;
                        if (locator.IsAutoLearned)
                        {

                            locateValue = locator.LocateValue;
                        }
                        else
                        {
                            ElementLocator evaluatedLocator = locator.CreateInstance() as ElementLocator;
                            GingerCore.ValueExpression VE = new GingerCore.ValueExpression(projEnvironment, businessFlow);
                            locateValue =  VE.Calculate(evaluatedLocator.LocateValue);
                  
                        }
                        LocatorPayload = new NewPayLoad("Locator", locator.LocateBy.ToString(), locateValue);
                        LocatorsPayload.Add(LocatorPayload);
                    }
                    PL.AddListPayLoad(LocatorsPayload);
                }
                PL.ClosePackage();
                return PL;
            }
        }
    }
}