#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace GingerCoreNETUnitTest.Webservice.DiameterLib
{
    [TestClass]
    public class DiameterAVPTest
    {
        private DiameterAVP avp;

        [TestInitialize]
        public void TestInitialize()
        {
            avp = new DiameterAVP();
        }
        [TestMethod]
        public void TestCodeProperty()
        {
            // Arrange
            int expectedCode = 101;

            // Act
            avp.Code = expectedCode;
            int actualCode = avp.Code;

            // Assert
            Assert.AreEqual(expectedCode, actualCode);
        }
        [TestMethod]
        public void TestNameProperty()
        {
            // Arrange
            string expectedName = "TestAVPName";

            // Act
            avp.Name = expectedName;
            string actualName = avp.Name;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }
        [TestMethod]
        public void TestVendorIdProperty()
        {
            // Arrange
            int expectedVendorId = (int)eDiameterVendor._3GPP;

            // Act
            avp.VendorId = expectedVendorId;
            int actualVendorId = avp.VendorId;

            // Assert
            Assert.AreEqual(expectedVendorId, actualVendorId);
        }
        [TestMethod]
        public void TestIsMandatoryProperty()
        {
            // Arrange
            bool expectedIsMandatory = true;

            // Act
            avp.IsMandatory = expectedIsMandatory;
            bool actualIsMandatory = avp.IsMandatory;

            // Assert
            Assert.AreEqual(expectedIsMandatory, actualIsMandatory);
        }
        [TestMethod]
        public void TestIsVendorSpecificProperty()
        {
            // Arrange
            bool expectedIsVendorSpecific = true;

            // Act
            avp.IsVendorSpecific = expectedIsVendorSpecific;
            bool actualVendorSpecific = avp.IsVendorSpecific;

            // Assert
            Assert.AreEqual(expectedIsVendorSpecific, actualVendorSpecific);
        }
        [TestMethod]
        public void TestDefaultDataTypeProperty()
        {
            // Arrange
            eDiameterAvpDataType expectedDefaultDataType = eDiameterAvpDataType.Address;

            // Act

            // Assert
            Assert.AreEqual(expectedDefaultDataType, avp.DataType, "The property should use the default enum value when not explicitly set.");
        }
        [TestMethod]
        public void TestDataTypeProperty()
        {
            // Arrange
            eDiameterAvpDataType expectedDataType = eDiameterAvpDataType.Enumerated;

            // Act
            avp.DataType = expectedDataType;
            eDiameterAvpDataType actualDataType = avp.DataType;

            // Assert
            Assert.AreEqual(expectedDataType, actualDataType);
        }

        [TestMethod]
        public void TestNestedAvpListProperty()
        {
            // Arrange
            ObservableList<DiameterAVP> expectedList = [];
            avp.DataType = eDiameterAvpDataType.Grouped;
            DiameterAVP childAVP = new DiameterAVP();
            expectedList.Add(childAVP);

            // Act
            avp.NestedAvpList = expectedList;
            ObservableList<DiameterAVP> actualList = avp.NestedAvpList;

            // Assert
            Assert.AreEqual(1, actualList.Count, "The NestedAvpList should contain exactly 1 element.");
            Assert.AreEqual(expectedList, actualList);
        }
        [TestMethod]
        public void TestNestedAvpListReplaceReferenceProperty()
        {
            // Arrange
            var originalChildAvpList = avp.NestedAvpList;
            var newChildAvpList = new ObservableList<DiameterAVP>();

            // Act
            avp.NestedAvpList = newChildAvpList;
            var updatedChildAvpList = avp.NestedAvpList;

            // Assert
            Assert.AreSame(newChildAvpList, updatedChildAvpList, "The NestedAvpList setter should replace the list reference.");
            Assert.AreNotSame(originalChildAvpList, updatedChildAvpList, "The NestedAvpList setter should replace the list reference.");
        }
        [TestMethod]
        public void TestParentNameProperty()
        {
            // Arrange
            avp.Name = "TestParentName";
            avp.DataType = eDiameterAvpDataType.Grouped; // Not really required, just describing the logic
            var childAvp = new DiameterAVP();
            avp.NestedAvpList.Add(childAvp);
            string expectedParentName = avp.Name;

            // Act
            childAvp.ParentName = expectedParentName;
            string actualParentName = childAvp.ParentName;

            // Assert
            Assert.AreEqual(expectedParentName, actualParentName);
        }
        [TestMethod]
        public void TestParentAvpGuidProperty()
        {
            // Arrange
            avp.Name = "TestParentName";
            avp.DataType = eDiameterAvpDataType.Grouped; // Not really required, just describing the logic
            var childAvp = new DiameterAVP();
            avp.NestedAvpList.Add(childAvp);
            Guid expectedParentAvpGuid = avp.Guid;

            // Act
            childAvp.ParentAvpGuid = expectedParentAvpGuid;
            Guid actualParentAvpGuid = childAvp.ParentAvpGuid;

            // Assert
            Assert.AreEqual(expectedParentAvpGuid, actualParentAvpGuid);
        }
        [TestMethod]
        public void TestConstructor()
        {
            // Arrange & Act & Assert
            Assert.IsNotNull(avp.NestedAvpList);
        }
    }
}
