using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
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


        // TODO: try to work without it and make send email work on Linux 
        public void InitSmtpAuthenticationManager()
        {
            // Mail server support GSSAPI, NTLM and LOGIN, GSSAPI and NTLM can't work well on Linux, so we remove them            
            var smtpAuthenticationManager = typeof(SmtpClient).Assembly.GetType("System.Net.Mail.SmtpAuthenticationManager", false);
            var modules = (System.Collections.IList)smtpAuthenticationManager
                .GetTypeInfo()
                .GetField("s_modules", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(smtpAuthenticationManager);
            List<object> removeModules = new List<object>();
            for (int i = 0; i < modules.Count; i++)
            {
                var fullName = modules[i].GetType().FullName;
                if (string.Equals(fullName, "System.Net.Mail.SmtpNegotiateAuthenticationModule", StringComparison.OrdinalIgnoreCase))
                {
                    removeModules.Add(modules[i]);
                }
                else if (string.Equals(fullName, "System.Net.Mail.SmtpNtlmAuthenticationModule", StringComparison.OrdinalIgnoreCase))
                {
                    removeModules.Add(modules[i]);
                }
                //else if (string.Equals(fullName, "System.Net.Mail.SmtpLoginAuthenticationModule", StringComparison.OrdinalIgnoreCase))
                //{
                //    removeModules.Add(modules[i]);
                //}
            }
            removeModules.ForEach(m => modules.Remove(m));

        }
    }
}
