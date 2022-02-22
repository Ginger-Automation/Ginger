//#region License
///*
//Copyright © 2014-2022 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using GingerWPFDriverWindow;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Diagnostics;
//using System.Reflection;
//using System.Threading;
//using System.Windows.Threading;


//namespace GingerWPFDriverWindowTest
//{
//    [TestClass]
//    public class GingerWPFDriverWindowTest
//    {





//        public static Mutex mutex = new Mutex();

//        static GingerWPFDriverWindow.MainWindow MainWindow;

//        public static Dispatcher mDispatcher;
//        static void StartDriverDisplayWPF()
//        {
//            mutex.WaitOne();

//            // We start GingerWPFDriverWindow on STA thread
//            var t = new Thread(() =>
//            {
//                MainWindow = new MainWindow();
//                MainWindow.Show();
//                mDispatcher = MainWindow.Dispatcher;

//                // Makes the thread support message pumping                 
//                System.Windows.Threading.Dispatcher.Run();
//            });


//            // Configure the thread to be STA
//            t.SetApartmentState(ApartmentState.STA);
//            t.Start();

//            while (mDispatcher == null)
//            {
//                Thread.Sleep(100);
//            }

//            Thread.Sleep(100);

//            // Here GingerWPFDriverWindow is live and visible
//        }


//        [ClassInitialize]
//        public static void ClassInit(TestContext TC)
//        {
//            StartDriverDisplayWPF();
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {

//        }

      


//        [TestMethod]  [Timeout(60000)]
//        public void TestMethod1()
//        {
            
            
//            RemoteObjectsClient c = new RemoteObjectsClient();            
//            c.Connect(SocketHelper.GetDisplayHost(), SocketHelper.GetDisplayPort());            

//            //TODO: rmove hard coded !!!!!!!!!!!!!!!!!!!!!!!!!!
//            Assembly driverAssembly = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerWebServicesPlugin\bin\Debug\netstandard2.0\WebServices.GingerPlugin.dll");
//            Type t = driverAssembly.GetType("Amdocs.Ginger.WebServices.IWebServicesDriverDisplay");

//            // We do all using reflection, since we don't have ref to the driver dll, it will load at run time

//            //IWebServicesDriverDisplay webServicesDriverDisplay = c.GetObject<IWebServicesDriverDisplay>("ID aas as !!!");
//            MethodInfo mi = typeof(RemoteObjectsClient).GetMethod("GetObject").MakeGenericMethod(new Type[] { t });
//            object webServicesDriverDisplay = mi.Invoke(c, new object[] { "ID aas as !!!"});

//            // WebServicesDriver webServicesDriver = new WebServicesDriver();
//            object webServicesDriver = driverAssembly.CreateInstance("GingerWebServicesPlugin.WebServicesDriver");

//            // webServicesDriver.AttachDisplay((IWebServicesDriverDisplay)webServicesDriverDisplay);
//            webServicesDriver.GetType().GetMethod("AttachDisplay").Invoke(webServicesDriver, new object[] { webServicesDriverDisplay });


//            webServicesDriverDisplay.GetType().GetMethod("SetURL").Invoke(webServicesDriverDisplay, new object[] { "http://www.google.com" });

//            Stopwatch st = Stopwatch.StartNew();
//            for (int i = 0; i < 1000; i++)
//            {
//                webServicesDriverDisplay.GetType().GetMethod("SetURL").Invoke(webServicesDriverDisplay, new object[] { "#" + i});
//                Thread.Sleep(1);
//            }
//            st.Stop();
//        }
//    }
//}
