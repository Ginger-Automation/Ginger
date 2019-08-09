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
