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

using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Drivers.CommunicationProtocol
{

    [TestClass]
    [Level1]
    public class RemoteObjectsClientServerTest
    {
        public interface IDisplay
        {
            int Add(int a, int b);
        }


        [Ignore]  // fail fix me
        [TestMethod]
        [Timeout(60000)]
        public void RemoteObjectProxyTest1()
        {
            bool IsReady = false;
            Task t = new Task(() =>
            {
                RemoteObjectsServer s = new RemoteObjectsServer();
                s.Start(15111);  // TODO: get free port
                s.GetObjectHandler = GetObjectHandler;
                IsReady = true;
            });
            t.Start();

            Stopwatch st = Stopwatch.StartNew();
            while (!IsReady && st.ElapsedMilliseconds < 10000)
            {
                Thread.Sleep(50);
            }


            RemoteObjectsClient c = new RemoteObjectsClient();
            //TODO: temp get local host
            c.Connect(SocketHelper.GetDisplayHost(), SocketHelper.GetDisplayPort());


            IDisplay calc1 = c.GetObject<IDisplay>("aa1");

            int total = calc1.Add(2, 5);

            Assert.AreEqual(7, total);
        }



        // --------------------------------------------------------------------------------
        // Calculator implemented on server side
        // --------------------------------------------------------------------------------
        public class Calculator : IDisplay
        {

            public int Add(int a, int b)
            {
                return a + b;
            }
        }


        private object GetObjectHandler(string arg)
        {
            return new Calculator();
        }
    }
}
