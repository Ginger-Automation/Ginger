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

using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using GingerCoreNET.RosLynLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTests.ValueExpressionTest
{
    [TestClass]
    public class ValueExpressionRefrenceTest
    {
        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }


        [TestMethod]
        [Timeout(60000)]
        public void SerializeTheFile()
        {
            VERefrenceList VEL = new VERefrenceList();

            ValueExpressionReference VER = new ValueExpressionReference();
            VER.Category = "math";
            VER.Description = "abc";
            ValueExpressionReference VER2 = new ValueExpressionReference();
            VER2.Category = "math";
            VER2.Description = "eee";
            ValueExpressionReference VER3 = new ValueExpressionReference();
            VER3.Category = "math";
            VER3.Description = "ewee";
            VEL.Refrences.Add(VER);
            VEL.Refrences.Add(VER2);


            VEL.SavetoJson(@"C:\Users\mohdkhan\Desktop\VEL.json");
        }
        [TestMethod]
        [Timeout(60000)]
        public void LoadOldFile()
        {
            VERefrenceList VEL = VERefrenceList.LoadFromJson(@"C:\Users\mohdkhan\Desktop\VEL.json"); ;




        }

    }
}
