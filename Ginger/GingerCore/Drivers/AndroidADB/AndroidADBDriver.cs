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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Android;
using GingerCore.Actions.Common;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml;

namespace GingerCore.Drivers.AndroidADB
{
    public class AndroidADBDriver : DriverBase, IWindowExplorer, IVisualTestingDriver
    {

        public string mGingerPackageName = "amdocs.ginger.android";

        DeviceData mDevice; // The ADB device         

        string mPageSource;
        XmlDocument mPageSourceXml;

        GingerSocketClient mGingerSocket;

        Bitmap mLastScreenShot = null;

        public override bool IsSTAThread()
        {
            return true;
        }

        public enum eSwipeSide
        {
            Up, Down, Left, Right
        }

        public bool ConnectedToDevice=false;
        
        public override ePlatformType Platform { get { return ePlatformType.AndroidDevice; }} 

        //Mobile Agent configurations - We have dedicated Edit Page and not using the generic
        [UserConfigured]
        [UserConfiguredDefault("localhost")]   // Local host
        [UserConfiguredDescription("ADB Server Host - It can be on the local host or remote host")]
        public String ADBServerHost { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("5037")]   // Local host
        [UserConfiguredDescription("ADB Server Port - default is 5037")]
        public String ADBServerPort { get; set; }        
        
        [UserConfigured]
        // [UserConfiguredDescription("The tested device Model")]
        public String Model { get; set; }

        [UserConfigured]
        // [UserConfiguredDescription("The tested device Unique Serial identifier")]
        public String Serial { get; set; }

        private static TimeSpan INIT_TIMEOUT_SEC = TimeSpan.FromSeconds(90);
        private static TimeSpan IMPLICIT_TIMEOUT_SEC = TimeSpan.FromSeconds(10);
        private static TimeSpan SERVER_TIMEOUT_SEC = TimeSpan.FromSeconds(600);
        
        //TODO: add a switch which can keep it null when no UI is needed
        AndroidADBDriverWindow DriverWindow;

        string mDeviceConfigFolder;

        [UserConfigured]
        public string DeviceConfigFolder{ get; set; }

        public AndroidADBDriver(BusinessFlow BF, string DeviceConfigFolder)
        {
            BusinessFlow = BF;
            mDeviceConfigFolder = DeviceConfigFolder;            
        }

        public override void StartDriver()
        {
            CreateSTA(ShowDriverWindow);
        }

        public void ShowDriverWindow()
        {
            //show mobile window
            DriverWindow = new AndroidADBDriverWindow(this, mDeviceConfigFolder);
            DriverWindow.mBusinessFlow = BusinessFlow;            
            DriverWindow.DesignWindowInitialLook();
            DriverWindow.Show();

            try
            {
                ConnectedToDevice = ConnectToADB();
            }
            catch
            {
                // TODO: message unable to connect
            }

            if (ConnectedToDevice)
            {
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);
                Dispatcher = DriverWindow.Dispatcher;
                System.Windows.Threading.Dispatcher.Run();
            }
            else
            {
                if (DriverWindow != null)
                {                    
                    DriverWindow.CloseWindow();
                    DriverWindow = null;
                }
            }
        }

        public bool ConnectToADB()
        {
            AdbServer srv = GetADBServer();                

            var devices = AdbClient.Instance.GetDevices();

            if (devices.Count ==0)
            {
                throw new Exception("There are no devices connected, run 'ADB Devices' to verify you have the device connected");
            }
            
            // If user leave Model and Serial empty we pick the first device
            if (string.IsNullOrEmpty(Model) && string.IsNullOrEmpty(Serial))
            {
                mDevice = devices[0]; 
            }
            else            
            if (!string.IsNullOrEmpty(Serial))
            {
                // If we have Serial search for it
                mDevice = (from x in devices where x.Serial == Serial select x).FirstOrDefault();
            }
            else
            {
                // go by Model
                mDevice = (from x in devices where x.Model == Model select x).FirstOrDefault();
            }

            if (mDevice == null)
            {
                throw new Exception("Device not found - Model: " + Model + " ,Serial: " + Serial);
            }


            if (mDevice.State == DeviceState.Online)
            {
                InstallGingerAndroidServerOnDevice();
                DriverWindow.Title = "Android Device - " + mDevice.Model + " - " + mDevice.Serial;
                return true;
            }
            else
            {
                return false;
            }

           
        }

        private void InstallGingerAndroidServerOnDevice()
        {
            
            //TODO: add a flag if to stop/start
            // Meanwhile we start fresh clean every time
            if (IsGingerServiceRunning())
            {
                StopGingerService();                
            }
            
            // Step #2 We do port forward

            //TODO: fix the hard coded IPs/Port
            // Do ADB Port forward so socket on PC go to the device which is our Ginger Server running the the Andorid device
            // ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            // adb forward tcp:8888 tcp:7878


            //TODO: move to config
            AdbClient.Instance.CreateForward(mDevice,8888, 7878);
            
            // Step #1 - we start the test instrumentation                                       
            string ShellCommand = @"am instrument -w " + mGingerPackageName + ".test/android.support.test.runner.AndroidJUnitRunner";
            
            //TODO: need to exit nicely, below is async and will not stop...
            // AdbClient.Instance.ExecuteRemoteCommandAsync(ShellCommand, mDevice, receiver);
            CancellationToken CT = new CancellationToken();
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            Task t = AdbClient.Instance.ExecuteRemoteCommandAsync(ShellCommand, mDevice, receiver, CT, 0);            

            //TODO: check RC
            string rc = receiver.ToString();

            //TODO: add time out for test app to be ready max 30 sec

            //TODO: find how to know when device is ready and reduce the 3 secs wait
            Thread.Sleep(3000);
            
            while (!IsGingerServiceRunning())
            {
                General.DoEvents();
                Thread.Sleep(10);
            }            

            // Step #3 we check connectivity with Ginger Server running on the device - socket
            ConnectToDeviceSocket();

            CheckVersion();
            
            // #4 test comm/protocol version 

        }

