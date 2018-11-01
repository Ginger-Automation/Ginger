using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class SelectionListVariableTests
    {

        [TestMethod]
        public void SelectionListVar_TestVariableType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            string varType = variableSelectionList.VariableType();

            //Assert
            Assert.AreEqual(varType, "Selection List", "Selection List Variable Type mismatch");
        }

        [TestMethod]
        public void SelectionListVar_TestVariableUIType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            string varType = variableSelectionList.VariableUIType;

            //Assert
            Assert.AreEqual(varType, "Variable Selection List", "Selection List Variable UI Type mismatch");
        }
    }
}
