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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.EnvironmentsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.GingerConsole
{
    public class SolutionMenu
    {
        SolutionRepository SR;
        MenuItem OpenSolutionMenuItem;
        MenuItem CloseSolutionMenuItem;
        MenuItem BusinessFlowsCountMenuItem;
        MenuItem BusinessFlowsListMenuItem;
        MenuItem EnvironmentsListMenuItem;

        public MenuItem GetMenu()
        {
            OpenSolutionMenuItem = new MenuItem(ConsoleKey.O, "Open", () => OpenSolution(), true);
            CloseSolutionMenuItem = new MenuItem(ConsoleKey.C, "Close", CloseSolution, false);
            BusinessFlowsCountMenuItem = new MenuItem(ConsoleKey.B, "Business Flows Count", BusinessFlowsCount, false);
            BusinessFlowsListMenuItem = new MenuItem(ConsoleKey.L, "Business Flows List", BusinessFlowsList, false);
            EnvironmentsListMenuItem = new MenuItem(ConsoleKey.E, "Environments List", EnvironmentsList, false);

            MenuItem SolutionMenu = new MenuItem(ConsoleKey.S, "Solution");
            SolutionMenu.SubItems.Add(OpenSolutionMenuItem);
            SolutionMenu.SubItems.Add(CloseSolutionMenuItem);
            SolutionMenu.SubItems.Add(BusinessFlowsCountMenuItem);
            SolutionMenu.SubItems.Add(BusinessFlowsListMenuItem);
            SolutionMenu.SubItems.Add(EnvironmentsListMenuItem);

            return SolutionMenu;
        }

        private void OpenSolution()
        {
            Console.WriteLine("Solution Folder?");
            string sFolder = Console.ReadLine();
            if (Directory.Exists(sFolder))
            {

                Console.WriteLine("Opening Solution at folder: " + sFolder);

                SR = new SolutionRepository();
                SR.Open(sFolder);
                IEnumerable<RepositoryFile> files = SR.GetAllSolutionRepositoryFiles().ToList();
                Console.WriteLine("Files count = " + files.Count());

                Console.WriteLine("Ginger Menu - Solution: " + sFolder);

                OpenSolutionMenuItem.Active = false;
                CloseSolutionMenuItem.Active = true;
                BusinessFlowsCountMenuItem.Active = true;
                BusinessFlowsListMenuItem.Active = true;
                EnvironmentsListMenuItem.Active = true;
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }

        private void BusinessFlowsCount()
        {
            //Console.WriteLine("Executing BFs count");
            //ObservableList<BusinessFlow> BFs = SR.GetAllRepositoryItems<BusinessFlow>();
            //Console.WriteLine("BFs count = " + BFs.Count());
        }

        private void CloseSolution()
        {
            SR = null;
            OpenSolutionMenuItem.Active = true;
            CloseSolutionMenuItem.Active = false;
            BusinessFlowsCountMenuItem.Active = false;
            BusinessFlowsListMenuItem.Active = false;
            EnvironmentsListMenuItem.Active = false;
        }

        private void BusinessFlowsList()
        {
            //Console.WriteLine("Executing BFs List");
            //ObservableList<BusinessFlow> BFs = SR.GetAllRepositoryItems<BusinessFlow>();
            //int count = 0;
            //foreach (BusinessFlow BF in BFs)
            //{
            //    count++;
            //    Console.WriteLine("# " + count + " - " + BF.Name);
            //}
        }

        void EnvironmentsList()
        {            
            //ObservableList<ProjEnvironment> v = SR.GetAllRepositoryItems<ProjEnvironment>();
            //foreach (ProjEnvironment e in v)
            //{
            //    Console.WriteLine(e.Name);
            //}
        }
    }
}
