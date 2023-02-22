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

using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Amdocs.Ginger.GingerRuntime
{
    public class PluginMenu
    {
        // MenuItem LoadPluginMenuItem;
        MenuItem CreatePluginServicesinfojsonMenuItem;
        MenuItem CreatePluginPackageMenuItem;
        // MenuItem StartServiceMenuItem;
        // static GingerConsoleScriptGlobals g = new GingerConsoleScriptGlobals();
        public MenuItem GetMenu()
        {            
            // StartServiceMenuItem = new MenuItem(ConsoleKey.D2, "Start Service", () => StartService(), true);
            //StartServiceMenuItem = new MenuItem(ConsoleKey.D3, "Load Plugin and run Action", () => LoadPluginAndRunAction(), true);
            CreatePluginServicesinfojsonMenuItem = new MenuItem(ConsoleKey.D4, "Create Plugin Services info json", () => CreatePluginServicesinfojson(), true);
            CreatePluginPackageMenuItem = new MenuItem(ConsoleKey.D5, "Create Plugin Package", () => CreatePluginPackage(), true);
            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.P, "Plugin Menu");           
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
                p.PluginPackageOperations = new PluginPackageOperations(p);
                p.PluginPackageOperations.LoadPluginPackage(folder);
                Console.WriteLine("Plugin ID: " + p.PluginId);
                Console.WriteLine("Plugin Version: " + p.PluginPackageVersion);
                Console.WriteLine("StartupDLL: " + p.PluginPackageOperations.StartupDLL);
                Console.WriteLine("---------------------------------------------------------");
                Console.WriteLine("Creating ServicesInfo.json");
                
                p.PluginPackageOperations.CreateServicesInfo();

                string DLLFile = Path.Combine(folder, p.PluginPackageOperations.StartupDLL);                
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
            Console.WriteLine("!!! Obsolete !!! - Plugin Package Services json is automatically created - no need to generate");
            Console.WriteLine("BUT it must be packed in plugin package");
            Console.WriteLine("Use Create Plugin package feature which will zip it");
            Console.WriteLine("Plugin Package folder? (use plugin publish folder bin/debug)");
            string folder = Console.ReadLine();
            if (System.IO.Directory.Exists(folder))
            {
                PluginPackage p = new PluginPackage(folder);
                p.PluginPackageOperations = new PluginPackageOperations(p);
                p.PluginPackageOperations.LoadPluginPackage(folder);
                p.PluginPackageOperations.CreateServicesInfo();
            }
            else
            {
                Console.WriteLine("folder not found");
            }
        }

       

        //private void StartService()
        //{
            
        //    //TODO: let the user choose
        //    // Console.WriteLine("Starting Selenium Chrome Driver");
        //    // g.StartNode("Selenium Chrome Driver", "Chrome1");

        //    Console.WriteLine("Service Class full name?");
        //    string serviceClass = Console.ReadLine();
        //    string DLLFile = Path.Combine(p.Folder, p.StartupDLL);
        //    Assembly assembly = Assembly.LoadFrom(DLLFile);
        //    object service = assembly.CreateInstance(serviceClass);
        //    GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
        //    Console.WriteLine("Node name?");
        //    string nodeName = Console.ReadLine();
        //    Console.WriteLine("IP Address?");
        //    string ipAddr = Console.ReadLine();
        //    Console.WriteLine("Port Number?");
        //    string portNumber = Console.ReadLine();

        //    gingerNodeStarter.StartNode(nodeName, service, ipAddr, System.Convert.ToInt32(portNumber));
        //}
        

       
    }
}
