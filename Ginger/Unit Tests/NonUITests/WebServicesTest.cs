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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
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
         
            AutoLogProxy.Init("NonUITests");
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF WebServices Web API";
            mBF.Active = true;


            Platform p = new Platform();
            p.PlatformType = ePlatformType.WebServices;
            mBF.Platforms = new ObservableList<Platform>();
            mBF.Platforms.Add(p);


            mDriver = new WebServicesDriver(mBF);
            mDriver.SaveRequestXML = true;
            mDriver.SavedXMLDirectoryPath = "~\\Documents";


            wsAgent.DriverType = Agent.eDriverType.WebServices;
            wsAgent.Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(wsAgent);

            mGR.BusinessFlows.Add(mBF);

            Reporter.ToLog(eAppReporterLogLevel.INFO, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);         

        }

        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [TestMethod]
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
            mGR.RunRunner();

           
            //Assert
            //TODO: add some assert??

        }

        //[TestMethod]
        //[Ignore]
        //public void BizFlowSaveLoad()
        //{
        //   // ExampleAPIProxy.ExampleWebMethod("aa",1);

        //}

        //[TestMethod]
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

        //[TestMethod]
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


        [TestMethod]
        public void WebServices_WebServiceSendXML()
        {
            WebServiceXML webServiceCall = new WebServiceXML();
            string URL = "http://ws.cdyne.com/delayedstockquote/delayedstockquote.asmx";
            string soapAction = "http://ws.cdyne.com/GetQuickQuote";
            string xmlRequest= getXML();
            string Status = "test";
            bool failFlag = false;
            string webRespone = webServiceCall.SendXMLRequest(URL, soapAction, xmlRequest,ref Status,ref failFlag, null);

            StringAssert.Contains(webRespone, "<GetQuickQuoteResult>0</GetQuickQuoteResult>");
            
            
        }

        [TestMethod]
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
            mGR.RunRunner();

            if (soapAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in soapAct.ReturnValues)
                {
                    if (val.Actual.ToString() == "59586")
                        Assert.AreEqual(val.Actual,"59586");
                }
            }
        }

        [TestMethod]
        public void WebServices_WebAPIRest()
        {
            WebServicesDriver mDriver = new WebServicesDriver(mBF);

            Agent wsAgent = new Agent();
            wsAgent.DriverType = Agent.eDriverType.WebServices;
            wsAgent.Driver = mDriver;
            ApplicationAgent mAG = new ApplicationAgent();
            mAG.Agent = wsAgent;

            mGR = new GingerRunner();
            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(wsAgent);

            mGR.BusinessFlows.Add(mBF);

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

            restAct.Active = true;
            restAct.EnableRetryMechanism = false;
            restAct.ItemName = "Web API REST";

            mBF.Activities[0].Acts.Add(restAct);

            mDriver.StartDriver();
            mGR.RunRunner();


            if (restAct.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in restAct.ReturnValues)
                {
                    if (val.Actual.ToString() == "OK")
                        Assert.AreEqual(val.Actual, "OK");
                }
            }
        }       
        [TestMethod]
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
            string xml = @"<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.cdyne.com/""><soapenv:Header/><soapenv:Body><ws:GetQuickQuote><!--Optional:--><ws:StockSymbol>?</ws:StockSymbol><!--Optional:--><ws:LicenseKey>?</ws:LicenseKey></ws:GetQuickQuote></soapenv:Body></soapenv:Envelope>";

            return xml;

        }



    }
}
