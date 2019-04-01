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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.GingerConsole.ReporterLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.CommandProcessorLib;
// using GingerCoreNET.CommandProcessorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Amdocs.Ginger.GingerConsole
{
    class Program
    {
        static SolutionMenu mSolutionMenu;
        
        static ManualResetEvent mCloseGingerConsoleEvent = new ManualResetEvent(false);

        static bool Keepalive = false;

        static MenuManager mMenuManager;

        static GingerGridMenu gingerGridMenu;
        
        static void Main(string[] args)
        {

            // TODO: Console.SetOut
            Console.ForegroundColor = ConsoleColor.Yellow;            
            Console.WriteLine("Ginger Console v3.0.0.2");
            Console.ResetColor();
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            //Enable cancel with CTRL-C
            Console.CancelKeyPress += (sender, eArgs) => {
                Console.WriteLine("CTRL+C pressed Canceling...");
                mCloseGingerConsoleEvent.Set();
                eArgs.Cancel = true;
                Keepalive = false;                      
            };

            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("-            Press CTRL+C to exit                 -");
            Console.WriteLine("---------------------------------------------------");

            Reporter.WorkSpaceReporter = new GingerConsoleWorkspaceReporter();

            // Init RepositorySerializer to use new Ginger classes
            NewRepositorySerializer RS = new NewRepositorySerializer();
            try
            {
                if (args.Count() > 0)
                {
                    ProcessArgs(args);
            
                    Console.WriteLine("Closing");
                    Thread.Sleep(1000);
                }
                else
                {
                    InitWorkSpace();

                    Console.WriteLine("Ginger Grid Started at Port:" + WorkSpace.Instance.LocalGingerGrid.Port);

                    InitMenu();
                    Keepalive = true;
                    while (Keepalive)
                    {
                        MenuManager.eMenuReturnCode rc = mMenuManager.ShowMenu();
                        if (rc == MenuManager.eMenuReturnCode.Quit) return; 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Thread.Sleep(3000);
            }
        }

        private static void InitMenu()
        {
            mMenuManager = new MenuManager();
            mMenuManager.MenuItems = new List<MenuItem>();

            mSolutionMenu = new SolutionMenu();
            mMenuManager.MenuItems.Add(mSolutionMenu.GetMenu());

            CodeProcessorMenu codeProcessorMenu = new CodeProcessorMenu();
            mMenuManager.MenuItems.Add(codeProcessorMenu.GetMenu());

            gingerGridMenu = new GingerGridMenu();
            mMenuManager.MenuItems.Add(gingerGridMenu.GetMenu());

            PluginMenu pluginMenu = new PluginMenu();
            mMenuManager.MenuItems.Add(pluginMenu.GetMenu());

            ScriptMenu scriptMenu = new ScriptMenu();
            mMenuManager.MenuItems.Add(scriptMenu.GetMenu());
        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine(DateTime.Now + " UnhandledException: " + e.ToString());
            Console.WriteLine("--------------------------------------------------------------");
            Console.Out.Flush();            
            Thread.Sleep(5000);
        }

        private static void InitWorkSpace()
        {
            GingerConsoleWorkSpace ws = new GingerConsoleWorkSpace();  
            WorkSpace.Init(ws);
        }

        private static void ProcessArgs(string[] args)
        {            
            CommandProcessor CP = new CommandProcessor();
            CP.RunCommand(args[0]);
        }

        private static Module A_ModuleResolve1(object sender, ResolveEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static Module A_ModuleResolve(object sender, ResolveEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
