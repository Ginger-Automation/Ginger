using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
using System;
using static Amdocs.Ginger.Common.GeneralLib.ApplicationInfo;

namespace GingerCoreNETUnitTests.SolutionTestsLib
{
    [TestClass]
    public class SolutionVersionUpgradeTest
    {
       private string CurrentVersion = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationBackendVersion;
        private string tempFile;

        [TestCleanup]
        public void TestCleanup()
        {
            if (tempFile != null && File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        private string CreateTempXmlFile(string version)
        {
            tempFile = Path.GetTempFileName();
            // Simulate legacy XML format with version in comment
            File.WriteAllLines(tempFile, new[]
            {
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<GingerRepositoryItem>",
                $"<Header ItemGuid=\"8d88ff24-efbd-4891-b42a-9d2e5becc969\" ItemType=\"RunSetConfig\" CreatedBy=\"mkale\" Created=\"202302281146\" GingerVersion=\"{version}\" Version=\"10\" LastUpdateBy=\"mkale\" LastUpdate=\"202412230948\" />\r\n",
                "<RunSetConfig Guid=\"8d88ff24-efbd-4891-b42a-9d2e5becc969\" Name=\"OCR_Runset\" SealightsTestRecommendations=\"No\"></RunSetConfig></GingerRepositoryItem>",
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>",
            });
            return tempFile;
        }

        [TestMethod]
        public void Enum_ContainsAllValues()
        {
            Assert.IsTrue(Enum.IsDefined(typeof(eGingerVersionComparisonResult), "LowerVersion"));
            Assert.IsTrue(Enum.IsDefined(typeof(eGingerVersionComparisonResult), "SameVersion"));
            Assert.IsTrue(Enum.IsDefined(typeof(eGingerVersionComparisonResult), "HigherVersion"));
            Assert.IsTrue(Enum.IsDefined(typeof(eGingerVersionComparisonResult), "ComparisonFailed"));
        }

        [TestMethod]
        public void CompareSolutionFileGingerVersion_LowerVersion()
        {
            string fileVer = "";
            string filePath = CreateTempXmlFile("1.2.3.3");
            var resultOld = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrent(filePath, ref fileVer);
            var resultNew = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrentByRecord(filePath, ref fileVer);
            Assert.AreEqual(eGingerVersionComparisonResult.LowerVersion, resultOld);
            Assert.AreEqual(resultOld, resultNew);
        }

        [TestMethod]
        public void CompareSolutionFileGingerVersion_SameVersion()
        {
            string fileVer = "";
            string filePath = CreateTempXmlFile(CurrentVersion);
            var resultOld = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrent(filePath, ref fileVer);
            var resultNew = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrentByRecord(filePath, ref fileVer);
            Assert.AreEqual(eGingerVersionComparisonResult.SameVersion, resultOld);
            Assert.AreEqual(resultOld, resultNew);
        }

        [TestMethod]
        public void CompareSolutionFileGingerVersion_HigherVersion()
        {
            string fileVer = "";
            string filePath = CreateTempXmlFile("50.1.0.0"); // For People who will be using Ginger in 2050
            var resultOld = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrent(filePath, ref fileVer);
            var resultNew = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrentByRecord(filePath, ref fileVer);
            Assert.AreEqual(eGingerVersionComparisonResult.HigherVersion, resultOld);
            Assert.AreEqual(resultOld, resultNew);
        }

        [TestMethod]
        public void CompareSolutionFileGingerVersion_InvalidVersion()
        {
            string fileVer = "";
            string filePath = CreateTempXmlFile("invalid.version.string");
            var resultOld = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrent(filePath, ref fileVer);
            var resultNew = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrentByRecord(filePath, ref fileVer);
            Assert.AreEqual(eGingerVersionComparisonResult.ComparisonFailed, resultOld);
            Assert.AreEqual(resultOld, resultNew);
        }

        [TestMethod]
        public void CompareSolutionFileGingerVersion_BetaVersion()
        {
            string fileVer = "";
            string filePath = CreateTempXmlFile("3.0.0.0Beta");
            var resultOld = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrent(filePath, ref fileVer);
            var resultNew = SolutionUpgrade.CompareSolutionFileGingerVersionToCurrentByRecord(filePath, ref fileVer);
            Assert.AreEqual(eGingerVersionComparisonResult.LowerVersion, resultOld);
            Assert.AreEqual(resultOld, resultNew);
        }

        [Ignore]
        [TestMethod]
        public void CompareSolutionFileGingerVersion_Test()
        {
            // UT for testing versions with different versions
            var appVersion = new VersionParts(25,4,1,0);
            var fileVersion = new VersionParts(24,5,0,0);
            var resultOld = SolutionUpgrade.CompareVersionsByRecord(appVersion, fileVersion);           
            Assert.IsTrue(true);
        }
    }
}