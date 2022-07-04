#region License
/*
Copyright © 2014-2022 European Support Limited

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
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

namespace GingerCore.Drivers.Selenium.SeleniumBMP
{
    public class Server
    {
        private Process _serverProcess;
        private readonly int _port;
        private readonly String _path = string.Empty;
        private const string Host = "localhost";

        public Server(string path) : this(path, 8080)
        {}

        public Server(string path, int port)
        {
            _path = path;
            _port = port;
        }

        public void Start()
        {
            _serverProcess = Process.Start(new ProcessStartInfo() { FileName = _path, UseShellExecute = true });

            if (_port != 0)
            {
                _serverProcess.StartInfo.Arguments = String.Format("--port={0}", _port);
            }
            
            try
            {
                _serverProcess.Start();
                int count = 0;
                while (!IsListening())
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count == 30)
                    {
                        throw new Exception("Can not connect to BrowserMob Proxy");
                    }
                }
            }
            catch
            {
                _serverProcess.Dispose();
                _serverProcess = null;
                throw;
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {            
            if (_serverProcess != null && !_serverProcess.HasExited)
            {
                _serverProcess.CloseMainWindow();
                _serverProcess.Dispose();
                _serverProcess = null;
            }            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Client CreateProxy(){
            return new Client(Url);
        }

        /// <summary>
        /// 
        /// </summary>
        public string Url
        {
            get { return String.Format("http://{0}:{1}", Host, _port.ToString(CultureInfo.InvariantCulture)); }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsListening()
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(Host, _port);
                socket.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
