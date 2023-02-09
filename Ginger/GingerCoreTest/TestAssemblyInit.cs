#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common;
using Ginger;
using Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace GingerAutoPilotTest
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

            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();
            // Init Reporter
            Reporter.WorkSpaceReporter = new UnitTestWorkspaceReporter();


            ExtractTestResources();
        }

        private static void ExtractTestResources()
        {            
            string PBAppZip = TestResources.GetTestResourcesFile("PBTestApp.zip");
            string targetFolder = PBAppZip.Replace("PBTestApp.zip", "");

            string appFolder = Path.Combine(targetFolder , "PBTestApp");
            if (Directory.Exists(appFolder))
            {
                Directory.Delete(appFolder, true);
            }
            
            ZipFile.ExtractToDirectory(PBAppZip, targetFolder);           

        }
    }
}
