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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Amdocs.Ginger.Common.Helpers;

namespace Amdocs.Ginger.Common.OS
{
    class WindowsOS : OperatingSystemBase
    {
        public string UserAgent => "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Mozilla / 5.0(X11; od - database - crawler) Gecko / 20100101 Firefox / 52.0 Gecko) Chrome/58.0.3029.110 Safari/537.36";


        public override string GetFirstLocalHostIPAddress()
        {
            string LocalHostIP = string.Empty;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> IPList = ipEntry.AddressList.ToList();

            if (IPList.Count() == 1)
            {
                // if we have only one return it
                LocalHostIP = IPList[0].ToString();
            }
            else if (IPList.Count() > 1)
            {                
                foreach (IPAddress ip in IPList)
                {                    
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        LocalHostIP = ip.ToString();
                    }
                }
            }

            return LocalHostIP;
        }

 
    }
}
