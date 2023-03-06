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
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Amdocs.Ginger.GingerRuntime
{
    public class GingerGridMenu
    {
        MenuItem StartGridMenuItem;
        MenuItem StopGridMenuItem;
        MenuItem NodeListMenuItem;
        MenuItem TestPluginMenuItem;

        public MenuItem GetMenu()
        {
            StartGridMenuItem = new MenuItem(ConsoleKey.D1, "Start Grid", () => StartGrid(), true);
            StopGridMenuItem = new MenuItem(ConsoleKey.D2, "Stop Grid", () => StopGrid(), false);
            NodeListMenuItem = new MenuItem(ConsoleKey.D3, "Node List", () => NodeList(), true);
            TestPluginMenuItem = new MenuItem(ConsoleKey.D4, "Test Plugin", () => TestPlugin(), true);

            MenuItem GingerGridMenu = new MenuItem(ConsoleKey.G, "Ginger Grid");
            //GingerGridMenu.SubItems.Add(StartGridMenuItem);
            //GingerGridMenu.SubItems.Add(StopGridMenuItem);
            GingerGridMenu.SubItems.Add(NodeListMenuItem);
            GingerGridMenu.SubItems.Add(TestPluginMenuItem);

            return GingerGridMenu;
        }

        private void TestPlugin()
        {
            Stopwatch st = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                GingerNodeProxy GNP = new GingerNodeProxy(WorkSpace.Instance.LocalGingerGrid.NodeList[0]);
                GNP.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
                NewPayLoad pl = GetPL();
                GNP.RunAction(pl);
            }
            st.Stop();
            Console.WriteLine("Elapsed: " + (long)st.ElapsedMilliseconds);
            Console.WriteLine("AVG: " + (long)st.ElapsedMilliseconds / 1000);
        }

        NewPayLoad GetPL()
        {
            //TODO: reuse from GingerRunner
            NewPayLoad PL = new NewPayLoad("RunAction");
            PL.AddValue("Sum");

            List<NewPayLoad> Params = new List<NewPayLoad>();
            
            NewPayLoad p1 = new NewPayLoad("P", "a", "1");               
            Params.Add(p1);

            NewPayLoad p2 = new NewPayLoad("P", "b", "2");               
            Params.Add(p2);

            PL.AddListPayLoad(Params);
            PL.ClosePackage();
            return PL;
        }

        void StartGrid()
        {
            Console.WriteLine("Starting Ginger Grid Hub");
            //WorkSpace.Instance.LocalGingerGrid = new GingerGrid(SocketHelper.GetOpenPort());
            WorkSpace.Instance.LocalGingerGrid.Start();
            Console.WriteLine("Ginger Grid started - " + WorkSpace.Instance.LocalGingerGrid.Status);
            Console.WriteLine("Port: " + WorkSpace.Instance.LocalGingerGrid.Port);
        }

        void StopGrid()
        {
            WorkSpace.Instance.LocalGingerGrid.Stop();
            Console.WriteLine("Ginger Grid stopped");
            // Loop until key press

            Console.WriteLine("Ginger Grid Hub List");
        }


        ServiceGridTracker serviceGridTracker;
        void NodeList()
        {
            Console.WriteLine("Ginger Grid Hub List");
            serviceGridTracker = new ServiceGridTracker(WorkSpace.Instance.LocalGingerGrid);

            // Wait for user to press any key
            Console.WriteLine("List will refresh automatically when list changed, Press any key to exit");
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(500);
            }
            Console.Read();
        }

        
      

    }
}
