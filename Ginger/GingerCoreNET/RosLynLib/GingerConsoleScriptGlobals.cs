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

using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCoreNET.Drivers;
using GingerCoreNET.DriversLib;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RosLynLib
{
    // This is Roslyn global file which is passed to the script code
    // DO NOT put here general code unless it is intended to be used from script
    // DO NOT change functions signature once published as we might have script which use them, or make sure it is backward compatible

    public class GingerConsoleScriptGlobals
    {
        //TODO: we can put global vars too
        // TODO: Just example delete me later
        public int X = 1;
        public int Y;

        PluginPackage P;

        public void LoadPluginPackage(string folder)
        {
            Console.WriteLine("* Loading Plugin - " + folder);
            P = new PluginPackage(folder);            
            Console.WriteLine("* Plugin Loaded");
        }

        List<GingerNode> GingerNodes = new List<GingerNode>();

        public void StartNode(string Name, string ID, string GingerGridHost = "127.0.0.1", int GingerGridPort = 15001)
        {
            //Console.WriteLine("* StartNode - " + Name);

            //bNodest = true;
            //bool bReady = false;
            ////start node on task as we might have several running in parallel + we want the script to continue
            //Task t = new Task(() =>
            //{
            //    PluginDriverBase d = P.GetDriver(Name);
            //    GingerNode GN = StartDriverOnGingerNode(d, ID, GingerGridHost, GingerGridPort);
            //    GN.GingerNodeMessage += GingerNodeMessage;
            //    GingerNodes.Add(GN);
            //    bReady = true;
            //});
            //t.Start();
            //while (!bReady) // TODO: add timeout or use ManualResetEvent with timeout
            //{
            //    Console.WriteLine("Waiting for Node to be ready");
            //    Thread.Sleep(500);
            //}
        }

        private void GingerNodeMessage(GingerNode gingerNode, GingerNode.eGingerNodeEventType GingerNodeEventType)
        {
            switch (GingerNodeEventType)
            {
                case GingerNode.eGingerNodeEventType.Started:
                    break;
                case GingerNode.eGingerNodeEventType.Shutdown:
                    Console.WriteLine("Handling shutdown");
                    GingerNodes.Remove(gingerNode);
                    if (GingerNodes.Count == 0)
                    {
                        // if this is the last node allow shutdown.
                        mAllNodesAreDown.Set();
                        bNodest = false;
                    }
                    break;
            }
        }

        bool bNodest = false;
        ManualResetEvent mAllNodesAreDown = new ManualResetEvent(false);
        public void WaitforallNodesShutDown()
        {
            if (!bNodest) return;
            Console.WriteLine("Verifying/Waiting for all Nodes Shutdown");
            mAllNodesAreDown.WaitOne();
            Console.WriteLine("All Nodes are down , exit");
        }

        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        // ================================================================

        //private GingerNode StartDriverOnGingerNode(PluginDriverBase driver, string NodeName, string GingerGridHost = "127.0.0.1", int GingerGridPort = 15001)
        //{
        //    Console.WriteLine("Driver Name: " + driver.Name);
        //    DriverCapabilities DC = new DriverCapabilities();

        //    //TODO: add info about the driver and machine
        //    DC.Platform = "Web";  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //    DC.OS = "Win 7";  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //    GingerNode GN = new GingerNode(DC, driver);
        //    GN.StartGingerNode(NodeName, GingerGridHost, GingerGridPort);

        //    Console.WriteLine("Ginger Node started - " + NodeName);  // Add ports and GingerGrid details
        //    return GN;
        //}

        //public void StartService(string Name, string ID, string GingerGridHost = "127.0.0.1", int GingerGridPort = 15001)
        //{
        //    IGingerService d = P.GetService(Name);
        //    StartServiceOnGingerNode(d, ID, GingerGridHost, GingerGridPort);
        //}

        //private void StartServiceOnGingerNode(IGingerService service, string NodeName, string GingerGridHost = "127.0.0.1", int GingerGridPort = 15001)
        //{
        //    Console.WriteLine("Service Name: " + service );
        //    bool bReady = false;
        //    bNodest = true;
        //    Task t = new Task(() =>
        //    {
        //        GingerNode GN = new GingerNode(service);
        //        GN.StartGingerNode(NodeName, GingerGridHost, GingerGridPort);
        //        GN.GingerNodeMessage += GingerNodeMessage;
        //        GingerNodes.Add(GN);
        //        bReady = true;
        //    });

        //    t.Start();
        //    while (!bReady) // TODO: add timeout or use ManualResetEvent with timeout
        //    {
        //        Console.WriteLine("Waiting for service Node to be ready");
        //        Thread.Sleep(500);
        //    }
        //    Console.WriteLine("Ginger Service started - " + NodeName);  // Add ports and GingerGrid details
        //}
    }
}
