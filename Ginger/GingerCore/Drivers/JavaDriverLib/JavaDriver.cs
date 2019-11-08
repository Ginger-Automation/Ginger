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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.Java;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Threading;
using GingerCore.Platforms.PlatformsInfo;

namespace GingerCore.Drivers.JavaDriverLib
{
    public class JavaDriver :DriverBase, IWindowExplorer, IVisualTestingDriver, Amdocs.Ginger.Plugin.Core.IRecord
    {
        [UserConfigured]
        [UserConfiguredDefault("127.0.0.1")]  // Local host 
        [UserConfiguredDescription("Location of Agent - leave default or set the IP address of the remote machine")]
        public string JavaAgentHost { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("8888")]  // Local host 
        [UserConfiguredDescription("JavaAgent Host Port - default is 8888")]
        public int JavaAgentPort { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("120")]  // Local host 
        [UserConfiguredDescription("Communication Timeout - default is 120 seconds")]
        public int CommunicationTimout { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("120")]  // Local host 
        [UserConfiguredDescription("Command Timeout - default is 120 seconds")]
        public int CommandTimeout { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("30")]  // Local host 
        [UserConfiguredDescription("Implicit wait is to tell driver to sync for a certain amount of time when trying to find an element, if they are not immediately available")]
        public int ImplicitWait { get; set; }


        private IPEndPoint serverAddress;
        TcpClient clientSocket;
        private bool mConnected = false;
        private bool IsTryingToConnect;
        private Boolean mIsResetTimeout = false;
        public bool LogCommunication { get; set; }
        private DispatcherTimer mGetRecordingTimer;

        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        public JavaDriver(BusinessFlow BF)
        {
            BusinessFlow = BF;
        }

        public enum CommandType
        {
            AgentOperation,
            WindowExplorerOperation,
            WidgetAction,
            SwingElementAction
        }

        public enum AgentOperationType
        {
            AgentConfig,
            GetVersion,
            SetCommandTimeout
        }

        public enum WindowExplorerOperationType
        {
            GetCurrentWindowTitle,
            Highlight,
            GetAllWindows,
            GetActiveWindow,
            GetElementProperties,
            GetVisibleElements,
            GetElementChildren,
            HighLightElement,
            GetCurrentWindowVisibleControls,
            GetContainerControls,
            GetEditorChildrens,
            GetComponentFromCursor,
            Echo,
            GetProperties,
            GetOptionalValuesList,
            LocateElement,
            UnHighlight
        }
        public override void StartDriver()
        {
            if (JavaAgentHost == null || JavaAgentHost.Length == 0)
            {
                Reporter.ToLog(eLogLevel.WARN, "Missing JavaAgentHost config value- Please verify Agent config parameter JavaAgentHost is not empty");
                ErrorMessageFromDriver = "Missing JavaAgentHost config value- Please verify Agent config parameter JavaAgentHost is not empty";
                return;
            }

            serverAddress = new IPEndPoint(IPAddress.Parse(JavaAgentHost), JavaAgentPort);

            IsTryingToConnect = true;
            ConnectToJavaDriver();
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);

            while (IsTryingToConnect)
            {
                General.DoEvents();
                Thread.Sleep(100);
            }
            if (mConnected)
            {
                //Set Agent Configuration
                PayLoad plAC = new PayLoad(CommandType.AgentOperation.ToString());
                plAC.AddEnumValue(AgentOperationType.AgentConfig);
                plAC.AddValue(CommandTimeout);
                plAC.AddValue(ImplicitWait);
                plAC.ClosePackage();

                PayLoad rcAC = Send(plAC);
                string status = rcAC.GetValueString();

                //TODO: do check version with server version make sure protocol match - should be same Gigner version on both sides                
                if (status!= "Done")
                {
                    //TODO: Err
                }

                //Get Ginger Agent Version
                PayLoad pl = new PayLoad(CommandType.AgentOperation.ToString());
                pl.AddEnumValue(AgentOperationType.GetVersion);
                pl.ClosePackage();

                PayLoad rc = Send(pl);
                string s = rc.GetValueString();

                //TODO: do check version with server version make sure protocol match - should be same Gigner version on both sides                
                if (s != "v2.0.0")
                {
                    //TODO: Err
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Failed to connect Java Agent");
                ErrorMessageFromDriver = "Failed to connect Java Agent";
            }
        }


