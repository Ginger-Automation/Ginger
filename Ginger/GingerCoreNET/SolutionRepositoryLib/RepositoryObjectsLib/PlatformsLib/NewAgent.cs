//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Repository;
//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.CoreNET.RosLynLib;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.PlugInsLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerPlugInsNET.ActionsLib;
//using System;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using static GingerCoreNET.PlugInsLib.PluginsManager;
//using Amdocs.Ginger.CoreNET.Execution;
//using GingerCoreNET.CommandProcessorLib;
//using Amdocs.Ginger.CoreNET.GingerConsoleLib;
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
//{
//    public class NewAgent : RepositoryItem
//    {
//        public enum eStatus
//        {
//            [EnumValueDescription("Not Started")]
//            NotStarted,
//            Starting,
//            Ready,
//            Running,     
//            Completed
//        }

//        ~NewAgent()
//        {
//            // Cleanup and close driver process disconnect from Grid if not done nicely earlier
//            if (mGingerNodeProxy != null)
//            {
//                if (mGingerNodeProxy.IsConnected)
//                {
//                    mGingerNodeProxy.CloseDriver();
//                    mGingerNodeProxy = null;
//                }
//            }
//        }

//        bool mIsStarting = false;

//        public GingerGrid LocalGingerGrid { get; set; }
//        public PluginsManager PlugInsManager { get; set; }

//        [IsSerializedForLocalRepository]
//        public string PluginPackageName { get; set; }

//        [IsSerializedForLocalRepository]
//        public string PluginDriverName { get; set; }


//        [IsSerializedForLocalRepository]
//        public string GingerGridName { get; set; }    //TODO: add GUID


//        [IsSerializedForLocalRepository]
//        public Platform.ePlatformType Platform { get; set; }


//        bool IsLocalDriver = false;
//        // if we run the agent local then we keep the process
//        System.Diagnostics.Process LocalDriverProcess;

//        private string mName;
//        [IsSerializedForLocalRepository]
//        public string Name
//        {
//            get { return mName; }
//            set
//            {
//                if (mName != value)
//                {
//                    mName = value;
//                    OnPropertyChanged(nameof(Name));
//                }
//            }
//        }

//        private string mNodeID;
//        [IsSerializedForLocalRepository]
//        public string NodeID
//        {
//            get { return mNodeID; }
//            set
//            {
//                if (mNodeID != value)
//                {
//                    mNodeID = value;
//                    OnPropertyChanged(nameof(NodeID));
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();

//        public eStatus Status
//        {
//            get
//            {
//                //TODO: fix the correct status to be based on GNA

//                if (mIsStarting) return eStatus.Starting;
//                if (mGingerNodeProxy== null) return eStatus.NotStarted;
//                if (mGingerNodeProxy.IsConnected) return eStatus.Running;
                
//                return eStatus.NotStarted;
//            }
//        }        
        
//        [IsSerializedForLocalRepository]
//        public string Notes { get; set; }

//        [IsSerializedForLocalRepository]        
//        public ObservableList<DriverConfigParam> DriverConfiguration;
        
//        // with GingerCoreNET we use GNA
//        public GingerNodeProxy mGingerNodeProxy;
        
//        public void StartDriver()
//        {            
//            FindGingerNodeProxy();         
//            mGingerNodeProxy.StartDriver();
//        }

//        public void CloseDriver()
//        {
//            try
//            {
//                if (mGingerNodeProxy != null)
//                {
//                    mGingerNodeProxy.CloseDriver();

//                    if (IsLocalDriver)
//                    {
//                        //TODO: check if it is not closed after x second then kill
                        
//                        mGingerNodeProxy.Shutdown();
//                        Stopwatch st = Stopwatch.StartNew();
//                        while(!LocalDriverProcess.HasExited && st.ElapsedMilliseconds<20000)
//                        {
//                            Thread.Sleep(1000);
//                        }
//                        if (!LocalDriverProcess.HasExited)  
//                        {
//                            // so it didn't exist nicely close and assume dead - TODO: print message or...
//                            LocalDriverProcess.Close();                            
//                        }

//                        mGingerNodeProxy = null;
//                        LocalGingerGrid.NodeList.Remove(mGingerNodeInfo);
//                    }
//                }
//            }
//            finally
//            {
//                OnPropertyChanged(nameof(Status));
//            }
//        }

//        public string StartAgentScript()
//        {
//            DriverInfo DI = PlugInsManager.GetDriverInfo(PluginDriverName);
//            string PluginPackageFolder = DI.PluginPackageFolder;

//            //TODO: check for null PluginPackageFolder
//            string DriverName = PluginDriverName;

//            // We run it on another process using GingerConsole
//            string script = CommandProcessor.CreateLoadPluginScript(PluginPackageFolder);
//            script += CommandProcessor.CreateStartNodeScript(DriverName, mNodeID, SocketHelper.GetLocalHostIP() , LocalGingerGrid.Port) + Environment.NewLine;
//            return script;
//        }

//        public void RunAction(Act act)
//        {
//            if (mGingerNodeProxy == null)
//            {
//                throw new Exception("Unable to run action because GingerNodeAgent is not set");
//            }
//            mGingerNodeProxy.RunAction(act);           
//        }

//        public string Ping()
//        {
//            return mGingerNodeProxy.Ping();
//        }

//        public override string GetNameForFileName()
//        {
//            return Name;
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Name;
//            }
//            set
//            {
//                this.Name = value;
//            }
//        }

//        GingerNodeInfo mGingerNodeInfo = null;

//        private void FindGingerNodeProxy()
//        {
//            // TODO: write the algorithm to find the correct GNA based on agent config request: for example Byname, by type: Chrome, by OS, etc...

//            //TODO: seach in all ginger grids start in local !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

//            // if local grid
//            // find the GNI in local grid
//            mGingerNodeInfo = (from x in LocalGingerGrid.NodeList where x.Name == mNodeID select x).FirstOrDefault();  

//            if (mGingerNodeInfo == null)
//            {
//                // since it is local then start a new Ginger Node
//                mGingerNodeInfo = StartLocalDriver(LocalGingerGrid);
//            }

//            mGingerNodeProxy = new GingerNodeProxy(mGingerNodeInfo);
//            mGingerNodeProxy.GingerGrid = LocalGingerGrid;
//        }

//        private GingerNodeInfo StartLocalDriver(GingerGrid gingerGrid)
//        {            
//            IsLocalDriver = true;
//            mNodeID = this.Name;
//            string script = StartAgentScript();
//            LocalDriverProcess = GingerConsoleHelper.Execute(script);
//            LocalDriverProcess.Exited += Proc_Exited;
            
//            // Wait for the driver to connect
//            Stopwatch st = Stopwatch.StartNew();
//            GingerNodeInfo GNI = null;
//            while (GNI == null && !LocalDriverProcess.HasExited && st.ElapsedMilliseconds < 15000)
//            {
//                GNI = (from x in gingerGrid.NodeList where x.Name == mNodeID select x).FirstOrDefault();
//                Thread.Sleep(100);
//            }

//            if (GNI == null)
//            {
//                throw new Exception("Error: InitGingerNodeAgent - StartLocalDriver failed");
//            }

//            //TODO: handle null GNI - timeout

//            return GNI;
//        }

//        private void Proc_Exited(object sender, EventArgs e)
//        {
//            // TODO: notify closed - can happen if the user closed the window or agent crashed...
//        }

//        public void AttachDisplay()
//        {
//            mGingerNodeProxy.AttachDisplay();
//        }
//    }
//}
