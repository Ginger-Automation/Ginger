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
                                AgentOperations agentOperations = (AgentOperations)args.DataObject;                                
                                string classname = "Ginger.Drivers.DriversWindows." + ((IDriverWindow)driver).GetDriverWindowName(agentOperations.Agent.DriverType);
                                Type t = Assembly.GetExecutingAssembly().GetType(classname);
                                if (t == null)
                                {
                                    throw new Exception(string.Format("The Driver Window was not found '{0}'", classname));
                                }
                                Window window = (Window)Activator.CreateInstance(t, driver, agentOperations.Agent);
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
