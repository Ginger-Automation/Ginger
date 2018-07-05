using GingerUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerUtilsTest
{
    [TestClass]
    public class GMathTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            int a = 5;
            int b = 3;

            //Act
            int total = GMath.Sum(a, b);

            //Assert
            Assert.AreEqual(8, total, "5+3=8");
        }
    }
}
