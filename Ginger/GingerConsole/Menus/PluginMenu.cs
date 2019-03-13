#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Amdocs.Ginger.GingerConsole
{
    public class PluginMenu
    {
        MenuItem LoadPluginMenuItem;
        MenuItem CreatePluginServicesinfojsonMenuItem;
        MenuItem CreatePluginPackageMenuItem;
        MenuItem StartServiceMenuItem;
        // static GingerConsoleScriptGlobals g = new GingerConsoleScriptGlobals();
        public MenuItem GetMenu()
        {
            LoadPluginMenuItem = new MenuItem(ConsoleKey.D1, "Load Plugin", () => LoadPlugin(), true);
            StartServiceMenuItem = new MenuItem(ConsoleKey.D2, "Start Service", () => StartService(), true);
            //StartServiceMenuItem = new MenuItem(ConsoleKey.D3, "Load Plugin and run Action", () => LoadPluginAndRunAction(), true);
            CreatePluginServicesinfojsonMenuItem = new MenuItem(ConsoleKey.D4, "Create Plugin Services info json", () => CreatePluginServicesinfojson(), true);
            CreatePluginPackageMenuItem = new MenuItem(ConsoleKey.D5, "Create Plugin Package", () => CreatePluginPackage(), true);
            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.P, "Plugin Menu");
            GingerGridMenu.SubItems.Add(LoadPluginMenuItem);
            GingerGridMenu.SubItems.Add(StartServiceMenuItem);
            GingerGridMenu.SubItems.Add(CreatePluginServicesinfojsonMenuItem);
            GingerGridMenu.SubItems.Add(CreatePluginPackageMenuItem);
            return GingerGridMenu;
        }

        private void CreatePluginPackage()
        {
            Console.WriteLine("Plugin Package folder? (use plugin publish folder bin/debug)");
            Console.WriteLine("Make sure to update the version at: Project-->Properties-->Package-->Package Version");
            string folder = Console.ReadLine();
            if (System.IO.Directory.Exists(folder))
            {

                Console.WriteLine("Verify Ginger.PluginPackage.json");
                PluginPackage p = new PluginPackage(folder);
                Console.WriteLine("Plugin ID: " + p.PluginId);
                Console.WriteLine("Plugin Version: " + p.PluginPackageVersion);
                Console.WriteLine("StartupDLL: " + p.StartupDLL);
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine("Creating ServicesInfo.json");
                
                p.CreateServicesInfo();

                string DLLFile = Path.Combine(folder, p.StartupDLL);                
                FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(DLLFile);

                string PackageVersion = myFileVersionInfo.ProductVersion.ToString();

                Console.WriteLine("PackageVersion: " + PackageVersion);                                  

                Console.WriteLine("Creating Zipfile");
                string ZipFolder = Directory.GetParent(folder).FullName;
                string zipFileName = Path.Combine(ZipFolder, p.PluginId + "." + PackageVersion + ".zip");
                ZipFile.CreateFromDirectory(folder, zipFileName);
                Console.WriteLine("Zipfile Created successfully: " + zipFileName);

                //Console.WriteLine("Upload to online Ginger Store? (Y/N)");
                //ConsoleKeyInfo rc = Console.ReadKey();
                //if (rc.Key == ConsoleKey.Y)
                //{
                //    Console.WriteLine("Upload to GIT");
                //    string cmd = "";
                //    // run cmd

                //    // it can take several minutes to apear in Nuget from Validationg-- > Listed
                //    // Email is sent when done

                //}
            }
            else
            {
                Console.WriteLine("folder not found");
            }
        }

        private void CreatePluginServicesinfojson()
        {
            Console.WriteLine("Plugin Package folder? (use plugin publish folder bin/debug)");
            string folder = Console.ReadLine();
            if (System.IO.Directory.Exists(folder))
            {
                PluginPackage p = new PluginPackage(folder);
                p.CreateServicesInfo();
            }
            else
            {
                Console.WriteLine("folder not found");
            }
        }

        private void LoadPluginAndRunAction()
        {
            //PluginPackage p = new PluginPackage();
            //Console.WriteLine("Plugin Package folder?");
            //string s = Console.ReadLine();
            //p.Folder = s;
            //p.ScanPackage();
            //ObservableList<StandAloneAction> list = p.GetStandAloneActions();
            //int i = 0;
            //foreach (StandAloneAction a in list)
            //{
            //    Console.WriteLine(i + ": " + a.ID);
            //    i++;
            //}
            //string actnum = Console.ReadLine();
            //ActionHandler AH =  p.GetStandAloneActionHandler(list[int.Parse(actnum)].ID);
            
            // FIXME need lazy load of params
            //foreach(ActionParam v in AH.GingerAction.InputParams.Values)
            //{                
            //    Console.WriteLine(v.Name + "?");
            //    string val = Console.ReadLine();
            //    v.Value = val;
            //}

            //ActionRunner.RunAction(AH.Instance, AH.GingerAction, AH);
        }

        private void StartService()
        {
            
            //TODO: let the user choose
            // Console.WriteLine("Starting Selenium Chrome Driver");
            // g.StartNode("Selenium Chrome Driver", "Chrome1");

            Console.WriteLine("Service Class full name?");
            string serviceClass = Console.ReadLine();
            string DLLFile = Path.Combine(p.Folder, p.StartupDLL);
            Assembly assembly = Assembly.LoadFrom(DLLFile);
            object service = assembly.CreateInstance(serviceClass);
            GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
            Console.WriteLine("Node name?");
            string nodeName = Console.ReadLine();
            Console.WriteLine("IP Address?");
            string ipAddr = Console.ReadLine();
            Console.WriteLine("Port Number?");
            string portNumber = Console.ReadLine();

            gingerNodeStarter.StartNode(nodeName, service, ipAddr, System.Convert.ToInt32(portNumber));
        }

        PluginPackage p;

        private void LoadPlugin()
        {
            Console.WriteLine("Plugin Package folder?");
            string folder = Console.ReadLine();
            p = new PluginPackage(folder);
            

            // TODO: list services

            
            
        }
    }
}
