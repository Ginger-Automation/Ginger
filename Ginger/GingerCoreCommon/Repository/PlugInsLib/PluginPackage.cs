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
using Amdocs.Ginger.Common.Repository.PlugInsLib;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackage : RepositoryItemBase
    {
        // Not serialized loaded from Ginger.PluginPackage.json in the root folder
        PluginPackageInfo mPluginPackageInfo;
        ObservableList<PluginServiceInfo> mServices { get; set; }
        public ObservableList<PluginServiceInfo> Services
        {
            get
            {
                return mServices;
            }
        }

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

        
        [IsSerializedForLocalRepository]
        public string PluginId { get; set;  }

        [IsSerializedForLocalRepository]
        public string PluginPackageVersion { get; set; }
        
        public bool Isloaded = false;
        // must have empty constructor
        public PluginPackage()
        {
        }


        public PluginPackage(string folder)
        {            
            mFolder = folder;
            LoadInfoFromJSON();            
            PluginId = PluginPackageInfo.Id;
            PluginPackageVersion = PluginPackageInfo.Version;
        }

        private void LoadInfoFromJSON()
        {
            //TODO: compare saved plugin id and version with the info file on folder

            if (!Directory.Exists(mFolder))
            {
                // TODO: auto download and restore , or show user a message to restore
                throw new Exception("Plugin folder not found: " + mFolder);
            }


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
        public string Folder {
            get
            {       
                if (string.IsNullOrEmpty(mFolder))
                {
                    mFolder = Path.Combine(LocalPluginsFolder, PluginId, PluginPackageVersion);
                }

                return mFolder;
            }
            set
            {
                if (mFolder != value)
                {
                    mFolder = value;
                    LoadInfoFromJSON();
                    LoadServicesFromJSON();
                    OnPropertyChanged(nameof(Folder));
                }
            }
        }


        public static string LocalPluginsFolder
        {
            get
            {
                string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                userFolder = Path.Combine(userFolder, "Ginger", "PluginPackages");
                return userFolder;
            }
        }
      
        List<PluginAssemblyInfo> mAssembliesInfo = new List<PluginAssemblyInfo>();

        public override string ItemName { get { return PluginId; } set {  } }
        

        public override string GetNameForFileName()
        {
            return PluginId;
        }

        public void LoadInfoFromDLL()
        {
            LoadGingerPluginsDLL();
            string startupDLLFileName = Path.Combine(mFolder, mPluginPackageInfo.StartupDLL);            
            PluginAssemblyInfo PAI = new PluginAssemblyInfo();
            PAI.Name = startupDLLFileName;
            PAI.FilePath = startupDLLFileName;
            mAssembliesInfo.Add(PAI);

            LoadPluginServicesFromDll();
        }


        void LoadPluginServicesFromDll()
        {                    
            if (mServices == null)
            {
                mServices = new ObservableList<PluginServiceInfo>();
                foreach (PluginAssemblyInfo asssembly in mAssembliesInfo)
                {                    
                    IEnumerable<Type> types = from x in asssembly.Assembly.GetTypes() where x.GetCustomAttribute(typeof(GingerServiceAttribute)) != null select x;
                    foreach (Type t in types)
                    {
                        GingerServiceAttribute gingerServiceAttribute = (GingerServiceAttribute)Attribute.GetCustomAttribute(t, typeof(GingerServiceAttribute), false);
                        PluginServiceInfo pluginServiceInfo = new PluginServiceInfo() { ServiceId = gingerServiceAttribute.Id };
                        // expecting to get ExcelAction, FileAction, DatabaseAction...
                        MethodInfo[] methods = t.GetMethods();
                        foreach (MethodInfo MI in methods)
                        {
                            //Check if method have token [GingerAction]  else we ignore
                            GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);

                            if (token == null)
                            {
                                continue;
                            }

                            // pluginService.mStandAloneMethods.Add(MI);
                            // pluginServiceInfo.Actions = GetActionFromDLL(pluginServiceInfo);

                            // GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);
                            PluginServiceActionInfo action = new PluginServiceActionInfo();
                            action.ActionId = token.Id;                            
                            action.Description = token.Description;
                            foreach (ParameterInfo PI in MI.GetParameters())
                            {
                                if (PI.ParameterType.Name != nameof(IGingerAction))
                                {
                                    action.InputValues.Add(new ActionInputValueInfo() { Param = PI.Name, ParamType = PI.ParameterType });
                                }
                            }
                            pluginServiceInfo.Actions.Add(action);
                        }
                        mServices.Add(pluginServiceInfo);
                    }                    
                }
            }            
        }

        //public ObservableList<PluginServiceAction> GetActionFromDLL()
        //{
        //    ObservableList<PluginServiceAction> list = new ObservableList<PluginServiceAction>();

        //    foreach (PluginServiceInfo pluginService in LoadPluginServicesFromDLL())
        //    {
        //        foreach (MethodInfo MI in pluginService.mStandAloneMethods)
        //        {
        //            GingerActionAttribute token = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);
        //            StandAloneAction DA = new StandAloneAction();
        //            DA.ActionId = token.Id;
        //            // AssemblyName AN = MI.DeclaringType.Assembly.GetName();
        //            DA.PluginId = PluginID;  //AN.Name;
        //            DA.ServiceId = pluginService.ServiceId;
        //            DA.Description = token.Description;
        //            foreach (ParameterInfo PI in MI.GetParameters())
        //            {
        //                if (PI.ParameterType.Name != nameof(GingerAction))
        //                {
        //                    DA.InputValues.Add(new ActionInputValueInfo() { Param = PI.Name, ParamType = PI.ParameterType });
        //                }
        //            }
        //            list.Add(DA);
        //        }
        //    }
        //    return list;
        //}

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
        //public object GetService(string serviceName)
        //{
        //    if (!Isloaded)
        //    {
        //        LoadInfoFromDLL();
        //    }
        //    // TODO: fix make more efficent and load only what needed
        //    foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
        //    {
        //        var list = from type in PAI.Assembly.GetTypes()
        //                   where typeof(IGingerService).IsAssignableFrom(type) && type.IsAbstract == false
        //                   select type;

        //        foreach (Type t in list)
        //        {
        //            IGingerService service = (IGingerService)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
        //            //TODO: read the attr of service
        //            //if (service.Name == serviceName)
        //            //{
        //            //    return service;
        //            //}
        //        }

        //    }
        //    throw new Exception("Service not found in Plugin Package - " + serviceName);
        //}

        //public List<IGingerService> GetServices()
        //{
        //    // TODO: cache
        //    if (!Isloaded)
        //    {
        //        ScanPackage();
        //    }

        //    List<IGingerService> services = new List<IGingerService>();
        //    // TODO: fix make more efficent and load only what needed
        //    foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
        //    {
        //        var list = from type in PAI.Assembly.GetTypes()
        //                   where typeof(IGingerService).IsAssignableFrom(type) && type.IsAbstract == false
        //                   select type;

        //        foreach (Type t in list)
        //        {
        //            IGingerService service = (IGingerService)PAI.Assembly.CreateInstance(t.FullName);   // TODO: fix me find the driver without creating instance
        //            services.Add(service);
        //        }

        //    }
        //    return services;
        //}

        string PluginPackageServicesInfoFileName()
        {
            return Path.Combine(mFolder, "Ginger.PluginPackage.Services.json");
        }

        public void CreateServicesInfo()
        {
            LoadInfoFromDLL();
            
            // IEnumerable<StandAloneAction> actions = GetStandAloneActions();

            // Verify no dup IDs
            foreach (PluginServiceInfo pluginServiceInfo in mServices)
            {
                var query = pluginServiceInfo.Actions.GroupBy(x => new { x.ActionId })
                              .Where(g => g.Count() > 1)
                              .Select(y => y.Key)
                              .ToList();
                if (query.Count > 0)
                {
                    string errorText = "Error: Plugin contains duplicate Action ID for same service" + Environment.NewLine;
                    foreach (object item in query)
                    {
                        errorText += "Service=" + pluginServiceInfo.ServiceId + ", " + item.ToString() + Environment.NewLine;
                    }
                    throw new Exception(errorText);
                }
            }
            string txt = JsonConvert.SerializeObject(mServices, Formatting.Indented);
            File.WriteAllText(PluginPackageServicesInfoFileName(), txt);  
        }


        
        public void LoadServicesFromJSON()
        {                          
                string fileName = PluginPackageServicesInfoFileName();
                if (!File.Exists(fileName))
                {
                    throw new Exception("PluginPackage Services info file not found: " + fileName);
                }
                string txt = File.ReadAllText(fileName);
                mServices = JsonConvert.DeserializeObject<ObservableList<PluginServiceInfo>>(txt);                            
        }

        //public IEnumerable<string> Services   // temp !!!!!!!!!!!!!!!!!!!!!
        //{
        //    get
        //    {
        //        return from x in mServicesActions select x.ServiceId;   // temp !!!!!!!!!!!!!!!!!!!!!
        //    }
        //}



        public ObservableList<ITextEditor> GetTextFileEditors()
        {
            //TODO: cache
            if (!Isloaded)
            {
                LoadInfoFromDLL();
            }

            ObservableList<ITextEditor> textEditors = new ObservableList<ITextEditor>();
            foreach (PluginAssemblyInfo PAI in mAssembliesInfo)
            {

                Assembly assembly = null;
                if (string.IsNullOrEmpty(mPluginPackageInfo.UIDLL))
                {
                    continue;
                }
                string UIDLLFileName = Path.Combine(mFolder, "UI", mPluginPackageInfo.UIDLL);                
                if (!File.Exists(UIDLLFileName))
                {
                    throw new Exception("Plugin UI DLL not found: " + UIDLLFileName);
                }                
                assembly = Assembly.UnsafeLoadFrom(UIDLLFileName);               

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
                return nameof(this.PluginId);
            }
        }


        public string StartupDLL { get { return mPluginPackageInfo.StartupDLL; } }


    }
}
