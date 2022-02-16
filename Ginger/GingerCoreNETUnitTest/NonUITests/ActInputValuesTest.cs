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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [Level2]
    [TestClass]
    public class ActInputValuesTest
    {

        static ActDummy mAct;

        public enum eSampleEnum
        {
            [EnumValueDescription("Yes value")]
            Yes,
            [EnumValueDescription("No Value")]
            No,
            [EnumValueDescription("Maybe Value")]
            Maybe
        }


        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {            
            mAct = new ActDummy();
        }

        [TestMethod]  [Timeout(60000)]
        public void GetInputParamEnumValueTest()
        {
            mAct.AddOrUpdateInputParamValue("Enum Value", eSampleEnum.Yes.ToString());

            eSampleEnum enumValue =(eSampleEnum)mAct.GetInputParamValue<eSampleEnum>("Enum Value");
          
            Assert.AreEqual(eSampleEnum.Yes, enumValue);
        }
        
    }
}
