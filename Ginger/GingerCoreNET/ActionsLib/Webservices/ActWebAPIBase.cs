#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace GingerCore.Actions.WebServices
{
    public class ActWebAPIBase : Act, IActPluginPostRun
    {
        public ActWebAPIBase()
        {
            //Disable Auto Screenshot on failure by default. User can override it if needed
            AutoScreenShotOnFailure = false;
        }
        public override string ActionDescription { get { return "Web API Action"; } }

        public override eImageType Image { get { return eImageType.Exchange; } }

        public override string ActionUserDescription { get { return "Performs REST/SOAP actions"; } }
        public override string ActionEditPage { get { return "WebServices.ActWebAPIEditPage"; } }
        public override bool ValueConfigsNeeded { get { return false; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool IsSelectableAction { get { return false; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
        }

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

        public new static partial class Fields
        {
            //Common fields:
            public static string EndPointURL = "EndPointURL";
            public static string URLUser = "URLUser";
            public static string URLPass = "URLPass";
            public static string URLDomain = "URLDomain";
            public static string DoNotFailActionOnBadRespose = "DoNotFailActionOnBadRespose";
            public static string CertificateTypeRadioButton = "CertificateTypeRadioButton";
            public static string CertificatePath = "CertificatePath";
            public static string CertificatePassword = "CertificatePassword";
            public static string AuthorizationType = "AuthorizationType";
            public static string AuthUsername = "AuthUsername";
            public static string AuthPassword = "AuthPassword";
            public static string RequestBody = "RequestBody";
            public static string RequestBodyTypeRadioButton = "RequestBodyTypeRadioButton";
            public static string RequestFileName = "RequestFileName";
            public static string SecurityType = "SecurityType";
            public static string TemplateFileNameFileBrowser = "TemplateFileNameFileBrowser";
            public static string NetworkCredentialsRadioButton = "NetworkCredentialsRadioButton";
            public static string ImportRequestFile = "ImportRequestFile";
            public static string ImportCetificateFile = "ImportCetificateFile";

            public static string UseLegacyJSONParsing = "UseLegacyJSONParsing";

        }

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> DynamicElements = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> HttpHeaders = [];

        [IsSerializedForLocalRepository]
        public ObservableList<WebAPIKeyBodyValues> RequestKeyValues = [];

        //CREATE LIST OF ActInputValue for each field which requires expression calculation
        private ObservableList<ActInputValue> FormDataToAIVConverter(ObservableList<WebAPIKeyBodyValues> BodyKeyValueList)
        {
            ObservableList<ActInputValue> fa = [];
            foreach (WebAPIKeyBodyValues wiv in RequestKeyValues)
            {
                fa.Add(wiv);
            }
            return fa;
        }

        public override List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            List<ObservableList<ActInputValue>> list = [DynamicElements, HttpHeaders, FormDataToAIVConverter(RequestKeyValues)];

            return list;
        }



        public ApplicationAPIUtils.eNetworkCredentials NetworkCredentialsRadioButton
        {
            get
            {
                ApplicationAPIUtils.eNetworkCredentials eVal = ApplicationAPIUtils.eNetworkCredentials.Custom;
                if (Enum.TryParse<ApplicationAPIUtils.eNetworkCredentials>(GetInputParamValue(Fields.NetworkCredentialsRadioButton), out eVal))
                {
                    return eVal;
                }
                else
                {
                    return ApplicationAPIUtils.eNetworkCredentials.Default;  //default value          
                }
            }
        }

        private string ReqBody = String.Empty;

        public bool mUseTemplateFile = true;

        public bool mUseRequestBody = true;

        public bool UseRequestBodyValue
        {
            get
            {
                bool aa = false;
                if (Boolean.TryParse(GetInputParamValue(Fields.RequestBodyTypeRadioButton), out aa) == true)
                {
                    return aa;
                }
                return false;
            }
        }

        public override string ActionType
        {
            get
            {
                return ActionDescription;
            }
        }

        public bool UseLegacyJSONParsing
        {
            get
            {
                if (IsInputParamExist(Fields.UseLegacyJSONParsing) == false && ReturnValues.Count > 0)
                {
                    AddOrUpdateInputParamValue(Fields.UseLegacyJSONParsing, "True");//old action- for backward support- for not breaking existing validations using old parsing
                }

                if (IsInputParamExist(Fields.UseLegacyJSONParsing) == false)
                {
                    AddOrUpdateInputParamValue(Fields.UseLegacyJSONParsing, "False"); //as default use new JSON parser
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




        public static bool ParseNodesToReturnParams(ActWebAPIBase mAct, string ResponseMessage)
        {
            bool XMLResponseCanBeParsed = false;
            XMLResponseCanBeParsed = XMLStringCanBeParsed(ResponseMessage);

            Reporter.ToLog(eLogLevel.DEBUG, "XMLResponseCanBeParsed Indicator: " + XMLResponseCanBeParsed);

            if (XMLResponseCanBeParsed && mAct.GetType() == typeof(ActWebAPISoap))
            {
                return ParseXMLNodesToReturnParams(mAct, ResponseMessage);
            }
            else if (mAct.GetType() == typeof(ActWebAPIRest))
            {
                if (string.IsNullOrEmpty(ResponseMessage))
                {
                    return false;
                }

                string ResponseContentType = mAct.GetInputParamCalculatedValue(ActWebAPIRest.Fields.ResponseContentType);
                bool jsonParsinFailed = false;

                if (ResponseContentType == ApplicationAPIUtils.eContentType.JSon.ToString())
                {
                    if (!ParseJsonNodesToReturnParams(mAct, ResponseMessage))
                    {
                        jsonParsinFailed = true;//will try XML parsing instead
                    }
                    else
                    {
                        return true;
                    }
                }

                if (XMLResponseCanBeParsed && (
                   (mAct.GetInputParamValue(ActWebAPIRest.Fields.ResponseContentType) == ApplicationAPIUtils.eContentType.XML.ToString()) || jsonParsinFailed))
                {
                    return ParseXMLNodesToReturnParams(mAct, ResponseMessage);
                }
            }

            return false;
        }

        private static bool XMLStringCanBeParsed(string responseMessage)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseMessage);
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"XMLStringCanBeParsed:\n{ex.StackTrace}");
                return false;
            }
        }



        private static bool ParseXMLNodesToReturnParams(ActWebAPIBase mAct, string ResponseMessage)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ResponseMessage);

                if (mAct.UseLegacyJSONParsing)
                {
                    List<General.XmlNodeItem> outputTagsList = [];
                    outputTagsList = General.GetXMLNodesItems(doc, true);
                    foreach (General.XmlNodeItem outputItem in outputTagsList)
                    {
                        mAct.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                    }
                }
                else
                {
                    XMLDocExtended XDE = new XMLDocExtended(doc);
                    foreach (XMLDocExtended XDN in XDE.GetEndingNodes())
                    {
                        mAct.AddOrUpdateReturnParamActualWithPath(XDN.LocalName, XDN.Value, XDN.XPathWithoutNamspaces);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }


        private static bool ParseJsonNodesToReturnParams(ActWebAPIBase mAct, string ResponseMessage)
        {
            XmlDocument doc = null;

            try
            {
                var JsonCheck = JToken.Parse(ResponseMessage);
            }
            catch (JsonReaderException)
            {
                return false;
            }

            if (mAct.UseLegacyJSONParsing)
            {

                if (((ResponseMessage[0] == '[') && (ResponseMessage[^1] == ']')))
                {
                    doc = JsonConvert.DeserializeXmlNode("{\"root\":" + ResponseMessage + "}", "root");
                }
                else
                {
                    try
                    {
                        doc = JsonConvert.DeserializeXmlNode(ResponseMessage, "root");
                    }
                    catch
                    {
                        doc = JsonConvert.DeserializeXmlNode(General.CorrectJSON(ResponseMessage), "root");
                    }

                }

                List<General.XmlNodeItem> outputTagsList = [];
                outputTagsList = General.GetXMLNodesItems(doc, true);
                foreach (General.XmlNodeItem outputItem in outputTagsList)
                {
                    mAct.AddOrUpdateReturnParamActualWithPath(outputItem.param, outputItem.value, outputItem.path);
                }
            }
            else
            {
                try
                {
                    mAct.ParseJSONToOutputValues(ResponseMessage, 1);
                }
                catch
                {
                    mAct.ParseJSONToOutputValues(General.CorrectJSON(ResponseMessage), 1);
                }
            }
            return true;
        }

        internal string GetCalulatedRequestBodyString()
        {
            string RequestBodyType = GetInputParamCalculatedValue(Fields.RequestBodyTypeRadioButton);
            string RequestBody = string.Empty;
            if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.FreeText.ToString())
            {
                RequestBody = GetInputParamCalculatedValue(Fields.RequestBody);
                if (string.IsNullOrEmpty(RequestBody))
                {

                    return null;
                }
            }
            else if (RequestBodyType == ApplicationAPIUtils.eRequestBodyType.TemplateFile.ToString())
            {
                if (!string.IsNullOrEmpty(GetInputParamCalculatedValue(Fields.TemplateFileNameFileBrowser).ToString()))
                {



                    string FileContent = string.Empty;
                    string TemplateFileName = GetInputParamCalculatedValue(Fields.TemplateFileNameFileBrowser).ToString();

                    string TemplateFileNameFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(TemplateFileName);

                    FileStream ReqStream = File.OpenRead(TemplateFileNameFullPath);

                    using (StreamReader reader = new StreamReader(ReqStream))
                    {
                        RequestBody = reader.ReadToEnd();
                    }




                }
                if (string.IsNullOrEmpty(RequestBody))
                {

                    return null;
                }
            }

            foreach (ActInputValue AIV in DynamicElements)
            {
                string NewValue = AIV.ValueForDriver;
                RequestBody = RequestBody.Replace(AIV.Param, NewValue);
            }
            return RequestBody;
        }

        public void ParseOutput()
        {

            string ResponseMessage = ReturnValues.FirstOrDefault(x => x.Param == "Response:").Actual;
            ParseNodesToReturnParams(this, ResponseMessage);
        }

        /// <summary>  
        /// Decrypts the given password. If the password is an expression, it evaluates the expression first.  
        /// </summary>  
        /// <param name="password">The password to decrypt.</param>  
        /// <param name="isPasswordValueExpression">Indicates if the password is an expression that needs to be evaluated.</param>  
        /// <returns>The decrypted password.</returns>  
        public string DecryptPassword(string password, bool isPasswordValueExpression)
        {
            string decryptedPassword = string.Empty;

            if (isPasswordValueExpression)
            {
                this.ValueExpression.Value = password;
                string evaluatedValue = this.ValueExpression.ValueCalculated;

                decryptedPassword = EncryptionHandler.IsStringEncrypted(evaluatedValue) ? EncryptionHandler.DecryptwithKey(evaluatedValue) : evaluatedValue;
            }
            else
            {
                decryptedPassword = EncryptionHandler.IsStringEncrypted(password) ? EncryptionHandler.DecryptwithKey(password) : password;
            }

            return decryptedPassword;
        }
    }
}
