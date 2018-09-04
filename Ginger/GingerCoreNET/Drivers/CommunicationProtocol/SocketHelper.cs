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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol
{
    public class SocketHelper
    {
        public static string GetLocalHostIP()
        {
            IPHostEntry ii = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> IPList = (from x in ii.AddressList where x.AddressFamily == AddressFamily.InterNetwork select x).ToList();
            if (IPList.Count() == 1)
            {
                // if we have only one return it
                return IPList[0].ToString(); ;
            }

            if (IPList.Count() > 1)
            {
                // prefer local router/hub first usually 192, we will add GG config later to select
                foreach (IPAddress ip in IPList)
                {
                    if (ip.ToString().StartsWith("192"))
                    {
                        return ip.ToString();
                    }
                }

                // return the first in the list
                return IPList[0].ToString(); 
            }

            //TODO: throw
            return null; 
        }

        //TODO: think if we want to have multiple display - enable set for this value
        public static string GetDisplayHost()
        {
            // when we want to connect to display on another machine... modify.
            return GetLocalHostIP();
        }

        public static int GetDisplayPort()
        {
            return 15111;
        }

        //Verify we provide one port at a time, for unit test this function can be called in parallel
        static Semaphore semaphore = new Semaphore(3, 10);  // Enable 3 port allocator at a time with max up to 10        
        static int LastPort = 15000;  // Ginger Grid and nodes will be on 15000+ ports - this area is mainly free to find ports
        public static int GetOpenPort()
        {
            semaphore.WaitOne(); // control the reentry if several threads request at the same time                        
            int count = 999;         // We scan range of 999 ports
            int timeout = 0;
            int unusedPort = 0;
            bool bFound = false;
            while (!bFound)
            {
                Random rand = new Random();
                int toSkip = rand.Next(1, 10);
                int portStartIndex = LastPort + toSkip;
                if (LastPort > 16000)
                {
                    LastPort = 15000;
                }
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();
                List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
                unusedPort = Enumerable.Range(portStartIndex, count).Where(port => !usedPorts.Contains(port)).FirstOrDefault();
                if (unusedPort != LastPort)
                {
                    LastPort = unusedPort;
                    bFound = true;
                }
                else
                {
                    Thread.Sleep(100);
                    timeout++;
                    if (timeout > 30)
                    {
                        throw new Exception("Unable to GetOpenPort for mote than 3 seconds");
                    }
                }
            }
            semaphore.Release();
            return unusedPort;
        }
    }
}
