using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace GingerUtils.OSLib
{
    class LinuxOS : IOperationgSystem
    {
        public string UserAgent => "Mozilla/5.0 (X11; od-database-crawler) Gecko/20100101 Firefox/52.0";

        public Process Dotnet(string cmd)
        {
            return ShellHelper.Dotnet(cmd);
        }

        public string GetFirstLocalHostIPAddress()
        {
            List<UnicastIPAddressInformation> unicastIPAddressInformationList = GetIPAddressCollectionList().ToList();

            string network = unicastIPAddressInformationList.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                                                        ?.Address
                                                                        ?.ToString()
                                                                        ?? string.Empty;
            return network;
        }

        public UnicastIPAddressInformationCollection GetIPAddressCollectionList()
        {
            NetworkInterface networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(ni =>
                    ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                    && ni.OperationalStatus == OperationalStatus.Up
                    && ni.GetIPProperties().GatewayAddresses.FirstOrDefault() != null
                    && ni.GetIPProperties().UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork) != null);

            IPInterfaceProperties ipInterfaceProperties = networkInterface?.GetIPProperties();

            UnicastIPAddressInformationCollection ipAddressCollection = ipInterfaceProperties.UnicastAddresses;

            return ipAddressCollection;
        }

     

    }
}
