//using Amdocs.Ginger.Common.UIElement;
//using Amdocs.Ginger.CoreNET.External.Katalon.Conversion;
//using GingerCore.Drivers.Common;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace GingerCoreNETUnitTest.External.Katalon.Conversion
//{
//    [TestClass]
//    public class KatalonObjectRepositoryToPOMConverterTests
//    {
//        private static string TempDirectory;

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext _)
//        {
//            TempDirectory = CreateTempDirectory();
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {
//            if (TempDirectory != null)
//            {
//                Directory.Delete(TempDirectory, recursive: true);
//            }
//        }

//        private static string CreateTempDirectory()
//        {
//            string path = Path.Combine(Path.GetTempPath(), $"KatalonObjectRepositoryToPOMConverterTests_{DateTime.Now:ddMMyyyyHHmmSS}");
//            Directory.CreateDirectory(path);
//            return path;
//        }

//        [TestCategory(TestCategory.UnitTest)]
//        [TestMethod]
//        public void TryParseWebElementEntity_ContainsElementGuidId_SetToGuid()
//        {
//            XmlElement element = GetKatalonObjectRepositoryElement();

//            _ = KatalonObjectRepositoryToPOMConverter.TryConvertKatalonObjectToElementInfo(element, out ElementInfo elementInfo, out _);

//            Assert.AreEqual(expected: "167d6d22-adfb-4ee3-8035-e870ca200cc2", actual: elementInfo.Guid.ToString());
//        }

//        [TestCategory(TestCategory.UnitTest)]
//        [TestMethod]
//        public void TryParseWebElementEntity_ContainsName_SetToElementName()
//        {
//            XmlElement element = GetKatalonObjectRepositoryElement();

//            _ = KatalonObjectRepositoryToPOMConverter.TryConvertKatalonObjectToElementInfo(element, out ElementInfo elementInfo, out _);

//            Assert.AreEqual(expected: "input_Epic sadface Username is required_login-button", actual: elementInfo.ElementName);
//        }

//        [TestCategory(TestCategory.UnitTest)]
//        [TestMethod]
//        public void TryParseWebElementEntity_ContainsWebElementPropertiesWithTagName_SetToElementType()
//        {
//            XmlElement element = GetKatalonObjectRepositoryElement();

//            _ = KatalonObjectRepositoryToPOMConverter.TryConvertKatalonObjectToElementInfo(element, out ElementInfo elementInfo, out _);

//            Assert.AreEqual(expected: "input", actual: elementInfo.ElementType);
//        }

//        [TestCategory(TestCategory.UnitTest)]
//        [TestMethod]
//        public void TryParseWebElementEntity_ContainsWebElementPropertiesWithTagAndTypeName_SetToElementTypeEnum()
//        {
//            XmlElement element = GetKatalonObjectRepositoryElement();

//            _ = KatalonObjectRepositoryToPOMConverter.TryConvertKatalonObjectToElementInfo(element, out ElementInfo elementInfo, out _);

//            Assert.AreEqual(expected: eElementType.Button, actual: elementInfo.ElementTypeEnum);
//        }

//        [TestCategory(TestCategory.UnitTest)]
//        [TestMethod]
//        public void TryParseWebElementEntity_StaticValue_IsAutoLearnedIsTrue()
//        {
//            XmlElement element = GetKatalonObjectRepositoryElement();

//            _ = KatalonObjectRepositoryToPOMConverter.TryConvertKatalonObjectToElementInfo(element, out ElementInfo elementInfo, out _);

//            Assert.IsTrue(elementInfo.IsAutoLearned);
//        }

//        private static XmlElement GetKatalonObjectRepositoryElement()
//        {
//            XmlDocument document = new();
//            FileStream stream = File.OpenRead(@"C:\Users\harsisin\Katalon Studio\My First Web UI Project\Object Repository\Page_Swag Labs\input_Epic sadface Username is required_login-button.rs");
//            document.Load(stream);

//            foreach (XmlNode child in document.ChildNodes)
//            {
//                if (child.NodeType == XmlNodeType.Element && child is XmlElement element)
//                {
//                    return element;
//                }
//            }

//            throw new Exception("No root element found");
//        }
//    }
//}
