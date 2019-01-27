using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace GingerUtils
{
    public class Network
    {

        public static string GetFirstLocalHostIPAddress()
        {
            string networkIP = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                // use the Linux
                // networkIP = GetFirstLocalHostIPAddress_Windows();
                networkIP = GetFirstLocalHostIPAddress_Linux();
            }
            else if (OperatingSystem.IsLinux())
            {
                networkIP = GetFirstLocalHostIPAddress_Linux();
            }
            return networkIP;
        }


        public static string GetFirstLocalHostIPAddress_Linux()
        {
            List<UnicastIPAddressInformation> unicastIPAddressInformationList = GetIPAddressCollectionList().ToList();

            string network = unicastIPAddressInformationList.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                                                        ?.Address
                                                                        ?.ToString()
                                                                        ?? string.Empty;
            return network;
        }

        public static List<string> GetLocalHostIPAddressesList()
        {
            List<UnicastIPAddressInformation> unicastIPAddressInformationList = GetIPAddressCollectionList().ToList();

            List<string> networkList = new List<string>();

            foreach (UnicastIPAddressInformation unicastIPAddressInformation in unicastIPAddressInformationList)
            {
                string network = unicastIPAddressInformation.Address.ToString();

                if (!string.IsNullOrEmpty(network))
                {
                    networkList.Add(network);
                }
            }

            return networkList;

        }

        public static UnicastIPAddressInformationCollection GetIPAddressCollectionList()
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


        public static string GetFirstLocalHostIPAddress_Windows()
        {
            string LocalHostIP = string.Empty;
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> IPList = ipEntry.AddressList.ToList();
            Console.WriteLine("Number of IP Addresses Found: " + IPList.Count);

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
                    i++;
                    Console.WriteLine("IP Address [" + i + "] : " + ip.ToString() + " " + ip.AddressFamily.ToString() + " " + ip);

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
