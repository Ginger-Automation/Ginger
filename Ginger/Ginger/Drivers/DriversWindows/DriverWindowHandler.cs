using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
                    Thread staThread = new Thread(new ThreadStart(() =>
                    {
                        if (args.Driver is IDriverWindow)//TODO: think if better to do it with reflection using DriverWindowPath
                        {
                            try
                            {
                                DriverBase driver = args.Driver;
                                Agent agent = (Agent)args.DataObject;                                
                                string classname = "Ginger.Drivers.DriversWindows." + ((IDriverWindow)driver).GetDriverWindowName(agent.DriverType);
                                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                                if (t == null)
                                {
                                    throw new Exception(string.Format("The Driver Window was not found '{0}'", classname));
                                }
                                Window window = (Window)Activator.CreateInstance(t, driver, agent);
                                if (window != null)
                                {
                                    mOpenWindowsDic.Add(args.Driver, window);
                                    window.Show();
                                    System.Windows.Threading.Dispatcher.Run();
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, string.Format("Error occurred while loading/showing Agent windows, error: '{0}'", ex.Message));
                                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while loading/showing Agent windows", ex);
                            }
                        }
                    }));
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.IsBackground = true;
                    staThread.Start();
                    break;

                case DriverWindowEventArgs.eEventType.CloseDriverWindow:
                    if (mOpenWindowsDic.ContainsKey(args.Driver))
                    {
                        try
                        {
                            //mOpenWindowsDic[args.Driver].Close();
                            mOpenWindowsDic.Remove(args.Driver);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.WARN, "Exception while trying to close Driver Window", ex);
                        }
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
