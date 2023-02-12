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

using amdocs.ginger.GingerCoreNET;
using Ginger;
using Ginger.ReporterLib;
using Ginger.Repository;
using GingerWPF.WorkSpaceLib;
using GingerWPFUnitTest.POMs;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GingerTest
{

    public class GingerAutomator
    {
        // Enable to run only one Ginger for all tests and one test at a time
        private static Mutex TestMutex = new Mutex();

        static Ginger.App app;
        public MainWindowPOM MainWindowPOM;
        static bool isReady = false;
        static Thread mGingerThread = null;

        static GingerAutomator gingerAutomatorInstance;  // currently we have only one Ginger running for all tests
        static int SessionCount = 0; // count how many sessions are waiting in queue


        // Set to true if you want highlights when automation run and speak
        public static bool Highlight { get { return false; }  }

        public static GingerAutomator StartSession()
        {
            SessionCount++;
            TestMutex.WaitOne();  // Make sure we run one session at a time, wait for session to be free
            if (app == null)
            {
                gingerAutomatorInstance = new GingerAutomator();
                gingerAutomatorInstance.StartGinger();
                while (!isReady)
                {
                    Thread.Sleep(100);
                }

            }
            return gingerAutomatorInstance;
        }

        public static void EndSession()
        {         
            SessionCount--;
            TestMutex.ReleaseMutex();

            if (SessionCount == 0)
            {
                gingerAutomatorInstance.CloseGinger();                
            }            
        }
        

        private void StartGinger()
        {                        
            // We start Ginger on STA thread
            mGingerThread = new Thread(() =>
            {
                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));

                // we need sample class - Dummy
                Ginger.GeneralLib.Dummy d = new Ginger.GeneralLib.Dummy();
                Assembly asm1 = d.GetType().Assembly;

                // Set the app resources to Ginger so images and other resources will be locally avaiilable to Ginger
                var applicationType = typeof(Application);
                var resourceField = applicationType.GetField("_resourceAssembly", BindingFlags.NonPublic | BindingFlags.Static);
                resourceField.SetValue(null, asm1);

                app = new Ginger.App();
                WorkSpace.Init(new WorkSpaceEventHandler());
                WorkSpace.Instance.RunningFromUnitTest = true;
                WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new DotNetFrameworkHelper());
                

                app.HideConsoleWindow();
                app.StartGingerUI();

                GingerPOMBase.Dispatcher = app.GetMainWindowDispatcher();

                //Ginger.App.MainWindow.Closed += (sender1, e1) => 
                //    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);

                MainWindowPOM = new MainWindowPOM(Ginger.App.MainWindow);

                // Makes the thread support message pumping                 
                System.Windows.Threading.Dispatcher.Run();
            });


            //// Configure the thread            
            mGingerThread.SetApartmentState(ApartmentState.STA);
            mGingerThread.IsBackground = true;
            mGingerThread.Start();

            //max 60 seconds for Main window to be ready
            int i = 0;
            while (MainWindowPOM == null && i <600)
            {
                Thread.Sleep(100);
                i++;
            }

            
            // Here Ginger is live and visible
            isReady = true;
        }

        

        void CloseGinger()
        {
            // app.Shutdown(0);
            MainWindowPOM.Close();
            Thread.Sleep(5000);            
        }



        public void RunPageInTestWindow(Page p, Action a)
        {

            //var t = new Thread(() =>
            //{

            //    mDispatcher.Invoke(() => 
            //{


            //TestWindow TW = null;
            //TW = new TestWindow(p);
            //TW.Show();

            //TODO: we need to disable mouse event as element on screen can look different
            // or we need to make sure it is not in focus by loading another dummy window and focus

            //while (!TW.IsLoaded)
            //{
            //    SleepWithDoEvents(100);
            //}
            //SleepWithDoEvents(1000);


            //p1.ShowIcons(51, 100);
            //p1.StopSpinners();
            //mGingerWPFAutomator.SleepWithDoEvents(500);



          //  mDispatcher.Invoke(() =>
          //{
          //            // a.Invoke();
          //            //bool IsEquel2 = false;
          //            //Page1 p1 = new Page1();
          //            //p1.ShowIcons(51, 100);
          //            //p1.StopSpinners();
          //            TestWindow TW = new TestWindow(p);
          //    TW.Show();
              // SleepWithDoEvents(2000);
                      // IsEquel2 = General.IsWindowBitmapEquel(p1, "ImageMakerControlsVisualTest51_100");

                      //});


                      // TW.Close();
               //   });
            //});

        }

        internal void OpenSolution(string folder)
        {
            MainWindowPOM.ClickSolutionTab();
            GingerPOMBase.Dispatcher.Invoke(() =>
            {
                // TODO: do it like user with open solution page
                WorkSpace.Instance.OpenSolution(folder);                
            });
        }

        internal void CloseSolution()
        {
            GingerPOMBase.Dispatcher.Invoke(() =>
            {
                // TODO: do it like user with open solution page
                WorkSpace.Instance.CloseSolution();
            });

        }

        public void CreateSolution(string path)
        {
            // TODO: fix me to create real solution like user
            // SolutionRepository SR = new SolutionRepository();            
            // SR = Ginger.App.CreateGingerSolutionRepository();
            // Ginger.App.CreateGingerSolutionRepository()
            OpenSolution(path);

        }

        internal void ReloadSolution()
        {
            string path =  WorkSpace.Instance.Solution.ContainingFolderFullPath;
            CloseSolution();
            OpenSolution(path);
        }
    }
}
