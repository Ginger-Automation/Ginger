using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GingerUtils.OSLib
{
    class WindowsOS : IOperationgSystem
    {
        public string UserAgent => "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Mozilla / 5.0(X11; od - database - crawler) Gecko / 20100101 Firefox / 52.0 Gecko) Chrome/58.0.3029.110 Safari/537.36";

        public Process Dotnet(string cmd)
        {
            return ShellHelper.Dotnet(cmd);
        }

        public string GetFirstLocalHostIPAddress()
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
                int i = 0;
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
