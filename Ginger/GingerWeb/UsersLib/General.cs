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
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.GingerConsole;
using Amdocs.Ginger.GingerConsole.ReporterLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore;
using System;
using System.IO;

namespace GingerWeb.UsersLib
{

    public class General
    {
        public static SolutionRepository SR;
        public static void init()
        {            
            Reporter.WorkSpaceReporter = new GingerConsoleWorkspaceReporter();
            // NewRepositorySerializer RS = new NewRepositorySerializer();

            GingerWebWorkspace ws = new GingerWebWorkspace();
            WorkSpace.Init(ws);
            
            // WorkSpace.Instance.OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");
            WorkSpace.Instance.Solution = (Solution)(ISolution)SR.RepositorySerializer.DeserializeFromFile(Path.Combine(SR.SolutionFolder, "Ginger.Solution.xml"));

            var gg = WorkSpace.Instance.LocalGingerGrid;
            var nodes = gg.NodeList;

            var v = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();            
        }


        // Combine to one in core !!!!!!!!!!!!!!!!!!!!!

        private static void OpenSolution(string sFolder)
        {            
            if (Directory.Exists(sFolder))
            {
                Console.WriteLine("Opening Solution at folder: " + sFolder);
                SR = GingerSolutionRepository.CreateGingerSolutionRepository();
                WorkSpace.Instance.SolutionRepository = SR;  
                SR.Open(sFolder);
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }

    }
}
