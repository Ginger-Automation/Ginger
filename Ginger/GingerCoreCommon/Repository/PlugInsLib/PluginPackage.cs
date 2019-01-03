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
                if (mServices == null)
                {
                    LoadServicesFromJSON();
                }
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


        // When in dev mode we can add package from folder  which will ref a local folder
        string mLocalFolder;
        [IsSerializedForLocalRepository]
        public string LocalFolder
        {
            get
            {
                return mLocalFolder;
            }
            set
            {
                mLocalFolder = value;
                mFolder = value;
            }
        }


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
            if (mPluginPackageInfo.Version.ToLower().StartsWith("v"))
            {
                mPluginPackageInfo.Version = mPluginPackageInfo.Version.Substring(1);

            }
        }


        string mFolder;        
        public string Folder {
            get
            {       
                if (string.IsNullOrEmpty(mFolder))
                {
                    //if (!string.IsNullOrEmpty(LocalFolder))
                    //{
                    //    mFolder = LocalFolder;  // use for debug or when adding local folder instead of download
                    //}
                    //else
                    //{
                        mFolder = Path.Combine(LocalPluginsFolder, PluginId, PluginPackageVersion);
                   //}
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
                    foreach (Type type in types)
                    {
                        GingerServiceAttribute gingerServiceAttribute = (GingerServiceAttribute)Attribute.GetCustomAttribute(type, typeof(GingerServiceAttribute), false);
                        bool IsServiceSession = typeof(IServiceSession).IsAssignableFrom(type);  
                        // if service impl IServiceSession then mark it so we know we need and agent Start/Stop
                        // if it is not session , then all actions are stand alone 

                        PluginServiceInfo pluginServiceInfo = new PluginServiceInfo() { ServiceId = gingerServiceAttribute.Id, IsSession = IsServiceSession };
                        // expecting to get ExcelAction, FileAction, DatabaseAction...
                        MethodInfo[] methods = type.GetMethods();
                        Type[] interfaces = type.GetInterfaces();
                        foreach (MethodInfo MI in methods)
                        {
                            //Check if method have token [GingerAction] or is is of interface else we ignore
                            GingerActionAttribute gingerActionAttr = (GingerActionAttribute)Attribute.GetCustomAttribute(MI, typeof(GingerActionAttribute), false);                            

                            if (gingerActionAttr == null && !MI.IsVirtual)   // Virtual if it is interface
                            {                                
                                 continue;                                
                            }
                            
                            PluginServiceActionInfo action = new PluginServiceActionInfo();

                            if (gingerActionAttr != null)
                            {
                                action.ActionId = gingerActionAttr.Id;
                                action.Description = gingerActionAttr.Description;
                            }
                            else
                            {
                                // Check if the method is from interface
                                if (MI.IsVirtual)
                                {
                                    foreach (Type serviceInterface in interfaces)
                                    {
                                        //check if marked with [GingerInterface] 
                                        GingerInterfaceAttribute gingerInterfaceAttr = (GingerInterfaceAttribute)Attribute.GetCustomAttribute(serviceInterface, typeof(GingerInterfaceAttribute), false);
                                        
                                        if (gingerInterfaceAttr != null)
                                        {
                                            var mm = serviceInterface.GetMethod(MI.Name);
                                            if (mm != null)
                                            {
                                                // Get the action id and desc from the interface
                                                action.Interface = serviceInterface.FullName;

                                                GingerActionAttribute interfaceGAAttr = (GingerActionAttribute)Attribute.GetCustomAttribute(mm, typeof(GingerActionAttribute), false);
                                                if (interfaceGAAttr == null)
                                                {
                                                    // no GA attr then ignore
                                                    continue;
                                                }
                                                else
                                                {
                                                    action.ActionId = interfaceGAAttr.Id;
                                                    action.Description = interfaceGAAttr.Description;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (string.IsNullOrEmpty(action.ActionId)) continue;

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

        

        private void LoadGingerPluginsDLL()
        {
            // We make sure our latest in Ginger is the one to load GingerPlugin DLL so it will not be loaded from the folder of the plugin in case some one copied it too, or we point to debug folder
            // Just dummy does nothing except making sure we load the DLL first from inside Ginger
            GingerActionAttribute PIC = new GingerActionAttribute("ID DUMMY", "Desc DUMMY");
        }


        string PluginPackageServicesInfoFileName()
        {
            return Path.Combine(mFolder, "Ginger.PluginPackage.Services.json");
        }

        public void CreateServicesInfo()
        {
            LoadInfoFromDLL();

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
            string fileName = PluginPackageServicesInfoFileName();
            File.WriteAllText(fileName, txt);
            Console.WriteLine("Services file created at: " + fileName);
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


        public string StartupDLL
        {
            get
            {
                if (mPluginPackageInfo == null)
                {
                    LoadInfoFromJSON();
                }
                return mPluginPackageInfo.StartupDLL;
            }
        }


    }
}
