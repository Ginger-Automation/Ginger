using amdocs.ginger.GingerCoreNET;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace amdocs.ginger.GingerCoreNET
{
    public class WorkspaceLocker
    {
        static readonly object _locker = new object();
        static string mWorkspaceHolder;
        static readonly Mutex mMutex = new Mutex();

        // Enable to run only one Ginger for all tests and one test at a time
        private static Mutex TestMutex = new Mutex();

        static int SessionCount = 0; // count how many sessions are waiting in queue

        static WorkspaceLocker WorkspaceLockerInstance;  // currently we have only one Ginger running for all tests so use one workspace at time

        public static WorkspaceLocker StartSession(string name)
        {
            SessionCount++;
            TestMutex.WaitOne();  // Make sure we run one session at a time, wait for session to be free
            if (WorkspaceLockerInstance == null)
            {
                WorkspaceLockerInstance = new WorkspaceLocker();
            }
            mWorkspaceHolder = name;
            return WorkspaceLockerInstance;
        }

        public static string HoldBy
        {
            get
            {
                return mWorkspaceHolder;
            }
        }

        public static void EndSession()
        {
            SessionCount--;
            TestMutex.ReleaseMutex();

            if (SessionCount == 0)
            {
                // if needed do cleanup
            }
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
    }
}
