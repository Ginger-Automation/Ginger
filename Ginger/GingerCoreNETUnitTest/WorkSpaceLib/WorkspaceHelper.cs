#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Amdocs.Ginger;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using System.IO;

namespace GingerCoreNETUnitTest.WorkSpaceLib
{

    public class WorkspaceHelper
    {

        public static void CreateWorkspaceWithTempSolution(string solutionFolderName)
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            string solutionfolder = TestResources.GetTestTempFolder(solutionFolderName);
            if (Directory.Exists(solutionfolder))
            {
                Directory.Delete(solutionfolder, true);
            }
            WorkSpace.Instance.SolutionRepository.CreateRepository(solutionfolder);
            WorkSpace.Instance.SolutionRepository.Open(solutionfolder);

        }


        internal static void InitConsoleWorkspace()
        {
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            WorkSpace.Init(consoleWorkspaceEventHandler);
        }




        internal static SolutionRepository CreateWorkspaceAndOpenSolution(string path)
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            WorkSpace.Instance.SolutionRepository.Open(path);
            return WorkSpace.Instance.SolutionRepository;
        }

        internal static void CreateWorkspace2()
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        }

        internal static void CreateDummyWorkSpace()
        {
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws);
            WorkSpace.Instance.RunningFromUnitTest = true;
        }

        //internal static WorkSpace InitWS(string workspaceHolder)
        //{         
        //    WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
        //    WorkSpace.Init(WSEH, workspaceHolder);
        //    ws.RunningFromUnitTest = true;
        //    ws.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        //    return ws;
        //}
    }
}
