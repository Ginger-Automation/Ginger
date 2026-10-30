#region License
/*
Copyright © 2014-2026 European Support Limited

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

using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests.NonUITests.JavaDriverResponseTimeoutTest
{
    [TestClass]
    [Level1]
    public class JavaDriverResponseTimeoutTest
    {
        [TestMethod]
        [Timeout(60000)]
        public void StartDriver_AgentAcceptsConnectionButNeverResponds_FailsInsteadOfHanging()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            using CancellationTokenSource stopServer = new CancellationTokenSource();
            Task server = Task.Run(async () =>
            {
                using TcpClient agentSide = await listener.AcceptTcpClientAsync(stopServer.Token);
                await Task.Delay(Timeout.Infinite, stopServer.Token);
            });

            JavaDriver driver = CreateDriver(((IPEndPoint)listener.LocalEndpoint).Port);
            try
            {
                Stopwatch startTime = Stopwatch.StartNew();
                driver.StartDriver();
                startTime.Stop();

                Assert.IsTrue(startTime.Elapsed < TimeSpan.FromSeconds(20), $"StartDriver took {startTime.Elapsed}");
                Assert.IsFalse(driver.IsRunning());
                StringAssert.Contains(driver.ErrorMessageFromDriver, "did not respond");
            }
            finally
            {
                stopServer.Cancel();
                listener.Stop();
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void StartDriver_AgentClosesConnectionDuringStart_FailsWithClearMessage()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            using CancellationTokenSource stopServer = new CancellationTokenSource();
            Task server = Task.Run(async () =>
            {
                using TcpClient agentSide = await listener.AcceptTcpClientAsync(stopServer.Token);
                await ReadPayLoadAsync(agentSide.GetStream(), stopServer.Token);
            });

            JavaDriver driver = CreateDriver(((IPEndPoint)listener.LocalEndpoint).Port);
            driver.CommunicationTimout = 30;
            try
            {
                Stopwatch startTime = Stopwatch.StartNew();
                driver.StartDriver();
                startTime.Stop();

                Assert.IsTrue(startTime.Elapsed < TimeSpan.FromSeconds(20), $"StartDriver took {startTime.Elapsed}");
                Assert.IsFalse(driver.IsRunning());
                StringAssert.Contains(driver.ErrorMessageFromDriver, "was closed while the agent was starting");
            }
            finally
            {
                stopServer.Cancel();
                listener.Stop();
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void Send_LateResponseToTimedOutRequest_IsSkippedByNextRequest()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            using CancellationTokenSource stopServer = new CancellationTokenSource();
            Task server = Task.Run(async () =>
            {
                using TcpClient agentSide = await listener.AcceptTcpClientAsync(stopServer.Token);
                NetworkStream stream = agentSide.GetStream();
                while (!stopServer.IsCancellationRequested)
                {
                    PayLoad request = await ReadPayLoadAsync(stream, stopServer.Token);
                    string reply = request.Name switch
                    {
                        "SlowRequest" => "late reply",
                        "NextRequest" => "next reply",
                        _ => "Done"
                    };
                    if (request.Name == "SlowRequest")
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2.5), stopServer.Token);
                    }
                    PayLoad response = new PayLoad("Response");
                    response.AddValue(reply);
                    response.ClosePackage();
                    byte[] bytes = response.GetPackage();
                    await stream.WriteAsync(bytes, stopServer.Token);
                }
            });

            JavaDriver driver = CreateDriver(((IPEndPoint)listener.LocalEndpoint).Port);
            try
            {
                driver.StartDriver();
                Assert.IsTrue(driver.IsRunning(), driver.ErrorMessageFromDriver);

                MethodInfo sendWithTimeout = typeof(JavaDriver).GetMethod("Send", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(PayLoad), typeof(TimeSpan)], null);
                PayLoad slowRequest = new PayLoad("SlowRequest");
                slowRequest.ClosePackage();
                PayLoad timedOutResponse = (PayLoad)sendWithTimeout.Invoke(driver, [slowRequest, TimeSpan.FromSeconds(1)]);
                Assert.IsTrue(timedOutResponse.IsErrorPayLoad());

                PayLoad nextRequest = new PayLoad("NextRequest");
                nextRequest.ClosePackage();
                PayLoad nextResponse = driver.Send(nextRequest);

                Assert.AreEqual("next reply", nextResponse.GetValueString());
            }
            finally
            {
                driver.CloseDriver();
                stopServer.Cancel();
                listener.Stop();
            }
        }

        private static JavaDriver CreateDriver(int port)
        {
            return new JavaDriver(null)
            {
                JavaAgentHost = "127.0.0.1",
                JavaAgentPort = port,
                CommunicationTimout = 1,
                CommandTimeout = 120,
                ImplicitWait = 30,
                DriverLoadWaitingTime = 10
            };
        }

        private static async Task<PayLoad> ReadPayLoadAsync(NetworkStream stream, CancellationToken token)
        {
            byte[] lengthBytes = new byte[4];
            await stream.ReadExactlyAsync(lengthBytes, token);
            int length = (lengthBytes[0] << 24) + (lengthBytes[1] << 16) + (lengthBytes[2] << 8) + lengthBytes[3];
            byte[] package = new byte[length + 4];
            Array.Copy(lengthBytes, package, 4);
            await stream.ReadExactlyAsync(package.AsMemory(4, length), token);
            return new PayLoad(package);
        }
    }
}
