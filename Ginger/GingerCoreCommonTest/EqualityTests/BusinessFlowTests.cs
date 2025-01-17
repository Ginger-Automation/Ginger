using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GingerCoreCommonTest.Repository
{
    [TestClass]
    public class BusinessFlowEqualityTests
    {
        [TestMethod]
        public void ArAeEqual_ShouldReturnTrue_WhenBusinessFlowsAreEqual()
        {
            // Arrange
            var businessFlow1 = CreateBusinessFlow("TestFlow");
            var businessFlow2 = CreateBusinessFlow("TestFlow");

            // Act
            var result = businessFlow1.AreEqual(businessFlow2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ArAeEqual_ShouldReturnFalse_WhenBusinessFlowsAreNotEqual()
        {
            // Arrange
            var businessFlow1 = CreateBusinessFlow("TestFlow1");
            var businessFlow2 = CreateBusinessFlow("TestFlow2");

            // Act
            var result = businessFlow1.AreEqual(businessFlow2);

            // Assert
            Assert.IsFalse(result);
        }

        private BusinessFlow CreateBusinessFlow(string name)
        {
            return new BusinessFlow
            {
                Name = name,
                Activities =
                [
                    new Activity { ActivityName = "Activity1" },
                    new Activity { ActivityName = "Activity2" }
                ],
                Variables =
                [
                    new VariableString { Name = "Variable1" },
                    new VariableString { Name = "Variable2" }
                ],
                TargetApplications =
                [
                    new TargetApplication { AppName = "Target1", Guid = Guid.NewGuid() },
                    new TargetApplication { AppName = "Target2", Guid = Guid.NewGuid() }
                ]
            };
        }
    }
}
