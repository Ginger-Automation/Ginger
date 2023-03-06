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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Reports.ReportHelper;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.GingerRuntime.ReporterLib;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Amdocs.Ginger.GingerRuntime
{
    public class Program
    {
        static SolutionMenu mSolutionMenu;

        static ManualResetEvent mCloseGingerConsoleEvent = new ManualResetEvent(false);

        static bool Keepalive = false;

        static MenuManager mMenuManager;

        static GingerGridMenu gingerGridMenu;

        public static void Main(string[] args)
        {
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.InitLog4Net();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.ResetColor();
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            //Enable cancel with CTRL-C
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                Console.WriteLine("CTRL+C pressed Canceling...");
                mCloseGingerConsoleEvent.Set();
                eArgs.Cancel = true;
                Keepalive = false;
            };

            Reporter.WorkSpaceReporter = new GingerRuntimeWorkspaceReporter();

            Reporter.ReportAllAlsoToConsole = true;  //needed so all reporting will be added to Console      
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.PrintStartUpInfo();

            // Init RepositorySerializer to use new Ginger classes
            NewRepositorySerializer RS = new NewRepositorySerializer();

            try
            {
                if (args.Count() > 0)
                {
                    if (args.Count() == 1 && args[0].ToLower().Trim() == "menu")
                    {
                        Console.WriteLine();
                        Console.WriteLine("---------------------------------------------------");
                        Console.WriteLine("-            Press CTRL+C to exit                 -");
                        Console.WriteLine("---------------------------------------------------");

                        InitWorkSpace(true);
                        InitMenu();
                        Keepalive = true;
                        while (Keepalive)
                        {
                            MenuManager.eMenuReturnCode rc = mMenuManager.ShowMenu();
                            if (rc == MenuManager.eMenuReturnCode.Quit) return;
                        }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.INFO, "Processing command line arguments...");
                        ProcessArgs(args);
                        Reporter.ToLog(eLogLevel.INFO, "Processing command line arguments completed");
                    }
                }
                else
                {
                    Console.WriteLine("Please provide arguments for starting execution or add 'menu' command for more options.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Thread.Sleep(3000);
            }

            if (WorkSpace.Instance != null)
            {
                WorkSpace.Instance.Close();
            }
        }



        private static void InitMenu()
        {
            mMenuManager = new MenuManager();
            mMenuManager.MenuItems = new List<MenuItem>();

            mSolutionMenu = new SolutionMenu();
            mMenuManager.MenuItems.Add(mSolutionMenu.GetMenu());

            gingerGridMenu = new GingerGridMenu();
            mMenuManager.MenuItems.Add(gingerGridMenu.GetMenu());

            PluginMenu pluginMenu = new PluginMenu();
            mMenuManager.MenuItems.Add(pluginMenu.GetMenu());

        }

        private static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine(DateTime.Now + " UnhandledException: " + e.ToString());
            Console.WriteLine("--------------------------------------------------------------");
            Console.Out.Flush();
            Thread.Sleep(5000);
        }


        private static void InitWorkSpace(bool startLocalGrid)
        {
            GingerConsoleWorkSpace ws = new GingerConsoleWorkSpace();
            WorkSpace.Init(ws, startLocalGrid);
        }

        private async static void ProcessArgs(string[] args)
        {
            InitWorkSpace(false);
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reporting will be added to Console   
            WorkSpace.Instance.InitWorkspace(new GingerRuntimeWorkspaceReporter(), new DotnetCoreHelper());
            CLIProcessor CLI = new CLIProcessor();
            await CLI.ExecuteArgs(args);
        }


    }
}
