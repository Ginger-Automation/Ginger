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
using Amdocs.Ginger.CoreNET.Platform;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.WebServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace GingerCore.Actions.WebAPI
{
    public class HttpWebClientUtils
    {
         HttpClient Client = null;
         HttpClientHandler Handler = null;

        //Task _Task = null; //thread for sending events
         HttpRequestMessage RequestMessage = null;
         ActWebAPIBase mAct;
         NetworkCredential UserCredentials = null;
         static Dictionary<string, Cookie> SessionCokiesDic = new Dictionary<string, Cookie>();
         HttpResponseMessage Response = null;
        string BodyString = null;
        string ContentType;
        ApplicationAPIUtils.eContentType eContentType;
        string ResponseMessage = null;

        public bool RequestContstructor(ActWebAPIBase act, string ProxySettings,bool useProxyServerSettings)
        {
            mAct = act;
            
            //Client Init & TimeOut
            Client = InitilizeClient();

            //EndPointURL
            if (!SetEndPointURL())
                return false;

            //NetworkCredentials
            if (!SetNetworkCredentials())
                return false;

            //Certificates 
            if (!SetCertificates())
                return false;

            //SecurityType
            SetSecurityType();

            //Authorization
            if (!SetAuthorization())
                return false;

            //ProxySettings
            SetProxySettings(ProxySettings, useProxyServerSettings);

            //Headers
            AddHeadersToClient();

            //SetAutoDecompression
            SetAutoDecompression();
                        
            if (act.GetType() == typeof(ActWebAPISoap))
                return RequestConstracotSOAP((ActWebAPISoap)act);
            else
                return RequestConstractorREST((ActWebAPIRest)act);
        }

        private void SetAutoDecompression()
        {
            if (mAct.HttpHeaders.Count() > 0)
            {
                var encodType = mAct.HttpHeaders.FirstOrDefault(x => x.Param.ToUpper() == "ACCEPT-ENCODING" && x.ValueForDriver.ToUpper() == "GZIP,DEFLATE");
                if (encodType != null)
                {
                    Handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
            }
        }

        private void AddHeadersToClient()
        {
            //Add request headers
            if (mAct.HttpHeaders.Count() > 0)
            {
                for (int i = 0; i < mAct.HttpHeaders.Count(); i++)
                {
                   
                    var specialCharactersReg = new Regex("^[a-zA-Z0-9 ]*$");
                    string param = mAct.HttpHeaders[i].Param;
                    string value = mAct.HttpHeaders[i].ValueForDriver;

                    if (param == "Content-Type")
                    {
                        ContentType = value;
                    }
                    else if (param.ToUpper() == "DATE")
                    {

                        Client.DefaultRequestHeaders.Date = System.DateTime.Parse(value);
                    }
                    else if (!specialCharactersReg.IsMatch(value))
                    {
                        Client.DefaultRequestHeaders.TryAddWithoutValidation(param, value);
                    }
                    else
                    {
                        Client.DefaultRequestHeaders.Add(param, value);
                    }

                }    
            }
        }

        private void SetProxySettings(string ProxySettings,bool useProxyServerSettings)
        {
            //Set proxy settings from local Server Proxy settings
            if (useProxyServerSettings)
            {
                Handler.Proxy = new WebProxy() { BypassProxyOnLocal = true };
            }
            //Set proxy settings from local
            else if (string.IsNullOrEmpty(ProxySettings))
            {
                Handler.Proxy = WebRequest.GetSystemWebProxy();
            }
            //Use proxy from Webservices window
            else if (!string.IsNullOrEmpty(ProxySettings))
            {
                WebProxy wsProxy = new WebProxy(ProxySettings, false)
                {
                    UseDefaultCredentials = Handler.UseDefaultCredentials,
                    Credentials = Handler.Credentials,
                };

                Handler.Proxy = wsProxy;
                Handler.PreAuthenticate = true;
            }
        }

        private bool SetAuthorization()
        {
            //Set Authentication method and retrieve fields
            ApplicationAPIUtils.eAuthType AuthorizationType = (ApplicationAPIUtils.eAuthType)mAct.GetInputParamCalculatedValue<ApplicationAPIUtils.eAuthType>(ActWebAPIBase.Fields.AuthorizationType);
            switch (AuthorizationType)
            {
                case ApplicationAPIUtils.eAuthType.NoAuthentication:
                    break;
                case ApplicationAPIUtils.eAuthType.BasicAuthentication:
                    string AuthorizationUser = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.AuthUsername);
                    string AuthorizationPassword = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.AuthPassword);
                    if ((!string.IsNullOrEmpty(AuthorizationUser) || (!string.IsNullOrEmpty(AuthorizationPassword))))
                    {
                        byte[] encodedAuthorization = Encoding.ASCII.GetBytes(AuthorizationUser + ":" + AuthorizationPassword);
                        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encodedAuthorization));
                    }
                    else
                    {
                        mAct.Error = "Request setup Failed because of missing/wrong input";
                        mAct.ExInfo = "Authorization User/Password is missing";
                        return false;
                    }
                    break;
            }
            return true;
        }
        
        private void SetSecurityType()
        {
            //Set Security Protocol( ssl etc.)
            ApplicationAPIUtils.eSercurityType secutityType = (ApplicationAPIUtils.eSercurityType)mAct.GetInputParamCalculatedValue<ApplicationAPIUtils.eSercurityType>(ActWebAPIBase.Fields.SecurityType);
            switch (secutityType)
            {
                case ApplicationAPIUtils.eSercurityType.None:
                    break;
                case ApplicationAPIUtils.eSercurityType.Ssl3:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                    break;
                case ApplicationAPIUtils.eSercurityType.Tls:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
                    break;
                case ApplicationAPIUtils.eSercurityType.Tls11:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
                    break;
                case ApplicationAPIUtils.eSercurityType.Tls12:
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    break;
            }
        }

        private bool SetCertificates()
        {
            string CertificateTypeRadioButton = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.CertificateTypeRadioButton);

            if (CertificateTypeRadioButton == ApplicationAPIUtils.eCretificateType.AllSSL.ToString())
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }
            else if (CertificateTypeRadioButton == ApplicationAPIUtils.eCretificateType.Custom.ToString())
            {
                //Use Custom Certificate:
                Handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                //string path = (mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.CertificatePath).ToString().Replace(@"~\", mAct.SolutionFolder));
                string path = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.CertificatePath));

                if (!string.IsNullOrEmpty(path))
                {
                    string CertificateKey = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.CertificatePassword);
                    if (!string.IsNullOrEmpty(CertificateKey))
                    {
                        X509Certificate2 customCertificate = new X509Certificate2(path, CertificateKey);
                        Handler.ClientCertificates.Add(customCertificate);
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
                    }
                    else
                    { 
                        //Case Certifacte key/password is not required
                        X509Certificate2 customCertificate = new X509Certificate2(path);
                        Handler.ClientCertificates.Add(customCertificate);
                    }
                }
                else
                {
                    mAct.Error = "Request setup Failed because of missing/wrong input";
                    mAct.ExInfo = "Certificate path is missing";
                    return false;
                }
            }
            return true;
        }

        private bool SetNetworkCredentials()
        {
            //check if Network Credentials are required:
            string NetworkCredentialsRadioButton = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.NetworkCredentialsRadioButton);
            if (NetworkCredentialsRadioButton == ApplicationAPIUtils.eNetworkCredentials.Custom.ToString())
            {
                Handler.UseDefaultCredentials = false;
                UserCredentials = new NetworkCredential("", "", "");
                string URLDomain = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.URLDomain.ToString());
                string CustomUserName = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.URLUser);
                string CustomPassword = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.URLPass);
                if (URLDomain != null)
                {
                    UserCredentials.Domain = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.URLDomain.ToString());
                }
                if ((string.IsNullOrEmpty(CustomUserName)) || (string.IsNullOrEmpty(CustomPassword)))
                {
                    mAct.Error = "Request setup Failed because of missing/wrong input";
                    mAct.ExInfo = "UserName or Password is missing";
                    return false;
                }
                else
                {
                    UserCredentials.UserName = CustomUserName;
                    UserCredentials.Password = CustomPassword;
                    Handler.Credentials = UserCredentials;
                }
            }
            else if (NetworkCredentialsRadioButton == ApplicationAPIUtils.eNetworkCredentials.Default.ToString())
            {
                //Use Default Network Credentials:
                Handler.UseDefaultCredentials = true;
                Handler.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            return true;
        }

        private bool SetEndPointURL()
        {
            string url = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.EndPointURL);
            if (!string.IsNullOrEmpty(url))
                Client.BaseAddress = new Uri(url);
            else
            {
                mAct.Error = "Request setup Failed because of missing/wrong input";
                mAct.ExInfo = "URL is missing";
                return false;
            }
            Reporter.ToLog(eLogLevel.DEBUG, "EndPointURL: " + url);
            return true;
        }

        private HttpClient InitilizeClient()
        {
            try
            {
                Handler = new HttpClientHandler();
                Client = new HttpClient(Handler);

                int miliseconds = 0;
                if (!string.IsNullOrEmpty(Convert.ToString(mAct.Timeout)))
                    miliseconds = ((int)mAct.Timeout) * 1000;

                if (miliseconds > 0)
                    Client.Timeout = TimeSpan.FromMilliseconds(miliseconds);

                return Client;
            }
            catch (Exception ex)
            {
                mAct.Error = "Failed to created the HTTP Client";
                mAct.ExInfo += Environment.NewLine + ex.Message + Environment.NewLine + ex.InnerException;
                return null;
            }
        }

        public void SaveRequest(bool RequestSave, string SaveDirectory)
        {
            string RequestFileContent = string.Empty;
            if (RequestSave)
            {
                if (mAct.GetType() == typeof(ActWebAPISoap))
                {
                    RequestFileContent = BodyString;
                    mAct.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ContentType, "XML");
                }
                else if (mAct.GetType() == typeof(ActWebAPIRest))
                {
                    if (!string.IsNullOrEmpty(BodyString))
                    {
                        RequestFileContent = BodyString;
                    }
                    else if ((mAct.RequestKeyValues.Count() > 0) && (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == "XwwwFormUrlEncoded"))
                    {
                        HttpContent UrlEncoded = new FormUrlEncodedContent(ConstructURLEncoded((ActWebAPIRest)mAct));
                        RequestFileContent = UrlEncoded.ToString();
                    }
                    else if ((mAct.RequestKeyValues.Count() > 0) && (mAct.GetInputParamValue(ActWebAPIRest.Fields.ContentType) == "FormData"))
                    {
                        MultipartFormDataContent FormDataContent = new MultipartFormDataContent();
                        for (int i = 0; i < mAct.RequestKeyValues.Count(); i++)
                            FormDataContent.Add(new StringContent(mAct.RequestKeyValues[i].ValueForDriver), mAct.RequestKeyValues[i].ItemName.ToString());
                        RequestFileContent = FormDataContent.ToString();
                    }
                    else
                    {
                        RequestFileContent = RequestMessage.ToString();
                    }
                }

                string FileFullPath = Webserviceplatforminfo.SaveToFile("Request", RequestFileContent, SaveDirectory,mAct);
                mAct.AddOrUpdateReturnParamActual("Saved Request File Name", Path.GetFileName(FileFullPath));
            }
        }

        public bool SendRequest()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Client Sending Async Request");

                Response = Client.SendAsync(RequestMessage).Result;
                Reporter.ToLog(eLogLevel.DEBUG, "Response status: " + Response.StatusCode);

                if (ApplicationAPIUtils.eContentType.PDF.ToString() != mAct.GetInputParamValue(ActWebAPIRest.Fields.ResponseContentType))
                {
                    ResponseMessage = Response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    ResponseMessage = ReadByteArrayAndConvertToString();
                }
          
               
                Reporter.ToLog(eLogLevel.DEBUG, "ResponseMessage: " + ResponseMessage);
                Reporter.ToLog(eLogLevel.DEBUG, "Returning true on the end of the try in SendRequest method");
                return true;
            }
            catch (Exception WE)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Send Request went to exception: " + WE.Message + Environment.NewLine + WE.InnerException);
                if (WE.InnerException.ToString().Contains("The character set provided in ContentType is invalid. Cannot read content as string using an invalid character set."))
                {
                    Reporter.ToLog(eLogLevel.WARN, "Caught Content Type Exception:" + WE.Message);
                    ResponseMessage = ReadByteArrayAndConvertToString();
                    return false;
                }
                mAct.Error = "Request execution failed, reason: " + WE.Message;
                mAct.ExInfo += Environment.NewLine + WE.Message;
            }
            Reporter.ToLog(eLogLevel.DEBUG, "Returning true on the end of the SendRequest method");
            return true;
        }
        private string ReadByteArrayAndConvertToString()
        {
            byte[] data = Response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.Default.GetString(data);
        }

       

        public bool ValidateResponse()
        {
            try
            {
                if (Response != null)
                {
                    Response.EnsureSuccessStatusCode();
                    return true;
                }
                else
                {
                    mAct.Error = "Response returned as Null";
                    return false;
                }
            }
            catch (Exception WE)
            {
                mAct.Error = "Response include error code, Response Code:" + WE.Message;
                mAct.ExInfo += System.Environment.NewLine + WE.Message;
                return false;
            }
        }

        public bool ParseRespondToOutputParams()
        {
            if (Response != null)
            {
                mAct.AddOrUpdateReturnParamActual("Header: Status Code ", Response.StatusCode.ToString());
                Reporter.ToLog(eLogLevel.DEBUG, "Retrieve Response Status Code passed successfully");
                foreach (var Header in Response.Headers)
                {
                    string headerValues = string.Empty;
                    foreach (string val in Header.Value.ToArray())
                        headerValues = val + ",";
                    headerValues = headerValues.Remove(headerValues.Length - 1);
                    mAct.AddOrUpdateReturnParamActual("Header: " + Header.Key.ToString(), headerValues);
                }
                Reporter.ToLog(eLogLevel.DEBUG, "responseHeadersCollection passed successfully");
            }
            else
            {
                mAct.AddOrUpdateReturnParamActual("Respond", "Respond returned as null");
            }

          
            
          

            string prettyResponse = XMLDocExtended.PrettyXml(ResponseMessage);

            mAct.AddOrUpdateReturnParamActual("Response:", prettyResponse);

            if (!ActWebAPIBase.ParseNodesToReturnParams(mAct, ResponseMessage))
                return false;

            return true;
        }

        public void SaveResponseToFile(bool saveResponse, string savePath)
        {
            if (saveResponse)
            {
                string ResponseFileContent = string.Empty;

                if (mAct.GetType() == typeof(ActWebAPISoap))
                {
                    ResponseFileContent = ResponseMessage;
                    mAct.AddOrUpdateInputParamValueAndCalculatedValue(ActWebAPIRest.Fields.ResponseContentType, "XML");
                }
                else if (mAct.GetType() == typeof(ActWebAPIRest))
                {
                    if (!string.IsNullOrEmpty(ResponseMessage))
                    {
                        ResponseFileContent = ResponseMessage;
                    }
                    else
                    {
                        if (Response != null && Response.Headers != null)
                        {
                            ResponseFileContent = Response.Headers.ToString();
                        }
                        else
                        {
                            ResponseFileContent = string.Empty;
                        }
                    }
                }

                ResponseFileContent = Amdocs.Ginger.Common.XMLDocExtended.PrettyXml(ResponseFileContent);

                string FileFullPath = Webserviceplatforminfo.SaveToFile("Response", ResponseFileContent, savePath, mAct);
                mAct.AddOrUpdateReturnParamActual("Saved Response File Name", Path.GetFileName(FileFullPath));
            }
        }





        public void HandlePostExecutionOperations()
        {
            //Handle response cookies
            HandleResponseCookies();
            Reporter.ToLog(eLogLevel.DEBUG, "Handle response cookies Passed successfully");
        }

        private void HandleResponseCookies()
        {
            if (mAct.GetInputParamCalculatedValue(ActWebAPIRest.Fields.CookieMode) != ApplicationAPIUtils.eCookieMode.None.ToString())
            {

                CookieCollection responseCookies = Handler.CookieContainer.GetCookies(Client.BaseAddress);
                foreach (Cookie RespCookie in responseCookies)
                {
                    if (SessionCokiesDic.Keys.Contains(RespCookie.Name) == false)
                    {
                        SessionCokiesDic.Add(RespCookie.Name, RespCookie);
                    }
                    else
                    {
                        SessionCokiesDic[RespCookie.Name] = RespCookie;
                    }
                }
            }
        }

 
        
        private bool RequestConstractorREST(ActWebAPIRest act)
        {
            //Request Method:
            HttpMethod RequestMethod = new HttpMethod(mAct.GetInputParamCalculatedValue(ActWebAPIRest.Fields.RequestType).ToUpper());
            RequestMessage = new HttpRequestMessage(RequestMethod, Client.BaseAddress);
            //HTTP Version:
            SetHTTPVersion();
            //Request Content Type: 
            SetContentType();
            //Cookie Settings:
            SetCookies();
            //Request Body:
            SetRequestContent(RequestMethod);
            return true;
        }

        private void SetRequestContent( HttpMethod RequestMethod)
        {
            List<KeyValuePair<string, string>> KeyValues = new List<KeyValuePair<string, string>>();

            if ((RequestMethod.ToString() == ApplicationAPIUtils.eRequestType.GET.ToString()))
            {
                if (eContentType == ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded)
                {
                    string GetRequest = "?";
                    if (mAct.RequestKeyValues.Count() > 0)
                    {
                        for (int i = 0; i < mAct.RequestKeyValues.Count(); i++)
                        {
                            GetRequest += mAct.RequestKeyValues[i].ItemName.ToString() + "=" + mAct.RequestKeyValues[i].ValueForDriver + "&";
                        }
                    }
                    string ValuesURL = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.EndPointURL) + GetRequest.Substring(0, GetRequest.Length - 1);
                    Client.BaseAddress = new Uri(ValuesURL);
                }
                else
                {
                    Client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", ContentType);
                }
            }
            else
            {
                if ((eContentType != ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded) && (eContentType != ApplicationAPIUtils.eContentType.FormData))
                {
                    string RequestBodyType = mAct.GetInputParamValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton);
                    if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.FreeText.ToString())
                    {
                        string RequestBodyWithDynamicParameters = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.RequestBody).ToString();
                        BodyString = SetDynamicValues(RequestBodyWithDynamicParameters);
                    }
                    else if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
                    {
                        BodyString = SetDynamicValues(GetStringBodyFromFile());
                    }
                }

                switch (eContentType)
                {
                    case ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded:
                        if (mAct.RequestKeyValues.Count() > 0)
                        {
                            KeyValues = ConstructURLEncoded((ActWebAPIRest)mAct);
                            RequestMessage.Content = new FormUrlEncodedContent(KeyValues);
                        }
                        break;
                    case ApplicationAPIUtils.eContentType.FormData:
                        if (mAct.RequestKeyValues.Count() > 0)
                        {
                            MultipartFormDataContent requestContent = new MultipartFormDataContent();
                            List<KeyValuePair<string, string>> FormDataKeyValues = new List<KeyValuePair<string, string>>();
                            for (int i = 0; i < mAct.RequestKeyValues.Count(); i++)
                            {
                                if (mAct.RequestKeyValues[i].ValueType == WebAPIKeyBodyValues.eValueType.Text)
                                {
                                    FormDataKeyValues.Add(new KeyValuePair<string, string>(mAct.RequestKeyValues[i].ItemName.ToString(), mAct.RequestKeyValues[i].ValueForDriver));
                                    requestContent.Add(new StringContent(mAct.RequestKeyValues[i].ValueForDriver), mAct.RequestKeyValues[i].ItemName.ToString());
                                }
                                if (mAct.RequestKeyValues[i].ValueType == WebAPIKeyBodyValues.eValueType.File)
                                {
                                    string path = mAct.RequestKeyValues[i].ValueForDriver;
                                    //string FullPath = path.Replace("~\\", mAct.SolutionFolder);
                                    string FullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(path);

                                    FileStream FileStream = File.OpenRead(FullPath);
                                    var streamContent = new StreamContent(FileStream);
                                    var fileContent = new ByteArrayContent(streamContent.ReadAsByteArrayAsync().Result);
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                                    requestContent.Add(fileContent, mAct.RequestKeyValues[i].ItemName.ToString(), Path.GetFileName(path));
                                }

                            }
                            RequestMessage.Content = requestContent;
                        }
                        break;
                    case ApplicationAPIUtils.eContentType.XML:
                        string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                        if (BodyString.StartsWith(_byteOrderMarkUtf8))
                        {
                            var lastIndexOfUtf8 = _byteOrderMarkUtf8.Length - 1;
                            BodyString = BodyString.Remove(0, lastIndexOfUtf8);
                        }
                        RequestMessage.Content = new StringContent(BodyString, Encoding.UTF8, ContentType);
                        break;
                    default:
                        RequestMessage.Content = new StringContent(BodyString, Encoding.UTF8, ContentType);
                        break;
                }
            }
        }

        private void SetCookies()
        {
            ApplicationAPIUtils.eCookieMode contentType = (ApplicationAPIUtils.eCookieMode)mAct.GetInputParamCalculatedValue<ApplicationAPIUtils.eCookieMode>(ActWebAPIRest.Fields.CookieMode);
            switch (contentType)
            {
                case ApplicationAPIUtils.eCookieMode.None:
                    break;
                case ApplicationAPIUtils.eCookieMode.New:
                    SessionCokiesDic.Clear();
                    break;
                case ApplicationAPIUtils.eCookieMode.Session:
                    foreach (Cookie cooki in SessionCokiesDic.Values)
                    {
                        Uri domainName = new Uri(mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.EndPointURL.ToString()));
                        Cookie ck = new Cookie();
                        ck.Name = cooki.Name;
                        ck.Value = cooki.Value;
                        if (String.IsNullOrEmpty(cooki.Domain))
                        {
                            cooki.Domain = domainName.Host;
                        }
                        RequestMessage.Headers.Add(ck.Name, ck.Value);
                        Handler.CookieContainer.Add(cooki);
                    }
                    break;
            }
        }

        private void SetContentType()
        {
            if (ContentType == null)
            {
                eContentType = (ApplicationAPIUtils.eContentType)mAct.GetInputParamCalculatedValue<ApplicationAPIUtils.eContentType>(ActWebAPIRest.Fields.ContentType);
                switch (eContentType)
                {
                    case ApplicationAPIUtils.eContentType.JSon:
                        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        ContentType = "application/json";
                        break;
                    case ApplicationAPIUtils.eContentType.XwwwFormUrlEncoded:
                        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                        ContentType = "application/x-www-form-urlencoded";
                        break;
                    case ApplicationAPIUtils.eContentType.FormData:
                        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));
                        ContentType = "multipart/form-data"; //update to correct value
                        break;
                    case ApplicationAPIUtils.eContentType.TextPlain:
                        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                        ContentType = "text/plain; charset=utf-8";
                        break;
                    case ApplicationAPIUtils.eContentType.XML:
                        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
                        ContentType = "text/xml";
                        break;
                }
            }
        }

        private void SetHTTPVersion()
        {
            string ReqHttpVersion = mAct.GetInputParamCalculatedValue(ActWebAPIRest.Fields.ReqHttpVersion);
            if (ReqHttpVersion == ApplicationAPIUtils.eHttpVersion.HTTPV10.ToString())
                RequestMessage.Version = HttpVersion.Version10;
            else if (ReqHttpVersion == ApplicationAPIUtils.eHttpVersion.HTTPV11.ToString())
                RequestMessage.Version = HttpVersion.Version11;
        }

        private List<KeyValuePair<string, string>> ConstructURLEncoded(ActWebAPIRest act)
        {
            List<KeyValuePair<string, string>> KeyValues = new List<KeyValuePair<string, string>>();

            for (int i = 0; i < mAct.RequestKeyValues.Count(); i++)
            {
                KeyValues.Add(new KeyValuePair<string, string>(mAct.RequestKeyValues[i].ItemName.ToString(), mAct.RequestKeyValues[i].ValueForDriver));
            }

            return KeyValues;
        }

        private bool RequestConstracotSOAP(ActWebAPISoap act) 
        {
            //Set default parameters for SOAP Actions
            RequestMessage = new HttpRequestMessage(HttpMethod.Post, Client.BaseAddress);
            string SoapAction = mAct.GetInputParamCalculatedValue(ActWebAPISoap.Fields.SOAPAction);
            
            RequestMessage.Headers.Add("SOAPAction", SoapAction);

            //WorkArownd for configuring SOAP content type deferent then text/xml
            ActInputValue ContetnTypeHeader = mAct.HttpHeaders.Where(x => x.Param == "Content-Type").FirstOrDefault();

            if (ContetnTypeHeader != null)
                ContentType = ContetnTypeHeader.ValueForDriver;
            else
                ContentType = "text/xml";

            string RequestBodyType = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.RequestBodyTypeRadioButton);

            string RequestBodyWithDynamicParameters = string.Empty;
            if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.FreeText.ToString())
            {
                RequestBodyWithDynamicParameters = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.RequestBody).ToString();
                if (string.IsNullOrEmpty(RequestBodyWithDynamicParameters))
                {
                    act.Error = "Request setup Failed because of missing/wrong input";
                    act.ExInfo = "Request Body is missing";
                    return false;
                }
            }
            else if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
            {
                if (!string.IsNullOrEmpty(mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser).ToString()))
                {
                    RequestBodyWithDynamicParameters = GetStringBodyFromFile();
                    if (string.IsNullOrEmpty(RequestBodyWithDynamicParameters))
                    {
                        act.Error = "Request setup Failed because of missing/wrong input";
                        act.ExInfo = "Request body file content is empty";
                        return false;
                    }
                }
                else
                {
                    act.Error = "Request setup Failed because of missing/wrong input";
                    act.ExInfo = "Request body file path is missing";
                    return false;
                }
            }

            BodyString = SetDynamicValues(RequestBodyWithDynamicParameters);

            Reporter.ToLog(eLogLevel.DEBUG, "RequestBody: " + BodyString);

            RequestMessage.Content = new StringContent(BodyString, Encoding.UTF8, ContentType);

            return true;   
        }
        
        private string GetStringBodyFromFile()
        {
            string FileContent = string.Empty;
            string TemplateFileName = mAct.GetInputParamCalculatedValue(ActWebAPIBase.Fields.TemplateFileNameFileBrowser).ToString();
            //string TemplateFileNameFullPath = TemplateFileName.Replace(@"~\", mAct.SolutionFolder);
            string TemplateFileNameFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(TemplateFileName);

            FileStream ReqStream = File.OpenRead(TemplateFileNameFullPath);

            using (StreamReader reader = new StreamReader(ReqStream))
            {
                FileContent = reader.ReadToEnd();
            }
            return FileContent;
        }

        private string SetDynamicValues( string RequestBodyWithParameters)
        {
            foreach (ActInputValue AIV in mAct.DynamicElements)
            {
                string NewValue = AIV.ValueForDriver;
                RequestBodyWithParameters = RequestBodyWithParameters.Replace(AIV.Param, NewValue);
            }
            return RequestBodyWithParameters;
        }

        public static List<string> GetSoapSecurityHeaderContent(ref string txtBoxBodyContent)
        {
            List<string> SecuritryContent = new List<string>();
            StringBuilder soapHeaderContent = new StringBuilder();

            soapHeaderContent.Append("\t<soapenv:Header>\n");
            soapHeaderContent.Append("\t\t<wsse:Security xmlns:wsse=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"");
            soapHeaderContent.Append("\n\t\t xmlns:wsu=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"");
            soapHeaderContent.Append(">\n");
            soapHeaderContent.Append("\t\t\t\t<wsse:UsernameToken wsu:Id=");
            soapHeaderContent.Append("\"UsernameToken-{Function Fun=GetHashCode({Function Fun=GetGUID()})}");
            soapHeaderContent.Append("\">\n\t\t\t\t<wsse:Username>");
            soapHeaderContent.Append("wssecusername");
            soapHeaderContent.Append("</wsse:Username>\n\t\t\t\t<wsse:Password Type=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\"");
            soapHeaderContent.Append(">");
            soapHeaderContent.Append("wssecpassword");
            soapHeaderContent.Append("</wsse:Password>\n\t\t\t\t<wsse:Nonce EncodingType=");
            soapHeaderContent.Append("\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary\"");
            soapHeaderContent.Append(">" + "{Function Fun=GenerateHashCode(\"{Function Fun=GetGUID()}wssecpassword\")}" + "=");
            soapHeaderContent.Append("</wsse:Nonce>\n\t\t\t\t<wsu:Created>");
            soapHeaderContent.Append("{Function Fun=GetUTCTimeStamp()}");
            soapHeaderContent.Append("</wsu:Created>\n\t\t\t</wsse:UsernameToken>\n\t\t</wsse:Security>\n\t</soapenv:Header>\n");
            SecuritryContent.Add(soapHeaderContent.ToString());
            SecuritryContent.Add("wssecusername");
            SecuritryContent.Add("wssecpassword");


            string wsSecuritySettings = SecuritryContent.ElementAt(0);
            string pattern1 = "<soapenv:Header>(.*?)</soapenv:Header>|<soapenv:Header/>|<(\\s)soapenv:Header(\\s)/>";
            string pattern2 = "<soapenv:Body>|<soapenv:Body/>";
            string pattern3 = "\t<soapenv:Header>(.*?)\t</soapenv:Header>|\t<soapenv:Header/>|\t<(\\s)soapenv:Header(\\s)/>|\t<soapenv:Header>";
            bool isPatrn1Exists = Regex.IsMatch(txtBoxBodyContent, pattern1);
            if (!isPatrn1Exists)
            {
                bool isPattrn3Exists = Regex.IsMatch(txtBoxBodyContent, pattern3);
                if (!isPattrn3Exists)
                {
                    bool isPatrn2Exists = Regex.IsMatch(txtBoxBodyContent, pattern2);
                    if (isPatrn2Exists)
                    {
                        txtBoxBodyContent = Regex.Replace(txtBoxBodyContent, pattern2, wsSecuritySettings + "<soapenv:Body>");
                    }
                }
            }
            else
            {
                txtBoxBodyContent = Regex.Replace(txtBoxBodyContent, pattern1, wsSecuritySettings);
            }
            return SecuritryContent;
        }
    }
}
