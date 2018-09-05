#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerWPFUnitTest.POMs;
using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPFUnitTest
{

    public class GingerAutomator
    {
        // Enable to run only one Ginger for all tests and one test at a time
        public static Mutex TestMutex = new Mutex();

        static Ginger.App app;
        public MainWindowPOM MainWindowPOM;
        static bool isReady = false;
        Thread t = null;

        static GingerAutomator gingerAutomatorInstance;
        public static GingerAutomator Instance
        {
            get
            {
                if (app == null)
                {
                    gingerAutomatorInstance = new GingerAutomator();
                    gingerAutomatorInstance.StartGinger();                    
                }
                while (!isReady)
                {
                    Thread.Sleep(100);
                }
                return gingerAutomatorInstance;
            }
        }

        

        private void StartGinger()
        {            
            Ginger.SplashWindow splash = null;
            // We start Ginger on STA thread
            t = new Thread(() =>
            {
                // we need sample class - Dummy
                Ginger.GeneralLib.Dummy d = new Ginger.GeneralLib.Dummy();
                Assembly asm1 = d.GetType().Assembly;
                // Set the app resources to Ginger so image an other will be locally to Ginger
                Application.ResourceAssembly = asm1;

                app = new Ginger.App();
                Ginger.App.RunningFromUnitTest = true;
                splash = new Ginger.SplashWindow();
                splash.Show();
                //Ginger.App.UserProfile.AutoLoadLastSolution = false;                

                while (!app.IsReady && splash.IsVisible)
                {
                    Thread.Sleep(100);
                }

                GingerPOMBase.Dispatcher = app.GetMainWindowDispatcher();
                MainWindowPOM = new MainWindowPOM(Ginger.App.MainWindow);
                
                // Makes the thread support message pumping                 
                System.Windows.Threading.Dispatcher.Run();                
            });


            //// Configure the thread
            t.SetApartmentState(ApartmentState.STA);            
            t.Start();

            //max 60 seconds for Mainwindow to be ready
            int i = 0;
            while (MainWindowPOM == null && i <600)
            {
                Thread.Sleep(100);
                i++;
            }


            while (splash.IsVisible)
            {
                Thread.Sleep(100);
            }
            // Here Ginger is live and visible
            isReady = true;
        }

        
        internal void CloseGinger()
        {            
            MainWindowPOM.Dispatcher.Invoke(() => {                
                MainWindowPOM.Close();                
                Thread.Sleep(1000);
                int i = 0;
                while (app.Windows.Count > 0 && i < 100) //max 10 seconds for closing all windows
                {
                    i++;
                    Thread.Sleep(100);
                }                                
            });            
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

            //TODO: we need to diable mouse event as elemnt on screen can look different
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
                Ginger.App.SetSolution(folder);                
            });
        }

        internal void CloseSolution()
        {
            GingerPOMBase.Dispatcher.Invoke(() =>
            {
                // TODO: do it like user with open solution page
                Ginger.App.CloseSolution();

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


    }
}
