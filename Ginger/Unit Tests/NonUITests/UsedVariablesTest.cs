using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreCommonTest.VariablesTest
{
    [TestClass]
    public class UsedVariablesTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            //
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // before every test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //after every test
        }

        [TestMethod]
        public void ActionUsedVar()
        {
            //Arrange            
            ActDummy act1 = new ActDummy();                        
            act1.Value = "{Var Name=v1}";

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(act1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }


        [TestMethod]
        public void ActivityUsedVar()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();
            a1.Acts.Add(act1);
            VariableString v1 = new VariableString() { InitialStringValue = "abc" };
            a1.Variables.Add(v1);
            act1.Value = "{Var Name=v1}";

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }


        [TestMethod]
        public void UsedVarInFlowControlCondition()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();
            a1.Acts.Add(act1);         
            FlowControl flowControl = new FlowControl() { Condition = "{Var Name=v1}=123" };
            act1.FlowControls.Add(flowControl);            

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }

        [TestMethod]
        public void UsedVarInActionOutputExpected()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();
            ActReturnValue actReturnValue = new ActReturnValue() { Param = "out1", Expected = "{Var Name=v1}" };
            act1.ReturnValues.Add(actReturnValue);
            a1.Acts.Add(act1);            

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }


        [TestMethod]
        public void UsedVarInActionOutoutStoreToVar()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();
            ActReturnValue actReturnValue = new ActReturnValue() { Param = "out1", StoreToVariable = "v1" };
            act1.ReturnValues.Add(actReturnValue);
            a1.Acts.Add(act1);

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }


        [TestMethod]
        public void SameVarUsedInSeveralPlaces()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();            
            ActReturnValue actReturnValue = new ActReturnValue() { Param = "out1", StoreToVariable = "v1" };
            act1.ReturnValues.Add(actReturnValue);
            FlowControl flowControl = new FlowControl() { Condition = "{Var Name=v1}=123" };
            act1.FlowControls.Add(flowControl);
            a1.Acts.Add(act1);

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(1, usedVars.Count, "usedVars.Count");
            Assert.AreEqual("v1", usedVars[0], "usedVars[0]=v1");
        }

        [TestMethod]
        public void DifferenVarsUsedInSeveralPlaces()
        {
            //Arrange
            Activity a1 = new Activity() { ActivityName = "a1" };
            ActDummy act1 = new ActDummy();

            ActReturnValue actReturnValue = new ActReturnValue() { Param = "out1", StoreToVariable = "v1" };
            act1.ReturnValues.Add(actReturnValue);
            FlowControl flowControl = new FlowControl() { Condition = "{Var Name=v2}=123" };
            act1.FlowControls.Add(flowControl);
            a1.Acts.Add(act1);

            //Act
            List<string> usedVars = new List<string>();
            VariableBase.GetListOfUsedVariables(a1, ref usedVars);

            //Assert
            Assert.AreEqual(2, usedVars.Count, "usedVars.Count");
            Assert.IsTrue(usedVars.Contains("v1"), "usedVars.Contains 'v1'");
            Assert.IsTrue(usedVars.Contains("v2"), "usedVars.Contains 'v2'");
        }


    }

    

}
