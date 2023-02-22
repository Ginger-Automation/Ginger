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
using System.Diagnostics;

namespace Amdocs.Ginger.GingerRuntime
{
    public class MenuManager
    {
        public List<MenuItem> MenuItems;
        List<MenuItem> CurrentMenu;

        public enum eMenuReturnCode
        {
            Executed,
            Quit,
            Exception,
            MainMenu,
            UnknownKey,
        }

        public eMenuReturnCode ShowMenu()
        {
            if (CurrentMenu == null)
            {
                CurrentMenu = MenuItems;
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("");
            Console.WriteLine("==================================");
            Console.WriteLine("=====      Ginger Menu       =====");
            Console.WriteLine("==================================");

            foreach (MenuItem MI in CurrentMenu)
            {
                if (MI.Active)
                {
                    string s = MI.Key.ToString();

                    if (MI.Key == ConsoleKey.D0) s = "0";
                    if (MI.Key == ConsoleKey.D1) s = "1";
                    if (MI.Key == ConsoleKey.D2) s = "2";
                    if (MI.Key == ConsoleKey.D3) s = "3";
                    if (MI.Key == ConsoleKey.D4) s = "4";
                    if (MI.Key == ConsoleKey.D5) s = "5";
                    if (MI.Key == ConsoleKey.D6) s = "6";
                    if (MI.Key == ConsoleKey.D7) s = "7";
                    if (MI.Key == ConsoleKey.D8) s = "8";
                    if (MI.Key == ConsoleKey.D9) s = "9";                    

                    s += ". " + MI.Name;
                    Console.WriteLine(s);
                }
            }
            
            Console.WriteLine("Q. Quit");
            if (CurrentMenu != MenuItems)
            {
                Console.WriteLine("0. Top Menu");
            }
            Console.WriteLine("==================================");
            Console.Write("Please select:");

            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleKeyInfo key = Console.ReadKey();
            Console.WriteLine();

            foreach (MenuItem MI in CurrentMenu)
            {
                if (MI.Key == key.Key)
                {
                    if (MI.Action == null)
                    {
                        // this is sub menu
                        CurrentMenu = MI.SubItems;
                        return 0;
                    }
                    else
                    {
                        if (MI.Active)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine(DateTime.Now.ToString() +  " Executing: " + MI.Name);
                            Console.WriteLine("--------------------------------");
                            Stopwatch st = Stopwatch.StartNew();
                            try
                            {
                                MI.Action.Invoke();
                                st.Stop();
                                Console.WriteLine("--------------------------------");
                                Console.WriteLine("Elapsed: " + st.Elapsed);
                                return eMenuReturnCode.Executed;
                            }
                            catch (Exception ex)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Error while executing: " + MI.Name + Environment.NewLine + ex.Message);
                                Console.ResetColor();
                                return eMenuReturnCode.Exception;
                            }
                            
                        }                        
                    }
                }
            }

            if (key.Key == ConsoleKey.Q)
            {
                Console.WriteLine("bye bye");
                return eMenuReturnCode.Quit;
            }

            if (key.Key == ConsoleKey.D0)
            {
                CurrentMenu = MenuItems;
                return eMenuReturnCode.MainMenu;
            }

            Console.WriteLine("Unknown key menu: " + key.Key);
            return eMenuReturnCode.UnknownKey;
        }
    }
}
