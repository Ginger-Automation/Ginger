#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace GingerAutoPilotTest
{
    [TestClass]
    public class SourceControlTest
    {
        //private static BusinessFlow BF;
        private static Solution Solution;
        private static SourceControlBase SourceControl;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            Solution = new Solution();
            
            
        }

        [TestMethod]
        public void TestSVNSolution()
        {
            // Arrange
            SourceControl = new SVNSourceControl();
            SourceControl.SourceControlURL = "https://riouxsvn.com/repositories/";
            SourceControl.SourceControlUser = "Dummy";
            SourceControl.SourceControlPass = "DontKnow";

            //Act

            string error = string.Empty;
            bool res = false;

            res = SourceControl.TestConnection(ref error);

            //Assert
            Assert.AreEqual(res, true);

        }
    }
    }
