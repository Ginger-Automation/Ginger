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

using GingerCoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RosLynLib;
using System;

namespace Amdocs.Ginger.GingerConsole
{
    public class ScriptMenu
    {
        MenuItem ListScriptsMenuItem;
        MenuItem RunScriptMenuItem;

        public MenuItem GetMenu()
        {
            ListScriptsMenuItem = new MenuItem(ConsoleKey.D1, "List Scripts", () => ListScripts(), true);
            RunScriptMenuItem = new MenuItem(ConsoleKey.D2, "Run Script", () => RunScript(), true);

            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.R, "scRipts Menu");
            GingerGridMenu.SubItems.Add(ListScriptsMenuItem);
            GingerGridMenu.SubItems.Add(RunScriptMenuItem);

            return GingerGridMenu;
        }

        static string[] files;
        private void RunScript()
        {
            Console.WriteLine("Script file name or #?");
            string filename;
            string s = Console.ReadLine();
            int index;
            bool b = int.TryParse(s, out index);
            if (b)
            {
                filename = files[index-1];
            }
            else
            {
                filename = s;
            }
            string script = System.IO.File.ReadAllText(filename);
            object o = CodeProcessor.Execute(script);
            if (o!=null)
            {
                Console.WriteLine(o);
            }
        }

        private void ListScripts()
        {   
            string folder = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("Searching *.ginger files at: " + folder);

            files = System.IO.Directory.GetFiles(folder, "*.ginger");

            if (files.Length == 0)
            {
                Console.WriteLine("No files found");
                return;
            }
            int i = 1;
            foreach (string file in files)
            {
                Console.WriteLine(i + ". " + System.IO.Path.GetFileName(file));
            }
        }
    }
}
