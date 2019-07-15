using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.CoreNET.Repository;
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
        static readonly object _locker = new object();
        static string mWorkspaceHolder;
        static readonly Mutex mMutex = new Mutex();

        // Enable to run only one Ginger for all tests and one test at a time
        private static Mutex TestMutex = new Mutex();

        static int SessionCount = 0; // count how many sessions are waiting in queue

        static WorkspaceHelper gingerAutomatorInstance;  // currently we have only one Ginger running for all tests

        public static WorkspaceHelper StartSession()
        {
            SessionCount++;
            TestMutex.WaitOne();  // Make sure we run one session at a time, wait for session to be free
            if (gingerAutomatorInstance == null)
            {
                gingerAutomatorInstance = new WorkspaceHelper();                
            }
            return gingerAutomatorInstance;
        }

        public static void EndSession()
        {
            SessionCount--;
            TestMutex.ReleaseMutex();


            if (SessionCount == 0)
            {
                
            }
        }



        private static void LockWorkspace(string workspaceHolder)
        {
            

            StartSession();

            //lock (_locker)
            //{
            //    while (mWorkspaceHolder != null)
            //    {
            //        Thread.Sleep(100);
            //    }
            //    bool gotMutex = mMutex.WaitOne();  //Wait max 60 sec to get workspace - no WS test should take more than 60 seconds
            //    if (gotMutex)
            //    {
            //        //Thread.Sleep(2000);
            //        if (mWorkspaceHolder != null)
            //        {
            //            throw new Exception(" got Mutex but mWorkspaceHolder!= null and hold by: " + mWorkspaceHolder);
            //        }
            //        mWorkspaceHolder = workspaceHolder;
            //    }
            //    else
            //    {
            //        throw new Exception("Cannot lock Workspace Mutex after 60 seconds");
            //    }
            //}

        }

        public static void ReleaseWorkspace()
        {
            
            //lock (_locker)
            //{
            try
            {

                WorkSpace.Instance.CloseSolution();
                WorkSpace.Instance.LocalGingerGrid.Stop();

                WorkSpace.Instance.Close();
                mWorkspaceHolder = null;

            }
            catch (Exception ex)
            {

            }
            finally
            {
                
                //    mMutex.ReleaseMutex();
            }
            //}
            EndSession();
        }

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
