using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace GingerCoreNETUnitTest.WorkSpaceLib
{

    public class WorkspaceHelper
    {
        

        public static void CreateWorkspaceWithTempSolution(string workspaceHolder, string solutionFolderName)
        {
            LockWorkspace(workspaceHolder);
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


        private static void LockWorkspace(string workspaceHolder)
        {
            WorkspaceLocker.StartSession(workspaceHolder);
        }

        public static void ReleaseWorkspace()
        {
            WorkspaceLocker.EndSession();
        }

        internal static void InitConsoleWorkspace(string workspaceHolder)
        {
            LockWorkspace(workspaceHolder);
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            WorkSpace.Init(consoleWorkspaceEventHandler);
        }

        internal static SolutionRepository CreateWorkspaceAndOpenSolution(string workspaceHolder, string path)
        {
            LockWorkspace(workspaceHolder);
            SolutionRepository solutionRepository;            
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            solutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            solutionRepository.Open(path);
            return solutionRepository;
        }

        internal static void CreateWorkspace2(string workspaceHolder)
        {
            LockWorkspace(workspaceHolder);
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

        }

        internal static void CreateDummyWorkSpace(string workspaceHolder)
        {
            LockWorkspace(workspaceHolder);
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws);
            WorkSpace.Instance.RunningFromUnitTest = true;
        }

        internal static void InitWS(string workspaceHolder)
        {
            LockWorkspace(workspaceHolder);
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            
        }
    }
}
