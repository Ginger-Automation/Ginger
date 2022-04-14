#region License
/*
Copyright © 2014-2022 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.REST;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class WebServicesTest 
    {        
        static BusinessFlow mBF;
        static GingerRunner mGR;
        static Agent wsAgent = new Agent();
        static WebServicesDriver mDriver;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            SolutionRepository  mSolutionRepository = WorkSpace.Instance.SolutionRepository;
     
            string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
            mSolutionRepository.Open(TempRepositoryFolder);
            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution();
            sol.ContainingFolderFullPath = TempRepositoryFolder;
            WorkSpace.Instance.Solution = sol;
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder,"ExecutionResults");

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF WebServices Web API";
            mBF.Active = true;


            Platform p = new Platform();
            p.PlatformType = ePlatformType.WebServices;            


            mDriver = new WebServicesDriver(mBF);
            mDriver.SaveRequestXML = true;
            mDriver.SavedXMLDirectoryPath = "~\\Documents";
            mDriver.SecurityType = @"None";

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(wsAgent);

            mGR.Executor.BusinessFlows.Add(mBF);

          

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {
            
        }
        [TestCleanup]
        public void TestMethodCleanUP()
        {
            mBF.Activities.ClearAll();
        }

        [TestMethod]  [Timeout(60000)]
        public void APIModelExecutionTest()
        {

            //Arrange
            Activity Activity1 = new Activity();
            Activity1.Active = true;
            Activity1.ActivityName = "Web API Model";
            Activity1.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity1);


            ActWebAPIModel actWebAPIModel = new ActWebAPIModel();
            actWebAPIModel.APImodelGUID = Guid.Parse("fcac0742-4747-4939-99ab-2d23c33ab74e");
            actWebAPIModel.APIModelParamsValue.Add(new EnhancedActInputValue() { Param = "<<COUNTRYNAME>>", Value = "israel", ValueForDriver = "israel" });
            actWebAPIModel.Description = "Testing Action";
            actWebAPIModel.Active = true;
            actWebAPIModel.EnableRetryMechanism = false;

            mBF.Activities[0].Acts.Add(actWebAPIModel);

            mDriver.StartDriver();

            //Act            
            mGR.Executor.RunRunner();

           
            //Assert
            //TODO: add some assert??

        }

        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void BizFlowSaveLoad()
        //{
        //   // ExampleAPIProxy.ExampleWebMethod("aa",1);

        //}

        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void WebServHelloWorld()
        //{
        //    //// Create a request using a URL that can receive a post. 
        //    //WebRequest request = WebRequest.Create("http://localhost:52746/WebService1.asmx/HelloWorld ");
        //    //// Set the Method property of the request to POST.
        //    //request.Method = "POST";
        //    //// Create POST data and convert it to a byte array.
        //    //string postData = "";
        //    //byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        //    //// Set the ContentType property of the WebRequest.
        //    //request.ContentType = "application/x-www-form-urlencoded";
        //    //// Set the ContentLength property of the WebRequest.
        //    //request.ContentLength = byteArray.Length;
        //    //// Get the request stream.
        //    //Stream dataStream = request.GetRequestStream();
        //    //// Write the data to the request stream.
        //    //dataStream.Write(byteArray, 0, byteArray.Length);
        //    //// Close the Stream object.
        //    //dataStream.Close();
        //    //// Get the response.
        //    //WebResponse response = request.GetResponse();
        //    //// Display the status.
        //    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        //    //// Get the stream containing content returned by the server.
        //    //dataStream = response.GetResponseStream();
        //    //// Open the stream using a StreamReader for easy access.
        //    //StreamReader reader = new StreamReader(dataStream);
        //    //// Read the content.
        //    //string responseFromServer = reader.ReadToEnd();
        //    //// Display the content.
        //    //Console.WriteLine(responseFromServer);
        //    //// Clean up the streams.
        //    //reader.Close();
        //    //dataStream.Close();
        //    //response.Close();

        //}

        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void WebServCreateCustomer()
        //{
        //    // Create a request using a URL that can receive a post. 
        //    //WebRequest request = WebRequest.Create("http://localhost:49642/Service1.svc");
        //    //// Set the Method property of the request to POST.
        //    //request.Method = "POST";
        //    //// Create POST data and convert it to a byte array.
        //    //string postData = "FirstName=string&LastName=string";
        //    //byte[] byteArray = Encoding.UTF8.GetBytes(postData);
        //    //// Set the ContentType property of the WebRequest.
        //    //request.ContentType = "application/x-www-form-urlencoded";
        //    //// Set the ContentLength property of the WebRequest.
        //    //request.ContentLength = byteArray.Length;
        //    //// Get the request stream.
        //    //Stream dataStream = request.GetRequestStream();
        //    //// Write the data to the request stream.
        //    //dataStream.Write(byteArray, 0, byteArray.Length);
        //    //// Close the Stream object.
        //    //dataStream.Close();
        //    //// Get the response.
        //    //WebResponse response = request.GetResponse();
        //    //// Display the status.
        //    //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
        //    //// Get the stream containing content returned by the server.
        //    //dataStream = response.GetResponseStream();
        //    //// Open the stream using a StreamReader for easy access.
        //    //StreamReader reader = new StreamReader(dataStream);
        //    //// Read the content.
        //    //string responseFromServer = reader.ReadToEnd();
        //    //// Display the content.
        //    //Console.WriteLine(responseFromServer);
        //    //// Clean up the streams.
        //    //reader.Close();
        //    //dataStream.Close();
        //    //response.Close();

        //}

        [Ignore]
        [TestMethod]  
        [Timeout(60000)]
        public void WebServices_WebServiceSendXML()
        {
            WebServiceXML webServiceCall = new WebServiceXML();
            string URL = "http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso";
            string soapAction = "";
            string xmlRequest = @"<?xml version=""1.0"" encoding=""utf-8""?><soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope""> <soap12:Body> <ListOfCountryNamesByName xmlns=""http://www.oorsprong.org/websamples.countryinfo""> </ListOfCountryNamesByName></soap12:Body></soap12:Envelope>";
            string Status = "test";
            bool failFlag = false;
            string webRespone = webServiceCall.SendXMLRequest(URL, soapAction, xmlRequest,ref Status,ref failFlag, null);

            StringAssert.Contains(webRespone, "<m:sName>Åland Islands</m:sName>");
            
            
        }

        [TestMethod]  [Timeout(60000)]
        public void WebServices_WebAPISOAP()
        {
                       

            Activity Activity1 = new Activity();
            Activity1.Active = true;
            Activity1.ActivityName = "Web API Soap";
            Activity1.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity1);

            ActWebAPISoap soapAct = new ActWebAPISoap();
            ActWebAPIBase act = new ActWebAPIBase();
            //HttpWebClientUtils testClient = new HttpWebClientUtils();

            //Build SOAP Request:
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "http://www.thomas-bayer.com/axis2/services/BLZService");
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPISoap.Fields.SOAPAction, "UnitTest");
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            //Set Request body content:
            soapAct.mUseRequestBody = true;
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBody, getXML());
            soapAct.Active = true;
            soapAct.EnableRetryMechanism = false;
            soapAct.ItemName = "Web API Soap";

            //BusinessFlow bzFlow = new BusinessFlow();
            //WebServicesDriver testDriver = new WebServicesDriver(bzFlow);
            //testDriver.RunAction(soapAct);
            //Act Act1 = new Act();


            mBF.Activities[0].Acts.Add(soapAct);

            mDriver.StartDriver();
            mGR.Executor.RunRunner();

            if (soapAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in soapAct.ReturnValues)
                {
                    if (val.Actual.ToString() == "59586")
                        Assert.AreEqual(val.Actual,"59586");
                }
            }
        }

        //Start Here
        [TestMethod][Timeout(600000)]
        public void WebServices_RawRequestWebAPIRestWithJSON() 
        {
            //Arrange
            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://petstore.swagger.io/v2/pet");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBody, "{\r\n  \"id\": 55,\r\n  \"category\": {\r\n    \"id\": 0,\r\n    \"name\": \"string\"\r\n  },\r\n  \"name\": \"Rexi\",\r\n  \"photoUrls\": [\r\n    \"string\"\r\n  ],\r\n  \"tags\": [\r\n    {\r\n      \"id\": 0,\r\n      \"name\": \"string\"\r\n    }\r\n  ],\r\n  \"status\": \"available\"\r\n}");
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());


            context = Context.GetAsContext(restAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(restAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(restAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;
            StringAssert.Contains(rawRequestContent, "POST https://petstore.swagger.io/v2/pet HTTP/1.1");
            StringAssert.Contains(rawRequestContent, "Accept: application/json");
            StringAssert.Contains(rawRequestContent, "Content-Type: application/json; charset=utf-8");
            StringAssert.Contains(rawRequestContent, "Content-Length: 231");
            StringAssert.Contains(rawRequestContent, "Host: petstore.swagger.io");
            StringAssert.Contains(rawRequestContent, "\"id\": 55,");
            StringAssert.Contains(rawRequestContent, "\"category\": {");
            StringAssert.Contains(rawRequestContent, "\"id\": 0,");
            StringAssert.Contains(rawRequestContent, "\"name\": \"Rexi\"");
            StringAssert.Contains(rawRequestContent, "\"status\": \"available\"");
        }

        [TestMethod]
        [Timeout(600000)]
        public void WebServices_RawRequestWebAPISoap()
        {
            //Arrange
            ActWebAPISoap soapAct = new ActWebAPISoap();
            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);
            Context context = new Context();
            context.Runner = mGR.Executor;
            soapAct.Context = context;

            //Build SOAP Request:
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "http://www.dneonline.com/calculator.asmx");
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPISoap.Fields.SOAPAction, "http://tempuri.org/Multiply");
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBody, "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">\r\n  <soapenv:Header />\r\n  <soapenv:Body>\r\n    <tem:Multiply>\r\n      <tem:intA>2</tem:intA>\r\n      <tem:intB>3</tem:intB>\r\n    </tem:Multiply>\r\n  </soapenv:Body>\r\n</soapenv:Envelope>");
            soapAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());


            //Set Request body content:
            soapAct.mUseRequestBody = true;
            soapAct.Active = true;
            soapAct.EnableRetryMechanism = false;
            soapAct.ItemName = "Web API Soap";

            context = Context.GetAsContext(soapAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(soapAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(soapAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;
            StringAssert.Contains(rawRequestContent, "POST http://www.dneonline.com/calculator.asmx HTTP/1.1");
            StringAssert.Contains(rawRequestContent, "Content-Type: text/xml; charset=utf-8");
            StringAssert.Contains(rawRequestContent, "Content-Length: 289");
            StringAssert.Contains(rawRequestContent, "Host: www.dneonline.com");
            StringAssert.Contains(rawRequestContent, "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:tem=\"http://tempuri.org/\">");
            StringAssert.Contains(rawRequestContent, "<tem:intA>2</tem:intA>");
            StringAssert.Contains(rawRequestContent, "<tem:intB>3</tem:intB>");
        }

        [TestMethod]  [Timeout(600000)]
        public void WebServices_RawRequestWebAPIRestWithXML()
        {
            //Arrange:
            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://petstore.swagger.io/v2/pet");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.XML.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, ApplicationAPIUtils.eNetworkCredentials.Default.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBody, "<Pet>\r\n  <id>111</id>\r\n  <category>\r\n    <id>222</id>\r\n    <name>Dogs</name>\r\n  </category>\r\n  <name>Dogs</name>\r\n  <photoUrls />\r\n  <tags>\r\n    <id>333</id>\r\n    <name>Rexi</name>\r\n  </tags>\r\n  <status>available</status>\r\n</Pet>");
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString());

            context = Context.GetAsContext(restAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(restAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(restAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;
            StringAssert.Contains(rawRequestContent, "POST https://petstore.swagger.io/v2/pet HTTP/1.1");
            StringAssert.Contains(rawRequestContent, "Accept: application/xml");
            StringAssert.Contains(rawRequestContent, "Content-Type: application/xml; charset=utf-8");
            StringAssert.Contains(rawRequestContent, "Content-Length: 229");
            StringAssert.Contains(rawRequestContent, "Host: petstore.swagger.io");
            StringAssert.Contains(rawRequestContent, "<Pet>");
            StringAssert.Contains(rawRequestContent, "<id>111</id>");
            StringAssert.Contains(rawRequestContent, "<id>222</id>");
            StringAssert.Contains(rawRequestContent, "<name>Rexi</name>");
            StringAssert.Contains(rawRequestContent, "<name>Dogs</name>");
        }

        [TestMethod]  [Timeout(600000)]
        public void WebServices_RawRequestWebAPIRestWithFormData()
        {
            //Arrange
            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            WebAPIKeyBodyValues wakv = new WebAPIKeyBodyValues();
            wakv.Param = "name";
            wakv.Value = "Rexi";
            wakv.ValueType = WebAPIKeyBodyValues.eValueType.Text;
            restAct.RequestKeyValues.Add(wakv);

            WebAPIKeyBodyValues wakv1 = new WebAPIKeyBodyValues();
            wakv1.Param = "status";
            wakv1.Value = "available";
            wakv1.ValueType = WebAPIKeyBodyValues.eValueType.Text;
            restAct.RequestKeyValues.Add(wakv1);

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://petstore.swagger.io/v2/pet/9223372000668906000");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.FormData.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString());

            context = Context.GetAsContext(restAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(restAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(restAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;

            string untilBoundary = rawRequestContent.Substring(0, 143);
            string afterBoundary = rawRequestContent.Substring(180);
            StringAssert.Contains(rawRequestContent, "POST https://petstore.swagger.io/v2/pet/9223372000668906000 HTTP/1.1");
            StringAssert.Contains(rawRequestContent, "Accept: multipart/form-data");
            StringAssert.Contains(rawRequestContent, "Content-Type: multipart/form-data; boundary=");
            StringAssert.Contains(rawRequestContent, "Content-Length: 313");
            StringAssert.Contains(rawRequestContent, "Host: petstore.swagger.io");
            StringAssert.Contains(rawRequestContent, "Content-Disposition: form-data; name=\"name\"");
            StringAssert.Contains(rawRequestContent, "Rexi");
            StringAssert.Contains(rawRequestContent, "Content-Disposition: form-data; name=\"status\"");
            StringAssert.Contains(rawRequestContent, "available");
        }

        [TestMethod]  [Timeout(600000)]
        public void WebServices_RawRequestWebAPIRestWithHeaders()
        {
            //Arrange:
            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            ActInputValue aiv = new ActInputValue();
            aiv.Param = "IncludeRequestDetails";
            aiv.Value = bool.TrueString.ToLower();
            aiv.ValueForDriver = bool.TrueString.ToLower();
            restAct.HttpHeaders.Add(aiv);

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "http://usstlattstl01:8002/api/v1/executions?executionIds=33b2dea1-c24a-494c-97d4-0bd47e59620c");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.FormData.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV10.ToString());

            context = Context.GetAsContext(restAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(restAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(restAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;
            StringAssert.Contains(rawRequestContent, "GET http://usstlattstl01:8002/api/v1/executions?executionIds=33b2dea1-c24a-494c-97d4-0bd47e59620c HTTP/1.0");
            StringAssert.Contains(rawRequestContent, "IncludeRequestDetails: true");
            StringAssert.Contains(rawRequestContent, "Accept: multipart/form-data");
            StringAssert.Contains(rawRequestContent, "Host: usstlattstl01:8002");
        }

        [TestMethod] [Timeout(600000)]
        public void WebServices_RawRequestWebAPIRestWithAuthentication()
        {
            //Arrange:
            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://petstore.swagger.io/v2/user/login");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.BasicAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthUsername, "tom5012");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthPassword, "12345678");
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString());

            context = Context.GetAsContext(restAct.Context);
            if (context != null && context.Runner != null)
            {
                context.Runner.PrepActionValueExpression(restAct, context.BusinessFlow);
            }

            HttpWebClientUtils webAPI = new HttpWebClientUtils();
            webAPI.RequestContstructor(restAct, null, false);
            webAPI.CreateRawRequestContent();

            string rawRequestContent = webAPI.RequestFileContent;
            StringAssert.Contains(rawRequestContent, "GET https://petstore.swagger.io/v2/user/login HTTP/1.1");
            StringAssert.Contains(rawRequestContent, "Authorization: Basic dG9tNTAxMjoxMjM0NTY3OA==");
            StringAssert.Contains(rawRequestContent, "Accept: application/json");
            StringAssert.Contains(rawRequestContent, "Host: petstore.swagger.io");
        }

        [TestMethod]
        [Timeout(600000)]
        public void WebServices_RawResponseWebAPIRest()
        {
            //Arrange
            WebServicesDriver mDriver = new WebServicesDriver(mBF);

            Agent wsAgent = new Agent();
            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(wsAgent);

            mGR.Executor.BusinessFlows.Add(mBF);

            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Web API REST";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();
            Context context = new Context();
            context.Runner = mGR.Executor;
            restAct.Context = context;

            //Build REST Request:
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://petstore.swagger.io/v2/pet");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.POST.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.Session.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose, Boolean.FalseString);
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.UseLegacyJSONParsing, Boolean.FalseString);
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.RequestBody, "{\r\n  \"id\": 55,\r\n  \"category\": {\r\n    \"id\": 0,\r\n    \"name\": \"string\"\r\n  },\r\n  \"name\": \"{CS Exp=System.Environment.UserName}\",\r\n  \"photoUrls\": [\r\n    \"string\"\r\n  ],\r\n  \"tags\": [\r\n    {\r\n      \"id\": 0,\r\n      \"name\": \"string\"\r\n    }\r\n  ],\r\n  \"status\": \"available\"\r\n}");
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV10.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton,ApplicationAPIUtils.eNetworkCredentials.Default.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            restAct.AddNewReturnParams = true;

            mBF.Activities[0].Acts.Add(restAct);

            mDriver.StartDriver();
            mGR.Executor.RunRunner();

            Assert.AreEqual(restAct.ReturnValues[0].Actual, "OK");
        }

        [TestMethod]  [Timeout(600000)]
        public void WebServices_WebAPIRest()
        {
            //Arrange
            WebServicesDriver mDriver = new WebServicesDriver(mBF);

            Agent wsAgent = new Agent();
            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(wsAgent);

            mGR.Executor.BusinessFlows.Add(mBF);

            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Web API REST";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActWebAPIRest restAct = new ActWebAPIRest();

            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.EndPointURL, "https://jsonplaceholder.typicode.com/posts/1");
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.RequestType, ApplicationAPIUtils.eRequestType.GET.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.ResponseContentType, ApplicationAPIUtils.eContentType.JSon.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.SecurityType, ApplicationAPIUtils.eSercurityType.None.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());
            restAct.AddOrUpdateInputParamValue(ActWebAPIRest.Fields.CookieMode, ApplicationAPIUtils.eCookieMode.None.ToString());

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";
            restAct.AddNewReturnParams = true;

            mBF.Activities[0].Acts.Add(restAct);

            mDriver.StartDriver();
            mGR.Executor.RunRunner();

            Assert.AreEqual(restAct.ReturnValues[0].Actual, "OK");
        }       




        [TestMethod]  [Timeout(60000)]
        public void TestXMLReader()
        {

            string xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:blz=""http://thomas-bayer.com/blz/"">
               <soapenv:Header/>
               <soapenv:Body>
                  <blz:getBank>
                     <blz:blz>46451012</blz:blz>
                  </blz:getBank>
               </soapenv:Body>
            </soapenv:Envelope>";

            MemoryStream InnerXml = new MemoryStream(Encoding.GetEncoding("utf-8").GetBytes(xml));
            XmlReader rdr = XmlReader.Create(InnerXml);
            string Elm = "";
            string sPath = "";
            int depth = 0;
            ArrayList ls = new ArrayList();
            while (rdr.Read())
            {
                if (rdr.NodeType == XmlNodeType.Element)
                {
                    Elm = rdr.Name;
                    if (ls.Count <= rdr.Depth)
                        ls.Add(Elm);
                    else
                        ls[rdr.Depth] = Elm;
                    depth = rdr.Depth;
                }

                if (rdr.NodeType == XmlNodeType.Text)
                {
                    // soup req contains sub xml, so parse them 
                    if (rdr.Value.StartsWith("<?xml"))
                    {
                        XmlDocument xmlnDoc = new XmlDocument();
                        xmlnDoc.LoadXml(rdr.Value);
                        //SetOutput(xmlnDoc);
                    }
                    else
                    {
                        sPath = "/" + string.Join("/", ls.ToArray().Take(rdr.Depth));
                        
                    
                    }
                }

            }
        }
        private string getXML()
        {
            //            string xml = @"
            //<?xml version=""1.0"" encoding=""utf-8""?>
            //<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
            //  <soap:Body>
            //    <CreateCustomer xmlns=""http://localhost:52746/WebService1.asmx/CreateCustomer"">
            //      <FirstName>David</FirstName>
            //      <LastName>Cohen</LastName>
            //    </CreateCustomer>
            //  </soap:Body>
            //</soap:Envelope>
            //";
            string xml = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body><NumberToWords xmlns = ""http://www.dataaccess.com/webservicesserver/"" ><ubiNum> 10000 </ubiNum></NumberToWords></soap:Body></soap:Envelope> ";

            return xml;

        }

        [TestMethod]
        [Timeout(10000)]
        public void WebServices_SoapWrapperActionTest()
        {
            WebServicesDriver mDriver = new WebServicesDriver(mBF);

            Agent wsAgent = new Agent();
            AgentOperations agentOperations = new AgentOperations(wsAgent);
            wsAgent.AgentOperations = agentOperations;

            wsAgent.DriverType = Agent.eDriverType.WebServices;
            ((AgentOperations)wsAgent.AgentOperations).Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(wsAgent);

            mGR.Executor.BusinessFlows.Add(mBF);

            Activity Activity2 = new Activity();
            Activity2.Active = true;
            Activity2.ActivityName = "Soap UI Wrapper action";
            Activity2.CurrentAgent = wsAgent;
            mBF.Activities.Add(Activity2);

            ActSoapUI actSoapUi = new ActSoapUI();

            var xmlFilePath = TestResources.GetTestResourcesFile(@"XML"+Path.DirectorySeparatorChar+"calculator_soapui_project.xml");
            actSoapUi.AddNewReturnParams=true;
            actSoapUi.AddOrUpdateInputParamValue(ActSoapUI.Fields.ImportFile, xmlFilePath);
            
            mBF.Activities[0].Acts.Add(actSoapUi);

            Assert.AreEqual(1, actSoapUi.ActInputValues.Count);
            Assert.AreEqual(xmlFilePath, actSoapUi.ActInputValues[0].Value.ToString());

        }

        [TestMethod]
        [Timeout(60000)]
        public void SoapUICreateCopyTest()
        {
            //Arrange
            ActSoapUI actSoapUI = new ActSoapUI();
            actSoapUI.Description = "Soap Wrapper acttion test ";
            
            var xmlFilePath = TestResources.GetTestResourcesFile(@"XML"+ Path.DirectorySeparatorChar + "calculator_soapui_project.xml");
            
            actSoapUI.AddOrUpdateInputParamValue(ActSoapUI.Fields.ImportFile, xmlFilePath);
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.UIrelated, "False");
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.ImportFile, "True");
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.IgnoreValidation, "False");
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.TestCasePropertiesRequiered, "False");
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.AddXMLResponse, "False");
            actSoapUI.GetOrCreateInputParam(ActSoapUI.Fields.TestCasePropertiesRequieredControlEnabled, "False");


            //Act
            var duplicateAct = (ActSoapUI)actSoapUI.CreateCopy(true);

            //Assert
            Assert.AreEqual(actSoapUI.ActInputValues.Count, duplicateAct.ActInputValues.Count);
            Assert.AreEqual(actSoapUI.ActInputValues[1].Value.ToString(), duplicateAct.ActInputValues[1].Value.ToString());

        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void LegacyWebServiceToNewWebApiSoap_Converter_Test()
        {
            Activity oldActivity = new Activity();
            oldActivity.Active = true;
            oldActivity.ActivityName = "Legacy Web Service activity";
            oldActivity.CurrentAgent = wsAgent;
            mBF.Activities.Add(oldActivity);

            ActWebService actLegacyWebService = new ActWebService();

            actLegacyWebService.AddOrUpdateInputParamValue(ActWebService.Fields.URL, @"http://webservices.oorsprong.org/websamples.countryinfo/CountryInfoService.wso");
            actLegacyWebService.AddOrUpdateInputParamValue(ActWebService.Fields.SOAPAction, @"");

            var xmlFileNamePath = TestResources.GetTestResourcesFile(@"XML"+ Path.DirectorySeparatorChar + "stock.xml");
            actLegacyWebService.AddOrUpdateInputParamValue(ActWebService.Fields.XMLfileName, xmlFileNamePath);
            
            actLegacyWebService.FileName = "Web Service Action";
            actLegacyWebService.FilePath = "Web Service Action";
            actLegacyWebService.Active = true;
            actLegacyWebService.AddNewReturnParams = true;
           
            mBF.Activities[0].Acts.Add(actLegacyWebService);
            mDriver.StartDriver();
            mGR.Executor.RunRunner();

            Assert.AreNotEqual(0, actLegacyWebService.ReturnValues.Count);
            Assert.AreEqual("Åland Islands", actLegacyWebService.ReturnValues.FirstOrDefault(x =>x.Param == @"m:sName").Actual);

            //Convert the legacy action
            Activity newActivity = new Activity() { Active = true };
            newActivity.ActivityName = "New - " + oldActivity.ActivityName;
            newActivity.CurrentAgent = wsAgent;
            mBF.Activities.Add(newActivity);

            Act newAction = ((IObsoleteAction)actLegacyWebService).GetNewAction();
            newAction.AddNewReturnParams = true;
            newAction.Active = true;
            newAction.ItemName = "Converted webapisoap action";
            newActivity.Acts.Add((ActWebAPISoap)newAction);
            mBF.Activities[1].Acts.Add(newAction);

            //Assert converted action
            Assert.AreNotEqual(0, newAction.ReturnValues.Count);
            Assert.AreEqual("Åland Islands", newAction.ReturnValues.FirstOrDefault(x => x.Param == @"m:sName").Actual);

            //Run newAction
            mGR.Executor.RunRunner();
            
            //assert newaction
            Assert.AreNotEqual(0, newAction.ReturnValues.Count);
            Assert.AreEqual("Åland Islands", newAction.ReturnValues.FirstOrDefault(x => x.Param == @"m:sName").Actual);
        }

        [TestMethod]
        [Timeout(600000)]
        public void LegacyRestActionToNewWebApiRest_Converter_Test()
        {
            Activity oldActivity = new Activity();
            oldActivity.Active = true;
            oldActivity.ActivityName = "Legacy Rest Service activity";
            oldActivity.CurrentAgent = wsAgent;
            mBF.Activities.Add(oldActivity);

            ActREST actLegacyRestService = new ActREST();
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.RequestType, ActREST.eRequestType.GET.ToString());
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.ReqHttpVersion, ActREST.eHttpVersion.HTTPV10.ToString());
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.ContentType, ActREST.eContentType.JSon.ToString());
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.CookieMode, ActREST.eCookieMode.None.ToString());
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.SecurityType, ActREST.eSercurityType.None.ToString());
            actLegacyRestService.AddOrUpdateInputParamValue(ActREST.Fields.EndPointURL, @"https://jsonplaceholder.typicode.com/posts/1");

            actLegacyRestService.FileName = "Web Rest Action";
            actLegacyRestService.FilePath = "Web Rest Action";
            actLegacyRestService.Active = true;
            actLegacyRestService.AddNewReturnParams = true;

            mBF.Activities[0].Acts.Add(actLegacyRestService);
            mDriver.StartDriver();
            mGR.Executor.RunRunner();

            //Assert old action
            Assert.AreNotEqual(0, actLegacyRestService.ReturnValues.Count);
            var expected = actLegacyRestService.ReturnValues.FirstOrDefault(x =>x.Actual == "OK");
            Assert.AreNotEqual(null, expected);

            //Convert the legacy action
            Activity newActivity = new Activity() { Active = true };
            newActivity.ActivityName = "New - " + oldActivity.ActivityName;
            newActivity.CurrentAgent = wsAgent;
            mBF.Activities.Add(newActivity);

            Act newAction = ((IObsoleteAction)actLegacyRestService).GetNewAction();
            newAction.AddNewReturnParams = true;
            newAction.Active = true;
            newAction.ItemName = "Converted webapiRest action";
            newActivity.Acts.Add((ActWebAPIRest)newAction);
            mBF.Activities[1].Acts.Add(newAction);

            //Assert converted action
            Assert.AreNotEqual(0, newAction.ReturnValues.Count);
            var expected1 = newAction.ReturnValues.FirstOrDefault(x => x.Actual == "OK");
            Assert.AreNotEqual(null, expected1);

            //Run newAction
            mGR.Executor.RunRunner();

            //assert newaction
            Assert.AreNotEqual(0, newAction.ReturnValues.Count);
            var expected2 = newAction.ReturnValues.FirstOrDefault(x => x.Actual == "OK");
            Assert.AreNotEqual(null, expected2);
        }

        [TestMethod]
        [Timeout(600000)]
        public void EnhanceSecurityProtocalOnVersionUpgradeCheck()
        {
            var targetFrameworkAttribute = typeof(WebServicesDriver).Assembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false)
                                           .SingleOrDefault();
            var versionName = ((System.Runtime.Versioning.TargetFrameworkAttribute)targetFrameworkAttribute).FrameworkName;
            string versionStr = string.Empty;

            int versionNumber;

            for (int i = 0; i < versionName.Length; i++)
            {
                if (Char.IsDigit(versionName[i]))
                    versionStr += versionName[i];
            }

            if (versionStr.Length > 0)
            {
                versionNumber = Convert.ToInt32(versionStr);

                if (versionNumber > 461)
                {
                    throw new NotImplementedException("Enhance SecurityProtocolType code in WebService driver asper dotnet framework version");
                }
            }
               

        }
    }
}
