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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using GingerWPF.ApplicationModelsLib.APIModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerTest
{
    [TestClass]
    public class WSDLParserTest
    {



        [TestInitialize]
        public void TestInitialize()
        {
            string TempSolutionFolder = TestResources.GetTestTempFolder(@"Solutions\WSDL");
            if (Directory.Exists(TempSolutionFolder))
            {
                Directory.Delete(TempSolutionFolder, true);
            }
            //Solution.CreateNewSolution("WSDL", TempSolutionFolder);
            //SR = new SolutionRepository();
            //SR.Open(TempSolutionFolder);                
        }


        [TestCleanup]
        public void TestCleanUp()
        {

        }

        [Ignore] // TODO: FIXME
        [Level2]
        [TestMethod]  [Timeout(60000)]
        public void GenerateAPIfromWSDL()
        {
            // Arrange
            string filename = TestResources.GetTestResourcesFile(@"APIModel\globalweather.xml");
            WSDLParser WSDLP = new WSDLParser();

            //Act
            ObservableList<ApplicationAPIModel> AAMList = new ObservableList<ApplicationAPIModel>();
            AAMList = WSDLP.ParseDocument(filename, AAMList);

            //Assert
            Assert.AreEqual(AAMList.Count, 4, "Objects count");
            //Assert.AreEqual(AAMList[0].GroupName, "GlobalWeatherSoap", "binding name");
            Assert.AreEqual(AAMList[0].Name, "GetWeather_GlobalWeatherSoap", "operation name");
            Assert.AreEqual(AAMList[0].RequestBody.Length, 439, "operation name");

            //Assert.AreEqual(AAMList[1].GroupName, "GlobalWeatherSoap", "binding name");
            Assert.AreEqual(AAMList[1].Name, "GetCitiesByCountry_GlobalWeatherSoap", "operation name");
            Assert.AreEqual(AAMList[1].RequestBody.Length, 357, "operation name");

            //Assert.AreEqual(AAMList[2].GroupName, "GlobalWeatherSoap12", "binding name");
            Assert.AreEqual(AAMList[2].Name, "GetWeather_GlobalWeatherSoap12", "operation name");

            //Assert.AreEqual(AAMList[3].GroupName, "GlobalWeatherSoap12", "binding name");
            Assert.AreEqual(AAMList[3].Name, "GetCitiesByCountry_GlobalWeatherSoap12", "operation name");


            //Assert.AreEqual(AAMList[4].GroupName, "GlobalWeatherHttpGet", "binding name");
            //Assert.AreEqual(AAMList[4].Name, "GlobalWeatherHttpGet_GetWeather", "operation name");

            //Assert.AreEqual(AAMList[5].GroupName, "GlobalWeatherHttpGet", "binding name");
            //Assert.AreEqual(AAMList[5].Name, "GlobalWeatherHttpGet_GetCitiesByCountry", "operation name");

            //Assert.AreEqual(AAMList[6].GroupName, "GlobalWeatherHttpPost", "binding name");
            //Assert.AreEqual(AAMList[6].Name, "GlobalWeatherHttpPost_GetWeather", "operation name");

            //Assert.AreEqual(AAMList[7].GroupName, "GlobalWeatherHttpPost", "binding name");
            //Assert.AreEqual(AAMList[7].Name, "GlobalWeatherHttpPost_GetCitiesByCountry", "operation name");

            foreach (ApplicationAPIModel AAM in AAMList)
            {
                if ((AAMList.IndexOf(AAM) % 2) == 0)
                {
                    Assert.AreEqual(AAM.AppModelParameters.Count, 2, "Dynamic Parameters count");
                    Assert.AreEqual(AAM.Description, "Get weather report for all major cities around the world.", "Description check");
                    Assert.AreEqual(AAM.RequestBody.Length, 439, "Request Body Length Check");
                }
                else
                {
                    Assert.AreEqual(AAM.AppModelParameters.Count, 1, "Dynamic Parameters count");
                    Assert.AreEqual(AAM.Description, "Get all major cities by country name(full / part).", "Description check");
                    Assert.AreEqual(AAM.RequestBody.Length, 357, "Request Body Length Check");
                }
                //Assert.AreEqual(AAM.APIModelKeyValueHeaders.Count, 8, "KeyValueHeaders count");
                Assert.AreEqual(AAM.EndpointURL, "http://www.webservicex.net/globalweather.asmx" , "KeyValueHeaders count");
            }



        }



        
    }
}
