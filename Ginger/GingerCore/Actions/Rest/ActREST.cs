#region License
/*
Copyright © 2014-2019 European Support Limited

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
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Web;
using System.Xml;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.WebServices;

namespace GingerCore.Actions.REST
{
    public class ActREST : ActWithoutDriver, IObsoleteAction
    {
        public override List<ePlatformType> LegacyActionPlatformsList { get { return Platforms; } }
        // We keep a dictionary of each host and cookies
        // this is in order to avoid creating a special rest driver
        // Since we can have more than one host to send/get rest services
        // it is static so singleton for all objects
        //  static Dictionary<string, string> HostCookies = new Dictionary<string, string>();

        // static CookieCollection SessionCokkies = new CookieCollection();
        static Dictionary<string, Cookie> _sessionCokkiesDic = new Dictionary<string, Cookie>();
        //  static Dictionary<string, CookieCollection> HostCookies = new Dictionary<string, CookieCollection>();

        public new static partial class Fields
        {
            public static string EndPointURL = "EndPointURL";
            public static string RequestType = "RequestType";
            public static string ContentType = "ContentType";
            public static string ResponseContentType = "ResponseContentType";
            public static string RequestBody = "RequestBody";
            public static string TemplateFile = "TemplateFile";
            public static string SaveRequestResponseFolderPath = "SaveRequestResponseFolderPath";
            public static string URLUser = "URLUser";
            public static string URLPass = "URLPass";
            public static string URLDomain = "URLDomain";
            public static string ReqHttpVersion = "ReqHttpVersion";
            public static string CookieMode = "CookieMode";
            public static string RestRequestSave = "RestRequestSave";
            public static string RestResponseSave = "RestResponseSave";
            public static string UseTemplateFile = "UseTemplateFile";
            public static string UseRequestBody = "UseRequestBody";
            public static string DoNotFailActionOnBadRespose = "DoNotFailActionOnBadRespose";
            public static string AcceptAllSSLCertificate = "AcceptAllSSLCertificate";
            public static string SecurityType = "SecurityType";
            public static string UseLegacyJSONParsing = "UseLegacyJSONParsing";

        }

        public ActInputValue EndPointURL { get { return GetOrCreateInputParam(Fields.EndPointURL); } }

        public ActInputValue RequestBody { get { return GetOrCreateInputParam(Fields.RequestBody); } }

        private string ReqBody=String.Empty;
        public ActInputValue TemplateFile { get { return GetOrCreateInputParam(Fields.TemplateFile); } }

        public ActInputValue SaveRequestResponseFolderPath { get { return GetOrCreateInputParam(Fields.SaveRequestResponseFolderPath); } }

        public ActInputValue URLUser { get { return GetOrCreateInputParam(Fields.URLUser); } }

        public ActInputValue URLPass { get { return GetOrCreateInputParam(Fields.URLPass); }  }

        public ActInputValue URLDomain { get { return GetOrCreateInputParam(Fields.URLDomain); }  }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicElements = new ObservableList<ActInputValue>();

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> HttpHeaders = new ObservableList<ActInputValue>();

        private bool mRestRequestSave { get; set; }
        [IsSerializedForLocalRepository]
        public bool RestRequestSave { get { return mRestRequestSave; } set { mRestRequestSave = value; OnPropertyChanged(Fields.RestRequestSave); } }

        private bool mRestResponseSave { get; set; }
        [IsSerializedForLocalRepository]
        public bool RestResponseSave { get { return mRestResponseSave; } set { mRestResponseSave = value; OnPropertyChanged(Fields.RestResponseSave); } }


        [IsSerializedForLocalRepository]
        public bool DoNotFailActionOnBadRespose { get; set; }

        [IsSerializedForLocalRepository]
        public bool AcceptAllSSLCertificate { get; set; }

        private HttpWebResponse WebReqResponse = null;
        public HttpStatusCode ResponseCode
        {
            get
            {
                if (WebReqResponse == null)
                {
                    return HttpStatusCode.Ambiguous;
                }
                else
                {
                    return WebReqResponse.StatusCode;
                }
            }
        }

        [IsSerializedForLocalRepository(true)]
        public bool UseTemplateFile
        {
            get;
            set;
        }

        [IsSerializedForLocalRepository(true)]
        public bool UseRequestBody
        {
            get;
            set;
        }

        public enum eRequestType
        {
            GET,
            POST,
            PUT,
            PATCH
        }

        public enum eCookieMode
        {
            [EnumValueDescription("Use Session Cookies")]
            Session,
            [EnumValueDescription("Don't use Cookies")]
            None,
            [EnumValueDescription("Use fresh Cookies")]
            New
        }

        public enum eHttpVersion
        {
            [EnumValueDescription("Version 1.0")]
            HTTPV10,
            [EnumValueDescription("Version 1.1")]
            HTTPV11,
        }

        public enum eSercurityType
        {
            None,
            Ssl3,
            Tls,
            Tls11,
            Tls12
        }

        [IsSerializedForLocalRepository]
        public eRequestType RequestType { set; get; }

        public enum eContentType
        {
            [EnumValueDescription("application/json")]
            JSon,
            [EnumValueDescription("text/plain; charset=utf-8")]
            TextPlain,
            XML,
            [EnumValueDescription("application/pdf")]
            PDF
        }

        [IsSerializedForLocalRepository]
        public eContentType ContentType { set; get; }

        [IsSerializedForLocalRepository]
        public eHttpVersion ReqHttpVersion { set; get; }


        [IsSerializedForLocalRepository]
        public eContentType ResponseContentType { set; get; }

        [IsSerializedForLocalRepository]
        public eCookieMode CookieMode { set; get; }

        [IsSerializedForLocalRepository]
        public eSercurityType SecurityType { set; get; }
        public bool UseLegacyJSONParsing
        {
            get
            {
                if (!IsInputParamExist(Fields.UseLegacyJSONParsing) && ReturnValues.Count > 0)
                {
                    AddOrUpdateInputParamValue(Fields.UseLegacyJSONParsing, "True");//old action- for backward support- for not breaking existing validations using old parsing
                }
                if (!IsInputParamExist(Fields.UseLegacyJSONParsing))
                {
                    AddOrUpdateInputParamValue(Fields.UseLegacyJSONParsing, "False"); //as defualt use new JSON parser
                }
                bool eVal = true;
                if (bool.TryParse(GetInputParamValue(Fields.UseLegacyJSONParsing), out eVal))

                {
                    return eVal;
                }
                else
                {
                    return false;  //default value          
                }
            }
            set
            {
                AddOrUpdateInputParamValue(Fields.UseLegacyJSONParsing, value.ToString());
            }
        }

        public override string ActionDescription { get { return "REST Action"; } }
        public override string ActionUserDescription { get { return "Performs REST action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any REST actions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a REST action, enter End Point URL,Request Type,Content Type,Request Body and Request Body content");
        }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }


        //TODO:  Need to find a way to select the page and not in string, so will pass compile check
        public override string ActionEditPage { get { return "WebServices.ActRESTEditPage"; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.WebServices);
                }
                return mPlatforms;
            }
        }

        public override String ActionType
        {
            get
            {
                return "REST";
            }
        }

        public override void Execute()
        {
            if (AcceptAllSSLCertificate == true)
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(
                 delegate
                 {
                     return true;
                 }
                 );
            }


            switch (SecurityType)
            {
                case eSercurityType.None:

                    break;
                case eSercurityType.Ssl3:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    break;

                case eSercurityType.Tls:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    break;

                case eSercurityType.Tls11:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                    break;
                case eSercurityType.Tls12:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    break;
                default:
                    throw new NotSupportedException("The Configured security type is not supported");

            }


            try
            {
                string strURL = EndPointURL.ValueForDriver;
      
                HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(strURL);
                SetHTTPHeaders(WebReq);
                //Nathan added customizable Network Credentials
                NetworkCredential CustCreds = null;
                if (URLDomain.Value != "" && URLDomain.Value != null)
                {
                    CustCreds = new NetworkCredential("", "", "");
                    CustCreds.UserName = URLUser.Value;
                    CustCreds.Password = URLPass.Value;
                    CustCreds.Domain = URLDomain.Value;
                }
                else if (URLUser.Value != "" && URLUser.Value != null) //use current domain
                {
                    CustCreds = new NetworkCredential("", "", "");
                    CustCreds.UserName = URLUser.Value;
                    CustCreds.Password = URLPass.Value;
                }

                if (CustCreds == null)
                {
                    WebReq.UseDefaultCredentials = true;
                    WebReq.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                }
                else
                {
                    //Nathan - customized for Swapna ATT D2 Architechture to Pass Network Credentials
                    WebReq.UseDefaultCredentials = false;
                    WebReq.Credentials = CustCreds;
                    //End Nathan - customized for Swapna ATT D2 Architechture
                }

                //Method type
                WebReq.Method = RequestType.ToString();

                // POSTRequest.ContentType = "text/xml";
                WebReq.ContentType = General.GetEnumValueDescription(typeof(eContentType), ContentType);
                WebReq.KeepAlive = false;
                if (this.Timeout != null)
                {
                    WebReq.Timeout = (int)this.Timeout * 1000;
                }
                else
                {
                    WebReq.Timeout = int.MaxValue;
                }

                byte[] dataByte = null;
                if (!string.IsNullOrEmpty(RequestBody.ValueForDriver))
                {
                    dataByte = GetBody();
                }
                else if(!string.IsNullOrEmpty(TemplateFile.ValueForDriver))
                {
                    dataByte = GetBodyFromFile();
                }

                switch (CookieMode) //moved
                {

                    case eCookieMode.Session:

                        WebReq.CookieContainer = new CookieContainer();
                        foreach (Cookie cooki in _sessionCokkiesDic.Values)
                        {
                            Uri domainName = new Uri(EndPointURL.ValueForDriver);
                            Cookie ck = new Cookie();
                            ck.Name = cooki.Name;
                            ck.Value = cooki.Value;
                            if (String.IsNullOrEmpty(cooki.Domain)||true)
                            {
                                cooki.Domain = null;
                            }
                            WebReq.CookieContainer.Add(domainName, ck);
                        }
                        break;

                    case eCookieMode.None:

                        break;
                    case eCookieMode.New:
                        _sessionCokkiesDic.Clear();
                        break;
                    default:
                        throw new NotSupportedException("Configured mode is not supported");
                }

                //Not sending data on a get request 
                if (RequestType != eRequestType.GET)
                {

                    if (ContentType == eContentType.JSon)
                    {
                        WebReq.ContentType = "application/json";
                    }

                    if (RequestType == eRequestType.PATCH)
                    {
                        WebReq.Method = WebRequestMethods.Http.Post;
                        WebReq.Headers.Add("X-Http-Method-Override", "PATCH");
                    }



                    string req = HttpUtility.UrlEncode(RequestBody.Value);
                    if (ReqHttpVersion == eHttpVersion.HTTPV10)
                    {
                        WebReq.ProtocolVersion = HttpVersion.Version10;
                    }
                    else
                    {
                        WebReq.ProtocolVersion = HttpVersion.Version11;
                    }
                    WebReq.ContentLength = dataByte.Length;
              
                    Stream Webstream = WebReq.GetRequestStream();
                    Webstream.Write(dataByte, 0, dataByte.Length);
                    Webstream.Close();
                }
                // Write the data bytes in the request stream


                //Get response from server
                try
                {
                    WebReqResponse = (HttpWebResponse)WebReq.GetResponse();

                    for (int i=0;i<WebReqResponse.Headers.Count;i++)
                    {
                        AddOrUpdateReturnParamActual("Header: "+ WebReqResponse.Headers.Keys[i], WebReqResponse.Headers[i]);
                    }

                    AddOrUpdateReturnParamActual("Header: Status Code ", WebReqResponse.StatusDescription);
                }
                catch (WebException WE)
                {
                    WebReqResponse = (HttpWebResponse)WE.Response;

                    this.ExInfo = WE.Message;
                    if (DoNotFailActionOnBadRespose != true)
                    {
                        base.Status=Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        base.Error = WE.Message;
                    }


                }
                if (CookieMode != eCookieMode.None)
                {
                    foreach (Cookie cooki in WebReqResponse.Cookies)
                    {

                        if (_sessionCokkiesDic.Keys.Contains(cooki.Name) == false)
                        {
                            _sessionCokkiesDic.Add(cooki.Name, cooki);
                        }
                        else
                        {
                            _sessionCokkiesDic[cooki.Name] = cooki;
                        }
                    }

                    for (int k = 0; k < WebReqResponse.Headers.Count; k++)
                    {
                        if (WebReqResponse.Headers.Keys[k] == "Set-Cookie")
                        {
                            foreach(string httpCookie in  WebReqResponse.Headers.GetValues(k))
                            {

                                String[] cookiearray = httpCookie.Split(new char[] {';'}, 3);
                                String[] cookiearray2 = cookiearray[0].Split(new char[] { '=' }, 2);

                                Cookie cks= new Cookie();
                                cks.Name = cookiearray2[0];
                                cks.Value = cookiearray2[1];
                                cks.Path = cookiearray[1].Split(new char[] { '=' }, 2)[1];

                                if (cookiearray.Length == 3)
                                {
                                    if (cookiearray[2].Contains("HttpOnly"))
                                    {
                                        cks.HttpOnly = true;
                                    }
                                }
                                if (cks.Path.Length > 1)
                                {
                                    if (cks.Path.StartsWith("."))
                                    {

                                        Uri domainName = new Uri("http://"+ cks.Path.Substring(1));
                                        cks.Domain = domainName.Host;
                                    }
                                    else
                                    {
                                        Uri domainName = null;

                                        if (cks.Path.StartsWith("http://") || cks.Path.StartsWith("https://"))
                                        {
                                            domainName = new Uri(cks.Path);

                                        }

                                        else
                                        {
                                            domainName = new Uri("http://"+ cks.Path);
                                        }




                                        cks.Domain = domainName.Host;
                                    }
                                }
                                else
                                {
                                    Uri domainName = new Uri(EndPointURL.ValueForDriver);
                                    cks.Domain = domainName.Host;
                                }
                                if (cks.Path.StartsWith("."))
                                {
                                    cks.Path = cks.Path.Substring(1);
                                }
                                try
                                {
                                    _sessionCokkiesDic.Remove(cks.Name);
                                }
                                finally
                                {
                                    _sessionCokkiesDic.Add(cks.Name, cks);
                                }

                            }
                        }
                    }


                }

                string resp = string.Empty;
                byte[] data = new byte[0];
                if (ResponseContentType != eContentType.PDF)
                {
                    //TODO: check if UTF8 is good for all
                 StreamReader reader = new StreamReader(WebReqResponse.GetResponseStream(), Encoding.UTF8);                                  
                 Reporter.ToLog(eLogLevel.DEBUG, "Response");

                    resp = reader.ReadToEnd();
                    Reporter.ToLog(eLogLevel.DEBUG, resp);
                    reader.Close();
                }
                else
                {
                    MemoryStream memoryStream = new MemoryStream();
                    WebResponse webResponse = WebReq.GetResponse();

                    webResponse.GetResponseStream().CopyTo(memoryStream);

                    data = memoryStream.ToArray();
                    memoryStream.Close();
                }


                if (RestRequestSave==true && RequestType!=eRequestType.GET)
                {
                    string fileName= createRequestOrResponseXMLInFolder("Request", ReqBody, ContentType);
                    AddOrUpdateReturnParamActual("Saved Request File Name", fileName);
                }
                if (RestResponseSave == true)
                {
                    if (ResponseContentType != eContentType.PDF)
                    {
                        string fileName = createRequestOrResponseXMLInFolder("Response", resp, ResponseContentType);
                        AddOrUpdateReturnParamActual("Saved Response File Name", fileName);
                    }
                    else
                    {
                        CreatePdfFile(data);
                    }
                }


                AddOrUpdateReturnParamActual("Respose", resp);
                if(  String.IsNullOrEmpty(resp))
                {
                    return;
                }
                XmlDocument doc=null;
                if (ResponseContentType == eContentType.JSon)
                {
                    if (UseLegacyJSONParsing)
                    {
                        JsonTextReader Jreader = new JsonTextReader(new StringReader(resp));
                        while (Jreader.Read())
                        {
                            if (Jreader.Value != null)
                            {
                                Console.WriteLine("Token: {0}, Value: {1}", Jreader.TokenType, Jreader.Value);
                            }
                            else
                            {
                                Console.WriteLine("Token: {0}", Jreader.TokenType);
                            }
                        }


                        if (((resp[0]=='[')&& (resp[resp.Length-1] ==']')))
                        {
                            doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode("{\"root\":" + resp + "}", "root");
                        }
                        else
                        {
                            doc= Newtonsoft.Json.JsonConvert.DeserializeXmlNode(resp, "root");
                        }


                        List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem> outputTagsList = new List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem>();
                        outputTagsList = Amdocs.Ginger.Common.GeneralLib.General.GetXMLNodesItems(doc);
                        foreach (Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem outputItem in outputTagsList)
                        {
                            this.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                        }

                    }
                    else
                    {
                        try
                        {
                            this.ParseJSONToOutputValues(resp, 1);
                        }
                        catch
                        {
                            this.ParseJSONToOutputValues(General.CorrectJSON(resp), 1);
                        }
                    }
                    // Console.WriteLine(doc.);
                }

                else if (ResponseContentType == eContentType.XML)
                {
                    doc = new XmlDocument();
                    doc.LoadXml(resp);

                    List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem> outputTagsList = new List<Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem>();
                    outputTagsList = Amdocs.Ginger.Common.GeneralLib.General.GetXMLNodesItems(doc);
                    foreach (Amdocs.Ginger.Common.GeneralLib.General.XmlNodeItem outputItem in outputTagsList)
                    {
                        this.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                    }

                }
                //    XMLProcessor x = new XMLProcessor();
                // string s = x
                // x.ParseToReturnValues(s, this);
            }
            catch (WebException WEx)
            {
                this.ExInfo = WEx.Message;
                base.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                base.Error = WEx.Message;
            }

        }

        private void CreatePdfFile(byte[] data)
        {
            string filePath = Path.Combine(CreateFolder("Response"), CreateFileName("Response", "pdf"));
            File.WriteAllBytes(filePath, data);
        }

        private string CreateFolder(string fileType)
        {
            var DirectoryPath = string.Empty;
            DirectoryPath = SaveRequestResponseFolderPath.ValueForDriver;

            //if (DirectoryPath.StartsWith(@"~\"))
            //{
            //    DirectoryPath = DirectoryPath.Replace(@"~\", SolutionFolder);
            //}
            DirectoryPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(DirectoryPath);

            DirectoryPath = Path.Combine(DirectoryPath,fileType);

            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            return DirectoryPath;
        }

        private string CreateFileName(string fileType,string extension)
        {
            String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff");
            string fileName = string.Empty;
            fileName += this.Description;
            fileName += "_" + fileType;
            fileName += "_" + timeStamp;
            fileName += "." + extension;
            return fileName;
        }

        public string createRequestOrResponseXMLInFolder(string fileType, string fileContent,eContentType CT)
        {
            string fileName = string.Empty;
            string fileExtension = string.Empty;

            string DirectoryPath = CreateFolder(fileType);
           
            if (CT == eContentType.XML)
            {
                XmlDocument xmlDoc = new XmlDocument();

                xmlDoc.LoadXml(fileContent);
               
                try
                {
                    fileName= CreateFileName(fileType, "xml");
                    xmlDoc.Save(Path.Combine(DirectoryPath, fileName));
                }
                catch (Exception e)
                {
                    Reporter.ToUser(eUserMsgKey.FileOperationError, e.Message);
                }
            }
            else
            {
                if (CT == eContentType.JSon)

                {
                    fileExtension = "json";
                }

                else
                {
                    fileExtension = "txt";
                }

                 fileName = CreateFileName(fileType, fileExtension);
               File.WriteAllText(Path.Combine(DirectoryPath, fileName), fileContent);
            }

            return fileName;
        }

        private byte[] GetBody()
        {
            ReqBody=   RequestBody.ValueForDriver;
            ReqBody=SetDynamicValues(this,ReqBody);
            byte[] b1 = System.Text.Encoding.UTF8.GetBytes(ReqBody);
            return b1;
        }

        private byte[] GetBodyFromFile()
        {
            string ReqString = string.Empty;
            //FileStream ReqStream = System.IO.File.OpenRead(TemplateFile.ValueForDriver.Replace(@"~\", this.SolutionFolder));
            FileStream ReqStream = System.IO.File.OpenRead(amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(TemplateFile.ValueForDriver));

            using (StreamReader reader = new StreamReader(ReqStream))
            {
                ReqString = reader.ReadToEnd();
            }
            ReqString = SetDynamicValues(this, ReqString);
            ReqBody = ReqString;
            byte[] b1 = System.Text.Encoding.UTF8.GetBytes(ReqString);
            return b1;
        }

        private void SetHTTPHeaders(HttpWebRequest WebReq)
        {
            ValueExpression Ve=new ValueExpression(this.RunOnEnvironment,this.RunOnBusinessFlow,this.DSList);

            foreach(ActInputValue httpHeader in HttpHeaders)
            {
                WebReq.PreAuthenticate = true;
                Ve.Value = httpHeader.Value;
                httpHeader.ValueForDriver = Ve.ValueCalculated;

                switch (httpHeader.Param.ToUpper())
                {
                    case "DATE":
                        WebReq.Date = DateTime.Parse(httpHeader.ValueForDriver);
                        break;
                    case "CONTENT-TYPE":
                        WebReq.ContentType = httpHeader.ValueForDriver;
                        break;
                    case "ACCEPT":
                        WebReq.Accept = httpHeader.ValueForDriver;
                        break;
                    case "REFERER":
                        WebReq.Referer = httpHeader.ValueForDriver;
                        break;
                    case "":

                        break;
                    default:
                        WebReq.Headers.Add(httpHeader.Param, httpHeader.ValueForDriver);
                        break;
                }
            }
        }

        private string SetDynamicValues(ActREST AR, string ReqBody)
        {
            ValueExpression Ve=new ValueExpression(this.RunOnEnvironment,this.RunOnBusinessFlow,this.DSList);
            string NewReqBody = ReqBody;
            foreach (ActInputValue AIV in AR.DynamicElements)
            {

                string NewValue = AIV.ValueForDriver;

                if (String.IsNullOrEmpty(NewValue))
                {
                    Ve.Value=AIV.Value;
                    AIV.ValueForDriver=Ve.ValueCalculated;
                    NewValue = AIV.ValueForDriver;
                }
                NewReqBody = NewReqBody.Replace(AIV.Param, NewValue);


            }
            return NewReqBody;
        }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platform)
        {
            if(platform == ePlatformType.WebServices || platform == ePlatformType.NA)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Act IObsoleteAction.GetNewAction()
        {
            AutoMapper.MapperConfiguration mapConfigUIElement = new AutoMapper.MapperConfiguration(cfg => { cfg.CreateMap<Act, ActWebAPIRest>(); });
            ActWebAPIRest convertedActWebAPIRest = mapConfigUIElement.CreateMapper().Map<Act, ActWebAPIRest>(this);

            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.DoNotFailActionOnBadRespose, Convert.ToString(this.DoNotFailActionOnBadRespose));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.UseLegacyJSONParsing, Convert.ToString(this.UseLegacyJSONParsing));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLDomain, this.URLDomain.Value);
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLUser, this.URLUser.Value);
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.URLPass, this.URLPass.Value);
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.SecurityType, Convert.ToString(this.SecurityType));

            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.RequestType, Convert.ToString(this.RequestType));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ResponseContentType, Convert.ToString(this.ResponseContentType));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.CookieMode, Convert.ToString(this.CookieMode));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ContentType, Convert.ToString(this.ContentType));
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ReqHttpVersion, Convert.ToString(this.ReqHttpVersion));

            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.AuthorizationType, ApplicationAPIUtils.eAuthType.NoAuthentication.ToString());

            if (this.UseTemplateFile)
            {
                //Note:Don't confuse with field names. Use Template file is binded to Free text radio button in old Rest Action. 
                convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.FreeText.ToString());
                convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBody, this.RequestBody.Value);
            }
            else if (this.UseRequestBody)
            {
                convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton, ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString());
                convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser, this.TemplateFile.Value);
            }
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.CertificateTypeRadioButton, ApplicationAPIUtils.eCretificateType.AllSSL.ToString());
            convertedActWebAPIRest.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton, ApplicationAPIUtils.eNetworkCredentials.Default.ToString());

            convertedActWebAPIRest.DynamicElements = this.DynamicElements;
            convertedActWebAPIRest.HttpHeaders = this.HttpHeaders;

            if (convertedActWebAPIRest.ReturnValues != null && convertedActWebAPIRest.ReturnValues.Count != 0)
            {
                //Old rest action add response as --> Respose
                //And new adds it as Response:
                // so we update it when converting from old action to new
                ActReturnValue ARC = convertedActWebAPIRest.ReturnValues.Where(x => x.Param == "Respose").FirstOrDefault();
                if (ARC != null)
                {
                    ARC.Param = "Response:";
                }
            }
            return convertedActWebAPIRest;
        }

        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActWebAPIRest);
        }

        string IObsoleteAction.TargetActionTypeName()
        {
            ActWebAPIRest newActApiRest = new ActWebAPIRest();
            return newActApiRest.ActionDescription;
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.WebServices;
        }
    }
}
