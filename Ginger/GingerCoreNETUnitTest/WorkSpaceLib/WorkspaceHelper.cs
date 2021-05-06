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
            WorkSpace.Instance.SolutionRepository  = GingerSolutionRepository.CreateGingerSolutionRepository();
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
