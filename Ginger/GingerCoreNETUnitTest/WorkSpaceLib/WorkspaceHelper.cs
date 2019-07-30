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

        public static void CreateWorkspaceWithTempSolution(string workspaceLocker, string solutionFolderName)
        {
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder, workspaceLocker);
            WorkSpace.Init(new WorkSpaceEventHandler(), workspaceLocker);
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

     
        internal static void InitConsoleWorkspace(string workspaceLocker)
        {            
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            WorkSpace.Init(consoleWorkspaceEventHandler, workspaceLocker);
        }


       

        internal static SolutionRepository CreateWorkspaceAndOpenSolution(string workspaceLocker, string path)
        {         
            SolutionRepository solutionRepository;            
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH, workspaceLocker);
            WorkSpace.Instance.RunningFromUnitTest = true;
            solutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            solutionRepository.Open(path);
            return solutionRepository;
        }

        internal static void CreateWorkspace2(string holdBy)
        {         
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH, holdBy);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        }

        internal static void CreateDummyWorkSpace(string workspaceLocker)
        {         
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws, workspaceLocker);
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
