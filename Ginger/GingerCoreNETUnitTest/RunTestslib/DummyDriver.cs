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

using GingerPlugInsNET.ActionsLib;
using GingerPlugInsNET.DriversLib;
using GingerPlugInsNET.PlugInsLib;
using System;
using System.Collections.Generic;

namespace GingerCoreNETUnitTests.RunTestslib
{
    public class DummyDriver : PluginDriverBase
    {
        public override string Name { get { return "DummyDriver"; } }

        public override void CloseDriver()
        {
            Console.WriteLine("CloseDriver");
        }

        public override void StartDriver()
        {
            Console.WriteLine("StartDriver");
        }



        [GingerAction("A1", "A1 desc")]
        public void A1(GingerAction act)
        {
            Console.WriteLine("A1");
            act.ExInfo = "A1 Result";
        }


        [GingerAction("A2", "A2 desc")]
        public void A2(GingerAction act)
        {
            Console.WriteLine("A2");
            act.ExInfo = "A2 Result";
        }

        [GingerAction("Time", "Get current driver time")]
        public void Time(GingerAction act)
        {
            Console.WriteLine("Echo");
            act.Output.Values.Add(new ActionOutputValue() { Param = "CurrentTime", ValueString = DateTime.Now.ToString() });
            act.ExInfo = "Echo Result";
        }




    }
}
