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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Plugin.Core;
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

        //public PluginDriverBase GetDriver(string driverName)
        //{
        //    if (!Isloaded)
        //    {
        //        ScanPackage();
        //    }

        //    // TODO: fix make more efficent and load only what needed
        //    foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
        //    {
        //        var list = from type in PAI.Assembly.GetTypes()
        //                   where typeof(PluginDriverBase).IsAssignableFrom(type) && type.IsAbstract == false
        //                   select type;

        //        foreach (Type t in list)
        //        {
        //            PluginDriverBase driver = (PluginDriverBase)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
        //            if (driver.Name == driverName)
        //            {
        //                return driver;
        //            }
        //        }
        //    }
        //    throw new Exception("Driver not found in Plugin Package - " + driverName);

        //}

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

            string startupDLLFileName = Path.Combine(mFolder, mPluginPackageInfo.StartupDLL);
            AssemblyName assmenblyName = AssemblyName.GetAssemblyName(startupDLLFileName);
            PluginAssemblyInfo PAI = new PluginAssemblyInfo();
            PAI.Name = startupDLLFileName;
            PAI.FilePath = startupDLLFileName;

            mAssembliesInfo.Add(PAI);


            // Scan all DLLs in the folder, only ones which ends with *.GingerPlugin.dll - so we don't load or scan unneeded DLLs - faster

            //string[] files = Directory.GetFiles(Folder, "*.GingerPlugin.dll", SearchOption.AllDirectories);
            //if (files.Length > 0)
            //{
            //    foreach (string fileName in files)
            //    {
            //        // Just get assmebly info - not loading it!
            //        AssemblyName assmenblyName = AssemblyName.GetAssemblyName(fileName);
            //        PluginAssemblyInfo PAI = new PluginAssemblyInfo();
            //        PAI.Name = assmenblyName.Name;
            //        PAI.FilePath = fileName;

            //        mAssembliesInfo.Add(PAI);
            //    }
            //    Isloaded = true;
            //}
            //else
            //{
            //    throw new Exception("Plugin folder doesn't contain any *.GingerPlugin.dll - Folder" + Folder);
            //}
        }


        List<PluginService> mPluginServices = null;
        List<PluginService> GetPluginServices()
        {
            ScanPackage();  // do once !!!!!!!!!!!!!!!!!!!!!!
            if (mPluginServices == null)
            {
                mPluginServices = new List<PluginService>();
                foreach (PluginAssemblyInfo asssembly in mAssembliesInfo)
                {
                    IEnumerable<Type> types = from x in asssembly.Assembly.GetTypes() where typeof(IGingerService).IsAssignableFrom(x) select x;
                    foreach (Type t in types)
                    {
                        GingerServiceAttribute gingerServiceAttribute = (GingerServiceAttribute)Attribute.GetCustomAttribute(t, typeof(GingerServiceAttribute), false);
                        PluginService pluginService = new PluginService() { ServiceId = gingerServiceAttribute.Id };
                        // expecting to get ExcelAction, FileAction, DatabaseAction...
                        MethodInfo[] methods = t.GetMethods();
                        foreach (MethodInfo MI in methods)
                        {
                            //Check if method have token [GingerAction]  else we ignore
                            GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);

                            if (token == null) continue;

                            pluginService.mStandAloneMethods.Add(MI);
                        }
                        mPluginServices.Add(pluginService);
                    }
                    
                }
            }
            return mPluginServices;
        }

        public ObservableList<StandAloneAction> GetStandAloneActions()
        {
            ObservableList<StandAloneAction> list = new ObservableList<StandAloneAction>();

            foreach (PluginService pluginService in GetPluginServices())
            {
                foreach (MethodInfo MI in pluginService.mStandAloneMethods)
                {
                    GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);
                    StandAloneAction DA = new StandAloneAction();
                    DA.ID = token.Id;
                    // AssemblyName AN = MI.DeclaringType.Assembly.GetName();
                    DA.PluginID = PluginID;  //AN.Name;
                    DA.ServiceID = pluginService.ServiceId;
                    DA.Description = token.Description;
                    foreach (ParameterInfo PI in MI.GetParameters())
                    {
                        if (PI.ParameterType.Name != nameof(GingerAction))
                        {
                            DA.InputValues.Add(new ActionInputValueInfo() { Param = PI.Name, ParamType = PI.ParameterType });
                        }
                    }
                    list.Add(DA);
                }
            }
            return list;
        }

        //public ActionHandler GetStandAloneActionHandler(string id)
        //{
        //    if (!Isloaded)
        //    {
        //        ScanPackage();
        //    }
        //    foreach (MethodInfo MI in GetStandAloneMethods())
        //    {
        //        GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);
        //        if (token.ID == id)
        //        {
        //            ActionHandler AH = new ActionHandler();
        //            AH.ID = id;
        //            AH.MethodInfo = MI;

        //            //TODO: create on demand nad cache to 1
        //            AH.Instance = MI.DeclaringType.Assembly.CreateInstance(MI.DeclaringType.FullName); // !!!!!!!!!!!!
        //            AH.GingerAction = new GingerAction(id);
        //            // do we need the params?
        //            return AH;
        //        }
        //    }
        //    return null;
        //}

        private void LoadGingerPluginsDLL()
        {

            //??

            // We make sure our latest in Ginger is the one to load GingerPlugin DLL so it will not be loaded from the folder of the plugin in case some one copied it too, or we point to debug folder

            // Just dummy does nothing except making sure we load the DLL first from inside Ginger
            GingerActionAttribute PIC = new GingerActionAttribute("ID DUMMY", "Desc DUMMY");
        }

        //TODO: return DriverInfo

        //List<string> drivers = null;
        //public List<string> GetDrivers()
        //{
        //    if (!Isloaded)
        //    {
        //        ScanPackage();
        //    }
        //    if (drivers == null)
        //    {
        //        drivers = new List<string>();
        //        foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
        //        {
        //            var list = from type in PAI.Assembly.GetTypes()
        //                       where typeof(PluginDriverBase).IsAssignableFrom(type) && type.IsAbstract == false
        //                       select type;

        //            foreach (Type t in list)
        //            {
        //                PluginDriverBase driver = (PluginDriverBase)PAI.Assembly.CreateInstance(t.FullName);

        //                //TODO: get driver info from annotation on the class

        //                drivers.Add(driver.Name);
        //            }

        //        }
        //    }
        //    return drivers;
        //}

        // Services are used for standalone action/ ActWithoutDriver
        public IGingerService GetService(string serviceName)
        {
            if (!Isloaded)
            {
                ScanPackage();
            }
            // TODO: fix make more efficent and load only what needed
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {
                var list = from type in PAI.Assembly.GetTypes()
                           where typeof(IGingerService).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {
                    IGingerService service = (IGingerService)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
                    //TODO: read the attr of service
                    //if (service.Name == serviceName)
                    //{
                    //    return service;
                    //}
                }

            }
            throw new Exception("Service not found in Plugin Package - " + serviceName);
        }

        public List<IGingerService> GetServices()
        {
            // TODO: cache
            if (!Isloaded)
            {
                ScanPackage();
            }

            List<IGingerService> services = new List<IGingerService>();
            // TODO: fix make more efficent and load only what needed
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {
                var list = from type in PAI.Assembly.GetTypes()
                           where typeof(IGingerService).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {
                    IGingerService service = (IGingerService)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
                    services.Add(service);
                }

            }
            return services;
        }

        string PluginPackageServicesInfoFileName()
        {
            return Path.Combine(mFolder, "Ginger.PluginPackage.Actions.json");
        }

        public void CreateServicesInfo()
        {
            ObservableList<StandAloneAction> actions = GetStandAloneActions();
            
            string txt = JsonConvert.SerializeObject(actions, Formatting.Indented);
            File.WriteAllText(PluginPackageServicesInfoFileName(), txt);  
        }

        public List<StandAloneAction> LoadServicesInfoFromFile()
        {
            string fileName = PluginPackageServicesInfoFileName();
            if (!File.Exists(fileName))
            {
                throw new Exception("PluginPackage Services info file not found: " + fileName);
            }
            string txt = File.ReadAllText(fileName);
            List<StandAloneAction> actions = JsonConvert.DeserializeObject <List<StandAloneAction>>(txt);
            return actions;
        }


        public ObservableList<ITextEditor> GetTextFileEditors()
        {
            //TODO: cache
            if (!Isloaded)
            {
                ScanPackage();
            }

            ObservableList<ITextEditor> textEditors = new ObservableList<ITextEditor>();
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {                
                if (string.IsNullOrEmpty(mPluginPackageInfo.UIDLL))
                {
                    continue;
                }
                string UIDLLFileName = Path.Combine(mFolder, "UI", mPluginPackageInfo.UIDLL);                
                if (!File.Exists(UIDLLFileName))
                {
                    throw new Exception("Plugin UI DLL not found: " + UIDLLFileName);
                }
                Assembly assembly = Assembly.LoadFrom(UIDLLFileName);                

                var list = from type in assembly.GetTypes()
                           where typeof(ITextEditor).IsAssignableFrom(type) && type.IsAbstract == false
                           select type;

                foreach (Type t in list)
                {                    
                    ITextEditor textEditor = (ITextEditor)assembly.CreateInstance(t.FullName); // Activator.CreateInstance(t);
                    textEditors.Add(textEditor);                    
                }
            }
            return textEditors;
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.PluginPackage;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.PluginID);
            }
        }


        public string StartupDLL { get { return mPluginPackageInfo.StartupDLL; } }
    }
}
