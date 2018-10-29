using amdocs.ginger.GingerCoreNET;
using GingerCore.Variables;
using GingerTest.POMs;
using GingerTestHelper;
using GingerWPFUnitTest;
using GingerWPFUnitTest.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.VariablesLib
{
    [TestClass]
    [Level3]
    public class VariablesTest
    {

        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static TestContext mTC;
        static string SolutionFolder;
        static GingerAutomator mGingerAutomator;

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            mTC = TC;

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\BasicSimple");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\VariablesTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(SolutionFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }


        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        
        [TestMethod]
        public void AddGlobalStringVariable()
        {
            //Arrange
            string name = "str1";
            
            //Act                        
            GlobalVariablesPOM globalVariablesPOM = mGingerAutomator.MainWindowPOM.GotoGlobalVariables();
            globalVariablesPOM.AddStringVariable(name);
            VariableBase v = (from x in Ginger.App.UserProfile.Solution.Variables where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(name, v.Name, "Var Name");
        }
    }
}
