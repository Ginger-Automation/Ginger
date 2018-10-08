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
using System.Linq;
using System.Reflection;

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

            PluginPackage p = new PluginPackage(folder);            
            mPluginPackages.Add(p);
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

        




        //public void Execute(string PluginId, string ServiceId, NewPayLoad payLoad)
        //{            
        //    GingerGrid gingerGrid = WorkSpace.Instance.LocalGingerGrid;

        //    // string PID = GA.InputParams["PluginID"].GetValueAsString();
        //    PluginPackage p = (from x in mPluginPackages where x.PluginID == PluginId select x).SingleOrDefault();
        //    if (p == null)
        //    {
        //        throw new Exception("Plugin id not found: " + PluginId);
        //        // GA.AddError("Execute", "Plugin id not found: " + PID);
        //        // return;
        //    }

        //    //TODO: use nameof after ActPlugin move to common
        //    // string serviceID = GA.InputParams["PluginActionID"].GetValueAsString();


        //    GingerNodeInfo GNI = (from x in gingerGrid.NodeList where x.Name == p.PluginID select x).FirstOrDefault();
        //    //run script only if service is not up            
        //    if (GNI == null)
        //    {
        //        string script = CommandProcessor.CreateLoadPluginScript(p.Folder);

        //        // hard coded!!!!!!!!!!  - use ServiceId
        //        script += CommandProcessor.CreateStartServiceScript("PACTService", p.PluginID, SocketHelper.GetLocalHostIP(), gingerGrid.Port);
        //        // script += CommandProcessor.CreateStartServiceScript("ExcelService", p.PluginID, SocketHelper.GetLocalHostIP(), gingerGrid.Port);


        //        Task t = new Task(() =>
        //        {
        //            // GingerConsoleHelper.Execute(script);  // keep it for regular service dll load
        //            string StarterDLL = Path.Combine(p.Folder, "GingerPACTPluginConsole.dll");  //??
        //            StartService(StarterDLL);
        //        });
        //        t.Start();
        //    }                

        //    int counter = 0;
        //    while (GNI == null && counter < 30)
        //    {
        //        Thread.Sleep(1000);
        //        GNI = (from x in gingerGrid.NodeList where x.Name == "PACT" select x).FirstOrDefault();                
        //        counter++;
        //    }
        //    if (GNI == null)
        //    {
        //       // GA.AddError("Execute", "Cannot execute action beacuse Service was not found or was not abale to start: " + p.PluginID);
        //    }

        //    GingerNodeProxy GNA = new GingerNodeProxy(GNI);
        //    GNA.Reserve();
        //    GNA.GingerGrid = gingerGrid;

        //    //GNA.RunAction(GA);
        //}


        //public void StartService(string DLLFile)
        //{            
        //    string cmd = "dotnet " + DLLFile ;            
        //    System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);

        //    // The following commands are needed to redirect the standard output.
        //    // This means that it will be redirected to the Process.StandardOutput StreamReader.
        //    procStartInfo.UseShellExecute = true; // false
        //    // Do not create the black window.
        //    // Now we create a process, assign its ProcessStartInfo and start it
        //    System.Diagnostics.Process proc = new System.Diagnostics.Process();
        //    proc.StartInfo = procStartInfo;
        //    proc.Start();            
        //}

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
            string cmd = "dotnet " + dll + " " + nodeFileName;
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
            StandAloneAction standAloneAction = (from x in pluginPackage.LoadServicesInfoFromFile() where x.ServiceID == serviceId && x.ID == actionId select x).SingleOrDefault();
            return standAloneAction.InputValues;
        }

    }
}
