#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common.Helpers;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;

namespace Amdocs.Ginger.Common.OS
{
    public abstract class OperatingSystemBase
    {
        string UserAgent { get; }
        public virtual Process Dotnet(string cmd)
        {
            return ShellHelper.Dotnet(cmd);
        }

        public abstract string GetFirstLocalHostIPAddress();

        public virtual void InitSmtpAuthenticationManager()
        {
            //not required on windows
        }
        public virtual string AdjustFilePath(string path)
        {
            //not required on windows
            return path;
        }
        public virtual string AdjustOSChars(String content)
        {
            //not required on windows
            return content;
        }

        static OperatingSystemBase mCurrentOperatingSystem = GetOperatingSystem();
        public static OperatingSystemBase CurrentOperatingSystem
        {
            get
            {
                return mCurrentOperatingSystem;
            }
        }

        private static OperatingSystemBase GetOperatingSystem()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsOS();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return new MacOS();
            }
            else
            {
                return new LinuxOS();
            }
        }

        public static string GetSystemProxy()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Get the default proxy settings for the machine
                IWebProxy systemProxy = WebRequest.GetSystemWebProxy();

                var proxy = systemProxy.GetProxy(new Uri("https://ginger.amdocs.com/"));

                return proxy?.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string httpProxy = Environment.GetEnvironmentVariable("HTTP_PROXY");

                if (!string.IsNullOrEmpty(httpProxy))
                {
                    return httpProxy;
                }

                // Retrieve HTTPS proxy settings
                string httpsProxy = Environment.GetEnvironmentVariable("HTTPS_PROXY");

                if (!string.IsNullOrEmpty(httpsProxy))
                {
                    return httpsProxy;
                }
            }
            return null;
        }

        //public override string ToString()
        //{
        //    return base.ToString();
        //}
    }
}
