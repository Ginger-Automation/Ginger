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
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;

namespace Amdocs.Ginger.Common.OS
{
    class LinuxOS : OperatingSystemBase
    {
        public string UserAgent => "Mozilla/5.0 (X11; od-database-crawler) Gecko/20100101 Firefox/52.0";

    
        public override string GetFirstLocalHostIPAddress()
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
        public override void InitSmtpAuthenticationManager()
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
        public override string AdjustFilePath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return path.Replace("\\", "/");
            }
            else
            {
                return path;
            }
        }
        public override string AdjustOSChars(String content) 
        {
            if (!string.IsNullOrEmpty(content))
            {
                return content.Replace("\r\n", System.Environment.NewLine);
            }
            else
            {
                return content;
            }
        }
    }
} 