        private void ConnectToJavaDriver()
        {
            clientSocket = new TcpClient();
            clientSocket.ReceiveTimeout = CommunicationTimout * 1000; ;
            if (CommunicationTimout == 0) CommunicationTimout = 120;
            clientSocket.SendTimeout = CommunicationTimout * 1000; ;
            clientSocket.ReceiveBufferSize = 1000000;
            clientSocket.SendBufferSize = 1000000;
            clientSocket.NoDelay = true;

            // Do we need it on task? this cause some datagram errs when running from unit test
            Task t = Task.Factory.StartNew(() =>
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                // We try to connect for specified driver load wait time
                while (!cancelAgentLoading && st.ElapsedMilliseconds < DriverLoadWaitingTime * 1000)
                {
                    try
                    {
                        General.DoEvents();
                        //Will go to catch if agent is not ready
                        clientSocket.Connect(serverAddress);
                        if (SocketConnected(clientSocket))
                        {
                            mConnected = true;
                            //TODO: raise Propertychanged event on IsConnected
                            //SetConfig();
                            IsTryingToConnect = false;
                            return;
                        }
                    }
                    //TODO: catch exception of socket not all..         
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        Thread.Sleep(500);
                    }
                }
                //Connect Failed after x retry...   
                IsTryingToConnect = false;
            });
        }

        private Boolean Reconnect()
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Trying to reconnect Java Agent");
            try
            {
                clientSocket.Connect(serverAddress);
                mConnected = true;
                return true;
            }
            //TODO: catch exception of socket not all..         
            catch (Exception)
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public PayLoad Send(PayLoad pl)
        {
            try
            {
                if (!mConnected)
                {
                    return ErrorPayLoadMessage("Failed to connect to Java agent");
                }
                    

                // If connection failed then try to reconnect and if reconnect failed then go out of here
                if (!SocketConnected(clientSocket))
                {
                    if (!Reconnect())
                    {
                        return ErrorPayLoadMessage("The connection to Ginger Java Agent was closed and reconnect failed");
                    }
                }

                byte[] bytes = pl.GetPackage();
                NetworkStream ns = clientSocket.GetStream();
                clientSocket.Client.Send(bytes);
                ns.Flush();

                // speed is important so we do the shift with what we know, so it is super fast
                byte[] rcvLenBytesBB = new byte[4];

                //TODO: wait till max comm timeout   
                TimeSpan TD = new TimeSpan();

                while (!ns.DataAvailable && TD.TotalSeconds < CommunicationTimout)
                {
                    //TODO: adaptive sleep
                    General.DoEvents();
                    Thread.Sleep(10);
                    if (!SocketConnected(clientSocket))
                    {
                        return ErrorPayLoadMessage("ERROR | Lost connection or Not connected to Ginger Agent!!!");
                    }
                    //TODO: J.G: If current run stopped then notify Java Driver 
                    //if (PreviousRunStopped)
                    //{
                    //    ns.Flush();
                    //    return PayLoad.Error("Action was stopped");
                    //    // ns.Flush();
                    //    //break;
                    //}                    
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

                PayLoad plRC = new PayLoad(rcvBytes);
                return plRC;
            }
            catch (Exception e)
            {
                return PayLoad.Error(e.Message);
            }
        }

        private static PayLoad ErrorPayLoadMessage(string message)
        {
            PayLoad payLoad = new PayLoad("ERROR");
            payLoad.AddValue(0);
            payLoad.AddValue(message);
            payLoad.ClosePackage();
            return payLoad;
        }

        bool SocketConnected(TcpClient s)
        {
            if (s == null)
                return false;
            //Keep this part as sometime bad disconnect happened and the below s.connected will still report true!!
            bool part1 = s.Client.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Client.Available == 0);
            if (part1 & part2)
            {
                mConnected = false;
                return false;
            }

            // Check using the socket, not working all the time
            try
            {
                if (!s.Connected)
                {
                    //connection is closed
                    mConnected = false;
                    return false;
                }
                mConnected = true;
                return true;
            }
            catch
            {
                mConnected = false;
                return false;
            }
        }

        public override void CloseDriver()
        {
            try
            {
                cancelAgentLoading = true;
                clientSocket.Client.Shutdown(SocketShutdown.Both);
                if (clientSocket.Client.Connected)
                {
                    clientSocket.Client.Disconnect(true);
                }
                clientSocket.Close();

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close Ginger Java Agent Driver - " + ex.Message);
            }
            finally
            {
                clientSocket = null;
                mConnected = false;
            }
        }

        private PayLoad HandleActUIElement(ActUIElement act)
        {
            PayLoad response;
            if (act.ElementLocateBy.Equals(eLocateBy.POMElement))
            {
                response = HandlePOMElememntExecution(act);
            }
            else if (act.ElementType.Equals(eElementType.Window) && act.ElementAction.Equals(ActUIElement.eElementAction.Switch))
            {
                response = ActUISwitchWindow(act);
            }
            else
            {
                PayLoad PL = act.GetPayLoad();
                response = Send(PL);
            }

            if (!response.IsErrorPayLoad() && !response.IsOK())
            {
                List<KeyValuePair<string, string>> parsedResponse = response.GetParsedResult();
                act.AddOrUpdateReturnParsedParamValue(parsedResponse);
            }
            return response;
        }

        private PayLoad ActUISwitchWindow(ActUIElement act)
        {
            Stopwatch St = new Stopwatch();
            St.Reset();
            St.Start();
            var waitTime = this.ImplicitWait;

            var syncTime = Convert.ToInt32(act.GetInputParamCalculatedValue(ActUIElement.Fields.SyncTime));
            if (syncTime >= 0)
            {
                waitTime = syncTime;
            }

            PayLoad response;
            PayLoad Request = act.GetPayLoad();
            response = Send(Request);

            while (!response.IsOK())
            {
                response = Send(Request);
                if(St.ElapsedMilliseconds > waitTime * 1000)
                {
                    break;
                }
            }
            St.Stop();
            return response;
        }

        private PayLoad HandlePOMElememntExecution(ActUIElement act)
        {
            ObservableList<ElementLocator> locators = new ObservableList<ElementLocator>();

            var pomExcutionUtil = new POMExecutionUtils(act);

            var currentPOM = pomExcutionUtil.GetCurrentPOM();

            ElementInfo currentPOMElementInfo = null;
            if (currentPOM != null)
            {
                currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo();
                locators = currentPOMElementInfo.Locators;
            }

            PayLoad response = null;

            foreach (ElementLocator locator in locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            foreach (var locateElement in locators.Where(x => x.Active == true).ToList())
            {
                PayLoad payLoad = null;
                if (!locateElement.IsAutoLearned)
                {
                    ElementLocator evaluatedLocator = locateElement.CreateInstance() as ElementLocator;
                    ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);
                    evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
                    payLoad = act.GetPayLoad(evaluatedLocator);
                }
                else
                {
                    payLoad = act.GetPayLoad(locateElement);
                }

                response = Send(payLoad);

                //if isErrorPayLoad and Element is not found with current locater
                if (response.IsErrorPayLoad() && response.GetValueInt().Equals(Convert.ToInt32(PayLoad.ErrorCode.ElementNotFound)))
                {
                    if (!locateElement.Equals(locators.LastOrDefault()))
                    {
                        continue;
                    }
                    else
                    {
                        locateElement.StatusError = "Element not found";
                        UpdateRunDetails(act, locateElement, response);
                    }
                }
                else if (response.IsErrorPayLoad())
                {
                    locateElement.StatusError = "Unknown error";
                    UpdateRunDetails(act, locateElement, response);

                    break;
                }
                else
                {
                    locateElement.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    act.ExInfo += locateElement.LocateStatus;
                    break;
                }

            }

            return response;
        }

        private static void UpdateRunDetails(ActUIElement act, ElementLocator locateElement, PayLoad response)
        {
            locateElement.LocateStatus = ElementLocator.eLocateStatus.Failed;
            act.ExInfo += string.Format("'{0}', Error Details: ='{1}'", locateElement.LocateStatus, locateElement.StatusError);
            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

            Reporter.ToLog(eLogLevel.DEBUG, response.GetValueString());
        }

        private void SetCommandTimeoutForAction(int? timeout)
        {
            if (timeout != null && timeout != 0)
            {
                PayLoad PL = new PayLoad(CommandType.AgentOperation.ToString());
                PL.AddEnumValue(AgentOperationType.SetCommandTimeout);
                PL.AddValue((int)timeout);
                PL.ClosePackage();

                PayLoad resp = Send(PL);
                if (resp.IsOK())
                {
                    // This flag will indicate the Driver level timeout was modified and need to be reset for subsequent action.
                    mIsResetTimeout = true;
                }
            }
            else if (mIsResetTimeout)
            {
                PayLoad PL = new PayLoad(CommandType.AgentOperation.ToString());
                PL.AddEnumValue(AgentOperationType.SetCommandTimeout);
                PL.AddValue(CommandTimeout);
                PL.ClosePackage();

                PayLoad resp = Send(PL);
                if (resp.IsOK())
                {
                    //If command timeout is set to driver level timeout then no need to reset it again unless action is overriding it
                    mIsResetTimeout = false;
                }
            }
        }

        public override void RunAction(Act act)
        {
            string actClass = act.GetType().Name;
            PayLoad Response = null;

            SetCommandTimeoutForAction(act.Timeout);

            switch (actClass)
            {
                case "ActUIElement":
                    Response = HandleActUIElement((ActUIElement)act);
                    break;

                case "ActJavaElement":
                    ActJavaElement AJTE = (ActJavaElement)act;
                    Response = RunControlAction(AJTE);
                    break;

                case "ActGenElement":
                    ActGenElement AGE = (ActGenElement)act;
                    Response = RunHTMLControlAction(AGE);
                    break;

                case "ActSwitchWindow":
                    Response = SwitchWindow((ActSwitchWindow)act);
                    break;

                case "ActWindow":
                    Response = ActWindowHandler((ActWindow)act);
                    break;

                case "ActScreenShot":
                    ActScreenShot actSS = (ActScreenShot)act;
                    Response = TakeScreenShots(actSS);
                    break;

                case "ActBrowserElement":
                    Response = HandleJavaBrowserElementAction((ActBrowserElement)act);
                    break;

                case "ActTableElement":
                    Response = RunTableControlAction((ActTableElement)act);
                    break;

                case "ActSmartSync":
                    Response = SmartSyncHandler((ActSmartSync)act);
                    break;

                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }

            if (Response != null)
            {
                if (actClass.Equals("ActUIElement") && act.GetInputParamValue(ActUIElement.Fields.ElementLocateBy).Equals(eLocateBy.POMElement.ToString()))
                {
                    return;
                }
                else
                {
                    SetActionStatusFromResponse(act, Response);
                }
            }
            else
            {
                if (act.Error == null)
                {
                    act.Error = "Failed to get action response from application side";
                    act.ExInfo = "Response object is NULL";
                }
            }
        }

        private PayLoad ActWindowHandler(ActWindow act)
        {
            PayLoad Response = null;

            switch (act.WindowActionType)
            {
                case ActWindow.eWindowActionType.IsExist:
                    PayLoad PL = new PayLoad("WindowAction");
                    PL.AddValue("IsExist");
                    PL.AddValue(act.LocateBy.ToString());
                    PL.AddValue(act.LocateValueCalculated.ToString());
                    PL.ClosePackage();
                    Response = Send(PL);

                    if (!Response.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                        string value = Response.GetValueString();
                        act.AddOrUpdateReturnParamActual("Actual", value);
                    }
                    break;

                case ActWindow.eWindowActionType.Close:
                    PayLoad PLClose = new PayLoad("WindowAction");
                    PLClose.AddValue("CloseWindow");
                    PLClose.AddValue(act.LocateBy.ToString());
                    PLClose.AddValue(act.LocateValueCalculated);
                    PLClose.ClosePackage();
                    Response = Send(PLClose);

                    //TODO: Add a separate action to close the driver if closing the application window
                    //if (!(Response.IsErrorPayLoad()))
                    //{
                    //    CloseDriver();                      
                    //}
                    break;

                default:
                    act.Error = "Unknown Window Action - " + act.WindowActionType;
                    return Response;
            }
            return Response;
        }

        private PayLoad IsElementDisplayed(String LocateBy, String LocateValue)
        {
            PayLoad PL = new PayLoad("isElementDisplayed");
            PL.AddValue(LocateBy);
            PL.AddValue(LocateValue);
            PL.ClosePackage();
            PayLoad Response = Send(PL);
            //TODO: Implement boolean payload instead of using string
            return Response;
        }

        private PayLoad SmartSyncHandler(ActSmartSync act)
        {
            PayLoad PL = IsElementDisplayed(act.LocateBy.ToString(), act.LocateValueCalculated);
            String sResponse = PL.GetValueString();
            Stopwatch st = new Stopwatch();
            int MaxTimeout = 0;
            try
            {
                if (act.WaitTime.HasValue == true)
                {
                    MaxTimeout = act.WaitTime.GetValueOrDefault();
                }
                else if (string.IsNullOrEmpty(act.GetInputParamValue("Value")))
                {
                    MaxTimeout = 5;
                }
                else
                {
                    MaxTimeout = Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
                }
            }
            catch (Exception)
            {
                MaxTimeout = 5;
            }
            switch (act.SmartSyncAction)
            {
                case ActSmartSync.eSmartSyncAction.WaitUntilDisplay:
                    st.Reset();
                    st.Start();
                    while (!(sResponse.Contains("True")))
                    {
                        Thread.Sleep(100);
                        PL = IsElementDisplayed(act.LocateBy.ToString(), act.LocateValueCalculated);
                        sResponse = PL.GetValueString();

                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                        {
                            act.Error = "Smart Sync of WaitUntilDisplay is timeout";
                            break;
                        }
                    }
                    break;

                case ActSmartSync.eSmartSyncAction.WaitUntilDisapear:
                    st.Reset();
                    if (sResponse == null)
                    {
                        return PL;
                    }
                    else
                    {
                        st.Start();
                        while (!(sResponse.Contains("False")))
                        {
                            Thread.Sleep(100);
                            PL = IsElementDisplayed(act.LocateBy.ToString(), act.LocateValueCalculated);
                            sResponse = PL.GetValueString();
                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisapear is timeout";
                                break;
                            }
                        }
                    }
                    break;
            }
            return PL;
        }


        private PayLoad HandleJavaBrowserElementAction(ActBrowserElement actJavaBrowserElement)
        {
            PayLoad resp = null;
            switch (actJavaBrowserElement.ControlAction)
            {
                case ActBrowserElement.eControlAction.InitializeBrowser:

                    PayLoad PL = new PayLoad("InitializeBrowser");
                    PL.AddValue(actJavaBrowserElement.LocateBy.ToString());
                    PL.AddValue(actJavaBrowserElement.LocateValueCalculated);
                    PL.AddValue(actJavaBrowserElement.ImplicitWait);
                    List<string> jsList = new List<string>();
                    jsList.Add(MinifyJS(Properties.Resources.html2canvas));
                    jsList.Add(MinifyJS(Properties.Resources.ArrayBuffer));
                    jsList.Add(MinifyJS(Properties.Resources.PayLoad));
                    jsList.Add(MinifyJS(Properties.Resources.GingerHTMLHelper));
                    jsList.Add(MinifyJS(Properties.Resources.GingerLibXPath));
                    jsList.Add((Properties.Resources.wgxpath_install));
                    jsList.Add(MinifyJS(Properties.Resources.jquery_min));
                    PL.AddValue(jsList);
                    PL.ClosePackage();

                    PayLoad Response = Send(PL);
                    resp = Response;
                    break;

                case ActBrowserElement.eControlAction.GetPageSource:
                    PayLoad PL1 = new PayLoad("GetPageSource");
                    PL1.AddValue(actJavaBrowserElement.LocateBy.ToString());
                    PL1.AddValue(actJavaBrowserElement.LocateValueCalculated);
                    PL1.AddValue(actJavaBrowserElement.ValueForDriver);
                    PL1.ClosePackage();
                    PayLoad Response1 = Send(PL1);

                    if (!Response1.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                        string val = Response1.GetValueString();
                        actJavaBrowserElement.AddOrUpdateReturnParamActual("Actual", val);
                        actJavaBrowserElement.ExInfo = val;
                    }
                    resp = Response1;
                    break;
                case ActBrowserElement.eControlAction.GetPageURL:
                    PayLoad PL2 = new PayLoad("GetPageURL");
                    PL2.AddValue(actJavaBrowserElement.LocateBy.ToString());
                    PL2.AddValue(actJavaBrowserElement.LocateValueCalculated);
                    PL2.AddValue(actJavaBrowserElement.ValueForDriver);
                    PL2.ClosePackage();
                    PayLoad Response2 = Send(PL2);

                    if (!Response2.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                        string val = Response2.GetValueString();
                        actJavaBrowserElement.AddOrUpdateReturnParamActual("Actual", val);
                        actJavaBrowserElement.ExInfo = val;
                    }
                    resp = Response2;
                    break;
                case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                    PayLoad PL3 = new PayLoad("SwitchToDefaultFrame");
                    PL3.AddValue(actJavaBrowserElement.LocateBy.ToString());
                    PL3.AddValue(actJavaBrowserElement.LocateValueCalculated);
                    PL3.AddValue(actJavaBrowserElement.ValueForDriver);
                    PL3.ClosePackage();
                    PayLoad Response3 = Send(PL3);

                    if (!Response3.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                        string val = Response3.GetValueString();
                        actJavaBrowserElement.AddOrUpdateReturnParamActual("Actual", val);
                        actJavaBrowserElement.ExInfo = val;
                    }
                    resp = Response3;
                    break;

                case ActBrowserElement.eControlAction.SwitchFrame:
                    PayLoad PLSwitchFrame = new PayLoad("HTMLElementAction", "SwitchFrame", actJavaBrowserElement.LocateBy.ToString(), actJavaBrowserElement.LocateValueCalculated, actJavaBrowserElement.ValueForDriver);
                    PayLoad ResponseSwitchFrame = Send(PLSwitchFrame);
                    resp = ResponseSwitchFrame;
                    break;

                case ActBrowserElement.eControlAction.RunJavaScript:
                    PayLoad PLRunJS = new PayLoad("RunJavaScript", actJavaBrowserElement.LocateBy.ToString(), actJavaBrowserElement.LocateValueCalculated, actJavaBrowserElement.ValueForDriver);
                    PayLoad ResponseRunJS = Send(PLRunJS);
                    resp = ResponseRunJS;
                    break;
            }
            return resp;
        }

        private void SetActionStatusFromResponse(Act act, PayLoad Response)
        {
            if (Response.IsErrorPayLoad())
            {                            
                string ErrMsg = Response.GetErrorValue();
                act.Error = ErrMsg;
            }
            else if (Response.IsOK())
            {
                string Msg = Response.GetValueString();
                act.ExInfo = Msg;
            }
        }

        public override Act GetCurrentElement()
        {
            return null;
        }

        private PayLoad RunControlAction(ActJavaElement AJTE)
        {

            PayLoad resp = null;
            switch (AJTE.ControlAction)
            {

                //TODO: change all action to look like SetValue
                case ActJavaElement.eControlAction.SetValue:
                    PayLoad PL = new PayLoad("ElementAction", "SetValue", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    PayLoad Response = Send(PL);
                    resp = Response;
                    break;

                case ActJavaElement.eControlAction.Click:
                    PayLoad PLClick = new PayLoad("ElementAction");
                    PLClick.AddValue("Click");
                    PLClick.AddEnumValue(AJTE.WaitforIdle);
                    PLClick.AddValue(AJTE.LocateBy.ToString());
                    PLClick.AddValue(AJTE.LocateValueCalculated);

                    //TODO: fixme to remove value - for menu create something else, need to fix on the Java side too
                    // causing perf issue and wasting string concat for all ops which doesn't need it

                    //Need this value for Menu click action
                    PLClick.AddValue(AJTE.ValueForDriver);
                    //}
                    PLClick.ClosePackage();
                    PayLoad Response2 = Send(PLClick);
                    resp = Response2;
                    break;

                case ActJavaElement.eControlAction.AsyncClick:
                    PayLoad PLAsyncClick = new PayLoad("ElementAction");
                    PLAsyncClick.AddValue("AsyncClick");
                    PLAsyncClick.AddEnumValue(AJTE.WaitforIdle);
                    PLAsyncClick.AddValue(AJTE.LocateBy.ToString());
                    PLAsyncClick.AddValue(AJTE.LocateValueCalculated);
                    //    //Need this value for Menu click action
                    PLAsyncClick.AddValue(AJTE.ValueForDriver);
                    PLAsyncClick.ClosePackage();
                    PayLoad Response2_1 = Send(PLAsyncClick);
                    resp = Response2_1;
                    break;
                case ActJavaElement.eControlAction.MouseClick:
                    PayLoad PLMouseClick = new PayLoad("ElementAction", "MouseClick", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(PLMouseClick);
                    break;
                case ActJavaElement.eControlAction.MousePressRelease:
                    PayLoad PLMousePressRelease = new PayLoad("ElementAction", "MousePressRelease", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(PLMousePressRelease);
                    break;

                case ActJavaElement.eControlAction.DoubleClick:
                    PayLoad PLDoubleClick = new PayLoad("ElementAction", "DoubleClick", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(PLDoubleClick);
                    break;

                case ActJavaElement.eControlAction.WinClick:
                    PayLoad PLWinClick = new PayLoad("ElementAction");
                    PLWinClick.AddValue("WinClick");
                    PLWinClick.AddEnumValue(AJTE.WaitforIdle);
                    PLWinClick.AddValue(AJTE.LocateBy.ToString());
                    PLWinClick.AddValue(AJTE.LocateValueCalculated);
                    //Need this value for Menu click action
                    PLWinClick.AddValue(AJTE.ValueForDriver);
                    PLWinClick.ClosePackage();
                    PayLoad Response2_2 = Send(PLWinClick);
                    resp = Response2_2;
                    break;

                case ActJavaElement.eControlAction.winDoubleClick:
                    PayLoad PLWinDoubleClick = new PayLoad("ElementAction");
                    PLWinDoubleClick.AddValue("winDoubleClick");
                    PLWinDoubleClick.AddEnumValue(AJTE.WaitforIdle);
                    PLWinDoubleClick.AddValue(AJTE.LocateBy.ToString());
                    PLWinDoubleClick.AddValue(AJTE.LocateValueCalculated);
                    PLWinDoubleClick.AddValue(AJTE.ValueForDriver);
                    PLWinDoubleClick.ClosePackage();
                    PayLoad Response2_3 = Send(PLWinDoubleClick);
                    resp = Response2_3;
                    break;

                case ActJavaElement.eControlAction.Toggle:
                    PayLoad PLToggle = new PayLoad("ElementAction");
                    PLToggle.AddValue("Toggle");
                    PLToggle.AddEnumValue(AJTE.WaitforIdle);
                    PLToggle.AddValue(AJTE.LocateBy.ToString());
                    PLToggle.AddValue(AJTE.LocateValueCalculated);
                    PLToggle.AddValue(""); // TODO: remove if not needed
                    PLToggle.ClosePackage();
                    PayLoad toggleRespone = Send(PLToggle);
                    resp = toggleRespone;
                    break;

                case ActJavaElement.eControlAction.Select:
                    PayLoad PLSelect = new PayLoad("ElementAction");
                    PLSelect.AddValue("Select");
                    PLSelect.AddEnumValue(AJTE.WaitforIdle);
                    PLSelect.AddValue(AJTE.LocateBy.ToString());
                    PLSelect.AddValue(AJTE.LocateValueCalculated);
                    //Need to pass the value for Select Tab Action
                    PLSelect.AddValue(AJTE.ValueForDriver);
                    PLSelect.ClosePackage();
                    PayLoad selectRespone = Send(PLSelect);
                    resp = selectRespone;
                    break;
                case ActJavaElement.eControlAction.AsyncSelect:
                    PayLoad PLASelect = new PayLoad("ElementAction");
                    PLASelect.AddValue("AsyncSelect");
                    PLASelect.AddEnumValue(AJTE.WaitforIdle);
                    PLASelect.AddValue(AJTE.LocateBy.ToString());
                    PLASelect.AddValue(AJTE.LocateValueCalculated);
                    //Need to pass the value for Select Tab Action
                    PLASelect.AddValue(AJTE.ValueForDriver);
                    PLASelect.ClosePackage();
                    PayLoad selectARespone = Send(PLASelect);
                    resp = selectARespone;
                    break;
                case ActJavaElement.eControlAction.SelectByIndex:
                    PayLoad PLSelectByIndex = new PayLoad("ElementAction");
                    PLSelectByIndex.AddValue("SelectByIndex");
                    PLSelectByIndex.AddEnumValue(AJTE.WaitforIdle);
                    PLSelectByIndex.AddValue(AJTE.LocateBy.ToString());
                    PLSelectByIndex.AddValue(AJTE.LocateValueCalculated);
                    //Need to pass the value for Select Tab Action
                    PLSelectByIndex.AddValue(AJTE.ValueForDriver);
                    PLSelectByIndex.ClosePackage();
                    PayLoad selectByIndexRespone = Send(PLSelectByIndex);
                    resp = selectByIndexRespone;
                    break;

                case ActJavaElement.eControlAction.GetValueByIndex:
                    PayLoad PLGetValueByIndex = new PayLoad("ElementAction");
                    PLGetValueByIndex.AddValue("GetValueByIndex");
                    PLGetValueByIndex.AddEnumValue(AJTE.WaitforIdle);
                    PLGetValueByIndex.AddValue(AJTE.LocateBy.ToString());
                    PLGetValueByIndex.AddValue(AJTE.LocateValueCalculated);
                    //Need to pass the value for Select Tab Action
                    PLGetValueByIndex.AddValue(AJTE.ValueForDriver);
                    PLGetValueByIndex.ClosePackage();
                    PayLoad ResponsePLGetValueByIndex = Send(PLGetValueByIndex);

                    if (!ResponsePLGetValueByIndex.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse

                        String val = ResponsePLGetValueByIndex.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val);
                        AJTE.ExInfo = val;
                    }
                    resp = ResponsePLGetValueByIndex;
                    break;

                case ActJavaElement.eControlAction.IsChecked:
                    PayLoad PLIsChecked = new PayLoad("ElementAction");
                    PLIsChecked.AddValue("IsChecked");
                    PLIsChecked.AddEnumValue(AJTE.WaitforIdle);
                    PLIsChecked.AddValue(AJTE.LocateBy.ToString());
                    PLIsChecked.AddValue(AJTE.LocateValueCalculated);
                    PLIsChecked.AddValue(AJTE.ValueForDriver);

                    PLIsChecked.ClosePackage();
                    PayLoad ResponsePLIsChecked = Send(PLIsChecked);

                    if (!ResponsePLIsChecked.IsErrorPayLoad())
                    {
                        String val = ResponsePLIsChecked.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val);
                        AJTE.ExInfo = val;
                    }
                    resp = ResponsePLIsChecked;
                    break;

                case ActJavaElement.eControlAction.GetItemCount:
                    PayLoad PLGetItemCount = new PayLoad("ElementAction");
                    PLGetItemCount.AddValue("GetItemCount");
                    PLGetItemCount.AddEnumValue(AJTE.WaitforIdle);
                    PLGetItemCount.AddValue(AJTE.LocateBy.ToString());
                    PLGetItemCount.AddValue(AJTE.LocateValueCalculated);
                    PLGetItemCount.AddValue(""); // TODO: remove if not needed
                    PLGetItemCount.ClosePackage();
                    PayLoad ResponsePLGetItemCount = Send(PLGetItemCount);

                    if (!ResponsePLGetItemCount.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse

                        int val = ResponsePLGetItemCount.GetValueInt();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val + "");

                        AJTE.ExInfo = "Total Items counts returned:" + val;
                    }
                    resp = ResponsePLGetItemCount;
                    break;


                case ActJavaElement.eControlAction.GetValue:
                    PayLoad PLGetValue = new PayLoad("ElementAction");
                    PLGetValue.AddValue("GetValue");
                    PLGetValue.AddEnumValue(AJTE.WaitforIdle);
                    PLGetValue.AddValue(AJTE.LocateBy.ToString());
                    PLGetValue.AddValue(AJTE.LocateValueCalculated);
                    PLGetValue.AddValue(""); // TODO: remove if not needed
                    PLGetValue.ClosePackage();
                    PayLoad Response3 = Send(PLGetValue);

                    if (!Response3.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse

                        List<String> val = Response3.GetListString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val.FirstOrDefault());
                        for (int i = 1; i < val.Count; i++)
                        {

                            AJTE.AddOrUpdateReturnParamActual("Actual" + i.ToString(), val.ElementAt(i));
                        }
                        AJTE.ExInfo = val.FirstOrDefault();
                    }
                    resp = Response3;
                    break;

                case ActJavaElement.eControlAction.GetDialogText:
                    PayLoad PLGetDialogText = new PayLoad("DialogElementAction");
                    PLGetDialogText.AddValue("GetDialogText");
                    PLGetDialogText.AddEnumValue(AJTE.WaitforIdle);
                    PLGetDialogText.AddValue(AJTE.LocateBy.ToString());
                    PLGetDialogText.AddValue(AJTE.LocateValueCalculated);
                    PLGetDialogText.AddValue(""); // TODO: remove if not needed
                    PLGetDialogText.ClosePackage();
                    PayLoad Response4 = Send(PLGetDialogText);
                    if (!Response4.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                        string dialogText = Response4.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", dialogText);
                        AJTE.ExInfo = dialogText;
                    }
                    resp = Response4;
                    break;

                case ActJavaElement.eControlAction.AcceptDialog:
                    PayLoad PLAcceptDialog = new PayLoad("DialogElementAction");
                    PLAcceptDialog.AddValue("AcceptDialog");
                    PLAcceptDialog.AddEnumValue(AJTE.WaitforIdle);
                    PLAcceptDialog.AddValue(AJTE.LocateBy.ToString());
                    PLAcceptDialog.AddValue(AJTE.LocateValueCalculated);
                    PLAcceptDialog.AddValue(""); // TODO: remove if not needed
                    PLAcceptDialog.ClosePackage();
                    PayLoad Response5 = Send(PLAcceptDialog);
                    resp = Response5;
                    break;

                case ActJavaElement.eControlAction.DismissDialog:
                    PayLoad PLDismissDialog = new PayLoad("DialogElementAction");
                    PLDismissDialog.AddValue("DismissDialog");
                    PLDismissDialog.AddEnumValue(AJTE.WaitforIdle);
                    PLDismissDialog.AddValue(AJTE.LocateBy.ToString());
                    PLDismissDialog.AddValue(AJTE.LocateValueCalculated);
                    PLDismissDialog.AddValue(""); // TODO: remove if not needed
                    PLDismissDialog.ClosePackage();
                    PayLoad Response6 = Send(PLDismissDialog);
                    resp = Response6;
                    break;

                case ActJavaElement.eControlAction.GetName:
                    resp = GetJavaElementProperty(AJTE, "NAME");
                    break;

                case ActJavaElement.eControlAction.IsVisible:
                    resp = GetJavaElementProperty(AJTE, "VISIBLE");
                    break;
                case ActJavaElement.eControlAction.IsMandatory:
                    resp = GetJavaElementProperty(AJTE, "MANDATORY");
                    break;
                case ActJavaElement.eControlAction.IsEnabled:

                    resp = GetJavaElementProperty(AJTE, "ENABLED");
                    break;

                case ActJavaElement.eControlAction.ScrollUp:
                    PayLoad SU = new PayLoad("ElementAction");
                    SU.AddValue("ScrollUp");
                    SU.AddEnumValue(AJTE.WaitforIdle);
                    SU.AddValue(AJTE.LocateBy.ToString());
                    SU.AddValue(AJTE.LocateValueCalculated);
                    SU.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
                    SU.ClosePackage();
                    resp = Send(SU);
                    break;

                case ActJavaElement.eControlAction.ScrollDown:
                    PayLoad SD = new PayLoad("ElementAction");
                    SD.AddValue("ScrollDown");
                    SD.AddEnumValue(AJTE.WaitforIdle);
                    SD.AddValue(AJTE.LocateBy.ToString());
                    SD.AddValue(AJTE.LocateValueCalculated);
                    SD.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
                    SD.ClosePackage();
                    resp = Send(SD);
                    break;

                case ActJavaElement.eControlAction.ScrollLeft:
                    PayLoad SL = new PayLoad("ElementAction");
                    SL.AddValue("ScrollLeft");
                    SL.AddEnumValue(AJTE.WaitforIdle);
                    SL.AddValue(AJTE.LocateBy.ToString());
                    SL.AddValue(AJTE.LocateValueCalculated);
                    SL.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
                    SL.ClosePackage();
                    resp = Send(SL);
                    break;

                case ActJavaElement.eControlAction.ScrollRight:
                    PayLoad SR = new PayLoad("ElementAction");
                    SR.AddValue("ScrollRight");
                    SR.AddEnumValue(AJTE.WaitforIdle);
                    SR.AddValue(AJTE.LocateBy.ToString());
                    SR.AddValue(AJTE.LocateValueCalculated);
                    SR.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
                    SR.ClosePackage();
                    resp = Send(SR);
                    break;

                case ActJavaElement.eControlAction.SelectDate:
                    PayLoad SDate = new PayLoad("ElementAction");
                    SDate.AddValue("SetDate");
                    SDate.AddEnumValue(AJTE.WaitforIdle);
                    SDate.AddValue(AJTE.LocateBy.ToString());
                    SDate.AddValue(AJTE.LocateValueCalculated);
                    SDate.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
                    SDate.ClosePackage();
                    resp = Send(SDate);
                    break;

                case ActJavaElement.eControlAction.GetState:
                    PayLoad GetState = new PayLoad("ElementAction");
                    GetState.AddValue("GetState");
                    GetState.AddEnumValue(AJTE.WaitforIdle);
                    GetState.AddValue(AJTE.LocateBy.ToString());
                    GetState.AddValue(AJTE.LocateValueCalculated);
                    GetState.AddValue(""); // TODO: remove if not needed
                    GetState.ClosePackage();
                    PayLoad GetStateResponse = Send(GetState);
                    if (!GetStateResponse.IsErrorPayLoad())
                    {
                        string val = GetStateResponse.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val);
                    }
                    resp = GetStateResponse;
                    break;

                case ActJavaElement.eControlAction.SendKeys:
                    PayLoad SendKeysPL = new PayLoad("ElementAction", "SendKeys", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(SendKeysPL);
                    break;

                case ActJavaElement.eControlAction.SendKeyPressRelease:
                    PayLoad SendKeyPressReleasePL = new PayLoad("ElementAction", "SendKeyPressRelease", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(SendKeyPressReleasePL);
                    break;

                case ActJavaElement.eControlAction.Type:
                    PayLoad TypePL = new PayLoad("ElementAction", "Type", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(TypePL);
                    break;

                case ActJavaElement.eControlAction.SetFocus:
                    PayLoad SetFocusPL = new PayLoad("ElementAction", "SetFocus", AJTE.WaitforIdle, AJTE.LocateBy.ToString(), AJTE.LocateValueCalculated, AJTE.ValueForDriver);
                    resp = Send(SetFocusPL);
                    break;

                default:
                    AJTE.Error = "RunControlAction: Unknown Control Action - " + AJTE.ControlAction;

                    return resp;
            }

            return resp;
        }
        private PayLoad GetJavaElementProperty(ActJavaElement AJTE, String PropertyName)
        {
            PayLoad PLGetControlProperty = new PayLoad("ElementAction");
            PLGetControlProperty.AddValue("GetControlProperty");
            PLGetControlProperty.AddEnumValue(AJTE.WaitforIdle);
            PLGetControlProperty.AddValue(AJTE.LocateBy.ToString());
            PLGetControlProperty.AddValue(AJTE.LocateValueCalculated);
            PLGetControlProperty.AddValue(PropertyName); // TODO: remove if not needed
            PLGetControlProperty.ClosePackage();
            PayLoad ResponseGetControlProperty = Send(PLGetControlProperty);

            if (!ResponseGetControlProperty.IsErrorPayLoad())
            {
                //If the response is not error payload then only we retrieve value from payload.
                // if it is error payload then we read the error at the end, during SetActionStatusFromResponse
                string val = ResponseGetControlProperty.GetValueString();

                AJTE.AddOrUpdateReturnParamActual("Actual", val);
                AJTE.ExInfo = val;
            }
            return ResponseGetControlProperty;
        }


        private PayLoad RunHTMLControlAction(ActGenElement AGE)
        {
            PayLoad resp = null;
            switch (AGE.GenElementAction)
            {
                case ActGenElement.eGenElementAction.SetValue:

                    PayLoad PL = new PayLoad("HTMLElementAction", "SetValue", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad Response = Send(PL);
                    resp = Response;
                    break;
                case ActGenElement.eGenElementAction.SelectFromDropDownByIndex:

                    PayLoad PLSelectDDLByIndex = new PayLoad("HTMLElementAction", "SelectFromDropDownByIndex", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseSelectDDLByIndex = Send(PLSelectDDLByIndex);
                    resp = ResponseSelectDDLByIndex;
                    break;

                case ActGenElement.eGenElementAction.SelectFromDropDown:

                    PayLoad PLSelectDDL = new PayLoad("HTMLElementAction", "SelectFromDropDown", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseSelect = Send(PLSelectDDL);
                    resp = ResponseSelect;
                    break;

                case ActGenElement.eGenElementAction.GetValue:

                    PayLoad PL1 = new PayLoad("HTMLElementAction", "GetValue", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, "");
                    PayLoad Response2 = Send(PL1);
                    resp = Response2;

                    if (!Response2.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse                       
                        String val = Response2.GetValueString();
                        AGE.AddOrUpdateReturnParamActual("Actual", val);
                        AGE.ExInfo = val;
                    }
                    break;
                case ActGenElement.eGenElementAction.Click:

                    PayLoad PLClick = new PayLoad("HTMLElementAction", "Click", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseClick = Send(PLClick);
                    resp = ResponseClick;
                    break;
                case ActGenElement.eGenElementAction.AsyncClick:

                    PayLoad PLAsyncClick = new PayLoad("HTMLElementAction", "AsnycClick", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseAsyncClick = Send(PLAsyncClick);
                    resp = ResponseAsyncClick;
                    break;
                case ActGenElement.eGenElementAction.FireMouseEvent:

                    PayLoad PLFireMouseEvent = new PayLoad("HTMLElementAction", "FireMouseEvent", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseFireMouseEvent = Send(PLFireMouseEvent);
                    resp = ResponseFireMouseEvent;
                    break;

                case ActGenElement.eGenElementAction.FireSpecialEvent:

                    PayLoad PLFireSpecialEvent = new PayLoad("HTMLElementAction", "FireSpecialEvent", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseFireSpecialEvent = Send(PLFireSpecialEvent);
                    resp = ResponseFireSpecialEvent;
                    break;

                case ActGenElement.eGenElementAction.SwitchFrame:

                    PayLoad PLSwitchFrame = new PayLoad("HTMLElementAction", "SwitchFrame", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseSwitchFrame = Send(PLSwitchFrame);
                    resp = ResponseSwitchFrame;
                    break;

                case ActGenElement.eGenElementAction.Visible:

                    PayLoad PLVisible = new PayLoad("HTMLElementAction", "Visible", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseVisible = Send(PLVisible);
                    resp = ResponseVisible;

                    if (!ResponseVisible.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse                     

                        String valVisible = ResponseVisible.GetValueString();

                        AGE.AddOrUpdateReturnParamActual("Actual", valVisible);

                        AGE.ExInfo = valVisible;
                    }
                    break;
                case ActGenElement.eGenElementAction.Enabled:

                    PayLoad PLEnabled = new PayLoad("HTMLElementAction", "Enabled", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseEnabled = Send(PLEnabled);
                    resp = ResponseEnabled;

                    if (!ResponseEnabled.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        String valEnabled = ResponseEnabled.GetValueString();

                        AGE.AddOrUpdateReturnParamActual("Actual", valEnabled);

                        AGE.ExInfo = valEnabled;
                    }
                    break;
                //TODO: Add More actions here

                case ActGenElement.eGenElementAction.RunJavaScript:
                    PayLoad PLRunJS = new PayLoad("RunJavaScript", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseRunJS = Send(PLRunJS);
                    resp = ResponseRunJS;
                    break;
                case ActGenElement.eGenElementAction.ScrollDown:

                    PayLoad PLScrollDown = new PayLoad("HTMLElementAction", "ScrollDown", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseScrollDown = Send(PLScrollDown);
                    resp = ResponseScrollDown;
                    break;

                case ActGenElement.eGenElementAction.ScrollUp:

                    PayLoad PLScrollUp = new PayLoad("HTMLElementAction", "ScrollUp", AGE.LocateBy.ToString(), AGE.LocateValueCalculated, AGE.ValueForDriver);
                    PayLoad ResponseScrollUp = Send(PLScrollUp);
                    resp = ResponseScrollUp;
                    break;

                default:
                    AGE.Error = "Unknown Control Action - " + AGE.GenElementAction;
                    return resp;
            }
            return resp;
        }

        private PayLoad RunTableControlAction(ActTableElement AJTE)
        {
            PayLoad resp = null;
            List<String> Locators = new List<string>();
            List<String> vals = null;
            //New serializer restoring the value to null. So handling it to set it to empty
            if (AJTE.LocateColTitle == null)
            {
                AJTE.LocateColTitle = string.Empty;
            }

            PayLoad PL1 = new PayLoad("TableAction");
            PL1.AddValue(AJTE.ControlAction.ToString());
            PL1.AddValue(AJTE.LocateBy.ToString());
            PL1.AddValue(AJTE.LocateValueCalculated);
            PL1.AddValue(AJTE.ValueForDriver);
            Locators.Add(AJTE.LocateRowType);
            if (AJTE.LocateRowType.ToString() == "Where")
            {
                Locators.Add(AJTE.WhereColSelector.ToString());
                Locators.Add(AJTE.WhereColumnTitle);
                Locators.Add(AJTE.WhereProperty.ToString());
                Locators.Add(AJTE.WhereOperator.ToString());
                Locators.Add(AJTE.GetInputParamCalculatedValue(ActTableElement.Fields.WhereColumnValue));
            }
            else if (AJTE.LocateRowType.ToString() == "Row Number")
            {
                Locators.Add(AJTE.GetInputParamCalculatedValue(ActTableElement.Fields.LocateRowValue));
            }
            switch (AJTE.ControlAction)
            {
                case ActTableElement.eTableAction.SetValue:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.SetFocus:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.Type:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.GetValue:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);

                    if (!resp.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        vals = resp.GetListString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", vals.FirstOrDefault());
                        for (int i = 1; i < vals.Count; i++)
                        {
                            AJTE.AddOrUpdateReturnParamActual("Actual" + i.ToString(), vals.ElementAt(i));
                        }
                        AJTE.ExInfo = vals.FirstOrDefault();
                    }
                    break;
                case ActTableElement.eTableAction.GetRowCount:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);

                    if (!resp.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        int val = resp.GetValueInt();
                        AJTE.AddOrUpdateReturnParamActual("Actual", "" + val);
                        AJTE.ExInfo = "" + val;
                    }
                    break;
                case ActTableElement.eTableAction.GetSelectedRow:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);

                    if (!resp.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        int val = resp.GetValueInt();
                        AJTE.AddOrUpdateReturnParamActual("Actual", "" + val);
                        AJTE.ExInfo = "" + val;
                    }
                    break;
                case ActTableElement.eTableAction.IsCellEnabled:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);

                    if (!resp.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        string val = resp.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", "" + val);
                        AJTE.ExInfo = "" + val;
                    }
                    break;
                case ActTableElement.eTableAction.IsVisible:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);

                    if (!resp.IsErrorPayLoad())
                    {
                        //If the response is not error payload then only we retrieve value from payload.
                        // if it is error payload then we read the error at the end, during SetActionStatusFromResponse 
                        string val = resp.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", "" + val);
                        AJTE.ExInfo = "" + val;
                    }
                    break;
                case ActTableElement.eTableAction.Click:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.DoubleClick:
                case ActTableElement.eTableAction.ActivateCell:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.ActivateRow:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;
                case ActTableElement.eTableAction.AsyncClick:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.WinClick:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.SelectDate:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.MousePressAndRelease:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.SendKeys:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    break;

                case ActTableElement.eTableAction.IsChecked:
                    Locators.Add(AJTE.ColSelectorValue.ToString());
                    Locators.Add(AJTE.LocateColTitle);
                    PL1.AddValue(Locators);
                    PL1.ClosePackage();
                    resp = Send(PL1);
                    if (!resp.IsErrorPayLoad())
                    {
                        String val = resp.GetValueString();
                        AJTE.AddOrUpdateReturnParamActual("Actual", val);
                        AJTE.ExInfo = val;
                    }
                    break;

                default:
                    AJTE.Error = "Unknown Table Action - " + AJTE.ControlAction;

                    break;
            }

            //Must get the value for driver !!
            //temp ugly fix me later
            // string value = AAC.ValueForDriver;
            //TODO: fixme - temp ugly code to handle date

            Stopwatch st = new Stopwatch();
            st.Reset();
            st.Start();
            st.Stop();
            return resp;
        }

        //Adding code for TakeScreenShot
        public PayLoad TakeScreenShots(ActScreenShot actScreenShot)
        {
            PayLoad resp = null;
            switch (actScreenShot.WindowsToCapture)
            {
                case Act.eWindowsToCapture.OnlyActiveWindow:
                case Act.eWindowsToCapture.AllAvailableWindows:
                    String Type = actScreenShot.WindowsToCapture.ToString();
                    PayLoad Request = new PayLoad("TakeScreenShots", Type);
                    resp = Send(Request);
                    List<PayLoad> lstResponse = resp.GetListPayLoad();

                    foreach (PayLoad payLoad in lstResponse)
                    {
                        if (payLoad.IsErrorPayLoad())
                        {
                            
                            String error = payLoad.GetErrorValue();
                            if (error.IndexOf("ERROR: Handle : ") != -1)
                            {
                                String ErrorTitle = error.Replace("ERROR: Handle : ", "");
                                String[] arrWindows = ErrorTitle.Split(new string[] { "##" }, StringSplitOptions.None);
                                UIAComWrapperHelper uiHelper = new UIAComWrapperHelper();
                                List<object> listWindows = uiHelper.GetListOfWindows();
                                foreach (AutomationElement window in listWindows)
                                {
                                    String WindowTitle = "";
                                    WindowTitle = uiHelper.GetWindowInfo(window);
                                    Console.WriteLine(WindowTitle);
                                    //TODO: fixme why we have hard coded  !! ???????????????????????
                                    if (WindowTitle.Contains(arrWindows[0]) || WindowTitle.Contains(arrWindows[0].Replace("&", "")))
                                    {
                                        uiHelper.CurrentWindow = window;
                                        break;
                                    }
                                }
                                int iWinCount = 1;
                                while (iWinCount < arrWindows.Count())
                                {
                                    AutomationElementCollection AEList;
                                    PropertyCondition condDialog = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "window");
                                    AEList = uiHelper.CurrentWindow.FindAll(TreeScope.Children, condDialog);
                                    if (AEList.Count == 0)
                                    {
                                        condDialog = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "pane");
                                        AEList = uiHelper.CurrentWindow.FindAll(TreeScope.Children, condDialog);
                                    }

                                    if (AEList.Count >= 1)
                                    {
                                        bool bFound = false;
                                        for (int i = 0; i < AEList.Count; i++)
                                        {
                                            String WindowTitle = "";
                                            WindowTitle = uiHelper.GetWindowInfo(AEList[i]);
                                            Console.WriteLine(WindowTitle);

                                            if (WindowTitle.Contains(arrWindows[iWinCount]))
                                            {
                                                actScreenShot.AddScreenShot(uiHelper.WindowToBitmap(AEList[i]));
                                                bFound = true;
                                                break;
                                            }
                                        }
                                        if (bFound == false)
                                        {
                                            AutomationElement AE = findPane(uiHelper, arrWindows[iWinCount]);
                                            if (AE != null)
                                            {
                                                uiHelper.CurrentWindow = AE;
                                                actScreenShot.AddScreenShot(uiHelper.GetCurrentWindowBitmap());
                                                break;
                                            }
                                        }
                                    }
                                    iWinCount++;
                                }
                            }
                            else
                            {
                                actScreenShot.Error = "Failed to create Java application screenshot. Error= " + error;
                                Reporter.ToLog(eLogLevel.ERROR, actScreenShot.Error);
                            }
                        }
                        else
                        {
                            try
                            {
                                Byte[] screenShotbytes;

                                if (payLoad.Name == "HTMLScreenShot")
                                {
                                    String sURL = payLoad.GetValueString();
                                    screenShotbytes = Convert.FromBase64String(sURL);
                                }
                                else
                                {
                                    screenShotbytes = payLoad.GetBytes();
                                }

                                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                                Bitmap btmp = (Bitmap)tc.ConvertFrom(screenShotbytes);
                                actScreenShot.AddScreenShot(btmp);
                            }
                            catch (Exception ex)
                            {
                                actScreenShot.Error = "Failed to create Java application HTML Widget screenshot. Error= " + ex.Message;
                                Reporter.ToLog(eLogLevel.ERROR, actScreenShot.Error, ex);
                            }
                        }
                    }
                    break;

                case Act.eWindowsToCapture.DesktopScreen:
                    PayLoad requestWindowTitle = new PayLoad(CommandType.WindowExplorerOperation.ToString());
                    requestWindowTitle.AddEnumValue(WindowExplorerOperationType.GetCurrentWindowTitle);
                    requestWindowTitle.ClosePackage();
                    resp = Send(requestWindowTitle);

                    if (!resp.IsErrorPayLoad())
                    {
                        string title = resp.GetValueString();
                        try
                        {
                            UIAComWrapperHelper uiDriver = new UIAComWrapperHelper();
                            PropertyCondition nameCondition = new PropertyCondition(AutomationElement.NameProperty, title);

                            uiDriver.CurrentWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, nameCondition);
                            if (uiDriver.CurrentWindow == null)
                            {
                                nameCondition = new PropertyCondition(AutomationElement.NameProperty, title.Replace("&", ""));
                                uiDriver.CurrentWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, nameCondition);
                            }

                            if (uiDriver.CurrentWindow != null)
                            {
                                actScreenShot.AddScreenShot(uiDriver.GetCurrentWindowBitmap());
                            }
                        }
                        catch (Exception e)
                        {
                            actScreenShot.ExInfo += "Error capturing desktop screen" + e.Message;
                        }

                    }
                    break;
            }
            return resp;
        }

        private AutomationElement findPane(UIAComWrapperHelper uiHelper, String sTitle)
        {
            PropertyCondition condDialog = new PropertyCondition(AutomationElementIdentifiers.LocalizedControlTypeProperty, "pane");
            AutomationElementCollection AEList = uiHelper.CurrentWindow.FindAll(TreeScope.Children, condDialog);
            AutomationElement AE = null;
            bool sfound = false;
            do
            {
                String WindowTitle = "";
                for (int i = 0; i < AEList.Count; i++)
                {
                    WindowTitle = uiHelper.GetWindowInfo(AEList[i]);
                    Console.WriteLine(WindowTitle);
                    uiHelper.CurrentWindow = AEList[i];
                    if (WindowTitle.Contains(sTitle))
                    {
                        AE = AEList[i];
                        sfound = true;
                        break;
                    }
                    else
                    {
                        AE = findPane(uiHelper, sTitle);
                    }
                }
                AEList = uiHelper.CurrentWindow.FindAll(TreeScope.Children, condDialog);
            } while (sfound == false && AEList.Count > 0 && AE == null);

            return AE;
        }

        private PayLoad SwitchWindow(ActSwitchWindow ActSwitchWindow)
        {
            Stopwatch St = new Stopwatch();
            PayLoad Request = ActSwitchWindow.GetPayLoad();
            PayLoad Response = Send(Request);
            String sResponse = Response.Name.ToString();

            St.Reset();
            St.Start();
            while (!(sResponse.Contains("Done")))
            {
                Thread.Sleep(100);
                //e = LocateElement(act, true);
                Response = Send(Request);
                sResponse = Response.Name.ToString();
                if (St.ElapsedMilliseconds > ActSwitchWindow.WaitTime * 1000)
                {
                    break;
                }
            }
            return Response;
        }

        public override string GetURL()
        {
            return ((IWindowExplorer)this).GetActiveWindow().Title;
        }



        public override void HighlightActElement(Act act)
        {
        }

        public override ePlatformType Platform { get { return ePlatformType.Java; } }

        public override bool IsRunning()
        {
            return SocketConnected(clientSocket);
        }

        //IWindowExplorer

        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();

            PayLoad PL = new PayLoad(CommandType.WindowExplorerOperation.ToString());
            PL.AddEnumValue(WindowExplorerOperationType.GetAllWindows);
            PL.ClosePackage();
            PayLoad RC = Send(PL);
            if (RC.IsErrorPayLoad())
            {
                string ErrMsg = RC.GetErrorValue();
                throw new Exception(ErrMsg);
            }

            List<string> aWindows = RC.GetListString();

            foreach (string win in aWindows)
            {
                if (win.Length > 0)
                {
                    // string[] wininfo = win.Split('^');
                    AppWindow AW = new AppWindow() { Title = win, Path = win, WindowType = AppWindow.eWindowType.JFrmae };
                    list.Add(AW);
                }
            }
            return list;
        }

        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null,bool isPOMLearn = false)
        {
            List<ElementInfo> list = new List<ElementInfo>();

            PayLoad Request = new PayLoad(CommandType.WindowExplorerOperation.ToString());
            Request.AddEnumValue(WindowExplorerOperationType.GetCurrentWindowVisibleControls);
            Request.ClosePackage();
            PayLoad Response = Send(Request);

            if (Response.IsErrorPayLoad())
            {
                string ErrMsg = Response.GetErrorValue();
                throw new Exception(ErrMsg);
            }
            else
            {
                List<PayLoad> ControlsPL = Response.GetListPayLoad();
                foreach (PayLoad pl in ControlsPL)
                {
                    JavaElementInfo ci = (JavaElementInfo)GetControlInfoFromPayLoad(pl);

                    if(isPOMLearn)
                    {
                        ((IWindowExplorer)this).LearnElementInfoDetails(ci);
                        // set the Flag in case you wish to learn the element or not
                        bool learnElement = true;
                        if (filteredElementType != null)
                        {
                            if (!filteredElementType.Contains(ci.ElementTypeEnum))
                                learnElement = false;
                        }
                        if (learnElement)
                        {
                            ci.IsAutoLearned = true;
                            foundElementsList.Add(ci);
                        }
                    }
                    else
                    {
                        list.Add(ci);
                        List<ElementInfo> HTMLControlsPL = new List<ElementInfo>();
                        if (ci.ElementTypeEnum == eElementType.Browser)
                        {
                            PayLoad PL = IsElementDisplayed(eLocateBy.ByXPath.ToString(), ci.XPath);
                            String flag = PL.GetValueString();

                            if (flag.Contains("True"))
                            {
                                InitializeBrowser(ci);

                                HTMLControlsPL = GetBrowserVisibleControls();
                                if (HTMLControlsPL != null)
                                    list.AddRange(HTMLControlsPL);
                            }
                        }
                        //TODO: J.G. use elementTypeEnum instead of contains
                        else if (ci.ElementType != null && ci.ElementType.Contains("JEditor"))
                        {
                            InitializeJEditorPane(ci);
                            HTMLControlsPL = GetBrowserVisibleControls();
                            if (HTMLControlsPL != null)
                                list.AddRange(HTMLControlsPL);
                        }
                    }
                    
                }
            }

            if(isPOMLearn)
            {
                list = General.ConvertObservableListToList<ElementInfo>(foundElementsList);
            }            
            return list;

        }


        public List<ElementInfo> GetBrowserVisibleControls()
        {
            PayLoad PL = new PayLoad("GetVisibleElements");
            PL.ClosePackage();
            General.DoEvents();
            PayLoad Response = Send(PL);

            if (Response.IsErrorPayLoad())
            {
                //TODO: need to handle teh exception reason
                return null;
            }

            List<PayLoad> controls = Response.GetListPayLoad();
            List<ElementInfo> list = new List<ElementInfo>();

            foreach (PayLoad pl in controls)
            {
                ElementInfo ei = GetHTMLElementInfoFromPL(pl);
                list.Add(ei);
            }

            return list;
        }

        public List<ElementInfo> GetEditorVisibleControls()
        {
            PayLoad PL = new PayLoad("GetEditorVisibleElements");
            PL.ClosePackage();
            PayLoad Response = Send(PL);
            if (Response.IsErrorPayLoad())
            {
                //TODO: need to handle teh exception reason
                return null;
            }
            List<PayLoad> controls = Response.GetListPayLoad();
            List<ElementInfo> list = new List<ElementInfo>();

            foreach (PayLoad pl in controls)
            {
                ElementInfo ei = GetHTMLElementInfoFromPL(pl);
                list.Add(ei);
            }
            return list;
        }

        //List which contains all javascript elements to be injected. This list 
        //will be sent once StartRecording starts.
        public List<string> GetJSFilesList()
        {
            List<string> jsList = new List<string>();
            jsList.Add(MinifyJS(Properties.Resources.html2canvas));
            jsList.Add(MinifyJS(Properties.Resources.ArrayBuffer));
            jsList.Add(MinifyJS(Properties.Resources.PayLoad));
            jsList.Add(MinifyJS(Properties.Resources.GingerHTMLHelper));
            jsList.Add(MinifyJS(Properties.Resources.GingerLibXPath));
            jsList.Add((Properties.Resources.wgxpath_install));
            jsList.Add(MinifyJS(Properties.Resources.jquery_min));

            return jsList;
        }

        public void InitializeJEditorPane(JavaElementInfo JEI)
        {
            PayLoad plInitialize = new PayLoad("InitializeJEditorPane");
            plInitialize.AddValue("BYXPath");
            plInitialize.AddValue(JEI.XPath);
            plInitialize.ClosePackage();
            PayLoad Response = Send(plInitialize);

            if (Response.IsErrorPayLoad())
            {
                Reporter.ToUser(eUserMsgKey.FailedToInitiate, "JEditor Element");
                return;
            }
        }

        public void InitializeBrowser(JavaElementInfo JEI)
        {
            //TODO: Split this into multiple payload requests i.e. first check if browser already initialized etc.
            PayLoad PL = new PayLoad("InitializeBrowser");
            PL.AddValue("ByXPath");
            PL.AddValue(JEI.XPath);
            PL.AddValue(120);// We giving default wait time of 120 seconds for initialize browser to finish
            List<string> jsList = new List<string>();
            jsList.Add(MinifyJS(Properties.Resources.html2canvas));
            jsList.Add(MinifyJS(Properties.Resources.ArrayBuffer));
            jsList.Add(MinifyJS(Properties.Resources.PayLoad));
            jsList.Add(MinifyJS(Properties.Resources.GingerHTMLHelper));
            jsList.Add(MinifyJS(Properties.Resources.GingerLibXPath));
            jsList.Add((Properties.Resources.wgxpath_install));
            jsList.Add(MinifyJS(Properties.Resources.jquery_min));
            PL.AddValue(jsList);
            PL.ClosePackage();
            General.DoEvents();
            PayLoad Response = Send(PL);
            if (Response.IsErrorPayLoad())
            {
                //TODO:: Handle exception                 
                Reporter.ToUser(eUserMsgKey.FailedToInitiate, "Browser Element");
                return;
            }
        }

        public static ElementInfo GetControlInfoFromPayLoad(PayLoad pl)
        {
            JavaElementInfo JEI = new JavaElementInfo();
            JEI.ElementTitle = pl.GetValueString();
            JEI.ElementType = pl.GetValueString();
            JEI.Value = pl.GetValueString();
            JEI.Path = pl.GetValueString();
            JEI.XPath = pl.GetValueString();
            JEI.ElementTypeEnum = JavaPlatform.GetElementType(JEI.ElementType);
            //If name if blank keep it blank. else creating issue for spy and highlight, as we try to search with below
            if (String.IsNullOrEmpty(JEI.ElementTitle))
            {
                string[] s = JEI.XPath.Split('.');
                JEI.ElementTitle = s.Last();
            }

            //TODO: add to PayLoad bool            
            string IsExpandable = pl.GetValueString();
            if (IsExpandable == "Y")
            {
                JEI.IsExpandable = true;
            }
            else
            {
                JEI.IsExpandable = false;
            }

            return JEI;
        }

        public static ElementInfo GetHTMLElementInfoFromPL(PayLoad PL)
        {
            HTMLElementInfo EI = new HTMLElementInfo();
            EI.ElementTitle = PL.GetValueString();
            EI.ID = PL.GetValueString();
            EI.Value = PL.GetValueString();
            EI.Name = PL.GetValueString();
            EI.ElementType = PL.GetValueString();
            EI.ElementTypeEnum = GetHTMLElementType(EI.ElementType);
            EI.Path = PL.GetValueString();
            EI.XPath = PL.GetValueString();
            EI.RelXpath = PL.GetValueString();        
            return EI;
        }


        private static eElementType GetHTMLElementType(string elementTypeString)
        {
            elementTypeString = elementTypeString.ToUpper();

            switch (elementTypeString)
            {
                case "INPUT.TEXT":
                case "TEXTAREA":
                case "INPUT.UNDEFINED":
                case "INPUT.PASSWORD":
                case "INPUT.EMAIL":
                    return eElementType.TextBox;

                case "INPUT.BUTTON":
                case "BUTTON":
                case "INPUT.IMAGE":
                case "INPUT.SUBMIT":
                    return eElementType.Button;


                case "INPUT.CHECKBOX":
                    return eElementType.CheckBox;

                case "INPUT.RADIO":
                    return eElementType.RadioButton;

                case "DIV":
                    return eElementType.Div;

                case "SPAN":
                    return eElementType.Span;

                case "IFRAME":
                    return eElementType.Iframe;

                case "TD":
                case "TH":
                    return eElementType.TableItem;

                case "LINK":
                case "A":
                    return eElementType.HyperLink;

                case "LABEL":
                    return eElementType.Label;

                case "SELECT":                
                    return eElementType.ComboBox;

                case "TABLE":
                    return eElementType.Table;

                case "JEDITOR.TABLE":
                    return eElementType.EditorTable;

                case "IMG":
                    return eElementType.Image;


                case "CANVAS":
                    return eElementType.Canvas;

                default:
                    return eElementType.Unknown;
            }

        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            PayLoad PL = new PayLoad("SwitchWindow");
            PL.AddValue(Title);
            PL.ClosePackage();
            PayLoad RC = Send(PL);
            if (RC.IsErrorPayLoad())
            {
                string errmsg = RC.GetErrorValue();
                throw new Exception(errmsg);
            }
        }

        private string MinifyJS(string script)
        {
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var minifiedString = minifier.MinifyJavaScript(script);
            if (minifier.Errors.Count > 0)
            {
                //There are ERRORS !!!
                Console.WriteLine(script);
                return null;
            }
            return minifiedString + ";";
        }



        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            if (ElementInfo.GetType() == typeof(JavaElementInfo))
            {

                JavaElementInfo JEI = (JavaElementInfo)ElementInfo;

                if (JEI == null) return;

                PayLoad Request = new PayLoad(CommandType.WindowExplorerOperation.ToString()); //Keep the payload name different for swing element and widgets elements
                Request.AddEnumValue(WindowExplorerOperationType.Highlight);

                if (!(String.IsNullOrEmpty(JEI.ElementTitle)) && !JEI.ElementTitle.Contains("[") && !(JEI.XPath.Contains("[Value:")) && !(JEI.XPath.Contains(JEI.ElementTitle + "[")))
                {
                    Request.AddValue("ByName");
                    Request.AddValue(JEI.ElementTitle);
                }
                else
                {
                    Request.AddValue("ByXPath");
                    Request.AddValue(JEI.XPath); // Fix Me:Correct value sent from here, but on Java Side Locate value is getting null
                }
                // Request.AddValue("");  // Why?          
                Request.ClosePackage();
                Send(Request);
            }
            else if (ElementInfo.GetType() == typeof(HTMLElementInfo))
            {
                HTMLElementInfo HEI = (HTMLElementInfo)ElementInfo;
                if (ElementInfo.ElementType.Contains("JEditor"))
                {
                    PayLoad Request = new PayLoad("HighLightEditorElement");
                    Request.AddValue(HEI.Path);
                    Request.AddValue(HEI.XPath);
                    Request.ClosePackage();
                    Send(Request);
                }
                else
                {
                    PayLoad Request = new PayLoad("HighLightElement");
                    Request.AddValue(HEI.Path);
                    Request.AddValue(HEI.XPath);
                    Request.ClosePackage();
                    Send(Request);
                    //TODO:J.G: Fix it for JEDITOR Elements
                }
            }
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

            PayLoad response = null;
            if (ElementInfo.ElementType.Contains("JEditor"))
            {
                PayLoad PLReq = new PayLoad("GetEditorElementProperties");
                PLReq.AddValue("ByCSSSelector");
                PLReq.AddValue(ElementInfo.Path);
                PLReq.ClosePackage();
                response = Send(PLReq);
            }
            else if (ElementInfo.GetType() == typeof(HTMLElementInfo))
            {
                PayLoad PLReq = new PayLoad("GetElementProperties");
                PLReq.AddValue(ElementInfo.Path);
                PLReq.AddValue(ElementInfo.XPath);
                PLReq.ClosePackage();
                response = Send(PLReq);
            }
            else
            {
                PayLoad PLReq = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
                PLReq.AddEnumValue(JavaDriver.WindowExplorerOperationType.GetProperties);
                PLReq.AddValue("ByXPath");
                PLReq.AddValue(ElementInfo.XPath);
                PLReq.ClosePackage();
                response = Send(PLReq);
            }
            List<PayLoad> PropertiesPLs = new List<PayLoad>();
            if (response.IsErrorPayLoad())
            {                
                string ErrMSG = response.GetErrorValue();
                Reporter.ToLog(eLogLevel.ERROR, "Error while fetching properties :" + ErrMSG);
            }
            else
            {
                PropertiesPLs = response.GetListPayLoad();
            }

            foreach (PayLoad plp in PropertiesPLs)
            {
                string PName = plp.GetValueString();
                string PValue = string.Empty;
                if (PName != "Value")
                {
                    PValue = plp.GetValueString();
                }
                else
                {
                    List<String> valueList = plp.GetListString();
                    if (valueList.Count != 0)
                        PValue = valueList.ElementAt(0);
                }
                list.Add(new ControlProperty() { Name = PName, Value = PValue });
            }
            //TODO:J.G: Fix it for JEDITOR Elements

            return list;
        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            if (ElementInfo.ElementType == "SELECT")
            {
                ComboElementInfo cmbElementInfo = new ComboElementInfo();
                PayLoad PLListDetails = new PayLoad("HTMLElementAction", "GetListDetails", "ByXPath", ElementInfo.XPath, "");
                PayLoad RespListDetails = Send(PLListDetails);

                if (RespListDetails.IsErrorPayLoad())
                {
                    string ErrMSG = RespListDetails.GetValueString();
                    throw new Exception(ErrMSG);
                }

                if (RespListDetails.Name == "List Items")
                {
                    ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
                    List<String> itemList = RespListDetails.GetListString();
                    cmbElementInfo.ItemList = itemList;

                }
                return cmbElementInfo;
            }
            else if (ElementInfo.ElementType == eElementType.ComboBox.ToString())
            {
                List<String> props = new List<String>();
                PayLoad PLListDetails = new PayLoad("UIElementAction", elementLocateBy.ToString(), elementLocateValue.ToString(), eElementType.ComboBox.ToString(),
                      "GetAllValues", "", "");
                PayLoad RespListDetails = Send(PLListDetails);
                if (RespListDetails.IsErrorPayLoad())
                {
                    string ErrMSG = RespListDetails.GetValueString();
                    throw new Exception(ErrMSG);
                }
                if (RespListDetails.Name == "ComponentValue")
                {
                    ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();
                    props = RespListDetails.GetListString();
                }
                return props;
            }
            else if (ElementInfo.ElementType == "JEditor.table")
            {
                TableElementInfo tbElementInfo = new TableElementInfo();
                PayLoad Request = new PayLoad("GetEditorTableDetails");
                Request.AddValue("GetEditorTableDetails");
                Request.AddValue("ByCSSSelector");
                Request.AddValue(((HTMLElementInfo)ElementInfo).Path);
                Request.AddValue("");
                Request.ClosePackage();

                PayLoad Response = Send(Request);
                if (Response.IsErrorPayLoad())
                {
                    Response.GetErrorValue();
                    return null;
                }

                if (Response.Name == "ControlProperties")
                {
                    Response.GetListPayLoad();
                    tbElementInfo.ColumnNames = Response.GetListString();
                    tbElementInfo.RowCount = Response.GetValueInt();
                }
                return tbElementInfo;
            }
            else
            {
                TableElementInfo tbElementInfo = new TableElementInfo();
                PayLoad Request = new PayLoad("GetTableDetails");
                Request.AddValue("GetTableDetails");
                Request.AddValue("ByXPath");
                Request.AddValue(ElementInfo.XPath);
                Request.AddValue("");  // Get All Properties
                Request.ClosePackage();

                JavaDriver d = (JavaDriver)ElementInfo.WindowExplorer;
                PayLoad Response = d.Send(Request);
                if (Response.IsErrorPayLoad())
                {
                    Response.GetErrorValue();
                    return null;
                }

                if (Response.Name == "ControlProperties")
                {
                    Response.GetListPayLoad();
                    tbElementInfo.ColumnNames = Response.GetListString();
                    tbElementInfo.RowCount = Response.GetValueInt();
                }
                return tbElementInfo;
            }
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            ObservableList<ElementLocator> locatorList = new ObservableList<ElementLocator>();
            String bName;
            if (ElementInfo.GetType() == typeof(HTMLElementInfo))
                bName = ((HTMLElementInfo)ElementInfo).Name;
            else
                bName = (ElementInfo).ElementTitle;

            if (!(String.IsNullOrEmpty(bName)))
            {
                ElementLocator locator = new ElementLocator();
                if (ElementInfo.XPath == "/") // If it is root node the  only by title is applicable
                    locator.LocateBy = eLocateBy.ByTitle;
                else
                    locator.LocateBy = eLocateBy.ByName;
                locator.LocateValue = bName;
                locatorList.Add(locator);
            }

            if (!(String.IsNullOrEmpty(ElementInfo.XPath)))
            {
                if (ElementInfo.XPath != "/")
                {
                    ElementLocator locator = new ElementLocator();
                    locator.LocateBy = eLocateBy.ByXPath;
                    locator.LocateValue = ElementInfo.XPath;
                    locatorList.Add(locator);
                }
            }

            if (ElementInfo.GetType() == typeof(HTMLElementInfo))
            {

                if (!(String.IsNullOrEmpty(((HTMLElementInfo)ElementInfo).RelXpath)))
                {
                    if (ElementInfo.XPath != "/")
                    {
                        ElementLocator locator = new ElementLocator();
                        locator.LocateBy = eLocateBy.ByRelXPath;
                        locator.LocateValue = ((HTMLElementInfo)ElementInfo).RelXpath;
                        locatorList.Add(locator);
                    }
                }

                if (!(String.IsNullOrEmpty(((HTMLElementInfo)ElementInfo).ID)))
                {
                    if (ElementInfo.XPath != "/" && !ElementInfo.ElementType.Contains("JEditor"))//?????????
                    {
                        ElementLocator locator = new ElementLocator();
                        locator.LocateBy = eLocateBy.ByID;
                        locator.LocateValue = ((HTMLElementInfo)ElementInfo).ID;
                        locatorList.Add(locator);
                    }
                }

                if (ElementInfo.ElementType.Contains("JEditor"))
                {
                    if (!String.IsNullOrEmpty(ElementInfo.Path))
                    {
                        ElementLocator locator = new ElementLocator();
                        locator.LocateBy = eLocateBy.ByCSSSelector;
                        locator.LocateValue = ((HTMLElementInfo)ElementInfo).Path;
                        locatorList.Add(locator);
                    }
                }
            }
            locatorList.ToList().ForEach(x => x.IsAutoLearned = true);
            return locatorList;
        }

        string IWindowExplorer.GetFocusedControl()
        {
            string s = "";
            if (s.StartsWith("OK"))
            {
                s = s.Substring(3);
                return s;
            }
            else
            {
                return null;
            }
        }

        ElementInfo IWindowExplorer.LearnElementInfoDetails(ElementInfo EI)
        {
            EI.Locators = ((IWindowExplorer)this).GetElementLocators(EI);
            EI.Properties = ((IWindowExplorer)this).GetElementProperties(EI);
            if(ElementInfo.IsElementTypeSupportingOptionalValues(EI.ElementTypeEnum))
            {
                EI.OptionalValuesObjectsList = ((IWindowExplorer)this).GetOptionalValuesList(EI, eLocateBy.ByXPath, EI.XPath);
            }            
            if (EI.OptionalValuesObjectsList.Count > 0)
            {
                EI.OptionalValuesObjectsList[0].IsDefault = true;
            }
            return EI;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            //TODO:
            PayLoad Request = new PayLoad(CommandType.WindowExplorerOperation.ToString());
            Request.AddEnumValue(WindowExplorerOperationType.GetComponentFromCursor);
            Request.ClosePackage();
            General.DoEvents();

            PayLoad Response = Send(Request);
            if (!(Response.IsErrorPayLoad()))
            {
                if (Response.Name == "HTMLElement")
                {
                    return GetHTMLElementInfoFromPL(Response);
                }
                else if (Response.Name == "RequireInitializeBrowser")
                {
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    JavaElementInfo JE = (JavaElementInfo)GetControlInfoFromPayLoad(Response);
                    InitializeBrowser(JE);
                    //Adding automatically action for InitializeBrowser
                    BusinessFlow.AddAct(new ActBrowserElement
                    {
                        Description = "Initialize Browser Automatically - JExplorerBrowser",
                        LocateBy = eLocateBy.ByXPath,
                        LocateValue = JE.XPath,
                        Value = ""
                    });

                    Mouse.OverrideCursor = null;
                    Reporter.ToUser(eUserMsgKey.InitializeBrowser);
                    return null;
                }
                else
                    return GetControlInfoFromPayLoad(Response);
            }
            return null;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            PayLoad Request = new PayLoad(CommandType.WindowExplorerOperation.ToString());
            Request.AddEnumValue(WindowExplorerOperationType.GetActiveWindow);
            Request.ClosePackage();
            PayLoad Response = Send(Request);

            if (Response.Name == "ActiveWindow")
            {
                string title = Response.GetValueString();
                AppWindow AW = new AppWindow() { Title = title, Path = title, WindowType = AppWindow.eWindowType.JFrmae };
                return AW;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error in GetActiveForm");
                return null;
            }
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            try
            {
                PayLoad RequestPL = new PayLoad("GetElementChildren");
                RequestPL.AddValue(ElementInfo.Path); //Path
                RequestPL.AddValue(ElementInfo.XPath);  // XPath
                RequestPL.ClosePackage();

                PayLoad RCPL = Send(RequestPL);
                if (RCPL.IsErrorPayLoad()) return null;
                List<ElementInfo> list = GetElementsFromPL(RCPL);
                return list;
            }
            catch (Exception ex)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error in GetHTMLElements - " + ex.Message);
                return null;
            }
        }

        private List<ElementInfo> GetElementsFromPL(PayLoad PLRC)
        {
            List<ElementInfo> list = new List<ElementInfo>();
            List<PayLoad> ElementsPL = PLRC.GetListPayLoad();
            foreach (PayLoad PL in ElementsPL)
            {
                HTMLElementInfo EI = (HTMLElementInfo)GetHTMLElementInfoFromPL(PL);
                EI.WindowExplorer = this;
                list.Add(EI);
            }
            return list;
        }

        public event Amdocs.Ginger.Plugin.Core.RecordingEventHandler RecordingEvent;

        public override void StartRecording()
        {
            DoRecordings();
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.StartRecording(bool learnAdditionalChanges)
        {
            DoRecordings();
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        private void DoRecordings()
        {
            PayLoad plJE = new PayLoad("CheckJExplorerExists");
            plJE.ClosePackage();

            string recordingScript = MinifyJS(Properties.Resources.GingerHTMLRecorder);
            List<string> jsList = GetJSFilesList();

            PayLoad rPlJE = Send(plJE);

            if (!rPlJE.IsErrorPayLoad())
            {
                JavaElementInfo ci = (JavaElementInfo)GetControlInfoFromPayLoad(rPlJE);
                InitializeBrowser(ci);
                recordingScript = MinifyJS(Properties.Resources.GingerHTMLRecorder);

                //Adding automatically action for InitializeBrowser when recording starts 
                //and JExplorer browser is visible. 
                BusinessFlow.AddAct(new ActBrowserElement
                {
                    Description = "Initialize Browser - JExplorerBrowser",
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = ci.XPath,
                    Value = ""
                });

            }

            PayLoad plAC = new PayLoad("StartRecording");
            plAC.AddValue(recordingScript);
            plAC.AddValue(jsList);
            plAC.ClosePackage();
            Send(plAC);
            StartGetRecordingTimer();
        }

        public override void StopRecording()
        {
            EndRecordings();
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.StopRecording()
        {
            EndRecordings();
        }

        private void EndRecordings()
        {
            if (mGetRecordingTimer != null)
            {
                mGetRecordingTimer.Tick += dispatcherTimerElapsedTick;
                mGetRecordingTimer.Stop();
            }


            PayLoad plAC = new PayLoad("StopRecording");
            plAC.ClosePackage();
            Send(plAC);
        }

        private void StartGetRecordingTimer()
        {
            mGetRecordingTimer = new System.Windows.Threading.DispatcherTimer();
            mGetRecordingTimer.Tick += dispatcherTimerElapsedTick;
            mGetRecordingTimer.Interval = new TimeSpan(0, 0, 1);
            mGetRecordingTimer.Start();
        }

        private void dispatcherTimerElapsedTick(object sender, EventArgs e)
        {
            GetRecording();
        }

        private void GetRecording()
        {
            if (mGetRecordingTimer.IsEnabled == false)
                return;

            PayLoad plAC = new PayLoad("GetRecording");
            plAC.ClosePackage();
            PayLoad rcAC = Send(plAC);

            if (mGetRecordingTimer.IsEnabled == true && !rcAC.IsErrorPayLoad())
            {
                List<PayLoad> list = rcAC.GetListPayLoad();
                foreach (PayLoad pl in list)
                {
                    CreateAction(pl);
                }
            }
        }

        private void CreateAction(PayLoad pl)
        {
            switch (pl.Name)
            {
                case "JButton":
                    string ButtonXPath = pl.GetValueString();
                    string ButtonName = pl.GetValueString();

                    var actUIElementJButton = new ActUIElement()
                    {
                        Description = "Click Button '" + ButtonName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = ButtonXPath,
                        Value = ButtonName,
                        ElementType = eElementType.Button,
                        ElementAction = ActUIElement.eElementAction.Click,
                    };
                    actUIElementJButton.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.Medium.ToString());
                    BusinessFlow.AddAct(actUIElementJButton);
                    break;
                case "JCheckBox":
                    string CheckBoxXPath = pl.GetValueString();
                    string CheckBoxName = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Click checkBox '" + CheckBoxName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = CheckBoxXPath,
                        ElementType = eElementType.CheckBox,
                        ElementAction = ActUIElement.eElementAction.Toggle
                    });
                    break;

                case "JTextField":
                    string TextFieldXPath = pl.GetValueString();
                    string TextFieldValue = pl.GetValueString();
                    string TextFieldName = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Set Text Box '" + TextFieldName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = TextFieldXPath,
                        Value = TextFieldValue,
                        ElementType = eElementType.TextBox,
                        ElementAction = ActUIElement.eElementAction.SetValue
                    });
                    break;

                case "JComboBox":
                    string ComboBoxXPath = pl.GetValueString();
                    string ComboBoxValue = pl.GetValueString();
                    string ComboBoxName = pl.GetValueString();

                    var actUIElementJComboBox = new ActUIElement()
                    {
                        Description = "Select ComboBox '" + ComboBoxName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = ComboBoxXPath,
                        ElementType = eElementType.ComboBox,
                        ElementAction = ActUIElement.eElementAction.Select
                    };
                    actUIElementJComboBox.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, ComboBoxValue);

                    BusinessFlow.AddAct(actUIElementJComboBox);

                    break;

                case "JRadioButton":
                    string RadioButtonXPath = pl.GetValueString();
                    string RadioButtonName = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Select RadioButton'" + RadioButtonName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = RadioButtonXPath,
                        ElementType = eElementType.RadioButton,
                        ElementAction = ActUIElement.eElementAction.Select
                    });
                    break;

                case "JTextArea":
                    string TextAreaXPath = pl.GetValueString();
                    string TextAreaValue = pl.GetValueString();
                    string TextAreaName = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Set Text Area '" + TextAreaName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = TextAreaXPath,
                        Value = TextAreaValue,
                        ElementType = eElementType.TextBox,
                        ElementAction = ActUIElement.eElementAction.SetValue
                    });
                    break;

                case "JTable":
                    string JTableXPath = pl.GetValueString();
                    string JTableValue = pl.GetValueString();
                    string JTableName = pl.GetValueString();
                    string row = pl.GetValueString();
                    string column = pl.GetValueString();

                    var actUIElementTable = new ActUIElement()
                    {
                        Description = "Set Table '" + JTableName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = JTableXPath,
                        Value = JTableValue,
                        ElementType = eElementType.Table,
                        ElementAction = ActUIElement.eElementAction.TableCellAction,
                    };
                    actUIElementTable.GetOrCreateInputParam(ActUIElement.Fields.WhereColumnValue, column);
                    actUIElementTable.GetOrCreateInputParam(ActUIElement.Fields.LocateRowType, "Row Number");
                    actUIElementTable.GetOrCreateInputParam(ActUIElement.Fields.ColSelectorValue, ActUIElement.eTableElementRunColSelectorValue.ColNum.ToString());
                    actUIElementTable.GetOrCreateInputParam(ActUIElement.Fields.LocateColTitle, column);
                    actUIElementTable.GetOrCreateInputParam(ActUIElement.Fields.ControlAction, ActUIElement.eTableAction.DoubleClick.ToString());

                    BusinessFlow.AddAct(actUIElementTable);

                    break;

                case "JTextPane":
                    string TextPaneXPath = pl.GetValueString();
                    string TextPaneValue = pl.GetValueString();
                    string TextPaneName = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Set Text Pane '" + TextPaneName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = TextPaneXPath,
                        Value = TextPaneValue,
                        ElementType = eElementType.TextBox,
                        ElementAction = ActUIElement.eElementAction.SetValue
                    });
                    break;

                case "JMenu":
                    string MenuValue = pl.GetValueString();
                    string MenuName = pl.GetValueString();

                    var actUIElementJMenu = new ActUIElement()
                    {
                        Description = "Click Menu '" + MenuName + "'",
                        ElementLocateBy = eLocateBy.ByName,
                        ElementLocateValue = MenuName,
                        Value = MenuValue,
                        ElementType = eElementType.MenuItem,
                        ElementAction = ActUIElement.eElementAction.Click,
                    };
                    actUIElementJMenu.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.Medium.ToString());
                    BusinessFlow.AddAct(actUIElementJMenu);
                    break;

                case "JMenuItem":
                    string MenuItemValue = pl.GetValueString();
                    string MenuItemName = pl.GetValueString();

                    var actUIElementJMenuItem = new ActUIElement()
                    {
                        Description = "Click Menu Item '" + MenuItemName + "'",
                        ElementLocateBy = eLocateBy.ByName,
                        ElementLocateValue = MenuItemName,
                        Value = MenuItemValue,
                        ElementType = eElementType.MenuItem,
                        ElementAction = ActUIElement.eElementAction.Click
                    };
                    actUIElementJMenuItem.GetOrCreateInputParam(ActUIElement.Fields.WaitforIdle, ActUIElement.eWaitForIdle.None.ToString());
                    break;

                case "JList":
                    string JList = pl.GetValueString();
                    string vJList = pl.GetValueString();

                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Set JList '" + JList + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = JList,
                        ElementType = eElementType.Unknown,
                        ElementAction = ActUIElement.eElementAction.SetValue,
                        Value = vJList
                    });
                    break;

                case "JTree":
                    string JTreeXPath = pl.GetValueString();
                    string JTreeValue = pl.GetValueString();
                    string JTreeName = pl.GetValueString();
                    BusinessFlow.AddAct(new ActUIElement()
                    {
                        Description = "Set Tree '" + JTreeName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = JTreeXPath,
                        Value = JTreeValue,
                        ElementType = eElementType.Unknown,
                        ElementAction = ActUIElement.eElementAction.Click
                    });
                    break;

                case "JTabbedPane":
                    string JTPaneXPath = pl.GetValueString();
                    string JTPaneValue = pl.GetValueString();
                    string JTPaneName = pl.GetValueString();

                    var actUIElementJMenuTab = new ActUIElement()
                    {
                        Description = "Set Pane '" + JTPaneName + "'",
                        ElementLocateBy = eLocateBy.ByXPath,
                        ElementLocateValue = JTPaneXPath,
                        ElementType = eElementType.Tab,
                        ElementAction = ActUIElement.eElementAction.Select
                    };
                    actUIElementJMenuTab.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, JTPaneValue);
                    BusinessFlow.AddAct(actUIElementJMenuTab);
                    break;

                case "SwitchWindow":
                    string WindowTitle = pl.GetValueString();
                    var actUISwitch = new ActUIElement()
                    {
                        Description = "Switch Window '" + WindowTitle + "'",
                        ElementLocateBy = eLocateBy.ByTitle,
                        ElementLocateValue = WindowTitle,
                        ElementType = eElementType.Window,
                        ElementAction = ActUIElement.eElementAction.Switch
                    };
                    actUISwitch.GetOrCreateInputParam(ActUIElement.Fields.SyncTime, "30");
                    BusinessFlow.AddAct(actUISwitch);
                    break;

                //Widgets recorded elements
                case "htmlcheckbox":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlbutton":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmltext":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlpassword":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlselect-one":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlradio":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlsubmit":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlfile":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmltextarea":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlDIV":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlA":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlP":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlLI":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                case "htmlSPAN":

                    this.BusinessFlow.AddAct(GetHTMLAction(pl));
                    break;

                default:
                    throw new Exception("Unknown Action to create: " + pl.Name);
            }
        }

        private ActUIElement GetHTMLAction(PayLoad pl)
        {

            ActUIElement actUIElement = new ActUIElement();

            string elementLocateBy = pl.GetValueString();
            string elementLocateValue = pl.GetValueString();
            string elementValue = pl.GetValueString();
            string elementAction = pl.GetValueString();
            string Type = pl.GetValueString();

            actUIElement.GetOrCreateInputParam(ActUIElement.Fields.IsWidgetsElement, "true");

            actUIElement.ElementLocateBy = GetElementLocatBy(elementLocateBy);

            if (Type.ToLower().Equals("radio"))
            {
                actUIElement.ElementLocateValue = elementValue;
            }
            else
            {
                actUIElement.ElementLocateValue = elementLocateValue;
            }

            actUIElement.GetOrCreateInputParam(ActUIElement.Fields.ElementAction, GetElementACtion(elementAction).ToString());

            if (actUIElement.ElementLocateBy.Equals(eLocateBy.ByName) || actUIElement.ElementLocateBy.Equals(eLocateBy.ByID))
            {
                actUIElement.Description = string.Concat(elementAction, " ", elementLocateValue);
            }
            else
            {
                actUIElement.Description = string.Concat(elementAction, " ", Type.ToUpper());
            }


            actUIElement.Value = elementValue;

            actUIElement.GetOrCreateInputParam(ActUIElement.Fields.ValueToSelect, elementValue);

            return actUIElement;
        }

        private ActUIElement.eElementAction GetElementACtion(string elementAction)
        {
            try
            {
                return (ActUIElement.eElementAction)Enum.Parse(typeof(ActUIElement.eElementAction), elementAction);
            }
            catch
            {
                return ActUIElement.eElementAction.Unknown;
            }

        }

        private eLocateBy GetElementLocatBy(string elementLocateBy)
        {
            try
            {
                return (eLocateBy)Enum.Parse(typeof(eLocateBy), elementLocateBy);
            }
            catch
            {

                return eLocateBy.NA;
            }
        }

        private void CreateSwitchWindowAction(string windowTitle)
        {
            //ActSwitchWindow act = new ActSwitchWindow();
            //act.Description = "Switch Window - " + windowTitle;
            //act.LocateBy = eLocateBy.ByTitle;
            //act.LocateValue = windowTitle;
            //if (BusinessFlow != null)
            //{
            //    BusinessFlow.AddAct(act, true);
            //}
            //else
            //{
            //    Reporter.ToUser(eUserMsgKey.RestartAgent);
            //}
            ActUIElement actUIElement = new ActUIElement()
            {
                Description = "Switch Window - " + windowTitle,
                ElementLocateBy = eLocateBy.ByTitle,
                ElementLocateValue = windowTitle,
                ElementType = eElementType.Window
            };
            actUIElement.GetOrCreateInputParam(ActUIElement.Fields.SyncTime, "30");
            if (BusinessFlow != null)
            {
                BusinessFlow.AddAct(actUIElement, true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.RestartAgent);
            }

        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {

        }

        //copy from the act handler temp because I didn't want to mess with the above, TODO: fix both to use shared method
        Bitmap GetWindowScreenShot()
        {
            PayLoad Request = new PayLoad("TakeScreenShots", Act.eWindowsToCapture.OnlyActiveWindow.ToString());
            PayLoad sReq = Send(Request);
            List<PayLoad> Response = sReq.GetListPayLoad();

            foreach (PayLoad a in Response)
            {
                Byte[] screenShotbytes;
                screenShotbytes = a.GetBytes();
                TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
                Bitmap btmp = (Bitmap)tc.ConvertFrom(screenShotbytes);
                return btmp;
            }

            return null;
        }

        public Bitmap GetScreenShot(Tuple<int, int> setScreenSize = null)
        {
            return GetWindowScreenShot();
        }

        public VisualElementsInfo GetVisualElementsInfo()
        {
            throw new NotImplementedException();
        }

        public void ChangeAppWindowSize(int Width, int Height)
        {
            throw new NotImplementedException();
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        void IWindowExplorer.UnHighLightElements()
        {
            UnhighlightLast();
        }

        public void UnhighlightLast()
        {
            try
            {
                PayLoad Request = new PayLoad(CommandType.WindowExplorerOperation.ToString());
                Request.AddEnumValue(WindowExplorerOperationType.UnHighlight);
                Request.ClosePackage();
                Send(Request);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "failed to un-highlight object", ex);
            }
        }
        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
        {
            try
            {

                mIsDriverBusy = true;
                foreach (ElementLocator el in EI.Locators)
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Pending;
                }

                List<ElementLocator> activesElementLocators = EI.Locators.Where(x => x.Active == true).ToList();
                foreach (ElementLocator el in activesElementLocators)
                {

                    PayLoad Response = LocateElementByLocator(el);
                    if (Response != null && Response.GetValueString() == "true")
                    {
                        el.StatusError = string.Empty;
                        el.LocateStatus = ElementLocator.eLocateStatus.Passed;
                        if (GetOutAfterFoundElement)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        el.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    }
                }

                if (activesElementLocators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                foreach (ElementLocator el in EI.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Pending).ToList())
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                }
                mIsDriverBusy = false;
            }
        }

        public PayLoad LocateElementByLocator(ElementLocator locator)
        {
            PayLoad PLLocateElement = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
            PLLocateElement.AddEnumValue(JavaDriver.WindowExplorerOperationType.LocateElement);
            PLLocateElement.AddValue(locator.LocateBy.ToString());
            PLLocateElement.AddValue(locator.LocateValue.ToString());
            PLLocateElement.ClosePackage();
            PayLoad Response = Send(PLLocateElement);
            if (Response.IsErrorPayLoad())
            {
                string ErrMSG = Response.GetErrorValue();
                return null;
            }
            else
            {
                return Response;
            }
        }

        public PayLoad LocateElementByLocators(ObservableList<ElementLocator> Locators)
        {
            PayLoad elem = null;
            foreach (ElementLocator locator in Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            foreach (ElementLocator locator in Locators.Where(x => x.Active == true).ToList())
            {
                if (!locator.IsAutoLearned)
                {
                    ElementLocator evaluatedLocator = locator.CreateInstance() as ElementLocator;
                    ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);
                    evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
                    elem = LocateElementByLocator(evaluatedLocator);
                }
                else
                    elem = LocateElementByLocator(locator);

                if (elem != null)
                {
                    locator.StatusError = string.Empty;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                    return elem;
                }
                else
                {
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                }
            }
            return null;
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            try
            {
                mIsDriverBusy = true;

                foreach (ElementInfo EI in originalList)
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Pending;
                }


                foreach (ElementInfo EI in originalList)
                {
                    try
                    {
                        //SwitchFrame(EI); add switch to window
                        LocateElementByLocators(EI.Locators);
                        //if (e != null) // Needed ?
                        //{
                        //    EI.ElementObject = e;
                        //    EI.ElementStatus = ElementInfo.eElementStatus.Passed;
                        //}
                        //else
                        //{
                        //    EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        //}
                    }
                    catch (Exception ex)
                    {
                        EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        Console.WriteLine("CollectOriginalElementsDataForDeltaCheck error: " + ex.Message);
                    }
                }
            }
            finally
            {
                //Driver.SwitchTo().DefaultContent(); add switch to default for java
                mIsDriverBusy = false;
            }
        }

        public ElementInfo GetMatchingElement(ElementInfo element, ObservableList<ElementInfo> originalElements)
        {
            //try by type and Xpath comparison
            ElementInfo OriginalElementInfo = originalElements.Where(x => (x.ElementTypeEnum == element.ElementTypeEnum)
                                                                && (x.XPath == element.XPath)
                                                                && (x.Path == element.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(element.Path)))
                                                                && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                    || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                        && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                        )
                                                                    )
                                                                ).FirstOrDefault();
            return OriginalElementInfo;
        }

        public void StartSpying()
        {
            // ((IWindowExplorer)this).GetControlFromMousePosition();
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            ObservableList<OptionalValue> props = new ObservableList<OptionalValue>();
            PayLoad PLListDetails = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
            PLListDetails.AddEnumValue(JavaDriver.WindowExplorerOperationType.GetOptionalValuesList);
            PLListDetails.AddValue(elementLocateBy.ToString());
            PLListDetails.AddValue(elementLocateValue.ToString());
            PLListDetails.AddValue(eElementType.ComboBox.ToString());
            PLListDetails.ClosePackage();
            PayLoad RespListDetails = Send(PLListDetails);
            if (!RespListDetails.IsErrorPayLoad())
            {
                foreach (string res in RespListDetails.GetListString())
                {
                    props.Add(new OptionalValue { Value = res, IsDefault = false });
                }
            }   
            else
            {
                string ErrMSG = RespListDetails.GetErrorValue();
                Reporter.ToLog(eLogLevel.DEBUG, "Error while fetching optional values :" + ErrMSG);
            }
            return props;
        }
    }
}
