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

        // static WorkspaceLocker workspaceLocker;

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

        //private static void LockWorkspace(string workspaceHolder)
        //{


        //    StartSession();

        //    //lock (_locker)
        //    //{
        //    //    while (mWorkspaceHolder != null)
        //    //    {
        //    //        Thread.Sleep(100);
        //    //    }
        //    //    bool gotMutex = mMutex.WaitOne();  //Wait max 60 sec to get workspace - no WS test should take more than 60 seconds
        //    //    if (gotMutex)
        //    //    {
        //    //        //Thread.Sleep(2000);
        //    //        if (mWorkspaceHolder != null)
        //    //        {
        //    //            throw new Exception(" got Mutex but mWorkspaceHolder!= null and hold by: " + mWorkspaceHolder);
        //    //        }
        //    //        mWorkspaceHolder = workspaceHolder;
        //    //    }
        //    //    else
        //    //    {
        //    //        throw new Exception("Cannot lock Workspace Mutex after 60 seconds");
        //    //    }
        //    //}

        ////}

        

        //private static void LockWorkspace(string workspaceHolder)
        //{
        //    //check if null
        //    workspaceLocker = new WorkspaceLocker(workspaceHolder);
        //    // WorkspaceLocker.StartSession(workspaceHolder);
        //}

        //public static void ReleaseWorkspace()
        //{
        //    // WorkspaceLocker.EndSession();
        //    workspaceLocker.ReleaseWorkspace();
        //}

        internal static void InitConsoleWorkspace(string workspaceLocker)
        {
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder, workspaceLocker);
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            WorkSpace.Init(consoleWorkspaceEventHandler, workspaceLocker);
        }

        internal static SolutionRepository CreateWorkspaceAndOpenSolution(string workspaceLocker, string path)
        {
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder, workspaceHolder);
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
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder);
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH, holdBy);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        }

        internal static void CreateDummyWorkSpace(string workspaceLocker)
        {
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder);
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws, workspaceLocker);
            WorkSpace.Instance.RunningFromUnitTest = true;
        }

        internal static void InitWS(string workspaceHolder)
        {
            // workspaceLocker = new WorkspaceLocker(workspaceHolder);
            // LockWorkspace(workspaceHolder);
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH, workspaceHolder);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            
        }
    }
}
