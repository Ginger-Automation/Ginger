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

using amdocs.ginger.GingerCoreNET;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace GingerCoreNETUnitTest
{
    [TestClass]
    public class TestAssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Called once when the test assembly is loaded
            // We provide the assembly to GingerTestHelper.TestResources so it can locate the 'TestResources' folder path
            TestResources.Assembly = Assembly.GetExecutingAssembly();

            InitWS();
        }


        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Called once when the test assembly is done
            // WorkSpace.Instance.Close();
        }


        static void InitWS()
        {
            // TODO: check if ws is null

            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
        }

    }
}
