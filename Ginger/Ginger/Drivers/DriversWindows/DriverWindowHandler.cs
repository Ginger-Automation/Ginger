using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.Drivers.DriversWindows
{
    public class DriverWindowHandler
    {
        static Dictionary<DriverBase, Window> mOpenWindowsDic = new Dictionary<DriverBase, Window>();

        public static void Init()
        {
            DriversWindowUtils.DriverWindowEvent += DriverWindowUtils_DriverWindowEvent;
        }

        private static void DriverWindowUtils_DriverWindowEvent(DriverWindowEventArgs args)
        {
            switch (args.EventType)
            {
                case DriverWindowEventArgs.eEventType.ShowDriverWindow:                    
                    //Thread staThread = new Thread(()=>
                    //{
                        if (args.Driver is IDriverWindow)//TODO: think if better to do in reflection using DriverWindowPath
                        {
                            MobileDriverWindow mobileDriverWindow = new MobileDriverWindow((IMobileDriverWindow)args.Driver, (Agent)args.DataObject);
                            mOpenWindowsDic.Add(args.Driver, mobileDriverWindow);
                            mobileDriverWindow.Show();
                        }
                    //});
                    //staThread.SetApartmentState(ApartmentState.STA);
                    //staThread.IsBackground = true;
                    //staThread.Start();
                    //System.Windows.Threading.Dispatcher.Run();
                    break;

                case DriverWindowEventArgs.eEventType.CloseDriverWindow:
                    if (mOpenWindowsDic.ContainsKey(args.Driver))
                    {
                        mOpenWindowsDic[args.Driver].Close();
                    }
                    break;
            }
        }

        //public void CreateSTA(Action ThreadStartingPoint)
        //{
        //    STAThread = new Thread(new ThreadStart(ThreadStartingPoint));
        //    STAThread.SetApartmentState(ApartmentState.STA);
        //    STAThread.IsBackground = true;
        //    STAThread.Start();
        //}
    }
}
