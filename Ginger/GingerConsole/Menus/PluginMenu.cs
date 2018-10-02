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

using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.Repository;
using System;

namespace Amdocs.Ginger.GingerConsole
{
    public class PluginMenu
    {
        MenuItem LoadPluginMenuItem;
        MenuItem StartDriverMenuItem;

        static GingerConsoleScriptGlobals g = new GingerConsoleScriptGlobals();
        public MenuItem GetMenu()
        {
            LoadPluginMenuItem = new MenuItem(ConsoleKey.D1, "Load Plugin", () => LoadPlugin(), true);
            StartDriverMenuItem = new MenuItem(ConsoleKey.D2, "Start Driver", () => StartDriver(), true);
            StartDriverMenuItem = new MenuItem(ConsoleKey.D3, "Load Plugin and run Action", () => LoadPluginAndRunAction(), true);
            StartDriverMenuItem = new MenuItem(ConsoleKey.D4, "Create Plugin Actions info json", () => CreatePluginActionsinfojson(), true);
            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.P, "Plugin Menu");
            GingerGridMenu.SubItems.Add(LoadPluginMenuItem);
            GingerGridMenu.SubItems.Add(StartDriverMenuItem);
            return GingerGridMenu;
        }

        private void CreatePluginActionsinfojson()
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
            
            //// FIXME need lazy load of params
            //// FIXME params not loaded
            //foreach(ActionParam v in AH.GingerAction.InputParams.Values)
            //{                
            //    Console.WriteLine(v.Name + "?");
            //    string val = Console.ReadLine();
            //    v.Value = val;
            //}

            //ActionRunner.RunAction(AH.Instance, AH.GingerAction, AH);
        }

        private void StartDriver()
        {
            //TODO: let the user choose
            Console.WriteLine("Starting Selenium Chrome Driver");
            g.StartNode("Selenium Chrome Driver", "Chrome1");
        }

        private void LoadPlugin()
        {
            Console.WriteLine("Plugin Package folder?");
            string s = Console.ReadLine();            
            g.LoadPluginPackage(s);
        }
    }
}
