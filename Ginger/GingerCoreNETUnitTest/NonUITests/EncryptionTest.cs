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
            encryptedString = EncryptionHandler.EncryptwithKey("ginger");

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
            decryptedString = EncryptionHandler.DecryptwithKey("mDL33JRKM3Zv1FdtGQMNZg==");

            //Assert
            Assert.AreEqual("ginger", decryptedString);
        }

        [TestMethod]
        [Timeout(60000)]
        public void EncryptLongString()
        {
            //Arrange
            string encryptedString = string.Empty;
            bool result = false;

            //Act
            encryptedString = EncryptionHandler.EncryptwithKey("53d7ccd462fbfbc43c6dc9f3f638747582df333b");

            //Assert
            Assert.AreEqual("FJoU8gEfBEM33VFEL7RZI4begMs2zj35JHGmMn6MUd9V1uYhkpNmtvo3uBRLYhtL", encryptedString);
        }
        [TestMethod]
        [Timeout(60000)]
        public void DecryptLongString()
        {
            //Arrange
            string decryptedString = string.Empty;
            bool result = false;

            //Act
            decryptedString = EncryptionHandler.DecryptwithKey("FJoU8gEfBEM33VFEL7RZI4begMs2zj35JHGmMn6MUd9V1uYhkpNmtvo3uBRLYhtL");

            //Assert
            Assert.AreEqual("53d7ccd462fbfbc43c6dc9f3f638747582df333b", decryptedString);
        }

    }
}
