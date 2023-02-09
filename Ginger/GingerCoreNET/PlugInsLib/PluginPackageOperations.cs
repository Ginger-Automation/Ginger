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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository.PlugInsLib;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Plugin.Core.Attributes;
using Newtonsoft.Json;

namespace Amdocs.Ginger.Repository
{
    public class PluginPackageOperations : IPluginPackageOperations
    {
        // Not serialized loaded from Ginger.PluginPackage.json in the root folder
        PluginPackageInfo mPluginPackageInfo;
        ObservableList<PluginServiceInfo> mServices { get; set; }

        public PluginPackage PluginPackage;

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

        public PluginPackageInfo PluginPackageInfo
        {
            get
            {
                if (mPluginPackageInfo == null || mPluginPackageInfo.Id == null)
                {

                    LoadInfoFromJSON();
                }
                return mPluginPackageInfo;
            }
        }


        public bool Isloaded = false;
        public PluginPackageOperations(PluginPackage pluginPackage)
        {
            PluginPackage = pluginPackage;
            PluginPackage.PluginPackageOperations = this;
        }

        public void LoadPluginPackage(string folder)
        {
            PluginPackage.PluginPackageOperations = this;
            PluginPackage.Folder = folder;
            LoadInfoFromJSON();
            PluginPackage.PluginId = PluginPackageInfo.Id;
            PluginPackage.PluginPackageVersion = PluginPackageInfo.Version;
        }

        public void LoadInfoFromJSON()
        {
            //TODO: compare saved plugin id and version with the info file on folder

            if (!Directory.Exists(PluginPackage.Folder))
            {
                // TODO: auto download and restore , or show user a message to restore
                throw new Exception("Plugin folder not found: " + PluginPackage.Folder);
            }


            string pluginInfoFile = Path.Combine(PluginPackage.Folder, PluginPackageInfo.cInfoFile);

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


        List<PluginAssemblyInfo> mAssembliesInfo = new List<PluginAssemblyInfo>();

        public void LoadInfoFromDLL()
        {
            LoadGingerPluginsDLL();
            if (mPluginPackageInfo == null)
            {
                LoadInfoFromJSON();
            }
            string startupDLLFileName = Path.Combine(PluginPackage.Folder, mPluginPackageInfo.StartupDLL);
            PluginAssemblyInfo PAI = new PluginAssemblyInfo();
            PAI.Name = startupDLLFileName;
            PAI.FilePath = startupDLLFileName;
            mAssembliesInfo.Add(PAI);
            LoadPluginServicesFromDll();
        }


        void LoadPluginServicesFromDll()
        {
            if (mServices == null)  // Done only once
            {
                mServices = new ObservableList<PluginServiceInfo>();    // Move down !!!!!!!!!!!!!!!
                foreach (PluginAssemblyInfo pluginAssemblyInfo in mAssembliesInfo)
                {
                    var types2 = pluginAssemblyInfo.Assembly.GetExportedTypes();
                    IEnumerable<Type> types = from x in pluginAssemblyInfo.Assembly.GetTypes() where x.GetCustomAttribute(typeof(GingerServiceAttribute)) != null select x;
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

                                        /// !!!!!!!!!!!!! see new style and remove !!!!!!!!!!!!!!!!
                                        // Not sure if we need to list all method if they come from interface !!!!!!!!!!!!!! need to list only the interface

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

                            if (string.IsNullOrEmpty(action.ActionId))
                            {
                                continue;
                            }

                            foreach (ParameterInfo PI in MI.GetParameters())
                            {
                                if (PI.ParameterType.Name == nameof(IGingerAction))
                                {
                                    continue;
                                }

                                ActionInputValueInfo actionInputValueInfo = new ActionInputValueInfo() { Param = PI.Name, ParamType = PI.ParameterType };
                                actionInputValueInfo.ParamAttrs = new List<Attribute>();
                                action.InputValues.Add(actionInputValueInfo);

                                // Add Ginger param properties

                                Attribute[] attrs = Attribute.GetCustomAttributes(PI, typeof(Attribute), false);


                                // var v = PI.CustomAttributes; - not good
                                foreach (Attribute attribute in attrs)
                                {
                                    actionInputValueInfo.ParamAttrs.Add(attribute);
                                }

                            }


                            pluginServiceInfo.Actions.Add(action);
                        }

                        // Get all interfaces which are marked with attr 'GingerInterface'
                        foreach (Type PluginInterface in interfaces)
                        {
                            // decide if we need Feature for service and/or Interfaces seperate
                            GingerInterfaceAttribute gingerInterfaceAttr = (GingerInterfaceAttribute)Attribute.GetCustomAttribute(PluginInterface, typeof(GingerInterfaceAttribute), true);

                            if (gingerInterfaceAttr != null)
                            {
                                pluginServiceInfo.Interfaces.Add(gingerInterfaceAttr.Id);
                            }
                        }

                        MemberInfo[] members = type.GetMembers();

                        foreach (MemberInfo mi in members)
                        {
                            if (Attribute.GetCustomAttribute(mi, typeof(ServiceConfigurationAttribute), false) is ServiceConfigurationAttribute mconfig)
                            {
                                PluginServiceConfigInfo Config = new PluginServiceConfigInfo();
                                Config.Name = mconfig.Name;
                                Config.Description = mconfig.Description;
                                Config.Type = mconfig.GetType().Name;

                                if (Attribute.GetCustomAttribute(mi, typeof(ValidValueAttribute), false) is ValidValueAttribute validValues)
                                {

                                    foreach (var val in validValues.ValidValue)
                                    {
                                        Config.OptionalValues.Add(val.ToString());
                                    }

                                }
                                if (Attribute.GetCustomAttribute(mi, typeof(DefaultAttribute), false) is DefaultAttribute DefaultValue)
                                {

                                    Config.DefaultValue = DefaultValue == null ? string.Empty : DefaultValue.ToString();
                                }

                                pluginServiceInfo.Configs.Add(Config);
                            }
                        }
                        mServices.Add(pluginServiceInfo);
                    }
                }
            }
        }



        public PluginServiceInfo GetService(string serviceId)
        {
            return (from x in Services where x.ServiceId == serviceId select x).SingleOrDefault();
        }

        private void LoadGingerPluginsDLL()
        {
            // We make sure our latest in Ginger is the one to load GingerPlugin DLL so it will not be loaded from the folder of the plugin in case some one copied it too, or we point to debug folder
            // Just dummy does nothing except making sure we load the DLL first from inside Ginger
            GingerActionAttribute PIC = new GingerActionAttribute("ID DUMMY", "Desc DUMMY");
        }


        string PluginPackageServicesInfoFileName()
        {
            return Path.Combine(PluginPackage.Folder, "Ginger.PluginPackage.Services.json");
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
        }



        public void LoadServicesFromJSON()
        {
            string fileName = PluginPackageServicesInfoFileName();
            if (!File.Exists(fileName))
            {
                // Auto create if not exist                
                CreateServicesInfo();
            }
            string txt = File.ReadAllText(fileName);
            mServices = JsonConvert.DeserializeObject<ObservableList<PluginServiceInfo>>(txt);
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
