using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;
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

        [TestMethod]
        [Timeout(60000)]
        public void TelemetryTest_AddConfiguredFeature()
        {

            // Act
            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.ModelParameters.ToString(), true, false);

            //Assert            
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].Name, TelemetrySession.GingerUsedFeatures.ModelParameters.ToString());
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].PlatformType, string.Empty);
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].IsUsed.ToString(), Boolean.FalseString);
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].IsConfigured.ToString(), Boolean.TrueString);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TelemetryTest_UpdateUsedFeature()
        {

            // Act
            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.GlobalVaraibles.ToString(), true, true);
            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.GlobalVaraibles.ToString(), true, false);

            //Assert            
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].Name, TelemetrySession.GingerUsedFeatures.GlobalVaraibles.ToString());
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].PlatformType, string.Empty);
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].IsUsed.ToString(), Boolean.TrueString);
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures[0].IsConfigured.ToString(), Boolean.TrueString);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TelemetryTest_AddSameFeaturesDiffrentPlatforms()
        {

            // Act
            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.POM.ToString(), true, false, eImageType.Globe.ToString());
            UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.POM.ToString(), true, false, eImageType.Java.ToString());

            //Assert            
            Assert.AreEqual(WorkSpace.Instance.Telemetry.TelemetrySession.UsedFeatures.Count, 2);
        }

    }
}
