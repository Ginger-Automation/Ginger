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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GingerCore.Actions;
using System.IO;
using System.Drawing;
using System.Globalization;
using GingerCore.Actions.ASCF;
using System.Diagnostics;
using System.Reflection;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Repository;

namespace GingerCore.Drivers.ASCF
{
    // 10 Aug 2014 - This class is for ASCF connector actions
    public class ASCFDriver : DriverBase, IWindowExplorer, Amdocs.Ginger.Plugin.Core.IRecord
    {
        // remove do events !!!
        [UserConfigured]
        [UserConfiguredDefault("127.0.0.1")]  // Local host 
        [UserConfiguredDescription("Location of Agent - leave default or set the IP address of the remote machine")]
        public string GingerToolBoxHost { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("7777")]  // Local host 
        [UserConfiguredDescription("ASCF Host Port - default is 7777")]
        public int GingerToolBoxPort { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("120")]  // Local host 
        [UserConfiguredDescription("Communication Timeout - default is 120 seconds")]
        public int CommunicationTimout { get; set; }

        private string NA = " ";
        private IPEndPoint serverAddress;
        // Socket clientSocket;
        TcpClient clientSocket;
        
        private bool mConnected = false;

        // Used when we want to close this form so stop connecting 
        public bool bClosing = false;

        string mAgentName;

        private eLocateBy mBrowserLocateBy;
        private string mBrowserLocateValue;
        //private bool mGingerHelperInjected = false;        
        private bool IsTryingToConnect;

        public bool LogCommunication { get; set; }

        public ASCFDriver(BusinessFlow BF, string AgentName)
        {
            BusinessFlow = BF;
            mAgentName = AgentName;
        }

        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        public override bool IsShowWindowExplorerOnStart()
        {
            return true;
        }

        public override void StartDriver()
        {
            if (GingerToolBoxHost == null || GingerToolBoxHost.Length ==0)
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Missing GingerToolBoxHost config value- Please verify Agent config parameter GingerToolBoxHost is not empty");
                return;
            }

            serverAddress = new IPEndPoint(IPAddress.Parse(GingerToolBoxHost), GingerToolBoxPort);

            IsTryingToConnect = true;
            // OpenASCFDriverWindow();             
            ConnectToGingerToolBox();

