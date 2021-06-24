using Amdocs.Ginger;
using GingerCore;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.NonUITests
{
    [TestClass]
    public class EncryptionTest
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            
        }

        [TestMethod]  [Timeout(60000)]
        public void IsEncryptedFalse()
        {
            //Arrange
            string strToDecry = "erer";
            bool IsDecrypt;

            //Act
            IsDecrypt = EncryptionHandler.IsStringEncrypted(strToDecry);

            //Assert
            Assert.AreEqual(false, IsDecrypt);
        }

        [TestMethod]  [Timeout(60000)]
        public void IsEncryptedTrue()
        {
            //Arrange
            string strToDecry = "+2FvQl58QkZC0d7ebO19SA==";
            bool IsDecrypt;

            //Act
            IsDecrypt = EncryptionHandler.IsStringEncrypted(strToDecry);

            //Assert
            Assert.AreEqual(true, IsDecrypt);
        }

        [TestMethod]  [Timeout(60000)]
        public void EncryptString()
        {
            //Arrange
            string encryptedString = string.Empty;
            bool result = false;

            //Act
            encryptedString = EncryptionHandler.EncryptwithKey("ginger", "D3^hdfr7%ws4Kb56=Qt");

            //Assert
            Assert.AreEqual("mDL33JRKM3Zv1FdtGQMNZg==", encryptedString);
        }

        [TestMethod]  [Timeout(60000)]
        public void DecryptString()
        {
            //Arrange
            string decryptedString = string.Empty;
            bool result = false;

            //Act
            decryptedString = EncryptionHandler.DecryptString("mDL33JRKM3Zv1FdtGQMNZg==",ref result);

            //Assert
            Assert.AreEqual("ginger", decryptedString);
        }
        
    }
}
