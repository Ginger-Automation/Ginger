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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.WebAPI;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;

namespace GingerCore.Drivers.WebServicesDriverLib
{
    public class WebServicesDriver : DriverBase, IVirtualDriver
    {
        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Show Driver Window On Launch")]
        public bool ShowDriverWindowOnLaunch { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Save Request XML")]
        public bool SaveRequestXML { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Save Response XML")]
        public bool SaveResponseXML { get; set; }

        [UserConfigured]
        [UserConfiguredDefault(@"~\Documents")]
        [UserConfiguredDescription("Path to save XMLs")]
        public string SavedXMLDirectoryPath { get; set; }

        [UserConfigured]
        [UserConfiguredDescription(@"Related only to SoapUI | SoapUI installation folder e.g. C:\Program Files\SmartBear\SoapUI-5.3.0 | SoapUI version supported - v5.0.0 and above")]
        public string SoapUIDirectoryPath { get; set; }

        [UserConfigured]
        [UserConfiguredDefault(@"~\Documents\WebServices\SoapUI\ExecutionOutputs")]
        [UserConfiguredDescription("Related only to SoapUI | Execution Output Folder Which will contain the respond")]
        public string SoapUIExecutionOutputsDirectoryPath { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Related only to SoapUI | soapui-settings.xml file to use")]
        public string SoapUISettingFile { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Related only to SoapUI | soapui-settings.xml file's password")]
        public string SoapUISettingFilePassword { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI-Project's password")]
        public string SoapUIProjectPassword { get; set; }
        
        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Related only to SoapUI | Run SoapUI Process as Admin")]
        public bool RunSoapUIProcessAsAdmin { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI Process Redirect Standard Error")]
        public bool SoapUIProcessRedirectStandardError { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI Process Redirect Standard Output")]
        public bool SoapUIProcessRedirectStandardOutput { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI Process Use Shell Execute")]
        public bool SoapUIProcessUseShellExecute { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI Process Window Style Hidden")]
        public bool SoapUIProcessWindowStyle { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Related only to SoapUI | SoapUI Process Create No Window")]
        public bool SoapUIProcessCreateNoWindow { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Proxy Settings | Host:Port Example: [sub-domain.]example.com:8080")]
        public string WebServicesProxy { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use Proxy Server Settings | Set to true in order to use local Proxy Server settings, if set to true configured Agent 'Proxy Settings' will be avoided. ")]
        public bool UseServerProxySettings { get; set; }

        private bool mIsDriverWindowLaunched
        {
            get
            {
                if (mDriverWindow != null)
                    return true;
                else return false;
            }
        }

        IWebserviceDriverWindow mDriverWindow;
        private ActWebService mActWebService;
        public ActWebAPIBase mActWebAPI;
        public string mRawResponse;
        public string mRawRequest;
        private HttpWebClientUtils mWebAPI;

        public override bool IsSTAThread()
        {
            return ShowDriverWindowOnLaunch;
        }

        public WebServicesDriver(BusinessFlow BF)
        {
            BusinessFlow = BF;
        }

        public enum eSecurityType
        {
            None,
            Ssl3,
            Tls,
            Tls11,
            Tls12
        }

        [UserConfigured]
        [UserConfiguredDefault("None")]
        [UserConfiguredDescription("Default Security Setting Applicable only for legacy webserver  choose from None, Ssl3, Tls, Tls11, Tls12")]
        public string SecurityType { get; set; }

        private eSecurityType mSecurityType
        {
            get
            {
                return (eSecurityType)Enum.Parse(typeof(eSecurityType), SecurityType, true);
            }
        }
        public override void StartDriver()
        {
            if (ShowDriverWindowOnLaunch)
                CreateSTA(ShowDriverWindow);
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        public void LauncDriverWindow()
        {
            CreateSTA(mDriverWindow.ShowDriverWindow);
        }


        private void ShowDriverWindow()
        {
            mDriverWindow = WorkSpace.Instance.TargetFrameworkHelper.GetWebserviceDriverWindow(BusinessFlow);
            mDriverWindow.ShowDriverWindow();

            this.Dispatcher = mDriverWindow.GingerDispatcher;
            WorkSpace.Instance.TargetFrameworkHelper.DispatcherRun();
        }

        public override void CloseDriver()
        {
            try
            {
                if (mDriverWindow != null)
                {
                    mDriverWindow.Close();
                    mDriverWindow = null;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close Web Services Driver - " + ex.Message);
            }
        }

        public override Act GetCurrentElement()
        {
            return null;
        }

        public void CreateRawRequest(Act act)
        {
            HttpWebClientUtils WebAPI = new HttpWebClientUtils();

            if (act is ActWebAPISoap || act is ActWebAPIRest)
            {
                if (WebAPI.RequestContstructor((ActWebAPIBase)act, WebServicesProxy, UseServerProxySettings))
                {
                    WebAPI.SaveRequest(SaveRequestXML, SavedXMLDirectoryPath);

                }
            }
            else if (act is ActWebAPIModel ActWAPIM)
            {
                //pull pointed API Model
                ApplicationAPIModel AAMB = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.Guid == ((ActWebAPIModel)act).APImodelGUID).FirstOrDefault();
                if (AAMB == null)
                {
                    act.Error = "Failed to find the pointed API Model";
                    act.ExInfo = string.Format("API Model with the GUID '{0}' was not found", ((ActWebAPIModel)act).APImodelGUID);
                    return;
                }

                //init matching real WebAPI Action
                ActWebAPIBase actWebAPI = null;
                if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.REST)
                {
                    actWebAPI = ActWAPIM.CreateActWebAPIREST((ApplicationAPIModel)AAMB, ActWAPIM);
                }
                else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                {
                    actWebAPI = ActWAPIM.CreateActWebAPISOAP((ApplicationAPIModel)AAMB, ActWAPIM);
                }
                else
                {
                    throw new Exception("The Action from type '" + act.GetType().ToString() + "' is unknown/Not Implemented by the Driver - " + this.GetType().ToString());
                }

                if (WebAPI.RequestContstructor(actWebAPI, WebServicesProxy, UseServerProxySettings))
                {
                    WebAPI.SaveRequest(SaveRequestXML, SavedXMLDirectoryPath);
                }
            }
            mRawRequest = WebAPI.RequestFileContent;
        }



        public override void RunAction(Act act)
        {
            //TODO: add func to Act + Enum for switch

            if (act is ActWebService)
            {
                mActWebService = (ActWebService)act;
                string ReqXML = String.Empty;
                //FileStream ReqXMLStream = System.IO.File.OpenRead(mActWebService.XMLfileName.ValueForDriver.Replace(@"~\", mActWebService.SolutionFolder));
                FileStream ReqXMLStream = System.IO.File.OpenRead(WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(mActWebService.XMLfileName.ValueForDriver));

                using (StreamReader reader = new StreamReader(ReqXMLStream))
                {
                    ReqXML = reader.ReadToEnd();
                }

                string XMLwithValues = SetDynamicValues(mActWebService, ReqXML);
                int timeOut = string.IsNullOrEmpty(Convert.ToString(act.Timeout)) ? 350 : ((int)act.Timeout) * 1000;
                RunWebService(XMLwithValues, timeOut);
            }
            else if (act is ActSoapUI)
            {
                if (string.IsNullOrEmpty(SoapUIDirectoryPath))
                    throw new Exception("SoapUI Directory Path has not been set to the Agent");
                runSoapCommand(act);
            }
            else if (act is ActWebAPISoap || act is ActWebAPIRest)
            {
                mActWebAPI = (ActWebAPIBase)act;
                HandleWebApiRequest((ActWebAPIBase)act);
            }
            else if (act is ActWebAPIModel ActWAPIM)
            {
                if (Reporter.AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Start Execution");
                }

                //pull pointed API Model
                ApplicationAPIModel AAMB = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Where(x => x.Guid == ((ActWebAPIModel)act).APImodelGUID).FirstOrDefault();
                if (AAMB == null)
                {
                    act.Error = "Failed to find the pointed API Model";
                    act.ExInfo = string.Format("API Model with the GUID '{0}' was not found", ((ActWebAPIModel)act).APImodelGUID);
                    return;
                }

                //init matching real WebAPI Action
                ActWebAPIBase actWebAPI = null;
                if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.REST)
                {
                    actWebAPI = ActWAPIM.CreateActWebAPIREST((ApplicationAPIModel)AAMB, ActWAPIM);
                }
                else if (AAMB.APIType == ApplicationAPIUtils.eWebApiType.SOAP)
                {
                    actWebAPI = ActWAPIM.CreateActWebAPISOAP((ApplicationAPIModel)AAMB, ActWAPIM);
                }

                if (Reporter.AppLoggingLevel == eAppReporterLoggingLevel.Debug)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "ActWebAPIBase created successfully");
                }

                //Execution
                mActWebAPI = actWebAPI;
                HandleWebApiRequest(actWebAPI);

                //Post Execution Copy execution result fields from actWebAPI to ActWebAPIModel (act)
                CopyExecutionAttributes(act, actWebAPI);



            }
            else if (act is ActScreenShot)
            {
            }
            else
            {
                throw new Exception("The Action from type '" + act.GetType().ToString() + "' is unknown/Not Implemented by the Driver - " + this.GetType().ToString());
            }

        }

        private string ReplacePlaceHolderParameneterWithActual(string ValueBeforeReplacing, ObservableList<EnhancedActInputValue> APIModelDynamicParamsValue)
        {
            if (string.IsNullOrEmpty(ValueBeforeReplacing))
                return string.Empty;

            foreach (EnhancedActInputValue EAIV in APIModelDynamicParamsValue)
                ValueBeforeReplacing = ValueBeforeReplacing.Replace(EAIV.Param, EAIV.ValueForDriver);

            return ValueBeforeReplacing;
        }

        private void CopyExecutionAttributes(Act act, ActWebAPIBase actWebAPI)
        {
            act.Error = actWebAPI.Error;
            act.ExInfo = actWebAPI.ExInfo;
            act.RawResponseValues = actWebAPI.RawResponseValues;
        }
        
       
        

        private void HandleWebApiRequest(ActWebAPIBase act)
        {
            mWebAPI = new HttpWebClientUtils();

            //Call for Request Construction
            if (mWebAPI.RequestContstructor(act, WebServicesProxy, UseServerProxySettings))
            {

                mWebAPI.SaveRequest(SaveRequestXML, SavedXMLDirectoryPath);
                mRawRequest = mWebAPI.RequestFileContent;
                Reporter.ToLog(eLogLevel.DEBUG, "RequestContstructor passed successfully");

                if (mWebAPI.SendRequest())
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "SendRequest passed successfully");

                    //Call for  response validation
                    bool dontFailActionOnBadResponse = false;
                    Boolean.TryParse(act.GetInputParamCalculatedValue(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose), out dontFailActionOnBadResponse);
                    if (!dontFailActionOnBadResponse)
                        mWebAPI.ValidateResponse();

                    Reporter.ToLog(eLogLevel.DEBUG, "ValidateResponse passed successfully");

                    mWebAPI.SaveResponseToFile(SaveResponseXML, SavedXMLDirectoryPath);
                    mRawResponse = mWebAPI.ResponseFileContent;
                    mWebAPI.HandlePostExecutionOperations();
                    //Parse response
                    mWebAPI.ParseRespondToOutputParams();

                    Reporter.ToLog(eLogLevel.DEBUG, "ParseRespondToOutputParams passed successfully");
                }
            }
        }

        private void runSoapCommand(Act act)
        {
            SoapUIUtils soapUIUtils = new SoapUIUtils(act, SoapUIDirectoryPath, SoapUIExecutionOutputsDirectoryPath, SoapUISettingFile, SoapUISettingFilePassword, SoapUIProjectPassword, RunSoapUIProcessAsAdmin, SoapUIProcessRedirectStandardError, SoapUIProcessRedirectStandardOutput, SoapUIProcessUseShellExecute, SoapUIProcessWindowStyle, SoapUIProcessCreateNoWindow);
            string command = string.Empty;

            //return the command string and checking if it been populated successfully
            if (soapUIUtils.Command(ref command))
                //checking if the process got started successfully
                if (soapUIUtils.StartProcess(command))
                {
                    //validating run status and populating the Error field with the failure
                    soapUIUtils.Validation();
                    //Extracting the text to output grid
                    TestToOutput(soapUIUtils, act);

                }
        }

        private void TestToOutput(SoapUIUtils soapUIUtils, Act act)
        {
            Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>();
            dict = soapUIUtils.RequestsAndResponds();
            foreach (KeyValuePair<string, List<string>> kpr in dict)
            {

                if (!string.IsNullOrEmpty(kpr.Value[1]))
                {
                    string requestQouteFixed = kpr.Value[1].Replace("\"", "");
                    requestQouteFixed = requestQouteFixed.Replace("\0", "");
                    act.AddOrUpdateReturnParamActual(kpr.Value[0] + "-Request", requestQouteFixed);
                }
                if (!string.IsNullOrEmpty(kpr.Value[2]))
                {
                    string responseQouteFixed = string.Empty;
                   
                    //if response is JSON format then not replace double quotes
                    if (kpr.Value[2].IndexOf("{") > -1)
                        responseQouteFixed = kpr.Value[2];
                    else
                        responseQouteFixed = kpr.Value[2].Replace("\"", "");
                   

                    responseQouteFixed = responseQouteFixed.Replace("\0", "");
                       
                    act.AddOrUpdateReturnParamActual(kpr.Value[0] + "-Response", responseQouteFixed);
                    if (((ActSoapUI)act).AddXMLResponse_Value)
                    {
                        string fileContent = kpr.Value[2];
                        ObservableList<ActReturnValue> ReturnValues = null;
                        if (APIConfigurationsDocumentParserBase.IsValidJson(fileContent))
                        {
                            ReturnValues = JSONTemplateParser.ParseJSONResponseSampleIntoReturnValues(fileContent);
                            foreach (ActReturnValue ReturnValue in ReturnValues)
                            {
                                act.ReturnValues.Add(ReturnValue);
                            }
                        }
                        else if (APIConfigurationsDocumentParserBase.IsValidXML(fileContent))
                        {
                            XmlDocument xmlDoc1 = new XmlDocument();
                            xmlDoc1.LoadXml(kpr.Value[2]);

                            List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem> outputTagsList1 = new List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem>();
                            outputTagsList1 = General.GetXMLNodesItems(xmlDoc1);
                            foreach (Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem outputItem in outputTagsList1)
                            {
                                act.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(kpr.Value[4]))
                {
                    string messageQouteFixed = kpr.Value[4].Replace("\"", "");
                    messageQouteFixed = messageQouteFixed.Replace("\0", "");
                    act.AddOrUpdateReturnParamActual(kpr.Value[0] + "-Message", kpr.Value[4]);
                }
                if (!string.IsNullOrEmpty(kpr.Value[5]))
                {
                    string propertiesQouteFixed = kpr.Value[4].Replace("\"", "");
                    propertiesQouteFixed = propertiesQouteFixed.Replace("\0", "");
                    act.AddOrUpdateReturnParamActual(kpr.Value[0] + "-Properties", kpr.Value[5]);
                }

                act.RawResponseValues = mWebAPI.ResponseFileContent;
                act.AddOrUpdateReturnParamActual("Raw Request: ", mWebAPI.RequestFileContent);
                act.AddOrUpdateReturnParamActual("Raw Response: ", mWebAPI.ResponseFileContent);
            }

            Dictionary<List<string>, List<string>> dictValues = new Dictionary<List<string>, List<string>>();
            dictValues = soapUIUtils.OutputParamAndValues();
            foreach (KeyValuePair<List<string>, List<string>> Kpr in dictValues)
            {
                int index = 0;
                if (Kpr.Key.Count() != 0 && Kpr.Value.Count() != 0)
                    foreach (string property in Kpr.Key)
                    {
                        act.AddOrUpdateReturnParamActual(Kpr.Key[index], Kpr.Value[index]);
                        index++;
                    }
            }
        }

        private string SetDynamicValues(ActWebService AWS, string XML)
        {
            string NewXML = XML;
            foreach (ActInputValue AIV in AWS.DynamicXMLElements)
            {
                // We start with simple place holder replace
                // Later on we will add locate by: XPath and more

                //todo set VFD!? or is it already done!!
                string NewValue = AIV.ValueForDriver;

                //TODO: handle if not found what to replace
                NewXML = NewXML.Replace(AIV.Param, NewValue);
            }
            return NewXML;
        }

        private bool RunWebService(string mRequest, int timeout)
        {
            bool FailFlag = false;
            try
            {
                switch (mSecurityType)
                {
                    case eSecurityType.Ssl3:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                        break;

                    case eSecurityType.Tls:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                        break;

                    case eSecurityType.Tls11:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                        break;
                    case eSecurityType.Tls12:
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        break;

                }
                string URL = mActWebService.URL.ValueForDriver;
                string SOAPAction = mActWebService.SOAPAction.ValueForDriver;


                if (mIsDriverWindowLaunched)
                {


                    mDriverWindow.UpdateRequestParams(URL, mActWebService.SOAPAction.Value, mRequest);
                }

                SetStatus("Preparing new Web Service data");
                WebServiceXML c1 = new WebServiceXML();
                c1.ServiceConnectionTimeOut = timeout;
                string ResponseCode = null;

                SetStatus("Sending Request XML, Length=" + mRequest.Length);
                Stopwatch st = new Stopwatch();
                st.Start();
                string resp;
                try
                {
                    if (mActWebService.URLUser.Value == "" || mActWebService.URLUser.Value == null)
                    {
                        resp = c1.SendXMLRequest(URL, SOAPAction, mRequest, ref ResponseCode, ref FailFlag);

                        if (FailFlag == true)
                        {
                            mActWebService.Error = "Response parsing failed. Error:" + ResponseCode;
                        }
                    }
                    else
                    {
                        if (mActWebService.URLDomain.Value != "" && mActWebService.URLDomain.Value != null)
                        {
                            NetworkCredential CustCreds = new NetworkCredential("", "", "");
                            CustCreds.UserName = mActWebService.URLUser.Value;
                            CustCreds.Password = mActWebService.URLPass.Value;
                            CustCreds.Domain = mActWebService.URLDomain.Value;
                            resp = c1.SendXMLRequest(URL, SOAPAction, mRequest, ref ResponseCode, ref FailFlag, CustCreds);

                            if (FailFlag == true)
                            {
                                mActWebService.Error = "Response parsing failed. Error:" + ResponseCode;
                            }
                        }
                        else //use current domain
                        {
                            NetworkCredential CustCreds = new NetworkCredential("", "", "");
                            CustCreds.UserName = mActWebService.URLUser.Value;
                            CustCreds.Password = mActWebService.URLPass.Value;
                            resp = c1.SendXMLRequest(URL, SOAPAction, mRequest, ref ResponseCode, ref FailFlag, CustCreds);

                            if (FailFlag == true)
                            {
                                mActWebService.Error = "Response parsing failed. Error:" + ResponseCode;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    mActWebService.Error += "Failed to execute send xml request";
                    mActWebService.ExInfo += e.Message;
                    return false;
                }

                if (SaveRequestXML)
                {
                    mActWebService.AddOrUpdateReturnParamActual("Saved Request File Name", createRequestOrResponseXML("Request", mRequest));
                }


                if (mIsDriverWindowLaunched)
                {
                    SetStatus("Received response, Length=" + resp.Length + ", Elapsed (ms)= " + st.ElapsedMilliseconds);
                    mDriverWindow.UpdateResponseTextBox(ResponseCode);
              
                    mDriverWindow.UpdateResponseTextBox(ResponseCode);
                }

                mActWebService.AddOrUpdateReturnParamActual("FullReponseXML", resp);
                
                XmlDocument xmlReqDoc = new XmlDocument();
                xmlReqDoc.LoadXml(resp);
                if (mIsDriverWindowLaunched)
                {
                    mDriverWindow.updateResponseXMLText(ConvertHTMLTags(resp));
                  
                }

                if (SaveResponseXML)
                {
                    mActWebService.AddOrUpdateReturnParamActual("Saved Response File Name", createRequestOrResponseXML("Response", resp));
                }
                try
                {
                    List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem> outputList = new List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem>();
                    outputList = Amdocs.Ginger.Common.GeneralLib.General.GetXMLNodesItems(xmlReqDoc);
                    foreach (Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem outputItem in outputList)
                    {
                        mActWebService.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                    }
                }
                catch (Exception e)
                {
                    mActWebService.Error += "Failed to update the response to output values";
                    mActWebService.ExInfo += e.Message;
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {      
                mActWebService.Error = "Failed to complete the WebServices action.";
                mActWebService.ExInfo = e.Message;
                return false;
            }
        }

        public String createRequestOrResponseXML(string fileType, string fileContent)
        {
            XmlDocument xmlDoc = new XmlDocument();
            String fileName = null;
            xmlDoc.LoadXml(fileContent);
            string xmlFilesDir = "";
            try
            {
                //xmlFilesDir = SavedXMLDirectoryPath.Replace(@"~\", mActWebService.SolutionFolder) + @"\" + fileType + "XMLs";
                xmlFilesDir = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(SavedXMLDirectoryPath) + @"\" + fileType + "XMLs";

                if (!Directory.Exists(xmlFilesDir))
                    Directory.CreateDirectory(xmlFilesDir);
                String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss") + "_" + Guid.NewGuid();
                xmlDoc.Save(xmlFilesDir + @"\" + mActWebService.Description + "_" + timeStamp + "_" + fileType + ".xml");
                fileName = mActWebService.Description + "_" + timeStamp + "_" + fileType + ".xml";
            }
            catch (Exception e)
            {                
                Reporter.ToUser(eUserMsgKey.FailedToCreateRequestResponse, e.Message);
            }
            return fileName;
        }

        public void SetStatus(string Status)
        {
            if (mIsDriverWindowLaunched)
            {
                mDriverWindow.UpdateStatusLabel(Status);
               
            }
        }

        public override string GetURL()
        {
            return "TBD";
        }

        
        public override void HighlightActElement(Act act)
        {
        }

        public override ePlatformType Platform
        {
            get
            {
                return ePlatformType.WebServices;
            }
        }

        public override bool IsRunning()
        {
            if (mIsDriverWindowLaunched)
            {
                if (mDriverWindow != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return true;

        }

        private string ConvertHTMLTags(string txt)
        {
            string s = txt.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            return s;
        }

        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
