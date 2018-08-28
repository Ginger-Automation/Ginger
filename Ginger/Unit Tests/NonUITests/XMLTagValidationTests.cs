#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions;
using GingerCore.Actions.XML;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerTestHelper;

namespace UnitTests.NonUITests
{
    /// <summary>
    /// Summary description for XMLTagValidationTests
    /// </summary>
    [TestClass]
    [Level3]
    public class XMLTagValidationTests
    {
        static string jsonFileName = string.Empty;
        static string xmlFileName = string.Empty;
        static ObservableList<ActInputValue> DynamicElements ;
   

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //

         [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            jsonFileName= TestResources.GetTestResourcesFile (@"JSON\sample2.json");
            xmlFileName = TestResources.GetTestResourcesFile(@"XML\book.xml");
            DynamicElements = new ObservableList<ActInputValue>();
            
        }

        private static ObservableList<ActInputValue> SetJSONDynamicParameters()
        {
            ObservableList<ActInputValue> temp = new ObservableList<ActInputValue>();
            ActInputValue inp=new ActInputValue();
            inp.Param = ".info.description";
            temp.Add(inp);
            inp = null;
            inp = new ActInputValue();
            inp.Param = ".parameters.query-salesChannel.description";
            temp.Add(inp);
            inp = null;
            return temp;
        }

        
        #endregion

        [TestMethod]
        public void JSonTests()
        {
            
            ActXMLTagValidation XTA = new ActXMLTagValidation();

            XTA.DocumentType = ActXMLTagValidation.eDocumentType.JSON;
            XTA.ReqisFromFile = true;
            XTA.InputFile.ValueForDriver = jsonFileName;
            DynamicElements = SetJSONDynamicParameters();
            XTA.DynamicElements = DynamicElements;
            XTA.AddNewReturnParams = true;
            XTA.Execute();

            Assert.AreEqual(2 , XTA.ActReturnValues.Count);
            Assert.AreEqual("Sales Channel\n", XTA.ActReturnValues.Where(x=>x.Param== "InnerText" && x.Path== ".parameters.query-salesChannel.description").FirstOrDefault().Actual);
            //

            // TODO: Add test logic here
            //
        }

        [TestMethod]
        public void XMLTests()
        {
            ActXMLTagValidation XTA = new ActXMLTagValidation();

            XTA.DocumentType = ActXMLTagValidation.eDocumentType.XML;
            XTA.ReqisFromFile = true;
            XTA.InputFile.ValueForDriver = xmlFileName;
            DynamicElements = SetXMLDynamicParameters();
            XTA.DynamicElements = DynamicElements;
            XTA.AddNewReturnParams = true;
            XTA.Execute();

            Assert.AreEqual(4, XTA.ActReturnValues.Count);
            Assert.AreEqual("amdocs", XTA.ActReturnValues.Where(x => x.Param == "//book[@publisher='amdocs']" && x.Path == "publisher").FirstOrDefault().Actual);
        }

        private ObservableList<ActInputValue> SetXMLDynamicParameters()
        {
            ObservableList<ActInputValue> temp = new ObservableList<ActInputValue>();
            ActInputValue inp = new ActInputValue();
            inp.Param = "//book[@publisher='amdocs']";
            temp.Add(inp);
            inp = null;
            inp = new ActInputValue();
            inp.Param = "/catalog/book[2]";
            temp.Add(inp);
            inp = null;
            return temp;
            
        }
    }
}
