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

using Amdocs.Ginger.Plugin.Core;
using System;
using System.Collections.Generic;

namespace GingerCoreNETUnitTests.RunTestslib
{
    [GingerService(Id : "DummyService", Description: "Dummy Service")]
    public class DummyDriver : IServiceSession
    {
        public void StartSession()
        {
            Console.WriteLine("Dummy Driver Session Started");
        }

        public void StopSession()
        {
            Console.WriteLine("Dummy Driver Session Ended");
        }


        [GingerAction("A1", "A1 desc")]
        public void A1(IGingerAction act)
        {
            Console.WriteLine("A1");
            act.AddExInfo("A1 Result");
        }


        [GingerAction("A2", "A2 desc")]
        public void A2(GingerAction act)
        {
            Console.WriteLine("A2");
            act.AddExInfo("A2 Result");
        }

        

        [GingerAction("Echo", "Echo string as output")]
        public void Echo(GingerAction act, string text)
        {
            Console.WriteLine("Echo");
            act.AddOutput("echo", text);
            act.AddExInfo("Echo - " + text);
        }




    }
}