        private void StopGingerService()
        {            
            string c = "am force-stop " + mGingerPackageName;
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand(c, mDevice, receiver);

            string rc1 = receiver.ToString(); // todo: verify if we get err here...

            while (IsGingerServiceRunning())
            {
                General.DoEvents();
                Thread.Sleep(100);
            }
        }

        private void CheckVersion()
        {            
            PayLoad PL = new PayLoad("GetVersion");
            PL.ClosePackage();
            PayLoad rc = SendPayLoad(PL);
            if (!rc.IsErrorPayLoad())
            {
                string ver = rc.GetValueString();

                if (ver != "v1.0.0")
                {
                    //TODO: ...
                }
            }
            else
            {
                throw new Exception ("CheckVersion: Error cannot get Version " + rc.ToString());
            }
        }

        private bool IsGingerServiceRunning()
        {
            ConsoleOutputReceiver receiver = new ConsoleOutputReceiver();
            AdbClient.Instance.ExecuteRemoteCommand("ps " + mGingerPackageName, mDevice, receiver);            
            string rc = receiver.ToString();
            
            if (rc.Contains(mGingerPackageName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ConnectToDeviceSocket()
        {
            // TODO: config for hard coded port 
            mGingerSocket = new GingerSocketClient();
            

            // TODO: make it config 
            mGingerSocket.Connect("127.0.0.1", 8888);

            mGingerSocket.Message += HandleMessage;

        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            switch (e.MessageType)
            {
                case GingerSocket.eProtocolMessageType.LostConnection:
                    if (DriverWindow != null)
                    {
                        DriverWindow.LostConenction();
                    }
                    break;
                case GingerSocket.eProtocolMessageType.PayLoad:
                    // coming here when async 
                    break;
            }
        }

        public static AdbServer GetADBServer()
        {
            AdbServer srv = new AdbServer(); // can be with IP !!!!

            string ADBFileName = GetADBFileName();
            srv.StartServer(ADBFileName, restartServerIfNewer: false);                        
            return srv;
        }

        public static string GetADBFileName()
        {
            string AndroidHome = System.Environment.GetEnvironmentVariable("ANDROID_HOME");

            if (string.IsNullOrEmpty(AndroidHome))
            {
                throw new Exception("Error: ANDROID_HOME not defined, please install Android Studio and setup ANDROID_HOME in Environment Variables");
            }

            string ADBFileName = AndroidHome + @"\platform-tools\adb.exe";

            if (!File.Exists(ADBFileName))
            {
                throw new Exception("Error: adb.exe Not found at: " + ADBFileName + ", ANDROID_HOME=" + AndroidHome);
            }
            else{
                return ADBFileName;
            }
        }


        public override void CloseDriver()
        {
            mGingerSocket.CloseConnection();

            StopGingerService();
            

            if (DriverWindow != null)
            {
                DriverWindow.Close();
                DriverWindow = null;
            }          
        }
        
        void UpdatePageSource()
        {
            PayLoad pl = new PayLoad("GetPageSource");
            pl.ClosePackage();

            PayLoad plrc = SendPayLoad(pl);

            byte[] bbb = plrc.GetBytes();
            mPageSource = System.Text.Encoding.UTF8.GetString(bbb);        
            
            mPageSourceXml = new XmlDocument();
            mPageSourceXml.LoadXml(mPageSource);
        }


        public string GetPageSource()
        {
            UpdatePageSource();            
            return mPageSource;
        }

        public AndroidElementInfo LocateElement(Act act)
        {
            //assume it is Xpath for now

            string LocValue = act.LocateValueCalculated;

            UpdatePageSource();

            XmlNode node = mPageSourceXml.SelectSingleNode(LocValue);
            
            AndroidElementInfo AEI = new AndroidElementInfo();
            AEI.XmlDoc = mPageSourceXml;
            AEI.XmlNode = node;
            return AEI;
        }

        public override Act GetCurrentElement()
        {
            //if (DriverPlatformType == ePlatformType.AndroidBrowser || DriverPlatformType == ePlatformType.iOSBrowser)
            //    return mSeleniumDriver.GetCurrentElement();

            return null;
            // return mSeleniumDriver.GetCurrentElement();
        }

        public override void RunAction(Act act)
        {
            try
            {                             
                Type ActType = act.GetType();
                
                if (ActType == typeof(ActUIElement))
                {
                    HandleActUIElement((ActUIElement)act);
                    return;
                }
                //TODO:  make obsolete
                if (ActType == typeof(ActGenElement))
                {
                    GenElementHandler((ActGenElement)act);
                    return;
                }
                if (ActType == typeof(ActScreenShot))
                {
                    TakeScreenShot(act);
                    return;
                }
                if (ActType == typeof(ActShell))
                {
                    RunShellCommand((ActShell)act);
                    return;
                }
                if (ActType == typeof(ActDeviceButton))
                {
                    HandleDeviceButton((ActDeviceButton)act);
                    return;
                }
                if (ActType == typeof(ActDevice))
                {
                    HandleDeviceAction((ActDevice)act);
                    return;
                }
                if (ActType == typeof(ActPhone))
                {
                    HandlePhoneAction((ActPhone)act);
                    return;
                }
                if (ActType == typeof(ActBattery))
                {
                    HandleBatteryAction((ActBattery)act);
                    return;
                }
                if (ActType == typeof(ActMedia))
                {
                    HandleMediaAction((ActMedia)act);
                    return;
                }

                if (ActType == typeof(ActVisualTesting))
                {
                    HandleActVisualTesting((ActVisualTesting)act);
                    return;
                }


                act.Error = "Run Action Failed due to unrecognized action type: '" + ActType.ToString() + "'";
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            catch(Exception ex)
            {
                act.Error = "Run Action Failed, Error details: " + ex.Message;
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
        }

        private void HandleActVisualTesting(ActVisualTesting act)
        {
            act.Execute(this);
        }

        private void HandleMediaAction(ActMedia actMedia)
        {
            PayLoad PL = new PayLoad("Media");
            switch (actMedia.MediaAction)
            {
                case ActMedia.eMediaAction.RecordAudio:                    
                    PL.ClosePackage();
                    PayLoad rc = SendPayLoad(PL);

                    if (rc.IsErrorPayLoad())
                    {
                        string ErrMSG = rc.GetValueString();
                        actMedia.Error = ErrMSG;
                    }
                    else
                    {
                        string FileName = rc.GetValueString();
                        byte[] data = rc.GetBytes();
                        File.WriteAllBytes(@"c:\temp\" + FileName, data);
                    }

                    break;
                case ActMedia.eMediaAction.RecordVideo:                    
                    PL.ClosePackage();
                    PayLoad rc2 = SendPayLoad(PL);
                    break;
                
            }
        }

        private void HandleBatteryAction(ActBattery actBattery)
        {
            PayLoad PL = new PayLoad("Battery");

            switch (actBattery.BatteryAction)
            {
                case ActBattery.eBatteryAction.GetPowerStatus:
                PL.ClosePackage();

                PayLoad rc = SendPayLoad(PL);
                if (rc.IsErrorPayLoad())
                {
                    string ErrMSG = rc.GetValueString();
                    actBattery.Error = ErrMSG;
                }   
                else
                {
                    string s = rc.GetValueString();
                    actBattery.AddOrUpdateReturnParamActual("BatteryLevel", s);
                }
                break;

                // TODO: the rest
            }
        }

        private void HandlePhoneAction(ActPhone actPhone)
        {
            PayLoad PL = new PayLoad("Phone");

            switch (actPhone.PhoneAction)
            {
                case ActPhone.ePhoneAction.Dial:
                    PL.AddValue(actPhone.PhoneAction.ToString());
                    PL.AddValue(actPhone.GetInputParamCalculatedValue(ActPhone.Fields.PhoneNumber));            
                    PL.ClosePackage();
                    break;
                case ActPhone.ePhoneAction.EndCall:
                    PL.AddValue(actPhone.PhoneAction.ToString());                    
                    PL.ClosePackage();
                    break;

                    // TODO: the rest
            }
            PayLoad rc = SendPayLoad(PL);

            if (rc.IsErrorPayLoad())
            {
                string ErrMSG = rc.GetValueString();
                actPhone.Error = ErrMSG;
            }       
        }

        private void HandleDeviceAction(ActDevice actDevice)
        {

            // TODO: send device action

//             PayLoad PL = new PayLoad("DeviceAction");
            PayLoad PL = new PayLoad("ScreenRecord");

            // PL.AddEnumValue(actDevice.DeviceAction);
            PL.AddValue(actDevice.GetInputParamCalculatedValue(ActDevice.Fields.Size));
            PL.AddValue(int.Parse(actDevice.GetInputParamCalculatedValue(ActDevice.Fields.BitRate)));            
            PL.AddValue(int.Parse(actDevice.GetInputParamCalculatedValue(ActDevice.Fields.TimeLimit)));
            PL.ClosePackage();

            PayLoad rc = SendPayLoad(PL);

            if (rc.IsErrorPayLoad())
            {
                string ErrMSG = rc.GetValueString();
                actDevice.Error = ErrMSG;
            }        
        }

        private void HandleActUIElement(ActUIElement act)
        {
            PayLoad PL = act.GetPayLoad();

            PayLoad rc = SendPayLoad(PL);

            if (rc.IsErrorPayLoad())
            {
                string ErrMSG = rc.GetValueString();
                act.Error = ErrMSG;
            }        
    
        }

        private void HandleDeviceButton(ActDeviceButton act)
        {
            if (act.DeviceButtonAction == ActDeviceButton.eDeviceButtonAction.Press)
            {
                Press(act.PressKey.ToString());  
            }
            else if (act.DeviceButtonAction == ActDeviceButton.eDeviceButtonAction.PressKeyCode)
            {
                PressKeyCode(int.Parse(act.Value));
            }
                // Create input action: keyboard/mouse...
            else if (act.DeviceButtonAction == ActDeviceButton.eDeviceButtonAction.Input)
            {
                ExecuteShellCommand("input text " + act.ValueForDriver);
            }

            // UpdatePageSource();
        }

        private void RunShellCommand(ActShell actShell)
        {
            string rc = ExecuteShellCommand(actShell.ValueForDriver);

            string[] a = rc.Split(new string[] {"\n"}, StringSplitOptions.None);

            actShell.ReturnValues.Clear();
            int i = 1;
            foreach (string s in a)
            {
                if (string.IsNullOrEmpty(s)) continue;
                ActReturnValue ARV = new ActReturnValue();
                ARV.Actual = s;
                ARV.Path = i + "";
                ARV.Param = "Output Line";
                actShell.ReturnValues.Add(ARV);
                i++;
            }
            actShell.ExInfo = rc;

        }


        //TODO: Obsolete - OLD!!!!!!!!!!!!!!!!!!!!!!!!
        private void GenElementHandler(ActGenElement act)
        {
            PayLoad PL = new PayLoad("UIElementAction");
            //TODO: fixme enum to work
            // PL.AddEnumValue(act.LocateBy);
            PL.AddValue(act.LocateBy.ToString());
            PL.AddValue(act.LocateValueCalculated);
            //TODO: fixme enum to work
            // PL.AddEnumValue(act.GenElementAction);
            PL.AddValue(act.GenElementAction.ToString());
            List <PayLoad> PLParams = new List <PayLoad>();
            foreach (ActInputValue AIV in act.InputValues)
            {
                PayLoad AIVPL = new PayLoad("AIV", AIV.Param, AIV.ValueForDriver);
                PLParams.Add(AIVPL);
                //AIVPL.ClosePackage();
            }
            PL.AddListPayLoad(PLParams);
            PL.ClosePackage();
            PayLoad rc =  SendPayLoad(PL);

            // TODO: update the action with pass fail or err
            if (rc.IsErrorPayLoad())
            {

            }
            else
            {

            }
          
        }
        
        public override void HighlightActElement(Act act)
        {
        }        

        private void TakeScreenShot(Act act)
        {
            act.AddScreenShot(GetScreenShot());           
        }

        public void TapXY(long x, long y)
        {
            
        }

      

        public void SwipeScreen(int x, int y, int ex, int ey, int steps)
        {
            PayLoad pl = new PayLoad("Swipe");
            pl.AddValue(x);
            pl.AddValue(y);
            pl.AddValue(ex);
            pl.AddValue(ey);
            pl.AddValue(steps);
            pl.ClosePackage();
            PayLoad plrc = SendPayLoad(pl);            
        }


        public void SwipeScreen(eSwipeSide side)
        {
            //ITouchAction swipe;
            //Size sz = Driver.Manage().Window.Size;

            //switch (side)
            //{
            //    case eSwipeSide.Down:
            //        swipe = BuildDragAction(Driver, (int)(sz.Width * 0.5), (int)(sz.Height * 0.3), (int)(sz.Width * 0.5), (int)(sz.Height * 0.7), 1000);
            //        swipe.Perform();
            //        break;

            //    case eSwipeSide.Up:
            //        swipe = BuildDragAction(Driver, (int)(sz.Width * 0.5), (int)(sz.Height * 0.7), (int)(sz.Width * 0.5), (int)(sz.Height * 0.3), 1000);
            //        swipe.Perform();
            //        break;

            //    case eSwipeSide.Left:
            //        swipe = BuildDragAction(Driver, (int)(sz.Width * 0.8), (int)(sz.Height * 0.5), (int)(sz.Width * 0.1), (int)(sz.Height * 0.5), 1000);
            //        swipe.Perform();
            //        break;

            //    case eSwipeSide.Right:
            //        swipe = BuildDragAction(Driver, (int)(sz.Width * 0.1), (int)(sz.Height * 0.5), (int)(sz.Width * 0.8), (int)(sz.Height * 0.5), 1000);
            //        swipe.Perform();
            //        break;
            //}
        }

        //public ITouchAction BuildDragAction(AppiumDriver<AppiumWebElement> driver, int startX, int startY, int endX, int endY, int duration)
        //{
        //    ITouchAction touchAction = new TouchAction(driver)
        //        .Press(startX, startY)
        //        .Wait(duration)
        //        .MoveTo(endX, endY)
        //        .Release();

        //    return touchAction;
        //}

        //public void DoDrag(int startX, int startY, int endX, int endY)
        //{
        //    TouchAction drag =  new TouchAction(Driver);
        //    drag.Press(startX, startY).MoveTo(endX, endY).Release();
        //    drag.Perform();
        //}

        public override string GetURL()
        {
            return "TBD";
        }

        public override List<ActWindow> GetAllWindows()
        {
            return null;
        }

        public override List<ActLink> GetAllLinks()
        {
            return null;
        }

        public override List<ActButton> GetAllButtons()
        {
            return null;
        }

        public override bool IsRunning()
        {
            return ConnectedToDevice;           
        }

        
        //IWindowExplorer impl

        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            List<AppWindow> list = new List<AppWindow>();
            
            AppWindow AW = new AppWindow();
            AW.WindowType = AppWindow.eWindowType.AndroidDevice;
            AW.Title = "Device";   // TODO: add device name and info
            

            list.Add(AW);
            
            return list;
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            //NA
        }

        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.HighLightElement((AndroidElementInfo)ElementInfo);
            });
        }

        string IWindowExplorer.GetFocusedControl()
        {
            return null;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            AndroidElementInfo AEI = null;
            XmlNode node = GetElementXmlNodeFromMouse();
            if (node != null)
            {
                AEI = new AndroidElementInfo();
                AEI.XPath = GetXPathToNode(node);
                AEI.XmlNode = node;
            }
            return AEI;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            return null;
        }
        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null)
        {
            List<ElementInfo> list = new List<ElementInfo>();

            UpdatePageSource();
            //Get all elements but onlyy clickable elements= user can interact with them
            XmlNodeList nodes = mPageSourceXml.SelectNodes("//*");
            for (int i = 0; i < nodes.Count; i++)
            {
                //Show only clickable elements
                if (nodes[i].Attributes != null)
                {
                    var cattr = nodes[i].Attributes["clickable"];
                    if (cattr != null)
                    {
                        if (cattr.Value == "false") continue;
                    }
                }
                AndroidElementInfo AEI = GetElementInfoforXmlNode(nodes[i]);
                list.Add(AEI);
            }

            return list;
        }

        private AndroidElementInfo GetElementInfoforXmlNode(XmlNode xmlNode)
        {
            AndroidElementInfo AEI = new AndroidElementInfo();
            AEI.ElementTitle = GetNameFor(xmlNode);
            AEI.ElementType = GetAttrValue(xmlNode, "class");
            AEI.Value = GetAttrValue(xmlNode, "text");
            if (string.IsNullOrEmpty(AEI.Value))
            {
                AEI.Value = GetAttrValue(xmlNode, "content-desc");
            }
            AEI.XmlNode = xmlNode;
            AEI.XPath = GetNodeXPath(xmlNode);
            AEI.WindowExplorer = this;

            return AEI;
        }

        private string GetNodeXPath(XmlNode xmlNode)
        {
            string XPath = GetXPathToNode(xmlNode);
            return XPath;
        }


        /// Gets the X-Path to a given Node
        /// </summary>
        /// <param name="node">The Node to get the X-Path from</param>
        /// <returns>The X-Path of the Node</returns>
        public string GetXPathToNode(XmlNode node)
        {
            //TODO: verify XPath return 1 item back to same xmlnode.

            string resid = GetAttrValue(node, "resource-id");
            if (!string.IsNullOrEmpty(resid))
            {
                return string.Format("//*[@resource-id='{0}']", resid);
            }
            
            if (node.ParentNode == null)
            {
                // the only node with no parent is the root node, which has no path
                return "";
            }

            // Get the Index
            int indexInParent = 1;
            XmlNode siblingNode = node.PreviousSibling;
            // Loop thru all Siblings
            while (siblingNode != null)
            {
                // Increase the Index if the Sibling has the same Name
                if (siblingNode.Name == node.Name)
                {
                    indexInParent++;
                }
                siblingNode = siblingNode.PreviousSibling;
            }

            // the path to a node is the path to its parent, plus "/node()[n]", where n is its position among its siblings.         
            return String.Format("{0}/{1}[{2}]", GetXPathToNode(node.ParentNode), node.Name, indexInParent);
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {

            AndroidElementInfo EI = (AndroidElementInfo)ElementInfo;
            XmlNode node = EI.XmlNode;

            List<ElementInfo> list = new List<ElementInfo>();


            XmlNodeList nodes = node.ChildNodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                AndroidElementInfo AEI = GetElementInfoforXmlNode(nodes[i]);
                list.Add(AEI);
            }
            

            return list;
        }

        private string GetNameFor(XmlNode xmlNode)
        {
            string Name = GetAttrValue(xmlNode, "content-desc");
            if (!string.IsNullOrEmpty(Name)) return Name;

            string resid = GetAttrValue(xmlNode, "resource-id");
            if (!string.IsNullOrEmpty(resid))
            {
                // if we have resource id then get just the id out of it
                string[] a = resid.Split('/');
                Name = a[a.Length-1];
                return Name;
            }

            Name = GetAttrValue(xmlNode, "text");
            if (!string.IsNullOrEmpty(Name)) return Name;
            
            return xmlNode.Name;
            
        }

        string GetAttrValue(XmlNode xmlNode, string attr)
        {
            if (xmlNode.Attributes == null) return null;
            if (xmlNode.Attributes[attr] == null) return null;
            if (string.IsNullOrEmpty(xmlNode.Attributes[attr].Value)) return null;
            return xmlNode.Attributes[attr].Value;
        }
        
        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            ObservableList<ElementLocator> list = new ObservableList<ElementLocator>();

            AndroidElementInfo AEI = (AndroidElementInfo)ElementInfo;

            //// Show XPath, can have relative info
            list.Add(new ElementLocator()
            {
                LocateBy = eLocateBy.ByXPath,
                LocateValue = AEI.XPath,
                Help = "Highly Recommended when resourceid exist, long path with relative information is sensitive to screen changes"
            });


            //Only by Resource ID
            string resid = GetAttrValue(AEI.XmlNode, "resource-id");
            string residXpath = string.Format("//*[@resource-id='{0}']", resid);
            if (residXpath != AEI.XPath) // We show by res id when it is differnet then the elem XPath, so not to show twice the same, the AE.Apath can include relative info
            {
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = residXpath,
                    Help = "Use Resource id only when you don't want XPath with relative info, but the resource-id is unique"
                });
            }

            //By Content-desc
            string contentdesc = GetAttrValue(AEI.XmlNode, "content-desc");
            if (!string.IsNullOrEmpty(contentdesc))
            {
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = string.Format("//*[@content-desc='{0}']", contentdesc),
                    Help = "content-desc is Recommended when resource-id not exist"
                });
            }

            // By Class and text
            string eClass = GetAttrValue(AEI.XmlNode, "class");
            string eText = GetAttrValue(AEI.XmlNode, "text");
            if (!string.IsNullOrEmpty(eClass) && !string.IsNullOrEmpty(eText))
            {
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = string.Format("//{0}[@text='{1}']", eClass, eText),    // like: //android.widget.RadioButton[@text='Ginger']" 
                    Help = "use class and text when you have list of items and no resource-id to use"
                });
            }
            
            list.Add(new ElementLocator()
            {
                LocateBy = eLocateBy.ByXPath,
                LocateValue = string.Format("class='{0},text contains=123'", "aaa", "bbb"),
                Help = "Recommended if several attrs and no resource-id exist"
            });
            
            return list;
        }

        // Get the data of the element
        // For Combo box: will return all valid values - options avaialble - List<ComboBoxElementItem>
        // For Table: will return list of rows data: List<TableElementItem>        
        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            return null;
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            XmlNode node = ((AndroidElementInfo)ElementInfo).XmlNode;

            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

            if (node == null) return list;

            XmlAttributeCollection attrs = node.Attributes;

            if (attrs == null) return list;

            for (int i = 0; i < attrs.Count;i++ )
            {
                ControlProperty CP = new ControlProperty();
                CP.Name = attrs[i].Name;
                CP.Value = attrs[i].Value;
                list.Add(CP);
            }

            return list;
            ;
        }
        
        bool IWindowExplorer.AddSwitchWindowAction(string Title)
        {
            return false;
        }

        public override void StartRecording()
        {            
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StartRecording();
            });            
        }

        public override void StopRecording()
        {         
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StopRecording();
            });   
        }


        public Bitmap GetScreenShot()
        {
            // Check if we want to use it if we have rooted device and want to get secured screen shots
            //    var img = AdbClient.Instance.GetFrameBufferAsync(mDevice, CancellationToken.None);                 
            //    img.Wait(5000);  //Wait max 5 sec to get the bitmap screen shot
            //    return (Bitmap)img.Result;

            PayLoad pl = new PayLoad("GetScreenShot");
            pl.ClosePackage();
            PayLoad plrc = SendPayLoad(pl);


            // if there new screen shot use it, else we will use cache - the last screen shot taken
            if (plrc.Name == "ScreenShot")
            { 
                byte[] imageData = plrc.GetBytes();
                MemoryStream ms = new MemoryStream(imageData);
                mLastScreenShot = new Bitmap(ms);
            }
            return mLastScreenShot;
        }

        internal string ExecuteShellCommand(string ShellCommand)
        {                        
            PayLoad pl = new PayLoad("Shell", ShellCommand);
            PayLoad plrc = SendPayLoad(pl);
            string rc = plrc.GetValueString();
            return rc;
        }

        internal string DownLoadFileFromDevice(string FileName)
        {
            string LocalFileName = Path.GetTempFileName();
            var device = AdbClient.Instance.GetDevices().First();

                using (SyncService service = new SyncService(device))
                using (System.IO.Stream stream = File.OpenWrite(LocalFileName))
                {
                    service.Pull(FileName, stream, null, CancellationToken.None);     
                }

                return System.IO.File.ReadAllText(LocalFileName);
        }
        
        void InstallApplication(string APKFileName, bool reinstall = false)
        {      
            //FIXME not working
            // PackageManager manager = new PackageManager(mDevice);
            // manager.InstallPackage(APKFileName, reinstall);

            // -r = reinstall            
            string p = "-s " + mDevice.Serial + " install -r " + APKFileName;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetADBFileName(),
                    Arguments = p
                }
            };
            process.Start();
            process.WaitForExit();
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }
        
        private PayLoad SendPayLoad(PayLoad pl)
        {
            return mGingerSocket.SendPayLoad(pl);
        }
       
        // Used for live view
        internal BitmapImage GetScreenShotAsBitmapImage()
        {            

            PayLoad pl = new PayLoad("GetScreenShot");
            pl.ClosePackage();
            PayLoad plrc = SendPayLoad(pl);
            if (plrc.Name == "ScreenShot")
            {
                byte[] imageData = plrc.GetBytes();
                
                // Keep last screen shot for cache
                MemoryStream ms = new MemoryStream(imageData);
                mLastScreenShot = new Bitmap(ms);
                return ByteToImage(imageData);
            }
            else
            {
                // if screen is same then the Payload message will be ScreenShotIsSame and we return null = no change
                return null;
            }
        }

        public BitmapImage ByteToImage(byte[] imageData)
        {
            BitmapImage biImg = new BitmapImage();
            MemoryStream ms = new MemoryStream(imageData);
            biImg.BeginInit();
            biImg.StreamSource = ms;
            biImg.EndInit();            
            return biImg;
        }


        public void ClickXY(int x, int y)
        {            
            PayLoad pl = new PayLoad("ClickXY");
            pl.AddValue(x);
            pl.AddValue(y);
            pl.ClosePackage();
            PayLoad plrc = SendPayLoad(pl);            
        }

        internal void Press(string key)
        {
            PayLoad pl = new PayLoad("Press", key);
            PayLoad plrc = SendPayLoad(pl);
        }

        internal void PressKeyCode(int KeyCode)
        {
            PayLoad pl = new PayLoad("PressKeyCode", KeyCode);
            PayLoad plrc = SendPayLoad(pl);
        }


        public XmlNode FindElementXmlNodeByXY(double pointOnMobile_X, double pointOnMobile_Y)
        {

            try
            {
                //get screen elements nodes
                XmlNodeList ElmsNodes;


                // Do once?
                // if XMLSOurce changed we need to refresh
                //pageSourceString = AppiumDriver.GetPageSource();                                
                //pageSourceXml = new XmlDocument();
                //pageSourceXml.LoadXml(pageSourceString);

                // pageSourceXMLViewer.xmlDocument = pageSourceXml;                   

                ElmsNodes = mPageSourceXml.SelectNodes("//*");

                ///get the selected element from screen
                if (ElmsNodes != null && ElmsNodes.Count > 0)
                {
                    //move to collection for getting last node which fits to bounds
                    ObservableList<XmlNode> ElmsNodesColc = new ObservableList<XmlNode>();
                    foreach (XmlNode elemNode in ElmsNodes)
                    {                        
                        bool skipElement = false;
                        string FilterElementsChK = "";
                        if (FilterElementsChK.Length > 0)
                        {
                            string[] filterList = FilterElementsChK.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            try
                            {
                                for (int indx = 0; indx < filterList.Length; indx++)
                                    if (elemNode.Name.Contains(filterList[indx].Trim()) ||
                                           elemNode.LocalName.Contains(filterList[indx].Trim()))
                                    {
                                        skipElement = true;
                                        break;
                                    }
                            }
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                        }

                        if (!skipElement)
                            ElmsNodesColc.Add(elemNode);
                    }

                    Dictionary<XmlNode, long> foundElements = new Dictionary<XmlNode, long>();
                    foreach (XmlNode elementNode in ElmsNodesColc.Reverse())
                    {
                        //get the element location
                        long element_Start_X = -1;
                        long element_Start_Y = -1;
                        long element_Max_X = -1;
                        long element_Max_Y = -1;

                      
                      
                        try
                        {
                            string bounds = elementNode.Attributes["bounds"].Value;
                            bounds = bounds.Replace("[", ",");
                            bounds = bounds.Replace("]", ",");
                            string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (boundsXY.Count() == 4)
                            {
                                element_Start_X = Convert.ToInt64(boundsXY[0]);
                                element_Start_Y = Convert.ToInt64(boundsXY[1]);
                                element_Max_X = Convert.ToInt64(boundsXY[2]);
                                element_Max_Y = Convert.ToInt64(boundsXY[3]);
                            }
                        }
                        catch (Exception ex)
                        {
                            element_Start_X = -1;
                            element_Start_Y = -1;
                            element_Max_X = -1;
                            element_Max_Y = -1;
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                        }
                      


                        if (((pointOnMobile_X >= element_Start_X) && (pointOnMobile_X <= element_Max_X))
                                   && ((pointOnMobile_Y >= element_Start_Y) && (pointOnMobile_Y <= element_Max_Y)))
                        {
                            foundElements.Add(elementNode, ((element_Max_X - element_Start_X) * (element_Max_Y - element_Start_Y)));
                        }
                    }

                    //getting the smalles node size found
                    XmlNode foundNode = null;
                    long foundNodeSize = 0;
                    if (foundElements.Count > 0)
                    {
                        foundNode = foundElements.Keys.First();
                        foundNodeSize = foundElements.Values.First();
                    }
                    for (int indx = 0; indx < foundElements.Keys.Count; indx++)
                    {
                        if (foundElements.Values.ElementAt(indx) < foundNodeSize)
                        {
                            foundNode = foundElements.Keys.ElementAt(indx);
                            foundNodeSize = foundElements.Values.ElementAt(indx);
                        }
                    }
                    if (foundNode != null)
                        return foundNode;
                }

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                return null;
            }
        }



    


    public XmlNode GetElementXmlNodeFromMouse()
        {
            System.Windows.Point pointOnImage = DriverWindow.GetMousePositionOnDevice();
            return FindElementXmlNodeByXY((long)pointOnImage.X, (long)pointOnImage.Y);
        }




    public ActUIElement GetActionForClickedElement(double pointOnMobile_X, double pointOnMobile_Y)
    {
        UpdatePageSource();
        ActUIElement elemntAct = new ActUIElement() { Active = true, ElementAction = ActUIElement.eElementAction.Click };
                
        XmlNode clickedElementNode = FindElementXmlNodeByXY(pointOnMobile_X, pointOnMobile_Y);
            
        if (clickedElementNode != null)
        {
            string EClass = clickedElementNode.Attributes["class"].Value;
            // Need translate table 
            SetActionElementTypeByClass(elemntAct, EClass);            

            // Create description
            string desc = "Click ";
            string txt = clickedElementNode.Attributes["text"].Value;
            if (string.IsNullOrEmpty(txt))
            {
                txt = clickedElementNode.Attributes["content-desc"].Value;
            }
            desc += "'" + txt + "'";
            elemntAct.Description = desc;

            SetActionElementLocator(elemntAct, clickedElementNode);

            return elemntAct;
        }
        else
        {
            //identify by X,Y as last option 
            elemntAct.ElementLocateBy = eLocateBy.ByXY;

            elemntAct.SetLocateByXYValues(pointOnMobile_X, pointOnMobile_Y);            

            return elemntAct;                       
        }
        
    }

    private void SetActionElementTypeByClass(ActUIElement elemntAct, string EClass)
    {
        switch (EClass)
        {
            case "android.widget.Button":
                elemntAct.ElementType = eElementType.Button;
                break;
            case "android.widget.ImageButton":
                elemntAct.ElementType = eElementType.Button;  // ImageButton new enum ??
                break;
            case "android.widget.CheckBox":
                elemntAct.ElementType = eElementType.CheckBox;
                break;
            case "android.widget.RadioButton":
                elemntAct.ElementType = eElementType.RadioButton;
                break;

            default:
                elemntAct.ElementType = eElementType.Unknown;
                break;
        }
    }

    private void SetActionElementLocator(ActUIElement elemntAct, XmlNode clickedElementNode)
    {
         //try to identify best lcoator by priority nad validating it is unique
                
            //#1 - resource-id                
            string resourceid = GetAttrValue(clickedElementNode, "resource-id");
            if (!string.IsNullOrEmpty(resourceid))
            {
                string xpath = "//*[@resource-id='" + resourceid + "']";
                if (IsUnique(xpath))
                {
                    elemntAct.ElementLocateBy = eLocateBy.ByResourceID;
                    elemntAct.ElementLocateValue = resourceid;
                    return;
                }                    
            }

            //#2 - Content Description
            string contentdesc = GetAttrValue(clickedElementNode, "content-desc");
            if (!string.IsNullOrEmpty(contentdesc))
            {
                string xpath = "//*[@content-desc='" + contentdesc + "']";
                if (IsUnique(xpath))
                {
                    elemntAct.ElementLocateBy = eLocateBy.ByContentDescription;
                    elemntAct.ElementLocateValue = contentdesc;
                    return;
                }
                // TODO: if not unique we can use index if we have <=3 elems
            }

            //#3 - Text
            string text = GetAttrValue(clickedElementNode, "text");
            if (!string.IsNullOrEmpty(text))
            {
                string xpath = "//*[@text='" + text + "']";
                if (IsUnique(xpath))
                {
                    elemntAct.ElementLocateBy = eLocateBy.ByText;
                    elemntAct.ElementLocateValue = text;
                    return;
                }
            }

            // #4 try multiple attrs

            //#5 - create Xpath

            string XPath = GetelementXpath(clickedElementNode);
            elemntAct.ElementLocateBy = eLocateBy.ByXPath;
            elemntAct.ElementLocateValue = XPath;
    }

    private string GetelementXpath(XmlNode clickedElementNode)
    {
        //TODO: FIXME
        return "1.2.3.";

            //string element_Xpath;
            ////get element class
            //string element_Class;
            //element_Class = clickedElementNode.LocalName;//TO DO: validate!!
            //if (element_Desc_value != element_Text_value)
            //    element_Xpath = "*//" + element_Class +
            //                        "[@" + element_ID_attrib + "='" + element_ID_value + "' and " +
            //                        "@" + element_Text_attrib + "='" + element_Text_value + "' and " +
            //                        "@" + element_Desc_attrib + "='" + element_Desc_value + "']";
            //else
            //    element_Xpath = "*//" + element_Class +
            //                        "[@" + element_ID_attrib + "='" + element_ID_value + "' and " +
            //                        "@" + element_Text_attrib + "='" + element_Text_value + "']";
            //if (!string.IsNullOrEmpty(element_Xpath))
            //{
            //    //check if unique:
            //    validateElms = mPageSourceXml.SelectNodes(element_Xpath);
            //    if (validateElms != null && validateElms.Count != 1)
            //    {
            //        //add index
            //        string element_Xpath_Orig = element_Xpath;

            //        string element_Index;
            //        try
            //        {
            //            element_Index = clickedElementNode.Attributes["index"].Value;
            //        }
            //        catch { element_Index = string.Empty; }
            //        string element_Instance;
            //        try
            //        {
            //            element_Instance = clickedElementNode.Attributes["instance"].Value;
            //        }
            //        catch { element_Instance = string.Empty; }
            //        if (!string.IsNullOrEmpty(element_Index) || !string.IsNullOrEmpty(element_Instance))
            //            element_Xpath = element_Xpath.Replace("']", "' and @index='" + element_Index + "' and @instance='" + element_Instance + "']");


            //        if (element_Xpath_Orig != element_Xpath)
            //        {
            //            //check if unique
            //            validateElms = mPageSourceXml.SelectNodes(element_Xpath);
            //            if (validateElms != null && validateElms.Count == 1)
            //            {
            //                elemntAct.LocateBy = Act.eLocatorType.ByXPath;
            //                elemntAct.LocateValue = element_Xpath;
            //                return;
            //            }
            //        }
            //    }
            //    else
            //    {
            //        elemntAct.LocateBy = Act.eLocatorType.ByXPath;
            //        elemntAct.LocateValue = element_Xpath;
            //        return;
            //    }
            //}
        }

        private bool IsUnique(string xpath)
        {
            XmlNodeList validateElms = mPageSourceXml.SelectNodes(xpath);
            if (validateElms != null && validateElms.Count == 1)
            {            
                return true;
            }
            else
            {
                return false;
            }
        }

        public void GetInstallApps()
        {
            // mDevice.ListProcesses
        }

        //TODO: new function to be added to driver who impl VisualCompare, will save the info to folder with all images and page source of visible elements
        public void GetVisualPageInfo()
        {
                    XmlNodeList ElmsNodes;
                    ElmsNodes = mPageSourceXml.SelectNodes("//*");
                                
                    ObservableList<XmlNode> ElmsNodesColc = new ObservableList<XmlNode>();
                    foreach (XmlNode elemNode in ElmsNodes)
                    {   
                        // check visible else continue

                        // get location: x, y, w, h

                        // get relative grid location in 10x10 - for compare in other resolution

                        // take from screen shot the piece
                        // elemNode.Attributes[].Value

                        // get text
                    }
        
        }

        Bitmap IVisualTestingDriver.GetScreenShot()
        {
            return GetScreenShot();
        }

        VisualElementsInfo IVisualTestingDriver.GetVisualElementsInfo()
        {
            return null;
        }

        void IVisualTestingDriver.ChangeAppWindowSize(int Width, int Height)
        {
            // NA for Mobile
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {
            
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        public void UnHighLightElements()
        {
            throw new NotImplementedException();
        }

        public bool TestElementLocators(ObservableList<ElementLocator> elementLocators, bool GetOutAfterFoundElement = false)
        {
            throw new NotImplementedException();
        }

        //TODO: Phone state
        // Wait for incoming call
        // http://stackoverflow.com/questions/15563921/how-to-detect-incoming-calls-in-an-android-device
    }    
}
