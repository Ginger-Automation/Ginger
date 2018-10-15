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
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Reflection;
using Amdocs.Ginger.Common;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    class UnixServerSimulator
    {
        public void Start()
        {
            Task t = Task.Factory.StartNew(() =>
            {
                StartServer();
            });
        }

        private void StartServer()
        {
            IPAddress HOST = IPAddress.Parse("127.0.0.1");
            IPEndPoint serverEP = new IPEndPoint(HOST, 4433);
            Socket sck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sck.Bind(serverEP);
            sck.Listen(443);

            try
            {
                Console.WriteLine("Listening for clients...");
                Socket msg = sck.Accept();

                while (true)
                {
                    // Send a welcome greet
                    byte[] buffer = Encoding.Default.GetBytes("Welcome to Ginger Unix Server Simulator ");
                    msg.Send(buffer, 0, buffer.Length, 0);
                    buffer = new byte[255];

                    // Read the sended command
                    int rec = msg.Receive(buffer, 0, buffer.Length, 0);
                    byte[] bufferReaction = Encoding.Default.GetBytes(rec.ToString());

                    // Run the command
                    Process prcsCMD = new Process();
                    prcsCMD.StartInfo.FileName = bufferReaction.ToString();
                    prcsCMD.StartInfo.UseShellExecute = false;
                    prcsCMD.StartInfo.Arguments = string.Empty;
                    prcsCMD.StartInfo.RedirectStandardOutput = true;
                    prcsCMD.Start();

                    string output = prcsCMD.StandardOutput.ReadToEnd();
                    byte[] cmdOutput = Encoding.Default.GetBytes(output);
                    msg.Send(cmdOutput,0,cmdOutput.Length,0);
                    cmdOutput = new byte[255];
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
     }
}