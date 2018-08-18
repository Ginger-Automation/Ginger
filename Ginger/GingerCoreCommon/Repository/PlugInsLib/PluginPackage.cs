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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
using GingerPlugInsNET.ActionsLib;
using GingerPlugInsNET.DriversLib;
using GingerPlugInsNET.PlugInsLib;
using GingerPlugInsNET.ServicesLib;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackage : RepositoryItemBase
    {
        // Not serialized loaded from Ginger.PluginPackage.json in the root folder
        PluginPackageInfo mPluginPackageInfo;

        PluginPackageInfo PluginPackageInfo
        {
            get
            {
                if (mPluginPackageInfo == null)
                {
                    mPluginPackageInfo = new PluginPackageInfo();
                }
                return mPluginPackageInfo; 
            }
        }

        public enum eType
        {
            LocalFolder,
            SystemFolder
        }

        [IsSerializedForLocalRepository]
        public string PluginID { get; set;  }

        [IsSerializedForLocalRepository]
        public string PluginPackageVersion { get; set; }

        [IsSerializedForLocalRepository]
        public eType Type { get; set; }

        public bool Isloaded = false;

        public PluginPackage()
        {
        }

        public PluginPackage(string folder)
        {            
            mFolder = folder;
            LoadInfo();
            PluginID = PluginPackageInfo.Id;
            PluginPackageVersion = PluginPackageInfo.Version;
        }

        private void LoadInfo()
        {
            //TODO: compare saved plugin id and version with the info file on folder

            string pluginInfoFile = Path.Combine(mFolder, PluginPackageInfo.cInfoFile);

            if (!System.IO.File.Exists(pluginInfoFile))
            {
                throw new Exception("Plugin info file not found: " + pluginInfoFile);
            }

            // load info
            string txt = System.IO.File.ReadAllText(pluginInfoFile);
            mPluginPackageInfo = (PluginPackageInfo)JsonConvert.DeserializeObject(txt, typeof(PluginPackageInfo));
        }

        public PluginDriverBase GetDriver(string driverName)
        {
            if (!Isloaded)
            {
                ScanPackage();
            }

            // TODO: fix make more efficent and load only what needed
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {
                var list = from type in PAI.Assembly.GetTypes()
                           where typeof(PluginDriverBase).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {
                    PluginDriverBase driver = (PluginDriverBase)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
                    if (driver.Name == driverName)
                    {
                        return driver;
                    }
                }
            }
            throw new Exception("Driver not found in Plugin Package - " + driverName);

        }

        string mFolder;
        [IsSerializedForLocalRepository]        
        public string Folder { get { return mFolder; }
            set
            {
                if (mFolder != value)
                {
                    mFolder = value;
                    LoadInfo();
                    OnPropertyChanged(nameof(Folder));
                }
            }
        }

        List<PluginAssemblyInfo> mAssembliesInfo = new List<PluginAssemblyInfo>();

        public override string ItemName { get { return PluginID; } set {  } }

        

        public override string GetNameForFileName()
        {
            return PluginID;
        }

        public void ScanPackage()
        {
            LoadGingerPluginsDLL();

            //TODO: scan all DLLs in the folder, only ones which ends with *GingerPlugin.dll - so we don't load or scan unneeded DLLs 0- faster
            // add .
            string[] files = Directory.GetFiles(Folder, "*.GingerPlugin.dll", SearchOption.AllDirectories);
            if (files.Length > 0)
            {
                foreach (string filename in files)
                {
                    // Just get assmebly info - not loading it!
                    AssemblyName ans = AssemblyName.GetAssemblyName(filename);
                    PluginAssemblyInfo PAI = new PluginAssemblyInfo();
                    PAI.Name = ans.Name;
                    PAI.FilePath = filename;

                    mAssembliesInfo.Add(PAI);
                }
                Isloaded = true;
            }
            else
            {
                throw new Exception("Plugin folder doesn't contain any *.GingerPlugin.dll - Folder" + Folder);
            }
        }

        List<MethodInfo> mStandAloneMethods = null;
        List<MethodInfo> GetStandAloneMethods()
        {
            ScanPackage();  // do once !!!!!!!!!!!!!!!!!!!!!!
            if (mStandAloneMethods == null)
            {
                mStandAloneMethods = new List<MethodInfo>();
                foreach (PluginAssemblyInfo asssembly in mAssembliesInfo)
                {
                    IEnumerable<Type> types = from x in asssembly.Assembly.GetTypes() where typeof(IStandAloneAction).IsAssignableFrom(x) select x;
                    foreach (Type t in types)
                    {
                        if (t == typeof(IStandAloneAction)) continue;  // we ignore the interface itself

                        // expecting to get ExcelAction, FileAction, DatabaseAction...
                        MethodInfo[] methods = t.GetMethods();
                        foreach (MethodInfo MI in methods)
                        {
                            //Check if method have token [GingerAction]  else we ignore
                            GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);

                            if (token == null) continue;

                            mStandAloneMethods.Add(MI);
                        }
                    }
                }
            }
            return mStandAloneMethods;
        }

        public ObservableList<StandAloneAction> GetStandAloneActions()
        {
            ObservableList<StandAloneAction> list = new ObservableList<StandAloneAction>();

            foreach (MethodInfo MI in GetStandAloneMethods())
            {
                GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);                
                StandAloneAction DA = new StandAloneAction();
                DA.ID = token.ID;
                AssemblyName AN = MI.DeclaringType.Assembly.GetName();
                DA.PluginID = AN.Name;
                DA.Description = token.Description;
                foreach (ParameterInfo PI in MI.GetParameters())
                {
                    if (PI.ParameterType.Name != nameof(GingerAction))
                    {
                        DA.InputValues.Add(new ActInputValue() { Param = PI.Name, ParamType = PI.ParameterType });
                    }
                }
                list.Add(DA);
            }
            return list;
        }

        public ActionHandler GetStandAloneActionHandler(string id)
        {
            if (!Isloaded)
            {
                ScanPackage();
            }
            foreach (MethodInfo MI in GetStandAloneMethods())
            {
                GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);
                if (token.ID == id)
                {
                    ActionHandler AH = new ActionHandler();
                    AH.ID = id;
                    AH.MethodInfo = MI;

                    //TODO: create on demand nad cache to 1
                    AH.Instance = MI.DeclaringType.Assembly.CreateInstance(MI.DeclaringType.FullName); // !!!!!!!!!!!!
                    AH.GingerAction = new GingerAction(id);
                    // do we need the params?
                    return AH;
                }
            }
            return null;
        }

        private void LoadGingerPluginsDLL()
        {
            // We make sure our latest in Ginger is the one to load GingerPlugin DLL so it will not be loaded from the folder of the plugin in case some one copied it too, or we point to debug folder

            // Just dummy does nothing except making sure we load the DLL first from inside Ginger
            GingerActionAttribute PIC = new GingerActionAttribute("ID DUMMY", "Desc DUMMY");
        }

        //TODO: return DriverInfo

        List<string> drivers = null;
        public List<string> GetDrivers()
        {
            if (!Isloaded)
            {
                ScanPackage();
            }
            if (drivers == null)
            {
                drivers = new List<string>();
                foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
                {
                    var list = from type in PAI.Assembly.GetTypes()
                               where typeof(PluginDriverBase).IsAssignableFrom(type) && type.IsAbstract == false
                               select type;

                    foreach (Type t in list)
                    {
                        PluginDriverBase driver = (PluginDriverBase)PAI.Assembly.CreateInstance(t.FullName);

                        //TODO: get driver info from annotation on the class

                        drivers.Add(driver.Name);
                    }

                }
            }
            return drivers;
        }

        // Services are used for standalone action/ ActWithoutDriver
        public PluginServiceBase GetService(string serviceName)
        {
            if (!Isloaded)
            {
                ScanPackage();
            }
            // TODO: fix make more efficent and load only what needed
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {
                var list = from type in PAI.Assembly.GetTypes()
                           where typeof(PluginServiceBase).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {
                    PluginServiceBase service = (PluginServiceBase)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
                    if (service.Name == serviceName)
                    {
                        return service;
                    }
                }

            }
            throw new Exception("Service not found in Plugin Package - " + serviceName);
        }

        public List<PluginServiceBase> GetServices()
        {
            if (!Isloaded)
            {
                ScanPackage();
            }

            List<PluginServiceBase> services = new List<PluginServiceBase>();
            // TODO: fix make more efficent and load only what needed
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {
                var list = from type in PAI.Assembly.GetTypes()
                           where typeof(PluginServiceBase).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {
                    PluginServiceBase service = (PluginServiceBase)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
                    services.Add(service);
                }

            }
            return services;
        }
    }
}