            while (IsTryingToConnect)
            {
                General.DoEvents();
                Thread.Sleep(100);
            }

        }
        private void ConnectToGingerToolBox()
        {
            clientSocket = new TcpClient();

            //add timeout or it gets stuck
            //TODO: add to driver config
            
            clientSocket.ReceiveTimeout = CommunicationTimout * 1000; ;
            if (CommunicationTimout == 0) CommunicationTimout = 120;
            clientSocket.SendTimeout = CommunicationTimout * 1000; ;
            clientSocket.ReceiveBufferSize = 1000000;
            clientSocket.SendBufferSize = 1000000;
            clientSocket.NoDelay = true;            

            // Need to run the window on another task and wait 
            //TODO: maybe config time out
            // We try 240 times with interval of 0.5 sec each - max 120 sec before fail

            // Do we need it on task? this cause some datagram errs when running from unit test
            Task t = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 240; i++)
                {
                    try
                    {
                        if (bClosing) return;
                        
                        //Will go to catch if agent is not ready
                        clientSocket.Connect(serverAddress);
                        
                        if (CommunicationTimout == 0) CommunicationTimout = 120;
                        mConnected = true;
                        //TODO: raise Propertychanged event on IsConnected
                        SetConfig();
                        
                        IsTryingToConnect = false;
                        return;
                    }
                    //TODO: catch excpetion of socket not all..         
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Trying to connect ASCF Agent on address:" + serverAddress);
                        Thread.Sleep(500);
                    }
                }

                General.DoEvents();
                //Connect Failed after x retry...   
                IsTryingToConnect = false;
                Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, "ASCF","");
            });
        }

        private void SetConfig()
        {
        }

        private void Reconnect()
        {
            //TODO: put the 240 in driver config            
            
            for (int i = 0; i < 240; i++)
            {                
                Reporter.ToStatus(eStatusMsgKey.ASCFTryToConnect,null, "Try#" + i);
                try
                {
                    clientSocket.Connect(serverAddress);
                    mConnected = true;
                    Reporter.HideStatusMessage();
                    return;
                }
                //TODO: catch excpetion of socket not all..         
                catch (Exception)
                {
                    Thread.Sleep(500);
                    clientSocket = new TcpClient();                 
                }                
            }
            //Show message failed
            Reporter.HideStatusMessage();
        }
        
        public string Send(String Action, String LocateBy, String LocateValue, String Property, String Value, bool WaitForIdle)
        {            
            try
            {     
                // If connection failed then go out of here
                if (!mConnected)
                {
                    return "ERROR:" + "The connection to Ginger ToolBox was closed and reconnect failed";
                }

                // TODO: improve protocol remove [Tilde] workaround
                string v;
                if (Value == null)
                {
                    v = " ";
                }
                else{
                     v = Value.Replace("~", "[Tilde]");
                }

                
                string toSend = "1234" + Action + "~" + LocateBy + "~" + LocateValue.Replace("~", "[Tilde]") + "~" + Property + "~" + v;
                if (WaitForIdle) toSend += "~Y";
                                
                // Sending
                int toSendLen = System.Text.Encoding.UTF8.GetByteCount(toSend)-4;
                
                //TODO: make it multi lang support - currently hebrew dosen't pass thru
                byte[] toSendBytes = System.Text.Encoding.UTF8.GetBytes(toSend);
                
                byte[] toSendLenBytes = System.BitConverter.GetBytes(toSendLen);
                
                toSendBytes[0] = toSendLenBytes[0];
                toSendBytes[1] = toSendLenBytes[1];
                toSendBytes[2] = toSendLenBytes[2];
                toSendBytes[3] = toSendLenBytes[3];


                //TODO: check if it will speed sending one packet
                NetworkStream ns = clientSocket.GetStream();                

                clientSocket.Client.Send(toSendBytes);
                ns.Flush();
                
                string response ="";
                
                // speed is important so we do the shift with what we know, so it is super fast
                byte[] rcvLenBytesBB = new byte[4];

                //TODO: wait till max comm timeout
                while (!ns.DataAvailable)
                {
                    //TODO: adaptive sleep
                    Thread.Sleep(10);
                    if (!SocketConnected(clientSocket))
                    {
                        return "ERROR| Lost connection or Not connected to Ginger Tool Box";
                    }
                }
                ns.Read(rcvLenBytesBB, 0, 4);
                
                int rcvLen = ((rcvLenBytesBB[3]) << 24) + (rcvLenBytesBB[2] << 16) + (rcvLenBytesBB[1] << 8) + rcvLenBytesBB[0];
                
                int received = 0;
                
                byte[] rcvBytes = new byte[rcvLen];

                 while (received < rcvLen)
                 {
                    received += ns.Read(rcvBytes, received, rcvLen - received);
                 }
                   
                ns.Flush();

                response = Encoding.UTF8.GetString(rcvBytes, 0, rcvBytes.Length);
                return response;
            }
            catch (Exception e)
            {
                return "ERROR:" + e.Message;
            }
        }

        bool SocketConnected(TcpClient s)
        {
            try
            {
                //Keep this part as sometime bad discoonect happend and the below s.connected will still report true!!
                bool part1 = s.Client.Poll(1000, SelectMode.SelectRead);
                bool part2 = (s.Client.Available == 0);
                if (part1 & part2)
                {
                    mConnected = false;
                    return false;
                }

                // Check using the socket, not working all the time
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
                bClosing = true;
                clientSocket.Client.Shutdown(SocketShutdown.Both);                
                if (clientSocket.Client.Connected)
                {
                    clientSocket.Client.Disconnect(true);
                }
                clientSocket.Close();
                clientSocket = null;
                mConnected = false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close ASCF Driver - " + ex.Message);
            }
        }

        public override Act GetCurrentElement()
        {
            //TODO: cleanup if not used

            String RC = Send("GetCurrentElement", NA, NA, NA, NA, false);
            string [] a = RC.Split('|');
            if (a.Length < 4) return null;

            string ActType = a[0];
            string LocateValue = a[2];
            string Value = a[3];

            switch (ActType)
            {
                case "ActButton":
                    ActButton act = new ActButton();
                    act.LocateBy = eLocateBy.ByName;
                    act.LocateValue = LocateValue;
                    act.AddOrUpdateInputParamValue("Value",Value);
                    return act;
                   // break;
                case "ActTextBox" :
                    ActTextBox actTB = new ActTextBox();
                    actTB.LocateBy = eLocateBy.ByName;
                    actTB.LocateValue = LocateValue;
                    actTB.AddOrUpdateInputParamValue("Value",Value);
                    actTB.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
                    return actTB;
                default:
                    throw new Exception("Unknown Element/ActType: " + ActType);
            }
        }

        

        public override void RunAction(Act act)
        {
            // int cnt = 0;
            //TODO: add func to Act + Enum for switch
            string actClass = act.GetType().ToString();
            //TODO: avoid hard coded string...
            actClass = actClass.Replace("GingerCore.Actions.ASCF.", "");
            actClass = actClass.Replace("GingerCore.Actions.", "");
            
            if (!SocketConnected(clientSocket))
            {            
                Reconnect();
            }
            switch (actClass)
            {
                case "ActASCFControl":
                    ActASCFControl AAC = (ActASCFControl)act;
                    RunControlAction(AAC);
                    break;
                case "ActSwitchWindow":
                    SwitchWindow((ActSwitchWindow)act);
                    break;
                case "ActTextBox":
                    RunTextBoxAction((ActTextBox)act);
                    break;
                case "ActLink":
                    ActLink Alink = (ActLink)act;                    
                    ClickLink(Alink);
                    break;
                case "ActButton":
                    ActButton b = (ActButton)act;
                    ClickButton(b);
                    break;
                case "ActMenuItem":     

                    ActMenuItem ami = (ActMenuItem)act;
                    MenuItem(ami);
                    break;

                //TODO Add ActScreenShot     
                case "ActScreenShot":
                    ActScreenShot actSS = (ActScreenShot)act;
                                    
                    if (actSS.WindowsToCapture == ActScreenShot.eWindowsToCapture.AllAvailableWindows)
                    {
                        ObservableList<Bitmap> screenShots= TakeScreeShotAllWindows(actSS);

                        foreach (Bitmap screenShot in screenShots)
                        {
                            act.AddScreenShot(screenShot);
                        }
                    }
                    else
                    {
                        Bitmap tempBmp = TakeScreenShot(actSS);
                        act.AddScreenShot(tempBmp);
                    }
 
                    break;
                case "ActGetMsgboxText":
                    ActGetMsgboxText actmsg = (ActGetMsgboxText)act;
                    GetMsgboxText(actmsg);
                    break;
                    
                //TODO Add ActSetConfig     
                case "ActSetConfig":
                    ActSetConfig actsc = (ActSetConfig)act;
                    SetConfig(actsc);
                    break;
                case "ActActivateRow":
                    ActActivateRow AAR = (ActActivateRow)act;
                    ActivateRow(AAR);
                    break;
                case "ActWindow":
                    ActWindow AW = (ActWindow)act;
                    HandleActWindow(AW);
                    break;
                case "ActASCFBrowserElement":
                    ActASCFBrowserElement AABC = (ActASCFBrowserElement)act;
                    HandleBrowserElementAction(AABC);
                    break;
                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }
        }

        private void HandleBrowserElementAction(ActASCFBrowserElement act)
        {
            // step 1 - find the browser control

            // Step 2 - check 

            string script = "";
            
            switch (act.ControlAction)
            {
                case ActASCFBrowserElement.eControlAction.SetBrowserControl:
                    SetCurrentBrowserControl(act.LocateBy, act.LocateValueCalculated);                        
                        string RC1 = Send("GetControlInfo", mBrowserLocateBy.ToString(), mBrowserLocateValue, NA, NA, false);
                        if (RC1.StartsWith("OK|Name="))
                        {
                            // Inject Ginger Helper
                            // CheckInjectGingerHTMLHelper();

                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;                            
                        }
                        else
                        {
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            act.Error = "Browser Control not Found";
                            mBrowserLocateValue = null;
                        }
                        act.ExInfo = RC1;                        
                        return;
                    //break;
                case ActASCFBrowserElement.eControlAction.SetValue:

                    script = string.Format("SetElementValue('{0}','{1}','{2}');", act.LocateBy, act.LocateValue, act.ValueForDriver);
                    break;
                case ActASCFBrowserElement.eControlAction.GetValue:
                    script = string.Format("GetElementValue('{0}','{1}');", act.LocateBy, act.LocateValue);
                    script += ".value";
                    break;
                case ActASCFBrowserElement.eControlAction.Click:
                    script = string.Format("ClickElement('{0}','{1}');", act.LocateBy, act.LocateValue);
                    break;
                case ActASCFBrowserElement.eControlAction.SelectedIndex:                    
                    //TODO: fix me
                    // script = GetActScript(act);
                    script += ".selectedIndex=" + act.ValueForDriver;
                    break;
                case ActASCFBrowserElement.eControlAction.InjectGingerHTMLHelper:
                    //TODO: return script or RC or ...

                    /// Is needed? since SetBrowserControl does it, but naybe the script will dsiapear if new page loaded
                    InjectGingerHTMLHelper();  
                    break;                
                default:
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage,"Unknown Browser Control Action - " + act.ControlAction);
                    return;
            }
            // send the js script to the current browser, but first check that we have browser set
            if (string.IsNullOrEmpty(mBrowserLocateValue))
            {                
                act.Error = "Browser Control not set, please do Switch to Browser control first";
                return;
            }
            act.ExInfo += "Script=" + script;
            string RC = Send("InvokeScript", mBrowserLocateBy.ToString(), mBrowserLocateValue, NA, script, false);
            SetActionStatus(act, RC);
            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                act.AddOrUpdateReturnParamActual(act.GetOrCreateInputParam("Value").Param, act.ExInfo);
            }
        }

        public void SetCurrentBrowserControl(eLocateBy LocateBy, string LocateValue)
        {
            if (mBrowserLocateBy != LocateBy ||  mBrowserLocateValue != LocateValue)
            {
                mBrowserLocateBy = LocateBy;
                mBrowserLocateValue = LocateValue;

                InjectGingerHTMLHelper();
            }
        }
        
        private void HandleActWindow(ActWindow AW)
        {
            String RC = null;
            if (AW.WindowActionType == ActWindow.eWindowActionType.Switch)
            {
                RC = Send("SwitchWindow", AW.LocateBy.ToString(), AW.LocateValueCalculated, NA, NA, false);                
            }

            else  if (AW.WindowActionType == ActWindow.eWindowActionType.Close)
            {
                RC = Send("CloseWindow", AW.LocateBy.ToString(), AW.LocateValueCalculated, NA, NA, false);
                // If the app is closing then we close the connection so new app will get new driver - Ashwini case

                if (RC.StartsWith("OK") && AW.LocateValueCalculated == "Application")
                {
                 
                    CloseDriver();
                }
            }

            else  if (AW.WindowActionType == ActWindow.eWindowActionType.IsExist)
            {
                RC = Send("IsExist", AW.LocateBy.ToString(), AW.LocateValueCalculated, NA, NA, false);
            }
            
            else if (RC == null) RC = "Error - Unsupported Window Action - " + AW.WindowActionType.ToString();

            SetActionStatus(AW, RC);   
        }

        private void ActivateRow(ActActivateRow AAR)
        {
            String RC = Send("ActivateRow", AAR.LocateBy.ToString(), AAR.LocateValueCalculated, NA, NA, true);
            SetActionStatus(AAR, RC);
        }
        private void GetMsgboxText(ActGetMsgboxText actmsg)
        {
            String RC = Send("GetMsgboxText", actmsg.LocateBy.ToString(), actmsg.LocateValueCalculated, NA, NA, false);
            SetActionStatus(actmsg, RC);
            if (actmsg.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                string output = null;
                if (actmsg.ExInfo.StartsWith("OK|"))
                    output = actmsg.ExInfo.Remove(0, 3);
                else
                    output = actmsg.ExInfo;
                actmsg.AddOrUpdateReturnParamActual(actmsg.GetOrCreateInputParam("Value").Param, output);
            }
        }

        private void RunControlAction(ActASCFControl AAC)
        {
            string action = null;
            string RC;
            switch (AAC.ControlAction)
            {
                case ActASCFControl.eControlAction.SetValue:
                    AAC.WaitForIdle = false;
                    action = "SetControlValue";
                    break;
                case ActASCFControl.eControlAction.Collapse:
                    AAC.WaitForIdle = false;
                    action = "Collapse";
                    break;
                case ActASCFControl.eControlAction.Expand:
                    AAC.WaitForIdle = true;
                    action = "Expand";
                    break;
                case ActASCFControl.eControlAction.SetFocus:
                    AAC.WaitForIdle = false;
                    action = "SetFocus";
                    break;
                case ActASCFControl.eControlAction.IsVisible:
                    AAC.WaitForIdle = false;
                    action = "GetControlProperty";
                    AAC.ControlProperty = ActASCFControl.eControlProperty.Visible;
                    RC = Send(action, AAC.LocateBy.ToString(), AAC.LocateValueCalculated, AAC.ControlProperty.ToString(), string.Empty, false);
                    SetActionStatus(AAC, RC);
                    //Configuring IsVisible to always show Pass status so users will use it "Actual" return value to do Flow Control Decisions
                    if (AAC.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                    {
                        AAC.ExInfo = string.Empty;
                        AAC.AddOrUpdateReturnParamActual("Actual", "True");
                    }
                    else
                    {
                        AAC.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                        AAC.Error = string.Empty;
                        AAC.ExInfo = string.Empty;
                        AAC.AddOrUpdateReturnParamActual("Actual", "False");
                    }
                    return;
                case ActASCFControl.eControlAction.Click:
                    AAC.WaitForIdle = true;
                    action = "Click";
                    break;                
                case ActASCFControl.eControlAction.SetVisible:
                    AAC.WaitForIdle = true;
                    action = "SetVisible";
                    break;
                case ActASCFControl.eControlAction.SetWindowState:
                    AAC.WaitForIdle = true;
                    action = "SetWindowState";
                    break;
                case ActASCFControl.eControlAction.GetControlProperty:
                    AAC.WaitForIdle = false;
                    action = "GetControlProperty";
                    RC = Send(action, AAC.LocateBy.ToString(), AAC.LocateValueCalculated, AAC.ControlProperty.ToString(), AAC.GetOrCreateInputParam("Value").Value, false);
                    SetActionStatus(AAC, RC);
                    //TODO: fixme temp ugly too...
                    if (AAC.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && AAC.ControlProperty == ActASCFControl.eControlProperty.DateTimeValue)
                    {
                        // in case of datetime we convert it from unix time
                        string uxtime = AAC.ExInfo.Substring(3);
                        string val = UnixTimetoDateTimeLocalString(uxtime);
                        AAC.ExInfo = val;
                    }
                    if (AAC.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                    {
                        string val = AAC.ExInfo.Substring(3);
                        AAC.AddOrUpdateReturnParamActual(AAC.GetOrCreateInputParam("Value").Param, val);
                    }
                    return;
                case ActASCFControl.eControlAction.InvokeScript:
                    AAC.WaitForIdle = false;
                    action = "InvokeScript";
                    break;
                case ActASCFControl.eControlAction.KeyType:
                    AAC.WaitForIdle = true;
                    action = "KeyType";
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Unknown Control Action - " + AAC.ControlAction);
                    return;                    
            }
            //Must get the value for driver !!

            //temp ugly fix me later
            
                string value = AAC.ValueForDriver;
                //TODO: fixme - temp ugly code to handle date
                
                if (AAC.ControlProperty == ActASCFControl.eControlProperty.DateTimeValue)
                {
                    value = GetUnixTime(value);
                }

                Stopwatch st = new Stopwatch();
                st.Reset();
                st.Start();
                RC = Send(action, AAC.LocateBy.ToString(), AAC.LocateValueCalculated, NA, value, AAC.WaitForIdle);
                st.Stop();             
                RC += "| - ElapsedMS=" + st.ElapsedMilliseconds;
                SetActionStatus(AAC, RC);       
        }

        private string UnixTimetoDateTimeLocalString(string longUnixTime)
        {            
            //TODO: when we take .NET 4.6 we use built in functions

            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            
            long l = long.Parse(longUnixTime);
            DateTime dd = dt.AddMilliseconds(l);
            string s = dd.ToLocalTime().ToString("yyyy-MM-dd HH:mm");  
            return s;
        }

        private string GetUnixTime(string sDateTime)
        {
            DateTime dt = DateTime.ParseExact(sDateTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal );
            string s = GetUnixTimestamp(dt.ToUniversalTime()).ToString();
            return s;
        }

        public long GetUnixTimestamp(DateTime value)
        {
            var timeSpan = (value - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }


        private void MenuItem(ActMenuItem actMenuItem)
        {
            String RC = Send("SelectMenuItem", actMenuItem.LocateBy.ToString(), actMenuItem.LocateValueCalculated, NA, NA, true);
            SetActionStatus(actMenuItem, RC);
        }

        private void ClickButton(ActButton actButton)
        {
            String RC = Send("Click", actButton.LocateBy.ToString(), actButton.LocateValueCalculated, NA, NA, true);
            SetActionStatus(actButton, RC);                      
        }

        private void RunTextBoxAction(ActTextBox actTextBox)
        {
            String RC = Send("SetControlValue", actTextBox.LocateBy.ToString(), actTextBox.LocateValueCalculated, NA, actTextBox.GetInputParamCalculatedValue("Value"), false);
            SetActionStatus(actTextBox, RC);            
        }
        
        public Bitmap TakeScreenShot(ActScreenShot actScreenShot)
        {                   
            String RC = Send("ScreenShot", NA, NA, NA, NA, false);
            if (RC.StartsWith("ERROR"))
            {
                throw new Exception("Error taking screen shot: " + RC);
            }
            Image imageTmp = Base64ToImage(RC);

            // Make sure to set status on Action screen shot only and not when action fail, keep the err
            SetActionStatus(actScreenShot, RC);               
            Bitmap bmp = new Bitmap(imageTmp);
            return bmp;
         }

        public ObservableList<Bitmap> TakeScreeShotAllWindows(ActScreenShot actScreenShot)
        {
            String RC = Send("CaptureAllWindows", NA, NA, NA, NA, false);
            if (RC.StartsWith("ERROR"))
            {
                throw new Exception("Error taking screen shot: " + RC);
            }
            SetActionStatus(actScreenShot, RC);
            string[] a;

            //This Additional check is to support old version of GTB which are using '@' delimiter. Can be removed later
            if (RC.Contains("~|"))
            {
                a = RC.Split('~');
            }
            else
            {
                a = RC.Split('@');
            }               

            Image imageTemp;
            Bitmap bmp;
            ObservableList<Bitmap> bmpList=new ObservableList<Bitmap>();
            int count = 0;
            foreach (string str in a)
            {
                if (count == 0) { count++; continue; }
                imageTemp=Base64ToImage(str);
                bmp = new Bitmap(imageTemp);
               bmpList.Add(bmp);
            }
                       
           return bmpList;
        }

        //Adding code to convert base64 string to image
        public Image Base64ToImage(String RC)
        {

            string[] a = RC.Split('|');
            string base64String = a[1];
            
            //TODO: check that RC is base64 - or use try catch before convert
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0,
              imageBytes.Length);
            
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);            

            return image;
        }

        //actSetConfig
        private void SetConfig(ActSetConfig actSetConfig)
        {
        }

        private void SetActionStatus(Act act,string RC)
        {
            if (RC.StartsWith("OK"))
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }            
            else
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.Error = "Error RC=" + RC;
            }
            act.ExInfo += RC;
        }

        private void SwitchWindow(ActSwitchWindow ActSwitchWindow)
        {
            String RC = Send("SwitchWindow", ActSwitchWindow.LocateBy.ToString(), ActSwitchWindow.LocateValueCalculated, NA, NA, false);
            SetActionStatus(ActSwitchWindow, RC);                        
        }

        private void ClickLink(ActLink Alink)
        {
            String RC = Send("LaunchAction", NA, Alink.LocateValueCalculated, NA, Alink.GetInputParamCalculatedValue("Value"), true);
            SetActionStatus(Alink, RC);                                    
        }
        
        // used for unit test only
        public string LaunchForm(string FormName)
        {
            String RC = Send("LaunchForm", "ByName" ,FormName, NA, NA, true);
            return RC;
        }

        public override string GetURL()
        {
            return "TBD";
        }

        //TODO: is it used?
        //public override List<ActWindow> GetAllWindows()
        //{
        //    String sWindows = Send("FormsList", NA, NA, NA, NA, false);
        //    String[] aWindows = sWindows.Split('|');
            
        //    List<ActWindow> Actwindows = new List<ActWindow>();
        //    foreach (string s in aWindows)
        //    {
        //        ActWindow al = new ActWindow();
        //        al.AddOrUpdateInputParamValue("Value",s);
        //        al.LocateBy = eLocateBy.ByName;
        //        al.LocateValue = s;
        //        Actwindows.Add(al);
        //    }
        //    return Actwindows;
        //}

        //public override List<ActLink> GetAllLinks()
        //{
        //    String sLinks = Send("FormActions", NA, NA, NA, NA, false);            
        //    String[] aLinks = sLinks.Split('|');
            
        //    List<ActLink> ActLinks = new List<ActLink>();
        //    foreach (string s in aLinks)
        //    {                
        //        ActLink al = new ActLink();
        //        al.Description = "Click Menu/Link - " + s;
        //        al.LocateBy = eLocateBy.ByName;
        //        al.LocateValue = s;
        //        al.AddOrUpdateInputParamValue("Value","com.amdocs.crm.workspace.SalesMenuActionsGlobal");
        //        ActLinks.Add(al);
        //    }

        //    return ActLinks;
        //}

        //public override List<ActButton> GetAllButtons()
        //{
        //    return null;
        //}

        public override void HighlightActElement(Act act)
        {
            Send("HighLightControl", act.LocateBy + "" , act.LocateValue, NA, NA, false);
        }

        internal string StartRecord()
        {
            String st = Send("StartRecording", NA, NA, NA, NA, false);
            //TODO: check RC
            return st;            
        }

        public void SetHighLightMode(bool p)
        {
            Send("SetHighlightMode", NA, NA, NA, "on", false);
            //TODO: check RC
        }

        /// <summary>
        /// Used for testing the speed of conenction in unit test
        /// </summary>
        /// <returns></returns>
        public string Echo(string txt)
        {
            string s = Send("Echo", NA, NA, NA, txt, false);
            return s;
        }
        public string KeyType(string txt)
        {
            string s = Send("Echo", NA, NA, NA, txt, false);
            return s;
        }

        public override ePlatformType Platform { get { return ePlatformType.ASCF; } }

        public override bool IsRunning()
        {
            return SocketConnected(clientSocket);
        }
        
        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();

            //TODO: get list of forms from the driver            

            String sWindows = Send("GetFormsList", NA, NA, NA, NA, false);
            if (!sWindows.StartsWith("OK"))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Error Getting forms list - " + sWindows);
                return null;
            }

            //take all excpet the first 3 chars - OK|
            String[] aWindows = sWindows.Substring(3).Split('|');

            foreach (string win in aWindows)
            {
                if (win.Length > 0)
                {
                    string[] wininfo = win.Split('^');
                    AppWindow AW = new AppWindow() { Title = wininfo[0], Path = wininfo[1], WindowType = AppWindow.eWindowType.ASCFForm };
                    list.Add(AW);
                }
            }
            return list;
        }       
        
        void IWindowExplorer.SwitchWindow(string Title)
        {
        }

        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            ASCFControlInfo CI = (ASCFControlInfo)ElementInfo;
            
            string RC = Send("HighLightControl", "ByName" + "", CI.Path, " ", " ", false);
            if (!RC.StartsWith("OK"))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Element Not found - path=" + CI.Path);
            }
            
            //TODO: fix later to get HTMLPage
            //else
            //{
            //    if (o is ASCFBrowserElementInfo)
            //    {
            //        ASCFBrowserElementInfo BC = (ASCFBrowserElementInfo)TVI.NodeObject();
            //        HighLightBrowserElement(BC);
            //    }
            //}
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            return null;
        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            return null;
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            return null;
        }

        string IWindowExplorer.GetFocusedControl()
        {
            string s = "";
            s = Send("GetFocusedControl", " ", " ", " ", " ", false);
            if (s.StartsWith("OK"))
            {
                s = s.Substring(3);
                return s;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, s);
                return null;
            }
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            string s = Send("GetControlUnderMouse", " ", " ", " ", " ", false);
            if (s.StartsWith("OK"))
            {
                s = s.Substring(3);

                ASCFElementInfo EI = new ASCFElementInfo();
                EI.XPath = s;
                return EI;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, s);
                return null;
            }
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            string RC = Send("GetActiveForm", " ", " ", " ", " ", false);
            
            if (RC.StartsWith("OK"))
            {
                
                string[] WinInfo = RC.Substring(3).Split('^');
                
                //fixme zzz
                AppWindow AW = new AppWindow() { Title = WinInfo[0], Path = WinInfo[1], WindowType = AppWindow.eWindowType.ASCFForm };
                return AW;
                
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Error in GetActiveForm - " + RC);
            }
            return null;
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            //TODO: impl
            return null;
        }

        public event Amdocs.Ginger.Plugin.Core.RecordingEventHandler RecordingEvent;
        
        void Amdocs.Ginger.Plugin.Core.IRecord.StartRecording(bool learnAdditionalChanges)
        {
            StartRecord();
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.StopRecording()
        {
            GetRecording();
            Send("StopRecording", NA, NA, NA, NA, false);
            //TODO: check RC            
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        public override void StartRecording()
        {
 	        StartRecord();
        }

        public override void StopRecording()
        {
            GetRecording();
            Send("StopRecording", NA, NA, NA, NA, false);
            //TODO: check RC            
        }

        private void GetRecording()
        {
            if (!SocketConnected(clientSocket))
            {
                Reporter.ToUser(eUserMsgKey.ASCFNotConnected);
                return;
            }

            string s;
            do
            {
                s = Send("GetRecording", NA, NA, NA, NA, false);
                
                if (s.StartsWith("ERROR"))
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, s);
                    break;
                }                
                CreateAction(s);

            } while (s != "NA");
        }

        private void CreateAction(string s)
        {
            //sample  TextControl:SetValue||ByName|AccountId|1
            string[] a = s.Split('|');
            string action = a[0];
            string locateBy = null;
            if (a.Length >= 2)
            {
                locateBy = a[1];
            }

            string locateValue = null;
            string value = null;
            string value2 = null;
            if (a.Length >= 3)
            {
                locateValue = a[2];
            }
            if (a.Length >= 4)
            {
                value = a[3];
            }

            if (a.Length >= 5)
            {
                value2 = a[4];
            }

            switch (action)
            {
                case "SetControlValue":
                    ActASCFControl AAC = new ActASCFControl();
                    SetActLocatorAndValue(AAC, locateBy, locateValue, value);
                    AAC.ControlAction = ActASCFControl.eControlAction.SetValue;
                    AAC.Description = "Set Control Value " + locateValue + " to " + value;
                    BusinessFlow.AddAct(AAC);
                    break;
                case "KeyType":
                    ActASCFControl AKT = new ActASCFControl();
                    SetActLocatorAndValue(AKT, locateBy, locateValue, value);
                    AKT.ControlAction = ActASCFControl.eControlAction.SetValue;
                    AKT.Description = "Send Key Strokes " + value + " to control " + locateValue;
                    BusinessFlow.AddAct(AKT);
                    break;
                case "SwitchWindow":
                    ActSwitchWindow ASW = new ActSwitchWindow();
                    SetActLocatorAndValue(ASW, locateBy, locateValue, value);
                    ASW.Description = "Switch Window to " + locateValue;
                    BusinessFlow.AddAct(ASW);
                    break;
                case "LaunchForm":
                case "LaunchAction":
                    //TODO: Temp until we get click menu
                    ActLink l = new ActLink();
                    SetActLocatorAndValue(l, locateBy, locateValue, value);
                    l.Description = "Launch Form/Action - " + value;
                    l.LinkAction = ActLink.eLinkAction.Click;
                    BusinessFlow.AddAct(l);
                    break;

                case "Click":
                    ActASCFControl AACGP1 = new ActASCFControl();
                    SetActLocatorAndValue(AACGP1, locateBy, locateValue, value);
                    AACGP1.ControlAction = ActASCFControl.eControlAction.Click;
                    AACGP1.Description = "Click " + locateValue;
                    BusinessFlow.AddAct(AACGP1);
                    break;
                case "SelectMenuItem":
                    ActMenuItem ACTMenu = new ActMenuItem();
                    SetActLocatorAndValue(ACTMenu, locateBy, locateValue, value);
                    ACTMenu.MenuAction = ActMenuItem.eMenuAction.Click;
                    ACTMenu.Description = "Select Menu Item - " + locateValue;
                    BusinessFlow.AddAct(ACTMenu);
                    break;
                case "Collapse":
                    ActASCFControl AACC = new ActASCFControl();
                    SetActLocatorAndValue(AACC, locateBy, locateValue, value);
                    AACC.ControlAction = ActASCFControl.eControlAction.Collapse;
                    AACC.Description = "Collapse " + locateValue;
                    BusinessFlow.AddAct(AACC);
                    break;
                case "Expand":
                    ActASCFControl AACE = new ActASCFControl();
                    SetActLocatorAndValue(AACE, locateBy, locateValue, value);
                    AACE.ControlAction = ActASCFControl.eControlAction.Expand;
                    AACE.Description = "Expand " + locateValue;
                    BusinessFlow.AddAct(AACE);
                    break;
                case "SetFocus":
                    ActASCFControl AACSF = new ActASCFControl();
                    SetActLocatorAndValue(AACSF, locateBy, locateValue, value);
                    AACSF.ControlAction = ActASCFControl.eControlAction.SetFocus;
                    AACSF.Description = "SetFocus " + locateValue;
                    BusinessFlow.AddAct(AACSF);
                    break;
                case "SetVisible":
                    ActASCFControl AACV = new ActASCFControl();
                    SetActLocatorAndValue(AACV, locateBy, locateValue, value);
                    AACV.ControlAction = ActASCFControl.eControlAction.SetVisible;
                    AACV.Description = "SetVisible " + locateValue;
                    BusinessFlow.AddAct(AACV);
                    break;
                case "SetWindowState":
                    ActASCFControl AACWS = new ActASCFControl();
                    SetActLocatorAndValue(AACWS, locateBy, locateValue, value);
                    AACWS.ControlAction = ActASCFControl.eControlAction.SetWindowState;
                    AACWS.Description = "SetWindowsState " + locateValue;
                    BusinessFlow.AddAct(AACWS);
                    break;
                case "GetControlProperty":
                    ActASCFControl AACGP = new ActASCFControl();
                    SetActLocatorAndValue(AACGP, locateBy, locateValue, value);
                    AACGP.ControlAction = ActASCFControl.eControlAction.GetControlProperty;
                    AACGP.Description = "GetControlProperty " + locateValue;
                    AACGP.ControlProperty = value2 == null ? GetControlPropertyFromString(value) : GetControlPropertyFromString(value2);
                    BusinessFlow.AddAct(AACGP);
                    break;
                case "GetMsgboxText":
                    ActGetMsgboxText actmsg = new ActGetMsgboxText();
                    SetActLocatorAndValue(actmsg, locateBy, locateValue, value);
                    actmsg.Description = "GetMsgboxText " + locateValue;
                    BusinessFlow.AddAct(actmsg);
                    break;
                case "Validate":
                    // this is special case which we translate to get property action with expected value
                    ActASCFControl AACValidation = new ActASCFControl();
                    SetActLocatorAndValue(AACValidation, locateBy, locateValue, value);
                    //TODO: Getvalue or other Ginger toolbox support commands
                    AACValidation.ControlAction = ActASCFControl.eControlAction.GetControlProperty;
                    AACValidation.Description = "Validate Control Property " + locateValue + "." + value;
                    AACValidation.AddOrUpdateReturnParamExpected(value, value2);

                    AACValidation.ControlProperty = GetControlPropertyFromString(value);
                    BusinessFlow.AddAct(AACValidation);
                    break;
                case "ActivateRow":
                    ActActivateRow AAR = new ActActivateRow();
                    SetActLocatorAndValue(AAR, locateBy, locateValue, value);
                    AAR.Description = "ActivateRow" + locateValue;
                    BusinessFlow.AddAct(AAR);
                    break;
                case "IsExist":
                    ActWindow IsEx = new ActWindow();
                    IsEx.WindowActionType = ActWindow.eWindowActionType.IsExist;
                    SetActLocatorAndValue(IsEx, locateBy, locateValue, value);
                    IsEx.Description = "IsExist" + locateValue;
                    BusinessFlow.AddAct(IsEx);
                    break;
                case "NA":
                    // Ignore...
                    break;

                default:
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Unknown Action: " + action + System.Environment.NewLine + s);
                    break;
            }
        }

        private ActASCFControl.eControlProperty GetControlPropertyFromString(string value2)
        {
            switch (value2)
            {
                case "Value":
                    return ActASCFControl.eControlProperty.Value;
                case "Text":
                    return ActASCFControl.eControlProperty.Text;
                case "Type":
                    return ActASCFControl.eControlProperty.Type;
                case "Enabled":
                    return ActASCFControl.eControlProperty.Enabled;
                case "Visible":
                    return ActASCFControl.eControlProperty.Visible;
                case "List":
                    return ActASCFControl.eControlProperty.List;
                case "ToolTip":
                    return ActASCFControl.eControlProperty.ToolTip;
                default:
                    //TODO: ERR
                    return ActASCFControl.eControlProperty.Value;
            }
        }

        private void SetActLocatorAndValue(Act act, string locateBy, string locateValue, string value)
        {
            switch (locateBy)
            {
                case "ByName":
                    act.LocateBy= eLocateBy.ByName;
                    break;
                default:
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Unknown Locate By: " + locateBy);
                    break;
            }
            act.LocateValue = locateValue;
            act.Value = value;
        }

        public void InjectHTMLSpy(string BrowserControlLocator)
        {
            string script = Properties.Resources.HTMLSpy;
            Send("InvokeScript", "ByName", BrowserControlLocator, NA, script, false);
        }

        public string ExecuteScriptOnBrowser(string script)
        {            
            string RC = Send("InvokeScript", "ByName", mBrowserLocateValue , NA, script, false);           
            return RC;
        }

        private void InjectGingerHTMLHelper()
        {            
            // TODO: if needed only!!! Jquery

            string script0 = Properties.Resources.jquery_min ;
            string RC0 = Send("InvokeScript", mBrowserLocateBy.ToString(), mBrowserLocateValue, NA, script0, false);


            // InjectXpathScript
            string script1 = GetXPathScript();
            string RC1 = Send("InvokeScript", mBrowserLocateBy.ToString(), mBrowserLocateValue, NA, script1, false);
            //TODO: check RC
            
            string script = Properties.Resources.GingerHTMLHelper ;

            string RC2 = "";

            string RC = Send("InvokeScript", "ByName", mBrowserLocateValue, NA, script, false);

            if (RC.StartsWith("OK"))
            {
                if (RC.Length > 3)
                {
                    RC2 = RC.Substring(3);
                }
                else
                {
                }
            }
            else
            {
                //TODO:
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Error - " + RC);
            }
        }

        //TODO: move to resource js like others
        //TODO: add check of broweser type, only IE need XPath
        private string GetXPathScript()
        {
            string script = Properties.Resources.wgxpath_install;
            script += "wgxpath.install();";
            script += "function getElementByXPath(xPath){";
            script += "var xPathRes = document.evaluate(xPath, document.body, null, XPathResult.ORDERED_NODE_ITERATOR_TYPE, null);";
            // Get the first element matching
            script += "var nextElement = xPathRes.iterateNext();";
            script += "return nextElement;";
            script += "};\n";
            return script;
        }

        public List<ASCFBrowserElementInfo> GetBrowserElements()
        {
            //TODO: change to PayLoad
            string ElementDeli = "_E_";
            string AttrValDeli = "_|_";
            string EquelDeli = "_=_";

            List<ASCFBrowserElementInfo> list = new List<ASCFBrowserElementInfo>();

            string script = "GetVisibleElements();";
            string RC = Send("InvokeScript", "ByName", mBrowserLocateValue, NA, script, false);

            if (RC.StartsWith("OK"))
            {
                string[] ElementsArray = RC.Split(new string[] { ElementDeli }, StringSplitOptions.None);
                foreach (string Element in ElementsArray)
                {
                    ASCFBrowserElementInfo CI = new ASCFBrowserElementInfo();
                    CI.ControlType = ASCFBrowserElementInfo.eControlType.Unknown;

                    string[] ElementsProperties = Element.Split(new string[] { AttrValDeli }, StringSplitOptions.None);
                    foreach (string prop in ElementsProperties)
                    {
                        string[] attra = prop.Split(new string[] { EquelDeli }, StringSplitOptions.None);

                        ControlProperty CP = new ControlProperty();
                        CP.Name = attra[0];
                        CP.Value = attra[1];
                        CI.Properties.Add(CP);
                    }
                    CI.SetInfo();
                    list.Add(CI);
                }
            }
            return list;
        }


        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {

        }

        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false)
        {
            //DOTO add grid view contol lists
            return new List<ElementInfo>();
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        void IWindowExplorer.UnHighLightElements()
        {
            throw new NotImplementedException();
        }


        bool IWindowExplorer.TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
        {
            throw new NotImplementedException();
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            throw new NotImplementedException();
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            throw new NotImplementedException();
        }

        public void StartSpying()
        {
            throw new NotImplementedException();
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            return EI;
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }
    }
}
