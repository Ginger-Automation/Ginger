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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using GingerUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    public class PluginsManager
    {
        private ObservableList<PluginPackage> mPluginPackages;
        SolutionRepository mSolutionRepository;

        ObservableList<PluginProcessWrapper> mProcesses = new ObservableList<PluginProcessWrapper>();

        public ObservableList<PluginProcessWrapper> PluginProcesses
        {
            get
            {
                return mProcesses;
            }
        }

        public bool BackgroudDownloadInprogress { get;  set; }
        

        public void Init(SolutionRepository solutionRepository)
        {
            mSolutionRepository = solutionRepository;
            GetPackages();
        }

        private void GetPackages()
        {
            mPluginPackages = mSolutionRepository.GetAllRepositoryItems<PluginPackage>();            
        }

        public class DriverInfo
        {
            public string Name { get; set; }
            public string PluginPackageFolder { get; set; }
        }

      
        public void AddPluginPackage(string folder)
        {
            // Verify folder exist
            if (!System.IO.Directory.Exists(folder))
            {
                throw new Exception("Plugin folder not found: " + folder);
            }            

            PluginPackage pluginPackage = new PluginPackage(folder);
            pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);
            pluginPackage.PluginPackageOperations.LoadPluginPackage(folder);
            mSolutionRepository.AddRepositoryItem(pluginPackage);            
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string s = args.LoadedAssembly.FullName + " " + args.LoadedAssembly.Location;
            Console.WriteLine(s);
        }

        public void AddIsSession(string pluginId, string serviceId, bool isSession)
        {
            PluginServiceIsSeesionDictionary.Add(pluginId + "." + serviceId, isSession);
        }

        public string InstallPluginPackage(OnlinePluginPackage onlinePluginPackage, OnlinePluginPackageRelease release)
        {
            string folder = onlinePluginPackage.InstallPluginPackage(release);
            AddPluginPackage(folder);
            return folder;
        }

        public void UninstallPluginPackage(OnlinePluginPackage pluginPackageInfo)
        {
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginPackageInfo.Id select x).FirstOrDefault();
            WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(pluginPackage);
        }

        public string CreatePluginPackageInfo(string id, string version)
        {
            PluginPackageInfo pluginPackageInfo = new PluginPackageInfo() { Id = id, Version = version };
            string txt = JsonConvert.SerializeObject(pluginPackageInfo);
            return txt;
        }



        // We search the plug in for all files it can edit if this plugin implemented capability of text editor
        //        public List<PlugInTextFileEditorBase> TextEditors()
        //        {
        //            List<PlugInTextFileEditorBase> list = new List<PlugInTextFileEditorBase>();
        //            //TODO:: Need to be handled once PACT implementation is done
        //            //foreach (PlugInCapability c in PlugIn.Capabilities())
        //            //{
        //            //    if (c is PlugInTextFileEditorBase)
        //            //    {
        //            //        list.Add((PlugInTextFileEditorBase)c);
        //            //    }
        //            //}
        //            return list;
        //        }

        //        public List<string> GetExtentionsByEditorID(string plugInEditorID)
        //        {
        //            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
        //            {
        //                if (PITFEB.EditorID == plugInEditorID)
        //                {
        //                    return PITFEB.Extensions;
        //                }
        //            }
        //            return null;
        //        }

        //        public PlugInTextFileEditorBase GetPlugInTextFileEditorBaseByEditorID(string plugInEditorID)
        //        {
        //            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
        //            {
        //                if (PITFEB.EditorID == plugInEditorID)
        //                {
        //                    return PITFEB;
        //                }
        //            }
        //            return null;
        //        }

        //        public string GetTemplateContentByEditorID(string plugInEditorID, string plugInExtension)
        //        {
        //            foreach (PlugInTextFileEditorBase PITFEB in TextEditors())
        //            {
        //                if (PITFEB.EditorID == plugInEditorID)
        //                {
        //                    return PITFEB.GetTemplatesByExtensions("." + plugInExtension);
        //                }
        //            }
        //            return null;
        //        }

       
        public System.Diagnostics.Process StartService(string pluginId, string serviceID)
        {            
            Console.WriteLine("Starting Service: " + serviceID + " from Plugin: " + pluginId);
            if (string.IsNullOrEmpty(pluginId))
            {
                throw new ArgumentNullException(nameof(pluginId));
            }
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();
            pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);
            Console.WriteLine("Loading Plugin Services from JSON...");

            // TODO: only once !!!!!!!!!!!!!!!!!!!!!!!!! temp             
            pluginPackage.PluginPackageOperations.LoadServicesFromJSON();

            if (pluginPackage == null)
            {                
                throw new Exception("PluginPackage not found in solution PluginId=" + pluginId);
            }
            if (string.IsNullOrEmpty(pluginPackage.PluginPackageOperations.StartupDLL))
            {
                throw new Exception("StartupDLL is missing in the Ginger.PluginPackage.json for: " + pluginId);
            }

            string dll = Path.Combine(pluginPackage.Folder, pluginPackage.PluginPackageOperations.StartupDLL);
            Console.WriteLine("Plug-in dll path: " + dll);
            string nodeFileName = CreateNodeConfigFile(pluginId, serviceID);
            Console.WriteLine("nodeFileName: " + nodeFileName);

            string cmd = "\"" + dll + "\" \"" + nodeFileName + "\"";

            Console.WriteLine("Creating Process..");

            System.Diagnostics.Process proc = WorkSpace.Instance.OSHelper.Dotnet(cmd);

            mProcesses.Add(new PluginProcessWrapper(pluginId, serviceID, proc));
            Console.WriteLine("Plug-in Running on the Process ID: " + proc.Id);
            return proc;
            //TODO: delete the temp file - or create temp files tracker with auto delete 
        }

        int ServiceCounter = 0;
        private string CreateNodeConfigFile(string name, string serviceId)
        {
            ServiceCounter++;
            string NewName = name + " " + ServiceCounter;  // We add counter since this is auto start service and many can start so to identify
            string txt = NewName + " | " + serviceId + " | " + SocketHelper.GetLocalHostIP() + " | " + WorkSpace.Instance.LocalGingerGrid.Port + Environment.NewLine;
            string fileName = Path.GetTempFileName();
            Console.WriteLine("CreateNodeConfigFile content= " + txt);
            File.WriteAllText(fileName, txt);
            return fileName;
        }

        public void CloseAllRunningPluginProcesses()
        {            
            Reporter.ToLog(eLogLevel.INFO, "Closing all Running Plugins Processes");
            if (WorkSpace.Instance.LocalGingerGrid != null)
            {
                WorkSpace.Instance.LocalGingerGrid.NodeList.Clear();//??? do proper Reset()
            }
            foreach (PluginProcessWrapper process in mProcesses)
            {
                process.Close();
            }
            mProcesses.Clear();
        }

        public List<ActionInputValueInfo> GetActionEditInfo(string pluginId, string serviceId, string actionId)
        {
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();
            pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);
            PluginServiceInfo pluginServiceInfo = (from x in ((PluginPackageOperations)pluginPackage.PluginPackageOperations).Services where x.ServiceId == serviceId select x).SingleOrDefault();            
            PluginServiceActionInfo actionInfo = (from x in pluginServiceInfo.Actions where x.ActionId == actionId select x).SingleOrDefault();
            return actionInfo.InputValues;
        }

        public ObservableList<OnlinePluginPackage> GetOnlinePluginsIndex()
        {            
            // edit at: "https://github.com/Ginger-Automation/Ginger-Plugins-Index/blob/master/PluginsList.json";

            // raw url to get the file content            
            string url = "https://raw.githubusercontent.com/Ginger-Automation/Ginger-Plugins-Index/master/PluginsList.json";
            Reporter.ToLog(eLogLevel.DEBUG, "Getting Plugins list from " + url);

            ObservableList < OnlinePluginPackage > list = GitHTTPClient.GetJSON<ObservableList<OnlinePluginPackage>>(url);
            Reporter.ToLog(eLogLevel.DEBUG, "Online Plugins count=" + list.Count);
            ObservableList<PluginPackage> installedPlugins = mSolutionRepository.GetAllRepositoryItems<PluginPackage>();
            foreach (OnlinePluginPackage onlinePluginPackage in list)
            {                
                PluginPackage pluginPackage = (from x in installedPlugins where x.PluginId == onlinePluginPackage.Id select x).FirstOrDefault();
                if (pluginPackage != null)
                {                    
                    onlinePluginPackage.CurrentPackage = pluginPackage.PluginPackageVersion;
                    onlinePluginPackage.Status = "Installed - " + pluginPackage.PluginPackageVersion;
                }
            }
            return list;
        }

        //Cache is session info + enable to add isSession info directly
        public Dictionary<string, bool> PluginServiceIsSeesionDictionary = new Dictionary<string, bool>();

        public bool IsSessionService(string pluginId, string serviceId)
        {
            string key = pluginId + "." + serviceId;
            bool isSession;
            bool bFound = PluginServiceIsSeesionDictionary.TryGetValue(key, out isSession);
            if (bFound)
            {
                return isSession;
            }
            
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();
            pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);

            PluginServiceInfo pluginServiceInfo = ((PluginPackageOperations)pluginPackage.PluginPackageOperations).GetService(serviceId);
            if (pluginServiceInfo != null)
            {
                PluginServiceIsSeesionDictionary.Add(key, pluginServiceInfo.IsSession);
                return pluginServiceInfo.IsSession;
            }
            else
            {
                throw new Exception("IsSessionService Error: pluginServiceInfo not found for: " + pluginId + "." + serviceId);
            }
        }

        public void SolutionChanged(SolutionRepository solutionRepository)
        {
            mSolutionRepository = solutionRepository;

            //download missing plugins
            GetPackages();
            if (mPluginPackages != null && mPluginPackages.Count > 0)
            {                
                if ( WorkSpace.Instance.RunningInExecutionMode)
                {                    
                    DownloadMissingPlugins(); //need to download it before execution starts
                }
                else
                {
                    Task.Run(() =>
                    {
                        DownloadMissingPlugins(); //downloading the plugins async
                    });
                }
            }
        }

        private void DownloadMissingPlugins()
        {
            Reporter.ToStatus(eStatusMsgKey.DownloadingMissingPluginPackages);
            WorkSpace.Instance.PlugInsManager.BackgroudDownloadInprogress = true;
            try
            {
                ObservableList<OnlinePluginPackage> OnlinePlugins = null;
                foreach (PluginPackage SolutionPlugin in mPluginPackages)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Check PluginId: " + SolutionPlugin.PluginId);
                    Reporter.ToLog(eLogLevel.DEBUG, "Check Plugin folder: " + SolutionPlugin.Folder);
                    if (Directory.Exists(SolutionPlugin.Folder))
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Plugin folder exist so no need to download");
                        continue;   // Plugin folder exist so no need to download
                    }

                    if (isPrivatePlugin(SolutionPlugin))
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Private plugin folder no need to download");
                        continue;   // this is private plugin located on the developer machine will not be able to download from online
                    }

                    if (OnlinePlugins == null)
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Getting online plugins index");
                        OnlinePlugins = WorkSpace.Instance.PlugInsManager.GetOnlinePluginsIndex();
                    }


                    OnlinePluginPackage OnlinePlugin = OnlinePlugins.Where(x => x.Id == SolutionPlugin.PluginId).FirstOrDefault();
                    if (OnlinePlugin == null)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Plugin not found in online!");
                        continue;
                    }

                    Reporter.ToLog(eLogLevel.INFO, "Checking plugin release: version=" + SolutionPlugin.PluginPackageVersion);
                    OnlinePluginPackageRelease OPR = OnlinePlugin.Releases.Where(x => x.Version == SolutionPlugin.PluginPackageVersion).FirstOrDefault();

                    if (OPR != null)
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Plugin version found starting install");
                        OnlinePlugin.InstallPluginPackage(OPR);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Plugin version not found cannot install!");
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while downloading/updating the Ginger Plugins packages", ex);
            }
            finally
            {
                WorkSpace.Instance.PlugInsManager.BackgroudDownloadInprogress = false;
                Reporter.HideStatusMessage();
            }
        }

        private bool isPrivatePlugin(PluginPackage solutionPlugin)
        {
            // Plugin is considered private when the folder is not in LocalUserApplicationDataFolderPath
            // It means developer added the plugin using folder and not install from online
            if (solutionPlugin.Folder.StartsWith(Common.GeneralLib.General.LocalUserApplicationDataFolderPath))
            {
                return false;
            }
            else
            {
                return true;
            }
            
        }
    }
}
