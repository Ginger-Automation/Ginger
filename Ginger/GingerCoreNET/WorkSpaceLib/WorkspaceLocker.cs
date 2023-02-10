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

//using amdocs.ginger.GingerCoreNET;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading;

//namespace amdocs.ginger.GingerCoreNET
//{
//    public class WorkspaceLocker 
//    {
//        static readonly object _locker = new object();
//        static string mWorkspaceHolder;
//        static readonly Mutex mMutex = new Mutex();

//        // Enable to run only one Ginger for all tests and one test at a time
//        private static Mutex TestMutex = new Mutex();

//        static int SessionCount = 0; // count how many sessions are waiting in queue

//        static WorkspaceLocker WorkspaceLockerInstance;  // currently we have only one Ginger running for all tests so use one workspace at time

//        public WorkspaceLocker(string name)
//        {
//            SessionCount++;
//            //TestMutex.WaitOne();
//            mWorkspaceHolder = name;
//            // Make sure we run one session at a time, wait for session to be free
//            //if (WorkspaceLockerInstance == null)
//            //{
//            //    WorkspaceLockerInstance = new WorkspaceLocker(name);
//            //}
//            //mWorkspaceHolder = name;
//        }

        


//        // TOD: remove !!!!!!!!!!!!!!!!!!!
//        //public static WorkspaceLocker StartSession(string name)
//        //{
//        //    SessionCount++;
//        //    TestMutex.WaitOne();  // Make sure we run one session at a time, wait for session to be free
//        //    if (WorkspaceLockerInstance == null)
//        //    {
//        //        WorkspaceLockerInstance = new WorkspaceLocker(name);
//        //    }
//        //    mWorkspaceHolder = name;
//        //    return WorkspaceLockerInstance;
//        //}

//        public static string HoldBy
//        {
//            get
//            {
//                return mWorkspaceHolder;
//            }
//        }

//        private static void EndSession()
//        {
//            SessionCount--;
//            mWorkspaceHolder = null;
//            //TestMutex.ReleaseMutex();

//            if (SessionCount == 0)
//            {
//                // if needed do cleanup
//            }
//        }



        

//        public void ReleaseWorkspace()
//        {

//            //lock (_locker)
//            //{
//            try
//            {

//                WorkSpace.Instance.CloseSolution();
//                WorkSpace.Instance.LocalGingerGrid.Stop();

//                WorkSpace.Instance.Close();
//                mWorkspaceHolder = null;

//            }
//            catch (Exception ex)
//            {

//            }
//            finally
//            {

//                //    mMutex.ReleaseMutex();
//            }
//            //}
//            EndSession();
//        }

//        public void Dispose()
//        {
            
//        }
//    }
//}
