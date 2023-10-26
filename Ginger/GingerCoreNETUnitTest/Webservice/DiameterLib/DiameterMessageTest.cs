using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.Webservice.DiameterLib
{
    [TestClass]
    public class DiameterMessageTest
    {
        private DiameterMessage message;

        [TestInitialize]
        public void TestInitialize()
        {
            message = new DiameterMessage();
        }

        [TestMethod]
        public void TestNameProperty()
        {
            // Arrange
            string expectedName = "TestMessageName";

            // Act
            message.Name = expectedName;
            string actualName = message.Name;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestMethod]
        public void TestProtocolVersionProperty()
        {
            // Arrange
            int expectedVersion = 2;

            // Act
            message.ProtocolVersion = expectedVersion;
            int actualVersion = message.ProtocolVersion;

            // Assert
            Assert.AreEqual(expectedVersion, actualVersion);
        }

        [TestMethod]
        public void TestMessageLengthProperty()
        {
            // Arrange
            int expectedMessageLength = 132;

            // Act
            message.MessageLength = expectedMessageLength;
            int actualMessageLength = message.MessageLength;

            // Assert
            Assert.AreEqual(expectedMessageLength, actualMessageLength);
        }

        [TestMethod]
        public void TestCommandCodeProperty()
        {
            // Arrange
            int expectedCommandCode = 257;

            // Act
            message.CommandCode = expectedCommandCode;
            int actualCommandCode = message.CommandCode;

            // Assert
            Assert.AreEqual(expectedCommandCode, actualCommandCode);
        }
        [TestMethod]
        public void TestApplicationIdProperty()
        {
            // Arrange
            int expectedApplicationId = 4;

            // Act
            message.ApplicationId = expectedApplicationId;
            int actualApplicationId = message.ApplicationId;

            // Assert
            Assert.AreEqual(expectedApplicationId, actualApplicationId);
        }
        [TestMethod]
        public void TestHopByHopIdentifierProperty()
        {
            // Arrange
            int expectedHopByHopIdentifier = 1;

            // Act
            message.HopByHopIdentifier = expectedHopByHopIdentifier;
            int actualHopByHopIdentifier = message.HopByHopIdentifier;

            // Assert
            Assert.AreEqual(expectedHopByHopIdentifier, actualHopByHopIdentifier);
        }

        [TestMethod]
        public void TestEndToEndIdentifierProperty()
        {
            // Arrange
            int expectedEndToEndIdentifier = 100;

            // Act
            message.EndToEndIdentifier = expectedEndToEndIdentifier;
            int actualEndToEndIdentifier = message.EndToEndIdentifier;

            // Assert
            Assert.AreEqual(expectedEndToEndIdentifier, actualEndToEndIdentifier);
        }

        [TestMethod]
        public void TestIsRequestBitSetProperty()
        {
            // Arrange
            bool expectedIsRequest = true;

            // Act
            message.IsRequestBitSet = expectedIsRequest;
            bool actualIsRequest = message.IsRequestBitSet;

            // Assert
            Assert.AreEqual(expectedIsRequest, actualIsRequest);
        }

        [TestMethod]
        public void TestIsProxiableBitSetProperty()
        {
            // Arrange
            bool expectedIsProxiable = true;

            // Act
            message.IsProxiableBitSet = expectedIsProxiable;
            bool actualIsProxiable = message.IsProxiableBitSet;

            // Assert
            Assert.AreEqual(expectedIsProxiable, actualIsProxiable);
        }

        [TestMethod]
        public void TestIsErrorBitSetProperty()
        {
            // Arrange
            bool expectedIsError = true;

            // Act
            message.IsErrorBitSet = expectedIsError;
            bool actualIsError = message.IsErrorBitSet;

            // Assert
            Assert.AreEqual(expectedIsError, actualIsError);
        }

        [TestMethod]
        public void TestIsRetransmittedBitSetProperty()
        {
            // Arrange
            bool expectedIsRetransmitted = true;

            // Act
            message.IsRetransmittedBitSet = expectedIsRetransmitted;
            bool actualIsRetransmitted = message.IsRetransmittedBitSet;

            // Assert
            Assert.AreEqual(expectedIsRetransmitted, actualIsRetransmitted);
        }
        [TestMethod]
        public void TestAvpListProperty()
        {
            // Arrange
            ObservableList<DiameterAVP> expectedList = new ObservableList<DiameterAVP>();
            var avp = new DiameterAVP();
            expectedList.Add(avp);

            // Act
            message.AvpList = expectedList;
            ObservableList<DiameterAVP> actualList = message.AvpList;

            // Assert
            Assert.AreEqual(1, actualList.Count, "The AvpList should contain exactly 1 element.");
            Assert.AreEqual(expectedList, actualList);
        }
        [TestMethod]
        public void TestAvpListReplaceReferenceProperty()
        {
            // Arrange
            var originalAvpList = message.AvpList;
            var newAvpList = new ObservableList<DiameterAVP>();

            // Act
            message.AvpList = newAvpList;
            var updatedAvpList = message.AvpList;

            // Assert
            Assert.AreSame(newAvpList, updatedAvpList, "The AvpList setter should replace the list reference.");
            Assert.AreNotSame(originalAvpList, updatedAvpList, "The AvpList setter should replace the list reference.");
        }
        [TestMethod]
        public void TestToStringMethod()
        {
            // Arrange
            string expectedName = message.Name;

            // Act
            string actualToStringResult = message.ToString();

            // Assert
            Assert.AreEqual(expectedName, actualToStringResult);
        }
    }
}
