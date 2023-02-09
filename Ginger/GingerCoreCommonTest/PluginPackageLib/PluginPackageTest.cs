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

using Amdocs.Ginger.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreCommonTest.PluginPackageLib
{
    [TestClass]
    public class PluginPackageTest
    {


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {

        }


        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        // Restore after moveing to .NET Core 3

        //[TestMethod]
        //[Timeout(60000)]
        //public void PluginGetTextFileEditors()
        //{
        //    //Arrange   
        //    PluginPackage pluginPackage = new PluginPackage(@"C:\Users\yaronwe\source\repos\Ginger-Core-Plugin\PluginExample1\bin\Release\netcoreapp2.1\publish");


        //    // Act            
        //    var v = pluginPackage.GetTextFileEditors();            

        //    //Assert
        //    Assert.AreEqual(0, v.Count, "Text editors count=0");            
        //}

        //[TestMethod]
        //[Timeout(60000)]
        //public void PACTPluginGetTextFileEditors()
        //{
        //    //Arrange   
        //    PluginPackage pluginPackage = new PluginPackage(@"C:\Users\yaronwe\source\repos\Ginger-PACT-Plugin\GingerPACTPluginConsole\bin\Release\netcoreapp2.1\publish");


        //    // Act            
        //    var v = pluginPackage.GetTextFileEditors();

        //    //Assert
        //    Assert.AreEqual(0, v.Count, "Text editors count=0");
        //}


    }
}
