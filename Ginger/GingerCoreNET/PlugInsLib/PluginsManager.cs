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
using Amdocs.Ginger.CoreNET.RunLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    public class PluginsManager
    {
        private ObservableList<PluginPackage> mPluginPackages;

        public PluginsManager()
        {
            mPluginPackages = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
        }

        public class DriverInfo
        {
            public string Name { get; set; }
            public string PluginPackageFolder { get; set; }
        }

        public List<DriverInfo> GetAllDrivers()
        {
            //TODO: cache !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            List<DriverInfo> drivers = new List<DriverInfo>();

            foreach (PluginPackage p in mPluginPackages)
            {
                //foreach (string s in p.GetDrivers())
                //{
                //    DriverInfo di = new DriverInfo();
                //    di.Name = s;
                //    di.PluginPackageFolder = p.Folder;
                //    drivers.Add(di);
                //}
            }
            return drivers;
        }

        //public List<GingerAction> GetDriverActions(DriverInfo DI)
        //{
        //    List<GingerAction> actions = new List<GingerAction>();
        //    PluginPackage p = (from x in mPluginPackages where x.Folder == DI.PluginPackageFolder select x).FirstOrDefault();
        //    PluginDriverBase driver = p.GetDriver(DI.Name);
        //    foreach (ActionHandler AH in driver.ActionHandlers)
        //    {
        //        GingerAction GA = new GingerAction(AH.ID);
        //        UpdateActionParamTypes(GA, AH.MethodInfo);
        //        actions.Add(GA);
        //    }
        //    return actions;
        //}

        ObservableList<StandAloneAction> list;
        public ObservableList<StandAloneAction> GetStandAloneActions()
        {
            if (list == null)
            {
                list = new ObservableList<StandAloneAction>();
                foreach (PluginPackage p in mPluginPackages)
                {
                    foreach (StandAloneAction SAA in p.GetStandAloneActions())
                    {
                        list.Add(SAA);
                    }
                }
            }
            return list;
        }

        //private void UpdateActionParamTypes(GingerAction gA, MethodInfo methodInfo)
        //{
        //    foreach (ParameterInfo PI in methodInfo.GetParameters())
        //    {
        //        if (PI.ParameterType != typeof(GingerAction))
        //        {
        //            ActionParam AP = gA.InputParams.GetOrCreateParam(PI.Name);
        //            AP.ParamType = PI.ParameterType;
        //        }
        //    }
        //}

        public void AddPluginPackage(string folder)
        {
            // Verify folder exist
            if (!System.IO.Directory.Exists(folder))
            {
                throw new Exception("Plugin folder not found: " + folder);
            }            

            PluginPackage pluginPackage = new PluginPackage(folder);                                 
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(pluginPackage);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string s = args.LoadedAssembly.FullName + " " + args.LoadedAssembly.Location;
            Console.WriteLine(s);
        }

        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);


                return assembly;
            }
            else
            {
                return null;
            }
        }

        internal DriverInfo GetDriverInfo(string PluginDriverName)
        {
            foreach (DriverInfo di in GetAllDrivers())
            {
                if (di.Name == PluginDriverName)
                {
                    return di;
                }
            }
            return null;
        }

        //internal ActionHandler GetStandAloneActionHandler(string pluginID, string ID)
        //{
        //    foreach (PluginPackage PP in mPluginPackages)
        //    {
        //        PP.ScanPackage();
        //        ActionHandler AH = PP.GetStandAloneActionHandler(ID);
        //        if (AH != null)
        //        {
        //            return AH;
        //        }
        //    }
        //    throw new Exception("Action handler not found for Action ID: " + ID);
        //}

        static List<PluginPackage> mInstalledPluginPackages = null;

        // Get list of installed plugins in Ginger folder 'PluginPackages'
        public List<PluginPackage> GetInstalledPluginPackages()
        {
            if (mInstalledPluginPackages != null)
            {
                //TODO: check for new added plugins

                return mInstalledPluginPackages;
            }

            mInstalledPluginPackages = new List<PluginPackage>();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (path.Contains("GingerWPF"))   // We are running from GingerWPF in debug mode
            {
                path = path.Replace(@"GingerWPF\bin\Debug", "");   // temp need to be Ginger installation folder !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }

            if (path.Contains("GingerCoreNETUnitTest"))   // We are running from GingerWPF in debug mode
            {
                path = path.Replace(@"GingerCoreNETUnitTest\bin\Debug\netcoreapp2.0", "");
            }

            string pluginPackagesPath = Path.Combine(path, "PluginPackages");

            // Each directory is a plugin package

            foreach (string d in Directory.GetDirectories(pluginPackagesPath))
            {
                PluginPackage p = new PluginPackage(d);
                mInstalledPluginPackages.Add(p);
            }
            return mInstalledPluginPackages;
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


        public void StartService(string PluginId)
        {
            if (string.IsNullOrEmpty(PluginId))
            {
                throw new Exception("Plugin action missing PluginId");
            }
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginID == PluginId select x).SingleOrDefault();

            if (pluginPackage == null)
            {
                throw new Exception("PluginPackage not found in solution PluginId=" + PluginId);
            }
            if (string.IsNullOrEmpty(pluginPackage.StartupDLL))
            {
                throw new Exception("PluginPackage StartupDLL is missing in the Ginger.PluginPackage.json" + PluginId);
            }

            string dll = Path.Combine(pluginPackage.Folder, pluginPackage.StartupDLL);

            string nodeFileName = NodeConfigFile.CreateNodeConfigFile(PluginId + "1");  // TODO: check if 1 exist then try 2,3 in case more than one same id service start
            string cmd = "dotnet \"" + dll + "\" \"" + nodeFileName + "\"";
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);            
            procStartInfo.UseShellExecute = true;             
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();

            //TODO: delete the temp file - or create temp files tracker with auto delete 
        }


        public List<ActionInputValueInfo> GetActionEditInfo(string pluginId, string serviceId, string actionId)
        {
            PluginPackage pluginPackage = (from x in mPluginPackages where x.PluginID == pluginId select x).SingleOrDefault();
            StandAloneAction standAloneAction = (from x in pluginPackage.LoadServicesInfoFromFile() where x.ServiceId == serviceId && x.ActionId == actionId select x).SingleOrDefault();
            return standAloneAction.InputValues;
        }

        public ObservableList<OnlinePluginPackage> GetPluginsIndex()
        {            
            // edit at: "https://github.com/Ginger-Automation/Ginger-Plugins-Index/blob/master/PluginsList.json";

            // raw url to get the file content
            string url = "https://raw.githubusercontent.com/Ginger-Automation/Ginger-Plugins-Index/master/PluginsList.json";
            string packagesjson = GetResponseString(url).Result;

            ObservableList<OnlinePluginPackage> list = JsonConvert.DeserializeObject<ObservableList<OnlinePluginPackage>>(packagesjson);
            return list;
        }


        /// <summary>
        /// Get list of releases for plguin from Git        
        /// </summary>
        /// <param name="pluginRepositoryName"></param>
        /// Repositry name in Fit for example: Ginger-PACT-Plugin
        /// <returns></returns>
        public List<dynamic> GetPluginReleases(string URL)
        {
            if (string.IsNullOrEmpty(URL))
            {
                return null;
            }
            // string url = "https://api.github.com/repos/Ginger-Automation" + pluginRepositoryName +  "/releases";
                            
            string url = URL.Replace("https://github.com/Ginger-Automation", "https://api.github.com/repos/Ginger-Automation");
            url += "/releases";            
            string releases = GetResponseString(url).Result;

            List<dynamic> lst = JsonConvert.DeserializeObject<List<dynamic>>(releases);
            return lst;
        }

        async Task<string> GetResponseString(string url)
        {
            using (var client = new HttpClient())
            {
                // Simulate a browser
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");

                var result = client.GetAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();                    
                    return json;
                }
                else
                {
                    return "Error: " + result.ReasonPhrase;
                }
            }
        }

        public void InstallPluginPackage(OnlinePluginPackage onlinePluginPackage, string version, string zipFileURL)
        {                 
                string f = Path.Combine(onlinePluginPackage.Name, version);
                string folder = DownloadPackage(zipFileURL, f).Result;
                AddPluginPackage(folder);
                // PluginPackage p = new PluginPackage(folder);
         
        }

        async Task<string> DownloadPackage(string url, string subfolder)
        {
            //TODO: show user some progress... update a shared string status
            using (var client = new HttpClient())
            {
                var result = client.GetAsync(url).Result;

                if (result.IsSuccessStatusCode)
                {
                    byte[] zipContent = await result.Content.ReadAsByteArrayAsync();  // Get the Plugin package zip content
                    string fileName = Path.GetTempFileName();  // temp file for the zip
                    File.WriteAllBytes(fileName, zipContent);  // save content to file                    
                    string localPluginPackageFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); // target folder to extract the plugin to on user folder
                    localPluginPackageFolder = Path.Combine(localPluginPackageFolder, "Ginger", "PluginPackages", subfolder); // Extract it to: \users\[user]\Ginger\PluginPackages/[PluginFolder]
                    ZipFile.ExtractToDirectory(fileName, localPluginPackageFolder); // Extract 
                    return localPluginPackageFolder;
                }
                else
                {
                    throw new Exception("Error downloading Plugin Package: " + result.ReasonPhrase + Environment.NewLine + url);
                }
            }
        }

    }
}
