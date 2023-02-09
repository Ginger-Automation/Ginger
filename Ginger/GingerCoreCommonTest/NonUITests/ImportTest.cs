#region License
/*
Copyright © 2014-2023 European Support Limited

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


using Ginger.Imports.UFT;
using System.Collections.Generic;
using System.Linq;


using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;
using GingerTestHelper;

namespace UnitTests.NonUITests
{
    [TestClass]
    public class ImportTest 
    {
        [TestInitialize]
        public void TestInitialize()
        {


        }

        [TestCleanup()]
        public void TestCleanUp()
        {
            
        }


        //[TestMethod]  [Timeout(60000)]
        //public void ObjectCount_OR()
        //{
        //    //Arrange            
        //    List<ObjectRepositoryItem> Objectlist_ORI = new List<ObjectRepositoryItem>();
        //    string sXMLPath = TestResources.GetTestResourcesFile(@"UFTFiles\Sample_OR.xml");

        //    //Act
        //    Objectlist_ORI = ObjectRepositoryConverter.ProcessXML(sXMLPath);

        //    //Assert

        //    // Preeti - please put small file with expected number of obj
        //   Assert.AreEqual(Objectlist_ORI.Count(), 10, "Objects count");
        //    // MessageBox.Show("Actual Object Count fetched:" + Objectlist_ORI.Count());

        //}


        //[TestMethod]  [Timeout(60000)]
        //public void BusAndAssociatedGUI()
        //{
        //    //Arrange   
        //    List<BusFunction> BusList = new List<BusFunction>();
        //    List<string> GuiFunctionList = new List<string>();
        //    string AllBusFunctions = "";

        //    string sBUSfilePath = TestResources.GetTestResourcesFile(@"UFTFiles\Sample_BUS_FILE.txt");

        //    //Act
        //    BusFunctionHandler BusHandler = new BusFunctionHandler();
        //    BusList = BusHandler.ProcessBusScript(sBUSfilePath);

        //    foreach (BusFunction x in BusList)
        //    {
        //        AllBusFunctions = AllBusFunctions + "\n" + x.BusFunctionName;

        //        if (x.ListOfGuiFunctions.Count!=0)
        //        {
        //            AllBusFunctions = AllBusFunctions + "\n\n";
        //            foreach (string gui in x.ListOfGuiFunctions)
        //            {
        //                AllBusFunctions = AllBusFunctions + "\t" + gui;
        //            }
        //        }

        //    }


        //    // No messages in UT
        //    // MessageBox.Show("=== ALL BUS & GUI LIST FUNCTIONS ==== : " + AllBusFunctions);

        //}

        [Level3]
        [TestMethod]  [Timeout(60000)]
        public void Import_ASAP_1()
        {
            //Arrange            
            // OR file, code files bus

            //Act
            

            //Assert
            
        }

    }
}


//UFTObjectRepositoryTextBox.Text = @"C:\Users\Preetigu\Desktop\UFT_GINGER\ASAP files\ASAP_Repository.xml";
//ScriptFileTextBox.Text = @"C:\Users\Preetigu\Desktop\UFT_GINGER\ASAP files\AIO_ASAP_Gui.txt";
//CalendarTextBox.Text = @"C:\Users\Preetigu\Desktop\UFT_GINGER\ASAP files\ASAP_REG.xls";