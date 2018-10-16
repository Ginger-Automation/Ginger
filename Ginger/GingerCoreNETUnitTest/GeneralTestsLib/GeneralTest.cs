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

using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GingerCoreNETUnitTests.GeneralTestsLib
{

    [TestClass]
    [Level1]
    public class GeneralTest
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
        public void CheckReferences()
        {
            // we check reference to make sure we keep Gigner CoreNET minimal with less dependecies
            // if this test fail and you need to add ref or change please add relevant comment why we need this ref
            // see examples 

            //Arrange
            // Get the Ginger Core assembly
            Assembly a = Assembly.GetAssembly(typeof(GingerCoreNET.DriversLib.GingerNode));
            List<string> assemblies = new List<string>();

            // Core Net lib
            assemblies.Add("netstandard");

            // Ginger Core Common with old Ginger
            assemblies.Add("GingerCoreCommon");

            // for JSON
            assemblies.Add("Newtonsoft.Json");
            // For Ginger Plugins
            assemblies.Add("GingerPluginCore");
            // For evaluatng C# code at run time, will replace the VB script, so we can run on Linux - cross platform
            assemblies.Add("Microsoft.CodeAnalysis");
            // For evaluatng C# code at run time, will replace the VB script, so we can run on Linux - cross platform
            assemblies.Add("Microsoft.CodeAnalysis.Scripting");
            // ??? TODO: verify
            assemblies.Add("System.Collections.Immutable");
            // For evaluatng C# code at run time, will replace the VB script, so we can run on Linux - cross platform
            assemblies.Add("Microsoft.CSharp");
            // For evaluatng C# code at run time, will replace the VB script, so we can run on Linux - cross platform
            assemblies.Add("Microsoft.CodeAnalysis.CSharp.Scripting");
            // For evaluatng C# code at run time, will replace the VB script, so we can run on Linux - cross platform
            assemblies.Add("Microsoft.CodeAnalysis.CSharp");
            // Added for RemoteObjectServerClient - enable to cretae object proxy/wrapper in .NET standards 2.0, repalces RealProxy from 4.6
            assemblies.Add("System.Reflection.DispatchProxy");

            //Act
            AssemblyName[] referencedAssemblies = a.GetReferencedAssemblies();



            //Assert

            // We make sure we have exact ref count - if ref are added please add with explanantion 
            Assert.AreEqual(referencedAssemblies.Length, 11);

            foreach  (string s in assemblies)
            {
                AssemblyName name = (from x in referencedAssemblies where x.Name == s select x).FirstOrDefault();                
                Assert.IsTrue(name != null, "Verify assembly ref exist - " + s);
            }
            
            
            
        }
        
    }
}
