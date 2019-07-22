using Amdocs.Ginger.CoreNET.TelemetryLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.TelemetryTestLib
{

    [TestClass]
    [Level1]
    public class TelemetryTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
           
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }
 

        [TestMethod]  [Timeout(60000)]
        public void VerifyApplicationAPIModelFileExtension()
        {


            // Arrange
            Telemetry.CheckVersionAndNews();

            // Act


            //Assert            
            // Assert.AreEqual(ext, "Ginger.ApplicationAPIModel");
        }
        
    }
}
