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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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


        public PluginsManager(SolutionRepository solutionRepository)
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
            mSolutionRepository.AddRepositoryItem(pluginPackage);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string s = args.LoadedAssembly.FullName + " " + args.LoadedAssembly.Location;
            Console.WriteLine(s);
        }


        public string InstallPluginPackage(OnlinePluginPackage onlinePluginPackage, OnlinePluginPackageRelease release)
        {
            string folder = onlinePluginPackage.InstallPluginPackage(release);
            AddPluginPackage(folder);
            return folder;
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
            if (string.IsNullOrEmpty(pluginId))
            {
                throw new ArgumentNullException(nameof(pluginId));
            }
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();

            // TODO: only once !!!!!!!!!!!!!!!!!!!!!!!!! temp             
            pluginPackage.LoadServicesFromJSON();


            if (pluginPackage == null)
            {                
                throw new Exception("PluginPackage not found in solution PluginId=" + pluginId);
            }
            if (string.IsNullOrEmpty(pluginPackage.StartupDLL))
            {
                throw new Exception("StartupDLL is missing in the Ginger.PluginPackage.json for: " + pluginId);
            }

            string dll = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);

            string nodeFileName = CreateNodeConfigFile(pluginId, serviceID);  
            string cmd = "dotnet \"" + dll + "\" \"" + nodeFileName + "\"";
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);            
            procStartInfo.UseShellExecute = true;
            
            // TODO: Make it config not to show the console window
           // procStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;

            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            mProcesses.Add(new PluginProcessWrapper(pluginId, serviceID, proc));
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
            File.WriteAllText(fileName, txt);
            return fileName;
        }

        public void CloseAllRunningPluginProcesses()
        {
            foreach (PluginProcessWrapper process in mProcesses)
            {
                process.Close();                
            }
            mProcesses.Clear();
        }

        public List<ActionInputValueInfo> GetActionEditInfo(string pluginId, string serviceId, string actionId)
        {
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();
            PluginServiceInfo pluginServiceInfo = (from x in pluginPackage.Services where x.ServiceId == serviceId select x).SingleOrDefault();            
            PluginServiceActionInfo actionInfo = (from x in pluginServiceInfo.Actions where x.ActionId == actionId select x).SingleOrDefault();
            return actionInfo.InputValues;
        }

        public ObservableList<OnlinePluginPackage> GetOnlinePluginsIndex()
        {
            // edit at: "https://github.com/Ginger-Automation/Ginger-Plugins-Index/blob/master/PluginsList.json";

            // raw url to get the file content            
            string url = "https://raw.githubusercontent.com/Ginger-Automation/Ginger-Plugins-Index/master/PluginsList.json";
            ObservableList < OnlinePluginPackage > list = GitHTTPClient.GetJSON<ObservableList<OnlinePluginPackage>>(url);
            ObservableList<PluginPackage> installedPlugins = mSolutionRepository.GetAllRepositoryItems<PluginPackage>();
            foreach (OnlinePluginPackage onlinePluginPackage in list)
            {                
                PluginPackage pluginPackage = (from x in installedPlugins where x.PluginId == onlinePluginPackage.Id select x).SingleOrDefault();
                if (pluginPackage != null)
                {                
                    onlinePluginPackage.Status = "Installed - " + pluginPackage.PluginPackageVersion;
                }
            }
            return list;
        }

        

        public bool IsSessionService(string pluginId, string serviceId)
        {
            // TODO: Cache
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginId == pluginId select x).SingleOrDefault();
            pluginPackage.LoadServicesFromJSON();
            PluginServiceInfo pluginServiceInfo = (from x in pluginPackage.Services where x.ServiceId == serviceId select x).SingleOrDefault();
            return pluginServiceInfo.IsSession;
        }

        public void SolutionChanged(SolutionRepository solutionRepository)
        {        
            mSolutionRepository = solutionRepository;
            GetPackages();
         }
    }
}
