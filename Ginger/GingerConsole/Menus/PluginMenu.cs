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
            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.P, "Plugin Menu");
            GingerGridMenu.SubItems.Add(LoadPluginMenuItem);
            GingerGridMenu.SubItems.Add(StartDriverMenuItem);
            return GingerGridMenu;
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
