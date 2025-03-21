#region License
/*
Copyright © 2014-2025 European Support Limited

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace GingerWPF.ApplicationModelsLib.APIModels
{
    public class WSDLParser : APIConfigurationsDocumentParserBase
    {

        private List<string> RegularTypesList = ["date", "anyType", "string", "byte", "double", "short", "int", "char", "long", "boolean", "normalizedString", "dateTime", "decimal", "integer", "Array"];
        private List<string> HeaderNamesList = ["header", "MessageHeader", "Hdr", "Header"];

        string tab1 = " ";
        string tab2 = "  ";
        string tab3 = "   ";

        private Dictionary<string, List<string>> AllNameSpaces = [];
        private Dictionary<List<string>, string> AllSourcesNameSpaces = [];
        private Dictionary<string, int> AllPlaceHolders = [];
        private List<Element> ElementsList = [];
        private List<ComplexType> ComplexTypesList = [];
        private ObservableList<ApplicationAPIModel> mAAMList = [];
        private List<Tuple<string, string>> AllURLs = [];
        private List<ServiceDescriptionExtended> mServiceDescriptionsExtendedList = [];
        private BindingCollection bindColl;
        private ServiceCollection Services;
        private PortTypeCollection portTypColl;
        private MessageCollection Messages;
        private List<XmlSchemasExtended> mSchemasList = [];
        private string mURL;
        public bool ErrorFound;
        public string ErrorReason;
        public string LogFile;
        public bool mStopParsing;



        public override ObservableList<ApplicationAPIModel> ParseDocument(string URL, ObservableList<ApplicationAPIModel> AAMSList, bool avoidDuplicatesNodes = false)
        {
            mURL = URL;
            mAAMList = AAMSList;
            AddServiceDescription(URL);

            //Make it recursivly
            ImportAllServiceDescription(mServiceDescriptionsExtendedList[0].mServiceDescription.Imports, mServiceDescriptionsExtendedList[0].ContainingFolder);




            foreach (ServiceDescriptionExtended SDExtended in mServiceDescriptionsExtendedList)
            {
                if (SDExtended.mServiceDescription.Types.Schemas.Count > 0)
                {
                    XmlSchemasExtended xmlSchemasExtended = new XmlSchemasExtended() { mXmlSchemas = SDExtended.mServiceDescription.Types.Schemas, ContainingFolder = SDExtended.ContainingFolder };

                    mSchemasList.Add(xmlSchemasExtended);
                }


                if (bindColl == null && Services == null && portTypColl == null && Messages == null && Messages == null)
                {
                    bindColl = SDExtended.mServiceDescription.Bindings;
                    Services = SDExtended.mServiceDescription.Services;
                    portTypColl = SDExtended.mServiceDescription.PortTypes;
                    Messages = SDExtended.mServiceDescription.Messages;
                }
                else
                {
                    foreach (Binding b in SDExtended.mServiceDescription.Bindings)
                    {
                        if (bindColl[b.Name] == null)
                        {
                            bindColl.Add(b);
                        }
                    }
                    foreach (Service s in SDExtended.mServiceDescription.Services)
                    {
                        if (Services[s.Name] == null)
                        {
                            Services.Add(s);
                        }
                    }
                    foreach (PortType p in SDExtended.mServiceDescription.PortTypes)
                    {
                        if (portTypColl[p.Name] == null)
                        {
                            portTypColl.Add(p);
                        }
                    }
                    foreach (Message m in SDExtended.mServiceDescription.Messages)
                    {
                        if (Messages[m.Name] == null)
                        {
                            Messages.Add(m);
                        }
                    }
                }

            }



            PopulateAllURLsList();

            GetAllElementsAndComplexTypesFromMainSchema();

            PullDataIntoComplexTypesAndElementsLists();

            CreateApplicationAPIModels();

            String timeStamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
            string UserTempPath = Path.Combine(Path.GetTempPath(), "GingerTemp");
            if (!Directory.Exists(UserTempPath))
            {
                Directory.CreateDirectory(UserTempPath);
            }
            string UserTempFile = Path.Combine(UserTempPath, "APIParserLogFile" + timeStamp + ".log");
            File.WriteAllText(UserTempFile, LogFile);

            return mAAMList;
        }

        private void ImportAllServiceDescription(ImportCollection imports, string containingFolder)
        {

            foreach (Import import in imports)
            {
                if (import.Location.ToUpper().EndsWith("WSDL"))
                {
                    string CompleteURL = GetCompleteURL(import.Location, containingFolder);

                    ImportCollection importsList = AddServiceDescription(CompleteURL);
                    if (importsList.Count > 0)
                    {
                        ImportAllServiceDescription(importsList, containingFolder);
                    }

                }
            }
        }

        private ImportCollection AddServiceDescription(string URL)
        {
            XmlTextReader reader = new XmlTextReader(URL);

            ServiceDescription sd = ServiceDescription.Read(reader);
            string containingFolder = GetContainingFolderFromURL(URL);
            ServiceDescriptionExtended serviceDescriptionExtended = new ServiceDescriptionExtended() { mServiceDescription = sd, ContainingFolder = containingFolder };

            mServiceDescriptionsExtendedList.Add(serviceDescriptionExtended);

            return sd.Imports;
        }

        private void CreateApplicationAPIModels()
        {
            foreach (Binding binding in bindColl)
            {
                foreach (OperationBinding operation in binding.Operations)
                {
                    if (mStopParsing)
                    {
                        return;
                    }

                    ApplicationAPIModel AAM = new ApplicationAPIModel();

                    if (operation.Extensions.Count != 0)
                    {
                        if (operation.Extensions[0] is SoapOperationBinding)
                        {
                            AAM.SOAPAction = ((SoapOperationBinding)operation.Extensions[0]).SoapAction;
                        }
                        else if (operation.Extensions[0] is SoapOperationBinding)
                        {
                            AAM.SOAPAction = ((HttpOperationBinding)operation.Extensions[0]).Location;
                        }
                        else
                        {
                            break;
                        }
                    }

                    AAM.APIType = ApplicationAPIUtils.eWebApiType.SOAP;
                    string BindingName = binding.Name;
                    string OperationName = operation.Name;
                    AAM.Name = operation.Name + "_" + binding.Name;


                    if (binding.Name.EndsWith("12"))
                    {
                        AAM.ReqHttpVersion = ApplicationAPIUtils.eHttpVersion.HTTPV10;
                    }
                    else
                    {
                        AAM.ReqHttpVersion = ApplicationAPIUtils.eHttpVersion.HTTPV11;
                    }

                    string SoapEnvelopeURL = "http://schemas.xmlsoap.org/soap/envelope/";


                    PortTypeOperationDetails portTypeOperationDetails = GetOperationInputMessage(operation, portTypColl);
                    BindingOperationInputTag OperationInputTag = GetOperationInputTagByOperation(operation);
                    List<Part> messagePartsList = GetOperationInputParts(portTypeOperationDetails.InputMessageName, Messages, OperationInputTag);

                    AAM.EndpointURL = GetEndPointURL(Services, BindingName);
                    AAM.Description = GetDescription(portTypColl, BindingName, OperationName);
                    ObservableList<AppModelParameter> AMPList = [];

                    string RequestBody = CreateRequestBody(messagePartsList, portTypeOperationDetails, OperationInputTag, SoapEnvelopeURL, OperationName, AMPList);

                    if (!string.IsNullOrEmpty(RequestBody))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(RequestBody);
                        XMLDocExtended XDE = new XMLDocExtended(doc);
                        IEnumerable<XMLDocExtended> NodeList = XDE.GetEndingNodes(false);
                        foreach (XMLDocExtended XDN in NodeList)
                        {

                            string UniqPlaceHolder = "{" + GetPlaceHolderName(XDN.LocalName.ToUpper()) + "}";
                            XDN.Value = UniqPlaceHolder;
                            AMPList.Add(new AppModelParameter(UniqPlaceHolder, string.Empty, XDN.LocalName, XDN.XPath, []));

                            if (XDN.Attributes != null && XDN.Attributes.Count > 0)
                            {
                                foreach (XmlAttribute XmlAttribute in XDN.Attributes)
                                {
                                    string UniqAttributePlaceHolder = "{" + GetPlaceHolderName(XmlAttribute.LocalName.ToUpper()) + "}";
                                    XmlAttribute.Value = UniqAttributePlaceHolder;
                                    AMPList.Add(new AppModelParameter(UniqAttributePlaceHolder, string.Empty, XmlAttribute.LocalName, XDN.XPath, []));
                                }
                            }
                        }
                        AAM.RequestBody = XDE.XMLString;
                        AAM.AppModelParameters = AMPList;
                    }

                    //Output Template Creation
                    //string OperationOutputMessage = GetOperationOutputMessage(operation.Name, portTypColl);
                    PortTypeOperationDetails portTypeOperationOutputDetails = GetOperationOutputMessage(operation, portTypColl);
                    BindingOperationInputTag OperationOutputTag = GetOperationOutputTagByOperation(operation);
                    List<Part> messagePartsOutputList = GetOperationInputParts(portTypeOperationOutputDetails.InputMessageName, Messages, OperationInputTag);


                    //MessageInputPart MessageOutputPart = GetMessagePartsByOperation(OperationOutputMessage, Messages, OperationOutputTag);

                    ObservableList<AppModelParameter> AMPListOutput = [];
                    string ResponseBody = CreateRequestBody(messagePartsOutputList, portTypeOperationOutputDetails, OperationOutputTag, SoapEnvelopeURL, OperationName, AMPListOutput);





                    //if (!string.IsNullOrEmpty(ResponseBody))
                    //{
                    //    XmlDocument docResponseBody = new XmlDocument();
                    //    docResponseBody.LoadXml(ResponseBody);
                    //    XMLDocExtended XDEResponseBody = new XMLDocExtended(docResponseBody);
                    //    IEnumerable<XMLDocExtended> NodeListResponseBody = XDEResponseBody.GetEndingNodes(false);

                    //    foreach (XMLDocExtended XDN in NodeListResponseBody)
                    //    {
                    //        AAM.ReturnValues.Add(new ActReturnValue() { Param = XDN.LocalName, Path = XDN.XPathWithoutNamspaces, Active = true });
                    //    }
                    //}

                    AAM.ReturnValues = XMLTemplateParser.ParseXMLResponseSampleIntoReturnValues(ResponseBody);

                    mAAMList.Add(AAM);
                }

            }
        }

        private List<Part> GetOperationInputParts(string operationInputMessage, MessageCollection messages, BindingOperationInputTag operationInputTag)
        {
            List<Part> MessageParts = [];
            foreach (Message Message in messages)
            {
                if (Message.Name == operationInputMessage || ((operationInputTag != null) && Message.Name == operationInputTag.HeaderMessage))
                {
                    //TODO:check if can be message for OperationInputMessage AND for OperationInputTag
                    foreach (MessagePart messagePart in Message.Parts)
                    {
                        Part part = new Part
                        {
                            PartName = messagePart.Name
                        };

                        if (string.IsNullOrEmpty(messagePart.Element.Name))
                        {
                            part.ElementName = messagePart.Type.Name;
                            part.ElementType = messagePart.Type.Name;
                            part.ElementNameSpace = messagePart.Type.Namespace;
                            part.PartElementType = Part.ePartElementType.Type;
                        }
                        else
                        {
                            part.ElementName = messagePart.Element.Name;
                            part.ElementType = messagePart.Element.Name;
                            part.ElementNameSpace = messagePart.Element.Namespace;
                            part.PartElementType = Part.ePartElementType.Element;
                        }


                        MessageParts.Add(part);
                    }
                }

            }
            return MessageParts;
        }

        private string CreateRequestBody(List<Part> messagePartsList, PortTypeOperationDetails portTypeOperationDetails, BindingOperationInputTag OperationInputTag, string SoapEnvelopeURL, string OperationName, ObservableList<AppModelParameter> AMPList)
        {
            ErrorFound = false;
            ErrorReason = string.Empty;
            AllPlaceHolders.Clear();

            StringBuilder RequestBody = new StringBuilder();
            Dictionary<string, string> NameSpacesToInclude = [];
            PopulateMessageInputPartsNameSpacesName(messagePartsList, NameSpacesToInclude);
            string Path = string.Empty;
            bool HeaderFound = false;

            foreach (Part part in messagePartsList)
            {
                if (HeaderNamesList.Contains(part.PartName) && part.PartElementType == Part.ePartElementType.Element)
                {
                    HeaderFound = true;
                    ComplexType HeadComplexType = GetNextComplexTypeByElementNameAndNameSpace(part.ElementName, part.ElementNameSpace, NameSpacesToInclude);
                    if (HeadComplexType != null)
                    {
                        RequestBody.Append(tab1 + "<soapenv:Header>" + Environment.NewLine);
                        RequestBody.Append(tab2 + "<" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.ElementName + ">" + Environment.NewLine);
                        string PathToPass = HeadComplexType.Source + ":" + HeadComplexType.Name + "/";
                        AddNameSpaceToInclude(NameSpacesToInclude, HeadComplexType.TargetNameSpace);
                        string NameSpaceName = NameSpacesToInclude[HeadComplexType.TargetNameSpace];
                        if (string.IsNullOrEmpty(NameSpaceName))
                        {
                            NameSpaceName = NameSpacesToInclude[part.ElementNameSpace];
                        }

                        AppendComplexTypeElements(RequestBody, null, null, HeadComplexType, NameSpaceName, tab3, NameSpacesToInclude, PathToPass, AMPList);
                        RequestBody.Append(tab2 + "</" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.ElementName + ">" + Environment.NewLine);
                        RequestBody.Append(tab1 + "</soapenv:Header>" + Environment.NewLine);
                    }
                }

            }

            if (!HeaderFound)
            {
                RequestBody.Append(tab1 + "<soapenv:Header/>" + Environment.NewLine);
            }

            RequestBody.Append(tab1 + "<soapenv:Body>" + Environment.NewLine);
            if (portTypeOperationDetails.ParameterOrder != null && !string.IsNullOrEmpty(OperationInputTag.BodyNameSpace))
            {
                AddNameSpaceToInclude(NameSpacesToInclude, OperationInputTag.BodyNameSpace);
                AddNameSpaceToInclude(NameSpacesToInclude, OperationInputTag.BodyEncodingStyle);
                RequestBody.Append(tab2 + "<" + NameSpacesToInclude[OperationInputTag.BodyNameSpace] + ":" + OperationName + tab1 + "soapenv:encodingStyle=\"" + OperationInputTag.BodyEncodingStyle + "\">" + Environment.NewLine);
                tab2 = tab2 + " ";
            }
            foreach (Part part in messagePartsList)
            {

                string startingPartTagToAppend = GetStartingPartTypeToAppend(part, NameSpacesToInclude);
                string endingPartTagToAppend = GetEndingPartTypeToAppend(part, NameSpacesToInclude);
                if (!(HeaderNamesList.Contains(part.PartName) && part.PartElementType == Part.ePartElementType.Element))
                {
                    ComplexType ParametersComplexType = GetNextComplexTypeByElementNameAndNameSpace(part.ElementName, part.ElementNameSpace, NameSpacesToInclude);
                    RequestBody.Append(startingPartTagToAppend);
                    if (ParametersComplexType != null)
                    {
                        string PathToPass = ParametersComplexType.Source + ":" + ParametersComplexType.Name + "/";
                        string NodeXpath = "Envelope/Body/" + part.ElementName + "/";
                        AppendComplexTypeElements(RequestBody, null, null, ParametersComplexType, NameSpacesToInclude[part.ElementNameSpace], tab3, NameSpacesToInclude, PathToPass, AMPList);
                    }
                    RequestBody.Append(endingPartTagToAppend);
                }

            }
            if (portTypeOperationDetails.ParameterOrder != null && !string.IsNullOrEmpty(OperationInputTag.BodyNameSpace))
            {
                tab2 = tab2[1..];
                RequestBody.Append(tab2 + "</" + NameSpacesToInclude[OperationInputTag.BodyNameSpace] + ":" + OperationName + ">" + Environment.NewLine);

            }
            RequestBody.Append(tab1 + "</soapenv:Body>" + Environment.NewLine);

            RequestBody.Append("</soapenv:Envelope>" + Environment.NewLine);

            AppendEnvelope(RequestBody, messagePartsList, SoapEnvelopeURL, NameSpacesToInclude);
            if (ErrorFound)
            {
                AMPList.Clear();
                return string.Empty;
            }
            else
            {
                return RequestBody.ToString();
            }
        }

        private string GetEndingPartTypeToAppend(Part part, Dictionary<string, string> NameSpacesToInclude)
        {
            string endingPartTypeToAppend = string.Empty;
            if (part.PartElementType == Part.ePartElementType.Element)
            {
                endingPartTypeToAppend = tab2 + "</" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.ElementName + ">" + Environment.NewLine;
            }
            else
            {
                endingPartTypeToAppend = tab2 + "</" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.PartName + ">" + Environment.NewLine;
            }

            return endingPartTypeToAppend;
        }

        private string GetStartingPartTypeToAppend(Part part, Dictionary<string, string> NameSpacesToInclude)
        {
            string startingPartTypeToAppend = string.Empty;
            if (part.PartElementType == Part.ePartElementType.Element)
            {
                startingPartTypeToAppend = tab2 + "<" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.ElementName + ">" + Environment.NewLine;
            }
            else
            {
                if (!NameSpacesToInclude.ContainsKey("http://www.w3.org/2001/XMLSchema-instance"))
                {
                    NameSpacesToInclude.Add("http://www.w3.org/2001/XMLSchema-instance", "xsi");
                }

                startingPartTypeToAppend = tab2 + "<" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.PartName + " xsi:type=\"" + NameSpacesToInclude[part.ElementNameSpace] + ":" + part.ElementName + "\" " + "xmlns:" + NameSpacesToInclude[part.ElementNameSpace] + "=\"" + part.ElementNameSpace + "\">" + Environment.NewLine;
            }

            return startingPartTypeToAppend;
        }

        private void AppendEnvelope(StringBuilder requestBody, List<Part> messagePartsList, string soapEnvelopeURL, Dictionary<string, string> NameSpacesToInclude)
        {
            string envelope = "<soapenv:Envelope xmlns:soapenv=\"" + soapEnvelopeURL + "\" ";

            foreach (KeyValuePair<string, string> KVP in NameSpacesToInclude)
            {
                if (!string.IsNullOrEmpty(KVP.Value) && !string.IsNullOrEmpty(KVP.Key))
                {
                    envelope = envelope + "xmlns:" + KVP.Value + "=\"" + KVP.Key + "\" ";
                }
            }

            envelope = envelope + ">" + Environment.NewLine;
            requestBody.Insert(0, envelope);
        }

        private void AppendComplexTypeElements(StringBuilder RequestBody, RefElement RefElement, Element SourceElement, ComplexType ComplexType, string NameSpaceName, string CurrentTab, Dictionary<string, string> NameSpacesToInclude, string Path, ObservableList<AppModelParameter> AMPList)
        {
            if (ErrorFound)
            {
                return;
            }

            if (Regex.Matches(RequestBody.ToString(), Environment.NewLine).Count > 10000)
            {
                ErrorFound = true;
                AMPList.Clear();
                ErrorReason = "Request Passed the 10,000 lines";
                return;
            }

            if (ComplexType.Extension != null)
            {
                ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ComplexType.Extension.BaseName, ComplexType.Extension.BaseNameSpace);
                string PathToPass = Path;
                if (NextComplexType != null)
                {
                    PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/";
                }
                AppendComplexTypeElementWithExtension(RequestBody, RefElement, NextComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);
                if (ComplexType.Extension.ComplexTypeElementsList.Count > 0)
                {
                    AppendComplexTypeElementList(RequestBody, ComplexType.Extension.ComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, Path, AMPList);
                }
            }
            if (ComplexType.Restriction != null)
            {
                if (RegularTypesList.Contains(ComplexType.Restriction.BaseName))
                {
                    ApendElement(RequestBody, SourceElement, NameSpaceName, ComplexType.Restriction.Attributes, CurrentTab, NameSpacesToInclude, AMPList);
                }
                else if (ComplexType.Restriction.BaseNameSpace == "http://www.w3.org/2001/XMLSchema")
                {
                    AddXSDNameSpace(NameSpacesToInclude);
                    ApendElement(RequestBody, SourceElement, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
                }
                else
                {
                    ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ComplexType.Restriction.BaseName, ComplexType.Restriction.BaseNameSpace);
                    string PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/"; //Maybe to put the base too?
                    AppendComplexTypeElementWithRestriction(RequestBody, SourceElement, NextComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, AMPList);
                }
            }

            if (RefElement != null && ComplexType.ComplexTypeElementsList.Count > 0)
            {
                CurrentTab = CurrentTab + tab1;
                AppendComment(RequestBody, CurrentTab, RefElement.MinOccurs, RefElement.MaxOccurs);
                RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + RefElement.Name + ">" + Environment.NewLine);
            }


            AppendComplexTypeElementList(RequestBody, ComplexType.ComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, Path, AMPList);


            if (RefElement != null && ComplexType.ComplexTypeElementsList.Count > 0)
            {
                RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + RefElement.Name + ">" + Environment.NewLine);
            }
        }

        private void AppendComment(StringBuilder RequestBody, string CurrentTab, decimal MinOccurs, decimal MaxOccurs)
        {
            if (MinOccurs == 0 && MaxOccurs == 1)
            {
                RequestBody.Append(CurrentTab + "<!--Optional:-->" + Environment.NewLine);
            }
            else if (MaxOccurs != 1 && MinOccurs != MaxOccurs)
            {
                RequestBody.Append(CurrentTab + "<!--Zero or more repetitions:-->" + Environment.NewLine);
            }
            else if (MinOccurs == 0 && MaxOccurs == 0)
            {
                //Do Nothing
            }
            else if (MinOccurs == MaxOccurs)
            {
            }
        }

        private void AddXSDNameSpace(Dictionary<string, string> NameSpacesToInclude)
        {
            if (!NameSpacesToInclude.ContainsKey("http://www.w3.org/2001/XMLSchema"))
            {
                NameSpacesToInclude.Add("http://www.w3.org/2001/XMLSchema", "xsd");
            }
        }


        private void AppendComplexTypeElementList(StringBuilder RequestBody, List<ComplexTypeChild> ComplexTypeElementsList, RefElement RefElement, Element SourceElement, ComplexType ComplexType, string NameSpaceName, string CurrentTab, Dictionary<string, string> NameSpacesToInclude, string Path, ObservableList<AppModelParameter> AMPList)
        {
            string LastCommentedGroupID = string.Empty;

            foreach (ComplexTypeChild ComplexTypeChild in ComplexTypeElementsList)
            {
                if (!string.IsNullOrEmpty(ComplexTypeChild.ChoiceGroup) && ComplexTypeChild.ChoiceGroup != LastCommentedGroupID)
                {
                    int Choices = ComplexTypeElementsList.Where(x => x.ChoiceGroup == ComplexTypeChild.ChoiceGroup).ToList().Count;
                    bool ChooseIsEqualToListCount = ComplexTypeElementsList.Count == Choices;
                    if (Choices != 0 && !ChooseIsEqualToListCount)
                    {
                        RequestBody.Append(CurrentTab + "<!--You have a CHOICE of the next " + Choices + " items at this level-->" + Environment.NewLine);
                        LastCommentedGroupID = ComplexTypeChild.ChoiceGroup;
                    }
                }

                if (ErrorFound)
                {
                    return;
                }

                if (ComplexTypeChild is Element)
                {
                    Element ChildElement = ComplexTypeChild as Element;

                    if (ChildElement.InnerElementComplexType != null)
                    {
                        if (ChildElement.InnerElementComplexType.Extension != null)
                        {
                            ComplexType BaseComplexType = GetNextComplexTypeByElementNameAndNameSpace(ChildElement.InnerElementComplexType.Extension.BaseName, ChildElement.InnerElementComplexType.Extension.BaseNameSpace, NameSpacesToInclude);
                            string PathToPass = PathToPass = Path + BaseComplexType.Source + ":" + BaseComplexType.Name + "/";
                            if (BaseComplexType != null && BaseComplexType.ComplexTypeElementsList.Count != 0)
                            {
                                List<ComplexTypeChild> CombinedComplexTypeElementsList = [];
                                CombinedComplexTypeElementsList = BaseComplexType.ComplexTypeElementsList.Concat(ChildElement.InnerElementComplexType.Extension.ComplexTypeElementsList).ToList();
                                RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                AppendComplexTypeElementList(RequestBody, CombinedComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpacesToInclude[ChildElement.InnerElementComplexType.Extension.BaseNameSpace], CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                            }
                            else
                            {
                                AppendComplexTypeElementWithExtension(RequestBody, ChildElement, ChildElement.InnerElementComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);
                            }
                        }
                        else
                        {
                            AppendComment(RequestBody, CurrentTab, ChildElement.MinOccurs, ChildElement.MaxOccurs);
                            RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                            string PathToPass = Path + ChildElement.InnerElementComplexType.Source + ":" + ChildElement.InnerElementComplexType.Name + "/";
                            AppendComplexTypeElements(RequestBody, null, ChildElement, ChildElement.InnerElementComplexType, NameSpaceName, CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                            RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                        }
                    }
                    else if (ChildElement.InnerElementRestriction != null)
                    {
                        if (RegularTypesList.Contains(ChildElement.InnerElementRestriction.BaseName))
                        {
                            ApendElement(RequestBody, ChildElement, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
                        }
                        else if (ChildElement.InnerElementRestriction.BaseNameSpace == "http://www.w3.org/2001/XMLSchema")
                        {
                            AddXSDNameSpace(NameSpacesToInclude);
                            ApendElement(RequestBody, ChildElement, NameSpaceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                            continue;
                        }
                        else
                        {
                            ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ChildElement.InnerElementRestriction.BaseName, ChildElement.InnerElementRestriction.BaseNameSpace);
                            string PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/"; //Maybe to put the base too?
                            AppendComplexTypeElementWithRestriction(RequestBody, ChildElement, NextComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, AMPList);
                        }
                    }
                    else if (string.IsNullOrEmpty(ChildElement.Type) || RegularTypesList.Contains(ChildElement.Type))
                    {
                        ApendElement(RequestBody, ChildElement, NameSpaceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                    }
                    else
                    {
                        ComplexType NextComplexType = GetNextComplexTypeByElementNameAndNameSpace(ChildElement.Type, ChildElement.TypeNameSpace, NameSpacesToInclude);

                        if (ChildElement.TypeNameSpace == "http://www.w3.org/2001/XMLSchema")
                        {
                            AddXSDNameSpace(NameSpacesToInclude);
                            ApendElement(RequestBody, ChildElement, NameSpaceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                            continue;
                        }
                        if (NextComplexType == null)
                        {

                        }
                        string NodeElement = NextComplexType.Source + ":" + NextComplexType.Name + "/";
                        if (Path.Contains(NodeElement))
                        {
                            AppendComment(RequestBody, CurrentTab, ComplexTypeChild.MinOccurs, ComplexTypeChild.MaxOccurs);
                            AppendEmptyElement(RequestBody, ComplexTypeChild, NameSpaceName, CurrentTab, NameSpacesToInclude);
                        }
                        else
                        {
                            string PathToPass = Path + NodeElement;
                            if (NextComplexType.Extension != null)
                            {
                                if (RegularTypesList.Contains(NextComplexType.Extension.BaseName))
                                {
                                    ApendElement(RequestBody, ChildElement, NameSpaceName, NextComplexType.Extension.Attributes, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                                }
                                else
                                {
                                    ComplexType BaseComplexType = GetNextComplexTypeByElementNameAndNameSpace(NextComplexType.Extension.BaseName, NextComplexType.Extension.BaseNameSpace, NameSpacesToInclude);
                                    if (BaseComplexType != null && BaseComplexType.ComplexTypeElementsList.Count != 0)
                                    {
                                        List<ComplexTypeChild> CombinedComplexTypeElementsList = [];
                                        CombinedComplexTypeElementsList = BaseComplexType.ComplexTypeElementsList.Concat(NextComplexType.Extension.ComplexTypeElementsList).ToList();
                                        RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                        AppendComplexTypeElementList(RequestBody, CombinedComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpacesToInclude[NextComplexType.Extension.BaseNameSpace], CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                        RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                    }
                                    else if (BaseComplexType != null && BaseComplexType.ComplexTypeElementsList.Count == 0)
                                    {

                                        AppendComplexTypeElementWithExtension(RequestBody, ChildElement, NextComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);
                                    }
                                    else if (NextComplexType.Extension.ComplexTypeElementsList.Count != 0)
                                    {
                                        RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                        AppendComplexTypeElementList(RequestBody, NextComplexType.Extension.ComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpacesToInclude[NextComplexType.Extension.BaseNameSpace], CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                        RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                    }
                                }
                            }
                            else if (NextComplexType.Restriction != null)
                            {
                                if (NextComplexType.Restriction.ComplexTypeElementsList.Count != 0)
                                {
                                    RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                    AppendComplexTypeElementList(RequestBody, NextComplexType.Restriction.ComplexTypeElementsList, RefElement, SourceElement, ComplexType, NameSpacesToInclude[NextComplexType.Restriction.BaseNameSpace], CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                    RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                }
                                else
                                {
                                    AppendComplexTypeElements(RequestBody, null, ChildElement, NextComplexType, NameSpaceName, CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                }

                            }
                            else
                            {
                                AppendComment(RequestBody, CurrentTab, ChildElement.MinOccurs, ChildElement.MaxOccurs);
                                RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                                AppendComplexTypeElements(RequestBody, null, ChildElement, NextComplexType, NameSpacesToInclude[ChildElement.TypeNameSpace], CurrentTab + tab1, NameSpacesToInclude, PathToPass, AMPList);
                                RequestBody.Append(CurrentTab + "</" + NameSpaceName + ":" + ChildElement.Name + ">" + Environment.NewLine);
                            }
                        }
                    }
                }
                else if (ComplexTypeChild is RefElement)
                {

                    RefElement RefElementChild = ComplexTypeChild as RefElement;

                    if (RefElementChild.RefNameSpace == "http://www.w3.org/2001/XMLSchema")
                    {
                        AddXSDNameSpace(NameSpacesToInclude);
                        ApendElement(RequestBody, RefElementChild, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
                        break;
                    }

                    ComplexType NextComplexType = GetNextComplexTypeByElementNameAndNameSpace(RefElementChild.Name, RefElementChild.RefNameSpace, NameSpacesToInclude);

                    if (NextComplexType != null)
                    {
                        string NodeRefElement = NextComplexType.Source + ":" + NextComplexType.Name;
                        if (Path.Contains(NodeRefElement))
                        {
                            AppendComment(RequestBody, CurrentTab, ComplexTypeChild.MinOccurs, ComplexTypeChild.MaxOccurs);
                            AppendEmptyElement(RequestBody, ComplexTypeChild, NameSpaceName, CurrentTab, NameSpacesToInclude);
                        }
                        else
                        {
                            string PathToPass = Path;
                            if (NextComplexType != null)
                            {
                                PathToPass = Path + NodeRefElement + "/";
                                AppendComplexTypeElements(RequestBody, RefElementChild, null, NextComplexType, NameSpacesToInclude[RefElementChild.RefNameSpace], CurrentTab, NameSpacesToInclude, PathToPass, AMPList);
                            }
                        }
                    }
                    else
                    {
                        Element Element = GetElementByNameAndNameSpace(RefElementChild.Name, RefElementChild.RefNameSpace);
                        if (Element == null)
                        {
                            return;
                        }
                        if (RegularTypesList.Contains(Element.Type) || Element.TypeNameSpace == "http://www.w3.org/2001/XMLSchema")
                        {
                            AddXSDNameSpace(NameSpacesToInclude);
                            ApendElement(RequestBody, RefElementChild, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
                        }
                        else if (Element.InnerElementRestriction != null)
                        {
                            if (RegularTypesList.Contains(Element.InnerElementRestriction.BaseName))
                            {
                                AppendComment(RequestBody, CurrentTab, RefElementChild.MinOccurs, RefElementChild.MaxOccurs);
                                ApendElement(RequestBody, Element, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
                            }
                            else if (Element.InnerElementRestriction.BaseNameSpace == "http://www.w3.org/2001/XMLSchema")
                            {
                                AddXSDNameSpace(NameSpacesToInclude);
                                AppendComment(RequestBody, CurrentTab, RefElementChild.MinOccurs, RefElementChild.MaxOccurs);
                                ApendElement(RequestBody, Element, NameSpaceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                                continue;
                            }
                            else
                            {
                                ComplexType NextRestrictionComplexType = GetComplexTypeByNameAndNameSpace(Element.InnerElementRestriction.BaseName, Element.InnerElementRestriction.BaseNameSpace);
                                string PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/"; //Maybe to put the base too?
                                AppendComplexTypeElementWithRestriction(RequestBody, Element, NextComplexType, NameSpaceName, CurrentTab, NameSpacesToInclude, AMPList);
                            }
                        }

                        else
                        {
                            LogFile += "<<<<<ERROR: ComplexType-" + RefElementChild.Name + " CANNOT BE FOUND ON NAMESPACE" + RefElementChild.RefNameSpace + Environment.NewLine;
                            ErrorFound = true;
                            ErrorReason = "<<<<<ERROR: ComplexType-" + RefElementChild.Name + " CANNOT BE FOUND ON NAMESPACE" + RefElementChild.RefNameSpace;
                        }
                    }
                }
            }
        }

        private void AddNameSpaceToInclude(Dictionary<string, string> NameSpacesToInclude, string TargetNameSpace)
        {
            if (!NameSpacesToInclude.ContainsKey(TargetNameSpace))
            {
                NameSpacesToInclude.Add(TargetNameSpace, GetNameWithoutDots(TargetNameSpace, NameSpacesToInclude.Values.ToList()));
            }
        }

        private void AppendEmptyElement(StringBuilder RequestBody, ComplexTypeChild ComplexTypeChild, string NameSpaceName, string CurrentTab, Dictionary<string, string> NameSpacesToInclude)
        {
            RequestBody.Append(CurrentTab + "<" + NameSpaceName + ":" + ComplexTypeChild.Name + "/>" + Environment.NewLine);
        }

        private void AppendComplexTypeElementWithRestriction(StringBuilder RequestBody, ComplexTypeChild SourceElement, ComplexType nextComplexType, string NameSpaceName, string CurrentTab, Dictionary<string, string> NameSpacesToInclude, ObservableList<AppModelParameter> AMPList)
        {
            //TODO: Add Validation for the values which about to be entered
            ApendElement(RequestBody, SourceElement, NameSpaceName, null, CurrentTab, NameSpacesToInclude, AMPList);
        }

        private void AppendComplexTypeElementWithExtension(StringBuilder RequestBody, ComplexTypeChild ComplexTypeChildToAppend, ComplexType ComplexType, string NameSapceName, string CurrentTab, Dictionary<string, string> NameSpacesToInclude, string Path, ObservableList<AppModelParameter> AMPList)
        {

            if (ComplexType == null)
            {
                ApendElement(RequestBody, ComplexTypeChildToAppend, NameSapceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                return;
            }
            else if (ComplexType.Extension != null)
            {
                if (RegularTypesList.Contains(ComplexType.Extension.BaseName))
                {
                    ApendElement(RequestBody, ComplexTypeChildToAppend, NameSapceName, ComplexType.Extension.Attributes, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                }
                else
                {
                    ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ComplexType.Extension.BaseName, ComplexType.Extension.BaseNameSpace);
                    string PathToPass = string.Empty;
                    if (NextComplexType != null)
                    {
                        PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/";
                    }
                    else
                    {
                        PathToPass = Path;
                    }

                    AppendComplexTypeElementWithExtension(RequestBody, ComplexTypeChildToAppend, NextComplexType, NameSapceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);
                    if (ComplexType.Extension.ComplexTypeElementsList.Count > 0)
                    {
                        AppendComplexTypeElementList(RequestBody, ComplexType.Extension.ComplexTypeElementsList, null, null, ComplexType, NameSapceName, CurrentTab, NameSpacesToInclude, Path, AMPList);
                    }

                }
            }
            else if (ComplexType.Restriction != null)
            {
                //TODO Put it in a funciton

                if (RegularTypesList.Contains(ComplexType.Restriction.BaseName))
                {
                    ApendElement(RequestBody, ComplexTypeChildToAppend, NameSapceName, ComplexType.Restriction.Attributes, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                }
                else
                {
                    ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ComplexType.Restriction.BaseName, ComplexType.Restriction.BaseNameSpace);
                    string PathToPass = string.Empty;
                    if (NextComplexType != null)
                    {
                        PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/";
                    }
                    else
                    {
                        PathToPass = Path;
                    }

                    AppendComplexTypeElementWithExtension(RequestBody, ComplexTypeChildToAppend, NextComplexType, NameSapceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);


                }
            }

            foreach (ComplexTypeChild ComplexTypeChild in ComplexType.ComplexTypeElementsList)
            {
                if (ComplexTypeChild is Element)
                {
                    Element ChildElement = ComplexTypeChild as Element;
                    if (RegularTypesList.Contains(ChildElement.Type))
                    {
                        if (ComplexTypeChildToAppend == null)
                        {
                            ComplexTypeChildToAppend = ChildElement;
                        }

                        ApendElement(RequestBody, ComplexTypeChildToAppend, NameSapceName, null, CurrentTab + tab1, NameSpacesToInclude, AMPList);
                        ComplexTypeChildToAppend = null;
                    }
                    else
                    {
                        ComplexType NextComplexType = GetComplexTypeByNameAndNameSpace(ChildElement.Type, ChildElement.TypeNameSpace);
                        string NodeElement = NextComplexType.Source + ":" + NextComplexType.Name + "/";
                        if (Path.Contains(NodeElement))
                        {
                            AppendComment(RequestBody, CurrentTab, ComplexTypeChild.MinOccurs, ComplexTypeChild.MaxOccurs);
                            AppendEmptyElement(RequestBody, ComplexTypeChild, NameSapceName, CurrentTab, NameSpacesToInclude);

                        }
                        else
                        {
                            string PathToPass = string.Empty;
                            if (NextComplexType != null)
                            {
                                PathToPass = Path + NextComplexType.Source + ":" + NextComplexType.Name + "/";
                            }
                            else
                            {
                                PathToPass = Path;
                            }

                            AppendComplexTypeElementWithExtension(RequestBody, ComplexTypeChildToAppend, NextComplexType, NameSapceName, CurrentTab, NameSpacesToInclude, PathToPass, AMPList);

                        }



                    }
                }
                else if (ComplexTypeChild is RefElement)
                {
                    LogFile += "Unhandled option2 on AppendComplexTypeElementWithExtension" + Environment.NewLine;
                    //TODO
                }
            }
        }

        private void ApendElement(StringBuilder RequestBody, ComplexTypeChild ComplexTypeChild, string NameSapceName, List<string> Attributes, string CurrentTab, Dictionary<string, string> NameSpacesToInclude, ObservableList<AppModelParameter> AMPList)
        {
            string AttributesString = string.Empty;
            string FullTagName = string.Empty;
            if (ComplexTypeChild != null)
            {
                if (string.IsNullOrEmpty(NameSapceName))
                {
                    FullTagName = ComplexTypeChild.Name;
                }
                else
                {
                    FullTagName = NameSapceName + ":" + ComplexTypeChild.Name;
                }

                if (Attributes != null && Attributes.Count != 0)
                {
                    AttributesString = GetAttributeStringByAttributesList(Attributes);
                }

                if (ComplexTypeChild.MinOccurs == 0 && ComplexTypeChild.MaxOccurs == 1)
                {
                    RequestBody.Append(CurrentTab + "<!--Optional:-->" + Environment.NewLine);
                    AppendRow(RequestBody, CurrentTab, FullTagName, AttributesString);
                }
                else if (ComplexTypeChild.MaxOccurs != 1 && ComplexTypeChild.MinOccurs != ComplexTypeChild.MaxOccurs)
                {
                    RequestBody.Append(CurrentTab + "<!--Zero or more repetitions:-->" + Environment.NewLine);
                    AppendRow(RequestBody, CurrentTab, FullTagName, AttributesString);
                }
                else if (ComplexTypeChild.MinOccurs == 0 && ComplexTypeChild.MaxOccurs == 0)
                {
                    AppendRow(RequestBody, CurrentTab, FullTagName, AttributesString);
                }
                else if (ComplexTypeChild.MinOccurs == ComplexTypeChild.MaxOccurs)
                {
                    for (int i = 1; i <= ComplexTypeChild.MaxOccurs; i++)
                    {
                        AppendRow(RequestBody, CurrentTab, FullTagName, AttributesString);
                    }
                }
            }
        }

        private void AppendRow(StringBuilder RequestBody, string CurrentTab, string FullTagName, string AttributesString)
        {
            RequestBody.Append(CurrentTab + "<" + FullTagName + AttributesString + "></" + FullTagName + ">" + Environment.NewLine);
        }

        private string GetPlaceHolderName(string ElementName)
        {
            string PlaceHolderName = ElementName.ToUpper();
            if (AllPlaceHolders.ContainsKey(PlaceHolderName))
            {
                AllPlaceHolders[PlaceHolderName]++;
                return ElementName.ToUpper() + AllPlaceHolders[PlaceHolderName];
            }
            else
            {
                AllPlaceHolders.Add(ElementName.ToUpper(), 0);
                return ElementName.ToUpper();
            }
        }

        public string GetStringWithoutDigits(string s)
        {
            char[] toBeRemoved = "1234567890".ToCharArray();
            string result = s.TrimEnd(toBeRemoved);
            return result;
        }

        private string GetAttributeStringByAttributesList(List<string> attributes)
        {
            string AttributesString = string.Empty + " ";
            foreach (string attribute in attributes)
            {
                AttributesString = AttributesString + attribute + "=" + "\"\" ";
            }
            return AttributesString;
        }

        public ComplexType GetComplexTypeByElement(Element Element)
        {
            if (AllNameSpaces.ContainsKey(Element.TypeNameSpace))
            {
                List<string> ComplexElementSource = AllNameSpaces[Element.TypeNameSpace];
                ComplexType ComplexType = ComplexTypesList.FirstOrDefault(x => x.Name == Element.Type && ComplexElementSource.Contains(x.Source));

                return ComplexType;
            }
            else
            {
                return null;
            }
        }

        public ComplexType GetComplexTypeByNameAndNameSpace(string ComplexTypeName, string ComplexTypeNameSpace)
        {
            List<string> ComplexElementSource = [];
            if (AllNameSpaces.ContainsKey(ComplexTypeNameSpace))
            {
                ComplexElementSource = AllNameSpaces[ComplexTypeNameSpace];
            }

            ComplexType ComplexType = null;
            if (ComplexElementSource.Count != 0)
            {
                ComplexType = ComplexTypesList.FirstOrDefault(x => x.Name == ComplexTypeName && ComplexElementSource.Contains(x.Source.Replace("%20", " ")));
            }
            else
            {
                ComplexType = ComplexTypesList.FirstOrDefault(x => x.Name == ComplexTypeName && x.TargetNameSpace == ComplexTypeNameSpace);
            }

            if (ComplexType == null)
            {
                ComplexType = ComplexTypesList.FirstOrDefault(x => x.Name == ComplexTypeName);
            }

            return ComplexType;
        }

        public Element GetElementByNameAndNameSpace(string ElementName, string ElementNameSpace)
        {
            List<string> ElementSource = [];
            if (AllNameSpaces.ContainsKey(ElementNameSpace))
            {
                ElementSource = AllNameSpaces[ElementNameSpace];
            }

            Element Element = null;
            if (ElementSource.Count != 0)
            {
                Element = ElementsList.FirstOrDefault(x => x.Name == ElementName && ElementSource.Contains(x.Source.Replace("%20", " ")));
            }
            else
            {
                Element = ElementsList.FirstOrDefault(x => x.Name == ElementName);
            }

            if (Element == null)
            {
                Element = ElementsList.FirstOrDefault(x => x.Name == ElementName && string.IsNullOrEmpty(x.Source));
            }
            return Element;
        }

        public ComplexType GetNextComplexTypeByElementNameAndNameSpace(string ElementName, string ElementNameSpace, Dictionary<string, string> NameSpacesToInclude)
        {
            Element Element = GetElementByNameAndNameSpace(ElementName, ElementNameSpace);
            ComplexType ComplexType = null;
            if (Element != null)
            {
                ComplexType = GetComplexTypeByElement(Element);
                if (ComplexType == null)
                {
                    ComplexType = GetComplexTypeByNameAndNameSpace(ElementName, ElementNameSpace);
                }
                if (ComplexType == null)
                {
                    ComplexType = GetComplexTypeByNameAndNameSpace(Element.Type, Element.TypeNameSpace);
                }
            }
            else
            {
                ComplexType = GetComplexTypeByNameAndNameSpace(ElementName, ElementNameSpace);
            }
            AddNameSpaceToInclude(NameSpacesToInclude, ElementNameSpace);

            if (ComplexType == null)
            {
                LogFile += "ComplexType:" + ElementName + "Not found on:" + ElementNameSpace + Environment.NewLine;
            }
            return ComplexType;
        }


        private void PopulateMessageInputPartsNameSpacesName(List<Part> messagePartsList, Dictionary<string, string> NameSpacesToInclude)
        {
            string NameSpaceName = string.Empty;
            List<string> NameSpaceNames = [];
            foreach (Part part in messagePartsList)
            {

                if (!NameSpacesToInclude.ContainsKey(part.ElementNameSpace))
                {
                    NameSpaceName = GetNameWithoutDots(part.ElementNameSpace, NameSpacesToInclude.Values.ToList());
                    NameSpacesToInclude.Add(part.ElementNameSpace, NameSpaceName);
                }
            }
        }

        private string GetNameWithoutDots(string headNameSpace, List<string> AllNameSpacesShortCuts = null)
        {
            //TODO:Get A List to make it unique
            if (string.IsNullOrEmpty(headNameSpace))
            {
                return string.Empty;
            }

            string s = string.Empty;
            string WorkURL = string.Empty;
            if (headNameSpace.StartsWith("http"))
            {
                if (headNameSpace[^1] == '/')
                {
                    headNameSpace = headNameSpace[..^1];
                }

                int StartIndex = headNameSpace.LastIndexOf("/");
                if (StartIndex != -1)
                {
                    headNameSpace = headNameSpace[(StartIndex + 1)..];
                }
                if (headNameSpace.ToUpper().StartsWith("WWW"))
                {
                    WorkURL = headNameSpace[4..];
                }
                else
                {
                    WorkURL = headNameSpace.ToLower();
                }
            }
            else
            {
                WorkURL = headNameSpace;
            }

            for (int i = 0; i < 3; i++)
            {
                if (WorkURL.Length <= i)
                {
                    break;
                }
                if (WorkURL[i] is '.' or '_')
                {
                    break;
                }
                else
                {
                    s = s + WorkURL[i];
                }
            }

            int j = 1;
            if (AllNameSpacesShortCuts != null)
            {
                while (true)
                {
                    if (AllNameSpacesShortCuts.Contains(s) || s == "xml")
                    {
                        if (s.Any(c => char.IsDigit(c)))
                        {
                            s = s[..^1] + j;
                        }
                        else
                        {
                            s = s + j;
                        }
                        j = j + 1;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return s;
        }

        #region PullingAndCreatingElements

        private void PullDataIntoComplexTypesAndElementsLists()
        {
            foreach (Tuple<string, string> URLTuple in AllURLs)
            {
                if (mStopParsing)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(URLTuple.Item1))
                {
                    string CompleteURL = GetCompleteURL(URLTuple.Item1, URLTuple.Item2);
                    using (XmlReader reader = XmlReader.Create(CompleteURL))
                    {
                        XmlSchema schema = XmlSchema.Read(reader, null);
                        if (!string.IsNullOrEmpty(schema.TargetNamespace) && !AllNameSpaces.ContainsKey(schema.TargetNamespace))
                        {
                            List<string> AllNameSpaceURLs = [CompleteURL];
                            AllNameSpaces.Add(schema.TargetNamespace, AllNameSpaceURLs);
                            AllSourcesNameSpaces.Add(AllNameSpaceURLs, schema.TargetNamespace);
                        }
                        else if (schema.TargetNamespace != null && AllNameSpaces.ContainsKey(schema.TargetNamespace))
                        {
                            AllNameSpaces[schema.TargetNamespace].Add(CompleteURL);
                            KeyValuePair<List<string>, string> KeyValue = AllSourcesNameSpaces.FirstOrDefault(x => x.Value == schema.TargetNamespace);
                            KeyValue.Key.Add(CompleteURL);
                        }
                        GetAllElementsAndComplexTypesFromImportedSchema(schema);
                    }
                }
            }
        }

        private string GetSourceByReferanceType(string SourceURI)
        {
            if (SourceURI != null)
            {
                if (SourceURI.StartsWith("file"))
                {
                    return Path.GetFullPath(new Uri(SourceURI).AbsolutePath);
                }
                else
                {
                    return SourceURI;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        private void GetAllElementsAndComplexTypesFromImportedSchema(XmlSchema XmlSchema)
        {
            foreach (object item in XmlSchema.Items)
            {
                if (mStopParsing)
                {
                    return;
                }

                if (item is XmlSchemaElement)
                {
                    XmlSchemaElement XmlSchemaElement = item as XmlSchemaElement;
                    Element Element = new Element
                    {
                        Name = XmlSchemaElement.Name,
                        Source = GetSourceByReferanceType(XmlSchemaElement.SourceUri),
                        TargetNameSpace = XmlSchemaElement.QualifiedName.Namespace,

                        Type = XmlSchemaElement.SchemaTypeName.Name,
                        TypeNameSpace = XmlSchemaElement.SchemaTypeName.Namespace
                    };

                    ElementsList.Add(Element);
                    if (XmlSchemaElement.SchemaType is XmlSchemaComplexType)
                    {
                        ComplexType NewComplexType = CreateComplexType(XmlSchemaElement, XmlSchemaElement.SchemaType as XmlSchemaComplexType);
                        ComplexTypesList.Add(NewComplexType);
                    }
                    else if (XmlSchemaElement.SchemaType is XmlSchemaSimpleType)
                    {
                        Restriction restriction = new Restriction();
                        //Remove and create just restriction as its in the WSDL element
                        ComplexType NewComplexType = CreateSimpleType(XmlSchemaElement.SchemaType as XmlSchemaSimpleType, ref restriction);
                        Element.InnerElementRestriction = restriction;
                        ComplexTypesList.Add(NewComplexType);
                    }
                    else
                    {
                        LogFile += "Element got missed Type:" + XmlSchemaElement.SchemaType + "is not XmlSchemaComplexType or XmlSchemaSimpleType" + Environment.NewLine;
                    }
                }
                else if (item is XmlSchemaComplexType)
                {

                    XmlSchemaComplexType XmlSchemaComplexType = item as XmlSchemaComplexType;


                    XmlSchemaParticle XmlSchemaParticle = XmlSchemaComplexType.Particle;
                    List<ComplexTypeChild> ComplexTypeElementsList = [];
                    List<ComplexTypeChild> ExtenstionComplexTypeElementsList = [];
                    Extension ExtensionChild = null;
                    Restriction RestrictionChild = null;
                    if (XmlSchemaParticle is XmlSchemaSequence)
                    {
                        XmlSchemaSequence XmlSchemaSequence = XmlSchemaParticle as XmlSchemaSequence;
                        ComplexTypeElementsList = GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaSequence.Items);

                    }
                    else if (XmlSchemaParticle is XmlSchemaAll)
                    {
                        XmlSchemaAll XmlSchemaAll = XmlSchemaParticle as XmlSchemaAll;
                        ComplexTypeElementsList = GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaAll.Items);

                    }
                    else if (XmlSchemaParticle is XmlSchemaChoice)
                    {
                        XmlSchemaChoice XmlSchemaChoice = XmlSchemaParticle as XmlSchemaChoice;
                        List<ComplexTypeChild> ComplexTypeElementsChoisesList = GetElementListFromSchemaChoice(XmlSchemaChoice);
                        foreach (ComplexTypeChild CTC in ComplexTypeElementsChoisesList)
                        {
                            ComplexTypeElementsList.Add(CTC);
                        }
                    }
                    else if (XmlSchemaComplexType.ContentModel is XmlSchemaSimpleContent)
                    {
                        XmlSchemaSimpleContent XmlSchemaSimpleContent = XmlSchemaComplexType.ContentModel as XmlSchemaSimpleContent;

                        if (XmlSchemaSimpleContent.Content is XmlSchemaSimpleContentExtension)
                        {
                            XmlSchemaSimpleContentExtension XmlSchemaSimpleContentExtension = XmlSchemaSimpleContent.Content as XmlSchemaSimpleContentExtension;
                            Extension Extension = new Extension
                            {
                                BaseName = XmlSchemaSimpleContentExtension.BaseTypeName.Name,
                                BaseNameSpace = XmlSchemaSimpleContentExtension.BaseTypeName.Namespace
                            };
                            if (XmlSchemaSimpleContentExtension.Attributes.Count != 0)
                            {
                                foreach (XmlSchemaAttribute Attribute in XmlSchemaSimpleContentExtension.Attributes)
                                {
                                    Extension.Attributes.Add(Attribute.Name);
                                }
                            }
                            ExtensionChild = Extension;
                        }
                        else if (XmlSchemaSimpleContent.Content is XmlSchemaSimpleContentRestriction)
                        {
                            XmlSchemaSimpleContentRestriction XmlSchemaSimpleContentRestriction = XmlSchemaSimpleContent.Content as XmlSchemaSimpleContentRestriction;
                            Restriction Restriction = new Restriction
                            {
                                BaseName = XmlSchemaSimpleContentRestriction.BaseTypeName.Name,
                                BaseNameSpace = XmlSchemaSimpleContentRestriction.BaseTypeName.Namespace
                            };
                            if (XmlSchemaSimpleContentRestriction.Attributes.Count != 0)
                            {
                                foreach (XmlSchemaAttribute Attribute in XmlSchemaSimpleContentRestriction.Attributes)
                                {
                                    Restriction.Attributes.Add(Attribute.Name);
                                }
                            }
                            RestrictionChild = Restriction;


                        }

                    }
                    else if (XmlSchemaComplexType.ContentModel is XmlSchemaComplexContent)
                    {
                        XmlSchemaComplexContent XmlSchemaComplexContent = XmlSchemaComplexType.ContentModel as XmlSchemaComplexContent;
                        if (XmlSchemaComplexContent.Content is XmlSchemaComplexContentExtension)
                        {
                            XmlSchemaComplexContentExtension XmlSchemaComplexContentExtension = XmlSchemaComplexContent.Content as XmlSchemaComplexContentExtension;
                            ExtensionChild = CreateExtension(XmlSchemaComplexContentExtension);
                        }
                        else if (XmlSchemaComplexContent.Content is XmlSchemaComplexContentRestriction)
                        {
                            XmlSchemaComplexContentRestriction XmlSchemaComplexContentRestriction = XmlSchemaComplexContent.Content as XmlSchemaComplexContentRestriction;
                            Restriction Restriction = new Restriction
                            {
                                BaseName = XmlSchemaComplexContentRestriction.BaseTypeName.Name,
                                BaseNameSpace = XmlSchemaComplexContentRestriction.BaseTypeName.Namespace
                            };

                            if (XmlSchemaComplexContentRestriction.Attributes.Count != 0)
                            {
                                int i = 0;
                                foreach (XmlSchemaAttribute Attribute in XmlSchemaComplexContentRestriction.Attributes)
                                {
                                    if (!string.IsNullOrEmpty(Attribute.Name))
                                    {
                                        Restriction.Attributes.Add(Attribute.Name);
                                    }
                                    else
                                    {
                                        Restriction.Attributes.Add(Attribute.UnhandledAttributes[i].LocalName);
                                    }

                                    i++;
                                }
                            }
                            if (XmlSchemaComplexContentRestriction.Particle is XmlSchemaSequence)
                            {
                                XmlSchemaSequence XmlSchemaSequence = XmlSchemaComplexContentRestriction.Particle as XmlSchemaSequence;
                                Restriction.ComplexTypeElementsList = GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaSequence.Items);
                            }
                            RestrictionChild = Restriction;
                        }

                    }
                    else
                    {
                        LogFile += "Unhandled Code on line else 915" + Environment.NewLine;
                        //TODO take care for more items
                    }


                    ComplexType NewComplexType = new ComplexType
                    {
                        Name = XmlSchemaComplexType.Name,
                        Source = GetSourceByReferanceType(XmlSchemaComplexType.SourceUri),
                        TargetNameSpace = XmlSchemaComplexType.QualifiedName.Namespace,
                        ComplexTypeElementsList = ComplexTypeElementsList,
                        Extension = ExtensionChild,
                        Restriction = RestrictionChild
                    };
                    ComplexTypesList.Add(NewComplexType);
                }
                else if (item is XmlSchemaSimpleType)
                {
                    XmlSchemaSimpleType XmlSchemaSimpleType = item as XmlSchemaSimpleType;
                    if (XmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction)
                    {
                        XmlSchemaSimpleTypeRestriction XmlSchemaSimpleTypeRestriction = XmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                        Restriction Restriction = CreateRestriction(XmlSchemaSimpleTypeRestriction);

                        ComplexType NewComplexType = new ComplexType
                        {
                            Name = XmlSchemaSimpleType.Name,
                            Restriction = Restriction,
                            Source = GetSourceByReferanceType(XmlSchemaSimpleTypeRestriction.SourceUri),
                            TargetNameSpace = XmlSchemaSimpleType.QualifiedName.Namespace,

                            ComplexTypeElementsList = []
                        };
                        ComplexTypesList.Add(NewComplexType);
                    }
                }
            }
        }



        private Extension CreateExtension(XmlSchemaComplexContentExtension XmlSchemaComplexContentExtension)
        {
            Extension Extension = new Extension
            {
                BaseName = XmlSchemaComplexContentExtension.BaseTypeName.Name,
                BaseNameSpace = XmlSchemaComplexContentExtension.BaseTypeName.Namespace
            };
            if (XmlSchemaComplexContentExtension.Attributes.Count != 0)
            {
                foreach (XmlSchemaAttribute Attribute in XmlSchemaComplexContentExtension.Attributes)
                {
                    Extension.Attributes.Add(Attribute.Name);
                }
            }
            if (XmlSchemaComplexContentExtension.Particle is XmlSchemaSequence)
            {
                XmlSchemaSequence XmlSchemaSequence = XmlSchemaComplexContentExtension.Particle as XmlSchemaSequence;

                Extension.ComplexTypeElementsList = GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaSequence.Items);



            }
            return Extension;
        }

        private Restriction CreateRestriction(XmlSchemaSimpleTypeRestriction XmlSchemaSimpleTypeRestriction)
        {
            Restriction Restriction = new Restriction
            {
                BaseName = XmlSchemaSimpleTypeRestriction.BaseTypeName.Name,
                BaseNameSpace = XmlSchemaSimpleTypeRestriction.BaseTypeName.Namespace
            };
            List<string> Enumerations = [];
            foreach (var Facet in XmlSchemaSimpleTypeRestriction.Facets)
            {
                if (Facet is XmlSchemaEnumerationFacet)
                {
                    Enumerations.Add(((XmlSchemaEnumerationFacet)Facet).Value);
                }
                else if (Facet is XmlSchemaMinLengthFacet)
                {
                    Restriction.MinLeanth = ((XmlSchemaMinLengthFacet)Facet).Value;
                }
                else if (Facet is XmlSchemaMaxInclusiveFacet)
                {
                    LogFile += "Unhandled Code on line else 951" + Environment.NewLine;
                }
                else if (Facet is XmlSchemaPatternFacet)
                {
                    LogFile += "Unhandled Code on line else 965" + Environment.NewLine;
                }
                else if (Facet is XmlSchemaMinInclusiveFacet)
                {
                    LogFile += "Unhandled Code on line else 958" + Environment.NewLine;
                }
                else
                {
                    LogFile += "Unhandled Code on line else 963" + Environment.NewLine;
                }
            }
            Restriction.Enumerations = Enumerations;
            return Restriction;
        }

        private List<ComplexTypeChild> GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaObjectCollection xmlSchemaObjectCollection)
        {
            List<ComplexTypeChild> ComplexTypeElementsList = [];

            foreach (object ChildElementObj in xmlSchemaObjectCollection)
            {
                ComplexTypeChild ComplexTypeChild = null;
                if (ChildElementObj is XmlSchemaElement)
                {
                    XmlSchemaElement XmlSchemaElement = ChildElementObj as XmlSchemaElement;
                    if (string.IsNullOrEmpty(XmlSchemaElement.RefName.Name))
                    {
                        ComplexTypeChild = CreateElement(XmlSchemaElement);
                    }
                    else
                    {
                        ComplexTypeChild = CreateRefElement(XmlSchemaElement);
                    }

                    if (ComplexTypeChild is Element && string.IsNullOrEmpty(((Element)ComplexTypeChild).Type) && string.IsNullOrEmpty(((Element)ComplexTypeChild).TypeNameSpace))
                    {
                        string CompleteSourceUri = GetPathWithoutFileConvention(XmlSchemaElement.SourceUri);
                        if (!string.IsNullOrEmpty(XmlSchemaElement.SourceUri))
                        {
                            KeyValuePair<List<string>, string> KeyValue = AllSourcesNameSpaces.FirstOrDefault(x => x.Key.Contains(CompleteSourceUri));
                            ((Element)ComplexTypeChild).TypeNameSpace = KeyValue.Value;
                        }

                        else
                        {
                            ((Element)ComplexTypeChild).TypeNameSpace = string.Empty;
                        }

                        if (XmlSchemaElement.SchemaType is XmlSchemaComplexType)
                        {
                            XmlSchemaComplexType XmlSchemaComplexType = XmlSchemaElement.SchemaType as XmlSchemaComplexType;
                            ComplexType NewComplexType = CreateComplexType(XmlSchemaElement, XmlSchemaComplexType);
                            ComplexTypesList.Add(NewComplexType);
                        }
                        else if (XmlSchemaElement.SchemaType is XmlSchemaSimpleType)
                        {
                            XmlSchemaSimpleType XmlSchemaSimpleType = XmlSchemaElement.SchemaType as XmlSchemaSimpleType;
                            if (XmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction)
                            {
                                XmlSchemaSimpleTypeRestriction XmlSchemaSimpleTypeRestriction = XmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                                Restriction Restriction = CreateRestriction(XmlSchemaSimpleTypeRestriction);
                                ((Element)ComplexTypeChild).Type = XmlSchemaSimpleTypeRestriction.BaseTypeName.Name;
                            }

                        }

                    }

                    ComplexTypeElementsList.Add(ComplexTypeChild);
                }
                else if (ChildElementObj is XmlSchemaChoice)
                {
                    XmlSchemaChoice XmlSchemaChoice = ChildElementObj as XmlSchemaChoice;
                    List<ComplexTypeChild> ComplexTypeElementsChoisesList = GetElementListFromSchemaChoice(XmlSchemaChoice);

                    foreach (ComplexTypeChild CTC in ComplexTypeElementsChoisesList)
                    {
                        ComplexTypeElementsList.Add(CTC);
                    }
                }
                else if (ChildElementObj is XmlSchemaSequence)
                {
                    XmlSchemaSequence InnerXmlSchemaSequence = ChildElementObj as XmlSchemaSequence;

                    List<ComplexTypeChild> ComplexTypeElementsSequenceList = GetComplexTypeChildListFromXmlSchemaObjectCollection(InnerXmlSchemaSequence.Items);
                    foreach (ComplexTypeChild CTC in ComplexTypeElementsSequenceList)
                    {
                        ComplexTypeElementsList.Add(CTC);
                    }
                }
            }

            return ComplexTypeElementsList;

        }

        public string GenerateRandomGroupID()
        {
            int _min = 100;
            int _max = 999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

        private List<ComplexTypeChild> GetElementListFromSchemaChoice(XmlSchemaChoice XmlSchemaChoice)
        {
            List<ComplexTypeChild> ComplexTypeElementsList = [];
            string GroupID = GenerateRandomGroupID();
            foreach (object ChoiseElementObj in XmlSchemaChoice.Items)
            {
                ComplexTypeChild ComplexTypeChild = new ComplexTypeChild();
                if (ChoiseElementObj is XmlSchemaElement)
                {
                    XmlSchemaElement XmlSchemaElement = ChoiseElementObj as XmlSchemaElement;
                    if (string.IsNullOrEmpty(XmlSchemaElement.RefName.Name))
                    {
                        ComplexTypeChild = CreateElement(XmlSchemaElement);
                    }
                    else
                    {
                        ComplexTypeChild = CreateRefElement(XmlSchemaElement);
                    }

                    ComplexTypeChild.ChoiceGroup = GroupID;

                    ComplexTypeElementsList.Add(ComplexTypeChild);
                }
                else if (ChoiseElementObj is XmlSchemaSequence)
                {
                    XmlSchemaSequence InnerXmlSchemaSequence = ChoiseElementObj as XmlSchemaSequence;

                    List<ComplexTypeChild> ComplexTypeElementsSequenceList = GetComplexTypeChildListFromXmlSchemaObjectCollection(InnerXmlSchemaSequence.Items);
                    foreach (ComplexTypeChild CTC in ComplexTypeElementsSequenceList)
                    {
                        CTC.ChoiceGroup = GroupID + string.Empty;
                        ComplexTypeElementsList.Add(CTC);
                    }
                }
            }
            return ComplexTypeElementsList;
        }

        private string GetPathWithoutFileConvention(string sourceUri)
        {
            if (sourceUri != null)
            {
                if (sourceUri.StartsWith("file"))
                {
                    return Path.GetFullPath(new Uri(sourceUri).AbsolutePath);
                }
                else
                {
                    return sourceUri;
                }
            }
            else
            {
                return string.Empty;
            }

        }

        private void GetAllElementsAndComplexTypesFromMainSchema()
        {
            foreach (XmlSchemasExtended XMLSExtended in mSchemasList)
            {
                foreach (XmlSchema XmlSchema in XMLSExtended.mXmlSchemas)
                {
                    GetAllElementsAndComplexTypesFromImportedSchema(XmlSchema);
                }
            }
        }

        private ComplexType CreateComplexType(XmlSchemaElement XmlSchemaElement, XmlSchemaComplexType XmlSchemaComplexType)
        {
            ComplexType NewComplexType = new ComplexType
            {
                Name = XmlSchemaElement.Name
            };
            if (NewComplexType.Name == "ArrayOf_tns3_WSMessageType")
            {

            }
            if (XmlSchemaElement.SourceUri != null)
            {
                NewComplexType.Source = GetSourceByReferanceType(XmlSchemaElement.SourceUri);
            }
            else
            {
                NewComplexType.Source = string.Empty;
            }

            NewComplexType.TargetNameSpace = XmlSchemaElement.QualifiedName.Namespace;
            if (XmlSchemaComplexType.Particle is XmlSchemaSequence)
            {
                XmlSchemaSequence XmlSchemaSequence = XmlSchemaComplexType.Particle as XmlSchemaSequence;
                NewComplexType.ComplexTypeElementsList = GetComplexTypeChildListFromXmlSchemaObjectCollection(XmlSchemaSequence.Items);
            }
            else if (XmlSchemaComplexType.Particle is XmlSchemaChoice)
            {
                XmlSchemaChoice XmlSchemaChoice = XmlSchemaComplexType.Particle as XmlSchemaChoice;
                NewComplexType.ComplexTypeElementsList = GetElementListFromSchemaChoice(XmlSchemaChoice);
            }
            else
            {
                LogFile += "CreateComplexType Failed as Particle is not XMLSchema" + Environment.NewLine;
                //Write to log
            }
            if (XmlSchemaComplexType.ContentModel != null && XmlSchemaComplexType.ContentModel.Content is XmlSchemaComplexContentExtension)
            {
                XmlSchemaComplexContentExtension XmlSchemaComplexContentExtension = XmlSchemaComplexType.ContentModel.Content as XmlSchemaComplexContentExtension;
                NewComplexType.Extension = CreateExtension(XmlSchemaComplexContentExtension);
            }
            return NewComplexType;
        }

        private ComplexType CreateSimpleType(XmlSchemaSimpleType XmlSchemaSimpleType, ref Restriction Restriction)
        {
            if (XmlSchemaSimpleType.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction XmlSchemaSimpleTypeRestriction = XmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction;
                Restriction.BaseName = XmlSchemaSimpleTypeRestriction.BaseTypeName.Name;
                Restriction.BaseNameSpace = XmlSchemaSimpleTypeRestriction.BaseTypeName.Namespace;
                List<string> Enumerations = [];

                foreach (var Facet in XmlSchemaSimpleTypeRestriction.Facets)
                {

                    if (Facet is XmlSchemaEnumerationFacet)
                    {
                        Enumerations.Add(((XmlSchemaEnumerationFacet)Facet).Value);
                    }
                    else if (Facet is XmlSchemaMinLengthFacet)
                    {
                        Enumerations.Add(((XmlSchemaMinLengthFacet)Facet).Value);
                    }
                    else if (Facet is XmlSchemaMaxLengthFacet)
                    {
                        Enumerations.Add(((XmlSchemaMaxLengthFacet)Facet).Value);
                    }
                }
                Restriction.Enumerations = Enumerations;
                ComplexType NewComplexType = new ComplexType
                {
                    Name = XmlSchemaSimpleType.Name,
                    Restriction = Restriction
                };

                if (XmlSchemaSimpleType.SourceUri != null)
                {
                    NewComplexType.Source = GetSourceByReferanceType(XmlSchemaSimpleType.SourceUri);
                }
                else
                {
                    NewComplexType.Source = string.Empty;
                }
                //if (string.IsNullOrEmpty(NewComplexType.Source))
                NewComplexType.TargetNameSpace = XmlSchemaSimpleType.QualifiedName.Namespace;

                NewComplexType.ComplexTypeElementsList = [];
                return NewComplexType;
            }
            else
            {
                LogFile += "CreateSimpleType Failed as XmlSchemaSimpleType.Content is not XmlSchemaSimpleTypeRestriction 1114" + Environment.NewLine;
                return null;
            }
        }

        private Element CreateElement(XmlSchemaElement xmlSchemaElement)
        {
            Element Element = new Element
            {
                Name = xmlSchemaElement.Name
            };
            if (xmlSchemaElement.SourceUri != null)
            {
                Element.Source = GetSourceByReferanceType(xmlSchemaElement.SourceUri);
            }
            else
            {
                Element.Source = mURL;
            }
            //if (string.IsNullOrEmpty(Element.Source))
            Element.TargetNameSpace = xmlSchemaElement.QualifiedName.Namespace;

            Element.Type = xmlSchemaElement.SchemaTypeName.Name;
            Element.TypeNameSpace = xmlSchemaElement.SchemaTypeName.Namespace;
            Element.MinOccurs = xmlSchemaElement.MinOccurs;
            Element.MaxOccurs = xmlSchemaElement.MaxOccurs;

            if (xmlSchemaElement.SchemaType is XmlSchemaComplexType)
            {
                XmlSchemaComplexType XmlSchemaComplexType = xmlSchemaElement.SchemaType as XmlSchemaComplexType;
                ComplexType NewComplexType = CreateComplexType(xmlSchemaElement, XmlSchemaComplexType);
                Element.InnerElementComplexType = NewComplexType;
            }
            else if (xmlSchemaElement.SchemaType is XmlSchemaSimpleType)
            {
                XmlSchemaSimpleType XmlSchemaSimpleType = xmlSchemaElement.SchemaType as XmlSchemaSimpleType;
                Restriction Restriction = CreateRestriction(XmlSchemaSimpleType.Content as XmlSchemaSimpleTypeRestriction);
                Element.InnerElementRestriction = Restriction;
            }

            return Element;
        }

        private ComplexTypeChild CreateRefElement(XmlSchemaElement xmlSchemaElement)
        {
            RefElement RefElement = new RefElement
            {
                Name = xmlSchemaElement.RefName.Name,
                RefNameSpace = xmlSchemaElement.RefName.Namespace
            };

            if (xmlSchemaElement.SourceUri != null)
            {
                RefElement.Source = GetSourceByReferanceType(xmlSchemaElement.SourceUri);
            }
            else
            {
                RefElement.Source = mURL;
            }

            RefElement.MinOccurs = xmlSchemaElement.MinOccurs;
            RefElement.MaxOccurs = xmlSchemaElement.MaxOccurs;

            return RefElement;
        }

        private string GetContainingFolderFromURL(string URL)
        {
            string containingFolder = string.Empty;
            int lastFolderIndexBackSlash = URL.LastIndexOf(@"\");
            int lastFolderInderSlash = URL.LastIndexOf("/");

            containingFolder = URL[..Math.Max(lastFolderIndexBackSlash, lastFolderInderSlash)];
            return containingFolder;
        }

        #endregion PullingAndCreatingElements

        #region MessagesRegion

        private PortTypeOperationDetails GetOperationInputMessage(OperationBinding bindingOperation, PortTypeCollection portTypColl)
        {
            PortTypeOperationDetails OD = new PortTypeOperationDetails();
            foreach (PortType PortType in portTypColl)
            {
                foreach (Operation portTypeOperation in PortType.Operations)
                {
                    if (portTypeOperation.Name == bindingOperation.Name)
                    {
                        foreach (var message in portTypeOperation.Messages)
                        {
                            if (message is OperationInput)
                            {
                                string OperationInputMessageName = ((OperationInput)message).Message.Name;
                                string OperationInputBindingName = bindingOperation.Input.Name;
                                string OperationInputName = ((OperationInput)message).Name;
                                if ((!string.IsNullOrEmpty(OperationInputBindingName) && OperationInputMessageName == OperationInputBindingName) || (string.IsNullOrEmpty(OperationInputBindingName) || (!string.IsNullOrEmpty(OperationInputName) && (OperationInputName == OperationInputBindingName))))
                                {
                                    OD.InputMessageName = OperationInputMessageName;
                                    OD.ParameterOrder = portTypeOperation.ParameterOrder;
                                    return OD;

                                }
                            }
                        }
                    }
                }
            }
            return OD;
        }

        private PortTypeOperationDetails GetOperationOutputMessage(OperationBinding bindingOperation, PortTypeCollection portTypColl)
        {
            //foreach (PortType PortType in portTypColl)
            //{
            //    foreach (Operation operation in PortType.Operations)
            //    {
            //        if (operation.Name == name)
            //        {
            //            foreach (var message in operation.Messages)
            //            {
            //                if (message is OperationOutput)
            //                {
            //                    return ((OperationOutput)message).Message.Name;
            //                }
            //            }
            //        }
            //    }
            //}
            //return null;

            PortTypeOperationDetails OD = new PortTypeOperationDetails();
            foreach (PortType PortType in portTypColl)
            {
                foreach (Operation portTypeOperation in PortType.Operations)
                {
                    if (portTypeOperation.Name == bindingOperation.Name)
                    {
                        foreach (var message in portTypeOperation.Messages)
                        {
                            if (message is OperationOutput)
                            {
                                string OperationOutputMessageName = ((OperationOutput)message).Message.Name;
                                string OperationOutputBindingName = bindingOperation.Input.Name;

                                if ((!string.IsNullOrEmpty(OperationOutputBindingName) && OperationOutputMessageName == OperationOutputBindingName) || (string.IsNullOrEmpty(OperationOutputBindingName)))
                                {
                                    OD.InputMessageName = OperationOutputMessageName;
                                    OD.ParameterOrder = portTypeOperation.ParameterOrder;
                                    return OD;

                                }
                            }
                        }
                    }
                }
            }
            return OD;

        }

        private BindingOperationInputTag GetOperationInputTagByOperation(OperationBinding operation)
        {
            BindingOperationInputTag OIT = new BindingOperationInputTag();
            foreach (var extension in operation.Input.Extensions)
            {
                if (extension is SoapBodyBinding)
                {
                    OIT.BodyEncodingStyle = ((SoapBodyBinding)extension).Encoding;
                    OIT.BodyNameSpace = ((SoapBodyBinding)extension).Namespace;
                }
                else if (extension is SoapHeaderBinding)
                {
                    if (((SoapHeaderBinding)extension).Message != null)
                    {
                        OIT.HeaderMessage = ((SoapHeaderBinding)extension).Message.Name;
                        OIT.HeadNameSpace = ((SoapHeaderBinding)extension).Message.Namespace;
                    }
                }
            }

            return OIT;
        }

        private BindingOperationInputTag GetOperationOutputTagByOperation(OperationBinding operation)
        {
            BindingOperationInputTag OIT = new BindingOperationInputTag();
            foreach (var extension in operation.Output.Extensions)
            {
                if (extension is SoapBodyBinding)
                {
                    OIT.BodyEncodingStyle = ((SoapBodyBinding)extension).Encoding;
                    OIT.BodyNameSpace = ((SoapBodyBinding)extension).Namespace;
                }
                else if (extension is SoapHeaderBinding)
                {
                    if (((SoapHeaderBinding)extension).Message != null)
                    {
                        OIT.HeaderMessage = ((SoapHeaderBinding)extension).Message.Name;
                        OIT.HeadNameSpace = ((SoapHeaderBinding)extension).Message.Namespace;
                    }
                }
            }

            return OIT;
        }

        private MessageInputPart GetMessagePartsByOperation(string OperationInputMessage, MessageCollection messages, BindingOperationInputTag OperationInputTag)
        {
            MessageInputPart MessageParts = new MessageInputPart();
            foreach (Message Message in messages)
            {
                if (Message.Name == OperationInputMessage || ((OperationInputTag != null) && Message.Name == OperationInputTag.HeaderMessage))
                {
                    //TODO:check if can be message for OperationInputMessage AND for OperationInputTag
                    foreach (MessagePart Part in Message.Parts)
                    {
                        if (Part.Name == "header" || Part.Name == "MessageHeader" || Part.Name.Contains("Hdr") || Part.Element.Name.Contains("Header"))
                        {
                            MessageParts.Head = Part.Element.Name;
                            MessageParts.HeadNameSpace = Part.Element.Namespace;
                        }
                        else if (Part.Name == "body")
                        {
                            MessageParts.Body = Part.Element.Name;
                            MessageParts.BodyNameSpace = Part.Element.Namespace;
                        }
                        else if (Part.Name == "parameters")
                        {
                            MessageParts.Parameters = Part.Element.Name;
                            MessageParts.ParametersNameSpace = Part.Element.Namespace;
                        }
                        else
                        {
                            MessageParts.Parameters = Part.Element.Name;
                            MessageParts.ParametersNameSpace = Part.Element.Namespace;
                        }
                    }

                }

            }
            return MessageParts;
        }

        #endregion MessagesRegion

        #region ApplicationAPIModelFields

        private string GetEndPointURL(ServiceCollection Services, string BindingName)
        {
            foreach (Service service in Services)
            {
                foreach (Port port in service.Ports)
                {
                    if ((port.Name == BindingName) || (port.Binding != null && port.Binding.Name == BindingName))
                    {
                        return ReturnEndPointURL(port);
                    }
                }
            }
            return string.Empty;
        }

        private string ReturnEndPointURL(Port port)
        {
            if (port.Extensions[0] is SoapAddressBinding)
            {
                return ((SoapAddressBinding)port.Extensions[0]).Location;
            }
            else if (port.Extensions[0] is HttpAddressBinding)
            {
                return ((HttpAddressBinding)port.Extensions[0]).Location;
            }
            else
            {
                return string.Empty;
            }
        }

        private string GetDescription(PortTypeCollection portTypColl, string BindingName, string OperationName)
        {
            foreach (PortType portType in portTypColl)
            {
                if (portType.Name == BindingName || portType.Name + "12" == BindingName)
                {
                    foreach (Operation op in portType.Operations)
                    {
                        if (OperationName == op.Name)
                        {
                            return op.Documentation;
                        }
                    }
                }
            }
            return string.Empty;
        }

        #endregion ApplicationAPIModelFields

        #region URLs
        private void PopulateAllURLsList()
        {

            List<Tuple<string, string>> InnerImportsURLs = [];

            foreach (XmlSchemasExtended XMLSExtended in mSchemasList)
            {
                foreach (XmlSchema schema in XMLSExtended.mXmlSchemas)
                {
                    XmlSchemaObjectCollection Items = schema.Includes;

                    foreach (var item in Items)
                    {
                        if (item is XmlSchemaImport)
                        {
                            XmlSchemaImport XmlSchemaImportItem = item as XmlSchemaImport;
                            string URL = GetURLFromSchemaLocationBymURLType(XmlSchemaImportItem.SchemaLocation);
                            AllURLs.Add(new Tuple<string, string>(URL, XMLSExtended.ContainingFolder));
                            InnerImportsURLs.Add(new Tuple<string, string>(URL, XMLSExtended.ContainingFolder));
                        }
                        else if (item is XmlSchemaInclude)
                        {
                            XmlSchemaInclude XmlSchemaIncludeItem = item as XmlSchemaInclude;
                            string URL = GetURLFromSchemaLocationBymURLType(XmlSchemaIncludeItem.SchemaLocation);
                            AllURLs.Add(new Tuple<string, string>(URL, XMLSExtended.ContainingFolder));
                            InnerImportsURLs.Add(new Tuple<string, string>(URL, XMLSExtended.ContainingFolder));
                        }
                    }
                }
            }

            foreach (Tuple<string, string> URLTuple in InnerImportsURLs)
            {
                if (!string.IsNullOrEmpty(URLTuple.Item1))
                {
                    string CompleteURL = string.Empty;
                    if (!URLTuple.Item1.Contains(URLTuple.Item2))
                    {
                        CompleteURL = Path.Combine(URLTuple.Item2, URLTuple.Item1);
                    }
                    else
                    {
                        CompleteURL = URLTuple.Item1;
                    }

                    XmlTextReader reader = new XmlTextReader(CompleteURL);
                    XmlSchema schema = XmlSchema.Read(reader, null);
                    XmlSchemaObjectCollection Items = schema.Includes;

                    string directory = this.GetDirectoryName(CompleteURL);
                    string relativeDirectories = string.Empty;
                    if (!directory.StartsWith(URLTuple.Item2))
                    {
                        int ContainingFolderLeanth = URLTuple.Item2.Length;
                        if (ContainingFolderLeanth < directory.Length)
                        {
                            relativeDirectories = directory[ContainingFolderLeanth..].TrimStart('\\');
                        }
                    }
                    GetAllURLsFFromSchemaItems(Items, relativeDirectories, URLTuple.Item2);
                }
            }
        }

        private string GetURLFromSchemaLocationBymURLType(string schemaLocation)
        {
            if (mURL.StartsWith("http"))
            {
                return schemaLocation;
            }
            else
            {
                if (schemaLocation != null)
                {
                    return schemaLocation.Replace("/", "\\");
                }
                else
                {
                    return string.Empty;
                }
            }

        }

        private void GetAllURLsFFromSchemaItems(XmlSchemaObjectCollection items, string relativeDirectories, string containigFolder)
        {
            foreach (var item in items)
            {
                if (item is XmlSchemaImport)
                {
                    Tuple<string, string> schemaFullLocation = new Tuple<string, string>(Path.Combine(relativeDirectories, ((XmlSchemaImport)item).SchemaLocation), containigFolder);
                    Tuple<string, string> schemaFullLocationReplacedSlashes = new Tuple<string, string>(Path.Combine(relativeDirectories, ((XmlSchemaImport)item).SchemaLocation).Replace("\\", "/"), containigFolder);
                    if (!AllURLs.Contains(schemaFullLocation) && !AllURLs.Contains(schemaFullLocationReplacedSlashes))
                    {
                        string CompleteURL = GetCompleteURL(schemaFullLocation.Item1, containigFolder);
                        AllURLs.Add(schemaFullLocation);
                        ReadSchemaURLs(CompleteURL, containigFolder);
                    }

                }
                else if (item is XmlSchemaInclude)
                {
                    Tuple<string, string> schemaFullLocation = new Tuple<string, string>(Path.Combine(relativeDirectories, ((XmlSchemaInclude)item).SchemaLocation), containigFolder);
                    Tuple<string, string> schemaFullLocationReplacedSlashes = new Tuple<string, string>(Path.Combine(relativeDirectories, ((XmlSchemaInclude)item).SchemaLocation).Replace("\\", "/"), containigFolder);
                    if (!AllURLs.Contains(schemaFullLocation) && !AllURLs.Contains(schemaFullLocationReplacedSlashes))
                    {
                        string CompleteURL = GetCompleteURL(schemaFullLocation.Item1, containigFolder);
                        AllURLs.Add(schemaFullLocation);
                        ReadSchemaURLs(CompleteURL, containigFolder);
                    }
                }
            }
        }

        public void ReadSchemaURLs(string CompleteURL, string containigFolder)
        {
            XmlTextReader reader = new XmlTextReader(CompleteURL);
            XmlSchema schema = XmlSchema.Read(reader, null);
            XmlSchemaObjectCollection Items = schema.Includes;
            string directory = this.GetDirectoryName(CompleteURL);
            string relativeDirectories = string.Empty;
            if (directory != containigFolder)
            {
                int ContainingFolderLeanth = containigFolder.Length;
                if (ContainingFolderLeanth < directory.Length && !directory.StartsWith("http"))
                {
                    relativeDirectories = directory[ContainingFolderLeanth..].TrimStart('\\');
                }
            }
            GetAllURLsFFromSchemaItems(Items, relativeDirectories, containigFolder);
        }

        private string GetCompleteURL(string URL, string containingFolder)
        {
            string completeURL = string.Empty;
            if (!URL.Contains(containingFolder))
            {
                if (containingFolder.StartsWith("http"))
                {
                    completeURL = containingFolder + "/" + URL;
                }
                else
                {
                    completeURL = Path.Combine(containingFolder, URL);
                }
            }
            else
            {
                completeURL = URL;
            }

            return completeURL;
        }

        private string GetDirectoryName(string URL)
        {
            string result = string.Empty;
            int LastSlash = -1;

            if (URL.StartsWith("http"))
            {
                //Uri parent = new Uri(new Uri(URL), ".");

                if (URL.EndsWith("/"))
                {
                    URL = URL.Remove(URL.Length - 1, 1);
                }
                LastSlash = URL.LastIndexOf("/");
                if (LastSlash != -1)
                {
                    result = URL[..LastSlash];
                }
            }
            else
            {
                result = Path.GetDirectoryName(URL);
            }
            return result;
        }

        #endregion URL

        #region Functions to Check Circles

        private bool CheckIfPathContainsCircles(string Path)
        {
            string[] Nodes = Path.Split('/');
            if (Nodes.Length < 6)
            {
                return false;
            }

            for (int i = 1; i < Nodes.Length; i++)
            {
                for (int j = i + 2; j < Nodes.Length; j++)
                {
                    if ((j - i) * 2 > Nodes.Length)
                    {
                        break;
                    }

                    string substring = GetSubstringByIndex(Nodes, i, j);
                    int SubStringIndex = Path.IndexOf(substring);
                    string StringToCheck = Path[(SubStringIndex + substring.Length)..];
                    if (StringToCheck.StartsWith(substring))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private string GetSubstringByIndex(string[] nodes, int start, int leanth)
        {
            string s = string.Empty;
            for (int i = start; i < leanth; i++)
            {
                s = s + nodes[i] + '/';
            }
            return s;
        }

        #endregion Functions to check Circles

        #region WizardFunctions

        public bool ValidateWSDLURL(string URL, bool? URLRadioButton, ref string error)
        {
            if (URLRadioButton == true)
            {
                if (!(URL.Trim().ToUpper().StartsWith("HTTP") && URL.Trim().ToUpper().EndsWith("WSDL")))
                {
                    error = "Please specify valid http WSDL URL";
                    return false;
                }
            }
            else
            {
                if (!(URL.ToUpper().EndsWith("XML") || (URL.ToUpper().EndsWith("WSDL"))))
                {
                    error = "Please specify valid XML/WSDL File";
                    return false;
                }
            }

            try
            {
                XmlTextReader reader = new XmlTextReader(URL);

                ServiceDescription sd = ServiceDescription.Read(reader);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is System.Net.Http.HttpRequestException)
                {
                    error += ex.InnerException?.Message;
                }
                else
                {
                    error += "There is a problem in this WSDL file format, please verify the WSDL format and re-try";
                }
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return false;
            }
            return true;
        }

        public void ValidateWSDLInputs(string URL, bool? URLRadioButton, ref string WizardEventArgsErrorString)
        {
            if (string.IsNullOrEmpty(URL))
            {
                WizardEventArgsErrorString = "URL/File field cannot be empty";
            }
            else
            {
                string error = string.Empty;
                if (!ValidateWSDLURL(URL, URLRadioButton, ref error))
                {
                    WizardEventArgsErrorString = error;
                    return;
                }
            }
        }

        #endregion WizardFunctions
    }

    #region Classes

    public class ComplexTypeChild
    {
        public string ChoiceGroup;
        public bool ContainsCircle;
        public string Name;
        public decimal MinOccurs;
        public decimal MaxOccurs;
    }
    public class Restriction
    {
        public string BaseName;
        public List<string> Enumerations = [];
        public string MinLeanth;
        public string Source;
        public string BaseNameSpace;
        public List<ComplexTypeChild> ComplexTypeElementsList = [];
        public List<string> Attributes = [];
    }

    public class Element : ComplexTypeChild
    {
        public string Type;
        public string TypeNameSpace;
        public string Source;
        public ComplexType InnerElementComplexType;
        public Restriction InnerElementRestriction;
        public string TargetNameSpace { get; set; }
    }

    public class Extension
    {
        public string BaseName;
        public List<string> Attributes = [];
        public List<ComplexTypeChild> ComplexTypeElementsList = [];
        public string Source;
        public string BaseNameSpace;
    }

    public class RefElement : ComplexTypeChild
    {
        public string RefNameSpace;
        public string Source;
        public string ParentName;
    }

    public class ComplexType
    {
        public string Name;
        public string Source;
        public List<ComplexTypeChild> ComplexTypeElementsList = [];
        public Extension Extension;
        public Restriction Restriction;
        public List<string> Attributes = [];
        public string TargetNameSpace { get; set; }
    }

    public class Part
    {
        public enum ePartElementType
        {
            Type,
            Element

        }

        public string PartName { get; set; }
        public string ElementName { get; set; }
        public string ElementType { get; set; }
        public string ElementNameSpace { get; set; }

        public ePartElementType PartElementType { get; set; }
    }



    public class MessageInputPart
    {
        public string Head { get; set; }
        public string HeadNameSpace { get; set; }
        public string HeadNameSpaceName { get; set; }
        public string Body { get; set; }
        public string BodyNameSpace { get; set; }
        public string BodyNameSpaceName { get; set; }
        public string Parameters { get; set; }
        public string ParametersNameSpace { get; set; }
        public string ParametersNameSpaceName { get; set; }
    }

    public class BindingOperationInputTag
    {
        public string HeaderMessage { get; set; }
        public string HeadNameSpace { get; set; }

        public string BodyName { get; set; }
        public string BodyEncodingStyle { get; set; }
        public string BodyNameSpace { get; set; }
    }


    public class PortTypeOperationDetails
    {
        public string InputMessageName { get; set; }
        public string[] ParameterOrder { get; set; }
    }

    public class ServiceDescriptionExtended
    {
        public ServiceDescription mServiceDescription;
        public string ContainingFolder;
    }

    public class XmlSchemasExtended
    {
        public XmlSchemas mXmlSchemas;
        public string ContainingFolder;
    }

    #endregion Classes
}